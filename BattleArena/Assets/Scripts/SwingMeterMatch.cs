using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwingMeterMatch : SwingMeter {
    
    // number of keys to press per wave
    public int noOfKeys = 3;
    public int noOfWaves = 3;
    // list of keys to press this wave
    private string[] keysToPressWave;
    
    // number of keys pressed so far
    private int keyPressNo = 0;
    // number of waves so far
    private int waveNo = 0;
	
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
            string hitType = CheckHitType(stoppingColor, keysToPressPanels[0], missed);

            // calculate the damage using the stats of the source unit
            int damage = sourceUnit.CalculateDamage(sourceUnit, hitType);
            // apply the damage to the target unit

            targetUnit.TakeDamage(damage);

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

    public override void InitialiseKeys()
    {
        base.InitialiseKeys();
        // initials the keys to press for this wave using the number of keys per wave
        keysToPressWave = new string[noOfKeys];

        // if the list of keys we want to press doesnt add up to the total number of waves * keys per wave; randomise it all
        if (keysToPress.Length != noOfKeys * noOfWaves)
        {
            RandomiseKeys();
        }

        keyPressNo = 0;
        waveNo = 0;

        // Load first wave of keysToPress
        GetNextWaveOfKeys();
    }

    public override void RandomiseKeys()
    {
        keysToPress = new string[noOfKeys * noOfWaves];
        int x = 0;
        for (int i = 0; i < noOfKeys * noOfWaves; i++)
        {
            x = Random.Range(0, possibleKeys.Length);
            keysToPress[i] = possibleKeys[x];
        }
    }

    void GetNextWaveOfKeys()
    {
        for (int i = 0; i < noOfKeys; i++)
        {
            keysToPressWave[i] = keysToPress[keyPressNo + i];
        }

        keysToPressText[0].text = "";
        string keyText = "";
        // Display the keysToPress in the correct text fields
        for (int i = 0; i < keysToPressWave.Length; i++)
        {
            keyText = keysToPressWave[i].ToUpper();
            keysToPressText[0].text += keyText + "  ";
        }
        keysToPressText[0].text = keysToPressText[0].text.Trim();

        waveNo++;
    }
}
