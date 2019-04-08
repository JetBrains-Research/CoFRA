using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.Warnings
{
    [StaticSeverityHighlighting(Severity.WARNING, "Tainted entity", OverlapResolve = OverlapResolveKind.WARNING)]
    internal class TaintedSinkWarning : IHighlighting
    {
        private const string MESSAGE = "{0}";
        private readonly DocumentRange myDocumentRange;
    
        public string ToolTip { get; }
        public string ErrorStripeToolTip { get; }

        public TaintedSinkWarning(
            IPersistentIndexManager persistentIndexManager,
            DocumentRange documentRange,
            List<Statement> trace)
        {
            myDocumentRange = documentRange;

            ErrorStripeToolTip = "";
            var innerTrace = string.Join("\n",
                trace.Skip(1).Take(trace.Count - 2).Select(statement => PrintStatement(persistentIndexManager, statement)));

            var writer = new StringWriter();
            writer.WriteLine("Tainted sink");
            writer.WriteLine(PrintStatement(persistentIndexManager, trace[0], true));

            if (innerTrace.Length > 0)
                writer.WriteLine(innerTrace);

            writer.WriteLine(PrintStatement(persistentIndexManager, trace.Last(), false, true));

            ToolTip = writer.ToString();
            ErrorStripeToolTip = ToolTip;
        }

        private string PrintStatement(
            IPersistentIndexManager persistentIndexManager,
            Statement statement,
            bool first = false,
            bool last = false)
        {
            var location = LocationToString(persistentIndexManager, statement.Location);

            if (statement is InfoStatement invocation)
                return $"{(last ? "sink" : "pass")} -> {location} " + invocation.Info;

            if (statement is ReturnStatement)
                return $"return <- {location}";

            if (statement is AssignmentStatement assignment)
                return $"{(first ? "source" : "assign")} - {location}";

            return "???";
        }

        private string LocationToString(IPersistentIndexManager persistentIndexManager, Location location)
        {
            if (location == null)
                return null;

            var source = persistentIndexManager[location.File];
            var document = source?.Document;
            if (document == null)
                return "???";

            var result = $"{source.Name}:{document.GetCoordsByOffset(location.StartOffset).Line.Plus1()}";
            return result;
        }
    
        public bool IsValid()
        {
            return myDocumentRange.IsValid();
        }

        public DocumentRange CalculateRange()
        {
            return myDocumentRange;
        }
    }
}