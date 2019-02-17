using System;
using System.Collections.Generic;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.DeclaredElements;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ReferenceNameCompiler : ElementCompiler
    {
        private IReferenceName myReferenceName;
        public ReferenceNameCompiler(IReferenceName node, AbstractILCompilerParams myParams) : base(myParams)
        {
            myReferenceName = node;
        }

        public override ICompilationResult GetResult()
        {
            var referenceDeclaredElement = myReferenceName.Reference.Resolve().DeclaredElement;
            Reference myReference;
            switch (referenceDeclaredElement)
            {
                case IClass @class:
                    myReference = @class.GetClassReference();
                    break;
                case IStruct @struct:
                    myReference = @struct.GetClassReference();
                    break;
                case IEnum @enum:
                    myReference = @enum.GetClassReference();
                    break;
                default:
                    myReference = null;
                    break;
            }
            return new ExpressionCompilationResult(new InstructionBlock(), GetLocation(myReferenceName), reference: myReference);
        }
    }
}