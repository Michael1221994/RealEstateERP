using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastracture.Base
{
    public abstract class BaseEntity
    {
        public int ID { get; set; }
      
    }

    public interface IPersistedEntity<T>
    {
         T MapToModel();
         T MapToModel(T t);
    }

    
}
