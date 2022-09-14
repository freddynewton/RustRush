using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zone.nonon
{
    public class PrefabDirectory : NononSingleton<PrefabDirectory>
    {
        [Serializable]
        public class PrefabHolder
        {
            [SerializeField]
            public Transform prefab;
        }

        public List<PrefabHolder> prefabs = new List<PrefabHolder>();

    }
}
