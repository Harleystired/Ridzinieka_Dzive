using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FridgeItemUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityText;
    public Image icon;

    private string itemName;
    private GameManager gameManager;
    private FridgeUI fridgeUI;

    public void Setup(string name, Sprite itemIcon, GameManager gm, FridgeUI ui)
    {
        itemName = name;
        gameManager = gm;
        fridgeUI = ui;

        nameText.text = name;
        icon.sprite = itemIcon;

        int count = 0;
        foreach (var item in gameManager.ownedItems)
            if (item == name) count++;

        quantityText.text = count + "x";
        var button = GetComponent<Button>();
        if (button != null) button.interactable = gameManager.hunger < 0;
    }

    public void OnClick()
    {
        // Ja jau esi paēdis (hunger = 0), neko nedarām
        if (gameManager.hunger >= 0)
            return;

        int hungerGain = fridgeUI.itemDatabase.GetHungerValue(itemName);

        // hungerGain ir pozitīvs, piemēram +5
        gameManager.AddHunger(hungerGain);

        // Izņemam vienu gabalu tikai tad, ja tiešām ēdām
        gameManager.ownedItems.Remove(itemName);

        fridgeUI.RefreshFridge();

        var statsUI = FindFirstObjectByType<StatsUI>();
        if (statsUI != null)
            statsUI.UpdateStats();
        
    }

}