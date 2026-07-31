using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Water25D.Samples.Benchmark;

namespace Water25D.Editor
{
    internal static class Water25DBenchmarkGenerator
    {
        private const string BenchmarkScenePath = "Assets/Water25D/Samples/Benchmark/Water25D_Benchmark.unity";

        [MenuItem("GameObject/Water 2.5D/Create Deterministic Benchmark Scene", false, 25)]
        private static void CreateBenchmarkScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8f;
            camera.transform.position = new Vector3(10f, 3f, -20f);
            camera.transform.rotation = Quaternion.identity;
            camera.tag = "MainCamera";

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            var driverObject = new GameObject("Water25D Benchmark");
            driverObject.AddComponent<Water25DBenchmarkDriver>();
            Selection.activeGameObject = driverObject;

            EditorSceneManager.SaveScene(scene, BenchmarkScenePath);
            EditorGUIUtility.PingObject(driverObject);
        }
    }
}
