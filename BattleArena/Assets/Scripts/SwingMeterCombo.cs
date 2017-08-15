using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwingMeterCombo : SwingMeter {

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
            if (swingMeter.value >= swingMeter.maxValue  || (canPress && swingMeter.value >= nextPressThreshold))
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

    public override void InitialiseKeys()
    {
        base.InitialiseKeys();

        // calculate threshold for next swing (i.e corresponding swingMeter.value)
        pressAgainThreshold = (1f / (float)noOfSwings) * 100f;
        nextPressThreshold = pressAgainThreshold;

        // Display the keysToPress in the correct text fields
        for (int i = 0; i < keysToPressText.Length; i++)
        {
            string keyText = keysToPress[i].ToUpper();
            keysToPressText[i].text = keyText;
        }

        // set up the panel to start following the swingMeter.value
        currentPanel = keysToPressPanels[0];
        currentPanelSpawn = keysToPressPanelSpawns[0];
    }

    public override void RandomiseKeys()
    {
        keysToPress = new string[noOfSwings];
        for (int i = 0; i < noOfSwings; i++)
        {
            int x = Random.Range(0, possibleKeys.Length);
            keysToPress[i] = possibleKeys[x];
        }
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

    string CheckHitType(Color stoppingColor, Transform panel)
    {
        string textToAdd = stoppingColor.ToString();
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
            panel.gameObject.GetComponent<Image>().color = Color.black;
            textToAdd = "miss";
        }

        return textToAdd;
    }
}
