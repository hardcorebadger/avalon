// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 
using Improbable.Unity.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity.Internal
{
    /// <summary>
    /// Reports interested component overrides to SpatialOS.
    /// </summary>
    public class EntityComponentInterestOverridesUpdater : IEntityComponentInterestOverridesUpdater, IDisposable
    {
        private readonly MonoBehaviour coroutineHostBehaviour;
        private Coroutine updateCoroutine;
        private readonly IDictionary<EntityId, IEntityObject> entitiesToBeUpdated;
        private readonly IDictionary<EntityId, HashSet<uint>> updatesSentCache;
        private readonly HashSet<IComponentInterestOverridesUpdateReceiver> updateReceiversSet;
        private readonly IList<IComponentInterestOverridesUpdateReceiver> updateReceiversList;

        public EntityComponentInterestOverridesUpdater(MonoBehaviour coroutineHostBehaviour)
        {
            entitiesToBeUpdated = new Dictionary<EntityId, IEntityObject>();
            updatesSentCache = new Dictionary<EntityId, HashSet<uint>>();
            updateReceiversSet = new HashSet<IComponentInterestOverridesUpdateReceiver>();
            updateReceiversList = new List<IComponentInterestOverridesUpdateReceiver>();
            this.coroutineHostBehaviour = coroutineHostBehaviour;
        }

        /// <inheritdoc />
        public void InvalidateEntity(EntityId entityId, IEntityObject entityObject)
        {
            entitiesToBeUpdated[entityId] = entityObject;
            ScheduleInterestedComponentsUpdate();
        }

        /// <inheritdoc />
        public void RemoveEntity(EntityId entityId)
        {
            entitiesToBeUpdated.Remove(entityId);
        }

        /// <inheritdoc />
        public void AddUpdateReceiver(IComponentInterestOverridesUpdateReceiver updateReceiver)
        {
            if (!updateReceiversSet.Contains(updateReceiver))
            {
                updateReceiversSet.Add(updateReceiver);
                updateReceiversList.Add(updateReceiver);
            }
        }

        /// <inheritdoc />
        public void RemoveUpdateReceiver(IComponentInterestOverridesUpdateReceiver updateReceiver)
        {
            if (updateReceiversSet.Contains(updateReceiver))
            {
                updateReceiversSet.Remove(updateReceiver);
                updateReceiversList.Remove(updateReceiver);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (updateCoroutine != null)
            {
                coroutineHostBehaviour.StopCoroutine(updateCoroutine);
                updateCoroutine = null;
            }
            entitiesToBeUpdated.Clear();
            updateReceiversSet.Clear();
            updateReceiversList.Clear();
        }

        /// <summary>
        /// Wait until the end of the current frame and send an InterestedComponent update once per frame per entity.
        /// </summary>
        private void ScheduleInterestedComponentsUpdate()
        {
            if (updateCoroutine == null)
            {
                updateCoroutine = coroutineHostBehaviour.StartCoroutine(UpdateInterestedComponentsCoroutine());
            }
        }

        private IEnumerator UpdateInterestedComponentsCoroutine()
        {
            yield return new WaitForEndOfFrame();
            UpdateInterestedComponents();
            updateCoroutine = null;
        }

        internal void UpdateInterestedComponents()
        {
            if (entitiesToBeUpdated.Count == 0)
            {
                return;
            }

            var entitiesEnumerator = entitiesToBeUpdated.GetEnumerator();
            while (entitiesEnumerator.MoveNext())
            {
                var entityId = entitiesEnumerator.Current.Key;
                var entityObject = entitiesEnumerator.Current.Value;

                var interestedComponents = new HashSet<uint>(entityObject.Components.RegisteredComponents.Keys);
                var interestedComponentsVisualizers = entityObject.Visualizers.RequiredComponents;
                if (interestedComponentsVisualizers.Count > 0)
                {
                    foreach (var visualizerComponent in interestedComponentsVisualizers)
                    {
                        interestedComponents.Add(visualizerComponent);
                    }
                }

                if (EntityNeedsUpdate(entityId, interestedComponents))
                {
                    var cache = updatesSentCache.ContainsKey(entityId) ? updatesSentCache[entityId] : new HashSet<uint>();
                    Dictionary<uint, InterestOverride> interestOverrides = new Dictionary<uint, InterestOverride>();
                    foreach (var interestedComponent in interestedComponents)
                    {
                        if (!cache.Contains(interestedComponent))
                        {
                            interestOverrides.Add(interestedComponent, new InterestOverride { IsInterested = true });
                        }
                    }
                    foreach (var cachedInterestedComponent in cache)
                    {
                        if (!interestedComponents.Contains(cachedInterestedComponent))
                        {
                            interestOverrides.Add(cachedInterestedComponent, new InterestOverride { IsInterested = false });
                        }
                    }

                    for (int i = 0; i < updateReceiversList.Count; i++)
                    {
                        updateReceiversList[i].OnComponentInterestOverridesUpdated(entityId, interestOverrides);
                    }
                    updatesSentCache[entityId] = interestedComponents;
                }
            }

            entitiesToBeUpdated.Clear();
        }

        private bool EntityNeedsUpdate(EntityId entity, HashSet<uint> components)
        {
            return !updatesSentCache.ContainsKey(entity) || !updatesSentCache[entity].SetEquals(components);
        }
    }
}
