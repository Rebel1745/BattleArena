using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwingMeterMatch : MonoBehaviour {

    public Slider swingMeter;
    // Image to compare colour of stopping point pixel to
    public Texture2D meterImage;

    public GameObject sourceUnit;
    public GameObject targetUnit;

    private bool isSwinging = false;

    // default speed of the swing meter
    public float meterSpeed = 20f;

    // Text to populate with list of keys
    public Text keysToPressText;
    // panel containing keysToPressText
    public Transform keysToPressPanel;
    // Define a color for the keysToPress panels so they can be reverted after they change to the color corresponding to the stopping color
    public Color panelColor;

    public bool keysToPressRandom = false;
    // number of keys to press per wave
    public int noOfKeys = 3;
    public int noOfWaves = 3;
    // list of all the keys to press
    public string[] keysToPress;
    // list of keys to press this wave
    private string[] keysToPressWave;
    // for randomised key presses, a list of the keys to choose from
    public string[] possibleKeys;

    // number of keys pressed so far
    private int keyPressNo = 0;
    // number of waves so far
    private int waveNo = 0;

    private string keyPressed = "";

    // Use this for initialization
    void Start () {
        // initials the keys to press for this wave using the number of keys per wave
        keysToPressWave = new string[noOfKeys];
        // if the list of keys we want to press doesnt add up to the total number of waves * keys per wave; randomise it all
        if(keysToPress.Length != noOfKeys * noOfWaves)
        {
            keysToPress = new string[noOfKeys*noOfWaves];
            keysToPressRandom = true;
            Debug.Log("Not enough strings in the keysToPress array.  Reinitialising with random values");
        }
        initialiseKeys();
        sourceUnit = TurnManager.instance.GetUnitInPlay();
        targetUnit = TurnManager.instance.GetTargetUnit();
    }
	
	// Update is called once per frame
	void Update () {
        // Press SPACE to start the swing meter
        if (!isSwinging && Input.GetButtonDown("Jump"))
        {
            isSwinging = true;
            return;
        }

        // The meter is swinging; a button was pressed; check to see whether it was the correct button
        if (isSwinging && Input.anyKeyDown)
        {
            StopSwinging(false);
        }

        if (isSwinging)
        {
            // Move the swing meter according to the meterSpeed variable
            swingMeter.value += meterSpeed * Time.deltaTime;

            // If the meter gets to 100 without the final required key being pressed, return a miss
            if (swingMeter.value >= swingMeter.maxValue)
            {
                keyPressNo = 0;
                StopSwinging(true);
            }
        }
    }

    void StopSwinging(bool missed)
    {
        // If a button was pressed (i.e not a miss) increment the current swing number
        if (!missed)
        {
            keyPressed = Input.inputString;
            if(keyPressed != keysToPress[keyPressNo])
            {
                missed = true;
                keyPressNo = 0;
            }
            keyPressNo++;
        }

        // when a wave completes, get the next set of keys to press
        if(keyPressNo == (noOfKeys * waveNo) && keyPressNo != (noOfKeys * noOfWaves))
        {
            GetNextWaveOfKeys();
        }

        // If it was the last key pressed (or a miss) reset the swing meter
        if (keyPressNo == (noOfKeys * noOfWaves) || missed)
        {
            // Check the color of the image corresponding with the position on the image the swing meter stopped on
            Color stoppingColor = GetColorAtStoppingPoint(swingMeter.value);
            string hitType = CheckHitType(stoppingColor, keysToPressPanel, missed);

            // calculate the damage using the stats of the source unit
            int damage = sourceUnit.GetComponent<Unit>().CalculateDamage(sourceUnit, hitType);
            // apply the damage to the target unit
            targetUnit.GetComponent<Unit>().TakeDamage(damage);

            isSwinging = false;
            swingMeter.value = swingMeter.minValue;
            keyPressNo = 0;
            Invoke("Close", 1f);
        }

    }

    string CheckHitType(Color stoppingColor, Transform panel, bool missed)
    {
        string textToAdd = "";
        if (!missed)
        {
            if (stoppingColor.g == 1f)
            {
                textToAdd = "crit";
            }
            else if (stoppingColor.b == 1f)
            {
                textToAdd = "normal";
            }
            else if (stoppingColor.r == 1f)
            {
                textToAdd = "weak";
            }
            panel.gameObject.GetComponent<Image>().color = stoppingColor;
        }
        else
        {
            panel.gameObject.GetComponent<Image>().color = Color.black;
            textToAdd = "miss";
        }

        return textToAdd;
    }

    float GetXPixelFromSwingMeterValue(float meterValue)
    {
        // Returns the pixel corresponding to the position of the swing meter click
        return (meterValue / 100f) * meterImage.width;
    }

    // returns the colour in the background image corresponding to the point the swing meter stopped
    Color GetColorAtStoppingPoint(float xPos)
    {
        float xPixel = GetXPixelFromSwingMeterValue(xPos);

        Color pixelColor = meterImage.GetPixel(Mathf.RoundToInt(xPixel), 0);

        return pixelColor;
    }

    void initialiseKeys()
    {
        keyPressNo = 0;
        waveNo = 0;

        // check to make sure possibleKeys is not null - if it is add 1,2,3 and 4 as options
        if (possibleKeys == null || possibleKeys.Length == 0)
        {
            possibleKeys = new string[4];
            possibleKeys[0] = "1";
            possibleKeys[1] = "2";
            possibleKeys[2] = "3";
            possibleKeys[3] = "4";
        }

        // if the keysToPress is going to be random, pick from the possibleKeys as many times as needed
        if (keysToPressRandom)
        {
            keysToPress = new string[noOfKeys * noOfWaves];
            int x = 0;
            for (int i = 0; i < noOfKeys * noOfWaves; i++)
            {                
                x = Random.Range(0, possibleKeys.Length);
                keysToPress[i] = possibleKeys[x];                
            }
        }

        // Load first wave of keysToPress
        GetNextWaveOfKeys();

        // set / reset the colour of the panel
        keysToPressPanel.gameObject.GetComponent<Image>().color = panelColor;
    }

    void GetNextWaveOfKeys()
    {
        for (int i = 0; i < noOfKeys; i++)
        {
            keysToPressWave[i] = keysToPress[keyPressNo + i];
        }

        keysToPressText.text = "";
        string keyText = "";
        // Display the keysToPress in the correct text fields
        for (int i = 0; i < keysToPressWave.Length; i++)
        {
            keyText = keysToPressWave[i].ToUpper();
            keysToPressText.text += keyText + "  ";
        }
        keysToPressText.text = keysToPressText.text.Trim();

        waveNo++;
    }

    public void Close()
    {
        MoveListController mlc = FindObjectOfType<MoveListController>();
        mlc.Cancel();
    }
}
