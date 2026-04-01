using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public enum HomeArea
    {
        Computer = 0,
        Kitchen = 1,
        Other = 2
    }

    public event Action<HomeArea> OnHomeAreaChanged;

    private HomeArea _currentArea = HomeArea.Computer;

    private void Awake()
    {
        transform.position = new Vector3(0, 0, -10);
        SetArea(HomeArea.Computer);
    }

    private void SetArea(HomeArea area)
    {
        if (_currentArea == area) return;
        _currentArea = area;
        OnHomeAreaChanged?.Invoke(_currentArea);
    }

    public void Kitchen()
    {
        Debug.Log("Kitchen() clicked on: " + gameObject.name);
        transform.position = new Vector3(20.22f, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
        SetArea(HomeArea.Kitchen);
    }

    public void Computer()
    {
        Debug.Log("Computer() clicked on: " + gameObject.name);
        transform.position = new Vector3(0, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
        SetArea(HomeArea.Computer);
    }

    public void Outside()
    {
        Debug.Log("Outside() clicked on: " + gameObject.name);
        transform.position = new Vector3(-25, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
        SetArea(HomeArea.Other);
    }

    public void Shop()
    {
        Debug.Log("Shop() clicked on: " + gameObject.name);
        transform.position = new Vector3(-50, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
        SetArea(HomeArea.Other);
    }

    public void workOffice()
    {
        Debug.Log("Close() clicked on: " + gameObject.name);
        transform.position = new Vector3(-75, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
        SetArea(HomeArea.Other);
    }

    public void workCashier()
    {
        Debug.Log("Close() clicked on: " + gameObject.name);
        transform.position = new Vector3(-100, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
        SetArea(HomeArea.Other);
    }

    public void workTaxi()
    {
        Debug.Log("Close() clicked on: " + gameObject.name);
        transform.position = new Vector3(-130, 0, -10);
        Debug.Log("Camera position now: " + transform.position);
        SetArea(HomeArea.Other);
    }
}
