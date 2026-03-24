using System.Collections;
using TMPro;
using UnityEngine;

public class SimpleTooltip : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text text;
    [SerializeField] private float defaultSeconds = 2.0f;

    private Coroutine _routine;

    private void Awake()
    {
        if (root == null) root = gameObject;
        root.SetActive(false);
    }

    public void Show(string message, float? seconds = null)
    {
        EnsureHierarchyActive();

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine(message, seconds ?? defaultSeconds));
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

    private IEnumerator ShowRoutine(string message, float seconds)
    {
        if (text != null) text.text = message ?? "";
        root.SetActive(true);
        yield return new WaitForSecondsRealtime(seconds);
        root.SetActive(false);
        _routine = null;
    }
}
