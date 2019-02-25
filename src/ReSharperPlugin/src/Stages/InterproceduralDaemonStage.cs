using System;
using System.Collections.Generic;
using System.Linq;
using Cofra.AbstractIL.Common;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types;
using Cofra.ReSharperPlugin.ILCompiler;
using Cofra.ReSharperPlugin.SolutionComponents;
using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp.Stages;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Feature.Services.CSharp.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches.Persistence;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl.Cache2;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.Stages
{
    [DaemonStage(StagesBefore = new[] {typeof(LanguageSpecificDaemonStage)})]
    internal class InterproceduralDaemonStage : CSharpDaemonStageBase
    {
        protected override IDaemonStageProcess CreateProcess(IDaemonProcess process,
            IContextBoundSettingsStore settings,
            DaemonProcessKind processKind, ICSharpFile file)
        {
            return new InterproceduralDaemonStageProcess(process, file);
        }
    }

    internal class InterproceduralDaemonStageProcess : CSharpDaemonStageProcessBase
    {
        private readonly bool isVisibleFile;

        private readonly OneToListMap<IConstructor, IConstructor> myConstructorBases =
            new OneToListMap<IConstructor, IConstructor>();

        private readonly OneToListMap<IMethod, IMethod> myHierarchyMembers = new OneToListMap<IMethod, IMethod>();

        private readonly ISolution mySolution;

        public InterproceduralDaemonStageProcess(
            [NotNull] IDaemonProcess process, 
            [NotNull] ICSharpFile file) :
            base(process, file)
        {
            mySolution = process.Solution;
            
            var completedStageProcesses = process.GetCompletedStageProcesses();
            var filtered = completedStageProcesses.Where(completedProcess => completedProcess is IHierarchyAnalysisStageProcess).ToList();
            var hierarchyAnalysisStageProcess = filtered.IsEmpty() ? null : filtered.First() as IHierarchyAnalysisStageProcess;
            if (hierarchyAnalysisStageProcess == null) return;

            foreach (var memberInheritanceInfo in hierarchyAnalysisStageProcess.MemberInheritances)
            {
                if (memberInheritanceInfo.Member is IMethod method)
                {
                    myHierarchyMembers.AddValue(method, memberInheritanceInfo.BaseMember as IMethod);
                }
//                else if (memberInheritanceInfo.Member is IConstructor constructor)
//                {
//                    System.IO.File.WriteAllText(@"C:\methodsExceptions\constructors.txt", constructor.ShortName + '\n');
//                    myConstructorBases.AddValue(constructor, memberInheritanceInfo.BaseMember as IConstructor);
//                }
            }
        }

        public override void Execute(Action<DaemonStageResult> committer)
        {
            var persistentIndexManager = mySolution.GetComponent<PersistentIndexManager>();
            var component = mySolution.GetComponent<CofraFacade>();
            var cacheProvider = CSharpLanguage.Instance.CacheProvider() as CSharpCacheProvider;
            var fileId = new FileId(DaemonProcess.SourceFile.GetPersistentID());
            var file = new File(fileId);
            var compilerParams = new AbstractILCompilerParams(file, myHierarchyMembers, DaemonProcess.SourceFile, persistentIndexManager, cacheProvider);
            var compiler = new AbstractILCompiler(compilerParams);
            File.ProcessDescendants(compiler);

            if (!compilerParams.InterruptCheck())
            {
                component.SubmitFile(file);

                foreach (var request in compilerParams.CollectedInteractiveRequests)
                {
                    component.Client.EnqueueRequest(request, _ => {});
                }
            }
        }
    }
}