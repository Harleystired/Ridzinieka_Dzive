using UnityEngine;

public class RoomArrowsController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("Arrows to toggle (set active only at Home)")]
    [SerializeField] private GameObject[] arrows;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnLocationChanged += HandleLocationChanged;

        if (gameManager != null)
            HandleLocationChanged(gameManager.CurrentLocation);
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnLocationChanged -= HandleLocationChanged;
    }

    private void HandleLocationChanged(GameManager.Location location)
    {
        bool show = location == GameManager.Location.Home;

        if (arrows == null) return;

        for (int i = 0; i < arrows.Length; i++)
        {
            if (arrows[i] != null)
                arrows[i].SetActive(show);
        }
    }
}
