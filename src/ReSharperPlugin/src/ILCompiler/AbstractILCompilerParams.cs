using System;
using System.Collections.Generic;
using System.Linq;
using Cofra.AbstractIL.Common;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.Contracts.Messages.Requests;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.UI.Controls.TreeListView.Contracts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp.Impl.Cache2;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.ILCompiler
{
    public class AbstractILCompilerParams
    {
        public readonly OneToListMap<IConstructor, IConstructor> ConstructorBases;
        public readonly OneToListMap<IMethod, IMethod> HierarchyMembers;

        private CSharpCacheProvider CacheProvider;

        //[NotNull] public IResolveContext ResolveContext { get; private set; }
        public File File;
        public IPsiSourceFile SourceFile;

        private int myNextInstructionIndex;

        private readonly IPersistentIndexManager myPersistentIndexManager;
        
        public readonly LocalVariableIndexer LocalVariableIndexer;

        private readonly AnonFunctionNamer myAnonFunctionNamer = new AnonFunctionNamer();

        private readonly List<Request> myCollectedInteractiveRequests;

        public IEnumerable<Request> CollectedInteractiveRequests => myCollectedInteractiveRequests;
        
        public AbstractILCompilerParams(
            /*[NotNull] IResolveContext resolveContext, */ File file,
            OneToListMap<IMethod, IMethod> hierarchyMembers,
            IPsiSourceFile sourceFile,
            IPersistentIndexManager persistentIndexManager,
            /*Dictionary<Type, Pointer> exceptionTypeToPointer, */
            CSharpCacheProvider cacheProvider,
            bool buildExpressions = true)
        {
            myPersistentIndexManager = persistentIndexManager; 
            SourceFile = sourceFile; 
            HierarchyMembers = hierarchyMembers;
            File = file;
            CacheProvider = cacheProvider;
            //ExceptionTypeToPointer = exceptionTypeToPointer;
            //ResolveContext = resolveContext;
            InterruptCheck = () =>
            {
                var interruptHandler = InterruptableActivityCookie.GetCheck();
                return interruptHandler != null && interruptHandler();
            };
            //BuildExpressions = buildExpressions;
            
            LocalVariableIndexer = new LocalVariableIndexer(this);

            myCollectedInteractiveRequests = new List<Request>();
        }

        public Func<bool> InterruptCheck { get; }
        
        private readonly Stack<Class> myCurrentClasses = new Stack<Class>();
        private readonly Stack<Method> myCurrentMethods = new Stack<Method>();

        public Reference CreateAnonymousFunction(IDeclaredElement declaredElemMethod)
        {
            var method = CreateMethod(declaredElemMethod, myAnonFunctionNamer.GetNextName());
            return new LocalFunctionReference(method.Id);
        }

//        public void CreateFakeProperty(string name)
//        { 
//            var currentMethod = new Method(new MethodId(name));
//            var vars = new List<Reference>();
//            
//            currentMethod.Variables = vars;
//            currentMethod.FillWithIndices();
//            
//            if (!myCurrentMethods.IsEmpty())
//                myCurrentMethods.Peek().LocalMethods.Add(currentMethod);
//            else
//                myCurrentClasses.Peek().Methods.Add(currentMethod);
//            
//        }

        public Method CreateMethod(IDeclaredElement declaredElemMethod, string name = null)
        {
            var method = new Method(name == null ? CompilerUtils.GetMethodId(declaredElemMethod) : new MethodId(name));
            myCurrentMethods.Push(method);
            
            LocalVariableIndexer.InitForNewMethod(declaredElemMethod as IParametersOwner);
            return method;
        }

        public void FinishCurrentMethod()
        {
            if (myCurrentMethods.IsEmpty())
                throw CreateException("no method to add to the current class");
            var currentMethod = myCurrentMethods.Pop();

            if (myCurrentMethods.Count == 0)
                myAnonFunctionNamer.Reset();
            
            for (var i = -1; i <= LocalVariableIndexer.GetCurrentVariableIndex(); i++)
                currentMethod.Variables.Add(new LocalVariableReference(i));
            
            try
            {
                currentMethod.FillWithIndices();
            }
            catch (Exception e)
            {
                throw CreateException("Index filling failed", e.StackTrace);
            }
            
            if (!myCurrentMethods.IsEmpty())
                myCurrentMethods.Peek().LocalMethods.Add(currentMethod);
            else
                myCurrentClasses.Peek().Methods.Add(currentMethod);

            LocalVariableIndexer.FinishMethod();
        }
        
        public Class CreateClass(string className)
        {
            var @class = new Class(new ClassId(className));
            myCurrentClasses.Push(@class);
            return @class;
        }

        public void FinishCurrentClass()
        {
            if (myCurrentClasses.IsEmpty()) 
                throw CreateException("no class to finish");
            File.Classes.Add(myCurrentClasses.Pop());
        }

        public void AddClassField(string name)
        {
            if (myCurrentClasses.IsEmpty())
                throw CreateException("field is not in class");
            myCurrentClasses.Peek().Fields.Add(name);
        }

        public Method GetCurrentMethod()
        {
            return myCurrentMethods.IsEmpty() ? throw CreateException("can't get current method") : myCurrentMethods.Peek();
        }

        public Class GetCurrentClass()
        {
            return myCurrentClasses.IsEmpty() ? throw CreateException("Can't get current class or method") : myCurrentClasses.Peek();
        }

        public ClassReference GetBaseOfCurrentClass()
        {
            if (GetCurrentClass().BaseClass == null)
                throw CreateException("Base class is null");
            return new ClassReference(GetCurrentClass().BaseClass);
        }

        public ClassReference GetCurrentClassReference()
        {
            return new ClassReference(GetCurrentClass().ClassId);
        }
        
        public InstructionId GetNewInstructionId()
        {
            return new InstructionId(myNextInstructionIndex++);
        }

        public int GetPersistentFileId()
        {
            return myPersistentIndexManager[SourceFile];
        }

        public string GetOwnerForLocalFunction()
        {
            return $"{GetCurrentClass().ClassId.Value}:{GetCurrentMethod().Id.Value}";
        }

        public Exception CreateException(string info, string stacktrace = "")
        {
            var @class = myCurrentClasses.IsEmpty() ? "" : myCurrentClasses.Peek().ClassId.ToString();
            var method = myCurrentMethods.IsEmpty() ? "" : myCurrentMethods.Peek().Id.ToString();
            return new Exception($"{info}\nFile: {SourceFile.Name}. Class: {@class}. Method: {method}.\n{stacktrace}");
        }

        public void CheckExistenceOfLocalFunction(LocalFunctionReference localFunctionReference)
        {
            if (myCurrentMethods.IsEmpty())
                throw CreateException($"no such local function {localFunctionReference}");    
            var names = myCurrentMethods.Peek().LocalMethods.Select(method => method.Id.Value).ToList();
            if (!(names.Contains(localFunctionReference.MethodId.Value)))
            {
                throw CreateException($"no such local function {localFunctionReference}. Actual names:[{ names.Join(";")}]");
            }
        }

        public void CheckAndAddIfTainted(ClassFieldCompilationResult compiled)
        {
            var tainted = compiled.Attributes.Any(attribute => attribute.Name.ShortName == "Tainted");
            if (tainted)
            {
                var request = new TaintClassFieldRequest(compiled.ContainingClass, compiled.FieldName);
                myCollectedInteractiveRequests.Add(request);
            }
        }
    }

    public class AnonFunctionNamer
    {
        private int myLocalFunctionsIndex = 0;

        public string GetNextName()
        {
            return $"Anon{myLocalFunctionsIndex++}";
        }

        public void Reset()
        {
            myLocalFunctionsIndex = 0;
        }
    }

    public class LocalVariableIndexer
    {
        private int myNextVariableIndex = 0;
        private readonly Dictionary<string, LocalVariableReference> myVariableNameToIndex =
            new Dictionary<string, LocalVariableReference>();
        
        private Stack<List<string>> myCurrentMethodVariableNames = new Stack<List<string>>();
        private AbstractILCompilerParams myParams;

        public LocalVariableIndexer(AbstractILCompilerParams @params)
        {
            myParams = @params;
        }

        public void InitForNewMethod(IParametersOwner declaredElement = null)
        {
            myCurrentMethodVariableNames.Push(new List<string>());
            if (declaredElement == null) return;
            foreach (var parameter in declaredElement.Parameters)
            {
                GetNextVariable(parameter.ShortName);
            }
        }

        public void FinishMethod()
        {
            if (myCurrentMethodVariableNames.IsEmpty()) return;
            foreach (var name in myCurrentMethodVariableNames.Pop())
            {
                myVariableNameToIndex.Remove(name);
            }
            
            if (myCurrentMethodVariableNames.IsEmpty())
                Reset();
        }
        
        public void Reset()
        {
            myNextVariableIndex = 0;
            myVariableNameToIndex.Clear();
            //myCurrentMethodVariableNames.Clear();
        }

        public int GetCurrentVariableIndex()
        {
            return myNextVariableIndex - 1;
        }

        public LocalVariableReference GetVariableIndex(string name)
        {
            if (!myVariableNameToIndex.TryGetValue(name, out var index))
                throw myParams.CreateException("variable not found in variable indexer");
            return index;
        }

        public LocalVariableReference GetNextVariable(string name = null)
        {
            if (name == null) return new LocalVariableReference(myNextVariableIndex++);
            
            if(myCurrentMethodVariableNames.IsEmpty())
                throw myParams.CreateException("stack of vars is empty");

            if (!myVariableNameToIndex.ContainsKey(name))
            {
                myVariableNameToIndex.Add(name, new LocalVariableReference(myNextVariableIndex++));
                myCurrentMethodVariableNames.Peek().Add(name);
            }

            return myVariableNameToIndex[name];
        }
    }
}