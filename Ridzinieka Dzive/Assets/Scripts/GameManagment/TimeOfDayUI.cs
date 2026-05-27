using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TimeOfDayUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Image timeIcon;
    [SerializeField] private Sprite morningSprite;
    [SerializeField] private Sprite daySprite;
    [SerializeField] private Sprite eveningSprite;
    [SerializeField] private Sprite nightSprite;
    [SerializeField] private TMP_Text dayText;
    
    // Date configuration
    private readonly DateTime _startDate = new DateTime(2026, 3, 31); // March 31, 2026

    private void Start()
    {
        UpdateIcon(gameManager.CurrentTime);
        gameManager.OnTimeOfDayChanged += UpdateIcon;
        gameManager.OnDayChanged += UpdateDayText;
        UpdateDayText(gameManager.CurrentDayIndex);
        
    }

    private void UpdateDayText(int dayIndex)
    {
        // Calculate the actual date based on day index
        DateTime currentDate = _startDate.AddDays(dayIndex);
        
        // Format the date as "dd/MM/yyyy" (e.g., "31/03/2026")
        // You can change the format if needed
        string formattedDate = currentDate.ToString("dd/MM/yyyy");
        
        // Alternative formats you might prefer:
        // "dd MMM yyyy" -> "31 Mar 2026"
        // "MMMM dd, yyyy" -> "March 31, 2026"
        // "dd/MM" -> "31/03" (just day/month, no year)
        
        dayText.text = formattedDate;
    }

    private void UpdateIcon(GameManager.TimeOfDay time)
    {
        switch (time)
        {
            case GameManager.TimeOfDay.Morning:
                timeIcon.sprite = morningSprite;
                break;
            case GameManager.TimeOfDay.Day:
                timeIcon.sprite = daySprite;
                break;
            case GameManager.TimeOfDay.Evening:
                timeIcon.sprite = eveningSprite;
                break;
            case GameManager.TimeOfDay.Night:
                timeIcon.sprite = nightSprite;
                break;
        }
    }

    private void OnDestroy()
    {
        gameManager.OnTimeOfDayChanged -= UpdateIcon;
        gameManager.OnDayChanged -= UpdateDayText;
    }
}