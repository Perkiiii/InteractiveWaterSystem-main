using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Water25D.Rendering;

namespace Water25D.Tests
{
    /// <summary>
    /// Owns the unsaved preview scene and every temporary Unity object used by an EditMode
    /// fixture. Creating the root in the active scene and moving it later is not sufficient:
    /// controller OnEnable can run before the move and leave generated state behind when it
    /// throws. The root is therefore tracked immediately and moved before any component is
    /// added.
    /// </summary>
    public abstract class Water25DEditModeFixture
    {
        private readonly List<UnityEngine.Object> _createdObjects = new List<UnityEngine.Object>(8);
        private readonly List<string> _temporaryAssetPaths = new List<string>(4);
        private readonly List<string> _temporaryAssetPrefixes = new List<string>(2);
        private readonly HashSet<string> _preExistingTemporaryAssets = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<WaterReflectionManager> _preExistingReflectionManagers = new List<WaterReflectionManager>(2);
        private Scene _testScene;
        private bool _resourcesDisposed;
        private bool _sceneClosed;

        protected Scene TestScene => _testScene;
        protected Scene ActiveSceneBeforeFixture { get; private set; }
        protected int ActiveSceneRootCountBeforeFixture { get; private set; }
        protected bool ActiveSceneDirtyBeforeFixture { get; private set; }
        protected bool ReflectionManagerPresentBeforeFixture => _preExistingReflectionManagers.Count != 0;

        [SetUp]
        public void SetUpWater25DFixture()
        {
            ActiveSceneBeforeFixture = SceneManager.GetActiveScene();
            ActiveSceneRootCountBeforeFixture = GetRootCount(ActiveSceneBeforeFixture);
            ActiveSceneDirtyBeforeFixture = ActiveSceneBeforeFixture.IsValid() && ActiveSceneBeforeFixture.isDirty;
            CaptureExistingReflectionManagers();
            _testScene = EditorSceneManager.NewPreviewScene();
            _resourcesDisposed = false;
            _sceneClosed = false;
        }

        [TearDown]
        public void TearDownWater25DFixture()
        {
            DisposeFixtureNow();
        }

        protected GameObject CreateGameObject(string objectName)
        {
            Assert.IsTrue(_testScene.IsValid(), "The EditMode fixture preview scene is not valid.");
            var gameObject = Track(new GameObject(objectName));
            SceneManager.MoveGameObjectToScene(gameObject, _testScene);
            return gameObject;
        }

        protected T Track<T>(T createdObject) where T : UnityEngine.Object
        {
            if (createdObject != null)
            {
                _createdObjects.Add(createdObject);
            }

            return createdObject;
        }

        protected string ReserveTemporaryAssetPath(string desiredPath)
        {
            var uniquePath = AssetDatabase.GenerateUniqueAssetPath(desiredPath);
            _temporaryAssetPaths.Add(uniquePath);
            return uniquePath;
        }

        protected void TrackTemporaryAssetPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return;
            }

