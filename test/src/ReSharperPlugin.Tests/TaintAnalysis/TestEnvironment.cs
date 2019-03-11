using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Application.Infra;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Resources;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TestFramework;
using JetBrains.Threading;
using NUnit.Framework;

[assembly: RequiresSTA]
namespace ReSharperPlugin.Tests.TaintAnalysis
{
    [SetUpFixture]
    public class TestEnvironment : ExtensionTestEnvironmentAssembly<IMyTestsZone>
    {
        public override void SetUp()
        {
            base.SetUp();
            /*
            ReentrancyGuard.Current.Execute("LoadAssemblies", () =>
            {
            });
            */
        }

        public override void TearDown()
        {
            /*
            ReentrancyGuard.Current.Execute("UnloadAssemblies", () => 
            {
              Shell.Instance.GetComponent<AssemblyManager>().UnloadAssemblies(GetType().Name, GetAssembliesToLoad());
            });
            */
            base.TearDown();
        }

        public static void Main(string[] args)
        {

        }
    }
}
