using Improbable.Unity.Internal;
using UnityEditor;

namespace Improbable.Unity.EditorTools.Init
{
    [InitializeOnLoad]
    public class InitializeSDKLogging
    {
        static InitializeSDKLogging()
        {
            SDKLogging.InitializeSDKLogging();
        }
    }
}
