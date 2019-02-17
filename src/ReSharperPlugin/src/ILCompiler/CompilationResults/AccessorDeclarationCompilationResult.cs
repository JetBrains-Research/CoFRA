using JetBrains.ReSharper.Psi;

namespace Cofra.ReSharperPlugin.ILCompiler.CompilationResults
{
    internal class AccessorDeclarationCompilationResult : ElementCompilationResult
    {
        public AccessorKind KindOfAccesor;

        public AccessorDeclarationCompilationResult(AccessorKind kindOfAccesor)
        {
            KindOfAccesor = kindOfAccesor;
        }
    }
}