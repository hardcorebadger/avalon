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
    public class SpatialOsPositionComponent: SpatialOsComponentBase
    {
        /// <inheritdoc />
        public override uint ComponentId { get { return 54; } }

        protected global::Improbable.Coordinates coordsValue;
        /// <summary>
        ///     Returns the value of the field 'Coords'.
        /// </summary>
        public global::Improbable.Coordinates Coords { get { return coordsValue; } }


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
            OnAddComponentDispatcherCallback(new AddComponentOp<global::Improbable.Position> { EntityId = entityId, Data = new global::Improbable.Position.Data(((global::Improbable.Position.Impl)op.ComponentObject).Data) });
        }

        /// <inheritdoc />
        public override void OnComponentUpdatePipelineOp(UpdateComponentPipelineOp op)
        {
            OnComponentUpdateDispatcherCallback(new ComponentUpdateOp<global::Improbable.Position> { EntityId = entityId, Update = (global::Improbable.Position.Update)op.UpdateObject });
        }

        protected void OnAddComponentDispatcherCallback(AddComponentOp<global::Improbable.Position> op)
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
        public virtual void SendComponentUpdate(global::Improbable.Position.Update update)
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
        public delegate void OnComponentUpdateCallback(global::Improbable.Position.Update update);

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
            global::Improbable.Worker.ComponentUpdateOp<global::Improbable.Position> op)
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

        protected void OnComponentUpdateDispatcherCallback(global::Improbable.Position.Update update) {
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
            if (update.coords.HasValue) {
                coordsValue = update.coords.Value;
#if UNITY_EDITOR
                LogComponentUpdate("coords", coordsValue);
#endif
                if (onCoordsUpdateCallbacks != null)
                {
                    for (var i = 0; i < onCoordsUpdateCallbacks.Count; i++)
                    {
                        try
                        {
                            onCoordsUpdateCallbacks[i](update.coords.Value);
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
        ///     The type of callback to listen for updates to field 'Coords'.
        /// </summary>
        public delegate void OnCoordsUpdateCallback(global::Improbable.Coordinates newCoords);

        protected System.Collections.Generic.List<OnCoordsUpdateCallback> onCoordsUpdateCallbacks;

        /// <summary>
        ///     Invoked when the field 'Coords' is updated.
        /// </summary>
        public event OnCoordsUpdateCallback OnCoordsUpdate
        {
            add
            {
                if (onCoordsUpdateCallbacks == null)
                {
                    onCoordsUpdateCallbacks = new System.Collections.Generic.List<OnCoordsUpdateCallback>();
                }
                onCoordsUpdateCallbacks.Add(value);
            }
            remove
            {
                if (onCoordsUpdateCallbacks != null)
                {
                    onCoordsUpdateCallbacks.Remove(value);
                }
            }
        }

    }
}