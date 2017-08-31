using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public List<Tile> neighbours;
    public int X;
    public int Y;
    public int tileType = 0;

    public Tile(int x, int y)
    {
        this.X = x;
        this.Y = y;
        neighbours = new List<Tile>();
    }

    public float DistanceTo(Tile n)
    {
        return Vector2.Distance(
                new Vector2(X, Y),
                new Vector2(n.X, n.Y)
            );
    }

    public Vector3 Position()
    {
        return new Vector3(this.X, this.Y, 0);
    }
}