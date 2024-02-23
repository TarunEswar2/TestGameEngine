using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GameEngineEditor.Components
{
    //empty interface so we dont have to specify MScompoenet<T> type T in MSEntities observable collection
    interface IMSComponents
    {

    }

    [DataContract]
    abstract class Components : viewModelBase
    {
        [DataMember]
        public gameEntitiy owner { get; private set; }

        public Components(gameEntitiy entity) 
        { 
            Debug.Assert(entity != null);
            owner = entity;
        }
    }

    abstract class MScomponent<T> : viewModelBase , IMSComponents where T : Components
    {

    }
}
