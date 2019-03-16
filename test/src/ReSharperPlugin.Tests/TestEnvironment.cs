using JetBrains.TestFramework;
using NUnit.Framework;

[assembly: RequiresSTA]
namespace ReSharperPlugin.Tests
{
    [SetUpFixture]
    public class TestEnvironment : ExtensionTestEnvironmentAssembly<IMyTestsZone>
    {
    }
}
