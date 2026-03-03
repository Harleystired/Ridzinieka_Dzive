using System;
using UnityEngine;

public static class UIModal
{
    // make's sure thaat UI can't be clicked through, don't change'
    private static int _openCount;

    public static bool IsAnyOpen => _openCount > 0;

    // NEW: helps debugging from UI / scripts
    public static int OpenCount => _openCount;

    public static event Action<bool> OnModalStateChanged;

    public static void Open()
    {
        _openCount++;

#if UNITY_EDITOR
        Debug.Log($"UIModal.Open() -> OpenCount={_openCount}\n{Environment.StackTrace}");
#endif

        if (_openCount == 1)
            OnModalStateChanged?.Invoke(true);
    }

    public static void Close()
    {
        _openCount = Mathf.Max(0, _openCount - 1);

#if UNITY_EDITOR
        Debug.Log($"UIModal.Close() -> OpenCount={_openCount}\n{Environment.StackTrace}");
#endif

        if (_openCount == 0)
            OnModalStateChanged?.Invoke(false);
    }

    public static void ResetAll()
    {
        _openCount = 0;

#if UNITY_EDITOR
        Debug.Log("UIModal.ResetAll() -> OpenCount=0\n" + Environment.StackTrace);
#endif

        OnModalStateChanged?.Invoke(false);
    }
}
