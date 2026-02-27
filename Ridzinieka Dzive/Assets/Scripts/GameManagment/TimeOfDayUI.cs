using UnityEngine;
using UnityEngine.UI;

public class TimeOfDayUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Image timeIcon;
    [SerializeField] private Sprite morningSprite;
    [SerializeField] private Sprite daySprite;
    [SerializeField] private Sprite eveningSprite;
    [SerializeField] private Sprite nightSprite;

    private void Start()
    {
        UpdateIcon(gameManager.CurrentTime);
        gameManager.OnTimeOfDayChanged += UpdateIcon;
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
    }
}