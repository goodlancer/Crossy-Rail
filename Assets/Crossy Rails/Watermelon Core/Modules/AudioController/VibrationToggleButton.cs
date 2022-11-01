using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class VibrationToggleButton : MonoBehaviour
{
    [SerializeField]
    public Image graphic;

    [Space]
    [SerializeField]
    private Color activeColor = Color.white;
    [SerializeField]
    private Color disableColor = Color.white;

    private bool isActive = true;

    private void Start()
    {
        isActive = GameSettingsPrefs.Get<bool>("vibration");

        if (isActive)
            graphic.color = activeColor;
        else
            graphic.color = disableColor;
    }

    public void SwitchState()
    {
        isActive = !isActive;

        if (isActive)
        {
            graphic.color = activeColor;

            GameSettingsPrefs.Set("vibration", true);
        }
        else
        {
            graphic.color = disableColor;

            GameSettingsPrefs.Set("vibration", false);
        }
    }
}
