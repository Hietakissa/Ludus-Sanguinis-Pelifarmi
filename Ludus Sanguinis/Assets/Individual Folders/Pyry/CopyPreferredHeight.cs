using HietakissaUtils;
using UnityEngine;
using TMPro;

public class CopyPreferredHeight : MonoBehaviour
{
    [SerializeField] RectTransform transformToCopy;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] bool update;
    [SerializeField] Vector2 offset;
    [SerializeField] bool overrideWidth = true;

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
        if (overrideWidth)
        {
            rectTransform.sizeDelta = text.rectTransform.sizeDelta;
            rectTransform.anchoredPosition = text.rectTransform.anchoredPosition + offset * rectTransform.sizeDelta;
        }
        else
        {
            Vector2 preferredSize = new Vector2(Mathf.Min(startWidth, text.preferredWidth), text.preferredHeight);
            rectTransform.sizeDelta = preferredSize;
        }
    }
}
