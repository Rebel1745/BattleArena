using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QPath;

public class Unit : IQPathUnit {

    public int tileX;
    public int tileY;

    // unit stats
    public string Name = "Unnamed Fighter";
    public float Health = 100;
    public int AttackDamage = 10;
    public int CritMultiplier = 2;
    public int WeakDivisor = 2;
    public int Movement = 2;
    public int MovementRemaining = 2;

    public TileMap map;
    public Tile Tile { get; protected set; }

    public List<Tile> currentPath = null;
    List<Tile> tilePath;
    
    // create an event listener to run when a unit moves
    public delegate void UnitMovedDelegate(Tile oldTile, Tile newTile);
    public event UnitMovedDelegate OnUnitMoved;

    public void ClearTilePath()
    {
        this.tilePath = new List<Tile>();
    }

    public void SetTilePath(Tile[] tileArray)
    {
        this.tilePath = new List<Tile>(tileArray);     
    }

    public Tile[] GetTilePath()
    {
        return (this.tilePath == null) ? null : this.tilePath.ToArray();
    }

    public void SetTile(Tile newTile)
    {
        Tile oldTile = Tile;

        if(newTile != null)
        {
            newTile.RemoveUnit(this);
        }
        Tile = newTile;

        newTile.AddUnit(this);

        if(OnUnitMoved != null)
        {
            OnUnitMoved(oldTile, newTile);
        }
    }

    public void DUMMY_PATHING_FUNCTION()
    {
        Tile[] pathTiles = QPath.QPath.FindPath<Tile>(
            Tile.TileMap, 
            this, 
            Tile, 
            Tile.TileMap.GetTileAt(Tile.X - 3, Tile.Y), 
            Tile.CostEstimate
        );

        Debug.Log("Got pathfinding length of " + pathTiles.Length);

        SetTilePath(pathTiles);
    }

    // TODO: change hit type to ENUM? 
    public int CalculateDamage(GameObject source, string hitType)
    {
        int damage = 0;
        Unit sourceGo = source.GetComponent<Unit>();

        switch (hitType)
        {
            case "crit":
                damage = sourceGo.AttackDamage * sourceGo.CritMultiplier;
                break;
            case "normal":
                damage = sourceGo.AttackDamage;
                break;
            case "weak":
                damage = sourceGo.AttackDamage / sourceGo.WeakDivisor;
                break;
            case "miss":
                damage = 0;
                break;
            default:
                damage = 0;
                Debug.LogError("Invalid hit type " + hitType);
                break;
        }

        return damage;
    }

    // Function to alter the health of a unit
    /*public void TakeDamage(int damage)
    {
        Health -= damage;
        Debug.Log(Health);
        if(Health <= 0)
        {
            Debug.Log("Dude died yo");
            Destroy(gameObject,2f);
        }
    }*/

    public bool UnitWaitingForOrders()
    {
        if(MovementRemaining > 0 && (tilePath == null || tilePath.Count == 0))
        {
            // add other clauses to the if statement if more things can be added to player e.g. defend
            return true;
        }

        return false;
    }

    // processes one tile worth of movement for the unit
    public bool DoMove()
    {
        Debug.Log("Doing Turn");

        if (MovementRemaining <= 0)
            return false;

        if(tilePath == null || tilePath.Count == 0)
        {
            return false;
        }

        Tile tileWeAreLeaving = tilePath[0];
        Tile newTile = tilePath[1];

        int costToEnter = MovementCostToEnterTile(newTile);

        if(costToEnter > MovementRemaining && MovementRemaining < Movement)
        {
            // we cant enter the tile this turn
            return false;
        }

        tilePath.RemoveAt(0);

        if (tilePath.Count == 1)
        {
            // last tile in the path, clear the queue
            tilePath = null;
        }

        SetTile(newTile);
        MovementRemaining = Mathf.Max(MovementRemaining - costToEnter, 0);

        return tilePath != null && MovementRemaining > 0;
    }

    public int MovementCostToEnterTile(Tile tile)
    {
        // this may override the base movement cost depending on unit type and elevation_type
        return tile.BaseMovementCost();
    }

    public float AggregateTurnsToEnterTile(Tile tile, float turnsToDate)
    {
        // This should be used if there are going to be different costs to tile types
        //  E.g. if there is a swamp tile it may cost 2 to go through one tile

        float baseTurnsToEnterTile = MovementCostToEnterTile(tile) / Movement;

        if(baseTurnsToEnterTile < 0)
        {
            // Impassible terrain
            return -999999;
        }

        if(baseTurnsToEnterTile > 1)
        {
            baseTurnsToEnterTile = 1;
        }

        float turnsRemaining = MovementRemaining / Movement;

        float turnsToDateWhole = Mathf.Floor(turnsToDate);
        float turnsToDateFraction = turnsToDate - turnsToDateWhole;

        if((turnsToDateFraction > 0 && turnsToDateFraction < 0.01f) || turnsToDateFraction > 0.99f)
        {
            Debug.Log("Have we got some rounding issues? " + turnsToDate);

            if (turnsToDateFraction < 0.01f)
                turnsToDateFraction = 0;

            if(turnsToDateFraction > 0.99f)
            {
                turnsToDateWhole += 1;
                turnsToDateFraction = 0;
            }
        }

        float turnsUsedAfterThisMove = turnsToDateFraction + baseTurnsToEnterTile;

        if(turnsUsedAfterThisMove > 1)
        {
            // Not enough movement left!
            turnsUsedAfterThisMove = 1;
        }

        return turnsToDateWhole + turnsUsedAfterThisMove;
    }

    public float CostToEnterTile(IQPathTile sourceTile, IQPathTile destinationTile)
    {
        return 1;
    }

    public void RefreshMovement()
    {
        MovementRemaining = Movement;
    }
}
