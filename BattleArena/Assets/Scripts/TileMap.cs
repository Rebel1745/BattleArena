﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TileMap : MonoBehaviour {

    public GameObject selectedUnit;

    public GameObject unitPlayerPrefab;
    public GameObject unitEnemyPrefab;

    public TileType[] tileTypes;

    int[,] tiles;
    Tile[,] graph;

    int mapSizeX = 20;
    int mapSizeY = 20;

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

    public void SpawnUnitAt(GameObject unitPrefab, int x, int y)
    {
        GameObject unitGO = (GameObject)Instantiate(unitPrefab, new Vector3(x, y, 0), Quaternion.identity);
        // THIS IS ALL TEMPORARY (I HOPE)
        selectedUnit = unitGO;
        selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
        selectedUnit.GetComponent<Unit>().tileY = (int)selectedUnit.transform.position.y;
        selectedUnit.GetComponent<Unit>().map = this;
    }

    public void GenerateMap()
    {
        // allocate our map tiles
        GenerateMapData();
        GeneratePathfindingGraph();
        // now spawn visual prefabs
        GenerateMapVisuals();
        SpawnUnitAt(unitEnemyPrefab, 6, 5);
        SpawnUnitAt(unitPlayerPrefab, 5, 5);
    }

    void GenerateMapVisuals()
    {
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                TileType tt = tileTypes[tiles[x, y]];
                GameObject go = (GameObject)Instantiate(tt.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);
                go.name = tt.name + " " + x + ", " + y;
                ClickableTile ct = go.GetComponent<ClickableTile>();
                ct.tileX = x;
                ct.tileY = y;
                ct.map = this;
            }
        }
    }

    void GenerateMapData()
    {
        tiles = new int[mapSizeX, mapSizeY];

        // Initialise our map tiles to be grass
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 0;
            }
        }

        // Chuck some water in
        tiles[3, 3] = 1;
        tiles[4, 3] = 1;
        tiles[5, 3] = 1;
        tiles[3, 4] = 1;
        tiles[3, 5] = 1;
        tiles[3, 6] = 1;
    }

    public float CostToEnterTile(int targetX, int targetY)
    {
        TileType tt = tileTypes[tiles[targetX, targetY]];

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
        return tileTypes[tiles[x,y]].isWalkable;
    }

    public void GeneratePathTo(int x, int y)
    {
        // clear any current path
        selectedUnit.GetComponent<Unit>().currentPath = null;
        // code from pseudo https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm

        if (!UnitCanEnterTile(x, y))
        {
            // cant go through water so just bail
            return;
        }

        Dictionary <Tile, float> dist = new Dictionary<Tile, float>();
        Dictionary<Tile, Tile> prev = new Dictionary<Tile, Tile>();

        List<Tile> unvisited = new List<Tile>();

        // find the initial node
        Tile source = graph[selectedUnit.GetComponent<Unit>().tileX, selectedUnit.GetComponent<Unit>().tileY];
        Tile target = graph[x, y];
        // source is 0 distance from itself
        dist[source] = 0;
        // there is nothing before the source
        prev[source] = null;

        // initialise everything to have INFINITY distance (until we know the truth)
        foreach(Tile v in graph)
        {
            if(v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }

            unvisited.Add(v);
        }

        while(unvisited.Count > 0)
        {
            Tile u = null;

            foreach(Tile possibleU in unvisited)
            {
                if(u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            if(u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach(Tile v in u.neighbours)
            {
                float alt = dist[u] + CostToEnterTile(v.X, v.Y);
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        // if we get here we found the shortest route or there is no possible route to target
        if(prev[target] == null)
        {
            // no route from target to source
            return;
        }

        List<Tile> currentPath = new List<Tile>();

        Tile curr = target;

        while(curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        // currentPath is a route from target to source, reverse for correct path
        currentPath.Reverse();

        selectedUnit.GetComponent<Unit>().currentPath = currentPath;
    }

    void GeneratePathfindingGraph()
    {
        graph = new Tile[mapSizeX, mapSizeY];

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                graph[x, y] = new Tile(x, y);
            }
        }

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                // add the tiles (nodes) to the top, bottom, left, and right
                if (x > 0)
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                if (x < mapSizeX - 1)
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                if (y > 0)
                    graph[x, y].neighbours.Add(graph[x, y - 1]);
                if (y < mapSizeY - 1)
                    graph[x, y].neighbours.Add(graph[x, y + 1]);

            }
        }
    }
}
