using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SimpleTooltip : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text text;
    [SerializeField] private float defaultSeconds = 2.0f;

    [Header("Cursor Follow")]
    [SerializeField] private bool followCursor;
    [SerializeField] private Vector2 cursorOffset = new Vector2(20f, -20f);

    [Header("Dynamic Sizing")]
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private RectTransform textRectTransform;
    [SerializeField] private Vector2 minSize = new Vector2(434f, 100f);  // Your desired min size
    [SerializeField] private Vector2 maxSize = new Vector2(800f, 400f);
    [SerializeField] private Vector2 padding = new Vector2(20f, 15f);

    private Coroutine _routine;
    private Vector2 originalPanelSize;

    private void Awake()
    {
        if (root == null) root = gameObject;
        
        // Get references if not set
        if (panelRectTransform == null && root != null)
            panelRectTransform = root.GetComponent<RectTransform>();
        
        if (textRectTransform == null && text != null)
            textRectTransform = text.GetComponent<RectTransform>();
        
        // Save original size for reference
        if (panelRectTransform != null)
            originalPanelSize = panelRectTransform.sizeDelta;
        
        // Remove any interfering components
        RemoveConflictingComponents();
        
        // Force text to have flexible sizing
        if (textRectTransform != null)
        {
            // Reset anchors to allow proper sizing
            textRectTransform.anchorMin = new Vector2(0, 0);
            textRectTransform.anchorMax = new Vector2(1, 1);
            textRectTransform.pivot = new Vector2(0.5f, 0.5f);
            textRectTransform.offsetMin = Vector2.zero;
            textRectTransform.offsetMax = Vector2.zero;
        }
        
        DisableRaycasts();
        root.SetActive(false);
    }

    private void RemoveConflictingComponents()
    {
        // Remove ContentSizeFitter from panel and children
        ContentSizeFitter[] sizeFitters = root.GetComponentsInChildren<ContentSizeFitter>(true);
        foreach (var fitter in sizeFitters)
        {
            Destroy(fitter);
        }
        
        // Remove LayoutGroup components
        LayoutGroup[] layoutGroups = root.GetComponentsInChildren<LayoutGroup>(true);
        foreach (var layout in layoutGroups)
        {
            Destroy(layout);
        }
        
        // Remove LayoutElement if it's forcing a size
        LayoutElement[] layoutElements = root.GetComponentsInChildren<LayoutElement>(true);
        foreach (var element in layoutElements)
        {
            Destroy(element);
        }
    }

    public void Show(string message, float? seconds = null)
    {
        EnsureHierarchyActive();
        
        // Set the text first
        if (text != null) 
            text.text = message ?? "";
        
        // Wait one frame for Unity to calculate layout
        StartCoroutine(ShowAfterLayout(message, seconds ?? defaultSeconds));
    }

    private IEnumerator ShowAfterLayout(string message, float seconds)
    {
        // Wait for end of frame to ensure text has been updated
        yield return new WaitForEndOfFrame();
        
        // Force text mesh update
        text.ForceMeshUpdate();
        
        // Resize panel
        ResizePanel();
        
        // Show the panel
        root.SetActive(true);
        
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(HideRoutine(seconds));
    }

    public void ShowAtCursor(string message, float? seconds = null)
    {
        followCursor = true;
        Show(message, seconds);
        MoveToCursor();
    }

    public void Hide()
    {
        followCursor = false;

        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        if (root != null)
            root.SetActive(false);
    }

    private void ResizePanel()
    {
        if (panelRectTransform == null || text == null || textRectTransform == null)
        {
            Debug.LogError("Missing references for tooltip resizing!", this);
            return;
        }
        
        // Force a rebuild of the text mesh to get accurate preferred dimensions
        text.ForceMeshUpdate();
        
        // Get the text's preferred size
        Vector2 preferredSize = new Vector2(text.preferredWidth, text.preferredHeight);
        
        // For debugging - check what values you're getting
        Debug.Log($"Text: '{text.text}' - Preferred Size: {preferredSize}");
        
        // Calculate desired panel size with padding
        float panelWidth = Mathf.Clamp(preferredSize.x + padding.x, minSize.x, maxSize.x);
        float panelHeight = Mathf.Clamp(preferredSize.y + padding.y, minSize.y, maxSize.y);
        
        Debug.Log($"Setting panel size to: {panelWidth} x {panelHeight}");
        
        // Set panel size
        panelRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
        panelRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
        
        // Set text size to fill the panel (accounting for padding)
        float textWidth = panelWidth - padding.x;
        float textHeight = panelHeight - padding.y;
        
        textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
        textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
        
        // Reset text position to center
        textRectTransform.anchoredPosition = Vector2.zero;
        
        // Force immediate layout update
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
    }

    private void MoveToCursor()
    {
        if (root == null || !root.activeSelf)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 tooltipPosition = mousePosition + cursorOffset;
        
        // Keep tooltip within screen bounds
        if (panelRectTransform != null)
        {
            Vector2 tooltipSize = panelRectTransform.sizeDelta;
            
            // Check right edge
            if (tooltipPosition.x + tooltipSize.x > Screen.width)
                tooltipPosition.x = mousePosition.x - tooltipSize.x - cursorOffset.x;
            
            // Check left edge
            if (tooltipPosition.x < 0)
                tooltipPosition.x = 5;
            
            // Check top edge
            if (tooltipPosition.y + tooltipSize.y > Screen.height)
                tooltipPosition.y = mousePosition.y - tooltipSize.y - cursorOffset.y;
            
            // Check bottom edge
            if (tooltipPosition.y < 0)
                tooltipPosition.y = 5;
        }

        panelRectTransform.position = tooltipPosition;
    }
    
    private void EnsureHierarchyActive()
    {
        Transform t = transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf)
                t.gameObject.SetActive(true);

            t = t.parent;
        }
    }

    private void DisableRaycasts()
    {
        if (root == null)
            return;

        Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);

        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].raycastTarget = false;
        }

        CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = root.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private IEnumerator HideRoutine(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        root.SetActive(false);
        followCursor = false;
        _routine = null;
    }
}
