using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : MonoBehaviour {

    void OnMouseUp()
    {
        // Set flag in GameManager for tiles to no longer be clickable
        TurnManager.instance.SetTileClickability(false);
        Debug.Log("Clicked Enemy");
        // Find out which unit initiated click (use a game manager to track the current focused object - which units turn it is)
        GameObject currentUnit = TurnManager.instance.GetUnitInPlay();
        TurnManager.instance.SetTargetUnit(transform.parent.gameObject);
        // show the list of attacks this unit can perform
        currentUnit.GetComponent<PlayerUnit>().ShowMoveList();
        // get simple stats (health etc.) of the target unit (this)

    }
}
