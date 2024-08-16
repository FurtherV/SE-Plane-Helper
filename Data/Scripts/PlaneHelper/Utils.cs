using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace PlaneHelper
{
    public static class Utils
    {
        public static Vector3I FindPropellerTip(IMyFunctionalBlock propeller)
        {
            IMyFunctionalBlock block = propeller;
            IMySlimBlock slim = propeller.SlimBlock;
            IMyCubeGrid grid = block.CubeGrid;

            Vector3I mid;
            int midLength;
            if (grid.GridSizeEnum == MyCubeSize.Small)
            {
                mid = (slim.Max - slim.Min);
                midLength = mid.Length();
                if (midLength > 1)
                    return (Vector3I)(mid * 0.5f + block.LocalMatrix.Backward * midLength * 0.5f + slim.Min);
                else if (mid == block.LocalMatrix.Backward)
                    return slim.Max;
                else
                    return slim.Min;
            }
            else
            {
                mid = (slim.Min - slim.Max);
                midLength = mid.Length();
                if (midLength > 1)
                    return (Vector3I)(mid * 0.5f + block.LocalMatrix.Forward * mid.Length() * 0.5f + slim.Max);
                else if (mid == block.LocalMatrix.Forward)
                    return slim.Min;
                else
                    return slim.Max;
            }
        }

        public static float CalculateManhattanDistance(Vector3I propellerPosition1, Vector3I propellerPosition2)
        {
            return Vector3I.DistanceManhattan(propellerPosition1, propellerPosition2);
        }

        public static int CalculatePerpendicularDistance(Vector3I propellerPosition1, Vector3I propellerPosition2, Base6Directions.Direction flowDirection)
        {
            // Calculate the Manhattan distance
            Vector3I diff = propellerPosition2 - propellerPosition1;

            // Convert the flow direction into a vector
            Vector3I directionVector = Base6Directions.GetIntVector(flowDirection);

            // Project the difference onto the flow direction vector
            int projection = Vector3I.Dot(diff, directionVector);

            // Calculate the perpendicular distance from the tube's axis
            Vector3I perpendicularDiff = diff - projection * directionVector;

            return Math.Abs(perpendicularDiff.X) + Math.Abs(perpendicularDiff.Y) + Math.Abs(perpendicularDiff.Z);
        }

        public static bool WouldInterfere(Vector3I propellerPosition1, Vector3I propellerPosition2, Base6Directions.Direction flowDirection)
        {
            float manhattanDistance = CalculateManhattanDistance(propellerPosition1, propellerPosition2);
            int perpendicularDistance = CalculatePerpendicularDistance(propellerPosition1, propellerPosition2, flowDirection);

            return manhattanDistance < Constants.MAX_MANHATTEN_DISTANCE || Constants.MAX_PERPENDICULAR_DISTANCE < 5;
        }

        public static bool IsEntityValid(IMyEntity entity)
        {
            return entity != null && !entity.MarkedForClose && !entity.Closed && entity.Visible;
        }
    }
}
