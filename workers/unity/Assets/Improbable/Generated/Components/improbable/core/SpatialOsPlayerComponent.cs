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

namespace Improbable.Core
{
    public class SpatialOsPlayerComponent: SpatialOsComponentBase
    {
        /// <inheritdoc />
        public override uint ComponentId { get { return 1003; } }

        protected int playerIdValue;
        /// <summary>
        ///     Returns the value of the field 'PlayerId'.
        /// </summary>
        public int PlayerId { get { return playerIdValue; } }


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
            OnAddComponentDispatcherCallback(new AddComponentOp<global::Improbable.Core.Player> { EntityId = entityId, Data = new global::Improbable.Core.Player.Data(((global::Improbable.Core.Player.Impl)op.ComponentObject).Data) });
        }

        /// <inheritdoc />
        public override void OnComponentUpdatePipelineOp(UpdateComponentPipelineOp op)
        {
            OnComponentUpdateDispatcherCallback(new ComponentUpdateOp<global::Improbable.Core.Player> { EntityId = entityId, Update = (global::Improbable.Core.Player.Update)op.UpdateObject });
        }

        protected void OnAddComponentDispatcherCallback(AddComponentOp<global::Improbable.Core.Player> op)
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
        public virtual void SendComponentUpdate(global::Improbable.Core.Player.Update update)
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
        public delegate void OnComponentUpdateCallback(global::Improbable.Core.Player.Update update);

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
            global::Improbable.Worker.ComponentUpdateOp<global::Improbable.Core.Player> op)
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

        protected void OnComponentUpdateDispatcherCallback(global::Improbable.Core.Player.Update update) {
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
            if (update.playerId.HasValue) {
                playerIdValue = update.playerId.Value;
#if UNITY_EDITOR
                LogComponentUpdate("playerId", playerIdValue);
#endif
                if (onPlayerIdUpdateCallbacks != null)
                {
                    for (var i = 0; i < onPlayerIdUpdateCallbacks.Count; i++)
                    {
                        try
                        {
                            onPlayerIdUpdateCallbacks[i](update.playerId.Value);
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
        ///     The type of callback to listen for updates to field 'PlayerId'.
        /// </summary>
        public delegate void OnPlayerIdUpdateCallback(int newPlayerId);

        protected System.Collections.Generic.List<OnPlayerIdUpdateCallback> onPlayerIdUpdateCallbacks;

        /// <summary>
        ///     Invoked when the field 'PlayerId' is updated.
        /// </summary>
        public event OnPlayerIdUpdateCallback OnPlayerIdUpdate
        {
            add
            {
                if (onPlayerIdUpdateCallbacks == null)
                {
                    onPlayerIdUpdateCallbacks = new System.Collections.Generic.List<OnPlayerIdUpdateCallback>();
                }
                onPlayerIdUpdateCallbacks.Add(value);
            }
            remove
            {
                if (onPlayerIdUpdateCallbacks != null)
                {
                    onPlayerIdUpdateCallbacks.Remove(value);
                }
            }
        }

    }
}