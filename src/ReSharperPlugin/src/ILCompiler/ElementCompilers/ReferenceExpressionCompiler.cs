using System;
using System.Linq;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.DeclaredElements;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl.Types;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ReferenceExpressionCompiler : ExpressionCompiler
    {
        private readonly IReferenceExpression myReferenceExpression;
        
        public ReferenceExpressionCompiler(IReferenceExpression referenceExpression, AbstractILCompilerParams @params) :
            base(@params)
        {
            myReferenceExpression = referenceExpression;
        }

        public override ICompilationResult GetResult()
        {
            // TODO: refactor
            var resultsWithoutTypeParameter = MyChildToResult.Where(kvp => !(kvp.Key is ITypeArgumentList))
                .Select(kvp => kvp.Value)
                .ToList();
            var instructionBlock0 = GetInstructionsConnectedSequentially(resultsWithoutTypeParameter);
            var reference0 = GetReferenceFromResults(resultsWithoutTypeParameter);
            var withOutGetter = new ExpressionCompilationResult(instructionBlock0, null, reference0);
            //
            var resultWithGetter = withOutGetter.TryConvertToGetter(MyParams);
            var instructions = resultWithGetter.GetInstructionBlock();
            var childReference = resultWithGetter.GetReference();
            
            
            var declaredElement = myReferenceExpression.Reference.Resolve().DeclaredElement;
            if (declaredElement == null) return new ExpressionCompilationResult(instructions, GetLocation(myReferenceExpression));
            var referenceName = declaredElement.GetNameWithHash();
            switch (declaredElement)
            {
                case IField field:
                    var defaultFieldType = field.Type.IsClassType() ? new ClassId(field.Type.ToString()) : null;
                    myReference = new ClassFieldReference(GetReferenceToOwner(childReference), referenceName, defaultFieldType);
                    break;
                case ILocalFunctionDeclaration _:
                    if (childReference != null)
                        throw MyParams.CreateException($"local function invocation({referenceName}) has owner({childReference})");
                    myReference = new ClassMethodReference(new ClassReference(new ClassId(MyParams.GetOwnerForLocalFunction())), referenceName);
                    break;
                case IMethod _:
                    myReference = new ClassMethodReference(GetReferenceToOwner(childReference), referenceName);
                    break;
                case IProperty _:
                    myReference = new ClassPropertyReference(GetReferenceToOwner(childReference), referenceName);
                    break;
                case IParameter param:
                    var owner = param.ContainingParametersOwner ?? throw MyParams.CreateException("method parameter's owner is null");
                    var parameterNumber = owner.Parameters.IndexOf(param);

                    var defaultType = param.Type.IsClassType() ? new ClassId(param.Type.ToString()) : null;
                    myReference = new LocalVariableReference(parameterNumber, defaultType);

                    break;
                case IClass @class:
                    myReference = @class.GetClassReference();
                    break;
                case IStruct @struct:
                    myReference = @struct.GetClassReference();
                    break;
                case IEnum @enum:
                    myReference = @enum.GetClassReference();
                    break;
                case IExternAlias _:
                case INamespace _:
                    myReference = null;
                    break;
                case IInterface @interface:
                    myReference = @interface.GetClassReference();
                    break;
                case IEvent @event:
                    myReference = new ClassFieldReference(GetReferenceToOwner(childReference), referenceName);
                    break;
                case IAnonymousTypeProperty _ :
                    //todo
                    break;
                case ITypeParameter _ :
                    //todo
                    break;
//                case ISingleVariableDesignation _:
//                case ILocalConstantDeclaration _:
//                case ILocalVariableDeclaration _:
//                case ICatchVariableDeclaration _:
                case IVariableDeclaration variableDeclaration :
                    if (childReference != null) throw MyParams.CreateException("");
                    myReference = MyParams.LocalVariableIndexer.GetVariableIndex(referenceName);
                    ((LocalVariableReference) myReference).DefaultType = 
                        variableDeclaration.Type.IsClassType() ? new ClassId(variableDeclaration.Type.ToString()) : null;
                    break;
                case IDelegate @delegate:
                    myReference = new ClassMethodReference(GetReferenceToOwner(childReference), referenceName);
                    break;
                default:
                {
                    throw MyParams.CreateException($"unsupported reference type {declaredElement.GetType()}");
                    // reference to method or field
//                    var nameOfIdentifier = myReferenceExpression.NameIdentifier.Name;
//
//                    if (myReferenceExpression.FirstChild != null)
//                    {
//                        if (MyChildToResult[myReferenceExpression.FirstChild] is IExpressionCompilationResult varOwner)
//                        {
//                            myReference = new ClassMemberReference(varOwner.GetReference(), nameOfIdentifier);
//                        }
//                        else
//                        {
//                            throw MyParams.GetException("Owner of class member is not an expression");
//                        }
//                    }
//                    else
//                    {
//                        throw MyParams.GetException("Class member has no no owner");
//                    }
//
//                    break;
                }
            }

            if (myReference is LocalVariableReference localVariable)
            {
            }

            return new ExpressionCompilationResult(instructions, GetLocation(myReferenceExpression), myReference);
        }

        private Reference GetReferenceToOwner(Reference childReference)
        {
            if (childReference == null || myReferenceExpression.FirstChild is IThisExpression)
            {
                //self field || method || property
                return new ClassReference(MyParams.GetCurrentClass().ClassId);
            }

            return childReference;
        }
    }
}