using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            Type type = typeof(T);
            if (!services.ContainsKey(type))
            {
                services.Add(type, service);
            }
            else
            {
                Debug.LogWarning($"ServiceLocator: Service of type {type.Name} is already registered. Overwriting.");
                services[type] = service;
            }
        }

        public static void Unregister<T>()
        {
            Type type = typeof(T);
            if (services.ContainsKey(type))
            {
                services.Remove(type);
            }
        }

        public static T Get<T>()
        {
            Type type = typeof(T);
            if (services.TryGetValue(type, out object service))
            {
                return (T)service;
            }

            Debug.LogWarning($"ServiceLocator: Requested service of type {type.Name} is not registered.");
            return default;
        }
    }
}