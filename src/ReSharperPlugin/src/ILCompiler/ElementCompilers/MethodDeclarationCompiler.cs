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
            var method = MyParams.GetCurrentMethod();

            var declaredElemMethod = myMethodDeclaration.DeclaredElement;
            if (declaredElemMethod == null)
            {
                MyParams.FinishCurrentMethod();
                return new ElementCompilationResult();
            }

            foreach (var attribute in myMethodDeclaration.AttributesEnumerable)
            {
                method.AddAttribute(attribute.Name.ShortName);
            }

            var instructionBlock = GetInstructionsConnectedSequentially(MyResults);

            if (CompilerUtils.NeedToSkipMethod(declaredElemMethod))
            {
                MyParams.FinishCurrentMethod();
                return new ElementCompilationResult();
            }

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