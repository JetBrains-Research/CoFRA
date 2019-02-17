using System;
using System.Collections.Generic;
using System.Linq;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal interface IElementCompiler
    {
        [CanBeNull] ICompilationResult GetResult();
        void AddChildResult(ITreeNode node, ICompilationResult child);
    }

    internal class ElementCompiler : IElementCompiler
    {
        public ElementCompiler(AbstractILCompilerParams myParams)
        {
            MyParams = myParams;
        }

        [NotNull]
        protected Dictionary<ITreeNode, ICompilationResult> MyChildToResult { get; } =
            new Dictionary<ITreeNode, ICompilationResult>();

        [NotNull] protected List<ICompilationResult> MyResults { get; } = new List<ICompilationResult>();
        [NotNull] protected AbstractILCompilerParams MyParams { get; }
        //[NotNull] protected ITreeNode myNode { get; }

        public ElementCompiler Parent { get; set; }

        public virtual ICompilationResult GetResult()
        {
            return new ElementCompilationResult(GetInstructionsConnectedSequentially(MyResults));
        }

        public virtual void AddChildResult(ITreeNode element, ICompilationResult result)
        {
            MyChildToResult.Add(element, result);
            MyResults.Add(result);
        }
        
        [NotNull]
        public static InstructionBlock GetInstructionsConnectedSequentially(IEnumerable<IInstructionsContainer> instructionContainers)
        {
            var nonEmptyContainers = instructionContainers.Where(res => res != null && !res.IsEmpty()).ToList();

            if (nonEmptyContainers.Count == 0) return new InstructionBlock();

            var newInstructions = new List<Instruction>();
            
            newInstructions.AddRange(nonEmptyContainers.First().GetInstructionBlock().Instructions);
            
            for (var index = 0; index < nonEmptyContainers.Count - 1; index++)
            {
                ConnectTwoContainers(nonEmptyContainers[index], nonEmptyContainers[index + 1]);
                newInstructions.AddRange(nonEmptyContainers[index + 1].GetInstructionBlock().Instructions);
            }

            return new InstructionBlock(nonEmptyContainers.First().GetInstructionBlock().InitialInstructions,
                newInstructions,
                nonEmptyContainers.Last().GetInstructionBlock().Continuations);
        }

        protected static InstructionBlock ConnectOneWithManyResultsToInstructionBlock(IInstructionsContainer first,
            IEnumerable<IInstructionsContainer> others)
        {
            if (first == null) throw new ArgumentNullException();
            if (others == null) throw new ArgumentNullException();

            var notNullOthers = others.Where(res => res != null && !res.IsEmpty()).ToList();

            if (notNullOthers.Count == 0) return first.GetInstructionBlock();

            var newInstructions = new List<Instruction>();
            var newContinuations = new List<Continuation>();
            var initialInstructions = new List<InstructionId>();
            foreach (var other in notNullOthers)
            {
                newInstructions.AddRange(other.GetInstructionBlock().Instructions);
                newContinuations.AddRange(other.GetInstructionBlock().Continuations);
            }

            if (first.IsEmpty())
            {
                foreach (var res in notNullOthers)
                    initialInstructions.AddRange(res.GetInstructionBlock().InitialInstructions);

                return new InstructionBlock(initialInstructions, newInstructions, newContinuations);
            }

            initialInstructions.AddRange(first.GetInstructionBlock().InitialInstructions);
            newInstructions.AddRange(first.GetInstructionBlock().Instructions);

            foreach (var continuation in first.GetInstructionBlock().Continuations)
            foreach (var other in notNullOthers)
                continuation.NextInstructions.AddRange(other.GetInstructionBlock().InitialInstructions);

            return new InstructionBlock(initialInstructions, newInstructions, newContinuations);
        }

        private static void ConnectTwoContainers(IInstructionsContainer first, IInstructionsContainer second)
        {
            foreach (var continuation in first.GetInstructionBlock().Continuations)
                FillContinuation(continuation, second.GetInstructionBlock());
        }

        private static void FillContinuation(Continuation continuation, InstructionBlock instructions)
        {
            foreach (var instruction in instructions.InitialInstructions)
                continuation.NextInstructions.Add(instruction);
        }

        protected Location GetLocation(ICSharpTreeNode expression)
        {
            var start = expression.GetDocumentStartOffset().Offset;
            var end = expression.GetDocumentEndOffset().Offset;
            return new Location(MyParams.GetPersistentFileId(), start, end);
        }
    }
}