// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 
using System;
using Improbable.Unity.Core;
using UnityEngine;

namespace Improbable.Unity.Util
{
    internal class WorkerMetricsMemoryUsage : MonoBehaviour
    {
        private void Update()
        {
            SpatialOS.Metrics.SetGauge("Unity used heap size", GC.GetTotalMemory( /* forceFullCollection */ false));
        }
    }
}