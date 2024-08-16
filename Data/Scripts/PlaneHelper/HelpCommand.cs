using Sandbox.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace PlaneHelper
{
    internal class HelpCommand : BaseCommand
    {
        public HelpCommand(CommandHandler handler) : base(handler, "help", "h") { }

        public override void Execute(MyCommandLine commandLine, string rawText)
        {
            string helpText = string.Join("\n",
                "Usage:",
                "Commands are not case sensitive.",
                "You can use /planehelper, /planeHelper as well as /ph for these commands.",
                "[optional option] denotes an optional argument while <required argument> denotes a required argument.",
                "Changes to the overlay state are not saved between sessions, reloads and worlds!",
                "",
                "Commands:",
                "/ph help - Shows this screen.",
                "/ph overlay <on/off/toggle> - Changes overlay enabled state.",
                "/ph overlay mode <0/1/simple/full> - Changes overlay type, simple shows props with interference while full shows all positions that would interfere.",
                "/ph overlay grid [auto] - Selects current or looked at grid for the overlay, auto will automatically update the current grid.",
                "/ph overlay updateBlocks [auto] - Updates propellers for the overlay, auto will automatically grab new propellers.",
                "/ph overlay prop <index/all/alt> - Changes for which propeller the full overlay is shown, if enabled.",
                "/ph overlay radius <Size> - Changes the full overlay radius, numbers greater than 20 can cause performance issues.",
                "/ph overlay color <Red> <Green> <Blue> [Alpha] - Changes the overlay color.",
                "",
                "Defaults:",
                "By default, the overlay is off and in simple mode.",
                "It will automatically select the current grid and propeller blocks though.",
                "If switched to full overlay mode, it will alternate between propellers.");
            MyAPIGateway.Utilities.ShowMissionScreen("PlaneHelper Command Overview", null, null, helpText, null, "Close");
        }
    }
}
