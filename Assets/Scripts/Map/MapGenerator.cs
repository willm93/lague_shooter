using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] Map[] maps;
    [SerializeField] int mapIndex;
    Map currentMap;

    [SerializeField] Vector2Int maxMapSize;
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] GameObject navMeshFloor;
    [SerializeField] GameObject navMeshMaskPrefab;

    [Range(0,1)]
    public float outlinePercent = 0f;
    [Range(0,6)]
    [SerializeField] float tileScale; 
    public float TileScale {get => tileScale;}

    void Awake()
    {
        FindAnyObjectByType<EnemySpawner>().OnNewWave += OnNewWave;
        GenerateMap();
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }
    
    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        currentMap.tileMap = new GameObject[currentMap.mapSize.x, currentMap.mapSize.y];

        System.Random prng = new System.Random(currentMap.seed);
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileScale, 0.5f, currentMap.mapSize.y * tileScale);
        RenderSettings.skybox = currentMap.skyboxMaterial;

        //delete previous map if present
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        //create new map
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = MapCoordToPosition(new MapCoordinate(x, y));
                GameObject newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90), mapHolder);
                newTile.transform.localScale = (1 - outlinePercent) * tileScale * Vector3.one;
                currentMap.tileMap[x, y] = newTile;
            }
        }

        //create tile coordinates for obstacles
        currentMap.allCoordinates = new List<MapCoordinate>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                currentMap.allCoordinates.Add(new MapCoordinate(x, y));
            }
        }
        currentMap.shuffledCoordinates = new Queue<MapCoordinate>(Utility.ShuffleArray(currentMap.allCoordinates.ToArray(), currentMap.seed));

        //create obstacles
        currentMap.obstacleMap = new bool[currentMap.mapSize.x, currentMap.mapSize.y]; 
        int obstacleCount = Mathf.RoundToInt(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        currentMap.allOpenCoordinates = new List<MapCoordinate>(currentMap.allCoordinates);

        for (int i = 0; i < obstacleCount - 1; i++)
        {
            MapCoordinate randomCoord = GetRandomMapCoordinate();
            Vector3 obstaclePosition = MapCoordToPosition(randomCoord);

            currentMap.obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (!randomCoord.Equals(currentMap.MapCenter) && MapIsFullyAccessible(currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float) prng.NextDouble());
                GameObject newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity, mapHolder);
                newObstacle.transform.localScale = (1 - outlinePercent) * new Vector3(1, 0, 1) * tileScale + (Vector3.up * obstacleHeight);

                //change color
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float) currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                currentMap.allOpenCoordinates.Remove(randomCoord);
            } else {
                currentMap.obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }
        currentMap.unclaimedCoordinates = currentMap.allOpenCoordinates;
        currentMap.shuffledOpenCoordinates = new Queue<MapCoordinate>(Utility.ShuffleArray(currentMap.allOpenCoordinates.ToArray(), currentMap.seed));

        //block edges for navmesh
        GameObject leftNavMask = Instantiate(navMeshMaskPrefab, Vector3.left * (maxMapSize.x + currentMap.mapSize.x) / 4f * tileScale, Quaternion.identity, mapHolder);
        leftNavMask.transform.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 0, currentMap.mapSize.y) * tileScale + Vector3.up;

        GameObject rightNavMask = Instantiate(navMeshMaskPrefab, Vector3.right * (maxMapSize.x + currentMap.mapSize.x) / 4f * tileScale, Quaternion.identity, mapHolder);
        rightNavMask.transform.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 0, currentMap.mapSize.y) * tileScale + Vector3.up;

        GameObject topNavMask = Instantiate(navMeshMaskPrefab, Vector3.forward * (maxMapSize.y + currentMap.mapSize.y) / 4f * tileScale, Quaternion.identity, mapHolder);
        topNavMask.transform.localScale = new Vector3(maxMapSize.x, 0, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileScale + Vector3.up;

        GameObject bottomNavMask = Instantiate(navMeshMaskPrefab, Vector3.back * (maxMapSize.y + currentMap.mapSize.y) / 4f * tileScale, Quaternion.identity, mapHolder);
        bottomNavMask.transform.localScale = new Vector3(maxMapSize.x, 0, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileScale + Vector3.up;

        navMeshFloor.transform.localScale = new Vector3(maxMapSize.x/10f, 0, maxMapSize.y/10f) * tileScale + Vector3.up;
        navMeshFloor.GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    private bool MapIsFullyAccessible(int currentObstacleCount)
    {
        bool[,] checkedTiles = new bool[currentMap.mapSize.x, currentMap.mapSize.y];
        Queue<MapCoordinate> queue = new Queue<MapCoordinate>();
        queue.Enqueue(currentMap.MapCenter);
        checkedTiles[currentMap.MapCenter.x, currentMap.MapCenter.y] = true;

        int accesibleTileCount = 1;

        bool coordinateInMap;
        bool coordinateNotObstacle = false;
        bool coordinateUnchecked = false;

        while (queue.Count > 0){
            MapCoordinate coordinate = queue.Dequeue();

            MapCoordinate[] adjacentCoordinates = new MapCoordinate[4];    
            adjacentCoordinates[0] = new MapCoordinate(coordinate.x + 1, coordinate.y);
            adjacentCoordinates[1] = new MapCoordinate(coordinate.x - 1, coordinate.y);
            adjacentCoordinates[2] = new MapCoordinate(coordinate.x, coordinate.y + 1);
            adjacentCoordinates[3] = new MapCoordinate(coordinate.x, coordinate.y - 1);

            foreach(MapCoordinate adjacentCoordinate in adjacentCoordinates)
            {
                coordinateInMap = currentMap.ContainsCoordinate(adjacentCoordinate);
                if (coordinateInMap)
                {
                    coordinateNotObstacle = !currentMap.obstacleMap[adjacentCoordinate.x, adjacentCoordinate.y];
                    coordinateUnchecked = !checkedTiles[adjacentCoordinate.x, adjacentCoordinate.y];
                }
                
                if (coordinateInMap && coordinateUnchecked && coordinateNotObstacle)
                {
                    checkedTiles[adjacentCoordinate.x, adjacentCoordinate.y] = true;
                    queue.Enqueue(adjacentCoordinate);
                    accesibleTileCount++;
                }
            }
        }

        int targetAccessibleTileCount = currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount;
        return targetAccessibleTileCount == accesibleTileCount;
    }

    private MapCoordinate GetRandomMapCoordinate()
    {
        MapCoordinate randomCoord = currentMap.shuffledCoordinates.Dequeue();
        currentMap.shuffledCoordinates.Enqueue(randomCoord);

        return randomCoord;
    }

    public GameObject GetRandomOpenTile(){
        MapCoordinate randomOpenCoord = GetRandomMapCoordinate();
        return currentMap.tileMap[randomOpenCoord.x, randomOpenCoord.y];
    }

    private Vector3 MapCoordToPosition(MapCoordinate coordinate)
    {
        float x = (-currentMap.mapSize.x / 2f + 0.5f + coordinate.x) * tileScale;
        float z = (-currentMap.mapSize.y / 2f + 0.5f + coordinate.y) * tileScale;
        return new Vector3(x, 0, z);
    }

    private GameObject MapCoordToTile(MapCoordinate coordinate)
    {
        if (currentMap.ContainsCoordinate(coordinate))
        {
            return currentMap.tileMap[coordinate.x, coordinate.y];
        } 
        return null;
    }

    public GameObject GetTileNearPosition(Vector3 position)
    {
        //formula for x? given position
        //position.x = (-mapSize.x /2 + 0.5f + x?) * tileScale
        //position.x / tileScale = -mapSize.x / 2 + 0.5f + x?
        int x = Mathf.RoundToInt(position.x / tileScale + (currentMap.mapSize.x - 1) * 0.5f);
        int y = Mathf.RoundToInt(position.z / tileScale + (currentMap.mapSize.y - 1) * 0.5f);
        x = Mathf.Clamp(x, 0, currentMap.tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, currentMap.tileMap.GetLength(1) - 1);

        return currentMap.tileMap[x, y];
    }

    private MapCoordinate GetCoordinateNearPosition(Vector3 position)
    {
        //formula for x? given position
        //position.x = (-mapSize.x /2 + 0.5f + x?) * tileScale
        //position.x / tileScale = -mapSize.x / 2 + 0.5f + x?
        int x = Mathf.RoundToInt(position.x / tileScale + (currentMap.mapSize.x - 1) * 0.5f);
        int y = Mathf.RoundToInt(position.z / tileScale + (currentMap.mapSize.y - 1) * 0.5f);
        x = Mathf.Clamp(x, 0, currentMap.tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, currentMap.tileMap.GetLength(1) - 1);

        return new MapCoordinate(x, y);
    }

    public GameObject ClaimRandomOpenTile(){
        MapCoordinate coordinate = GetRandomMapCoordinate();

        if (currentMap.ClaimCoordinate(coordinate))
        {
            GameObject openTile = MapCoordToTile(coordinate);
            return openTile;
        }

        return null;
    }

    public GameObject ClaimTileNearPosition(Vector3 position)
    {
        //formula for x? given position
        //position.x = (-mapSize.x /2 + 0.5f + x?) * tileScale
        //position.x / tileScale = -mapSize.x / 2 + 0.5f + x?
        int x = Mathf.RoundToInt(position.x / tileScale + (currentMap.mapSize.x - 1) * 0.5f);
        int y = Mathf.RoundToInt(position.z / tileScale + (currentMap.mapSize.y - 1) * 0.5f);
        x = Mathf.Clamp(x, 0, currentMap.tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, currentMap.tileMap.GetLength(1) - 1);

        MapCoordinate coordinate = new MapCoordinate(x, y);

        if (currentMap.ClaimCoordinate(coordinate))
        {
            GameObject openTile = MapCoordToTile(coordinate);
            return openTile;
        }

        return null;
    }

    public GameObject ClaimRandomAdjacentOpenTile(Vector3 position)
    {
        MapCoordinate currentCoordinate = GetCoordinateNearPosition(position);
        int x = currentCoordinate.x;
        int y = currentCoordinate.y;

        MapCoordinate[] adjacentCoordinates = new MapCoordinate[4];
        adjacentCoordinates[0] = new MapCoordinate(currentCoordinate.x + 1, y);
        adjacentCoordinates[1] = new MapCoordinate(x, y + 1);
        adjacentCoordinates[2] = new MapCoordinate(x - 1, y);
        adjacentCoordinates[3] = new MapCoordinate(x, y - 1);
        adjacentCoordinates = Utility.ShuffleArray(adjacentCoordinates);

        foreach (MapCoordinate coordinate in adjacentCoordinates)
        {
            if (currentMap.ClaimCoordinate(coordinate))
            {
                GameObject adjacentOpenTile = MapCoordToTile(coordinate);
                return adjacentOpenTile;
            }
        }

        return null;
    }

    public void UnclaimTile(GameObject tile)
    {
        MapCoordinate currentCoordinate = GetCoordinateNearPosition(tile.transform.position);
        currentMap.UnclaimCoordinate(currentCoordinate);
    }

    [System.Serializable]
    public struct MapCoordinate : IEquatable<MapCoordinate>
    {
        public int x;
        public int y;

        public MapCoordinate(int _x, int _y){
            x = _x;
            y = _y;
        }

        public override string ToString()
        {
            return $"MapCoordinate:({x}, {y})";
        }

        public bool IsValid()
        {
            return x >= 0 && y >= 0;
        }

        public bool Equals(MapCoordinate coordinate)
        {
            return coordinate.x == x && coordinate.y == y;
        }
    }

    [System.Serializable]
    public class Map 
    {
        public Material skyboxMaterial;
        public Vector2Int mapSize;
        public MapCoordinate MapCenter {get => new MapCoordinate(mapSize.x / 2, mapSize.y / 2); }
        public int seed;

        [Range(0,1)]
        public float obstaclePercent; 
        public float maxObstacleHeight;
        public float minObstacleHeight;

        public Color foregroundColor;
        public Color backgroundColor;
        
        public GameObject[,] tileMap {get; set;}
        public bool[,] obstacleMap {get; set;}
        
        public List<MapCoordinate> allCoordinates {get; set;}
        public List<MapCoordinate> allOpenCoordinates {get; set;}
        public List<MapCoordinate> unclaimedCoordinates {get; set;}
        public Queue<MapCoordinate> shuffledCoordinates {get; set;}
        public Queue<MapCoordinate> shuffledOpenCoordinates {get; set;}

        public bool ContainsCoordinate(MapCoordinate coordinate)
        {
            return coordinate.IsValid() && coordinate.x < mapSize.x && coordinate.y < mapSize.y;
        }

        public bool CoordinateUnclaimed(MapCoordinate coordinate)
        {
            return unclaimedCoordinates.Contains(coordinate);
        }

        public bool ClaimCoordinate(MapCoordinate coordinate)
        {
            if (CoordinateUnclaimed(coordinate)){
                unclaimedCoordinates.Remove(coordinate);
                return true;
            }
            
            return false;    
        }

        public bool UnclaimCoordinate(MapCoordinate coordinate)
        {
            if (!CoordinateUnclaimed(coordinate) && ContainsCoordinate(coordinate)){
                unclaimedCoordinates.Add(coordinate);
                return true;
            } 

            if (!ContainsCoordinate(coordinate))
            {
                Debug.LogError($"{coordinate} is not on the current map");
            }

            if (ContainsCoordinate(coordinate))
            {
                Debug.LogError($"{coordinate} is already unclaimed");
            }

            return false;
        }
    }
}


