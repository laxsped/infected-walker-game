#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class ShaderCollector : MonoBehaviour
{
    [MenuItem("Tools/Generate PSX Shader Collection")]
    static void GenerateCollection()
    {
        ShaderVariantCollection collection = new ShaderVariantCollection();
        string[] guids = AssetDatabase.FindAssets("t:Shader");

        int added = 0;
        int skipped = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);

            if (shader == null || shader.name.Contains("TextMeshPro"))
            {
                skipped++;
                continue;
            }

            try
            {
                var variant = new ShaderVariantCollection.ShaderVariant(shader, PassType.Normal);
                collection.Add(variant);
                added++;
            }
            catch
            {
                Debug.LogWarning($"⛔ Пропущен шейдер: {shader.name}");
                skipped++;
            }
        }

        string savePath = "Assets/AllPSXShaders.shadervariants";
        AssetDatabase.CreateAsset(collection, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"✅ Добавлено шейдеров: {added} / ❌ Пропущено: {skipped}");
    }
}
#endif
