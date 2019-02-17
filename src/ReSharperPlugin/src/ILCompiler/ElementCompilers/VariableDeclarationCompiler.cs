using System;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class VariableDeclarationCompiler : ExpressionCompiler
    {
        private readonly IVariableDeclaration myVariableDeclaration;
        
        public VariableDeclarationCompiler(IVariableDeclaration variableDeclaration,
            AbstractILCompilerParams @params) : base(@params)
        {
            myVariableDeclaration = variableDeclaration;
            var variableName = myVariableDeclaration.DeclaredName;
            try
            {
                MyParams.LocalVariableIndexer.GetNextVariable(variableName);
            }
            catch (Exception e)
            {
                throw MyParams.CreateException($"var stacks exception");
            }
        }
    }
}