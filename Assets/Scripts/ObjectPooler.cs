using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    // Universal dictionary pool to recycle any ground tile, obstacle, coin, or prop by prefab name
    private Dictionary<string, List<GameObject>> pools = new Dictionary<string, List<GameObject>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject GetPooledItem(GameObject prefab)
    {
        if (prefab == null) return null;

        string key = prefab.name;

        if (!pools.ContainsKey(key))
        {
            pools[key] = new List<GameObject>();
        }

        List<GameObject> list = pools[key];

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null && !list[i].activeInHierarchy)
            {
                return list[i];
            }
        }

        GameObject newObj = Instantiate(prefab, transform);
        newObj.name = prefab.name;
        newObj.SetActive(false);
        list.Add(newObj);
        return newObj;
    }
}