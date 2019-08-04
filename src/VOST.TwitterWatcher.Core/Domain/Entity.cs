using System;
using System.Collections.Generic;

namespace VOST.TwitterWatcher.Repo
{
    /// <summary>
    /// Entity.
    /// </summary>
    public class Entity : IEntity, IEquatable<Entity>
    {
        /// <summary>
        /// Gets or sets the id. This is a UniqueId (Guid) that used to uniquely identify the entity. Ex: 0f8fad5b-d9cb-469f-a165-70867728950e.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        /// <seealso cref="P:NOS.IEntity.Id"/>
        public virtual string Id { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        /// <value>The request date.</value>
        /// <returns>DateTime.</returns>
        /// <seealso cref="P:NOS.IEntity.Created" />
        public DateTime Created
        {
            get { return this._created; }
            set { this._created = value.ToUniversalTime(); }
        }

        /// <summary>
        /// Date/Time of the created.
        /// </summary>
        private DateTime _created;


        /// <summary>
        /// Gets or sets the updated date of this purchase.
        /// </summary>
        /// <value>
        /// The updated.
        /// </value>
        /// <seealso cref="P:NOS.IEntity.Updated"/>
        public DateTime Updated
        {
            get { return this._updated; }
            set { this._updated = value.ToUniversalTime(); }
        }

        /// <summary>
        /// Date/Time of the updated.
        /// </summary>
        private DateTime _updated;

        /// <summary>
        /// Gets or sets the metadata for extra information related to the purchase.
        /// </summary>
        /// <value>
        /// The metadata.
        /// </value>
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Gets or sets the version associated with this entity. This value is auto-incremented per-update basis.
        /// For internal purposes only.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public long Version { get; set; }


        /// <summary>
        /// Initializes a new instance of the Entity class.
        /// </summary>
        public Entity()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Created = DateTime.UtcNow;
            this.Updated = DateTime.UtcNow;
            this.Version = 0;
            this.Metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        public override string ToString()
        {
            return this.Id;
        }

        /// <summary>
        /// Tests if this Entity is considered equal to another.
        /// </summary>
        /// <param name="other">The entity to compare to this object.</param>
        /// <returns>
        /// true if the objects are considered equal, false if they are not.
        /// </returns>
        public bool Equals(Entity other)
        {
            // Check whether the compared object is null. 
            if (object.ReferenceEquals(other, null))
                return false;

            // Check whether the compared object references the same data. 
            if (object.ReferenceEquals(this, other))
                return true;

            return this.Id != null && this.Id.Equals(other.Id);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.GetHashCode()"/>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

    }

    /// <summary>
    /// IEntity interface to represent an entity
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets the entity unique id
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        /// <value>
        /// The created.
        /// </value>
        DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the updated.
        /// </summary>
        /// <value>
        /// The updated.
        /// </value>
        DateTime Updated { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        long Version { get; set; }
    }

}