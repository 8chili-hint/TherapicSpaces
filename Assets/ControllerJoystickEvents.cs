using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Collections;
public class ControllerJoystickEvents : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionProperty leftJoystickAction;
    public InputActionProperty rightJoystickAction;


    public InputActionProperty leftJoystickActionForTrigger;
    public InputActionProperty rightJoystickActionForTrigger;

    public InputActionProperty BackButtonAction;

   
    [Header("LeftControllerEvents")]
    public UnityEvent onJoystickLeftLeft; // For left joystick moving left
    public UnityEvent onJoystickLeftRight;

    public UnityEvent onTriggerPressed;
    [Header("LeftControllerEvents")]
    public UnityEvent onJoystickRightLeft; // For right joystick moving left
    public UnityEvent onJoystickRightRight; // For right joystick moving right

    public UnityEvent OnBackButtonPressed; 

    [Header("Thresholds")]
    public float triggerThreshold = 0.5f; // How far to move the joystick to trigger the event



    private Vector2  leftJoystickValue, rightJoystickValue;
    bool RightRoutineHasStarted, LeftRoutineHasStarted;

    private void OnEnable()
    {
        leftJoystickActionForTrigger.action.performed += ((arg0) => { onTriggerPressed?.Invoke(); });
        rightJoystickActionForTrigger.action.performed += ((arg0) => { onTriggerPressed?.Invoke(); });

        if (BackButtonAction != null)
        {
            BackButtonAction.action.performed += ((arg0) => { OnBackButtonPressed?.Invoke(); });
        }
    }
    private void OnDisable()
    {
        leftJoystickActionForTrigger.action.performed -= ((arg0) => { onTriggerPressed?.Invoke(); });
        rightJoystickActionForTrigger.action.performed -= ((arg0) => { onTriggerPressed?.Invoke(); });

        if (BackButtonAction != null)
        {
            BackButtonAction.action.performed -= ((arg0) => { OnBackButtonPressed?.Invoke(); });
        }
    }
    private void Start()
    {
        StartCoroutine(JoystickRightTriggerEvent());
        StartCoroutine(JoystickLeftTriggerEvent());

    }
    private void Update()
    {
        // Handle Left Controller Joystick
        if (leftJoystickAction.action != null)
        {
            leftJoystickValue = leftJoystickAction.action.ReadValue<Vector2>();
         /*   if (!LeftRoutineHasStarted)
            {
                if (leftJoystickValue.x != 0)
                {
                  //  StartCoroutine(JoystickLeftTriggerEvent());
                    LeftRoutineHasStarted = true;

                }
                else
                {
                    //StopCoroutine(JoystickLeftTriggerEvent());
                    LeftRoutineHasStarted = false;
                }

            }*/
           
        }

        // Handle Right Controller Joystick
        if (rightJoystickAction.action != null)
        {
            rightJoystickValue = rightJoystickAction.action.ReadValue<Vector2>();
           // Debug.LogError(rightJoystickValue);
         /*   if (!RightRoutineHasStarted)
            {
            if (rightJoystickValue.x != 0)
            {
               // StartCoroutine(JoystickRightTriggerEvent());
                    RightRoutineHasStarted = true;
            }
            else
            {
                //StopCoroutine(JoystickRightTriggerEvent());
                    RightRoutineHasStarted = false;

                }

            }*/
        }
        
    }
    [ContextMenu("JoystickRight")]
    public void JoystickRight()
    {
        onJoystickRightRight?.Invoke();
    }
    [ContextMenu("JoystickLeft")]

    public void JoyStickLeft()
    {
        onJoystickRightLeft?.Invoke();

    }

    [ContextMenu("PressTRigger")]

    public void SelectSelectedItem()
    {
        onTriggerPressed?.Invoke();
    }
    private IEnumerator JoystickLeftTriggerEvent()
    {
        while (true) // This outer loop will run forever
        {
                if (leftJoystickValue.x < -triggerThreshold)
                {
                    onJoystickLeftLeft?.Invoke();
                }
                else if (leftJoystickValue.x > triggerThreshold)
                {
                    onJoystickLeftRight?.Invoke();
                }
                yield return new WaitForSeconds(1f);
            
            yield return null;
       
        }
    }

    private IEnumerator JoystickRightTriggerEvent()
    {
        while (true) // This outer loop will run forever
        {

           
                if (rightJoystickValue.x < -triggerThreshold)
                {
                    onJoystickRightLeft?.Invoke();
                }
                else if (rightJoystickValue.x > triggerThreshold)
                {
                    onJoystickRightRight?.Invoke();
                }

                yield return new WaitForSeconds(0.25f);
            
            yield return null;

            
        }
    }
}
    public enum joystickState{ Joystickreleased, JoystickActive}