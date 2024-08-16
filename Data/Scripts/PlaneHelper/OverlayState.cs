using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace PlaneHelper
{
    public class OverlayState
    {
        public bool IsEnabled { get; set; } = false;
        public bool AutoSelect { get; set; } = true;
        public bool AutoUpdate { get; set; } = true;
        public OverlayMode OverlayMode { get; set; } = OverlayMode.Simple;
        public IMyCubeGrid SelectedGrid { get; set; } = null;
        public List<IMyFunctionalBlock> SelectedPropellers { get; } = new List<IMyFunctionalBlock>();
        public Color InterferingPositionColor { get; set; } = Color.OrangeRed;
        public int FullRadius { get; set; } = 10;
        public int FullPropIndex { get; set; } = 0;
        public bool FullShouldAlternate { get; set; } = true;
        public OverlayState() { }
    }
}
