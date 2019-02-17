using System;
using System.Linq;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl.Types;
using JetBrains.ReSharper.Psi.Util;
using AssignmentType = JetBrains.ReSharper.Psi.VB.Tree.AssignmentType;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class LocalVariableDeclarationCompiler : ElementCompiler
    {
        private readonly ILocalVariableDeclaration myLocalVariableDeclaration;
        private readonly LocalVariableReference myLocalVariableReference;

        public LocalVariableDeclarationCompiler(ILocalVariableDeclaration localVariableDeclaration,
            AbstractILCompilerParams @params) : base(@params)
        {
            myLocalVariableDeclaration = localVariableDeclaration;
            var variableName = myLocalVariableDeclaration.DeclaredName;
            myLocalVariableReference = MyParams.LocalVariableIndexer.GetNextVariable(variableName);
        }

        public override ICompilationResult GetResult()
        {
            var variableInitializer = myLocalVariableDeclaration.Initial;
            if (variableInitializer == null) return new ElementCompilationResult();

            if (!(MyChildToResult[variableInitializer] is IExpressionCompilationResult
                initialValueCompilationResult))
                throw MyParams.CreateException("initial value of variable is not an expression");

            // call getter
            var initialValueReference = initialValueCompilationResult.GetReference();

            if (initialValueReference == null)
            {
                return new ElementCompilationResult();
            }

            var location = GetLocation(myLocalVariableDeclaration);
            var assignmentStatement = new AssignmentStatement(location, initialValueReference, myLocalVariableReference);
            var instruction = new Instruction(assignmentStatement, MyParams.GetNewInstructionId());

            var containingType = myLocalVariableDeclaration.DeclaredElement.Type;
            myLocalVariableReference.DefaultType = containingType.IsClassType() ? new ClassId(containingType.ToString()) : null;

            return new ElementCompilationResult(GetInstructionsConnectedSequentially(
                new IInstructionsContainer[] {initialValueCompilationResult, instruction}));
        }
    }
}