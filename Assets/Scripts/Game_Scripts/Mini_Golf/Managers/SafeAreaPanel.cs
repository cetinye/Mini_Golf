using UnityEngine;

public class SafeAreaPanel : MonoBehaviour
{
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        RefreshPanel(Screen.safeArea);
    }

    void OnEnable()
    {
        SafeAreaDetection.OnSafeAreaChanged += RefreshPanel;
    }

    void OnDisable()
    {
        SafeAreaDetection.OnSafeAreaChanged -= RefreshPanel;
    }

    private void RefreshPanel(Rect safeArea)
    {
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}
