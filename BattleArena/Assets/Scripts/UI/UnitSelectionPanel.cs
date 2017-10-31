using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectionPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
        mouseController = GameObject.FindObjectOfType<MouseController>();
	}

    public Text Title;
    public Text Movement;
    public Text TilePath;

    MouseController mouseController;

	// Update is called once per frame
	void Update () {
		if(mouseController.SelectedUnit != null)
        {
            Title.text = mouseController.SelectedUnit.Name;
            Movement.text = string.Format(
                "{0}/{1}",
                mouseController.SelectedUnit.MovementRemaining, 
                mouseController.SelectedUnit.Movement
            );

            Tile[] tilePath = mouseController.SelectedUnit.GetTilePath();
            TilePath.text = tilePath == null ? "0" : tilePath.Length.ToString();
        }
	}
}
