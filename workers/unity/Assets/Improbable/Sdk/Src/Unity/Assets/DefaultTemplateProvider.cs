// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 
using System;
using System.Collections.Generic;
using System.IO;
using Improbable.Assets;
using Improbable.Unity.Assets;
using Improbable.Unity.Configuration;
using Improbable.Unity.Core;
using UnityEngine;

namespace Improbable.Unity.Entity
{
    /// <summary>
    ///     The DefaultTemplateProvider switches between three strategies, based on whether it's running in the editor, is
    ///     configured to use local prefabs or has a streaming strategy set.
    /// </summary>
    public class DefaultTemplateProvider : MonoBehaviour, IEntityTemplateProvider
    {
        private Dictionary<string, string> assetBundleNameToUrl;
        private List<Action> pendingPrepareTemplates;

        // These can be overridden on the command line.
        public bool UseLocalPrefabs;
        public string LocalAssetDatabasePath = "../../build/assembly/";
        protected AssetDatabaseStrategy LoadingStrategy;

        // The template provider can't be instantiated during construction as Application.isEditor doesn't work.
        private IEntityTemplateProvider templateProvider;

        private IEntityTemplateProvider TemplateProvider
        {
            get
            {
                if (templateProvider == null)
                {
                    var gameObjectLoader = InitializeAssetLoader();
                    templateProvider = InitializeTemplateProvider(gameObjectLoader);
                }
                return templateProvider;
            }
        }

        protected virtual IAssetLoader<GameObject> InitializeAssetLoader()
        {
            var deployment = SpatialOS.Deployment;
            var appName = deployment.HasValue ? deployment.Value.DeploymentName : SpatialOS.Configuration.AppName;
            var assemblyName = deployment.HasValue ? deployment.Value.AssemblyName : SpatialOS.Configuration.AssemblyName;

            // If an assembly name is set, default to streaming from it. The strategy can still be overridden by the command line.
            AssetDatabaseStrategy defaultStrategy = AssetDatabaseStrategy.Local;
            if (!string.IsNullOrEmpty(appName) && !string.IsNullOrEmpty(assemblyName))
            {
                defaultStrategy = AssetDatabaseStrategy.Streaming;
            }

            UseLocalPrefabs = SpatialOS.Configuration.GetCommandLineValue(CommandLineConfigNames.UseLocalPrefabs, UseLocalPrefabs);
            LoadingStrategy = SpatialOS.Configuration.GetCommandLineValue(CommandLineConfigNames.AssetDatabaseStrategy, defaultStrategy);
            LocalAssetDatabasePath = SpatialOS.Configuration.GetCommandLineValue(CommandLineConfigNames.LocalAssetDatabasePath, LocalAssetDatabasePath);

            IAssetLoader<GameObject> gameObjectLoader;

            if (Application.isEditor && UseLocalPrefabs)
            {
                gameObjectLoader = new PrefabGameObjectLoader();
            }
            else
            {
                switch (LoadingStrategy)
                {
                    case AssetDatabaseStrategy.Local:
                        var path = Path.GetFullPath(LocalAssetDatabasePath);
                        gameObjectLoader = new GameObjectFromAssetBundleLoader(new LocalAssetBundleLoader(path));
                        break;
                    case AssetDatabaseStrategy.Streaming:

                        pendingPrepareTemplates = new List<Action>();

                        var assetBundleDownloader = gameObject.AddComponent<AssetBundleDownloader>();
                        assetBundleDownloader.GetAssetUrl = GetAssetUrl;

                        var exponentialBackoffRetryAssetLoader = gameObject.GetComponent<ExponentialBackoffRetryAssetLoader>()
                                                                 ?? gameObject.AddComponent<ExponentialBackoffRetryAssetLoader>();
                        exponentialBackoffRetryAssetLoader.AssetLoader = assetBundleDownloader;
                        exponentialBackoffRetryAssetLoader.MaxRetries = SpatialOS.Configuration.GetCommandLineValue(
                            CommandLineConfigNames.MaxAssetLoadingRetries, exponentialBackoffRetryAssetLoader.MaxRetries);

                        exponentialBackoffRetryAssetLoader.StartBackoffTimeout = SpatialOS.Configuration.GetCommandLineValue(
                            CommandLineConfigNames.AssetLoadingRetryBackoffMilliseconds, exponentialBackoffRetryAssetLoader.StartBackoffTimeout);

                        gameObjectLoader = new GameObjectFromAssetBundleLoader(exponentialBackoffRetryAssetLoader);
                        StartCoroutine(CloudAssemblyArtifactResolver.ResolveAssetUrls(
                            appName, assemblyName, SpatialOS.Configuration, OnAssetBundleNameToUrlMapResolved, OnAssetResolveFailed));
                        break;
                    default:
                        throw new Exception(string.Format("Unknown loading strategy '{0}'", LoadingStrategy));
                }
            }
            return gameObjectLoader;
        }

        protected virtual IEntityTemplateProvider InitializeTemplateProvider(IAssetLoader<GameObject> gameObjectLoader)
        {
            return new AssetDatabaseTemplateProvider(new CachingAssetDatabase(new PreprocessingGameObjectLoader(gameObjectLoader)));
        }

        public virtual void PrepareTemplate(string prefabName, Action<string> onSuccess, Action<Exception> onError)
        {
            // TemplateProvider is initialized-on-access, so ensure we're all setup before checking pendingPrepareTemplates
            var provider = TemplateProvider;
            if (pendingPrepareTemplates != null)
            {
                pendingPrepareTemplates.Add(() => provider.PrepareTemplate(prefabName, onSuccess, onError));
                return;
            }

            provider.PrepareTemplate(prefabName, onSuccess, onError);
        }

        public virtual GameObject GetEntityTemplate(string prefabName)
        {
            return TemplateProvider.GetEntityTemplate(prefabName);
        }

        private string GetAssetUrl(string prefabName)
        {
            var assetBundleName = Platform.PrefabNameToAssetBundleName(prefabName.ToLowerInvariant());
            string url;
            if (!assetBundleNameToUrl.TryGetValue(assetBundleName, out url))
            {
                throw new KeyNotFoundException(string.Format("Trying to load a non-existent asset bundle named '{0}'", assetBundleName));
            }
            return url;
        }

        private static void OnAssetResolveFailed(Exception err)
        {
            throw err;
        }

        private void OnAssetBundleNameToUrlMapResolved(Dictionary<string, string> map)
        {
            assetBundleNameToUrl = map;
            InvokePendingPrepareTemplates();
        }

        private void InvokePendingPrepareTemplates()
        {
            // Start all pending PrepareTemplate requests now that we've finished resolving
            for (var i = 0; i < pendingPrepareTemplates.Count; i++)
            {
                pendingPrepareTemplates[i]();
            }
            pendingPrepareTemplates = null;
        }
    }
}
