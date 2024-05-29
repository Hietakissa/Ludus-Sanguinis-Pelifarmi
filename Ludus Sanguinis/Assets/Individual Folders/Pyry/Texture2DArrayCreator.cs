using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Texture2DArrayCreator : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] Texture2D[] textures;

    [SerializeField] string path;
    [SerializeField] string filename;

    [ContextMenu("Create 2D Array")]
    void CompileArray()
    {
        if (textures == null || textures.Length == 0 || string.IsNullOrEmpty(path) || string.IsNullOrEmpty(filename))
        {
            Debug.LogError("Invalid settings for Texture2D[] creation!");
            return;
        }

        string uri = System.IO.Path.Combine(path, filename) + ".asset";

        Texture2D sample = textures[0];
        Texture2DArray textureArray = new Texture2DArray(sample.width, sample.height, textures.Length, sample.format, false);
        textureArray.filterMode = sample.filterMode;
        textureArray.wrapMode = sample.wrapMode;

        for (int i = 0; i < textures.Length; i++)
        {
            Texture2D tex = textures[i];
            textureArray.SetPixels(tex.GetPixels(0), i, 0);
        }
        textureArray.Apply();


        AssetDatabase.CreateAsset(textureArray, uri);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Successfully created asset '{uri}'");
    }
#endif
}
