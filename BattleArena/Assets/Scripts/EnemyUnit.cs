using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : MonoBehaviour {

    void OnMouseUp()
    {
        // Set flag in GameManager for tiles to no longer be clickable
        TurnManager.instance.SetTileClickability(false);
        // Find out which unit initiated click
        GameObject currentUnit = TurnManager.instance.GetUnitInPlay();
        currentUnit.GetComponent<Unit>().currentPath = null;
        // set the target unit (this unit that was clicked on)
        TurnManager.instance.SetTargetUnit(transform.parent.gameObject);
        // show the list of attacks this unit can perform
        currentUnit.GetComponent<PlayerUnit>().ShowMoveList();
        // get simple stats (health etc.) of the target unit (this)

    }
}
