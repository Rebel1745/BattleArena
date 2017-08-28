using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    public int tileX;
    public int tileY;

    // unit stats
    public float health = 100;
    public int attackDamage = 10;
    public int critMultiplier = 2;
    public int weakDivisor = 2;

    public TileMap map;

    public List<Tile> currentPath = null;

    int moveSpeed = 2;

    void Update()
    {
        if(currentPath != null)
        {
            int currTile = 0;

            while(currTile < currentPath.Count - 1)
            {
                Vector3 offset = new Vector3(0, 0, -0.75f);
                Vector3 start = map.TileCoordToWorldCoord(currentPath[currTile].X, currentPath[currTile].Y) + offset;
                Vector3 end = map.TileCoordToWorldCoord(currentPath[currTile + 1].X, currentPath[currTile + 1].Y) + offset;

                Debug.DrawLine(start, end, Color.red);

                currTile++;
            }
        }
    }

    public void MoveNextTile()
    {
        float remainingMovement = moveSpeed;

        while(remainingMovement > 0)
        {
            if (currentPath == null)
            {
                return;
            }

            // Get cost from current tile to next tile
            remainingMovement -= map.CostToEnterTile(currentPath[1].X, currentPath[1].Y);

            // move us to the next tile in the sequence
            tileX = currentPath[1].X;
            tileY = currentPath[1].Y;
            transform.position = map.TileCoordToWorldCoord(tileX, tileY);

            // Remove the old "current" tile
            currentPath.RemoveAt(0);

            if (currentPath.Count == 1)
            {
                // this tile is ultimate destination so clear current path
                currentPath = null;
            }

        }
        
    }

    // TODO: change hit type to ENUM? 
    public int CalculateDamage(GameObject source, string hitType)
    {
        int damage = 0;
        Unit sourceGo = source.GetComponent<Unit>();

        switch (hitType)
        {
            case "crit":
                damage = sourceGo.attackDamage * sourceGo.critMultiplier;
                break;
            case "normal":
                damage = sourceGo.attackDamage;
                break;
            case "weak":
                damage = sourceGo.attackDamage / sourceGo.weakDivisor;
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
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log(health);
        if(health <= 0)
        {
            Debug.Log("Dude died yo");
            Destroy(gameObject,2f);
        }
    }
}
