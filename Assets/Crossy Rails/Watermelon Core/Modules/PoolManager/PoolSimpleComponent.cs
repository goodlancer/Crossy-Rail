using System.Collections.Generic;
using UnityEngine;

//Pool module v 1.5.1

namespace Watermelon
{
    [System.Serializable]
    public class PoolSimpleComponent<T> : PoolSimple where T : Component
    {
        public List<T> pooledComponents = new List<T>();

        public PoolSimpleComponent(string name, GameObject prefab, int poolSize, bool willGrow, Transform container = null) : base(name, prefab, poolSize, willGrow, container)
        {

        }

        protected override void InitGameObject(GameObject prefab)
        {
            pooledComponents.Add(prefab.GetComponent<T>());
        }

        public T GetPooledComponent(bool checkTypeActiveSelf = true)
        {
            int poolIndex = GetPooledObjectIndex(checkTypeActiveSelf);
            if (poolIndex != -1)
                return pooledComponents[poolIndex];

            return null;
        }
    }
}