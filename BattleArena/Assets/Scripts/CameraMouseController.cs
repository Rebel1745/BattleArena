using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMouseController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    bool isDraggingCamera = false;
    Vector3 lastMousePosition;
    Vector3 cameraTargetOffset;

    // Update is called once per frame
    void Update () {

        Ray mouseRay = Camera.main.ScreenPointToRay (Input.mousePosition);
        // What is the point at which the mouse ray intersects Y=0
        if (mouseRay.direction.y <= 0) {
            //Debug.LogError("Why is mouse pointing up?");
            return;
        }
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);
        Vector3 hitPos = mouseRay.origin - (mouseRay.direction * rayLength);

        if (Input.GetMouseButtonDown(0))
        {
            // Mouse button just went down -- start a drag.
            isDraggingCamera = true;

            lastMousePosition = hitPos;
        } else if (Input.GetMouseButtonUp(0))
        {
            // Mouse button went up, stop drag
            isDraggingCamera = false;
        }

        if (isDraggingCamera)
        {
            Vector3 diff = lastMousePosition - hitPos;
            Camera.main.transform.Translate (diff, Space.World);
            mouseRay = Camera.main.ScreenPointToRay (Input.mousePosition);
            // What is the point at which the mouse ray intersects Y=0
            if (mouseRay.direction.y <= 0) {
                Debug.LogError ("Why is mouse pointing up?");
                return;
            }
            rayLength = (mouseRay.origin.y / mouseRay.direction.y);
            lastMousePosition = hitPos = mouseRay.origin - (mouseRay.direction * rayLength);
        }
    }
}
