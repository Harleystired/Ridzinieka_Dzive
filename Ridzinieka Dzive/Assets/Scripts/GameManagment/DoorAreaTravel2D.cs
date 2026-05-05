using UnityEngine;

public sealed class DoorAreaTravel2D : MonoBehaviour, IClickable2D, IHoverable2D
{
    public enum Destination
    {
        Computer,
        Kitchen
    }

    [Header("Travel")]
    [SerializeField] private Destination destination = Destination.Kitchen;
    [SerializeField] private CameraMovement cameraMovement;

    private void Awake()
    {
        if (cameraMovement == null)
            cameraMovement = FindFirstObjectByType<CameraMovement>();
    }

    public void OnClicked(RaycastHit2D hit)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.door);
        if (cameraMovement == null) return;

        switch (destination)
        {
            case Destination.Kitchen:
                cameraMovement.Kitchen();
                break;

            case Destination.Computer:
                cameraMovement.Computer();
                break;
        }
    }

    // Hover is optional here; keep empty if doors have no special hover behavior.
    public void OnHoverEnter(RaycastHit2D hit) { }
    public void OnHoverExit() { }
}
