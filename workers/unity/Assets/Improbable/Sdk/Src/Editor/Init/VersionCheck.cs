// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// ===========
using UnityEditor;

namespace Improbable.Unity.EditorTools.Init
{
    [InitializeOnLoad]
    public class VersionCheck
    {
        static VersionCheck()
        {
#if !(UNITY_5_5_3) && !(UNITY_5_6_0)
            UnityEngine.Debug.LogWarning("SpatialOS SDK: You are using an unsupported version of Unity. The currently supported versions are 5.5.3/5.6.0.");
#endif
        }
    }
}
