using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
  internal class ClassLikeDeclarationCompiler : ElementCompiler
  {
    private IClassLikeDeclaration myClassLikeDeclaration;
    
    public ClassLikeDeclarationCompiler(IClassLikeDeclaration classLikeDeclaration, AbstractILCompilerParams myParams) : base(myParams)
    {
      myClassLikeDeclaration = classLikeDeclaration;
      
      var newClass = myParams.CreateClass(myClassLikeDeclaration.DeclaredElement.GetClrName().FullName);
      if (myClassLikeDeclaration is IClassDeclaration classDeclaration)
      {
        var baseClass = new ClassId(classDeclaration.DeclaredElement.GetBaseClassType().GetClrName().FullName);
        newClass.BaseClass = baseClass;
      }
      
    }

    public override ICompilationResult GetResult()
    {
//        if (myClassLikeDeclaration is IClassDeclaration classDeclaration)
//        {
//          if (classDeclaration.ExtendsList != null && MyChildToResult.ContainsKey(classDeclaration.ExtendsList)
//              && MyChildToResult[classDeclaration.ExtendsList] is ExpressionCompilationResult parent)
//          {
//            var parentReference = parent.GetReference();
//            if (parentReference is ClassReference classReference)
//              MyParams.GetCurrentClass().BaseClass = classReference.ClassId;
//          }
//        }
        
        MyParams.FinishCurrentClass();
        return base.GetResult();
    }
  }
}