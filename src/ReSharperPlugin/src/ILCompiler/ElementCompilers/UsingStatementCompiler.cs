using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class UsingStatementCompiler : ElementCompiler
    {
        private IUsingStatement myUsingStatement;

        public UsingStatementCompiler(IUsingStatement usingStatement, AbstractILCompilerParams @params) : base(@params)
        {
            myUsingStatement = usingStatement;
        }
    }
}