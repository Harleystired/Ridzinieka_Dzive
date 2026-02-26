using UnityEngine;

public interface IHoverable2D
{ // interface for hoverable objects, the backbone of the hover highlight system, don't change
    void OnHoverEnter(RaycastHit2D hit);
    void OnHoverExit();
}
