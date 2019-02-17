using Cofra.AbstractIL.Common;
using Cofra.AbstractIL.Common.Types;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class MethodDeclarationCompiler : ElementCompiler
    {
        private readonly IMethodDeclaration myMethodDeclaration;

        public MethodDeclarationCompiler(IMethodDeclaration methodDeclaration, AbstractILCompilerParams @params) : base(@params)
        {
            myMethodDeclaration = methodDeclaration;
            MyParams.CreateMethod(myMethodDeclaration.DeclaredElement);
        }

        public override ICompilationResult GetResult()
        {
            var declaredElemMethod = myMethodDeclaration.DeclaredElement;
            if (declaredElemMethod == null)
            {
                MyParams.FinishCurrentMethod();
                return new ElementCompilationResult();
            }

            var instructionBlock = GetInstructionsConnectedSequentially(MyResults);

            // do not analyse locks methods itself
            if (CompilerUtils.NeedToSkipMethod(declaredElemMethod))
            {
                MyParams.FinishCurrentMethod();
                return new ElementCompilationResult();
            }

            var method = MyParams.GetCurrentMethod();
            var baseMembers = MyParams.HierarchyMembers?.GetValuesSafe(declaredElemMethod);
            if (baseMembers != null)
            {
                foreach (var baseMember in baseMembers)
                {
                    var baseMethodId = CompilerUtils.GetMethodId(baseMember);
                    method.AddBase(baseMethodId);
                }
            }

            method.FillWithInstructions(instructionBlock);
            MyParams.FinishCurrentMethod();
            return new MethodDeclarationCompilationResult(method);
        }
    }
}