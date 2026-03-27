using System.Collections.Generic;
using UnityEngine;

public class FridgeUI : MonoBehaviour
{
    public GameManager gameManager;
    public ItemDatabase itemDatabase;
    public Transform itemContainer;
    public GameObject itemPrefab;

    private void OnEnable()
    {
        RefreshFridge();
    }

    public void RefreshFridge()
    {
        // Notīrām vecos itemus
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        // Grupējam pēc nosaukuma
        var grouped = new Dictionary<string, int>();

        foreach (string item in gameManager.ownedItems)
        {
            if (!grouped.ContainsKey(item))
                grouped[item] = 0;
            grouped[item]++;
        }

        // Spawnojam katru produktu
        foreach (var kvp in grouped)
        {
            GameObject obj = Instantiate(itemPrefab, itemContainer);

            var ui = obj.GetComponent<FridgeItemUI>();
            ui.Setup(
                kvp.Key,
                itemDatabase.GetIcon(kvp.Key),
                gameManager,
                this
            );
        }
    }
}