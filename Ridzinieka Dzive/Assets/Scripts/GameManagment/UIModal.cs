using System;
using UnityEngine;

public static class UIModal
{
    private static int _openCount;

    public static bool IsAnyOpen => _openCount > 0;

    public static event Action<bool> OnModalStateChanged;

    public static void Open()
    {
        _openCount++;
        if (_openCount == 1)
            OnModalStateChanged?.Invoke(true);
    }

    public static void Close()
    {
        _openCount = Mathf.Max(0, _openCount - 1);
        if (_openCount == 0)
            OnModalStateChanged?.Invoke(false);
    }

    public static void ResetAll()
    {
        _openCount = 0;
        OnModalStateChanged?.Invoke(false);
    }
}
