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
    public class SpatialOsCharacterComponent: SpatialOsComponentBase
    {
        /// <inheritdoc />
        public override uint ComponentId { get { return 1002; } }

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

            DispatcherCallbackKeys.Add(
                communicator.RegisterCommandRequest<global::Improbable.Core.Character.Commands.Goto>(OnGotoCommandRequestDispatcherCallback));

            return true;
        }

        /// <inheritdoc />
        public override void OnAddComponentPipelineOp(AddComponentPipelineOp op)
        {
            OnAddComponentDispatcherCallback(new AddComponentOp<global::Improbable.Core.Character> { EntityId = entityId, Data = new global::Improbable.Core.Character.Data(((global::Improbable.Core.Character.Impl)op.ComponentObject).Data) });
        }

        /// <inheritdoc />
        public override void OnComponentUpdatePipelineOp(UpdateComponentPipelineOp op)
        {
            OnComponentUpdateDispatcherCallback(new ComponentUpdateOp<global::Improbable.Core.Character> { EntityId = entityId, Update = (global::Improbable.Core.Character.Update)op.UpdateObject });
        }

        protected void OnAddComponentDispatcherCallback(AddComponentOp<global::Improbable.Core.Character> op)
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
        public virtual void SendComponentUpdate(global::Improbable.Core.Character.Update update)
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
        public delegate void OnComponentUpdateCallback(global::Improbable.Core.Character.Update update);

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
            global::Improbable.Worker.ComponentUpdateOp<global::Improbable.Core.Character> op)
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

        protected void OnComponentUpdateDispatcherCallback(global::Improbable.Core.Character.Update update) {
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

        
        private GotoCommandRequestCallbackWrapper gotoCommandRequestCallbackWrapper;

        /// <summary>
        ///     Invoked when a 'Goto' request is received.
        /// </summary>
        public GotoCommandRequestCallbackWrapper OnGotoCommandRequest
        {
            get
            {
                if (gotoCommandRequestCallbackWrapper == null)
                {
                    gotoCommandRequestCallbackWrapper = new GotoCommandRequestCallbackWrapper();
                }
                return gotoCommandRequestCallbackWrapper;
            }
            set { gotoCommandRequestCallbackWrapper = value; }
        }
        /// <summary>
        ///     The type of callback to pass to listen for incoming 'Goto' command requests and respond asynchronously.
        /// </summary>
        public delegate void OnGotoCommandRequestAsyncCallback(GotoCommandResponseHandle responseHandle);
        /// <summary>
        ///     The type of callback to pass to listen for incoming 'Goto' command requests and respond synchronously.
        /// </summary>
        public delegate global::Improbable.Core.Nothing OnGotoCommandRequestSyncCallback(global::Improbable.Core.GotoRequest request, ICommandCallerInfo commandCallerInfo);
        /// <summary>
        ///     Wraps a synchronous or asynchronous callback to be invoked when a command response is received for the Goto command.
        /// </summary>
        public class GotoCommandRequestCallbackWrapper
        {
            private OnGotoCommandRequestSyncCallback syncCallback;
            private OnGotoCommandRequestAsyncCallback asyncCallback;
            /// <summary>
            ///     Registers a synchronous callback to be invoked immediately upon receiving a command request.
            /// </summary>
            public void RegisterResponse(OnGotoCommandRequestSyncCallback callback)
            {
                if (IsCallbackRegistered())
                {
                    ThrowCallbackAlreadyRegisteredException();
                }
                syncCallback = callback;
            }
            /// <summary>
            ///     Registers an asynchronous callback to be invoked with a response handle upon receiving a command request.
            /// </summary>
            public void RegisterAsyncResponse(OnGotoCommandRequestAsyncCallback callback)
            {
                if (IsCallbackRegistered())
                {
                    ThrowCallbackAlreadyRegisteredException();
                }
                asyncCallback = callback;
            }
            /// <summary>
            ///     Deregisters a previously registered command response.
            /// </summary>
            public void DeregisterResponse()
            {
                if (!IsCallbackRegistered())
                {
                    throw new InvalidOperationException("Attempted to deregister a command response when none is registered for command Goto");
                }
                syncCallback = null;
                asyncCallback = null;
            }
            /// <summary>
            ///     Returns whether or not a callback is currently registered.
            /// </summary>
            public bool IsCallbackRegistered()
            {
                return syncCallback != null || asyncCallback != null;
            }
            private void ThrowCallbackAlreadyRegisteredException()
            {
                throw new InvalidOperationException("Attempted to register a command response when one has already been registered for command Goto.");
            }
            /// <summary>
            ///     Invokes the registered callback. This is an implementation detail; it should not be called from user code.
            /// </summary>
            public void InvokeCallback(GotoCommandResponseHandle responseHandle)
            {
                if (syncCallback != null)
                {
                    responseHandle.Respond(syncCallback(responseHandle.Request, responseHandle.CallerInfo));
                }
                if (asyncCallback != null)
                {
                    asyncCallback(responseHandle);
                }
            }
        }

        /// <summary>
        ///     A response handle for the 'Goto' command.
        /// </summary>
        public class GotoCommandResponseHandle
        {
            private readonly
                global::Improbable.Worker.CommandRequestOp<global::Improbable.Core.Character.Commands.Goto>
                commandRequestOp;
            private readonly CommandCallerInfo commandCallerInfo;
            private readonly ISpatialCommunicator communicator;

            /// <summary>
            ///     Creates a new response handle. This is an implementation detail; it should not be called from user code.
            /// </summary>
            public GotoCommandResponseHandle(
                global::Improbable.Worker.CommandRequestOp<global::Improbable.Core.Character.Commands.Goto>
                    commandRequestOp, ISpatialCommunicator communicator)
            {
                this.commandRequestOp = commandRequestOp;
                this.commandCallerInfo = new CommandCallerInfo(commandRequestOp.CallerWorkerId, commandRequestOp.CallerAttributeSet);
                this.communicator = communicator;
            }

            /// <summary>
            ///     Returns the request object.
            /// </summary>
            public global::Improbable.Core.GotoRequest Request { get { return commandRequestOp.Request.Get().Value; } }

            /// <summary>
            /// Metadata for this command request.
            /// </summary>
            public ICommandCallerInfo CallerInfo
            {
                get { return commandCallerInfo; }
            }

            /// <summary>
            ///     Sends the command response.
            /// </summary>
            public void Respond(global::Improbable.Core.Nothing response)
            {
                var commandResponse = new global::Improbable.Core.Character.Commands.Goto.Response(response);
                communicator.SendCommandResponse(commandRequestOp.RequestId, commandResponse);
            }
        }

        protected void OnGotoCommandRequestDispatcherCallback(
            global::Improbable.Worker.CommandRequestOp<global::Improbable.Core.Character.Commands.Goto> op)
        {
            if (op.EntityId != entityId || gotoCommandRequestCallbackWrapper == null || !gotoCommandRequestCallbackWrapper.IsCallbackRegistered())
            {
                return;
            }
            var responseHandle = new GotoCommandResponseHandle(op, communicator);
            gotoCommandRequestCallbackWrapper.InvokeCallback(responseHandle);

#if UNITY_EDITOR
            LogCommandRequest(DateTime.Now, "Goto", op.Request.Get().Value);
#endif
        }

    }
}