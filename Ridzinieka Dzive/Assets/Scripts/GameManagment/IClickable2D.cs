using UnityEngine;

public interface IClickable2D
{
    // interface for clickable objects, the backbone of the click system, don't change'
    void OnClicked(RaycastHit2D hit);
}

