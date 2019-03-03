using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;

namespace Cofra.ReSharperPlugin.Actions
{
    public abstract class BasicAction : IExecutableAction
    {
        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            return true; 
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            RunAction(context, nextExecute);
        }

        protected abstract void RunAction(IDataContext context, DelegateExecute nextExecute);
    }
}