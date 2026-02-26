using UnityEngine;

[DisallowMultipleComponent]
public class HoverHighlight2D : MonoBehaviour, IHoverable2D
{
    // highlights the object when hovered, put the script o any object you wan't to highlight
    
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.6f, 1f);

    private void Reset()
    {
        targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (targetRenderer != null)
            targetRenderer.color = normalColor;
    }

    public void OnHoverEnter(RaycastHit2D hit)
    {
        if (targetRenderer != null)
            targetRenderer.color = hoverColor;
    }

    public void OnHoverExit()
    {
        if (targetRenderer != null)
            targetRenderer.color = normalColor;
    }
}
