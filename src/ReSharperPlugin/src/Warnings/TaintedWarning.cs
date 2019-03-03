using System.Collections.Generic;
using System.Linq;
using Cofra.AbstractIL.Common.Statements;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;

namespace Cofra.ReSharperPlugin.Warnings
{
    [StaticSeverityHighlighting(Severity.WARNING, "Tainted entity", OverlapResolve = OverlapResolveKind.WARNING)]
    internal class TaintedWarning : IHighlighting
    {
        private const string MESSAGE = "{0}";
        private readonly DocumentRange myDocumentRange;
    
        public string ToolTip { get; }
        public string ErrorStripeToolTip { get; }

        public TaintedWarning(
            DocumentRange documentRange)
        {
            myDocumentRange = documentRange;

            ToolTip = "Tainted";
            ErrorStripeToolTip = "???";
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