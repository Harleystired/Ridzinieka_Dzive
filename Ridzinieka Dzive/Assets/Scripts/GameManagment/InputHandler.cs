using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    // handles the input, add any extra input you wan't to handle'
    
    private Camera mainCam;
    private IHoverable2D currentHoverable;
    
    private void Awake()
    {
        mainCam = Camera.main;
    }
   
    private void Update()
    {
        UpdateHover();
    }
    
    private bool ShouldBlockWorldInput()
    {
        if (UIModal.IsAnyOpen)
            return true;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;

        return false;
    }

    private void UpdateHover()
    {
        if (mainCam == null || Mouse.current == null) return;
        if (ShouldBlockWorldInput()) return;

        var ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hit = Physics2D.GetRayIntersection(ray);

        IHoverable2D newHoverable = null;

        if (hit.collider != null)
            hit.collider.TryGetComponent<IHoverable2D>(out newHoverable);

        if (ReferenceEquals(newHoverable, currentHoverable)) return;

        if (currentHoverable != null)
            currentHoverable.OnHoverExit();

        currentHoverable = newHoverable;

        if (currentHoverable != null)
            currentHoverable.OnHoverEnter(hit);
    }
    
    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (ShouldBlockWorldInput()) return;

        var ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hit = Physics2D.GetRayIntersection(ray);
        if (!hit.collider) return;

        Debug.Log(hit.collider.gameObject.name);

        if (hit.collider.TryGetComponent<IClickable2D>(out var clickable))
        {
            clickable.OnClicked(hit);
        }
    }
}
