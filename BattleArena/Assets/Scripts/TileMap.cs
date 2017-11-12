using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using QPath;

public class TileMap : MonoBehaviour, IQPathWorld {

    void Start()
    {
        GenerateMap();
    }

    public GameObject selectedUnit;

    public GameObject unitPlayerPrefab;
    public GameObject unitEnemyPrefab;

    public TileType[] tileTypes;

    public int mapSizeX = 20;
    public int mapSizeY = 20;
    
    private Tile[,] tiles;
    private Dictionary<Tile, GameObject> tileToGameObjectMap;
    private Dictionary<GameObject, Tile> gameObjectToTileMap;

    private HashSet<Unit> units;
    private Dictionary<Unit, GameObject> unitToGameObjectMap;
    public Unit[] Units
    {
        get { return units.ToArray(); }
    }

    public bool animationIsPlaying = false;
    public delegate void UnitCreatedDelegate(Unit unit, GameObject unitGO);
    public event UnitCreatedDelegate OnUnitCreated;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(DoAllUnitMoves());
        }
    }

    IEnumerator DoAllUnitMoves()
    {
        if (units != null)
        {
            foreach (Unit u in units)
            {
                yield return DoUnitMoves(u);
            }
        }
    }

    public IEnumerator DoUnitMoves(Unit u)
    {
        while (u.DoMove())
        {
            Debug.Log("DoMove returned true -- will be called again");
            // Check if animation is playing, if so, let it finish
            while (animationIsPlaying)
            {
                yield return null;
            }
        }
    }

    public void EndTurn()
    {
        foreach(Unit u in units)
        {
            u.RefreshMovement();
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

        units.Add(unit);
        unitToGameObjectMap.Add(unit, unitGO);

        if(OnUnitCreated != null)
        {
            OnUnitCreated(unit, unitGO);
        }

    }

    public void GenerateMap()
    {
        // allocate our map tiles
        GenerateMapData();
        // now spawn visual prefabs
        GenerateMapVisuals();
        // Create enemy first so the player is the SelectedUnit
        Unit enemyUnit = new Unit();
        enemyUnit.Name = "Callum";
        SpawnUnitAt(enemyUnit, unitEnemyPrefab, 6, 5);
        Unit playerUnit = new Unit();
        playerUnit.Name = "Darren";
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
                TileType tt = tileTypes[(int)tiles[x, y].TerrainType];

                GameObject tileGO = (GameObject)Instantiate(
                    tt.tileVisualPrefab, 
                    new Vector3(x, y, 0), 
                    Quaternion.identity, 
                    this.transform
                );

                Tile t = tiles[x, y];
                t.TileType = tt;

                tileToGameObjectMap[t] = tileGO;
                gameObjectToTileMap[tileGO] = t;

                tileGO.name = string.Format("{0}: {1}, {2}", tt.name, x, y);
                
                //Removed the text from each tile
                //tileGO.GetComponentInChildren<TextMesh>().text = string.Format("{0},{1}\n{2}", x, y, t.BaseMovementCost());
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
                tiles[x, y].TerrainType = Tile.TERRAIN_TYPE.GRASS;
            }
        }

        // Chuck some water in
        tiles[3, 3].TerrainType = Tile.TERRAIN_TYPE.WATER;
        tiles[4, 3].TerrainType = Tile.TERRAIN_TYPE.WATER;
        tiles[5, 3].TerrainType = Tile.TERRAIN_TYPE.WATER;
        tiles[3, 4].TerrainType = Tile.TERRAIN_TYPE.WATER;
        tiles[3, 5].TerrainType = Tile.TERRAIN_TYPE.WATER;
        tiles[3, 6].TerrainType = Tile.TERRAIN_TYPE.WATER;

        // Test high box
        tiles[6, 6].TerrainType = Tile.TERRAIN_TYPE.BOX;
        tiles[6, 6].ElevationType = Tile.ELEVATION_TYPE.HIGH;
        tiles[6, 6].Elevation = 0.25f;
    }

    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x, y, 0);
    }
}
