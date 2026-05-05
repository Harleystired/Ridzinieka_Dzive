using UnityEngine;

public class Fridge : MonoBehaviour, IClickable2D
{
    [SerializeField] GameObject fridge; //assigns the UI element

    private void Awake() //hides the fridge
    {
        if (fridge != null)
            fridge.SetActive(false);
    }

    public void OnClicked(RaycastHit2D hit) //opens the fridge upon clicking
    {
        if (fridge == null) return;

        // 1) Atskaņo atvēršanas skaņu
        AudioManager.Instance.PlaySFX(AudioManager.Instance.fridge_open);

        bool newState = !fridge.activeSelf;
        fridge.SetActive(newState);

        if (newState)
        {
            // 2) Ieslēdz hum
            AudioManager.Instance.PlayAmbience(AudioManager.Instance.fridge_hum);
            UIModal.Open();
        }
        else
        {
            // 3) Izslēdz hum
            AudioManager.Instance.StopAmbience();
            UIModal.Close();
        }
    }

    public void OpenFridgeFromButton()
    {
        if (fridge == null) return;

        // 1) Atskaņo atvēršanas skaņu
        AudioManager.Instance.PlaySFX(AudioManager.Instance.fridge_open);

        // 2) Ieslēdz hum
        AudioManager.Instance.PlayAmbience(AudioManager.Instance.fridge_hum);

        fridge.SetActive(true);
        UIModal.Open();
    }

    public void CloseFridgeFromButton()
    {
        if (fridge == null) return;

        // Izslēdz hum
        AudioManager.Instance.StopAmbience();

        fridge.SetActive(false);
        UIModal.Close();
    }

    public void Eat()
    {
        // Te varēsi ielikt ēšanas skaņu vēlāk
    }

    public void CloseFridge() //closes the fridge
    {
        if (fridge == null) return;
        if (!fridge.activeSelf) return;

        // Izslēdz hum
        AudioManager.Instance.StopAmbience();

        fridge.SetActive(false);
        UIModal.Close();
    }
}