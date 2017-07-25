using Improbable.Unity.Internal;

namespace Improbable.Unity.CodeGeneration
{
    /// <summary>
    /// Base ComponentEditorDataObject class for SpatialOS generated components.
    /// </summary>
    [InternalLogic]
    public abstract class SpatialOsComponentEditorBase<TComponent> : ComponentEditorDataObject<TComponent>
        where TComponent : class, ISpatialOsComponent, ICanAttachEditorDataObject
    {
        /// <summary>
        /// Returns whether or not we have authority on this component.
        /// </summary>
        public bool HasAuthority
        {
            get { return Component.HasAuthority; }
        }

        /// <summary>
        /// Returns whether or not the component has received its first set of values and is listening for updates.
        /// </summary>
        public bool IsComponentReady
        {
            get { return Component.IsComponentReady; }
        }

        /// <summary>
        /// Returns the entity ID of the component.
        /// </summary>
        public EntityId EntityId
        {
            get { return Component.EntityId; }
        }
    }
}