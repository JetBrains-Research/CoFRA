using Cofra.AbstractIL.Common.Types.AnalysisSpecific;
using Cofra.ReSharperPlugin.SolutionComponents;
using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.Actions
{
    [Action("ActionPerformTaintAnalysis", "Perform taint analysis", Id = 4235783)]
    public sealed class PerformTaintAnalysisAction : BasicAction
    {
        protected override void RunAction(IDataContext context, DelegateExecute nextExecute)
        {
            var solution = context.GetData(JetBrains.ProjectModel.DataContext.ProjectModelDataConstants.SOLUTION);
            var cofra = solution?.GetComponent<CofraFacade>();

            if (cofra == null)
            {
                MessageBox.ShowInfo("Cofra or solution is null");
                return;
            }
            
            cofra.PerformAnalysis(AnalysisType.TaintChecking);
        }
    }
}
