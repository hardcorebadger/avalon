// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 
using System.Diagnostics;

namespace Improbable.Unity.Logging
{
    class UnityTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public override void WriteLine(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}