using UnityEngine;

namespace Assets.Gamelogic.Core
{
    public static class SimulationSettings
    {
        public static readonly int TargetClientFramerate = 60;
        public static readonly int TargetServerFramerate = 60;
        public static readonly int FixedFramerate = 20;

		public static readonly float HeartbeatCheckIntervalSecs = 3;
		public static readonly uint TotalHeartbeatsBeforeTimeout = 3;
		public static readonly float HeartbeatSendingIntervalSecs = 3;

        public static readonly string DefaultSnapshotPath = Application.dataPath + "/../../../snapshots/default.snapshot";
    }
}
