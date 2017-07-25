// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 
﻿using UnityEngine;

namespace Improbable.Metrics
{
    class StartTimer : MonoBehaviour
    {
        public ScriptLifecycleAnalytics Analytics { set; private get; }

        private void FixedUpdate()
        {
            Analytics.StartFixedUpdate();
        }

        private void Update()
        {
            Analytics.StartUpdate();
        }

        private void LateUpdate()
        {
            Analytics.StartLateUpdate();
        }
    }
}
