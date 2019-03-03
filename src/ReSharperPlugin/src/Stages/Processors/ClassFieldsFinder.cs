using System.Collections.Generic;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.ReSharperPlugin.ILCompiler;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace Cofra.ReSharperPlugin.Stages.Processors
{
    public class ClassFieldsFinder : IRecursiveElementProcessor
    {
        public List<(ClassId, string, DocumentRange)> FoundFields { get; }

        public ClassFieldsFinder()
        {
            FoundFields = new List<(ClassId, string, DocumentRange)>();
        }

        public bool InteriorShouldBeProcessed(ITreeNode element)
        {
            return element is IFieldDeclaration ||
                   element is IMultipleFieldDeclaration ||
                   element is IClassDeclaration ||
                   element is IClassBody ||
                   element is INamespaceDeclaration ||
                   element is INamespaceBody;
        }

        public void ProcessBeforeInterior(ITreeNode element)
        {
        }

        public void ProcessAfterInterior(ITreeNode element)
        {
            if (element is IFieldDeclaration fieldDeclaration)
            {
                //TODO: more lightweight name extraction
                var className = fieldDeclaration.DeclaredElement?.GetContainingType()?.GetClrName().FullName;

                if (className == null)
                {
                    return;
                }

                var classId = new ClassId(className);
                var fieldName = fieldDeclaration.DeclaredElement.GetNameWithHash();

                FoundFields.Add((classId, fieldName, fieldDeclaration.GetDocumentRange()));
            }
        }

        public bool ProcessingIsFinished => false;
    }
}
