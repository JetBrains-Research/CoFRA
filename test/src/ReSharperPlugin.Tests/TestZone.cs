using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework.Application.Zones;

namespace ReSharperPlugin.Tests
{
    [ZoneDefinition]
    public interface IMyTestsZone : ITestsEnvZone, IRequire<PsiFeatureTestZone>
    {
    }
}
