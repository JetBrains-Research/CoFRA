using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ConstantDeclarationCompiler : ExpressionCompiler
    {
        private readonly IConstantDeclaration myConstantDeclaration;
        
        public ConstantDeclarationCompiler(IConstantDeclaration constantDeclaration,
            AbstractILCompilerParams @params) : base(@params)
        {
            myConstantDeclaration = constantDeclaration;
        }

        public override ICompilationResult GetResult()
        {
            var variableName = myConstantDeclaration.DeclaredName;
            MyParams.AddClassField(variableName);

            return new ElementCompilationResult();
        }
    }
}