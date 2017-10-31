using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapObjectNameplateController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject.FindObjectOfType<TileMap>().OnUnitCreated += CreateUnitNameplate;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public GameObject ObjectNameplatePrefab;

    public void CreateUnitNameplate(Unit unit, GameObject unitGO)
    {
        GameObject nameGO = (GameObject)Instantiate(ObjectNameplatePrefab, this.transform);
        nameGO.GetComponent<MapObjectNameplate>().MyTarget = unitGO;
    }
}
