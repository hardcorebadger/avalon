// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// ===========

using System;
using System.Collections;
using System.Linq;
using Improbable.Unity.Configuration;
using Improbable.Unity.Entity;
using Improbable.Unity.Logging;
using Improbable.Unity.Util;
using Improbable.Worker;
using UnityEngine;
using Defaults = Improbable.Worker.Defaults;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Manages the tasks required to take a worker through its bootstrap lifecycle.
    /// </summary>
    internal class ConnectionLifecycle : MonoBehaviour
    {
        private readonly System.Collections.Generic.List<MonoBehaviour> managedBehaviours = new System.Collections.Generic.List<MonoBehaviour>();
        private DispatchEventHandler eventHandler;
        private WorkerConnectionParameters parameters;
        private SpatialCommunicator spatialCommunicator;
        private Connection connection;

        public DispatchEventHandler DispatchEventHandler { get { return eventHandler; } }
        public IUniverse Universe { get { return LocalEntities.Instance; } }
        public Deployment? Deployment { get; private set; }
        public View View { get; private set; }
        public Connection Connection { get { return connection; } }

        public IComponentCommander ComponentCommander { get; private set; }

        public Commander Commander { get; private set; }

        private IDeferredActionDispatcher deferredActionDispatcher;

        private LegacyEntityPipelineSetup legacyEntityPipeline;

        private void Start()
        {
            PrintClientInfo();

            parameters = CreateWorkerConnectionParameters(SpatialOS.Configuration);

            View = new View();
            View.OnDisconnect(SpatialOS.Disconnected);

            deferredActionDispatcher = new DeferredActionDispatcher();
            spatialCommunicator = new SpatialCommunicator(null, View, deferredActionDispatcher);
            eventHandler = new DispatchEventHandler(this, spatialCommunicator);

            InitializeEntityPipeline();

            // The current MonoBehaviour could be destroyed in response to aborted connection attempts,
            // causing leaks of unmanaged resources due to in-progress coroutines not running to completion.
            // Create a new host MonoBehaviour and run on it instead.
            var starter = gameObject.GetComponent<ConnectionCoroutineHost>() ?? gameObject.AddComponent<ConnectionCoroutineHost>();
            starter.StartCoroutine(Connect());
        }

        private void InitializeEntityPipeline()
        {
            // Setup legacy entity pipeline if alternative hasn't been set up.
            if (EntityPipeline.Internal.IsEmpty)
            {
                var config =
                    new LegacyEntityPipelineConfiguration(DispatchEventHandler.EntityInterestedComponentsUpdater,
                        DispatchEventHandler);

                legacyEntityPipeline = new LegacyEntityPipelineSetup(this /* Host MonoBehaviour */,
                    EntityPipeline.Instance,
                    spatialCommunicator,
                    LocalEntities.Internal, config);

                legacyEntityPipeline.Setup();
            }
            LocalEntities.Internal.Clear();
        }

        private IEnumerator Connect()
        {
            if (ShouldGetDeploymentList())
            {
                var callback = SpatialOS.OnDeploymentListReceived ?? OnDeploymentListReceived;
                var getList = WorkerConnection.GetDeploymentListAsync(parameters, callback, chosenDeployment => Deployment = chosenDeployment);

                yield return getList;
                var listWait = new WaitUntil(() => Deployment.HasValue);
                yield return listWait;
            }

            // Can't start prepooling assets until a deployment is chosen, since the Streaming strategy needs to know which assembly it should stream assets from.
            if (legacyEntityPipeline != null)
            {
                var prepareAssetsCoroutine = legacyEntityPipeline.PrepareAssets();
                if (prepareAssetsCoroutine != null)
                {
                    yield return prepareAssetsCoroutine;
                }
            }

            var connect = WorkerConnection.ConnectAsync(parameters, Deployment, AttachConnection);
            yield return connect;
            SetupComponents();

            if (SpatialOS.Configuration.ProtocolLoggingOnStartup)
            {
                Connection.SetProtocolLoggingEnabled(true);
            }

            eventHandler.ConnectionReady();

            var componentCommander = new ComponentCommander(null, spatialCommunicator);

            Commander = new Commander(componentCommander, spatialCommunicator);
            ComponentCommander = componentCommander;

            SpatialOS.Connected();
        }

        private void AttachConnection(Connection connectionToUse)
        {
            this.connection = connectionToUse;
            spatialCommunicator.AttachConnection(connectionToUse);
        }

        private static bool ShouldGetDeploymentList()
        {
            return !string.IsNullOrEmpty(SpatialOS.Configuration.LoginToken) ||
                   !string.IsNullOrEmpty(SpatialOS.Configuration.SteamToken);
        }

        private void Update()
        {
            ProcessEvents();
            eventHandler.ProcessEvents();

            if (SpatialOS.Disconnecting)
            {
                Dispose();
            }
        }

        private void OnApplicationQuit()
        {
            Dispose();
        }

        private void OnDeploymentListReceived(DeploymentList deployments, Action<Worker.Deployment> handleDeploymentChosen)
        {
            if (!string.IsNullOrEmpty(deployments.Error))
            {
                throw new Exception(string.Format("Could not retrieve deployment list for project '{0}': {1}", SpatialOS.Configuration.AppName, deployments.Error));
            }
            if (deployments.Deployments.Count == 0)
            {
                throw new Exception(string.Format("Could not find any deployments in project '{0}'", SpatialOS.Configuration.AppName));
            }

            Deployment chosenDeployment;
            if (string.IsNullOrEmpty(SpatialOS.Configuration.DeploymentId))
            {
                // No deployment was specified, so pick the first one returned by the Locator.
                chosenDeployment = deployments.Deployments.First();
                if (deployments.Deployments.Count > 1)
                {
                    Debug.LogError(string.Format("Locator returned {0} deployments, picking first the first one ('{1}')", deployments.Deployments.Count, chosenDeployment.DeploymentName));
                }
            }
            else
            {
                // A deployment was specified, so we either find it or fail.
                chosenDeployment = deployments.Deployments.Find(d => d.DeploymentName == SpatialOS.Configuration.DeploymentId);
                if (chosenDeployment.Equals(default(Deployment)))
                {
                    throw new Exception(string.Format("Could not find deployment '{0}' in project '{1}'", SpatialOS.Configuration.DeploymentId, SpatialOS.Configuration.AppName));
                }
            }
            handleDeploymentChosen(chosenDeployment);
        }

        private void OnApplicationFocus(bool isFocused)
        {
            if (!isFocused && !Application.runInBackground)
            {
                Debug.LogWarning("The application is being paused, as Application.runInBackground is false and the application is losing focus. This will cause connection to SpatialOS to be interrupted.");
            }
        }

        private void SetupComponents()
        {
            managedBehaviours.Add(gameObject.AddComponent<LogListener>());
            managedBehaviours.Add(gameObject.AddComponent<WorkerMetricsMemoryUsage>());
            managedBehaviours.Add(gameObject.AddComponent<WorkerMetricsFPS>());
            managedBehaviours.Add(gameObject.AddComponent<MetricsReporter>());

            if (SpatialOS.Configuration.UseInstrumentation)
            {
                managedBehaviours.Add(gameObject.AddComponent<MetricsUnityGui>());
                managedBehaviours.Add(gameObject.AddComponent<WorkerTypeDisplay>());
            }
        }

        private void RemoveComponents()
        {
            for (var i = 0; i < managedBehaviours.Count; i++)
            {
                Destroy(managedBehaviours[i]);
            }
        }

        private static void PrintClientInfo()
        {
            if (SpatialOS.Configuration.WorkerPlatform == WorkerPlatform.UnityClient)
            {
                if (Application.isEditor == false)
                {
                    Debug.LogFormat("ClientMetrics OS={0}.\nProcessor: type {1}, cores {2}.\nGraphics: id {3}, vendor {4}, name {5}, size {6}.\nMemory: {7}.",
                        SystemInfo.operatingSystem,
                        SystemInfo.processorType,
                        SystemInfo.processorCount,
                        SystemInfo.graphicsDeviceID,
                        SystemInfo.graphicsDeviceVendorID,
                        SystemInfo.graphicsDeviceName,
                        SystemInfo.graphicsMemorySize,
                        SystemInfo.systemMemorySize);
                }
            }
        }

        private static WorkerConnectionParameters CreateWorkerConnectionParameters(WorkerConfiguration instance)
        {
            return new WorkerConnectionParameters
            {
                ConnectionParameters = CreateConnectionParameters(instance),
                LocatorParameters = CreateLocatorParameters(instance),
                ReceptionistIp = instance.Ip,
                ReceptionistPort = instance.Port,
                WorkerId = instance.WorkerId,
            };
        }

        private static LocatorParameters CreateLocatorParameters(WorkerConfiguration instance)
        {
            var credentials = string.IsNullOrEmpty(instance.SteamToken) ? LocatorCredentialsType.LoginToken : LocatorCredentialsType.Steam;

            var locatorParameters = new LocatorParameters
            {
                ProjectName = instance.AppName,
                CredentialsType = credentials
            };

            switch (locatorParameters.CredentialsType)
            {
                case LocatorCredentialsType.LoginToken:
                    locatorParameters.LoginToken.Token = instance.LoginToken;
                    break;
                case LocatorCredentialsType.Steam:
                    locatorParameters.Steam.DeploymentTag = instance.DeploymentTag;
                    locatorParameters.Steam.Ticket = instance.SteamToken;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return locatorParameters;
        }

        private static ConnectionParameters CreateConnectionParameters(WorkerConfiguration instance)
        {
            var tcpParameters = new TcpNetworkParameters { MultiplexLevel = instance.TcpMultiplexLevel };
            var raknetParameters = new RakNetNetworkParameters { HeartbeatTimeoutMillis = instance.RaknetHeartbeatTimeoutMillis };

            var parameters = new ConnectionParameters
            {
                Network =
                {
                    ConnectionType = instance.LinkProtocol,
                    Tcp = tcpParameters,
                    RakNet = raknetParameters,
                    UseExternalIp = instance.GetCommandLineValue(CommandLineConfigNames.UseExternalIpForBridge, Defaults.UseExternalIp) == false,
                },
                WorkerType = WorkerTypeUtils.ToWorkerName(instance.WorkerPlatform),
                BuiltInMetricsReportPeriodMillis = 2000,
                EnableProtocolLoggingAtStartup = SpatialOS.Configuration.ProtocolLoggingOnStartup,
                ProtocolLogging =
                {
                    LogPrefix = SpatialOS.Configuration.ProtocolLogPrefix,
                    MaxLogFiles = 10,
                    MaxLogFileSizeBytes = SpatialOS.Configuration.ProtocolLogMaxFileBytes
                },
                ReceiveQueueCapacity = instance.ReceiveQueueCapacity,
                SendQueueCapacity = instance.SendQueueCapacity,
            };
            return parameters;
        }

        private void Dispose()
        {
            if (legacyEntityPipeline != null)
            {
                legacyEntityPipeline.Dispose();
            }

            // Empty the Entity Pipeline.
            EntityPipeline.Internal.Dispose();

            if (eventHandler != null)
            {
                eventHandler.Dispose();
            }
            if (View != null)
            {
                View.Dispose();
            }
            if (Connection != null)
            {
                Connection.Dispose();
            }
            if (ComponentCommander != null)
            {
                ComponentCommander.Dispose();
            }
            if (Commander != null)
            {
                Commander.Dispose();
            }
            if (deferredActionDispatcher != null)
            {
                deferredActionDispatcher.Dispose();
            }
            RemoveComponents();
            SpatialOS.Dispose();
            Destroy(this);
        }

        /// <summary>
        /// Processes all events available in the pipeline.
        /// </summary>
        public void ProcessEvents()
        {
            // Process local deferred actions.
            if (deferredActionDispatcher != null)
            {
                deferredActionDispatcher.ProcessEvents();
            }

            // Process remote events.
            if (Connection != null)
            {
                using (var ops = Connection.GetOpList(/* timeoutMillis */ 0))
                {
                    View.Process(ops);
                }
            }
        }
    }
}
