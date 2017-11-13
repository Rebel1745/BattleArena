using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileType {

    public string name;
    public GameObject[] tileVisualPrefabs;
    public Material BaseMaterial;
    public Material SelectedMaterial;
    public float VerticalOffset = 0f;
}
