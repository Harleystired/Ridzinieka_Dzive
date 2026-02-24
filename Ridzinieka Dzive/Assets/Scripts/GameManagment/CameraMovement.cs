using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private void Awake()
    {
        transform.position = new Vector3(0, 0, -10);
    }

    public void Kitchen()
    {
        Debug.Log("Kitchen() clicked on: " + gameObject.name);
        transform.position = new Vector3(20.22f, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
    }
    public void Computer()
    {
        Debug.Log("Computer() clicked on: " + gameObject.name);
        transform.position = new Vector3(0, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
    }
    
    public void Outside()
    {
        Debug.Log("Outside() clicked on: " + gameObject.name);
        transform.position = new Vector3(-25, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
    }
    
    public void Work1()
    {
        Debug.Log("Close() clicked on: " + gameObject.name);
    }
    
}
