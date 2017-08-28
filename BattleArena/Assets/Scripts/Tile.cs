using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public List<Tile> neighbours;
    public int X;
    public int Y;

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
}