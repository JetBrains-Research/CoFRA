using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Internal.Dumps;
using Cofra.AbstractIL.Internal.Indexing;
using Cofra.AbstractIL.Internal.Transducers;
using Cofra.AbstractIL.Internal.Types;

namespace Cofra.AbstractIL.Internal.ControlStructures
{
    using Node = Int32;

    public class GraphStructuredProgramBuilder
    {
        private readonly GraphStructuredProgram<Node> myProgram;
        private readonly DenseIdsProvider myNodeIdsProvider;

        public GraphStructuredProgramBuilder()
        {
            myNodeIdsProvider = new DenseIdsProvider();

            myProgram = new GraphStructuredProgram<Node>(
                () => myNodeIdsProvider.NextId(),
                id => myNodeIdsProvider.FreeId(id));
        }

        public GraphStructuredProgramBuilder(GraphStructuredProgramBuilderState state)
        {
            myNodeIdsProvider = state.NodeIdsProvider;

            myProgram = new GraphStructuredProgram<Node>(
                () => myNodeIdsProvider.NextId(),
                id => myNodeIdsProvider.FreeId(id),
                state.Program);
        }

        private Func<int, Node> GetLocalMapper(ResolvedMethod<Node> method)
        {
            var localToGlobalIndicesMap = new Dictionary<int, Node>();

            Node Map(int local)
            {
                var exists = localToGlobalIndicesMap.TryGetValue(local, out var global);
                if (!exists)
                {
                    global = myProgram.CreateNode();
                    method.AddOwnedNode(global);
                    localToGlobalIndicesMap.Add(local, global);
                }

                return global;
            }

            return Map;
        }

        public GraphStructuredProgram<Node> GetProgram()
        {
            return myProgram;
        }

        public void UpdateMethodBody(IMethodHolder<Node> owner, MethodId id, INodeBasedProgram<int> body)
        {
            var method = myProgram.GetOrCreateMethod(owner, id.Value);
            var mapper = GetLocalMapper(method);

            var identity = new IdentityTransducer<Node>();
            var invocationsResolver = new InvocationsResolvingTransducer<Node>();
            var assignmentsResolver = new AssignmentsResolvingTransducer<Node>();

            var transducer = assignmentsResolver.Compose(invocationsResolver).Compose(identity);

            transducer.Transform(body, myProgram, method, mapper);

            //myProgram.DumpToDot("C:/work/graph.dot");
        }

        public void UpdateMethod(IMethodHolder<Node> owner, Common.Types.Method source)
        {
            var method = myProgram.GetOrCreateMethod(owner, source.Id.Value);
            myProgram.ClearMethod(method);

            // Variables.
            var localVariables = source.Variables.Cast<LocalVariableReference>()
                .Select(reference =>
                {
                    var defaultType = reference.DefaultType == null ? -1 :
                                      myProgram.GetOrCreateClass(reference.DefaultType).Id.GlobalId;
                    return new ResolvedLocalVariable(reference.Index, defaultType);
                });

            if (owner is ResolvedMethod<Node> owningMethod)
            {
                var possibleVariables = owningMethod.Variables.ToDictionary(pair => pair.Key, pair => pair.Value);
                foreach (var local in localVariables)
                {
                    if (!possibleVariables.ContainsKey(local.LocalId))
                    {
                        possibleVariables.Add(local.LocalId, local);
                    }
                }
                localVariables = possibleVariables.Values;
            }

            foreach (var localVariable in localVariables)
            {
                method.AddLocalVariable(localVariable);    
            }

            // LocalFunctions.
            if (source.LocalMethods != null)
            {
                foreach (var localMethod in source.LocalMethods)
                {
                    UpdateMethod(method, localMethod);
                }
            }

            // Body.
            var bodyAdapter = new CommonMethodAsNodeBasedProgram(source);
            UpdateMethodBody(owner, source.Id, bodyAdapter);
        }

        public IEnumerable<FullMethodId> UpdateClass(Common.Types.Class rawClass)
        {
            var resolvedClass = myProgram.GetOrCreateClass(rawClass.ClassId);

            if (rawClass.BaseClass != null)
            {
                var @base = myProgram.GetOrCreateClass(rawClass.BaseClass);
                resolvedClass.SetBaseId(@base.Id);
            }

            foreach (var method in rawClass.Methods)
            {
                UpdateMethod(resolvedClass, method);
            }

            return rawClass.Methods.Select(method => new FullMethodId(rawClass.ClassId, method.Id));
        }

        public void UpdateFile(string fileName, IEnumerable<FullMethodId> methodNames)
        {
            var resolved = methodNames.Select(
                fullId =>
                {
                    var owner = myProgram.GetOrCreateClass(fullId.ClassId);

                    myProgram.GetOrCreateMethod(owner, fullId.MethodId.Value);
                    var methodId = myProgram.Methods.Find(fullId.MethodId.Value);
                    Trace.Assert(methodId.HasValue);

                    return new ResolvedFullMethodId(
                        owner.Id, new ResolvedMethodId(methodId.Value));
                });

            myProgram.UpdateFile(fileName, resolved);
        }

        public GraphStructuredProgramBuilderState DumpState()
        {
            return new GraphStructuredProgramBuilderState(myProgram.Dump(), myNodeIdsProvider);
        }
    }
}
