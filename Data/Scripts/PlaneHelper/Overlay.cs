using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace PlaneHelper
{
    public class Overlay : IDisposable
    {
        public const int TICKS_BETWEEN_AUTO_UPDATE = 60;
        public const int TICKS_BETWEEN_AUTO_SELECT = 60;
        public const int GRID_SELECT_RAYCAST_LENGTH = 150;
        public const int TICKS_BETWEEN_PROPELLER_SWITCH = 120;

        private readonly string[] _validPropSubtypeIds = new string[] { "Prop2B", "Prop3B", "Prop4B" };

        public OverlayState State { get; private set; } = new OverlayState();

        private PlaneHelperMod _mod;
        private int _ticks = 0;

        public Overlay(PlaneHelperMod mod)
        {
            _mod = mod;
        }

        public void UpdateAfterSimulation()
        {
            if (!State.IsEnabled)
                return;

            _ticks++;

            if (State.AutoSelect && (_ticks % TICKS_BETWEEN_AUTO_SELECT == 0))
            {
                UpdateSelectedGrid();
            }

            if (State.AutoUpdate && (_ticks % TICKS_BETWEEN_AUTO_UPDATE == 0))
            {
                UpdateSelectedPropellers();
            }

            if (State.FullShouldAlternate && State.FullPropIndex >= 0 && State.SelectedPropellers.Count > 0 && (_ticks % TICKS_BETWEEN_PROPELLER_SWITCH == 0))
            {
                State.FullPropIndex = (State.FullPropIndex + 1) % State.SelectedPropellers.Count;
            }
        }

        public void UpdateSelectedGrid()
        {
            IMyEntity controlledEntity = MyAPIGateway.Session?.ControlledObject?.Entity;
            if (controlledEntity == null)
                return;

            IMyTerminalBlock controlledBlock = controlledEntity as IMyTerminalBlock;
            if (controlledBlock != null)
            {
                if (controlledBlock.CubeGrid == State.SelectedGrid)
                    return;

                if (controlledBlock.CubeGrid.GridSizeEnum == MyCubeSize.Large)
                    return;

                State.SelectedGrid = controlledBlock.CubeGrid;
                UpdateSelectedPropellers();
                return;
            }

            List<IHitInfo> toList = new List<IHitInfo>();
            Vector3D from = MyAPIGateway.Session.Camera.WorldMatrix.Translation;
            Vector3D to = from + MyAPIGateway.Session.Camera.WorldMatrix.Forward * GRID_SELECT_RAYCAST_LENGTH;
            MyAPIGateway.Physics.CastRay(from, to, toList);
            foreach (var hitInfo in toList)
            {
                if (hitInfo.HitEntity == null)
                    continue;

                IMyCubeGrid foundGrid = hitInfo.HitEntity as IMyCubeGrid;

                if (foundGrid == null)
                    continue;

                if (foundGrid == State.SelectedGrid)
                    return;

                if (foundGrid.GridSizeEnum == MyCubeSize.Large)
                    return;

                State.SelectedGrid = foundGrid;
                UpdateSelectedPropellers();
                return;
            }
        }

        public void UpdateSelectedPropellers()
        {
            if (!Utils.IsEntityValid(State.SelectedGrid))
                return;

            State.SelectedPropellers.Clear();
            foreach (var block in State.SelectedGrid.GetFatBlocks<IMyFunctionalBlock>())
            {
                if (_validPropSubtypeIds.Contains(block.BlockDefinition.SubtypeId))
                    State.SelectedPropellers.Add(block);
            }
        }

        public void Draw()
        {
            if (!State.IsEnabled)
                return;

            if (_ticks < 60 || !Utils.IsEntityValid(State.SelectedGrid) || State.SelectedPropellers.Count == 0)
                return;

            MyStringId lineMaterial = MyStringId.GetOrCompute("Square");
            MyStringId boxMaterial = MyStringId.GetOrCompute("Square");
            Color color = State.InterferingPositionColor;
            Vector4 colorVec = color.ToVector4();

            foreach (var propA in State.SelectedPropellers)
            {
                if (!Utils.IsEntityValid(propA))
                    continue;

                Vector3I propATip = Utils.FindPropellerTip(propA);
                Base6Directions.Direction propADirection = propA.SlimBlock.Orientation.Forward;

                foreach (var propB in State.SelectedPropellers)
                {
                    if (!Utils.IsEntityValid(propB) || propA == propB)
                        continue;

                    Vector3I propBTip = Utils.FindPropellerTip(propB);

                    if (Utils.WouldInterfere(propATip, propBTip, propADirection))
                    {
                        Vector3D propATipWorld = propA.CubeGrid.GridIntegerToWorld(propATip);
                        Vector3D propBTipWorld = propB.CubeGrid.GridIntegerToWorld(propBTip);

                        MySimpleObjectDraw.DrawLine(propATipWorld, propBTipWorld, lineMaterial, ref colorVec, 0.05f, VRageRender.MyBillboard.BlendTypeEnum.Standard);
                    }
                }
            }

            if (State.OverlayMode == OverlayMode.Full)
            {
                Vector3D gridSizeVector = Vector3D.One * State.SelectedGrid.GridSize;
                BoundingBoxD gridBlockBox = new BoundingBoxD(-gridSizeVector * 0.5f, gridSizeVector * 0.5f);

                for (int i = 0; i < State.SelectedPropellers.Count; i++)
                {
                    if (State.FullPropIndex >= 0)
                    {
                        if (State.FullPropIndex != i)
                            continue;
                    }

                    IMyFunctionalBlock prop = State.SelectedPropellers[i];

                    if (!Utils.IsEntityValid(prop))
                        continue;

                    IMyCubeGrid propGrid = prop.CubeGrid;
                    Vector3I propTipPosition = Utils.FindPropellerTip(prop);
                    Base6Directions.Direction propDirection = prop.SlimBlock.Orientation.Forward;

                    MatrixD propWM = prop.WorldMatrix;
                    propWM = MatrixD.CreateWorld(propWM.Translation, propWM.Up, propWM.Forward);

                    MySimpleObjectDraw.DrawTransparentCylinder(ref propWM, (Constants.MAX_PERPENDICULAR_DISTANCE - 1) / 2, (Constants.MAX_PERPENDICULAR_DISTANCE - 1) / 2, State.FullRadius, ref colorVec, true, 4, 0.05f, boxMaterial);

                    int radius = Constants.MAX_MANHATTEN_DISTANCE - 1;
                    for (int x = -radius; x <= radius; x++)
                    {
                        for (int y = -radius; y <= radius; y++)
                        {
                            for (int z = -radius; z <= radius; z++)
                            {
                                int absTotal = Math.Abs(x) + Math.Abs(y) + Math.Abs(z);
                                if (absTotal == radius || absTotal == radius - 1)
                                {
                                    Vector3I offset = new Vector3I(x, y, z);
                                    Vector3D worldPosition = propGrid.GridIntegerToWorld(propTipPosition + offset);
                                    MatrixD drawMatrix = prop.WorldMatrix;
                                    drawMatrix.Translation = worldPosition;

                                    if (Utils.WouldInterfere(propTipPosition, propTipPosition + offset, propDirection))
                                        MySimpleObjectDraw.DrawTransparentBox(ref drawMatrix, ref gridBlockBox, ref color, MySimpleObjectRasterizer.Wireframe, 1, 0.01f, boxMaterial, boxMaterial, onlyFrontFaces: true, blendType: VRageRender.MyBillboard.BlendTypeEnum.Standard);
                                }

                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {

        }
    }
}
