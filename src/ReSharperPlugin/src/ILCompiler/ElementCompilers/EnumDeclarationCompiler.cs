using Cofra.AbstractIL.Common.Types;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
  internal class EnumDeclarationCompiler : ElementCompiler
  {
    private IEnumDeclaration myEnumDeclaration;
    
    public EnumDeclarationCompiler(IEnumDeclaration enumDeclaration, AbstractILCompilerParams myParams) : base(myParams)
    {
      myEnumDeclaration = enumDeclaration;
      myParams.CreateClass(myEnumDeclaration.DeclaredElement.GetClrName().FullName);
    }

    public override ICompilationResult GetResult()
    {
        MyParams.FinishCurrentClass();
        return base.GetResult();
    }
  }
}