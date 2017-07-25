// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 
using Improbable.Unity.Core;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity.Logging
{
    /// <summary>
    /// Report exceptions to the logger.
    /// </summary>
    public class LogListener : MonoBehaviour
    {
        private static readonly string LoggerName = "Unity";

        public void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        public void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (!SpatialOS.IsConnected)
            {
                return;
            }

            bool shouldSendLog = false;
            switch (type)
            {
                case LogType.Log:
                    shouldSendLog = SpatialOS.Configuration.LogDebugToSpatialOs;
                    break;
                case LogType.Assert:
                    shouldSendLog = SpatialOS.Configuration.LogAssertToSpatialOs;
                    break;
                case LogType.Warning:
                    shouldSendLog = SpatialOS.Configuration.LogWarningToSpatialOs;
                    break;
                case LogType.Error:
                    shouldSendLog = SpatialOS.Configuration.LogErrorToSpatialOs;
                    break;
                case LogType.Exception:
                    shouldSendLog = SpatialOS.Configuration.LogExceptionToSpatialOs;
                    break;
            }

            if (shouldSendLog)
            {
                SpatialOS.Connection.SendLogMessage(GetLogLevel(type), LoggerName, string.Format("{0}\n in {1}", logString, stackTrace));
            }
        }

        private static LogLevel GetLogLevel(LogType logType)
        {
            switch (logType)
            {
                case LogType.Log:
                case LogType.Assert:
                    return LogLevel.Info;
                case LogType.Warning:
                    return LogLevel.Warn;
                case LogType.Error:
                case LogType.Exception:
                    return LogLevel.Error;
                default:
                    return LogLevel.Info;
            }
        }
    }
}