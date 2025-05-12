using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Helper
{
   [DisallowMultipleComponent]
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static readonly ConditionalWeakTable<Type, Lazy<T>> Instances = new();
        private static readonly object PadLock = new();

        private static Lazy<T> LazyInstance =>
            Instances.GetValue(typeof(T),
                _ => new Lazy<T>(CreateInstance, LazyThreadSafetyMode.ExecutionAndPublication));

        /// <summary>
        /// Gets the instance of the singleton.
        /// </summary>
        public static T Instance => LazyInstance.Value;

        /// <summary>
        /// Configuration for the singleton behavior.
        /// </summary>
        protected static SingletonConfig Config { get; set; } = new();

        private static T CreateInstance()
        {
            lock (PadLock)
            {
                var objects = GetExistingInstances().ToArray();

                switch (objects.Length)
                {
                    case 0:
                        return CreateNewInstance();
                    case > 1:
                        HandleMultipleInstances(objects);
                        break;
                }

                var instance = objects[0];
                if (Config.Persist)
                {
                    EnsurePersistence(instance);
                }

                if (Config.Lazy)
                {
                    instance.ExecuteInitializers();
                }

                return instance;
            }
        }

        private static void EnsurePersistence(T instance)
        {
            if (instance != null && instance.gameObject.scene.name != null)
            {
                DontDestroyOnLoad(instance.gameObject);
            }
        }

        private static IEnumerable<T> GetExistingInstances() =>
            Config.FindInactive ? Resources.FindObjectsOfTypeAll<T>() : FindObjectsOfType<T>();

        private static T CreateNewInstance()
        {
            var singleton = new GameObject($"{typeof(T).Name} [Singleton]");
            var instance = singleton.AddComponent<T>();
            Debug.LogWarning(
                $"[Singleton] An instance of '{typeof(T).Name}' is needed in the scene, so '{singleton.name}' was " +
                $"created{(Config.Persist ? " with DontDestroyOnLoad." : ".")}"
            );
            if (Config.Persist)
            {
                EnsurePersistence(instance);
                Debug.Log($"[Singleton] '{typeof(T).Name}' is set to persist between scenes.");
            }

            instance.ExecuteInitializers();
            return instance;
        }

        private static void HandleMultipleInstances(T[] objects)
        {
            Debug.LogWarning($"[Singleton] {objects.Length} instances of '{typeof(T).Name}'!");
            if (!Config.DestroyOthers) return;
            foreach (var extra in objects.Skip(1))
            {
                Debug.LogWarning(
                    $"[Singleton] Deleting extra '{typeof(T).Name}' instance attached to '{extra.name}'"
                );
                Destroy(extra.gameObject);
            }
        }

        /// <summary>
        /// Virtual method that can be overridden in derived classes to perform additional initialization logic.
        /// </summary>
        protected virtual void InitializeSingleton()
        {
        }

        protected virtual void Awake()
        {
            if (Config.Lazy) return;

            if (Instance != this && Config.DestroyOthers)
            {
                Debug.LogWarning($"[Singleton] Deleting extra '{typeof(T).Name}' instance attached to '{name}'");
                Destroy(gameObject);
                return;
            }

            if (Config.Persist) DontDestroyOnLoad(gameObject);
            InitializeSingleton();
        }

        protected virtual void OnDestroy()
        {
            if (IsApplicationQuitting())
            {
                Instances.Remove(typeof(T));
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instances.Clear();
        }

        private static bool IsApplicationQuitting() =>
            Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.OSXEditor;

        /// <summary>
        /// Configures the singleton with the specified options.
        /// </summary>
        /// <param name="configAction">Action to configure the singleton.</param>
        protected static void Configure(Action<SingletonConfig> configAction)
        {
            configAction(Config);
        }

        /// <summary>
        /// Configuration options for the Singleton.
        /// </summary>
        protected class SingletonConfig
        {
            /// <summary>
            /// Gets or sets a value indicating whether the singleton instance persists between scenes.
            /// </summary>
            public bool Persist { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to automatically destroy extra instances of the singleton.
            /// </summary>
            public bool DestroyOthers { get; set; } = true;

            /// <summary>
            /// Gets or sets a value indicating whether to search for inactive instances of the singleton.
            /// </summary>
            public bool FindInactive { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether lazy initialization is enabled.
            /// </summary>
            public bool Lazy { get; set; } = true;
        }
    }

    /// <summary>
    /// Attribute to mark a method for execution when the singleton is initialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class SingletonInitializerAttribute : Attribute
    {
    }

    /// <summary>
    /// Extension methods for the Singleton class.
    /// </summary>
    public static class SingletonExtensions
    {
        /// <summary>
        /// Executes all methods marked with the SingletonInitializer attribute.
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        /// <param name="singleton">The singleton instance.</param>
        public static void ExecuteInitializers<T>(this T singleton) where T : Singleton<T>
        {
            var methods = typeof(T).GetMethods(
                    BindingFlags.Instance
                    | BindingFlags.NonPublic
                    | BindingFlags.Public
                )
                .Where(m => m.GetCustomAttribute<SingletonInitializerAttribute>() != null);

            foreach (var method in methods)
            {
                method.Invoke(singleton, null);
            }
        }
    }
}