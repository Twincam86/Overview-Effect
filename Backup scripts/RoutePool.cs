using System.Collections.Generic;
using UnityEngine;

public class RoutePool : MonoBehaviour
{
    public GameObject routePrefab; // Drag your route visualization prefab here in the inspector
    public int initialPoolSize = 100;
    private List<GameObject> pooledRoutes;

    void Start()
    {
        pooledRoutes = new List<GameObject>();
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(routePrefab);
            obj.SetActive(false);
            pooledRoutes.Add(obj);
        }
    }

    public GameObject GetPooledRoute()
    {
        foreach (GameObject obj in pooledRoutes)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        // Expand the pool if all routes are in use
        GameObject newObj = Instantiate(routePrefab);
        newObj.SetActive(false);
        pooledRoutes.Add(newObj);
        return newObj;
    }
}
