using UnityEngine;

public class SafeAreaDetection : MonoBehaviour
{
    public delegate void SafeAreaChanged(Rect safeArea);
    public static event SafeAreaChanged OnSafeAreaChanged;
    private Rect safeAreaRect;

    void Awake()
    {
        safeAreaRect = Screen.safeArea;
    }

    void Update()
    {
        if (safeAreaRect != Screen.safeArea)
        {
            safeAreaRect = Screen.safeArea;
            OnSafeAreaChanged?.Invoke(safeAreaRect);
        }
    }
}
