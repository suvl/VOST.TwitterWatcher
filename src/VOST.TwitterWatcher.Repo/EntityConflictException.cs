using System;
using System.Runtime.Serialization;

namespace VOST.TwitterWatcher.Repo
{
    [Serializable]
    internal class EntityConflictException : Exception
    {
        private IEntity entity;

        public EntityConflictException()
        {
        }

        public EntityConflictException(IEntity entity)
        {
            this.entity = entity;
        }

        public EntityConflictException(IEntity entity, string message) : base(message)
        {
            this.entity = entity;
        }

        public EntityConflictException(string message) : base(message)
        {
        }

        public EntityConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntityConflictException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}