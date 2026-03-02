using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private List<GameObject> panels;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(string panelName)
    {
        foreach (var p in panels)
            p.SetActive(p.name == panelName);
    }

    public void HideAll()
    {
        foreach (var p in panels)
            p.SetActive(false);
    }
}