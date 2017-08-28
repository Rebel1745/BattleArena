using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : MonoBehaviour {

    public GameObject moveList;
    public Transform canvasTransform;

    void Start()
    {
        moveList = GameObject.FindGameObjectWithTag("MoveList");
        moveList.SetActive(false);
    }

	public void ShowMoveList()
    {
        // Remove any previous moveLists that may be active
        /*GameObject[] gos = GameObject.FindGameObjectsWithTag("MoveList");
        foreach (GameObject g in gos)
        {
            Destroy(g);
        }
        GameObject go = (GameObject)Instantiate(moveListPrefab);
        go.transform.SetParent(canvasTransform, false);*/
        moveList.SetActive(true);
    }
}
