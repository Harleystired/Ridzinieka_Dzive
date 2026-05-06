using System;
using UnityEngine;

[Serializable]
public class TutorialStep
{
    public string id;          // Trigger ID
    public string text;        // Tooltip text
    public Vector2 position;   // Tooltip screen position
}