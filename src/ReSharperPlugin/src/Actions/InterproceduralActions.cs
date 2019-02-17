using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.UI.ActionSystem.ActionsRevised.Menu;
using JetBrains.IDE.Internal;
using JetBrains.ReSharper.Feature.Services.ExternalSources;

namespace Cofra.ReSharperPlugin.Actions
{
  [ActionGroup(ActionGroupInsertStyles.Submenu, Text = "Interprocedural", Id = 4234)]
  [ZoneMarker(typeof(ExternalSourcesZone))]
  public class InterproceduralActions : IAction, IInsertAfter<IntoInternalMenu, InternalWindowsMenu>
  {
    public InterproceduralActions(
      DumpILAction rAction)
    {
    }
  }
}