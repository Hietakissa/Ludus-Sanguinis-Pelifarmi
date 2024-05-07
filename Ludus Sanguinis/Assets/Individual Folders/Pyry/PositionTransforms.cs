using HietakissaUtils;
using UnityEngine;

public class PositionTransforms : MonoBehaviour
{
    [SerializeField] Transform[] transforms;

    [SerializeField] AnimationCurve localYOffsetCurve;
    [SerializeField] float yOffset;
    [SerializeField] float xOffset;
    [SerializeField] float zOffset = -0.01f;
    [SerializeField] float zRotOffset = 10f;

    void OnValidate()
    {
        Debug.Log($"validate, null: {transforms == null}, length: {(transforms != null ? transforms.Length : -1)}");
        if (transforms == null || transforms.Length == 0) return;

        int count = transforms.Length;
        float xStart = -((count - 1) * xOffset * 0.5f); // 5, 10 => 4 * 10 * 0.5 => 20
        float zRotStart = -((count - 1) * zRotOffset * 0.5f);

        for (int i = 0; i < count; i++)
        {
            Transform t = transforms[i];
            Vector3 localPos = t.localPosition;
            localPos = localPos.SetX(xStart + i * xOffset);
            localPos = localPos.SetY(localYOffsetCurve.Evaluate(i / (float)(count - 1)) * yOffset);
            localPos = localPos.SetZ((i + 1) * zOffset);

            t.localPosition = localPos;

            t.localEulerAngles = new Vector3(0f, 180f, zRotStart + i * zRotOffset);
        }
    }
}
