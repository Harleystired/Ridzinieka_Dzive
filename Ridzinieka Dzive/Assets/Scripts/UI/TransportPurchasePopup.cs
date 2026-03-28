using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TransportPurchasePopup : MonoBehaviour
{
    public enum PopupType { Bike, Car }

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private OutsideUI outsideUI;

    [Header("UI")]
    [SerializeField] private GameObject root; // panel object
    [SerializeField] private Button leftButton;   // "Buy Used ..."
    [SerializeField] private Button rightButton;  // "Buy New ..." or "Buy Other"
    [SerializeField] private Button useButton;    // "Use ..."
    [SerializeField] private Button closeButton;

    [Header("Optional Labels (can be null)")]
    [SerializeField] private TMP_Text leftLabel;
    [SerializeField] private TMP_Text rightLabel;
    [SerializeField] private TMP_Text useLabel;

    private PopupType _type;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public void OpenBike() => Open(PopupType.Bike);
    public void OpenCar() => Open(PopupType.Car);

    public void Open(PopupType type)
    {
        _type = type;

        if (root != null)
            root.SetActive(true);

        UIModal.Open();
        Refresh();
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);

        UIModal.Close();
    }

    private void Refresh()
    {
        if (gameManager == null) return;

        // Clear old listeners safely
        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();
        useButton.onClick.RemoveAllListeners();

        if (_type == PopupType.Bike)
        {
            bool hasOld = gameManager.oldBike;
            bool hasNew = gameManager.newBike;

            if (!hasOld && !hasNew)
            {
                SetButton(leftButton, leftLabel, $"Pērc vecu riteni (${gameManager.OldBikePrice})", true,
                    () => { if (gameManager.TryBuyOldBike()) Refresh(); });

                SetButton(rightButton, rightLabel, $"Pērc jaunu riteni (${gameManager.NewBikePrice})", true,
                    () => { if (gameManager.TryBuyNewBike()) Refresh(); });

                SetButton(useButton, useLabel, "Izmanto Riteni", false, null);
                return;
            }

            // Has at least one bike: allow using owned + buying the other
            if (hasOld)
            {
                SetButton(useButton, useLabel, "Izmanto veco riteni", true, () => UseTransport(GameManager.TransportMode.OldBike));
                SetButton(leftButton, leftLabel, "Izmanto jauno riteni", false, null);
            }
            else
            {
                SetButton(leftButton, leftLabel, $"Pērc vecu riteni (${gameManager.OldBikePrice})", true,
                    () => { if (gameManager.TryBuyOldBike()) Refresh(); });
            }

            if (hasNew)
            {
                SetButton(rightButton, rightLabel, "Tev ir jauns ritenis", false, null);
                if (!hasOld) SetButton(useButton, useLabel, "Tev ir vecs ritenis", true, () => UseTransport(GameManager.TransportMode.NewBike));
            }
            else
            {
                SetButton(rightButton, rightLabel, $"Pērc jaunu riteni (${gameManager.NewBikePrice})", true,
                    () => { if (gameManager.TryBuyNewBike()) Refresh(); });

                if (!hasOld) SetButton(useButton, useLabel, "Use New Bike", true, () => UseTransport(GameManager.TransportMode.NewBike));
            }

            // If both owned, prefer New Bike as the default "Use"
            if (hasOld && hasNew)
                SetButton(useButton, useLabel, "Izmanto Riteni", true, () => UseTransport(GameManager.TransportMode.NewBike));

            return;
        }

        // Car popup
        {
            bool hasOld = gameManager.oldCar;
            bool hasNew = gameManager.newCar;

            if (!hasOld && !hasNew)
            {
                SetButton(leftButton, leftLabel, $"Pērc vecu mašīnu (${gameManager.OldCarPrice})", true,
                    () => { if (gameManager.TryBuyOldCar()) Refresh(); });

                SetButton(rightButton, rightLabel, $"Pērc jaunu mašīnu (${gameManager.NewCarPrice})", true,
                    () => { if (gameManager.TryBuyNewCar()) Refresh(); });

                SetButton(useButton, useLabel, "Izmanto mašīnu", false, null);
                return;
            }

            if (hasOld)
            {
                SetButton(useButton, useLabel, "Izmanto veco mašīnu", true, () => UseTransport(GameManager.TransportMode.OldCar));
                SetButton(leftButton, leftLabel, "Tev ir veca mašīna", false, null);
            }
            else
            {
                SetButton(leftButton, leftLabel, $"Pērc vecu mašīnu (${gameManager.OldCarPrice})", true,
                    () => { if (gameManager.TryBuyOldCar()) Refresh(); });
            }

            if (hasNew)
            {
                SetButton(rightButton, rightLabel, "Tev ir jauna mašīna", false, null);
                if (!hasOld) SetButton(useButton, useLabel, "Izmanto jauno mašīnu", true, () => UseTransport(GameManager.TransportMode.NewCar));
            }
            else
            {
                SetButton(rightButton, rightLabel, $"Pērc jaunu mašīnu (${gameManager.NewCarPrice})", true,
                    () => { if (gameManager.TryBuyNewCar()) Refresh(); });

                if (!hasOld) SetButton(useButton, useLabel, "Use New Car", true, () => UseTransport(GameManager.TransportMode.NewCar));
            }

            if (hasOld && hasNew)
                SetButton(useButton, useLabel, "Izmanto mašīnu", true, () => UseTransport(GameManager.TransportMode.NewCar));
        }
    }

    private void UseTransport(GameManager.TransportMode mode)
    {
        if (gameManager == null) return;

        gameManager.ConfirmTravel(mode);

        Close();

        // Close outside menu too, so player continues to destination cleanly
        if (outsideUI != null)
            outsideUI.CloseOutsideMenu();
    }

    private static void SetButton(Button button, TMP_Text label, string text, bool interactable, UnityEngine.Events.UnityAction onClick)
    {
        button.interactable = interactable;

        if (label != null)
            label.text = text;

        button.onClick.RemoveAllListeners();

        if (interactable && onClick != null)
            button.onClick.AddListener(onClick);
    }
}
