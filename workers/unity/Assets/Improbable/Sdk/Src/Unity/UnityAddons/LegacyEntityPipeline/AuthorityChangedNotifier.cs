using Improbable.Unity.Entity;
using Improbable.Unity.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    /// Notifies the registered <see cref="IAuthorityChangedReceiver"/> objects
    /// about received authority changes.
    /// </summary>
    public class AuthorityChangedNotifier : PassOpsBlock
    {
        private readonly ILocalEntities localEntities;
        private readonly IList<IAuthorityChangedReceiver> changeReceivers = new List<IAuthorityChangedReceiver>();
        
        public AuthorityChangedNotifier(ILocalEntities localEntities)
        {
            this.localEntities = localEntities;
        }

        /// <summary>
        /// Registers an object to receive callbacks on component authority changes.
        /// </summary>
        public void RegisterAuthorityChangedReceiver(IAuthorityChangedReceiver receiver)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }
            changeReceivers.Add(receiver);
        }

        /// <summary>
        /// Removes an object from the collection of authority change callback receivers.
        /// </summary>
        /// <returns>True if the suppled object was registered as a receiver and was removed.</returns>
        public bool TryRemoveAuthorityChangedReceiver(IAuthorityChangedReceiver receiver)
        {
            return changeReceivers.Remove(receiver);
        }

        /// <inheritdoc />
        public override void ChangeAuthority(ChangeAuthorityPipelineOp op)
        {
            var entity = localEntities.Get(op.EntityId);
            if (entity != null)
            {
                var visualizers = entity.Visualizers as EntityVisualizers;
                if (visualizers != null)
                {
                    visualizers.OnAuthorityChanged(op.EntityId, op.ComponentMetaClass, op.HasAuthority, op.ComponentObject);
                    InvokeOnAuthorityChangedCallbacks(op);
                }
            }
            else
            {
                Debug.LogErrorFormat("Entity not found: OnAuthorityChanged({0}, {1}, {2})", op.EntityId, op.ComponentMetaClass, op.HasAuthority);
            }
        }

        private void InvokeOnAuthorityChangedCallbacks(ChangeAuthorityPipelineOp op)
        {
            for (var i = 0; i < changeReceivers.Count; i++)
            {
                changeReceivers[i].AuthorityChanged(op.EntityId, op.ComponentMetaClass, op.HasAuthority, op.ComponentObject);
            }
        }
    }
}