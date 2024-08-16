using Sandbox.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace PlaneHelper
{
    public class OverlayCommand : BaseCommand
    {
        private OverlayState State => Handler.Mod?.Overlay?.State;

        public OverlayCommand(CommandHandler handler) : base(handler, "overlay", "o") { }

        public override void Execute(MyCommandLine commandLine, string rawText)
        {
            if (State == null) return;

            string subCommand = commandLine.Argument(1)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(subCommand)) return;

            switch (subCommand)
            {
                case "on":
                    ToggleOverlay(true);
                    break;

                case "off":
                    ToggleOverlay(false);
                    break;

                case "toggle":
                    ToggleOverlay(!State.IsEnabled);
                    break;

                case "mode":
                    SetOverlayMode(commandLine.Argument(2));
                    break;

                case "grid":
                    SetGridMode(commandLine.Argument(2));
                    break;

                case "updateblocks":
                    UpdateBlocks(commandLine.Argument(2));
                    break;

                case "prop":
                    SetPropMode(commandLine.Argument(2));
                    break;

                case "radius":
                    SetRadius(commandLine.Argument(2));
                    break;

                case "color":
                    SetColor(commandLine);
                    break;

                default:
                    Handler.ShowMessage($"{subCommand} is not a valid command!");
                    break;
            }
        }

        private void ToggleOverlay(bool isEnabled)
        {
            State.IsEnabled = isEnabled;
            Handler.ShowMessage($"Overlay: {State.IsEnabled}");
        }

        private void SetOverlayMode(string mode)
        {
            if (string.IsNullOrEmpty(mode))
            {
                Handler.ShowMessage("No state specified!");
                return;
            }

            switch (mode.ToLowerInvariant())
            {
                case "simple":
                case "0":
                    State.OverlayMode = OverlayMode.Simple;
                    break;

                case "full":
                case "1":
                    State.OverlayMode = OverlayMode.Full;
                    break;

                default:
                    Handler.ShowMessage($"{mode} is not a valid mode!");
                    return;
            }
            Handler.ShowMessage($"Mode: {State.OverlayMode}");
        }

        private void SetGridMode(string mode)
        {
            State.AutoSelect = "auto".Equals(mode, System.StringComparison.InvariantCultureIgnoreCase);
            Handler.Mod.Overlay.UpdateSelectedGrid();
            Handler.ShowMessage($"Auto: {State.AutoSelect}");
            Handler.ShowMessage($"Grid: {State.SelectedGrid?.DisplayName}");
        }

        private void UpdateBlocks(string mode)
        {
            State.AutoUpdate = "auto".Equals(mode, System.StringComparison.InvariantCultureIgnoreCase);
            Handler.Mod.Overlay.UpdateSelectedPropellers();
            Handler.ShowMessage($"Auto: {State.AutoSelect}");

            for (int i = 0; i < State.SelectedPropellers.Count; i++)
            {
                IMyFunctionalBlock prop = State.SelectedPropellers[i];
                Handler.ShowMessage($"Found {prop?.CustomName}, Index = {i}");
            }
        }

        private void SetPropMode(string propMode)
        {
            if ("all".Equals(propMode, System.StringComparison.InvariantCultureIgnoreCase))
            {
                State.FullShouldAlternate = false;
                State.FullPropIndex = -1;
                Handler.ShowMessage("Full Overlay: All propellers.");
            }
            else if ("alt".Equals(propMode, System.StringComparison.InvariantCultureIgnoreCase))
            {
                State.FullShouldAlternate = true;
                State.FullPropIndex = 0;
                Handler.ShowMessage("Full Overlay: Alternating between propellers.");
            }
            else
            {
                int index;
                if (int.TryParse(propMode, out index) && index >= 0 && index < State.SelectedPropellers.Count)
                {
                    State.FullShouldAlternate = false;
                    State.FullPropIndex = index;
                    IMyFunctionalBlock prop = State.SelectedPropellers[index];
                    Handler.ShowMessage($"Full Overlay: Showing {prop?.CustomName}, Index = {index}");
                }
                else
                {
                    Handler.ShowMessage($"{propMode} is not a valid index of a propeller!");
                }
            }
        }

        private void SetRadius(string radiusStr)
        {
            int radius;
            if (int.TryParse(radiusStr, out radius) && radius >= 0)
            {
                State.FullRadius = radius;
                Handler.ShowMessage($"Full Overlay: Radius is now {State.FullRadius}");
            }
            else
            {
                Handler.ShowMessage("Invalid radius component!");
            }
        }

        private void SetColor(MyCommandLine commandLine)
        {
            int red, green, blue, alpha = 255;

            if (int.TryParse(commandLine.Argument(2), out red) &&
                int.TryParse(commandLine.Argument(3), out green) &&
                int.TryParse(commandLine.Argument(4), out blue))
            {
                if (commandLine.Argument(5) != null && !int.TryParse(commandLine.Argument(5), out alpha))
                {
                    alpha = 255;
                }

                State.InterferingPositionColor = new VRageMath.Color(red, green, blue, alpha);
                Handler.ShowMessage($"Color: {State.InterferingPositionColor}");
            }
            else
            {
                Handler.ShowMessage("Invalid color components!");
            }
        }
    }
}
