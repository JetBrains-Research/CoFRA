using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class AccessorDeclarationCompiler: ElementCompiler
    {
        private IAccessorDeclaration myAccessor;
        public AccessorDeclarationCompiler(IAccessorDeclaration accessor, AbstractILCompilerParams myParams) : base(myParams)
        {
            myAccessor = accessor;
            MyParams.CreateMethod(accessor.DeclaredElement);
        }

        public override ICompilationResult GetResult()
        {
            var instructionBlock = GetInstructionsConnectedSequentially(MyResults);

            var method = MyParams.GetCurrentMethod();
            
            method.FillWithInstructions(instructionBlock);
            MyParams.FinishCurrentMethod();
            
            return new AccessorDeclarationCompilationResult(myAccessor.Kind);
        }
    }
}