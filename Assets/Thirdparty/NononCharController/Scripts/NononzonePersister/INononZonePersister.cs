using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zone.nonon
{
    public interface INononZonePersister
    {
        public string Serialize2JSON();
        public void RestoreFromJSON(string serializedJSON);
    }

}
