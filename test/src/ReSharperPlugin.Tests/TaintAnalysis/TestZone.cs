using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.ReSharperPlugin;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Daemon.SolutionAnalysis;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework.Application.Zones;

namespace ReSharperPlugin.Tests.TaintAnalysis
{
    [ZoneDefinition]
    public interface IMyTestsZone : ITestsEnvZone, IRequire<PsiFeatureTestZone>
    {
    }
}
