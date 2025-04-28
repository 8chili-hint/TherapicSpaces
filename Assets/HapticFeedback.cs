using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;

public class HapticFeedback : MonoBehaviour
{
    // Public variables for inspector control
    public float duration = 0.1f; // Default duration of the haptic feedback
    public float amplitude = 0.7f; // Default amplitude (intensity) of the haptic feedback
    public XRNode hand = XRNode.RightHand; // Default hand to apply haptics to.
    public uint channel = 0; // The haptic channel (default to 0)

    private InputDevice inputDevice;

    //Get the input device
    void Start()
    {
        inputDevice = InputDevices.GetDeviceAtXRNode(hand);
    }


    /// <summary>
    /// Sends a haptic impulse to the specified XRNode (hand).
    /// </summary>
    /// <param name="xrNode">The XRNode (hand) to send haptics to (e.g., XRNode.LeftHand, XRNode.RightHand).</param>
    /// <param name="amplitude">The strength of the haptic feedback (0.0 to 1.0).</param>
    /// <param name="duration">The duration of the haptic feedback in seconds.</param>
    public void SendHapticImpulse(float amplitude, float duration)
    {
        // Ensure the amplitude is within the valid range
        amplitude = Mathf.Clamp01(amplitude);

        // Get the correct InputDevice
        if (!inputDevice.isValid)
        {
            inputDevice = InputDevices.GetDeviceAtXRNode(hand);
        }
        if (inputDevice.isValid)
        {
            inputDevice.SendHapticImpulse(channel, amplitude, duration);
        }
        else
        {
            Debug.LogWarning("Input device not valid.  Haptics not sent.");
        }


    }

    /// <summary>
    /// Sends a haptic impulse to the specified XRNode (hand) using the default values.
    /// </summary>
    ///  <param name="xrNode">The XRNode (hand) to send haptics to (e.g., XRNode.LeftHand, XRNode.RightHand).</param>
    public void SendHapticImpulse()
    {
        SendHapticImpulse(amplitude, duration);
    }
}
