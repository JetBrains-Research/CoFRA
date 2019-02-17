using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Internal.Dumps;
using Cofra.AbstractIL.Internal.Indexing;
using Cofra.AbstractIL.Internal.Restorers;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types;
using JetBrains.Annotations;
using QuickGraph;
using QuickGraph.Graphviz;

namespace Cofra.AbstractIL.Internal.ControlStructures
{
    public sealed class GraphStructuredProgram<TNode> :
        BidirectionalGraph<TNode, OperationEdge<TNode>>,
        IEdgeBasedProgram<TNode>
    {
        private readonly Dictionary<ResolvedClassId, ResolvedClass<TNode>> myClasses;
        private readonly Dictionary<int, File> myFiles;

        private readonly Func<TNode> myNodesProvider;
        private readonly Action<TNode> myOnNodeRemovedHandler;

        public DenseBidirectionalIndex<string> Files { get; }
        public DenseBidirectionalIndex<string> Classes { get; }
        public DenseBidirectionalIndex<string> Methods { get; }
        public DenseBidirectionalIndex<string> ClassFields { get; }

        private readonly HashSet<int> myUsedMethodIds;

        private Dictionary<TNode, string> myStartsToNameMap;

        public GraphStructuredProgram(Func<TNode> nodesProvider, Action<TNode> onNodeRemovedHandler)
        {
            Files = new DenseBidirectionalIndex<string>();
            Classes = new DenseBidirectionalIndex<string>();
            Methods = new DenseBidirectionalIndex<string>();
            ClassFields = new DenseBidirectionalIndex<string>();

            myNodesProvider = nodesProvider;
            myOnNodeRemovedHandler = onNodeRemovedHandler;

            myUsedMethodIds = new HashSet<int>();

            myFiles = new Dictionary<int, File>();
            myClasses = new Dictionary<ResolvedClassId, ResolvedClass<TNode>>();
        }

        public GraphStructuredProgram(
            Func<TNode> nodesProvider, 
            Action<TNode> onNodeRemovedHandler,
            GraphStructuredProgramDump<TNode> dump)
        {
            myNodesProvider = nodesProvider;
            myOnNodeRemovedHandler = onNodeRemovedHandler;

            Files = dump.FilesIndex;
            Classes = dump.ClassesIndex;
            Methods = dump.MethodsIndex;
            ClassFields = dump.ClassFieldsIndex;

            myUsedMethodIds = new HashSet<int>();
            myClasses = dump.Classes;
            myFiles = dump.Files;

            var nodes = CollectAllMethods()
                .SelectMany(method => method.GetOwnedNodes());

            AddVertexRange(nodes);
            AddVerticesAndEdgeRange(dump.Edges);

            RestoreBaseClasses();

            foreach (var rootMethodPair in myClasses.Values.SelectMany(@class => @class.Methods()))
            {
                RestoreLocalFunctionsVariables(rootMethodPair.Item2);
            }

            foreach (var method in CollectAllMethods())
            {
                method.ResetAdditionalVariables();

                var containingEdges = method.GetOwnedNodes()
                    .SelectMany(OutEdges)
                    .ToList();

                foreach (var edge in containingEdges)
                {
                    if (edge.Statement is InternalStatement internalStatement)
                    {
                        var restored = StatementRestorer.Restore(this, method, internalStatement);
                        base.RemoveEdge(edge);
                        base.AddEdge(new OperationEdge<TNode>(edge.Source, restored, edge.Target));
                    }
                }
            }
        }

        private void RestoreLocalFunctionsVariables(ResolvedMethod<TNode> root)
        {
            foreach (var localFunction in root.Methods.Values)
            {
                foreach (var localFunctionVariable in localFunction.Variables.Keys.ToList())
                {
                    if (root.Variables.ContainsKey(localFunctionVariable))
                    {
                        localFunction.RemoveLocalVariable(localFunctionVariable);
                        localFunction.AddLocalVariable(root.Variables[localFunctionVariable]);
                    }
                }

                RestoreLocalFunctionsVariables(localFunction);
            }
        }

        public void RestoreBaseClasses()
        {
            foreach (var clazz in myClasses.Values)
            {
                if (clazz.Base != null)
                {
                    myClasses.TryGetValue(clazz.Base, out var _base);
                    clazz.SetBase(_base);
                }
            }
        }

        public IEnumerable<Operation<TNode>> PossibleOperations(TNode position)
        {
            return OutEdges(position);
        }

        public IEnumerable<TNode> GetStarts()
        {
            return Enumerable.Empty<TNode>();
        }

        public TNode CreateNode()
        {
            var node = myNodesProvider();
            AddVertex(node);
            return node;
        }

        public void AddOperation(TNode position, Operation<TNode> operation)
        {
            AddEdge(new OperationEdge<TNode>(position, operation));
        }

        public void RemoveOperation(TNode position, Operation<TNode> operation)
        {
            RemoveEdge(new OperationEdge<TNode>(position, operation));
        }

        public void PrepareStartsToNameMap()
        {
            myStartsToNameMap = new Dictionary<TNode, string>();

            void CollectInternal(IMethodHolder<TNode> owner, string previous)
            {
                foreach (var pair in owner.CollectInternalMethods())
                {
                    var name = previous + Methods.GetKey(pair.Item1.GlobalId);

                    if (myStartsToNameMap.ContainsKey(pair.Item2.EntryPoint))
                    {
                        Console.WriteLine($"error: node {pair.Item2.EntryPoint} already has corresponding method: " +
                                          $"old: {myStartsToNameMap[pair.Item2.EntryPoint]}, new: {name}");
                    }

                    myStartsToNameMap[pair.Item2.EntryPoint] = name;

                    CollectInternal(pair.Item2, name);
                }
            }

            foreach (var resolvedClass in myClasses.Values)
            {
                foreach (var methodPair in resolvedClass.Methods())
                {
                    var name = Classes.GetKey(resolvedClass.Id.GlobalId) + "." + 
                               Methods.GetKey(methodPair.Item1.GlobalId);

                    if (myStartsToNameMap.ContainsKey(methodPair.Item2.EntryPoint))
                    {
                        Console.WriteLine($"error: node {methodPair.Item2.EntryPoint} already has corresponding method: " +
                                          $"old: {myStartsToNameMap[methodPair.Item2.EntryPoint]}, new: {name}");
                    }

                    myStartsToNameMap[methodPair.Item2.EntryPoint] = name;

                    CollectInternal(methodPair.Item2, name);
                }
            }
        }

        public string GetMethodByStartPoint(TNode startPoint)
        {
            myStartsToNameMap.TryGetValue(startPoint, out var name);

            return name ?? "Unknown function";
        }

        public IEnumerable<ResolvedMethod<TNode>> GetMethodsInFile(string file)
        {
            // TODO: existence check
            var found = Files.Find(file);
            if (found.HasValue)
            {
                return myFiles[found.Value].Owned.Select(
                    fullId => myClasses[fullId.ClassId].FindMethodInFullHierarchy(fullId.MethodId));
            }

            return Enumerable.Empty<ResolvedMethod<TNode>>();
        }

        public IEnumerable<ResolvedMethod<TNode>> CollectAllMethods()
        {
            return myClasses.Values.SelectMany(clazz => clazz.CollectAllInternalMethods());
        }

        public IEnumerable<ResolvedClass<TNode>> CollectAllClasses()
        {
            return myClasses.Values;
        }

        public ResolvedClass<TNode> GetOrCreateClass(ClassId name)
        {
            var rawClassId = Classes.Find(name.Value);

            if (!rawClassId.HasValue)
            {
                rawClassId = Classes.Add(name.Value);

                var classId = new ResolvedClassId(rawClassId.Value);
                var resolvedClass = new ResolvedClass<TNode>(classId);

                myClasses.Add(classId, resolvedClass);
                return resolvedClass;
            }

            return myClasses[new ResolvedClassId(rawClassId.Value)];
        }

        public ResolvedClass<TNode> FindClassById(ResolvedClassId id)
        {
            var exists = myClasses.TryGetValue(id, out var found);

            return exists ? found : null;
        }

        public ResolvedClassField GetOrCreateClassField(ResolvedClassId classId, string fieldName)
        {
            var clazz = myClasses[classId];
            var fieldId = ClassFields.FindOrAdd(fieldName);

            return clazz.GetOrCreateField(fieldId);
        }

        public ResolvedClassField FindClassFieldById(ResolvedClassId classId, int fieldId)
        {
            var clazz = myClasses[classId];

            return clazz.GetOrCreateField(fieldId);
        }

        public ResolvedMethod<TNode> GetOrCreateMethod(IMethodHolder<TNode> owner, string name)
        {
            var rawMethodId = Methods.FindOrAdd(name);
            var methodId = new ResolvedMethodId(rawMethodId);

            var method = owner.FindLocalMethod(methodId);
            if (method == null)
            {
                var start = myNodesProvider();
                var end = myNodesProvider();

                AddVertex(start);
                AddVertex(end);

                method = new ResolvedMethod<TNode>(methodId, start, end);

                owner.AddMethod(methodId, method);
                return method;
            }

            return method;
        }

        public ResolvedMethodId GetOrCreateMethodId(string name)
        {
            var id = Methods.FindOrAdd(name);

            // ReSharper disable once PossibleInvalidOperationException
            return new ResolvedMethodId(Methods.Find(name).Value);
        }

        public ResolvedMethod<TNode> FindMethodById(ResolvedClassId classId, ResolvedMethodId methodId)
        {
            if (classId == null)
            {
                return null;
            }

            myClasses.TryGetValue(classId, out var clazz);

            return clazz?.FindMethodInFullHierarchy(methodId);
        }

        public int GetOrCreateFieldId(string name)
        {
            var id = ClassFields.FindOrAdd(name);
            return id;
        }

        public void ClearMethod(ResolvedMethod<TNode> method)
        {
            var toRemove = method.Methods.Keys.ToList();
            foreach (var localFunction in toRemove)
            {
                RemoveMethod(method, localFunction);
            }

            var essential = new[] {method.Start, method.Final};
            var toDelete = method.GetOwnedNodes().Except(essential);
            foreach (var node in toDelete)
            {
                RemoveVertex(node);
                myOnNodeRemovedHandler(node);
            }

            RemoveOutEdgeIf(method.Start, _ => true);
            RemoveInEdgeIf(method.Final, _ => true);

            method.Clear();
        }

        public void RemoveMethod(IMethodHolder<TNode> owner, string methodName)
        {
            var methodId = Methods.Find(methodName);
            Trace.Assert(methodId.HasValue);

            RemoveMethod(owner, new ResolvedMethodId(methodId.Value));
        }

        public void RemoveMethod(IMethodHolder<TNode> owner, ResolvedMethodId methodId)
        {
            var method = owner.FindLocalMethod(methodId);

            var toRemove = method.Methods.Keys.ToList();
            foreach (var localFunction in toRemove)
            {
                RemoveMethod(method, localFunction);
            }

            var toDelete = method.GetOwnedNodes();
            foreach (var node in toDelete)
            {
                RemoveVertex(node);
                myOnNodeRemovedHandler(node);
            }

            var exists = owner.RemoveMethod(methodId);
            Trace.Assert(exists);
        }

        public void UpdateFile(string fileName, IEnumerable<ResolvedFullMethodId> fullMethodIds)
        {
            var possibleId = Files.Find(fileName);

            if (!possibleId.HasValue)
            {
                var id = Files.Add(fileName);
                var file = new File(id);
                myFiles.Add(id, file);

                file.Update(fullMethodIds, out _, out _, out _);
            }
            else
            {
                var file = myFiles[possibleId.Value];
                file.Update(fullMethodIds, out var removed, out _, out _);
                foreach (var methodId in removed)
                {
                    var owner = myClasses[methodId.ClassId];
                    RemoveMethod(owner, methodId.MethodId);
                }
            }
        }

        public void DumpToDot(string fileName)
        {
            var algorithm = new GraphvizAlgorithm<TNode, OperationEdge<TNode>>(this);
            algorithm.FormatEdge += (sender, args) => args.EdgeFormatter.Label.Value = args.Edge.Statement.ToString();
            algorithm.Generate(new FileDotEngine(), fileName);
        }

        public GraphStructuredProgramDump<TNode> Dump()
        {
            var edges = new List<OperationEdge<TNode>>(Edges);
            var files = myFiles.ToDictionary(pair => pair.Key, pair => pair.Value.Dump());
            return new GraphStructuredProgramDump<TNode>(
                edges, Files, Classes, Methods, ClassFields, myClasses, myFiles);
        }

        public void DumpVariablesToDot(string fileName)
        {
            var graph = new AdjacencyGraph<SecondaryEntity, TaggedEdge<SecondaryEntity, string>>();

            var allVariables = 
                CollectAllMethods()
                    .SelectMany(method => method.Variables.Values.Union(method.AdditionalVariables.Cast<SecondaryEntity>()))
                    .Concat(myClasses.Values
                        .SelectMany(clazz => clazz.Fields)
                        .Cast<SecondaryEntity>())
                    .ToList();

            foreach (var variable in allVariables)
            {
                graph.AddVertex(variable);
            }

            foreach (var variable in allVariables)
            {
                foreach (var (recipient, top) in variable.AllRecipients)
                {
                    var edge = new TaggedEdge<SecondaryEntity, string>(variable, recipient, top ? "t" : "b");
                    graph.AddEdge(edge);
                }
            }

            var algorithm = new GraphvizAlgorithm<SecondaryEntity, TaggedEdge<SecondaryEntity, string>>(graph);

            algorithm.FormatVertex += (sender, args) =>
                args.VertexFormatter.Label = String.Concat(
                    args.Vertex.CollectedPrimaries
                        .Select(reference => reference as ResolvedClassId)
                        .Where(entity => entity != null)
                        .Select(entity => entity.GlobalId.ToString() + " "));

            algorithm.FormatEdge += (sender, args) =>
            {
                args.EdgeFormatter.Label.Value = "";
                args.EdgeFormatter.StrokeColor = args.Edge.Tag == "t" ? Color.Red : Color.Blue;
            };

            algorithm.Generate(new FileDotEngine(), fileName);
        }
    }
}
