using UnityEngine;

public class OfficeEventTrigger : MonoBehaviour
{
    [SerializeField] private GameObject workEventPanel;

    private void OnEnable()
    {
        workEventPanel.SetActive(false);
        StartCoroutine(ShowEvent());
    }

    private System.Collections.IEnumerator ShowEvent()
    {
        yield return new WaitForSeconds(5f);
        workEventPanel.SetActive(true);
    }
}