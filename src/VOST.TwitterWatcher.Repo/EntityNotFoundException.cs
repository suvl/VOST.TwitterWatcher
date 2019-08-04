using System;
using System.Runtime.Serialization;

namespace VOST.TwitterWatcher.Repo
{
    [Serializable]
    public class EntityNotFoundException : Exception
    {
        private IEntity entity;

        public EntityNotFoundException()
        {
        }

        public EntityNotFoundException(IEntity entity)
        {
            this.entity = entity;
        }

        public EntityNotFoundException(string message) : base(message)
        {
        }

        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}