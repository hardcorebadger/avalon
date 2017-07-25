using Improbable.Unity.ComponentFactory;
using Improbable.Unity.Entity;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    /// Sets up the legacy entity pipeline implementation.
    /// </summary>
    public class LegacyEntityPipelineSetup : IDisposable
    {
        private readonly IEntityPipeline entityPipeline;

        private readonly AssetPreloader assetPreloader;
        private readonly CriticalSectionPipelineBlock criticalSectionPipelineBlock;
        private readonly ThrottledEntityDispatcher throttledEntityDispatcher;
        private readonly LegacyEntityCreator legacyEntityCreator;
        private readonly LegacyComponentPipeline legacyComponentPipeline;
        private readonly EntityComponentUpdater entityComponentUpdater;

        public LegacyEntityPipelineSetup(MonoBehaviour hostBehaviour, IEntityPipeline entityPipeline, ISpatialCommunicator spatialCommunicator, IMutableUniverse universe, ILegacyEntityPipelineConfiguration config)
        {
            this.entityPipeline = entityPipeline;

            IPrefabFactory<GameObject> prefabFactory;
            if (config.UsePrefabPooling)
            {
#pragma warning disable 0612
                assetPreloader = new AssetPreloader(hostBehaviour,
                    config.TemplateProvider,
                    config.AssetsToPrecache,
                    config.AssetsToPrePool,
                    config.MaxConcurrentPrecacheConnections);
#pragma warning restore 0612
                assetPreloader.PrecachingCompleted += () =>
                {
                    if (config.OnPrecachingCompleted != null)
                    {
                        config.OnPrecachingCompleted();
                    }
                };

                assetPreloader.PrecachingProgress += progress =>
                {
                    if (config.OnPrecacheProgress != null)
                    {
                        config.OnPrecacheProgress(progress);
                    }
                };

                prefabFactory = assetPreloader.PrefabFactory;
            }
            else
            {
                if (config.AssetsToPrePool != null && config.AssetsToPrePool.Any())
                {
                    Debug.LogError("There are prefabs specified for pre-pooling, but prefab pooling is not enabled.");
                }

                prefabFactory = new UnityPrefabFactory();
            }

            criticalSectionPipelineBlock = new CriticalSectionPipelineBlock();

            throttledEntityDispatcher = new ThrottledEntityDispatcher(universe, config.EntityCreationLimitPerFrame, config.Metrics);

            legacyEntityCreator = new LegacyEntityCreator(
                config.TemplateProvider,
                spatialCommunicator,
                prefabFactory,
                universe,
                config.EntityComponentInterestOverridesUpdater,
                config.InterestedComponentUpdaterProvider,
                config.Metrics);
            
            legacyComponentPipeline = new LegacyComponentPipeline(universe);

            entityComponentUpdater = new EntityComponentUpdater(universe);
        }

        public void Setup()
        {
            entityPipeline
                .AddBlock(criticalSectionPipelineBlock)
                .AddBlock(throttledEntityDispatcher)
                .AddBlock(legacyEntityCreator)
                .AddBlock(legacyComponentPipeline)
                .AddBlock(entityComponentUpdater);
        }

        /// <summary>
        /// Coroutine to prepare assets before establishing connection with SpatialOS.
        /// </summary>
        public IEnumerable PrepareAssets()
        {
            if (assetPreloader == null)
            {
                return null;
            }
            return assetPreloader.PrepareAssets();
        }

        public void Dispose()
        {
            legacyEntityCreator.Dispose();
        }
    }
}