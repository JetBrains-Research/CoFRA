using Cofra.ReSharperPlugin.SolutionComponents;
using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.Actions
{
    [Action("ActionDumpIL", "Dump IL", Id = 4235782)]
    public class DumpILAction : SampleAction
    {
        protected override void RunAction(IDataContext context, DelegateExecute nextExecute)
        {
            var solution = context.GetData(JetBrains.ProjectModel.DataContext.ProjectModelDataConstants.SOLUTION);
            var cofra = solution?.GetComponent<CofraFacade>();

            if (cofra == null)
            {
                MessageBox.ShowInfo("Cofra or solution is null");
            }
            
            System.IO.File.WriteAllText("C:\\work\\ILDump.txt", cofra.GetLastFile().ToString());
        }
    }
}