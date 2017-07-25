// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 

using Improbable.Unity.Core;
using Improbable.Unity.Internal;
using Improbable.Worker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Unity.CodeGeneration
{
    /// <summary>
    ///     The base class for all generated Spatial OS Component MonoBehaviours, which
    ///     implements all shared component logic.
    /// </summary>
    public abstract class SpatialOsComponentBase : MonoBehaviour, ICanAttachEditorDataObject, ISpatialOsComponentInternal, IDisposable
    {
        /// <inheritdoc />
        public abstract uint ComponentId { get; }

        /// <inheritdoc />
        public bool HasAuthority { get { return hasAuthority; } }

        /// <inheritdoc />
        public EntityId EntityId { get { return entityId; } }

        /// <inheritdoc />
        public bool IsComponentReady { get { return isComponentReady; } }

        /// <summary>
        ///     Exposes methods for sending commands from this component.
        /// </summary>
        public IComponentCommander SendCommand
        {
            get { return commander ?? (commander = new ComponentCommander(this, communicator)); }
        }
        
        protected bool hasAuthority;
        protected bool isComponentReady;

        private List<ulong> dispatcherCallbackKeys;

        protected ISpatialCommunicator communicator;
        protected IComponentCommander commander;
        protected EntityId entityId;
        protected EntityObject entityObject;
        protected List<OnAuthorityChangeCallback> onAuthorityChangeCallbacks;
        protected List<OnComponentReadyCallback> onComponentReadyCallbacks;

        #region Editor-only members
#if UNITY_EDITOR
        private IComponentEditorDataObject editorDataObject;
#endif
        #endregion

        protected List<ulong> DispatcherCallbackKeys
        {
            get { return dispatcherCallbackKeys ?? (dispatcherCallbackKeys = new List<ulong>()); }
        }

        /// <summary>
        ///     This is an implementation detail; it should not be called by user code.
        /// </summary>
        public virtual bool Init(ISpatialCommunicator communicator, EntityObject entityObject)
        {
            if (this.communicator != null)
            {
                return false;
            }
            this.communicator = communicator;
            this.entityObject = entityObject;
            this.entityId = entityObject.EntityId;

            entityObject.Components.RegisterInterestedComponent(ComponentId, this);
            return true;
        }

        public virtual void Dispose()
        {
            if (commander != null)
            {
                commander.Dispose();
                commander = null;
            }
            if (dispatcherCallbackKeys != null)
            {
                for (var i = 0; i < dispatcherCallbackKeys.Count; i++)
                {
                    communicator.Remove(dispatcherCallbackKeys[i]);
                }
                dispatcherCallbackKeys.Clear();
            }
            entityObject.Components.DeregisterInterestedComponent(ComponentId);
        }

        /// <inheritdoc />
        public abstract void OnAddComponentPipelineOp(AddComponentPipelineOp op);

        /// <inheritdoc />
        public void OnRemoveComponentPipelineOp(RemoveComponentPipelineOp op)
        {
            OnRemoveComponentDispatcherCallback(new RemoveComponentOp { EntityId = entityId });
        }

        /// <inheritdoc />
        public abstract void OnComponentUpdatePipelineOp(UpdateComponentPipelineOp op);

        /// <inheritdoc />
        public void OnAuthorityChangePipelineOp(ChangeAuthorityPipelineOp op)
        {
            OnAuthorityChangeDispatcherCallback(new AuthorityChangeOp { EntityId = entityId, HasAuthority = op.HasAuthority });
        }

        protected void OnRemoveComponentDispatcherCallback(RemoveComponentOp op)
        {
            if (op.EntityId != entityId)
            {
                return;
            }
            isComponentReady = false;
            Dispose();
        }

        /// <summary>
        ///     Invoked when authority changes for this component.
        /// </summary>
        public event OnAuthorityChangeCallback OnAuthorityChange
        {
            add
            {
                if (onAuthorityChangeCallbacks == null)
                {
                    onAuthorityChangeCallbacks = new List<OnAuthorityChangeCallback>();
                }
                onAuthorityChangeCallbacks.Add(value);
            }
            remove
            {
                if (onAuthorityChangeCallbacks != null)
                {
                    onAuthorityChangeCallbacks.Remove(value);
                }
            }
        }

        [InternalLogic]
        protected void OnAuthorityChangeDispatcherCallback(global::Improbable.Worker.AuthorityChangeOp op)
        {
            if (op.EntityId != entityId)
            {
                return;
            }

            hasAuthority = op.HasAuthority;

            if (onAuthorityChangeCallbacks == null)
            {
                return;
            }
            for (var i = 0; i < onAuthorityChangeCallbacks.Count; i++)
            {
                try
                {
                    onAuthorityChangeCallbacks[i](op.HasAuthority);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        ///     Invoked when this component is ready and listening for updates.
        /// </summary>
        public event OnComponentReadyCallback OnComponentReady
        {
            add
            {
                if (isComponentReady)
                {
                    value();
                    return;
                }
                if (onComponentReadyCallbacks == null)
                {
                    onComponentReadyCallbacks = new List<OnComponentReadyCallback>();
                }
                onComponentReadyCallbacks.Add(value);
            }
            remove
            {
                if (onComponentReadyCallbacks != null)
                {
                    onComponentReadyCallbacks.Remove(value);
                }
            }
        }

        #region Editor methods.

        /// <inheritdoc />
        [InternalLogic]
        public void AttachEditorDataObject(IComponentEditorDataObject editorDataObject)
        {
#if UNITY_EDITOR
            this.editorDataObject = editorDataObject;
#endif
        }

        /// <inheritdoc />
        [InternalLogic]
        public void RemoveEditorDataObject()
        {
#if UNITY_EDITOR
            this.editorDataObject = null;
#endif
        }

        #region Editor-only methods
#if UNITY_EDITOR
        protected void LogComponentUpdate(string componentName, object componentValue)
        {

            if (editorDataObject == null)
            {
                return;
            }

            editorDataObject.LogComponentUpdate(componentName, componentValue);
        }

        protected void LogCommandRequest(DateTime dateTime, string commandName, object payload)
        {

            if (editorDataObject == null)
            {
                return;
            }
            editorDataObject.LogCommandRequest(dateTime, commandName, payload);
        }

        protected void FinalizeComponentUpdateLog()
        {

            if (editorDataObject == null)
            {
                return;
            }

            editorDataObject.SendUpdateLog();
        }
#endif
        #endregion
        #endregion
    }
}