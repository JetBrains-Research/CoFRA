using System.Linq;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class LocalFunctionDeclarationCompiler : ExpressionCompiler
    {
        private ILocalFunctionDeclaration myLocalFunctionDeclaration;
        public LocalFunctionDeclarationCompiler(ILocalFunctionDeclaration localFunctionDeclaration, AbstractILCompilerParams @params) : base(@params)
        {
            myLocalFunctionDeclaration = localFunctionDeclaration;
            MyParams.CreateMethod(myLocalFunctionDeclaration.DeclaredElement);
        }
        
        public override ICompilationResult GetResult()
        {
            var instructionBlock = GetInstructionsConnectedSequentially(MyResults);

            var method = MyParams.GetCurrentMethod();
            
            method.FillWithInstructions(instructionBlock);
            MyParams.FinishCurrentMethod();
            
            return new ElementCompilationResult();
        }
    }
}