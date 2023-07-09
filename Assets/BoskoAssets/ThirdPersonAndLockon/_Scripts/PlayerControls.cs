using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Customizable Controls")]
    [TextArea]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string Notes = "Target swapping is currently ---MouseScroll--- and not customizable yet";

    public string inputHorizontal = "Horizontal";
    public string inputVertical = "Vertical";
    public KeyCode grab = KeyCode.E;
    public KeyCode target = KeyCode.F;
    public KeyCode jump = KeyCode.Space;
    public KeyCode equip = KeyCode.Q;
    public KeyCode letgo = KeyCode.X;
    public KeyCode crouch = KeyCode.C;
    public KeyCode sprint = KeyCode.LeftShift;
}