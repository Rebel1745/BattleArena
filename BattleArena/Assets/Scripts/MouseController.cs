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

    public GameObject UnitSelectionPanel;
    public GameObject UnitActionPanel;
    public GameObject UnitAttackPanel;
    public GameObject UnitItemPanel;

    public enum SELECTED_UNIT_STATE { WAITING, MOVE, ATTACK, ITEM };

    public SELECTED_UNIT_STATE SelectedUnitState = SELECTED_UNIT_STATE.WAITING;

    //Generic variables
    TileMap tileMap;
    Tile tileUnderMouse;
    Tile tileLastUnderMouse;
    Vector3 lastMousePosition;

    // Camera dragging variables
    int mouseDragThreshold = 1; // Drag more than x to drag cam
    Vector3 lastMouseGroundPlanePosition;
    Vector3 cameraTargetOffset;

    Unit __selectedUnit = null;
    public Unit SelectedUnit
    {
        get { return __selectedUnit; }
        set
        {
            Debug.Log("Selected Unit Changed");
            __selectedUnit = value;
            UnitSelectionPanel.SetActive(__selectedUnit != null);
        }
    }

    public Unit TargetUnit;

    Tile[] tilePath;
    LineRenderer lineRenderer;

    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc;

    public LayerMask LayerIDForTiles;

    // Update is called once per frame
    void Update()
    {

        tileUnderMouse = MouseToTile();

        HighlightTile();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SelectedUnit = null;
            TargetUnit = null;
            SelectedUnitState = SELECTED_UNIT_STATE.WAITING;
            CancelUpdateFunc();
        }

        Update_CurrentFunc();

        Update_ScrollZoom();

        lastMousePosition = Input.mousePosition;
        tileLastUnderMouse = tileUnderMouse;

        if(SelectedUnit != null)
        {
            // draw the path
            DrawPath((tilePath != null) ? tilePath : SelectedUnit.GetTilePath());
            // show/hide correct unit selection panels
            switch (SelectedUnitState) {
                case SELECTED_UNIT_STATE.MOVE:
                    UnitActionPanel.SetActive(true);
                    UnitAttackPanel.SetActive(false);
                    UnitItemPanel.SetActive(false);
                break;
                case SELECTED_UNIT_STATE.ATTACK:
                    UnitActionPanel.SetActive(false);
                    UnitAttackPanel.SetActive(true);
                    UnitItemPanel.SetActive(false);
                    break;
                case SELECTED_UNIT_STATE.ITEM:
                    UnitActionPanel.SetActive(false);
                    UnitAttackPanel.SetActive(false);
                    UnitItemPanel.SetActive(true);
                    break;
                case SELECTED_UNIT_STATE.WAITING:
                    UnitActionPanel.SetActive(true);
                    UnitAttackPanel.SetActive(false);
                    UnitItemPanel.SetActive(false);
                    break;
            }
        }
        else
        {
            DrawPath(null);
        }
    }

    void HighlightTile()
    {
        if((tileUnderMouse != null || tileLastUnderMouse != null) && tileUnderMouse != tileLastUnderMouse)
        {
            GameObject currentTile, lastTile;
            Renderer currentTileRenderer, lastTileRenderer;

            if(tileUnderMouse != null)
            {
                currentTile = tileMap.GetGameObjectFromTile(tileUnderMouse);
                currentTileRenderer = currentTile.GetComponentInChildren<Renderer>();
                currentTileRenderer.material = tileUnderMouse.TileType.SelectedMaterial;
            }           

            if(tileLastUnderMouse != null)
            {
                lastTile = tileMap.GetGameObjectFromTile(tileLastUnderMouse);
                lastTileRenderer = lastTile.GetComponentInChildren<Renderer>();
                lastTileRenderer.material = tileLastUnderMouse.TileType.BaseMaterial;
            }
        }
    }

    public void MoveButton()
    {
        SelectedUnitState = SELECTED_UNIT_STATE.MOVE;
    }

    public void AttackButton()
    {
        // replace with function call that deals with attacks
        Update_CurrentFunc = Update_UnitAttack;
    }

    void Update_UnitAttack()
    {
        // Click on enemy unit to set as TargetUnit
        if (tileUnderMouse != null && Input.GetMouseButtonUp(0))
        {
            Debug.Log("Mouse Up: Attack Click!");

            // could there be more than one unit on a tile?
            Unit[] us = tileUnderMouse.Units();

            if (us != null && us.Length > 0)
            {
                if (SelectedUnit != us[0])
                    TargetUnit = us[0];
            }
            else
            {
                return;
            }

            Debug.Log(SelectedUnit.Name);
            Debug.Log(TargetUnit.Name);

            // TargetUnit has been set, display attacks menu
            SelectedUnitState = SELECTED_UNIT_STATE.ATTACK;
        }
    }

    public void ItemButton()
    {
        SelectedUnitState = SELECTED_UNIT_STATE.ITEM;
        // replace with function call that deals with items
        CancelUpdateFunc();
    }

    public void CancelButton()
    {
        SelectedUnitState = SELECTED_UNIT_STATE.WAITING;
        CancelUpdateFunc();
    }

    void Update_DetectModeStart()
    {
        // If we are over a UI element ignore mouse clicks
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // LMB just went down
            //Debug.Log("Mouse Down");
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("Mouse Up: Click!");

            // could there be more than one unit on a tile?
            Unit[] us = tileUnderMouse.Units();

            if(us != null && us.Length > 0)
            {
                if(SelectedUnit != us[0])
                    SelectedUnitState = SELECTED_UNIT_STATE.WAITING;
                SelectedUnit = us[0];
            }

        }
        else if (SelectedUnit != null && SelectedUnitState == SELECTED_UNIT_STATE.MOVE)
        {
            // selected a unit and right mouse button is down
            Update_CurrentFunc = Update_UnitMovement;
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

        tilePath = null;
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
        if (Input.GetMouseButtonUp(0) || SelectedUnit == null)
        {
            Debug.Log("Complete unit movement");

            if(SelectedUnit != null)
            {
                SelectedUnit.SetTilePath(tilePath);

                // remove the below if you dont want to move to be started immediately after defining a path
                StartCoroutine(tileMap.DoUnitMoves(SelectedUnit));
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
                SelectedUnit, 
                SelectedUnit.Tile, 
                tileUnderMouse, 
                Tile.CostEstimate
            );

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

    void Update_ScrollZoom()
    {
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        float minHeight = -12;
        float maxHeight = -4;
        if(scrollAmount != 0)
        {
            Vector3 p = Camera.main.transform.position;

            p.z += scrollAmount * 5;

            if (p.z < minHeight)
            {
                p.z = minHeight;
            }
            if (p.z > maxHeight)
            {
                p.z = maxHeight;
            }
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, p, Time.deltaTime * 50f);
        }
    }
}
