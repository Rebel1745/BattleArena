using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour {

    void Start()
    {
        oldPosition = newPosition = this.transform.position;
    }

    Vector3 oldPosition;
    Vector3 newPosition;

    Vector3 currentVelocity;
    float smoothTime = 0.5f;

    public void OnUnitMoved(Tile oldTile, Tile newTile)
    {
        // Animate moving from tile to tile
        this.transform.position = oldTile.Position();
        newPosition = newTile.Position();
        currentVelocity = Vector3.zero;
        GameObject.FindObjectOfType<TileMap>().animationIsPlaying = true;

        // newPositions Z component needs to be altered to match the tile height
        newPosition.z -= newTile.Elevation;
    }

    void Update()
    {
        this.transform.position = Vector3.SmoothDamp(this.transform.position, newPosition, ref currentVelocity, smoothTime);

        if(Vector3.Distance(this.transform.position, newPosition) < 0.1f)
        {
            GameObject.FindObjectOfType<TileMap>().animationIsPlaying = false;
        }
    }
	
}
