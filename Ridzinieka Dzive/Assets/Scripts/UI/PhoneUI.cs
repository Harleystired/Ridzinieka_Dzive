using System;
using UnityEngine;
using UnityEngine.UI;

public class PhoneUI : MonoBehaviour
{
    [Header("Root Objects")]
    [SerializeField] private GameObject smallPhoneRoot; // always visible when closed
    [SerializeField] private GameObject bigPhoneRoot;   // visible when opened

    [Header("Buttons")]
    [SerializeField] private Button smallPhoneOpenButton; // click the peeking phone
    [SerializeField] private Button[] bigPhoneCloseButtons; // any button that should close the phone (apps, back, etc.)

    [Header("Optional")]
    [SerializeField] private bool startOpened = false;

    private bool _isOpen;

    public bool IsOpen => _isOpen;

    public event Action Opened;
    public event Action Closed;

    private void Awake()
    {
        if (smallPhoneOpenButton != null)
            smallPhoneOpenButton.onClick.AddListener(Open);

        if (bigPhoneCloseButtons != null)
        {
            foreach (var b in bigPhoneCloseButtons)
            {
                if (b != null)
                    b.onClick.AddListener(Close);
            }
        }

        SetOpen(startOpened, instant: true);
    }

    private void OnDestroy()
    {
        if (smallPhoneOpenButton != null)
            smallPhoneOpenButton.onClick.RemoveListener(Open);

        if (bigPhoneCloseButtons != null)
        {
            foreach (var b in bigPhoneCloseButtons)
            {
                if (b != null)
                    b.onClick.RemoveListener(Close);
            }
        }
    }

    public void Open() => SetOpen(true, instant: false);
    public void Close() => SetOpen(false, instant: false);
    public void Toggle() => SetOpen(!_isOpen, instant: false);

    private void SetOpen(bool open, bool instant)
    {
        if (_isOpen == open && !instant)
            return;

        _isOpen = open;

        if (smallPhoneRoot != null)
            smallPhoneRoot.SetActive(!open);

        if (bigPhoneRoot != null)
            bigPhoneRoot.SetActive(open);

        if (instant)
            return;

        if (open) Opened?.Invoke();
        else Closed?.Invoke();
    }
}
