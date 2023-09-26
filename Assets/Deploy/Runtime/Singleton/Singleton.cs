#nullable enable

using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace Causeless3t.Core
{
	public class Singleton<T> where T : class
	{
		private static Lazy<T>? _lazyInstance;

#region Access
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
#endregion Access

#region Generation
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
		
		public static T GenerateInstance()
		{
			var publicConstructors = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
			if (publicConstructors.Length == 0)
			{
				throw new MissingMethodException($"{typeof(T).Name} doesn't have a public constructor.");
			}
			if (publicConstructors[0].Invoke(null!) is not T instance)
			{
				throw new MissingMethodException($"{typeof(T).Name} has a public constructor that does not return an instance of {typeof(T).Name}.");
			}

			Debug.Log($"<color=red>+ Instance created:</color> <{typeof(T).Name}>");
			return instance;
		}

		private static Lazy<T> GenerateLazyInstance()
		{
			var lazyInstance = new Lazy<T>(GenerateInstance, LazyThreadSafetyMode.PublicationOnly);
			RegisterApplicationQuittingCallback();

			return lazyInstance;
		}
#endregion Generation

#region Destruction
		private static void RegisterApplicationQuittingCallback()
		{
			Action onApplicationQuitting = OnApplicationQuitting;
			Application.quitting -= onApplicationQuitting;
			Application.quitting += onApplicationQuitting;
		}

		private static void OnApplicationQuitting()
		{
			Application.quitting -= OnApplicationQuitting;
			DestroySingletonInstance();
		}

		public static void DestroySingletonInstance()
		{
			var instance = InstanceOrNull;

			(instance as IDisposable)?.Dispose();
			ResetInstanceReferences();

			if (instance != null)
			{
				Debug.Log($"<color=blue>- Instance destroyed:</color> <{typeof(T).Name}>");
			}
		}

		private static void ResetInstanceReferences()
		{
			_lazyInstance = null;
		}
#endregion Destruction
	}
}
