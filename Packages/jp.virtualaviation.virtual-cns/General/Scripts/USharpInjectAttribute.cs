using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UdonSharp;
using UnityEngine;
using System.Reflection;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
using UdonSharpEditor;
using VRC.SDKBase.Editor.BuildPipeline;
#endif

namespace VirtualCNS
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class USharpInjectAttribute : Attribute
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private static void AutoSetup(Scene scene)
        {
            var rootGameObjects = scene.GetRootGameObjects();
            var usharpComponents = rootGameObjects
                .SelectMany(o => o.GetComponentsInChildren<UdonSharpBehaviour>())
                .GroupBy(udon => udon.GetType())
                .SelectMany(group =>
                {
                    var type = group.Key;
                    var fields = type.GetFields().Where(f => f.GetCustomAttribute<USharpInjectAttribute>() != null).ToArray();
                    return group.SelectMany(udon => fields.Select(field => (udon, field)));
                });

            foreach (var (udon, field) in usharpComponents)
            {
                var isArray = field.FieldType.IsArray;
                var valueType = isArray ? field.FieldType.GetElementType() : field.FieldType;

                if (isArray)
                {
                    var components = rootGameObjects.SelectMany(o => o.GetComponentsInChildren(valueType)).ToArray();
                    var value = Array.CreateInstance(valueType, components.Length);
                    Array.Copy(components, value, components.Length);
                    field.SetValue(udon, value);
                }
                else
                {
                    field.SetValue(udon, rootGameObjects.SelectMany(o => o.GetComponentsInChildren(valueType)).FirstOrDefault());
                }
            }
        }

        [InitializeOnEnterPlayMode]
        public static void OnEnterPlayMode()
        {
            AutoSetup(SceneManager.GetActiveScene());
        }

        public class BuildCallback : Editor, IVRCSDKBuildRequestedCallback
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
