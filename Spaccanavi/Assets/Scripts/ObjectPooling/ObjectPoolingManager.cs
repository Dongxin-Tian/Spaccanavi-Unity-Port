using Spaccanavi.Utilities;
using Spaccanavi.Utilities.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spaccanavi.ObjectPooling
{
    public sealed class ObjectPoolingManager : MonoBehaviour, ISingleton
    {
        // Singleton reference
        public static ObjectPoolingManager Instance { get; private set; }

        // Pool
        [SerializeField] private PoolEntry[] pools;
        
        private readonly Dictionary<string, ReadOnlyQueue<GameObject>> poolDict = new();

        // Constants
        // NOTE: If need to be changed, / or \\ must be included in the end
        private const string prefabRootDirectoryPath = "Prefabs/";



        private void Awake()
        {
            Instance = this;

            // Preprocess the pools
            foreach (PoolEntry pool in pools)
            {
                if (poolDict.ContainsKey(pool.Tag))
                    throw new InvalidOperationException($"The object pool with tag, \"{pool.Tag}\", already existed.");

                // Set up pool queue
                GameObject prefab = Resources.Load<GameObject>($"{prefabRootDirectoryPath}{pool.PrefabPath}");
                GameObject[] array = new GameObject[pool.Size];
                for (int i = 0; i < pool.Size; i++)
                {
                    GameObject go = Instantiate(prefab, transform);
                    go.name = prefab.name;
                    go.SetActive(false);
                    array[i] = go;
                }
                ReadOnlyQueue<GameObject> queue = new ReadOnlyQueue<GameObject>(array);

                poolDict.Add(pool.Tag, queue);
            }
            pools = null; // Allow GC to collect
        }

        private void OnDestroy()
        {
            Instance = null;
        }



        /* Spawn Methods */

        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
        {
            GameObject go = Spawn(tag);
            go.transform.SetPositionAndRotation(position, rotation);
            return go;
        }

        public GameObject Spawn(string tag, Transform parent)
        {
            GameObject go = Spawn(tag);
            go.transform.SetParent(parent);
            return go;
        }

        public GameObject Spawn(string tag)
        {
            GameObject go = poolDict[tag].Dequeue();

            if (go == null)
                return null;

            go.SetActive(true);

            // Invoke OnSpawn method if IPooledGameObject interface is implemented
            if (go.TryGetComponent(out IPooledGameObject pooled))
                pooled.OnSpawn();

            return go;
        }



        /* Utility Methods */

        /// <summary>
        /// Check if a specific object pool has game objects that are not using currently.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>Whether the object pool has free game objects.</returns>
        public bool HasInactive(string tag)
        {
            GameObject[] gos = poolDict[tag].GetInnerArray();
            foreach (GameObject go in gos)
            {
                if (!go.activeSelf)
                    return true;
            }

            return false;
        }



        [Serializable]
        private sealed record PoolEntry
        {
            [SerializeField] private string tag;
            [SerializeField] private string prefabPath;
            [SerializeField] private int size;

            public string Tag => tag;
            public string PrefabPath => prefabPath;
            public int Size => size;
        }
    }
}
