using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        // Clear any lingering selection before showing new scenario
        ClearButtonSelection();
        
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

        // Reset button colors by forcing them to refresh their visual state
        ResetButtonVisualState(button1);
        ResetButtonVisualState(button2);
        if (button3 != null) ResetButtonVisualState(button3);
        
        root.SetActive(true);
    }

    public void Hide()
    {
        _onChoicePicked = null;
        
        // Clear selection when hiding
        ClearButtonSelection();

        if (root != null) root.SetActive(false);
    }

    private void Pick(int index)
    {
        _onChoicePicked?.Invoke(index);
        
        // Clear selection immediately after picking to prevent it from persisting
        ClearButtonSelection();
    }
    
    private void ClearButtonSelection()
    {
        // Tell the EventSystem to clear the current selection
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
    
    private void ResetButtonVisualState(Button button)
    {
        if (button == null) return;
        
        // Force the button to refresh its transition state
        button.OnDeselect(null);
        
        // If using color tint, force it back to normal color
        var colors = button.colors;
        var targetColor = colors.normalColor;
        
        // Hack to force transition refresh
        button.targetGraphic.canvasRenderer.SetColor(targetColor);
    }
    
}
