using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        tileMap = GameObject.FindObjectOfType<TileMap>();
	}

    TileMap tileMap;
	
	// Update is called once per frame
	public void EndTurnButton () {
        Debug.Log("EndTurn");

        Unit[] units = tileMap.Units;

        foreach(Unit u in units)
        {
            u.RefreshMovement();
        }
	}
}
