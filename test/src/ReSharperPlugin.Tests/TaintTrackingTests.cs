using System.Collections.Generic;
using System.Linq;
using Cofra.ReSharperPlugin.SolutionComponents;
using Cofra.ReSharperPlugin.Stages;
using JetBrains.Application.Settings;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.Impl;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using NUnit.Framework;

namespace ReSharperPlugin.Tests
{
    [TestFixture]
    public sealed class TaintTrackingTests : CSharpHighlightingTestBase 
    {
        protected override string RelativeTestDataPath => "../data/TaintAnalysis";

        protected override IReadOnlyCollection<IDaemonStage> GetActiveStages(ISolution solution)
        {
            var stageManager = DaemonStageManager.GetInstance(solution);

            return stageManager.Stages
                .Where(stage => stage is InterproceduralDaemonStage ||
                                stage is TaintedSinksHighlighterDaemonStage)
                .ToList();
        }

        protected override bool HighlightingPredicate(
            IHighlighting highlighting,
            IPsiSourceFile sourceFile,
            IContextBoundSettingsStore settingsStore)
        {
            return true;
        }

        protected override void DoTest(Lifetime lifetime)
        {
            var cofra = Solution.GetComponent<CofraFacade>();
            cofra.DropCaches();

            base.DoTest(lifetime);
        }

        [TestCase("PassThroughLocals")]
        [TestCase("PassAsArgumentAndReturn")]
        [TestCase("ObjectTaintingByFieldAssignment")]
        [TestCase("SimpleRecursion")]
        [TestCase("MutualRecursion")]
        [TestCase("Delegates")]
        [TestCase("AssigningByThis")]
        [TestCase("ObjectConstruction")]
        public void Execute(string source)
        {
            DoOneTest(source);
        }
    }
}
