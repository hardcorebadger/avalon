// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Improbable.Unity.Util;
using Improbable.Unity.Core;
using Improbable.Worker;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Improbable.Unity.Configuration
{
    /// <summary>
    ///     Provides easy access to SpatialOS-related configuration properties.
    ///     Arguments provided on the command line override any settings provided by code.
    /// </summary>
    public class WorkerConfiguration
    {        
        private const string ObsolescenceWarningFormat = "Command line parameter '{0}' is obsolete please use '{1}' instead.";

        private readonly Dictionary<string, string> commandLineDictionary;

        public string AppName { get; private set; }

        public string AssemblyName { get; private set; }

        public string DeploymentId { get; private set; }

        public string DeploymentTag { get; private set; }

        [Obsolete("Please use WorkerId instead.")]
        public string EngineId { get { return WorkerId; } private set { WorkerId = value; } }

        [Obsolete("Please use WorkerPlatform instead.")]
        public WorkerPlatform EnginePlatform { get { return WorkerPlatform; } private set { WorkerPlatform = value; } }

        public string WorkerId { get; private set; }

        public WorkerPlatform WorkerPlatform { get; private set; }

        /// <summary>
        /// Limits the number of new entities that will be added each frame.
        /// Set this to 0 to create entities as soon as they are received.
        /// </summary>
        public int EntityCreationLimitPerFrame { get; private set; }

        public string InfraServiceUrl { get; private set; }

        public string Ip { get; private set; }

        public NetworkConnectionType LinkProtocol { get; private set; }

        public string LoginToken {get; private set; }

        public ushort Port { get; private set; }

        public bool ProtocolLoggingOnStartup { get; private set; }

        public string ProtocolLogPrefix { get; private set; }

        public uint ProtocolLogMaxFileBytes { get; private set; }

        public uint RaknetHeartbeatTimeoutMillis { get; private set; }

        public uint ReceiveQueueCapacity { get; private set; }

        public uint SendQueueCapacity { get; private set; }

        public bool ShowDebugTraces { get; private set; }

        public string SteamToken { get; private set; }

        public byte TcpMultiplexLevel { get; private set; }

        public bool UseInstrumentation { get; private set; }

        public bool UsePerInstanceAssetCache { get; private set; }

        public bool UsePrefabPooling { get; private set; }

        public bool LogDebugToSpatialOs { get; private set; }

        public bool LogAssertToSpatialOs { get; private set; }

        public bool LogWarningToSpatialOs { get; private set; }

        public bool LogErrorToSpatialOs { get; private set; }

        public bool LogExceptionToSpatialOs { get; private set; }

        /// <summary>
        /// Constructs a new instance of <c>WorkerConfiguration</c>.
        /// </summary>
        /// <param name="data">User-configured data used to configure the worker.</param>
        /// <param name="commandLineArguments">
        /// A list of arguments specified on the command line. 
        /// If null, defaults to <example>global::System.Environment.GetCommandLineArgs()</example>
        /// </param>
        public WorkerConfiguration(WorkerConfigurationData data, IList<string> commandLineArguments = null)
        {
            if (commandLineArguments == null)
            {
                commandLineArguments = global::System.Environment.GetCommandLineArgs();
            }

            Debug.LogFormat("Command line {0}", string.Join(" ", commandLineArguments.ToArray()));

            commandLineDictionary = CommandLineUtil.ParseCommandLineArgs(commandLineArguments);

            var appName = string.Empty;

            if (Application.isEditor)
            {
                // Read AppName from the spatialos.json file, if not already specified.
                // This is only done in Editor mode, as we do not expect spatialos.json to exist outside of dev environment.

                if (!commandLineDictionary.ContainsKey(CommandLineConfigNames.AppName))
                {
                    try
                    {
                        appName = ProjectDescriptor.Load().Name;
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Cannot read app_name from '{0}'. You will not be able to connect to a deployment. Underlying error: {1}", ProjectDescriptor.ProjectDescriptorPath, e);
                    }
                }
            }

            var defaultWorkerPlatform = data.SpatialOsApplication.WorkerPlatform;

#pragma warning disable 618
            var workerPlatformString = GetObsoleteAndCurrentCommandLineValue(EditableConfigNames.WorkerType, EditableConfigNames.EngineType, string.Empty);
#pragma warning restore 618
            if (!string.IsNullOrEmpty(workerPlatformString))
            {
                defaultWorkerPlatform = WorkerTypeUtils.FromWorkerName(workerPlatformString);
            }

            var workerName = WorkerTypeUtils.ToWorkerName(data.SpatialOsApplication.WorkerPlatform);

            AppName = GetCommandLineValue(CommandLineConfigNames.AppName, appName);
            AssemblyName = GetCommandLineValue(EditableConfigNames.AssemblyName, data.SpatialOsApplication.AssemblyName);
            DeploymentId = GetCommandLineValue(EditableConfigNames.DeploymentId, data.SpatialOsApplication.DeploymentId);
            DeploymentTag = GetCommandLineValue(EditableConfigNames.DeploymentTag, data.SpatialOsApplication.DeploymentTag);
#pragma warning disable 618
            WorkerId = GetObsoleteAndCurrentCommandLineValue(EditableConfigNames.WorkerId, EditableConfigNames.EngineId, workerName + Guid.NewGuid());
#pragma warning restore 618
            WorkerPlatform = defaultWorkerPlatform;
            EntityCreationLimitPerFrame = GetCommandLineValue(EditableConfigNames.EntityCreationLimitPerFrame, data.Unity.EntityCreationLimitPerFrame);
            InfraServiceUrl = GetCommandLineValue(EditableConfigNames.InfraServiceUrl, data.Debugging.InfraServiceUrl);
            Ip = GetCommandLineValue(EditableConfigNames.ReceptionistIp, GetDefaultIp(data));
            LinkProtocol = GetCommandLineValue(EditableConfigNames.LinkProtocol, data.Networking.LinkProtocol);
            LoginToken = GetLoginTokenConfig(data);
            Port = GetCommandLineValue(EditableConfigNames.ReceptionistPort, data.Networking.Port);
            ProtocolLogPrefix = GetCommandLineValue(EditableConfigNames.ProtocolLogPrefix, data.Debugging.ProtocolLogPrefix);
            ProtocolLoggingOnStartup = GetCommandLineValue(EditableConfigNames.ProtocolLoggingOnStartup, data.Debugging.ProtocolLoggingOnStartup);
            ProtocolLogMaxFileBytes = GetCommandLineValue(EditableConfigNames.ProtocolLogMaxFileBytes, data.Debugging.ProtocolLogMaxFileBytes);
            RaknetHeartbeatTimeoutMillis = GetCommandLineValue(EditableConfigNames.RaknetHeartbeatTimeoutMillis, data.Networking.RaknetHeartbeatTimeoutMillis);
            ReceiveQueueCapacity = data.Networking.ReceiveQueueCapacity;
            SendQueueCapacity = data.Networking.SendQueueCapacity;
            ShowDebugTraces = GetCommandLineValue(EditableConfigNames.ShowDebugTraces, data.Debugging.ShowDebugTraces);
            SteamToken = GetCommandLineValue(EditableConfigNames.SteamToken, data.Networking.SteamToken);
            TcpMultiplexLevel = GetCommandLineValue(EditableConfigNames.TcpMultiplexLevel, data.Networking.TcpMultiplexLevel);
            UseInstrumentation = GetCommandLineValue(EditableConfigNames.UseInstrumentation, data.Debugging.UseInstrumentation);
            UsePerInstanceAssetCache = GetCommandLineValue(EditableConfigNames.UsePerInstanceAssetCache, data.Unity.UsePerInstanceAssetCache);
            UsePrefabPooling = GetCommandLineValue(EditableConfigNames.UsePrefabPooling, data.Unity.UsePrefabPooling);

            LogDebugToSpatialOs = GetCommandLineValue(EditableConfigNames.LogDebugToSpatialOs, data.Debugging.LogDebugToSpatialOs);
            LogAssertToSpatialOs = GetCommandLineValue(EditableConfigNames.LogAssertToSpatialOs, data.Debugging.LogAssertToSpatialOs);
            LogWarningToSpatialOs = GetCommandLineValue(EditableConfigNames.LogWarningToSpatialOs, data.Debugging.LogWarningToSpatialOs);
            LogErrorToSpatialOs = GetCommandLineValue(EditableConfigNames.LogErrorToSpatialOs, data.Debugging.LogErrorToSpatialOs);
            LogExceptionToSpatialOs = GetCommandLineValue(EditableConfigNames.LogExceptionToSpatialOs, data.Debugging.LogExceptionToSpatialOs);

            if (string.IsNullOrEmpty(AppName))
            {
                throw new ArgumentException(string.Format("The AppName must be set with {0}, or via command-line argument +{1}",
                                                          Path.GetFileName(ProjectDescriptor.ProjectDescriptorPath),
                                                          CommandLineConfigNames.AppName));
            }

            if (Application.isEditor == false)
            {
                PrintWorkerConfigurationSettings();
            }
        }

        private void PrintWorkerConfigurationSettings()
        {
            try
            {
                var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                var sb = new StringBuilder();
                sb.AppendLine("WorkerConfiguration settings");
                foreach (var propertyInfo in properties)
                {
                    var t = propertyInfo.PropertyType;
                    if (IsDictionary(t))
                    {
                        PrintDictionaryValues(t, propertyInfo, sb);
                    }
                    else
                    {
                        sb.AppendFormat("{0} = {1}", propertyInfo.Name, propertyInfo.GetValue(this, null));
                        sb.AppendLine();
                    }
                }

                Debug.Log(sb);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private bool IsDictionary(Type type)
        {
            return type.IsGenericType &&
                   (typeof(Dictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()) ||
                    typeof(IDictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()));
        }

        private void PrintDictionaryValues(Type type, PropertyInfo propertyInfo, StringBuilder sb)
        {
            var dict = (IDictionary) propertyInfo.GetValue(this, null);
            var values = string.Join(", ", dict.Keys.OfType<object>().Select(kv => string.Format("{{{0} : {1}}}", kv, dict[kv])).ToArray());
            sb.AppendFormat("{0} = {{ {1} }}\n", propertyInfo.Name, values);
        }

        /// <summary>
        /// Gets a value specified on the command line, in the form "+key" "value"
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="configKey">The name of the key, without the leading +, e.g. "key"</param>
        /// <param name="defaultValue">The value to return if the key was not specified on the command line.</param>
        /// <returns>The value of the key, or defaultValue if the key was not specified on the command line.</returns>
        public T GetCommandLineValue<T>(string configKey, T defaultValue)
        {
            T configValue;
            if (CommandLineUtil.TryGetConfigValue(commandLineDictionary, configKey, out configValue))
            {
                return configValue;
            }
            return defaultValue;
        }

        /// <inheritdoc cref="CommandLineUtil.GetCommandLineValue{T}"/>
        [Obsolete("Use CommandLineUtil.GetCommandLineValue")]
        public static T GetCommandLineValue<T>(IList<string> arguments, string configKey, T defaultValue)
        {
            return CommandLineUtil.GetCommandLineValue(arguments, configKey, defaultValue);
        }

        /// <summary>
        /// Gets a value specified on the command line, in the form "+key" "value"
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="configKey">The name of the key, without the leading +, e.g. "key"</param>
        /// <returns>The value of the key</returns>
        /// <exception cref="Exception">Thrown when the value of the given configuration is not found.</exception>
        public T GetCommandLineValue<T>(string configKey)
        {
            T configValue;
            if (CommandLineUtil.TryGetConfigValue(commandLineDictionary, configKey, out configValue))
            {
                return configValue;
            }
            throw new Exception(string.Format("Could not find the configuration value for '{0}'.", configKey));
        }

        /// <summary>
        /// Gets a value specified on the command line, falling back to the obsolete config key if not found.
        /// Prints a warning about obsolescence.
        /// </summary>
        /// <param name="currentConfigKey">The name of the key, without the leading +, e.g. "key"</param>
        /// <param name="obsoleteConfigKey">The name of the obsolete key, without the leading +, e.g. "key"</param>
        /// <param name="defaultValue">The value to return if the key was not specified on the command line.</param>
        /// <returns>The value of the key</returns>
        private T GetObsoleteAndCurrentCommandLineValue<T>(string currentConfigKey, string obsoleteConfigKey, T defaultValue)
        {
            var valueFromObsolete = GetCommandLineValue(obsoleteConfigKey, defaultValue);
            if (!Equals(valueFromObsolete, defaultValue))
            {
                Debug.LogWarningFormat(ObsolescenceWarningFormat, obsoleteConfigKey, currentConfigKey);
            }

            return GetCommandLineValue(currentConfigKey, valueFromObsolete);
        }
     
       private string GetLoginTokenConfig(WorkerConfigurationData data)
        {
            return GetCommandLineValue(EditableConfigNames.LoginToken, data.SpatialOsApplication.LoginToken);
        }

        private string GetDefaultIp(WorkerConfigurationData data)
        {
            // If the user specified a value in the Unity UI, use it.
            if (!string.IsNullOrEmpty(data.Networking.Ip))
            {
                return data.Networking.Ip;
            }

            // Otherwise, default to the Locator or the Receptionist, depending on whether a login token has been given.
            if (string.IsNullOrEmpty(GetLoginTokenConfig(data)))
            {
                return Defaults.ReceptionistAddress;
            }
            else
            {
                return Defaults.LocatorAddress;
            }
        }
    }

    
}
