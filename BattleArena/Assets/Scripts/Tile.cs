using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using QPath;

public class Tile : IQPathTile
{
    public int X;
    public int Y;
    public int tileType = 0;
    public readonly TileMap TileMap;

    HashSet<Unit> units;

    public int movementCost = 1;

    public float Elevation = 1;

    public Tile(TileMap tileMap, int x, int y)
    {
        this.TileMap = tileMap;
        this.X = x;
        this.Y = y;
    }

    public float DistanceTo(Tile n)
    {
        return Vector2.Distance(
                new Vector2(X, Y),
                new Vector2(n.X, n.Y)
            );
    }

    public static float CostEstimate(IQPathTile aa, IQPathTile bb)
    {
        return Distance((Tile)aa, (Tile)bb);
    }

    public static float Distance(Tile a, Tile b)
    {

        return Vector2.Distance(
                new Vector2(a.X, a.Y),
                new Vector2(b.X, b.Y)
            );
    }

    public Vector3 Position()
    {
        return new Vector3(this.X, this.Y, 0);
    }

    public void AddUnit (Unit unit)
    {
        if(units == null)
        {
            units = new HashSet<Unit>();
        }

        units.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        if(units != null)
        {
            units.Remove(unit);
        }
    }

    public Unit[] Units()
    {
        if (units == null)
            return null;

        return units.ToArray();
    }

    public int BaseMovementCost()
    {
        return movementCost;
    }

    Tile[] neighbours;

    public IQPathTile[] GetNeighbours()
    {
        if (this.neighbours != null)
            return this.neighbours;

        List<Tile> neighbours = new List<Tile>();

        neighbours.Add(TileMap.GetTileAt( X +  1, Y +  0));
        neighbours.Add(TileMap.GetTileAt( X + -1, Y +  0));
        neighbours.Add(TileMap.GetTileAt( X +  0, Y +  1));
        neighbours.Add(TileMap.GetTileAt( X +  0, Y + -1));

        List<Tile> neighbours2 = new List<Tile>();

        foreach (Tile t in neighbours)
        {
            if(t != null)
            {
                neighbours2.Add(t);
            }
        }

        this.neighbours = neighbours2.ToArray();

        return this.neighbours;
    }

    public float AggregateCostToEnter(float costSoFar, IQPathTile sourceTile, IQPathUnit theUnit)
    {
        return ((Unit)theUnit).AggregateTurnsToEnterTile(this, costSoFar);
    }
}