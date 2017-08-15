using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : MonoBehaviour {

    public GameObject moveListPrefab;
    public Transform canvasTransform;

	public void ShowMoveList()
    {
        // Remove any previous moveLists that may be active
        GameObject[] gos = GameObject.FindGameObjectsWithTag("MoveList");
        foreach (GameObject g in gos)
        {
            Destroy(g);
        }
        GameObject go = (GameObject)Instantiate(moveListPrefab);
        go.transform.SetParent(canvasTransform, false);
    }
}
