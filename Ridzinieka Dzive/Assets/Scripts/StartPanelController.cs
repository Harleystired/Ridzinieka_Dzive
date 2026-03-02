using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartPanelController : MonoBehaviour
{
    // Text objects (exact names from your hierarchy)
    public TextMeshProUGUI Gamename;
    public TextMeshProUGUI starttext1;
    public TextMeshProUGUI starttext2;
    public TextMeshProUGUI starttext3;
    public TextMeshProUGUI ortext;

    // Buttons (exact names from your hierarchy)
    public Button Findajob;
    public Button ExitButton;

    public float delay = 2.5f;

    private void Start()
    {
        // Hide all texts
        Gamename.gameObject.SetActive(false);
        starttext1.gameObject.SetActive(false);
        starttext2.gameObject.SetActive(false);
        starttext3.gameObject.SetActive(false);
        ortext.gameObject.SetActive(false);

        // Hide buttons
        Findajob.gameObject.SetActive(false);
        ExitButton.gameObject.SetActive(false);

        StartCoroutine(ShowIntroSequence());
    }

    IEnumerator ShowIntroSequence()
    {
        Gamename.gameObject.SetActive(true);
        yield return new WaitForSeconds(delay);

        starttext1.gameObject.SetActive(true);
        yield return new WaitForSeconds(delay);

        starttext2.gameObject.SetActive(true);
        yield return new WaitForSeconds(delay);

        starttext3.gameObject.SetActive(true);
        yield return new WaitForSeconds(delay);

        ortext.gameObject.SetActive(true);
        Findajob.gameObject.SetActive(true);
        ExitButton.gameObject.SetActive(true);
    }

    public void OnFindJobPressed()
    {
        UIManager.Instance.Show("LocationPanel");
    }

    public void OnExitPressed()
    {
        Application.Quit();
    }
}