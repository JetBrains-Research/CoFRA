using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Statements;
using Cofra.ReSharperPlugin.SolutionComponents;
using Cofra.ReSharperPlugin.Stages.Utils;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Caches.Persistence;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.Stages
{
    [DaemonStage(LongRunningStage = true, StagesBefore = new[] {typeof(InterproceduralDaemonStage)})]
    public sealed class TaintedSinksHighlighterDaemonStage : IDaemonStage
    {
        public IEnumerable<IDaemonStageProcess> CreateProcess(IDaemonProcess process, IContextBoundSettingsStore settings, DaemonProcessKind processKind)
        {
            if (processKind != DaemonProcessKind.VISIBLE_DOCUMENT)
            {
                return Enumerable.Empty<IDaemonStageProcess>();
            }

            var persistentIndexManager = process.Solution.GetComponent<PersistentIndexManager>().NotNull();
            return new[] {new TaintedSinksHighlighterDaemonStageProcess(process.SourceFile, process, persistentIndexManager)};
        }
    }

    internal sealed class TaintedSinksHighlighterDaemonStageProcess : IDaemonStageProcess
    {
        [NotNull] private readonly IPsiSourceFile mySourceFile;
        [NotNull] private readonly CofraFacade myCofraFacade;
        [NotNull] private readonly IPersistentIndexManager myPersistentIndexManager;
        
        public IDaemonProcess DaemonProcess { get; }

        public TaintedSinksHighlighterDaemonStageProcess(
            [NotNull] IPsiSourceFile sourceFile,
            [NotNull] IDaemonProcess process,
            [NotNull] PersistentIndexManager persistentIndexManager)
        {
            DaemonProcess = process;
            mySourceFile = sourceFile ?? throw new ArgumentNullException();
            myCofraFacade = process.Solution.GetComponent<CofraFacade>();
            myPersistentIndexManager = persistentIndexManager;
        }

        private void CheckInterruptAndThrow()
        {
            var interruptCheck = InterruptableActivityCookie.GetCheck();
            if (interruptCheck != null && interruptCheck())
                throw new OperationCanceledException();
        }

        public void Execute(Action<DaemonStageResult> committer)
        {
            var myLock = new object();
            IEnumerable<IEnumerable<Statement>> receivedTraces = null;
            
            void SetResponseTraces(IEnumerable<IEnumerable<Statement>> traces)
            {
                lock (myLock)
                {
                    receivedTraces = traces ?? new List<IEnumerable<Statement>>();
                }
            }

            bool GotResponse()
            {
                lock (myLock)
                {
                    return receivedTraces != null;
                }
            }
             
            myCofraFacade.GetTaintedSinks(myPersistentIndexManager[mySourceFile], SetResponseTraces);

            while (!GotResponse())
            {
                Thread.Sleep(100);
                CheckInterruptAndThrow();
            }

            var highlightings = TaintedSinksHighlighterUtils.HighlightingsFromStackTraces(
                myPersistentIndexManager, mySourceFile.Document, receivedTraces);

            var result = new DaemonStageResult(highlightings);
            using (ReadLockCookie.Create())
            {
                committer(result);        
            }
        }
    }
}
