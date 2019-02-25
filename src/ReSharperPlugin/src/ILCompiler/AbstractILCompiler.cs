using System;
using Cofra.AbstractIL.Common.Types;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using Cofra.ReSharperPlugin.ILCompiler.ElementCompilers;
using JetBrains.ReSharper.Daemon.CSharp.Stages.Analysis;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler
{
    public class AbstractILCompiler : IRecursiveElementProcessor
    {
        //private ElementProcessingState myCurrentState;
        private readonly AbstractILCompilerParams @params;
        private ElementCompiler myCurrentNodeCompiler;

        public AbstractILCompiler(AbstractILCompilerParams compilerParams)
        {
            @params = compilerParams;
        }

        public bool InteriorShouldBeProcessed(ITreeNode element)
        {
            switch (element)
            {
                case IFieldDeclaration _:
                case IDestructorDeclaration _:
                case IDelegateDeclaration _:
                case ISignOperatorDeclaration _:
                case IConversionOperatorDeclaration _:
                case IUsingList _:
                    return false;
            }

            return true;
        }

        public bool ProcessingIsFinished
        {
            get
            {
                var interruptHandler = @params.InterruptCheck;
                if (interruptHandler != null)
                    return interruptHandler();
                return false;
            }
        }

        public void ProcessBeforeInterior(ITreeNode element)
        {
            //myCurrentState = new ElementProcessingState(myCurrentState);
            ElementCompiler currentNodeCompiler;
            switch (element)
            {
                // TODO : Ternary operator
//                case IDelegateDeclaration delegateDeclaration:
//                    currentNodeCompiler = new DelegateDeclarationCompiler(delegateDeclaration, @params);
//                    break;
                case  IArrowExpressionClause arrowExpressionClause:
                    currentNodeCompiler = new ArrowExpressionClauseCompiler(arrowExpressionClause, @params);
                    break;
                case IReferenceName referenceName:
                    currentNodeCompiler = new ReferenceNameCompiler(referenceName, @params);
                    break;
                case IBaseExpression baseExpression:
                    currentNodeCompiler = new BaseExpressionCompiler(baseExpression, @params);
                    break;
                case IEventDeclaration eventDeclaration:
                    currentNodeCompiler = new EventDeclarationCompiler(eventDeclaration, @params);
                    break;
                case IConstantDeclaration constantDeclaration:
                    currentNodeCompiler = new ConstantDeclarationCompiler(constantDeclaration, @params);
                    break;
                case IIndexerDeclaration indexerDeclaration:
                    currentNodeCompiler = new IndexerDeclarationCompiler(indexerDeclaration, @params);
                    break;
                case IElementAccessExpression elementAccessExpression:
                    currentNodeCompiler = new ElementAccessCompiler(elementAccessExpression, @params);
                    break;
                case IAccessorDeclaration accessor:
                    currentNodeCompiler = new AccessorDeclarationCompiler(accessor, @params);
                    break;
                case IFieldDeclaration fieldDeclaration:
                    currentNodeCompiler = new ClassFieldDeclarationCompiler(fieldDeclaration, @params);
                    break;
                case IPropertyDeclaration propertyDeclaration:
                    currentNodeCompiler = new PropertyDeclarationCompiler(propertyDeclaration, @params);
                    break;
                case IEnumDeclaration enumDeclaration:
                    currentNodeCompiler = new EnumDeclarationCompiler(enumDeclaration, @params);
                    break;
                case IAssignmentExpression assignmentExpression:
                    currentNodeCompiler = new AssignmentExpressionCompiler(assignmentExpression, @params);
                    break;
                case ILocalFunctionDeclaration localFunctionDeclaration:
                    currentNodeCompiler = new LocalFunctionDeclarationCompiler(localFunctionDeclaration, @params);
                    break;
                case IAnonymousFunctionExpression anonymousFunctionExpression:
                    currentNodeCompiler = new AnonymousFunctionCompiler(anonymousFunctionExpression, @params);
                    break;
                case IObjectCreationExpression objectCreationExpression:
                    currentNodeCompiler = new ObjectCreationExpressionCompiler(objectCreationExpression, @params);
                    break;
                case ILocalVariableDeclaration localVariableDeclaration:
                    currentNodeCompiler = new LocalVariableDeclarationCompiler(localVariableDeclaration, @params);
                    break;
                case IVariableDeclaration variableDeclaration:
                    currentNodeCompiler = new VariableDeclarationCompiler(variableDeclaration, @params);
                    break;
                case IArgumentList argumentList:
                    currentNodeCompiler = new ArgumentListCompiler(argumentList, @params);
                    break;
                case IInvocationExpression invocationExpression:
                    currentNodeCompiler = new InvocationExpressionCompiler(invocationExpression, @params);
                    break;
                case IMethodDeclaration methodDeclaration:
                    currentNodeCompiler = new MethodDeclarationCompiler(methodDeclaration, @params);
                    break;
                case IConstructorDeclaration constructorDeclaration:
                    currentNodeCompiler = new ConstructorDeclarationCompiler(constructorDeclaration, @params);
                    break;
                case IIfStatement ifStatement:
                    currentNodeCompiler = new IfStatementCompiler(ifStatement, @params);
                    break;
                case ISwitchBlock switchBlock:
                    currentNodeCompiler = new SwitchBlockCompiler(switchBlock, @params);
                    break;
                case IReturnStatement returnStatement:
                    currentNodeCompiler = new ReturnStatementCompiler(returnStatement, @params);
                    break;
                case IClassLikeDeclaration classLikeDeclaration:
                    currentNodeCompiler = new ClassLikeDeclarationCompiler(classLikeDeclaration, @params);
                    break;
                case IReferenceExpression referenceExpression:
                    currentNodeCompiler = new ReferenceExpressionCompiler(referenceExpression, @params);
                    break;
                case IThisExpression thisExpression:
                    currentNodeCompiler = new ThisExpressionCompiler(thisExpression, @params);
                    break;
                default:
                    currentNodeCompiler = new ExpressionCompiler(@params);
                    break;
//                case ICSharpExpression expression:
//                    currentNodeCompiler = new ExpressionCompiler(@params);
//                    break;
//                default:
//                    currentNodeCompiler = new ElementCompiler(@params);
//                    break;
            }

            currentNodeCompiler.Parent = myCurrentNodeCompiler;
            myCurrentNodeCompiler = currentNodeCompiler;
        }

        public void ProcessAfterInterior(ITreeNode element)
        {
            ICompilationResult result = null;
            try
            {
                if (myCurrentNodeCompiler == null) return;
                result = myCurrentNodeCompiler.GetResult();

                if (result is ClassFieldCompilationResult compiledClassField)
                {
                    @params.CheckAndAddIfTainted(compiledClassField);
                }
            }
            catch (Exception e)
            {
                //throw;
                System.IO.File.AppendAllText("C:\\work\\exceptions.txt",$"\n\n{e}\n\n");
            }
            finally
            {
                myCurrentNodeCompiler = myCurrentNodeCompiler.Parent;
                var res = result ?? new ExpressionCompilationResult();
                myCurrentNodeCompiler?.AddChildResult(element, res);
                
//                //todo:rework
//                if (element.Parent is IClassDeclaration classDeclaration &&
//                    classDeclaration.ExtendsList != null &&
//                    classDeclaration.ExtendsList == element &&
//                    res is ExpressionCompilationResult parent)
//                {
//                    var parentReference = parent.GetReference();
//                    if (parentReference is ClassReference classReference)
//                    {
//                        @params.GetCurrentClass().BaseClass = classReference.ClassId;
//                    }
//
//                }
            }
            
        }
    }
}