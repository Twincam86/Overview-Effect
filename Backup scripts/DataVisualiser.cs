using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Localspace;

public class DataVisualizer : MonoBehaviour
{
    public GameObject airportMarkerPrefab;
    public GameObject routePrefab;
    public RoutePool routePool;
    public float cullDistanceThreshold = 2000f;
    public int chunkSize = 10;
    public int segments = 32; // Number of segments for the great circle. Adjust for more smoothness.
    public int maxPoolsDisplayed = 2000; // Define maximum number of active pools

    private int currentActivePools = 0; // Keep track of active pools
    private Transform planetTransform;
    private const float UNIT_SPHERE_RADIUS = 2100f; // Set the radius to 30
    private Dictionary<string, Vector3> airportPositions = new Dictionary<string, Vector3>();
    private List<Route> routes;

    void Start()
    {
        planetTransform = GameObject.Find("Planet").transform;
        LoadData();
        StartCoroutine(VisualizeDataInChunks());
    }

    void LoadData()
    {
        TextAsset airportData = Resources.Load<TextAsset>("Datasets/airports");
        List<Airport> airports = CsvParser.ParseAirports(airportData.text);

        foreach (Airport airport in airports)
        {
            Vector3 position = GetPositionFromLatLon(airport.Lat, airport.Long, UNIT_SPHERE_RADIUS);
            airportPositions[airport.Id] = position;
        }

        TextAsset routeData = Resources.Load<TextAsset>("Datasets/routes");
        routes = CsvParser.ParseRoutes(routeData.text);
    }

    IEnumerator VisualizeDataInChunks()
    {
        Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;

        for (int i = 0; i < routes.Count; i += chunkSize)
        {
            for (int j = i; j < Mathf.Min(i + chunkSize, routes.Count); j++)
            {
                if (currentActivePools >= maxPoolsDisplayed)
                {
                    yield return new WaitUntil(() => currentActivePools < maxPoolsDisplayed);
                }

                Route route = routes[j];
                if (airportPositions.ContainsKey(route.SourceAirportId) && airportPositions.ContainsKey(route.DestinationAirportId))
                {
                    Vector3 start = airportPositions[route.SourceAirportId];
                    Vector3 end = airportPositions[route.DestinationAirportId];

                    float distanceToStart = Vector3.Distance(playerPosition, start);
                    float distanceToEnd = Vector3.Distance(playerPosition, end);

                    if (distanceToStart < cullDistanceThreshold || distanceToEnd < cullDistanceThreshold)
                    {
                        List<Vector3> greatCirclePoints = GetGreatCirclePoints(start, end, segments);
                        GameObject pooledRoute = routePool.GetPooledRoute();
                        LineRenderer lineRenderer = pooledRoute.GetComponent<LineRenderer>();
                        
                        lineRenderer.useWorldSpace = false; // Ensure it inherits the transform of the parent
                        pooledRoute.transform.SetParent(planetTransform, false); // Set parent to Planet

                        lineRenderer.positionCount = greatCirclePoints.Count;
                        for (int k = 0; k < greatCirclePoints.Count; k++)
                        {
                            lineRenderer.SetPosition(k, greatCirclePoints[k]);
                        }
                        pooledRoute.SetActive(true);
                        currentActivePools++; // Increase active pool count
                    }
                }
            }

            yield return null; 
        }
    }

    Vector3 GetPositionFromLatLon(float latitude, float longitude, float radius)
    {
        float lat = Mathf.Deg2Rad * latitude;
        float lon = Mathf.Deg2Rad * longitude;

        float x = radius * Mathf.Cos(lat) * Mathf.Cos(lon);
        float y = radius * Mathf.Sin(lat);
        float z = radius * Mathf.Cos(lat) * Mathf.Sin(lon);

        return new Vector3(x, y, z);
    }

    List<Vector3> GetGreatCirclePoints(Vector3 start, Vector3 end, int segments)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 intermediatePoint = Vector3.Slerp(start.normalized, end.normalized, t) * UNIT_SPHERE_RADIUS;
            points.Add(intermediatePoint);
        }
        return points;
    }
}
