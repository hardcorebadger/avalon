// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 

using Improbable.Worker;

namespace Improbable.Unity.Configuration
{
    public static class Defaults
    {
        public const string DeploymentTag = "prod";
        public const int EntityCreationLimitPerFrame = 100;
        public const NetworkConnectionType LinkProtocol = NetworkConnectionType.RakNet;
        public const bool LogDebugToSpatialOs = false;
        public const bool LogAssertToSpatialOs = false;
        public const bool LogWarningToSpatialOs = true;
        public const bool LogErrorToSpatialOs = true;
        public const bool LogExceptionToSpatialOs = true;

        public const string InfraServiceUrl = "https://api.spatial.improbable.io";
        public const string ReceptionistAddress = "127.0.0.1";
        public const string LocatorAddress = "locator.improbable.io";
        public const ushort Port = 7777;
        public const string ProtocolLogPrefix = "protocol-";
        public const uint ProtocolLogMaxFileBytes = 100U * 1024U * 1024U;
        public const bool ProtocolLoggingOnStartup = false;
        public const uint RaknetHeartbeatTimeoutMillis = Worker.Defaults.RakNetHeartbeatTimeoutMillis;
        public const uint ReceiveQueueCapacity = 32768U;
        public const uint SendQueueCapacity = 16384U;
        public const bool ShowDebugTraces = false;
        public const byte TcpMultiplexLevel = Worker.Defaults.TcpMultiplexLevel;
        public const bool UseInstrumentation = true;
        public const bool UsePerInstanceAssetCache = true;
    }
}