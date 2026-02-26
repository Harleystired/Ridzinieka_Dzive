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

        bool newState = !fridge.activeSelf;
        fridge.SetActive(newState);

        if (newState) UIModal.Open(); // makes it so other object can't be clicked through UI
        else UIModal.Close();
    }

    public void Eat()
    {
    }

    public void CloseFridge() //closes the fridge
    {
        if (fridge == null) return;
        if (!fridge.activeSelf) return;

        fridge.SetActive(false);

        UIModal.Close(); // allows other objects to be clicked, if this is not done, NOTHING will be clickable
    }
}
