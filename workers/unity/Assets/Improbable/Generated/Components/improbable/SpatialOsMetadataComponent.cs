// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 
using Improbable.Entity.Component;
using Improbable.Unity.CodeGeneration;
using Improbable.Unity.Core;
using Improbable.Unity.Internal;
using Improbable.Worker;
using UnityEngine;
using System;

namespace Improbable
{
    public class SpatialOsMetadataComponent: SpatialOsComponentBase
    {
        /// <inheritdoc />
        public override uint ComponentId { get { return 53; } }

        protected string entityTypeValue;
        /// <summary>
        ///     Returns the value of the field 'EntityType'.
        /// </summary>
        public string EntityType { get { return entityTypeValue; } }


        /// <inheritdoc />
        public override bool Init(ISpatialCommunicator communicator, EntityObject entityObject)
        {
            if (!base.Init(communicator, entityObject)) 
            {
                return false;
            }


            return true;
        }

        /// <inheritdoc />
        public override void OnAddComponentPipelineOp(AddComponentPipelineOp op)
        {
            OnAddComponentDispatcherCallback(new AddComponentOp<global::Improbable.Metadata> { EntityId = entityId, Data = new global::Improbable.Metadata.Data(((global::Improbable.Metadata.Impl)op.ComponentObject).Data) });
        }

        /// <inheritdoc />
        public override void OnComponentUpdatePipelineOp(UpdateComponentPipelineOp op)
        {
            OnComponentUpdateDispatcherCallback(new ComponentUpdateOp<global::Improbable.Metadata> { EntityId = entityId, Update = (global::Improbable.Metadata.Update)op.UpdateObject });
        }

        protected void OnAddComponentDispatcherCallback(AddComponentOp<global::Improbable.Metadata> op)
        {
            if (op.EntityId != entityId)
            {
                return;
            }
            var update = op.Data.ToUpdate();
            OnComponentUpdateDispatcherCallback(update.Get());
        }

        /// <summary>
        ///     Send a component update.
        /// </summary>
        public virtual void SendComponentUpdate(global::Improbable.Metadata.Update update)
        {
            if (!hasAuthority)
            {
                Debug.LogError(string.Format("Component {0}: Attempted to send a component update without write authority. The component update was discarded. Please make sure you only send component updates when component.HasAuthority equals true.", this.GetType()));
                return;
            }
            communicator.SendComponentUpdate(entityId, update);
        }

        /// <summary>
        ///     The type of callback to listen for component updates.
        /// </summary>
        public delegate void OnComponentUpdateCallback(global::Improbable.Metadata.Update update);

        protected System.Collections.Generic.List<OnComponentUpdateCallback> onComponentUpdateCallbacks;

        /// <summary>
        ///     Invoked when authority changes for this component.
        /// </summary>
        public event OnComponentUpdateCallback OnComponentUpdate
        {
            add
            {
                if (onComponentUpdateCallbacks == null)
                {
                    onComponentUpdateCallbacks = new System.Collections.Generic.List<OnComponentUpdateCallback>();
                }
                onComponentUpdateCallbacks.Add(value);
            }
            remove
            {
                if (onComponentUpdateCallbacks != null)
                {
                    onComponentUpdateCallbacks.Remove(value);
                }
            }
        }

        protected void OnComponentUpdateDispatcherCallback(
            global::Improbable.Worker.ComponentUpdateOp<global::Improbable.Metadata> op)
        {
            if (op.EntityId != entityId)
            {
                return;
            }
            var update = op.Update.Get();
            OnComponentUpdateDispatcherCallback(update);
#if UNITY_EDITOR
            FinalizeComponentUpdateLog();
#endif
        }

        protected void OnComponentUpdateDispatcherCallback(global::Improbable.Metadata.Update update) {
            if (onComponentUpdateCallbacks != null)
            {
                for (var i = 0; i < onComponentUpdateCallbacks.Count; i++)
                {
                    try
                    {
                        onComponentUpdateCallbacks[i](update);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            if (update.entityType.HasValue) {
                entityTypeValue = update.entityType.Value;
#if UNITY_EDITOR
                LogComponentUpdate("entityType", entityTypeValue);
#endif
                if (onEntityTypeUpdateCallbacks != null)
                {
                    for (var i = 0; i < onEntityTypeUpdateCallbacks.Count; i++)
                    {
                        try
                        {
                            onEntityTypeUpdateCallbacks[i](update.entityType.Value);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
            }

            if (!isComponentReady)
            {
                isComponentReady = true;
                if (onComponentReadyCallbacks != null)
                {
                    for (var i = 0; i < onComponentReadyCallbacks.Count; i++)
                    {
                        try
                        {
                            onComponentReadyCallbacks[i]();
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     The type of callback to listen for updates to field 'EntityType'.
        /// </summary>
        public delegate void OnEntityTypeUpdateCallback(string newEntityType);

        protected System.Collections.Generic.List<OnEntityTypeUpdateCallback> onEntityTypeUpdateCallbacks;

        /// <summary>
        ///     Invoked when the field 'EntityType' is updated.
        /// </summary>
        public event OnEntityTypeUpdateCallback OnEntityTypeUpdate
        {
            add
            {
                if (onEntityTypeUpdateCallbacks == null)
                {
                    onEntityTypeUpdateCallbacks = new System.Collections.Generic.List<OnEntityTypeUpdateCallback>();
                }
                onEntityTypeUpdateCallbacks.Add(value);
            }
            remove
            {
                if (onEntityTypeUpdateCallbacks != null)
                {
                    onEntityTypeUpdateCallbacks.Remove(value);
                }
            }
        }

    }
}