using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using QPath;

public class TileMap : MonoBehaviour, IQPathWorld {

    void Start()
    {
        /*if (selectedUnit == null)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
            if (gos != null)
                selectedUnit = gos[0];
            else
                Debug.LogError("NO PLAYER IN THE SCENE");
        }
        // Sort selected unit variables
        selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
        selectedUnit.GetComponent<Unit>().tileY = (int)selectedUnit.transform.position.y;
        selectedUnit.GetComponent<Unit>().map = this;*/
        GenerateMap();
    }

    public GameObject selectedUnit;

    public GameObject unitPlayerPrefab;
    public GameObject unitEnemyPrefab;

    public TileType[] tileTypes;

    public int mapSizeX = 20;
    public int mapSizeY = 20;

    //int[,] tiles;
    private Tile[,] tiles;
    private Dictionary<Tile, GameObject> tileToGameObjectMap;
    private Dictionary<GameObject, Tile> gameObjectToTileMap;

    private HashSet<Unit> units;
    private Dictionary<Unit, GameObject> unitToGameObjectMap;

    // remove this once clickable tile is working again. everything should be in the tiles array above
    Tile[,] graph;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(units != null)
            {
                foreach(Unit u in units)
                {
                    u.DoTurn();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P Pressed");
            if (units != null)
            {
                foreach (Unit u in units)
                {
                    Debug.Log("Pathfinding");
                    u.DUMMY_PATHING_FUNCTION();
                }
            }
        }
    }

    public Tile GetTileAt(int x, int y)
    {
        if (tiles == null)
        {
            Debug.LogError("Tile Array not yet instantiated");
            return null;
        }

        try
        {
            return tiles[x, y];
        }
        catch
        {
            //Debug.LogError("Hex not found @ x: " + x + " y: " + y);
            return null;
        }
    }

    public Tile GetTileFromGameObject(GameObject tileGO)
    {
        if (gameObjectToTileMap.ContainsKey(tileGO))
        {
            return gameObjectToTileMap[tileGO];
        }

        return null;
    }

    public GameObject GetGameObjectFromTile(Tile tile)
    {
        if (tileToGameObjectMap.ContainsKey(tile))
        {
            return tileToGameObjectMap[tile];
        }

        return null;
    }

    public void SpawnUnitAt(Unit unit, GameObject unitPrefab, int x, int y)
    {
        if(units == null)
        {
            units = new HashSet<Unit>();
            unitToGameObjectMap = new Dictionary<Unit, GameObject>();
        }

        Tile myTile = GetTileAt(x, y);
        GameObject myTileGO = tileToGameObjectMap[myTile];
        unit.SetTile(myTile);

        GameObject unitGO = (GameObject)Instantiate(unitPrefab, myTileGO.transform.position, Quaternion.identity, myTileGO.transform);
        unit.OnUnitMoved += unitGO.GetComponent<UnitView>().OnUnitMoved;
        // THIS IS ALL TEMPORARY (I HOPE)
       /* selectedUnit = unitGO;
        selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
        selectedUnit.GetComponent<Unit>().tileY = (int)selectedUnit.transform.position.y;
        selectedUnit.GetComponent<Unit>().map = this;*/

        units.Add(unit);
        unitToGameObjectMap.Add(unit, unitGO);
    }

    public void GenerateMap()
    {
        // allocate our map tiles
        GenerateMapData();
        // now spawn visual prefabs
        GenerateMapVisuals();
        // Create enemy first so the player is the SelectedUnit
        //Unit enemyUnit = new Unit();
        //SpawnUnitAt(enemyUnit, unitEnemyPrefab, 6, 5);
        Unit playerUnit = new Unit();
        SpawnUnitAt(playerUnit, unitPlayerPrefab, 5, 5);
    }

    void GenerateMapVisuals()
    {
        tileToGameObjectMap = new Dictionary<Tile, GameObject>();
        gameObjectToTileMap = new Dictionary<GameObject, Tile>();

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                TileType tt = tileTypes[tiles[x, y].tileType];

                GameObject tileGO = (GameObject)Instantiate(
                    tt.tileVisualPrefab, 
                    new Vector3(x, y, 0), 
                    Quaternion.identity, 
                    this.transform
                );

                Tile t = tiles[x, y];

                tileToGameObjectMap[t] = tileGO;
                gameObjectToTileMap[tileGO] = t;

                tileGO.name = string.Format("{0}: {1}, {2}", tt.name, x, y);
                

                tileGO.GetComponentInChildren<TextMesh>().text = string.Format("{0},{1}\n{2}", x, y, t.BaseMovementCost());
                /*ClickableTile ct = go.GetComponent<ClickableTile>();
                ct.tileX = x;
                ct.tileY = y;
                ct.map = this;*/
            }
        }
    }

    void GenerateMapData()
    {
        tiles = new Tile[mapSizeX, mapSizeY];

        // Initialise our map tiles to be grass
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
                tiles[x, y].tileType = 0;
            }
        }

        // Chuck some water in
        tiles[3, 3].tileType = 1;
        tiles[3, 3].movementCost = -999;
        tiles[4, 3].tileType = 1;
        tiles[4, 3].movementCost = -999;
        tiles[5, 3].tileType = 1;
        tiles[5, 3].movementCost = -999;
        tiles[3, 4].tileType = 1;
        tiles[3, 4].movementCost = -999;
        tiles[3, 5].tileType = 1;
        tiles[3, 5].movementCost = -999;
        tiles[3, 6].tileType = 1;
        tiles[3, 6].movementCost = -999;
    }

    public float CostToEnterTile(int targetX, int targetY)
    {
        TileType tt = tileTypes[tiles[targetX, targetY].tileType];

        if (!UnitCanEnterTile(targetX,targetY))
        {
            return Mathf.Infinity;
        }

        float cost = tt.movementCost;

        return cost;
    }

    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x, y, 0);
    }

    public bool UnitCanEnterTile(int x, int y)
    {
        return tileTypes[tiles[x,y].tileType].isWalkable;
    }
}
