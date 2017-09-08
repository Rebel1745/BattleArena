using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour {

    #region TurnManagerSingleton

    public static TurnManager instance;

    void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("More than one instance of TurnManager!");
            return;
        }
        instance = this;
    }

    #endregion

    public GameObject unitInPlay;
    public GameObject targetUnit;

    public bool isTileClickable = true;

	// Use this for initialization
	void Start () {
        /*if(unitInPlay == null)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
            if (gos != null)
                SetUnitInPlay(gos[0]);
            else
                Debug.LogError("NO PLAYER IN THE SCENE");
        }*/

        unitInPlay = null;
        targetUnit = null;
    }

    public GameObject GetUnitInPlay()
    {
        return unitInPlay;
    }

    public void SetUnitInPlay(GameObject unit)
    {
        unitInPlay = unit;
    }

    public GameObject GetTargetUnit()
    {
        return targetUnit;
    }

    public void SetTargetUnit(GameObject unit)
    {
        targetUnit = unit;
    }

    public bool GetTileClickability()
    {
        return isTileClickable;
    }

    public void SetTileClickability(bool clickable)
    {
        isTileClickable = clickable;
    }
}
