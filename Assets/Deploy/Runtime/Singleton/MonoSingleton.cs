#nullable enable

using System;
using System.Threading;
using UnityEngine;

namespace Causeless3t.Core
{
    public sealed class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static Lazy<T>? _lazyInstance;

        private static (T, GameObject?) _instanceGameObject;

        private static string GameObjectName => $"[{typeof(T).BaseType.Name}] ({typeof(T).Name})";
        
        private static string GetGameObjectPath(GameObject gameObject) => $"\"{gameObject.name}\" in scene \"{gameObject.scene.name}\"";

#region InstanceAccess
        public static bool InstanceExists => _lazyInstance?.IsValueCreated ?? false;

        public static T? InstanceOrNull => InstanceExists ? _lazyInstance?.Value : null;

        public static T Instance => GetOrCreateInstance();

        public static bool TryCreateInstance()
        {
            if (InstanceExists)
            {
                return false;
            }

            GetOrCreateInstance();
            return true;
        }
#endregion // InstanceAccess

#region InstanceGeneration
        private static T GetOrCreateInstance()
        {
            _lazyInstance ??= GenerateLazyInstance();

            var instance = _lazyInstance.Value;
            if (instance == null)
            {
                ResetInstanceReferences();
                throw new NullReferenceException($"{typeof(T).Name} instance is null.");
            }

            return instance;
        }

        private static Lazy<T> GenerateLazyInstance()
        {
            return new Lazy<T>(GenerateInstance, LazyThreadSafetyMode.PublicationOnly);
        }

        private static T GenerateInstance()
        {
            var instances = FindObjectsOfType<T>(true);
            if (instances == null || instances.Length == 0)
            {
                return CreateNewInstance();
            }

            if (instances.Length == 1)
            {
                return GetExistingInstance(instances[0]);
            }

            ResetInstanceReferences();
            throw new InvalidOperationException($"{typeof(T).Name} cannot be created because there are already {instances.Length} instances exists.");
        }

        private static T CreateNewInstance()
        {
            var gameObject = new GameObject(GameObjectName)
            {
                hideFlags = HideFlags.DontSave,
            };

            _instanceGameObject.Item2 = gameObject;

            var instance = gameObject.AddComponent<T>();
            if (instance == null)
            {
                ResetInstanceReferences();
                DestroyTarget(gameObject);
                throw new MissingComponentException($"{typeof(T).Name} failed to add component to gameObject.");
            }

            SetDontDestroyOnLoad(gameObject);

            Debug.Log($"<color=red>+ Instance created:</color> <{typeof(T).Name}>");
            return instance;
        }

        private static T GetExistingInstance(T instance)
        {
            var gameObject = instance.gameObject;

            _instanceGameObject.Item2 = gameObject;

            SetDontDestroyOnLoad(gameObject);

            Debug.Log($"<{typeof(T).Name}> instance assigned to existing {GetGameObjectPath(gameObject)}.");
            return instance;
        }
#endregion // InstanceGeneration

#region InstanceDestruction
        public static void DestroySingletonInstance()
        {
            var instance = InstanceOrNull;

            // ReSharper disable once SuspiciousTypeConversion.Global
            (instance as IDisposable)?.Dispose();
            ResetInstanceReferences();

            if (instance != null)
            {
                DestroyTarget(instance.gameObject);
                Debug.Log($"<color=blue>- Instance destroyed:</color> <{typeof(T).Name}>");
            }
        }

        private void DestroyDuplicatedInstance()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            (this as IDisposable)?.Dispose();
            DestroyTarget(this);
            Debug.LogWarning($"<{typeof(T).Name}> instance already exists. Destroying duplicated instance attached to gameObject.");
        }

        private static void ResetInstanceReferences()
        {
            _lazyInstance = null;
            _instanceGameObject.Item2 = null;
        }
#endregion // InstanceDestruction

#region MonoBehaviour
        private bool _isApplicationQuitting;

        private void Awake()
        {
            if (_instanceGameObject.Item2 == null)
            {
                TryCreateInstance();
            }
            else
            {
                DestroyDuplicatedInstance();
            }

            AwakeInvoked();
        }

        private void OnApplicationQuit()
        {
            _isApplicationQuitting = true;

            OnApplicationQuitInvoked();

            DestroySingletonInstance();
        }

        private void OnDestroy()
        {
            OnDestroyInvoked();

            DestroySingletonInstance();

            if (!_isApplicationQuitting)
            {
                Debug.LogError($"<{typeof(T).Name}> instance should not be destroyed manually. Singleton integrity is compromised.");
            }
        }

        private void AwakeInvoked()             { }
        private void OnApplicationQuitInvoked() { }
        private void OnDestroyInvoked()         { }
#endregion MonoBehaviour

#region Utils
        private static void DestroyTarget(UnityEngine.Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private static void SetDontDestroyOnLoad(UnityEngine.Object target)
        {
            if (Application.isPlaying)
                DontDestroyOnLoad(target);
        }
#endregion Utils
    }
}
