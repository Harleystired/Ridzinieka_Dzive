using UnityEngine;

public class Calendar : MonoBehaviour
{   [SerializeField] GameObject calendar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        calendar.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
