using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveListController : MonoBehaviour {

    public Transform parent;

    void Start()
    {
    }

    public void DoMove(GameObject prefab)
    {
        EventSystem.current.SetSelectedGameObject(null);
        // before instantiating a new move, remove any that may already be on screen
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Move");
        foreach (GameObject go in gos)
        {
            Destroy(go);
        }
        GameObject moveGO = (GameObject)Instantiate(prefab, parent);
        //gameObject.SetActive(false);
    }

    public void Cancel()
    {
        // Set flag in GameManager for tiles to be clickable again
        TurnManager.instance.SetTileClickability(true);
        TurnManager.instance.SetTargetUnit(null);

        // remove moves 
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Move");
        foreach (GameObject go in gos)
        {
            Destroy(go);
        }

        // remove move list
        //Destroy(gameObject);
        if(gameObject.activeInHierarchy)
            gameObject.SetActive(false);
    }
}
