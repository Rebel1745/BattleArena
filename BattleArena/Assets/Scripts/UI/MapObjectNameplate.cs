using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapObjectNameplate : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (TheCamera == null)
            TheCamera = Camera.main;

        rectTransform = GetComponent<RectTransform>();
	}

    public GameObject MyTarget;
    public Vector3 WorldPositionOffset = new Vector3(0, 0.1f, 0);
    public Vector3 ScreenPositionOffset = new Vector3(0, 30, 0);

    public Camera TheCamera;

    RectTransform rectTransform;
	
	// LateUpdate is called after the normal update; this prevents jitters as the camera is updated (moved)
	void LateUpdate () {

        if(MyTarget == null)
        {
            // the object we are tracking has been removed.  DESTROY OURSELVES
            Destroy(gameObject);
            return;
        }

        Vector3 screenPos = TheCamera.WorldToScreenPoint(MyTarget.transform.position + WorldPositionOffset);

        rectTransform.anchoredPosition = screenPos + ScreenPositionOffset;
	}
}
