
namespace Assets.Gamelogic.Core
{
    public static class BuildSettings
    {
        public static readonly string UnityClientScene = "UnityClient";
		public static readonly string LoginScene = "Login";

		public static readonly string ClientDefaultActiveScene = LoginScene;
		public static readonly string[] ClientScenes = { LoginScene, UnityClientScene };

        public static readonly string UnityWorkerScene = "UnityWorker";
        public static readonly string WorkerDefaultActiveScene = UnityWorkerScene;
        public static readonly string[] WorkerScenes = { UnityWorkerScene };

        public const string SceneDirectory = "Assets";
    }
}