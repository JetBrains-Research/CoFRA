using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ClassFieldDeclarationCompiler: ElementCompiler
    {
        private IFieldDeclaration myFieldDeclaration;
        public ClassFieldDeclarationCompiler(IFieldDeclaration fieldDeclaration, AbstractILCompilerParams myParams) : base(myParams)
        {
            myFieldDeclaration = fieldDeclaration;
        }

        public override ICompilationResult GetResult()
        {
            return base.GetResult();
        }
    }
}