using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class DelegateDeclarationCompiler : ExpressionCompiler
    {
        private readonly IDelegateDeclaration myDelegateDeclaration;
        
        public DelegateDeclarationCompiler(IDelegateDeclaration delegateDeclaration,
            AbstractILCompilerParams @params) : base(@params)
        {
            myDelegateDeclaration = delegateDeclaration;
        }

        public override ICompilationResult GetResult()
        {
            var variableName = myDelegateDeclaration.DeclaredName;
            MyParams.AddClassField(variableName);

            return new ElementCompilationResult();
        }
    }
}