            _temporaryAssetPrefixes.Add(prefix);
            var assetPaths = AssetDatabase.GetAllAssetPaths();
            for (var i = 0; i < assetPaths.Length; i++)
            {
                if (assetPaths[i].StartsWith(prefix, StringComparison.Ordinal))
                {
                    _preExistingTemporaryAssets.Add(assetPaths[i]);
                }
            }
        }

        /// <summary>
        /// Allows regression tests to exercise teardown before NUnit invokes it. The method is
        /// idempotent so the normal TearDown remains safe after an explicit disposal.
        /// </summary>
        protected void DisposeFixtureNow()
        {
            Exception cleanupException = null;
            try
            {
                DisposeTrackedResources(ref cleanupException);
            }
            finally
            {
                try
                {
                    ClosePreviewScene();
                }
                catch (Exception exception)
                {
                    if (cleanupException == null)
                    {
                        cleanupException = exception;
                    }
                }
            }

            if (cleanupException != null)
            {
                throw cleanupException;
            }
        }

        protected void AssertActiveSceneUnchanged()
        {
            Assert.AreEqual(ActiveSceneBeforeFixture, SceneManager.GetActiveScene());
            Assert.AreEqual(ActiveSceneRootCountBeforeFixture, GetRootCount(ActiveSceneBeforeFixture));
            Assert.AreEqual(
                ActiveSceneDirtyBeforeFixture,
                ActiveSceneBeforeFixture.IsValid() && ActiveSceneBeforeFixture.isDirty);
        }

        private void DisposeTrackedResources(ref Exception firstException)
        {
            if (_resourcesDisposed)
            {
                return;
            }

            _resourcesDisposed = true;
            for (var i = _createdObjects.Count - 1; i >= 0; i--)
            {
                var createdObject = _createdObjects[i];
                if (createdObject == null)
                {
                    continue;
                }

                try
                {
                    UnityEngine.Object.DestroyImmediate(createdObject);
                }
                catch (Exception exception)
                {
                    if (firstException == null)
                    {
                        firstException = exception;
                    }
                }
            }

            CleanupReflectionManagers(ref firstException);
            CleanupTemporaryAssets(ref firstException);
            _createdObjects.Clear();
        }

        private void CleanupReflectionManagers(ref Exception firstException)
        {
            var managers = Resources.FindObjectsOfTypeAll<WaterReflectionManager>();
            for (var i = managers.Length - 1; i >= 0; i--)
            {
                var manager = managers[i];
                if (manager == null || IsPreExistingReflectionManager(manager))
                {
                    continue;
                }

                try
                {
                    UnityEngine.Object.DestroyImmediate(manager.gameObject);
                }
                catch (Exception exception)
                {
                    if (firstException == null)
                    {
                        firstException = exception;
                    }
                }
            }
        }

        private void CleanupTemporaryAssets(ref Exception firstException)
        {
            for (var i = 0; i < _temporaryAssetPaths.Count; i++)
            {
                DeleteTemporaryAsset(_temporaryAssetPaths[i], ref firstException);
            }

            var assetPaths = AssetDatabase.GetAllAssetPaths();
            for (var i = 0; i < _temporaryAssetPrefixes.Count; i++)
            {
                var prefix = _temporaryAssetPrefixes[i];
                for (var pathIndex = 0; pathIndex < assetPaths.Length; pathIndex++)
                {
                    var path = assetPaths[pathIndex];
                    if (path.StartsWith(prefix, StringComparison.Ordinal) &&
                        !_preExistingTemporaryAssets.Contains(path))
                    {
                        DeleteTemporaryAsset(path, ref firstException);
                    }
                }
            }

            _temporaryAssetPaths.Clear();
            _temporaryAssetPrefixes.Clear();
            _preExistingTemporaryAssets.Clear();
        }

        private static void DeleteTemporaryAsset(string path, ref Exception firstException)
        {
            if (string.IsNullOrEmpty(path) || !AssetDatabase.LoadMainAssetAtPath(path))
            {
                return;
            }

            try
            {
                AssetDatabase.DeleteAsset(path);
            }
            catch (Exception exception)
            {
                if (firstException == null)
                {
                    firstException = exception;
                }
            }
        }

        private void ClosePreviewScene()
        {
            if (_sceneClosed || !_testScene.IsValid())
            {
                return;
            }

            _sceneClosed = true;
            EditorSceneManager.ClosePreviewScene(_testScene);
        }

        private void CaptureExistingReflectionManagers()
        {
            _preExistingReflectionManagers.Clear();
            var managers = Resources.FindObjectsOfTypeAll<WaterReflectionManager>();
            for (var i = 0; i < managers.Length; i++)
            {
                if (managers[i] != null && !_preExistingReflectionManagers.Contains(managers[i]))
                {
                    _preExistingReflectionManagers.Add(managers[i]);
                }
            }
        }

        private bool IsPreExistingReflectionManager(WaterReflectionManager manager)
        {
            return _preExistingReflectionManagers.Contains(manager);
        }

        private static int GetRootCount(Scene scene)
        {
            return scene.IsValid() && scene.isLoaded ? scene.rootCount : 0;
        }
    }
}
