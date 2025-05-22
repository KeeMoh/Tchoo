using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchControllerScript : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    //[SerializeField] private PlayerControllerOLD playerControllerOLD;
    [SerializeField] private TextMeshProUGUI debugInfo;
    [SerializeField] private JumpSettings[] settings;
    private int settingsIndex = 0;
    //private bool newIsActive = true;

    private void Start()
    {
        switchNextIndex();
    }
    public void SwitchController(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switchNextIndex();
            //newIsActive = !newIsActive;
            //if (playerController.enabled)
            //{
            //    playerController.enabled = false;
            //    playerControllerOLD.enabled = true;
            //    debugInfo.text = "PC - OLD";
            //}
            //else
            //{
            //    playerControllerOLD.enabled = false;
            //    playerController.enabled = true;
            //    debugInfo.text = "PC - NEW";

            //}
        }
    }

    private void switchNextIndex()
    {
        playerController.SwitchSettings(settings[settingsIndex]);
        debugInfo.text = "settings number " + (settingsIndex + 1).ToString();
        settingsIndex = (settingsIndex + 1) % settings.Length;
    }

    //public void Move(InputAction.CallbackContext context)
    //{
    //        if (newIsActive)
    //        {
    //            playerController.Move(context);
    //        }
    //        else
    //        {
    //            playerControllerOLD.Move(context);
    //        }
    //}

    //public void Jump(InputAction.CallbackContext context)
    //{
        
    //        if (newIsActive)
    //        {
    //            playerController.Jump(context);
    //        }
    //        else
    //        {
    //            playerControllerOLD.Jump(context);
    //        }
        
    //}
}

[Serializable]
public struct JumpSettings
{
    [Header("o_ -------------- JUMP ELEMENT SETTING -------------- _o")]
    public float JumpForce;
    public float[] JumpHoldDurations;
    public float[] MinimumJumpForceForDurations;
    public float BaseGravity;
    public float GravityMultiplier;
    [Range(0, 1)] public float DecelerationValue;
    //public bool clampVelocityForDurations;
}
