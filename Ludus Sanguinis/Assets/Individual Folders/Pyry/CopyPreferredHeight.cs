using HietakissaUtils;
using UnityEngine;
using TMPro;

public class CopyPreferredHeight : MonoBehaviour
{
    [SerializeField] RectTransform transformToCopy;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] bool update;

    RectTransform rectTransform;
    float startWidth;

    void Awake()
    {
        rectTransform = (RectTransform)transform;
        //startWidth = rectTransform.sizeDelta.x;
        startWidth = rectTransform.rect.width;
    }

    void LateUpdate()
    {
        SetSize();
    }

    void SetSize()
    {
        Vector2 preferredSize = new Vector2(Mathf.Min(startWidth, text.preferredWidth), text.preferredHeight);
        rectTransform.sizeDelta = preferredSize;
    }
}
