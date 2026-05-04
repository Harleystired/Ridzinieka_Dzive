using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioPanelTMP : MonoBehaviour, IScenarioPanel
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text promptText;

    [Header("Buttons")]
    [SerializeField] private Button button1;
    [SerializeField] private TMP_Text button1Text;

    [SerializeField] private Button button2;
    [SerializeField] private TMP_Text button2Text;

    [SerializeField] private Button button3;
    [SerializeField] private TMP_Text button3Text;

    private Action<int> _onChoicePicked;

    public bool IsVisible => root != null && root.activeSelf;

    private void Awake()
    {
        if (root == null) root = gameObject;
        Hide();
    }

    public void Show(string prompt, string choice1, string choice2, string choice3, Action<int> onChoicePicked)
    {
        _onChoicePicked = onChoicePicked;

        if (promptText != null) promptText.text = prompt ?? "";

        if (button1Text != null) button1Text.text = choice1 ?? "";
        if (button2Text != null) button2Text.text = choice2 ?? "";
        if (button3Text != null) button3Text.text = choice3 ?? "";

        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();

        button1.onClick.AddListener(() => Pick(0));
        button2.onClick.AddListener(() => Pick(1));

        bool showThird = !string.IsNullOrWhiteSpace(choice3);
        if (button3 != null)
        {
            button3.onClick.RemoveAllListeners();
            button3.gameObject.SetActive(showThird);
        }

        if (showThird && button3 != null)
            button3.onClick.AddListener(() => Pick(2));

        root.SetActive(true);
    }

    public void Hide()
    {
        _onChoicePicked = null;

        if (root != null) root.SetActive(false);
    }

    private void Pick(int index)
    {
        _onChoicePicked?.Invoke(index);
    }
}
