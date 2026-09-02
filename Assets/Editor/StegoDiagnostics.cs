using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class StegoDiagnostics
{
    public static void Run()
    {
        const string modelPath = "Assets/Dinasour/Diplodok/Model/stego_glavni.fbx";
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(modelPath))
        {
            if (asset is Material material)
                Debug.Log($"FBX material: {material.name}; shader={material.shader.name}; mainTexture={material.mainTexture?.name ?? \"<none>\"}");
        }

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Stegozaver.unity", OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.name.Contains("stego", System.StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var material in renderer.sharedMaterials)
                        Debug.Log($"Scene renderer: {renderer.name}; material={material?.name ?? \"<none>\"}; texture={material?.mainTexture?.name ?? \"<none>\"}");
                }
            }
        }
    }
}
