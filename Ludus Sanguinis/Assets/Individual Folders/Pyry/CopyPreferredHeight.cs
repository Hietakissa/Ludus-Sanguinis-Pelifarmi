using HietakissaUtils;
using UnityEngine;
using TMPro;

public class CopyPreferredHeight : MonoBehaviour
{
    [SerializeField] RectTransform transformToCopy;
    RectTransform rectTransform;
    [SerializeField] TextMeshProUGUI text;


    void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    void LateUpdate()
    {
        rectTransform.sizeDelta = rectTransform.sizeDelta.SetY(text.preferredHeight);
    }
}
