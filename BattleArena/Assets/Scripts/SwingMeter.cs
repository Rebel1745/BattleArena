﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwingMeter : MonoBehaviour {

    public Slider swingMeter;
    public Texture2D meterImage;

    public GameObject sourceUnit;
    public GameObject targetUnit;

    private bool isSwinging = false;

    private bool canPress = true;
    private float pressAgainThreshold;
    private float nextPressThreshold;

    public float meterSpeed = 20f;

    //public Text resultText;

    public int noOfSwings;
    private int swingNo = 0;
    public bool keysToPressRandom = false;
    public string[] keysToPress;
    public string[] possibleKeys;

    // Define a color for the keysToPress panels so they can be reverted after they change to the color corresponding to the stopping color
    public Color panelColor;

    private string keyPressed = "";

    // Define an array of text boxes for the keysToPress
    public Text[] keysToPressText;
    // Define an array of transforms for the keysToPress (allows their position to follow the swingMeter.value)
    public Transform[] keysToPressPanels;
    // Define spawn points for the keysToPressPanels
    public Transform[] keysToPressPanelSpawns;
    // Keep a reference to the current moving panel
    private Transform currentPanel;
    private Transform currentPanelSpawn;

    // Use this for initialization
    void Start()
    {
        initialiseKeys();
        sourceUnit = TurnManager.instance.GetUnitInPlay();
        targetUnit = TurnManager.instance.GetTargetUnit();
    }

    // Update is called once per frame
    void Update()
    {
        // Press SPACE to start the swing meter
        if (!isSwinging && Input.GetButtonDown("Jump"))
        {
            isSwinging = true;
            return;
        }

        // The meter is swinging; a button was pressed; check to see whether it was the correct button
        if (isSwinging && canPress && Input.anyKeyDown)
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
                swingNo = 0;
                StopSwinging(true);
            }

            // Stop multiple clicking being allowed (double-tap)
            if(!canPress && swingMeter.value >= nextPressThreshold)
            {
                canPress = true;
                nextPressThreshold += pressAgainThreshold;
                currentPanel = keysToPressPanels[swingNo];
                currentPanelSpawn = keysToPressPanelSpawns[0];
            }

            // Move the current keyToPressPanel along with the slider value
            if(currentPanel != null)
            {
                Vector3 newPos = new Vector3(GetXPixelFromSwingMeterValue(swingMeter.value), 0, 0);
                currentPanel.position = currentPanelSpawn.position + newPos;
            }
        }
    }

    void initialiseKeys()
    {
        canPress = true;
        //resultText.text = "";

        // calculate threshold for next swing (i.e corresponding swingMeter.value)
        pressAgainThreshold = (1f / (float)noOfSwings) * 100f;
        nextPressThreshold = pressAgainThreshold;

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
            keysToPress = new string[noOfSwings];
            for (int i = 0; i < noOfSwings; i++)
            {
                int x = Random.Range(0, possibleKeys.Length);
                keysToPress[i] = possibleKeys[x];
            }
        }

        // Place the keysToPressPanels in the positions dictated by keysToPressPanelSpawns
        for (int i = 0; i < keysToPressPanels.Length; i++)
        {
            keysToPressPanels[i].position = keysToPressPanelSpawns[i].position;
            keysToPressPanels[i].gameObject.GetComponent<Image>().color = panelColor;
        }

        // Display the keysToPress in the correct text fields
        for (int i = 0; i < keysToPressText.Length; i++)
        {
            string keyText = keysToPress[i].ToUpper();
            if (keyText == " ")
            {
                keyText = "SPACE";
            }
            keysToPressText[i].text = keyText;
        }

        // set up the panel to start following the swingMeter.value
        currentPanel = keysToPressPanels[0];
        currentPanelSpawn = keysToPressPanelSpawns[0];
    }

    void StopSwinging(bool missed)
    {
        canPress = false;

        // If a button was pressed (i.e not a miss) increment the current swing number
        if (!missed)
        {
            swingNo++;
            keyPressed = Input.inputString;
        }

        // Check the color of the image corresponding with the position on the image the swing meter stopped on
        Color stoppingColor = GetColorAtStoppingPoint(swingMeter.value);
        string hitType = CheckHitType(stoppingColor, currentPanel);
        
        // calculate the damage using the stats of the source unit
        int damage = sourceUnit.GetComponent<Unit>().CalculateDamage(sourceUnit, hitType);
        // apply the damage to the target unit
        targetUnit.GetComponent<Unit>().TakeDamage(damage);

        currentPanel = null;
        currentPanelSpawn = null;

        // If it was the last key pressed (or a miss) reset the swing meter
        if (swingNo == noOfSwings || missed)
        {
            isSwinging = false;
            swingMeter.value = swingMeter.minValue;
            swingNo = 0;
            Invoke("Close",1f);
        }
        
    }

    float GetXPixelFromSwingMeterValue(float meterValue)
    {
        // Returns the pixel corresponding to the position of the swing meter click
        return (meterValue / 100f) * meterImage.width;
    }

    Color GetColorAtStoppingPoint(float xPos)
    {
        float xPixel = GetXPixelFromSwingMeterValue(xPos);

        Color pixelColor = meterImage.GetPixel(Mathf.RoundToInt(xPixel), 0);

        return pixelColor;
    }

    string CheckHitType(Color stoppingColor, Transform panel)
    {
        string textToAdd = "";
        bool wrongButton = false;
        if (swingNo > 0)
        {
            if (stoppingColor.g == 1f)
            {
                // CRIT BABY!
                if (keyPressed == keysToPress[swingNo - 1])
                {
                    textToAdd = "crit";
                }
                else
                {
                    textToAdd = "miss";
                    wrongButton = true;
                    swingNo = noOfSwings;
                }
            }
            else if (stoppingColor.b == 1f)
            {
                if (keyPressed == keysToPress[swingNo - 1])
                {
                    textToAdd = "normal";
                }
                else
                {
                    textToAdd = "miss";
                    wrongButton = true;
                    swingNo = noOfSwings;
                }
            }
            else if (stoppingColor.r == 1f)
            {
                if (keyPressed == keysToPress[swingNo - 1])
                {
                    textToAdd = "weak";
                }
                else
                {
                    textToAdd = "miss";
                    wrongButton = true;
                    swingNo = noOfSwings;
                }
            }

            if (wrongButton)
            {
                panel.gameObject.GetComponent<Image>().color = Color.black;
            }
            else
            {
                panel.gameObject.GetComponent<Image>().color = stoppingColor;
            }
            
        }
        else
        {
            textToAdd = "miss";
        }

        if(swingNo == 1)
        {
            //resultText.text = textToAdd;
        }
        else
        {
            //resultText.text += "\n" + textToAdd;
        }

        return textToAdd;
    }

    public void Close()
    {
        MoveListController mlc = FindObjectOfType<MoveListController>();
        mlc.Cancel();
    }
}