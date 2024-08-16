using Sandbox.ModAPI;
using VRage.Game.Components;

namespace PlaneHelper
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PlaneHelperMod : MySessionComponentBase
    {
        public const string MOD_NAME = "Plane Helper";
        public const string MOD_VERSION = "1.1.0";

        public static PlaneHelperMod Instance { get; private set; } // the only way to access session comp from other classes and the only accepted static.

        public Overlay Overlay { get; private set; }
        public CommandHandler CommandHandler { get; private set; }

        public bool ModEnabled => MyAPIGateway.Multiplayer.IsServer && !MyAPIGateway.Utilities.IsDedicated;

        public override void LoadData()
        {
            // amogst the earliest execution points, but not everything is available at this point.

            if (!ModEnabled)
                return;

            Instance = this;

            Overlay = new Overlay(this);
            CommandHandler = new CommandHandler(this);
        }

        public override void BeforeStart()
        {
            // executed before the world starts updating

            // Main entry point: MyAPIGateway
            // Entry point for reading/editing definitions: MyDefinitionManager.Static

            // Commands only work for non-dedicated server player.

            if (!ModEnabled)
                return;

            CommandHandler.Init();

            MyAPIGateway.Utilities.ShowMessage(MOD_NAME, $"Version {MOD_VERSION} initialized.");

            SetUpdateOrder(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.Simulation | MyUpdateOrder.AfterSimulation);
        }

        protected override void UnloadData()
        {
            // executed when world is exited to unregister events and stuff

            CommandHandler.Dispose();
            Overlay.Dispose();

            CommandHandler = null;
            Overlay = null;

            Instance = null; // important for avoiding this object to remain allocated in memory
        }

        public override void HandleInput()
        {
            // gets called 60 times a second before all other update methods, regardless of framerate, game pause or MyUpdateOrder.
        }

        public override void UpdateBeforeSimulation()
        {
            // executed every tick, 60 times a second, before physics simulation and only if game is not paused.
        }

        public override void Simulate()
        {
            // executed every tick, 60 times a second, during physics simulation and only if game is not paused.
            // NOTE in this example this won't actually be called because of the lack of MyUpdateOrder.Simulation argument in MySessionComponentDescriptor
        }

        public override void UpdateAfterSimulation()
        {
            // executed every tick, 60 times a second, after physics simulation and only if game is not paused.

            Overlay.UpdateAfterSimulation();
        }

        public override void Draw()
        {
            // gets called 60 times a second after all other update methods, regardless of framerate, game pause or MyUpdateOrder.
            // NOTE: this is the only place where the camera matrix (MyAPIGateway.Session_Component1.Camera.WorldMatrix) is accurate, everywhere else it's 1 frame behind.

            if (!ModEnabled)
                return;

            Overlay.Draw();
        }

        public override void SaveData()
        {
            // executed AFTER world was saved
        }

        public override void UpdatingStopped()
        {
            // executed when game is paused
        }
    }
}
