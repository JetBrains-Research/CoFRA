using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ClassFieldDeclarationCompiler : ElementCompiler
    {
        private readonly IFieldDeclaration myFieldDeclaration;

        public ClassFieldDeclarationCompiler(
            IFieldDeclaration fieldDeclaration, 
            AbstractILCompilerParams myParams) 
            : base(myParams)
        {
            myFieldDeclaration = fieldDeclaration;
        }

        public override ICompilationResult GetResult()
        {
            var className = myFieldDeclaration.DeclaredElement?.GetContainingType()?.GetClrName().FullName;
            var classId = new ClassId(className);

            var fieldName = myFieldDeclaration.DeclaredElement.GetNameWithHash();

            return new ClassFieldCompilationResult(
                GetInstructionsConnectedSequentially(MyResults),
                classId,
                fieldName,
                myFieldDeclaration.Attributes);
        }
    }
}