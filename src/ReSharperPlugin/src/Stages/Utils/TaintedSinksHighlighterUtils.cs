using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.ReSharperPlugin.Warnings;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.Stages.Utils
{
    public static class TaintedSinksHighlighterUtils
    {
        public static List<HighlightingInfo> HighlightingsFromStackTraces(
            [NotNull] IPersistentIndexManager persistentIndexManager,
            IDocument thisDocument,
            IEnumerable<IEnumerable<Statement>> stackTraces)
        {
            var highlightings =
                stackTraces.Select(stackTrace => stackTrace.ToList())
                    .Select(stackTrace =>
                    {
                        var frame = stackTrace.Last();
                        var range = LocationToDocumentRange(persistentIndexManager, frame.Location);
                        if (!range.IsValid())
                        {
                            return null;
                        }

                        if (range.Document.Equals(thisDocument))
                        {
                            using (ReadLockCookie.Create())
                            {
                                return CreateHighlightingInfo(persistentIndexManager, range, stackTrace);
                            }
                        }

                        return null;
                    })
                    .WhereNotNull();
            
            return highlightings.ToList();
        }

        private static DocumentRange LocationToDocumentRange(
            [NotNull] IPersistentIndexManager persistentIndexManager, Location location)
        {
            if (location == null)
            {
                return DocumentRange.InvalidRange;
            }

            var document = persistentIndexManager[location.File]?.Document;

            if (document == null)
            {
                return DocumentRange.InvalidRange;
            }

            var textRange = new TextRange(location.StartOffset, location.EndOffset);
            return new DocumentRange(document, textRange);
        }

        private static HighlightingInfo CreateHighlightingInfo(
            IPersistentIndexManager persistentIndexManager, DocumentRange documentRange, List<Statement> stackTrace)
        {
            return new HighlightingInfo(documentRange,
                new TaintedSinkWarning(
                    persistentIndexManager,
                    documentRange,
                    stackTrace));
        }
    }
}
