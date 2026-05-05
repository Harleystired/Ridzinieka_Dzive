using System;
using UnityEngine;
using UnityEngine.UI;

public class PhoneUI : MonoBehaviour
{
    [Header("Root Objects")]
    [SerializeField] private GameObject smallPhoneRoot; // always visible when closed
    [SerializeField] private GameObject bigPhoneRoot;   // visible when opened

    [Header("Big Phone Content")]
    [SerializeField] private GameObject bigPhoneStatsRoot;

    [Header("Buttons")]
    [SerializeField] private Button smallPhoneOpenButton; // click the peeking phone
    [SerializeField] private Button[] bigPhoneCloseButtons; // any button that should close the phone (apps, back, etc.)

    [Header("Optional")]
    [SerializeField] private bool startOpened = false;

    [Header("Auto Close")]
    [SerializeField] private bool autoCloseOnLocationChange = true;
    [SerializeField] private GameManager gameManager;

    [Header("Scenarios")]
    [SerializeField] private ScenarioManager scenarioManager;

    private bool _isOpen;
    public bool IsOpen => _isOpen;

    public event Action Opened;
    public event Action Closed;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (scenarioManager == null)
            scenarioManager = FindFirstObjectByType<ScenarioManager>();

        if (smallPhoneOpenButton != null)
            smallPhoneOpenButton.onClick.AddListener(Open);

        if (bigPhoneCloseButtons != null)
        {
            foreach (var b in bigPhoneCloseButtons)
            {
                if (b == null) continue;
                b.onClick.AddListener(Close);
            }
        }

        SetOpen(startOpened, instant: true);
    }

    private void OnEnable()
    {
        if (autoCloseOnLocationChange && gameManager != null)
            gameManager.OnLocationChanged += HandleLocationChanged;

        if (scenarioManager != null)
            scenarioManager.ScenarioActiveChanged += HandleScenarioActiveChanged;

        RefreshStatsVisibility();
    }

    private void OnDisable()
    {
        if (autoCloseOnLocationChange && gameManager != null)
            gameManager.OnLocationChanged -= HandleLocationChanged;

        if (scenarioManager != null)
            scenarioManager.ScenarioActiveChanged -= HandleScenarioActiveChanged;
    }

    private void OnDestroy()
    {
        if (smallPhoneOpenButton != null)
            smallPhoneOpenButton.onClick.RemoveListener(Open);

        if (bigPhoneCloseButtons != null)
        {
            foreach (var b in bigPhoneCloseButtons)
            {
                if (b == null) continue;
                b.onClick.RemoveListener(Close);
            }
        }
    }

    private void HandleLocationChanged(GameManager.Location newLocation)
    {
        // Close phone when arriving somewhere new (prevents immediate new scenarios popping on the open phone)
        // If you ONLY want to close when arriving at Work/Shop/Home, keep this condition.
        if (_isOpen && newLocation != GameManager.Location.Outside)
            Close();
    }

    private void HandleScenarioActiveChanged(bool active)
    {
        RefreshStatsVisibility();
    }

    public void Open() => SetOpen(true, instant: false);
    public void Close()
    {
        if (scenarioManager != null && scenarioManager.IsScenarioActive)
            return;

        SetOpen(false, instant: false);
    }

    public void Toggle()
    {
        if (_isOpen && scenarioManager != null && scenarioManager.IsScenarioActive)
            return;

        SetOpen(!_isOpen, instant: false);
    }

    private void SetOpen(bool open, bool instant)
    {
        if (_isOpen == open && !instant)
            return;

        _isOpen = open;

        if (smallPhoneRoot != null)
            smallPhoneRoot.SetActive(!open);

        if (bigPhoneRoot != null)
            bigPhoneRoot.SetActive(open);

        RefreshStatsVisibility();

        if (instant)
            return;

        if (open)
        {
            //  Swoosh skaņa, kad telefons tiek izvilkts
            AudioManager.Instance.PlaySFX(AudioManager.Instance.phoneSwoosh); 
            Opened?.Invoke();
        }
        else
        {
            //  Maza “close” skaņa, kad telefons aizveras
            AudioManager.Instance.PlaySFX(AudioManager.Instance.phoneSwoosh);
            Closed?.Invoke();
        }
    }

    private void RefreshStatsVisibility()
    {
        if (bigPhoneStatsRoot == null)
            return;

        bool scenarioActive = scenarioManager != null && scenarioManager.IsScenarioActive;
        bigPhoneStatsRoot.SetActive(_isOpen && !scenarioActive);
    }
}
