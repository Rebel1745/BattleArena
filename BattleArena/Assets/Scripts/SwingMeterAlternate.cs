using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwingMeterAlternate : SwingMeter {

    // Define max position for keysToPressPanel
    public Transform keysToPressPanelMax;
    // Define a color for the keysToPress panels so they can be reverted after they change to the color corresponding to the stopping color
    private int noOfKeys = 2;
    // list of all the keys to press
    private string currentKeyToPress;
    private int currentKeyToPressIndex = 0;
    // Define the number that will be added to on successful button press
    public float mashNo = 0f;
    // Number to add per successful button press
    public float noPerMash = 2f;
    // Max number allowed
    public float maxMashNo = 100f;
    // Speed mashNo decreases if no keys are pressed
    public float mashDepreciation = 5f;
	
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
            keyPressed = Input.inputString;
            if(keyPressed == currentKeyToPress)
            {
                StopSwinging(false);
            }
        }

        if (isSwinging)
        {
            // Move the swing meter according to the meterSpeed variable
            swingMeter.value += meterSpeed * Time.deltaTime;
            mashNo -= mashDepreciation * Time.deltaTime;

            mashNo = Mathf.Clamp(mashNo, 0, maxMashNo);

            // set the color of the keysToPressPanel to the same as the current mashNo placement on the slider
            Color currentMashNoColor = GetColorAtStoppingPoint(mashNo);
            keysToPressPanels[0].gameObject.GetComponent<Image>().color = currentMashNoColor;

            // move the keysToPressPanel to correspond with the mashNo
            Vector3 newPos = new Vector3(GetXPixelFromSwingMeterValue(mashNo), 0, 0);
            newPos = keysToPressPanelSpawns[0].position + newPos;
            newPos.x = Mathf.Clamp(newPos.x, 0, keysToPressPanelMax.position.x);
            keysToPressPanels[0].position = newPos;

            // If the meter gets to 100, swing is complete
            if (swingMeter.value >= swingMeter.maxValue)
            {
                StopSwinging(true);
            }
        }
    }

    void StopSwinging(bool complete)
    {
        // If the meter hits 100%
        if (complete)
        {
            // Check the color of the image corresponding with the position on the image the swing meter stopped on
            Color stoppingColor = GetColorAtStoppingPoint(mashNo);
            string hitType = CheckHitType(stoppingColor);

            // calculate the damage using the stats of the source unit
            int damage = sourceUnit.GetComponent<Unit>().CalculateDamage(sourceUnit, hitType);
            // apply the damage to the target unit
            targetUnit.GetComponent<Unit>().TakeDamage(damage);

            isSwinging = false;
            swingMeter.value = swingMeter.minValue;
            mashNo = 0f;
            Invoke("Close", 1f);
        }
        else
        {
            mashNo += noPerMash;
            mashNo = Mathf.Clamp(mashNo, 0, maxMashNo);

            currentKeyToPressIndex = 1 - currentKeyToPressIndex;
            currentKeyToPress = keysToPress[currentKeyToPressIndex];
        }

    }

    string CheckHitType(Color stoppingColor)
    {
        string textToAdd = "";
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

        return textToAdd;
    }

    public override void InitialiseKeys()
    {
        base.InitialiseKeys();

        mashNo = 0;

        keysToPressText[0].text = keysToPress[0] + " + " + keysToPress[1];

        currentKeyToPress = keysToPress[currentKeyToPressIndex];
    }

    public override void RandomiseKeys()
    {
        keysToPress = new string[noOfKeys];
        int x = 0;
        for (int i = 0; i < noOfKeys; i++)
        {
            x = Random.Range(0, possibleKeys.Length);
            if (i == 1)
            {
                if (possibleKeys[x] != keysToPress[0])
                {
                    keysToPress[i] = possibleKeys[x];
                }
                else
                {
                    i--;
                }
            }
            else
            {
                keysToPress[i] = possibleKeys[x];
            }
        }
    }
}
