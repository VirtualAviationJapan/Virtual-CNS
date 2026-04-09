using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Reflection;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
#endif

namespace VirtualCNS
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ComponentInjectAttribute : Attribute
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private static void AutoSetup(Scene scene)
        {
            var rootGameObjects = scene.GetRootGameObjects();
            var usharpComponents = rootGameObjects
                .SelectMany(o => o.GetComponentsInChildren<Component>())
                .Where(c => c != null)
                .GroupBy(component => component.GetType())
                .SelectMany(group =>
                {
                    var type = group.Key;
                    var fields = type.GetFields().Where(f => f.GetCustomAttribute<ComponentInjectAttribute>() != null).ToArray();
                    return group.SelectMany(component => fields.Select(field => (component, field)));
                });

            foreach (var (component, field) in usharpComponents)
            {
                var isArray = field.FieldType.IsArray;
                var valueType = isArray ? field.FieldType.GetElementType() : field.FieldType;

                if (isArray)
                {
                    var components = rootGameObjects.SelectMany(o => o.GetComponentsInChildren(valueType)).ToArray();
                    var value = Array.CreateInstance(valueType, components.Length);
                    Array.Copy(components, value, components.Length);
                    field.SetValue(component, value);
                }
                else
                {
                    field.SetValue(component, rootGameObjects.SelectMany(o => o.GetComponentsInChildren(valueType)).FirstOrDefault());
                }
            }
        }


        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            EditorApplication.playModeStateChanged += (PlayModeStateChange e) =>
            {
                if (e == PlayModeStateChange.EnteredPlayMode) AutoSetup(SceneManager.GetActiveScene());
            };
        }

        public class BuildCallback : IVRCSDKBuildRequestedCallback
        {
            public int callbackOrder => 10;

            public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
            {
                AutoSetup(SceneManager.GetActiveScene());
                return true;
            }
        }
#endif
    }
}
