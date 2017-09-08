using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Update_CurrentFunc = Update_DetectModeStart;

        tileMap = GameObject.FindObjectOfType<TileMap>();

        lineRenderer = transform.GetComponentInChildren<LineRenderer>();
		
	}

    //Generic variables
    TileMap tileMap;
    Tile tileUnderMouse;
    Tile tileLastUnderMouse;
    Vector3 lastMousePosition;

    // Camera dragging variables
    int mouseDragThreshold = 1; // Drag more than x to drag cam
    Vector3 lastMouseGroundPlanePosition;
    Vector3 cameraTargetOffset;

    Unit selectedUnit = null;
    Tile[] tilePath;
    LineRenderer lineRenderer;

    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc;

    public LayerMask LayerIDForTiles;

    // Update is called once per frame
    void Update()
    {

        tileUnderMouse = MouseToTile();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelUpdateFunc();
        }

        Update_CurrentFunc();

        lastMousePosition = Input.mousePosition;
        tileLastUnderMouse = tileUnderMouse;
    }

    void Update_DetectModeStart()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // LMB just went down
            Debug.Log("Mouse Down");
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Mouse Up: Click!");

            // could there be more than one unit on a tile?
            Unit[] us = tileUnderMouse.Units();

            if(us != null && us.Length > 0)
            {
                selectedUnit = us[0];
                Update_CurrentFunc = Update_UnitMovement;
            }

        }
        else if(Input.GetMouseButton(0) && Vector3.Distance( Input.mousePosition , lastMousePosition) > mouseDragThreshold)
        {
            // LMB held down AND the mouse moved.  Drag the camera
            Update_CurrentFunc = Update_CameraDrag;
            lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
            Update_CurrentFunc();
        }
    }

    void CancelUpdateFunc()
    {
        Update_CurrentFunc = Update_DetectModeStart;

        selectedUnit = null;
    }

    Tile MouseToTile()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        int layerMask = LayerIDForTiles.value;

        if(Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, layerMask))
        {
            // something got hit
            GameObject tileGO = hitInfo.rigidbody.gameObject;

            return tileMap.GetTileFromGameObject(tileGO);
        }

        //Debug.Log("Found Nothing");
        return null;
    }

    Vector3 MouseToGroundPlane(Vector3 mousePos)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        // What is the point at which the mouse ray intersects Y=0
        if (mouseRay.direction.z <= 0)
        {
            //Debug.LogError("Why is mouse pointing up?");
            return Vector3.zero;
        }
        float rayLength = (mouseRay.origin.z / mouseRay.direction.z);
        return mouseRay.origin - (mouseRay.direction * rayLength);
    }

    void Update_UnitMovement()
    {
        if (Input.GetMouseButtonUp(1) || selectedUnit == null)
        {
            Debug.Log("Complete unit movement");

            if(selectedUnit != null)
            {
                selectedUnit.SetTilePath(tilePath);
            }

            CancelUpdateFunc();
            return;
        }

        // we have a selected unit
        // is this a different tile than before (or we dont already have a path)
        if(tilePath == null || tileUnderMouse != tileLastUnderMouse)
        {
            tilePath = QPath.QPath.FindPath<Tile>(
                tileMap, 
                selectedUnit, 
                selectedUnit.Tile, 
                tileUnderMouse, 
                Tile.CostEstimate
            );

            // draw the path
            DrawPath(tilePath);
        }
    }

    void DrawPath(Tile[] tilePath)
    {
        if(tilePath == null || tilePath.Length == 0)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;

        Vector3[] ps = new Vector3[tilePath.Length];
        Vector3 offset = new Vector3(0, 0, -0.1f);

        for (int i = 0; i < tilePath.Length; i++)
        {
            GameObject tileGO = tileMap.GetGameObjectFromTile(tilePath[i]);
            ps[i] = tileGO.transform.position + offset;
        }

        lineRenderer.numPositions = ps.Length;
        lineRenderer.SetPositions(ps);
    }

    void Update_CameraDrag () {

        if (Input.GetMouseButtonUp(0))
        {
            // LMB released
            Debug.Log("Cancelling camera drag");
            CancelUpdateFunc();
            return;
        }

        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);

        Vector3 diff = lastMouseGroundPlanePosition - hitPos;
        diff.z = 0;

        Camera.main.transform.Translate(diff, Space.World);

        lastMouseGroundPlanePosition = hitPos = MouseToGroundPlane(Input.mousePosition);

    }
}
