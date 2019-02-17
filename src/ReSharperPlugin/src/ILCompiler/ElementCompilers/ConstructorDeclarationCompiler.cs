using Cofra.AbstractIL.Common.Types;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ConstructorDeclarationCompiler : ElementCompiler
    {
        private readonly IConstructorDeclaration myConstructorDeclaration;

        public ConstructorDeclarationCompiler(IConstructorDeclaration constructorDeclaration,
            AbstractILCompilerParams @params) : base(@params)
        {
            myConstructorDeclaration = constructorDeclaration;
            MyParams.CreateMethod(constructorDeclaration.DeclaredElement);
        }

        public override ICompilationResult GetResult()
        {
            var declaredElemMethod = myConstructorDeclaration.DeclaredElement;
            if (declaredElemMethod == null) return new ElementCompilationResult();

            var instructionBlock = GetInstructionsConnectedSequentially(MyResults);

            var method = MyParams.GetCurrentMethod();
            method.FillWithInstructions(instructionBlock);
//      var baseMembers = MyParams.HierarchyMembers?.GetValuesSafe(declaredElemMethod);
//      if (baseMembers != null)
//      {
//        foreach (var baseMember in baseMembers)
//        {
//          var baseMethodId = CompilerUtils.GetMethodId(baseMember);
//          method.AddBase(baseMethodId);
//        }
//      }

            MyParams.FinishCurrentMethod();
            return new ElementCompilationResult();
        }
    }
}