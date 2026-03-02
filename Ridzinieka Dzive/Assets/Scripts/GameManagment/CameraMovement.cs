using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // moves the camera around the room with the use of buttons, add any extra places and set camera position
    private void Awake()
    {
        transform.position = new Vector3(0, 0, -10); //sets the camera position at the start of game
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

    public void Shop()
    {
        Debug.Log("Shop() clicked on: " + gameObject.name);
        transform.position = new Vector3(-50, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
    }

    // all job locations will be added here
    public void workOffice()
    {
        Debug.Log("Close() clicked on: " + gameObject.name);
        transform.position = new Vector3(-75, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
    }
    
}
