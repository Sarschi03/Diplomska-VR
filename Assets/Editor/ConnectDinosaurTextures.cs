using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConnectDinosaurTextures
{
    private const string MaterialsFolder = "Assets/Dinasour/Diplodok/Materials";
    private const string StegoTexturePath = "Assets/Dinasour/Diplodok/Textures/782d4db6-21df-4c70-9f67-602a4a7410e0.png";
    private const string KoryTexturePath = "Assets/Dinasour/Diplodok/Textures/ChatGPT Image Aug 17, 2026, 01_02_22 PM.png";

    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Koritozaver.unity",
        "Assets/Scenes/Stegozaver.unity",
    };

    [MenuItem("Tools/Dinosaurs/Connect Stegozaver and Koritozaver Textures")]
    public static void ConnectAll()
    {
        if (HasDirtyOpenScene())
        {
            Debug.LogError("[DinosaurTextures] Save open scene changes before connecting textures.");
            return;
        }

        var stegoMaterial = CreateOrUpdateMaterial("Stegozaver", StegoTexturePath);
        var koryMaterial = CreateOrUpdateMaterial("Koritozaver", KoryTexturePath);
        if (stegoMaterial == null || koryMaterial == null)
            return;

        var previousSetup = EditorSceneManager.GetSceneManagerSetup();
        var textured = 0;

        try
        {
            foreach (var scenePath in ScenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var sceneCount = ApplySceneTextures(scene, stegoMaterial, koryMaterial);
                if (sceneCount > 0)
                    EditorSceneManager.SaveScene(scene);

                textured += sceneCount;
                Debug.Log($"[DinosaurTextures] {scenePath}: textured {sceneCount} model(s).");
            }
        }
        finally
        {
            EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[DinosaurTextures] Finished. Textured {textured} Stegozaver/Koritozaver model(s).");
    }

    private static Material CreateOrUpdateMaterial(string name, string texturePath)
    {
        EnsureMaterialsFolder();

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (texture == null || shader == null)
        {
            Debug.LogError($"[DinosaurTextures] Could not load texture or URP Lit shader for {name}.");
            return null;
        }

        var materialPath = $"{MaterialsFolder}/{name}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, materialPath);
        }

        material.shader = shader;
        material.SetTexture("_BaseMap", texture);
        material.SetTexture("_MainTex", texture);
        material.SetColor("_BaseColor", Color.white);
        material.SetColor("_Color", Color.white);
        material.SetFloat("_Smoothness", 0.25f);
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static int ApplySceneTextures(Scene scene, Material stegoMaterial, Material koryMaterial)
    {
        var instanceRoots = new HashSet<GameObject>();
        foreach (var sceneRoot in scene.GetRootGameObjects())
        {
            foreach (var transform in sceneRoot.GetComponentsInChildren<Transform>(true))
            {
                var instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(transform.gameObject);
                if (instanceRoot != null)
                    instanceRoots.Add(instanceRoot);
            }
        }

        var textured = 0;
        foreach (var instanceRoot in instanceRoots)
        {
            var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
            var material = SelectMaterial(assetPath, stegoMaterial, koryMaterial);
            if (material == null)
                continue;

            foreach (var renderer in instanceRoot.GetComponentsInChildren<Renderer>(true))
            {
                var slotCount = Math.Max(1, renderer.sharedMaterials.Length);
                renderer.sharedMaterials = Enumerable.Repeat(material, slotCount).ToArray();
                EditorUtility.SetDirty(renderer);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            textured++;
        }

        return textured;
    }

    private static Material SelectMaterial(string assetPath, Material stegoMaterial, Material koryMaterial)
    {
        var fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        if (fileName.StartsWith("stego_glavni", StringComparison.OrdinalIgnoreCase))
            return stegoMaterial;
        if (fileName.StartsWith("koryozadje", StringComparison.OrdinalIgnoreCase))
            return koryMaterial;
        return null;
    }

    private static void EnsureMaterialsFolder()
    {
        if (!AssetDatabase.IsValidFolder(MaterialsFolder))
            AssetDatabase.CreateFolder("Assets/Dinasour/Diplodok", "Materials");
    }

    private static bool HasDirtyOpenScene()
    {
        for (var index = 0; index < SceneManager.sceneCount; index++)
        {
            if (SceneManager.GetSceneAt(index).isDirty)
                return true;
        }

        return false;
    }
}
