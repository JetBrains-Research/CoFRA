using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class EventDeclarationCompiler : ExpressionCompiler
    {
        private readonly IEventDeclaration myEventDeclaration;
        
        public EventDeclarationCompiler(IEventDeclaration eventDeclaration,
            AbstractILCompilerParams @params) : base(@params)
        {
            myEventDeclaration = eventDeclaration;
        }

        public override ICompilationResult GetResult()
        {
            var variableName = myEventDeclaration.DeclaredName;
            MyParams.AddClassField(variableName);

            return new ElementCompilationResult();
        }
    }
}