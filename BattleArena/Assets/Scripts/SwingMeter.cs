using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwingMeter : MonoBehaviour {

    public Slider swingMeter;
    public Texture2D meterImage;

    public GameObject sourceUnit;
    public GameObject targetUnit;

    public bool isSwinging = false;

    public bool canPress = true;
    public float pressAgainThreshold;
    public float nextPressThreshold;

    public float meterSpeed = 20f;

    public int noOfSwings;
    public int swingNo = 0;
    public bool keysToPressRandom = false;
    public string[] keysToPress;
    public string[] possibleKeys;

    // Define a color for the keysToPress panels so they can be reverted after they change to the color corresponding to the stopping color
    public Color panelColor;

    public string keyPressed = "";

    // Define an array of text boxes for the keysToPress
    public Text[] keysToPressText;
    // Define an array of transforms for the keysToPress (allows their position to follow the swingMeter.value)
    public Transform[] keysToPressPanels;
    // Define spawn points for the keysToPressPanels
    public Transform[] keysToPressPanelSpawns;
    // Keep a reference to the current moving panel
    public Transform currentPanel;
    public Transform currentPanelSpawn;

    // Use this for initialization
    void Start () {
        InitialiseKeys();
        sourceUnit = TurnManager.instance.GetUnitInPlay();
        targetUnit = TurnManager.instance.GetTargetUnit();
    }

    public virtual void InitialiseKeys(){}

    public float GetXPixelFromSwingMeterValue(float meterValue)
    {
        // Returns the pixel corresponding to the position of the swing meter click
        return (meterValue / 100f) * meterImage.width;
    }

    public Color GetColorAtStoppingPoint(float xPos)
    {
        float xPixel = GetXPixelFromSwingMeterValue(xPos);

        Color pixelColor = meterImage.GetPixel(Mathf.RoundToInt(xPixel), 0);

        return pixelColor;
    }

    public void Close()
    {
        MoveListController mlc = FindObjectOfType<MoveListController>();
        mlc.Cancel();
    }
}
