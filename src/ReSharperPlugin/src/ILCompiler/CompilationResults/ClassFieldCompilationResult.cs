using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types.Ids;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.CompilationResults
{
    public sealed class ClassFieldCompilationResult : ElementCompilationResult
    {
        public TreeNodeCollection<IAttribute> Attributes { get; }
        public ClassId ContainingClass { get; }
        public string FieldName { get; }

        public ClassFieldCompilationResult(
            InstructionBlock instructionBlock, 
            ClassId containingClass, 
            string fieldName,
            TreeNodeCollection<IAttribute> attributes)
            : base(instructionBlock)
        {
            Attributes = attributes;
            ContainingClass = containingClass;
            FieldName = fieldName;
        }
    }
}
