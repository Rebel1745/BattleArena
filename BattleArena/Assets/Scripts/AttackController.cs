using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttackController : MonoBehaviour {

    public Transform parent;

    MouseController mc;

    void Start()
    {
        mc = GameObject.FindObjectOfType<MouseController>();
    }

    public void DoMove(Ability ability)
    {
        EventSystem.current.SetSelectedGameObject(null);
        // before instantiating a new move, remove any that may already be on screen
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Move");
        foreach (GameObject go in gos)
        {
            Destroy(go);
        }
        GameObject moveGO = (GameObject)Instantiate(ability.AbilitySwingMeterPrefab, parent);
        //gameObject.SetActive(false);
    }
}
