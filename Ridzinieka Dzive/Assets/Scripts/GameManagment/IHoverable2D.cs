using UnityEngine;

public interface IHoverable2D
{
    void OnHoverEnter(RaycastHit2D hit);
    void OnHoverExit();
}
