using System.Threading;
using JetBrains.TestFramework;
using NUnit.Framework;

[assembly: RequiresThread(ApartmentState.STA)]
namespace ReSharperPlugin.Tests
{
    [SetUpFixture]
    public class TestEnvironment : ExtensionTestEnvironmentAssembly<IMyTestsZone>
    {
    }
}
