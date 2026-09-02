using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConnectDinosaurAnimations
{
    private const string DinosaurFolder = "Assets/Dinasour/";
    private const string ExcludedFolder = "Assets/Dinasour/Pleziozaver/";

    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Diplodok.unity",
        "Assets/Scenes/Koritozaver.unity",
        "Assets/Scenes/Stegozaver.unity",
    };

    [MenuItem("Tools/Dinosaurs/Connect Embedded Animations")]
    public static void ConnectAll()
    {
        if (HasDirtyOpenScene())
        {
            Debug.LogError("[DinosaurAnimations] Save open scene changes before connecting animations.");
            return;
        }

        var previousSetup = EditorSceneManager.GetSceneManagerSetup();
        var connected = 0;

        try
        {
            foreach (var scenePath in ScenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var sceneConnections = ConnectScene(scene);
                if (sceneConnections > 0)
                    EditorSceneManager.SaveScene(scene);

                connected += sceneConnections;
                Debug.Log($"[DinosaurAnimations] {scenePath}: connected {sceneConnections} model(s).");
            }
        }
        finally
        {
            EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
        }

        Debug.Log($"[DinosaurAnimations] Finished. Connected {connected} land-dinosaur model(s); Pleziozaver was excluded.");
    }

    private static int ConnectScene(Scene scene)
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

        var connected = 0;
        foreach (var instanceRoot in instanceRoots)
        {
            var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
            if (!IsIncludedDinosaurModel(assetPath))
                continue;

            var clip = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(candidate => !candidate.name.StartsWith("__preview__", StringComparison.Ordinal));

            if (clip == null)
            {
                Debug.LogWarning($"[DinosaurAnimations] No embedded animation found for {assetPath}.");
                continue;
            }

            var player = instanceRoot.GetComponent<DinosaurClipPlayer>();
            if (player == null)
                player = instanceRoot.AddComponent<DinosaurClipPlayer>();

            player.AnimationClip = clip;
            EditorUtility.SetDirty(player);

            foreach (var animator in instanceRoot.GetComponentsInChildren<Animator>(true))
            {
                animator.enabled = false;
                EditorUtility.SetDirty(animator);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            connected++;
        }

        return connected;
    }

    private static bool IsIncludedDinosaurModel(string assetPath)
    {
        return assetPath.StartsWith(DinosaurFolder, StringComparison.OrdinalIgnoreCase)
               && !assetPath.StartsWith(ExcludedFolder, StringComparison.OrdinalIgnoreCase)
               && assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase);
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
