using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;
    Map currentMap;
    public Vector2Int maxMapSize;
    GameObject[,] tileMap;
    bool[,] obstacleMap;

    public GameObject tilePrefab;
    public GameObject obstaclePrefab;
    public GameObject navMeshFloor;
    public GameObject navMeshMaskPrefab;

    [Range(0,1)]
    public float outlinePercent = 0f;
    [Range(0,6)]
    public float tileScale;   

    List<MapCoordinate> allTileCoords;
    Queue<MapCoordinate> shuffledTileCoords;
    Queue<MapCoordinate> shuffledOpenTileCoords;

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
        tileMap = new GameObject[currentMap.mapSize.x, currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileScale, 0.5f, currentMap.mapSize.y * tileScale);
        RenderSettings.skybox = currentMap.skyboxMaterial;

        //delete previous map if present
        string holderName = "Generated Map";
        if (transform.Find(holderName)){
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        //create new map
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for (int x = 0; x < currentMap.mapSize.x; x++){
            for (int y = 0; y < currentMap.mapSize.y; y++){
                Vector3 tilePosition = MapCoordToPosition(new MapCoordinate(x, y));
                GameObject newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90), mapHolder);
                newTile.transform.localScale = (1 - outlinePercent) * tileScale * Vector3.one;
                tileMap[x, y] = newTile;
            }
        }

        //create tile coordinates for obstacles
        allTileCoords = new List<MapCoordinate>();
        for (int x = 0; x < currentMap.mapSize.x; x++){
            for (int y = 0; y < currentMap.mapSize.y; y++){
                allTileCoords.Add(new MapCoordinate(x, y));
            }
        }
        shuffledTileCoords = new Queue<MapCoordinate>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        //create obstacles
        obstacleMap = new bool[currentMap.mapSize.x, currentMap.mapSize.y]; 
        int obstacleCount = Mathf.RoundToInt(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        List<MapCoordinate> allOpenTileCoords = new List<MapCoordinate>(allTileCoords);

        for (int i = 0; i < obstacleCount - 1; i++){
            MapCoordinate randomCoord = GetRandomMapCoordinate();
            Vector3 obstaclePosition = MapCoordToPosition(randomCoord);

            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (!randomCoord.Equals(currentMap.MapCenter) && MapIsFullyAccessible(obstacleMap, currentObstacleCount)){
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float) prng.NextDouble());
                GameObject newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity, mapHolder);
                newObstacle.transform.localScale = (1 - outlinePercent) * new Vector3(1, 0, 1) * tileScale + (Vector3.up * obstacleHeight);

                //change color
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float) currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenTileCoords.Remove(randomCoord);
            } else {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }
        shuffledOpenTileCoords = new Queue<MapCoordinate>(Utility.ShuffleArray(allOpenTileCoords.ToArray(), currentMap.seed));

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

    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] checkedTiles = new bool[currentMap.mapSize.x, currentMap.mapSize.y];
        Queue<MapCoordinate> queue = new Queue<MapCoordinate>();
        queue.Enqueue(currentMap.MapCenter);
        checkedTiles[currentMap.MapCenter.x, currentMap.MapCenter.y] = true;

        int accesibleTileCount = 1;

        bool tileInMap;
        bool tileNotObstacle = false;
        bool tileUnchecked = false;

        while (queue.Count > 0){
            MapCoordinate tile = queue.Dequeue();

            MapCoordinate[] adjacentTiles = new MapCoordinate[4];    
            adjacentTiles[0] = new MapCoordinate(tile.x + 1, tile.y);
            adjacentTiles[1] = new MapCoordinate(tile.x - 1, tile.y);
            adjacentTiles[2] = new MapCoordinate(tile.x, tile.y + 1);
            adjacentTiles[3] = new MapCoordinate(tile.x, tile.y - 1);

            foreach(MapCoordinate adjacentTile in adjacentTiles){
                tileInMap = adjacentTile.x >= 0 && adjacentTile.y >= 0 && adjacentTile.x < currentMap.mapSize.x && adjacentTile.y < currentMap.mapSize.y;
                if (tileInMap){
                    tileNotObstacle = !obstacleMap[adjacentTile.x, adjacentTile.y];
                    tileUnchecked = !checkedTiles[adjacentTile.x, adjacentTile.y];
                }
                
                if (tileInMap && tileUnchecked && tileNotObstacle)
                {
                    checkedTiles[adjacentTile.x, adjacentTile.y] = true;
                    queue.Enqueue(adjacentTile);
                    accesibleTileCount++;
                }
            }
        }

        int targetAccessibleTileCount = currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount;
        return targetAccessibleTileCount == accesibleTileCount;
    }

    public MapCoordinate GetRandomMapCoordinate()
    {
        MapCoordinate randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);

        return randomCoord;
    }

    public MapCoordinate GetRandomOpenMapCoordinate()
    {
        MapCoordinate randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);

        return randomCoord;
    }

    public GameObject GetRandomOpenTile(){
        MapCoordinate randomOpenCoord = GetRandomMapCoordinate();
        return tileMap[randomOpenCoord.x, randomOpenCoord.y];
    }

    public Vector3 MapCoordToPosition(MapCoordinate mapCoord)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + mapCoord.x, 0, -currentMap.mapSize.y / 2f + 0.5f + mapCoord.y) * tileScale;
    }

    public GameObject GetTileNearPosition(Vector3 position)
    {
        //formula for x? given position
        //position.x = (-currentMap.mapSize.x /2 + 0.5f + x?) * tileScale
        //position.x / tileScale = -currentMap.mapSize.x / 2 + 0.5f + x?
        int x = Mathf.RoundToInt(position.x / tileScale + (currentMap.mapSize.x - 1) * 0.5f);
        int y = Mathf.RoundToInt(position.z / tileScale + (currentMap.mapSize.y - 1) * 0.5f);
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);

        return tileMap[x, y];
    }

    public GameObject GetRandomAdjacentTile(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileScale + (currentMap.mapSize.x - 1) * 0.5f);
        int y = Mathf.RoundToInt(position.z / tileScale + (currentMap.mapSize.y - 1) * 0.5f);

        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
    
        MapCoordinate[] adjacentTiles = new MapCoordinate[4];
        adjacentTiles[0] = new MapCoordinate(x + 1, y);
        adjacentTiles[1] = new MapCoordinate(x, y + 1);
        adjacentTiles[2] = new MapCoordinate(x - 1, y);
        adjacentTiles[3] = new MapCoordinate(x, y - 1);
        adjacentTiles = Utility.ShuffleArray(adjacentTiles);

        bool tileIsInBounds;
        bool tileIsNotBlocked = false;
        MapCoordinate openAdjacentTile = new MapCoordinate(-1,-1);
        
        foreach (MapCoordinate tile in adjacentTiles){
            tileIsInBounds = tile.x > 0 && tile.x < tileMap.GetLength(0) && tile.y > 0 && tile.y < tileMap.GetLength(1);
            if (tileIsInBounds){
                tileIsNotBlocked = !obstacleMap[tile.x, tile.y];
            }
            
            if (tileIsInBounds && tileIsNotBlocked){
                openAdjacentTile.x = tile.x;
                openAdjacentTile.y = tile.y;
                break;
            }
        }

        if (openAdjacentTile.x == -1){
            return null;
        } else {
            return tileMap[openAdjacentTile.x, openAdjacentTile.y];
        }
    }

    [System.Serializable]
    public struct MapCoordinate 
    {
        public int x;
        public int y;

        public MapCoordinate(int _x, int _y){
            x = _x;
            y = _y;
        }
    }

    [System.Serializable]
    public class Map 
    {
        public Material skyboxMaterial;
        public Vector2Int mapSize;
        public int seed;
        [Range(0,1)]
        public float obstaclePercent; 
        public float maxObstacleHeight;
        public float minObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;
        public MapCoordinate MapCenter {get { return new MapCoordinate(mapSize.x / 2, mapSize.y / 2); }}
    }
}
