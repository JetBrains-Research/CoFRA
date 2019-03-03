using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cofra.AbstractIL.Common.Statements;
using Cofra.ReSharperPlugin.SolutionComponents;
using Cofra.ReSharperPlugin.Stages.Processors;
using Cofra.ReSharperPlugin.Warnings;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Stages;
using JetBrains.ReSharper.Feature.Services.CSharp.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches.Persistence;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.Stages
{
    [DaemonStage(LongRunningStage = true, StagesBefore = new[] {typeof(InterproceduralDaemonStage)})]
    internal sealed class TaintedEntityHighlighterDaemonStage : CSharpDaemonStageBase 
    {
        protected override IDaemonStageProcess CreateProcess(
            IDaemonProcess process, 
            IContextBoundSettingsStore settings,
            DaemonProcessKind processKind, 
            ICSharpFile file)
        {
            var persistentIndexManager = process.Solution.GetComponent<PersistentIndexManager>().NotNull();

            //TODO: get rid of this workaround (do not create process at all if file is not visible)
            var fileIsVisible = processKind == DaemonProcessKind.VISIBLE_DOCUMENT;

            return new TaintedEntityHighlighterDaemonStageProcess(
                process, file, persistentIndexManager, fileIsVisible);
        }
    }

    internal sealed class TaintedEntityHighlighterDaemonStageProcess : CSharpDaemonStageProcessBase
    {
        [NotNull] private readonly CofraFacade myCofraFacade;
        [NotNull] private readonly PersistentIndexManager myPersistentIndexManager;

        private readonly bool myFileIsVisible;

        public TaintedEntityHighlighterDaemonStageProcess(
            [NotNull] IDaemonProcess process,
            [NotNull] ICSharpFile sourceFile,
            [NotNull] PersistentIndexManager persistentIndexManager,
            bool visible)
            : base(process, sourceFile)
        {
            myCofraFacade = process.Solution.GetComponent<CofraFacade>();
            myPersistentIndexManager = persistentIndexManager;
            myFileIsVisible = visible;
        }

        private void CheckInterruptAndThrow()
        {
            var interruptCheck = InterruptableActivityCookie.GetCheck();
            if (interruptCheck != null && interruptCheck())
                throw new OperationCanceledException();
        }

        public override void Execute(Action<DaemonStageResult> committer)
        {
            if (!myFileIsVisible)
            {
                return;
            }

            var fieldsFinder = FindClassFields();
            var flags = RequestTaintedFields(fieldsFinder);

            var fields = fieldsFinder.FoundFields;

            var taintedRanges = fields
                .Zip(flags, (left, right) => (left.Item3, right))
                .Where(pair => pair.Item2)
                .Select(pair => pair.Item1);

            var highlightings = taintedRanges.Select(
                range => new HighlightingInfo(range, new TaintedWarning(range)));

            var result = new DaemonStageResult(highlightings.ToList());
            using (ReadLockCookie.Create())
            {
                committer(result);
            }
        }

        private ClassFieldsFinder FindClassFields()
        {
            var classFieldsFinder = new ClassFieldsFinder();
            File.ProcessDescendants(classFieldsFinder);

            return classFieldsFinder;
        }

        private IEnumerable<bool> RequestTaintedFields(ClassFieldsFinder finder)
        {
            var fields = finder.FoundFields;

            var myLock = new object();
            IEnumerable<bool> taintedFieldsFlags = null;

            void SetTaintedFieldsFlags(IEnumerable<bool> flags)
            {
                lock (myLock)
                {
                    taintedFieldsFlags = flags;
                }
            }

            bool GotResponse()
            {
                lock (myLock)
                {
                    return taintedFieldsFlags != null;
                }
            }

            var fieldLocations = fields.Select(field => (field.Item1, field.Item2));
            myCofraFacade.CheckIfFieldsAreTainted(fieldLocations, SetTaintedFieldsFlags);

            while (!GotResponse())
            {
                Thread.Sleep(100);
                CheckInterruptAndThrow();
            }

            return taintedFieldsFlags;
        }
    }
}