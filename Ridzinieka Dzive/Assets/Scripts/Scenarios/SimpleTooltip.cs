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

    private Coroutine _routine;

    private void Awake()
    {
        if (root == null) root = gameObject;

        DisableRaycasts();

        root.SetActive(false);
    }

    private void Update()
    {
        if (!followCursor)
            return;

        if (root == null || !root.activeSelf)
            return;

        MoveToCursor();
    }

    public void Show(string message, float? seconds = null)
    {
        EnsureHierarchyActive();

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine(message, seconds ?? defaultSeconds));
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

    private void MoveToCursor()
    {
        if (root == null)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 tooltipPosition = mousePosition + cursorOffset;

        RectTransform rectTransform = root.GetComponent<RectTransform>();

        if (rectTransform != null)
            rectTransform.position = tooltipPosition;
        else
            root.transform.position = tooltipPosition;
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

    private IEnumerator ShowRoutine(string message, float seconds)
    {
        if (text != null) text.text = message ?? "";
        root.SetActive(true);
        yield return new WaitForSecondsRealtime(seconds);
        root.SetActive(false);
        followCursor = false;
        _routine = null;
    }
}
