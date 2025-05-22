using System.Collections;
using UnityEngine;
using UnityEditor;

public class ConvertAllMaterialsToURP : MonoBehaviour
{
    [MenuItem("Tools/Convert All Materials to URP Lit")]
    static void ConvertMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int converted = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat.shader.name == "Standard" || mat.shader.name.Contains("Legacy"))
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                EditorUtility.SetDirty(mat);
                converted++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Converted {converted} materials to URP/Lit.");
    }
}
