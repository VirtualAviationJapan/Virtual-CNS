using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UdonSharp;
using UnityEngine;
using System.Reflection;
using UdonToolkit;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
using UdonSharpEditor;
using VRC.SDKBase.Editor.BuildPipeline;
#endif

namespace VirtualAviationJapan
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    public class USharpInjectAttribute : UTPropertyAttribute
#else
    public class USharpInjectAttribute : Attribute
#endif
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public override void BeforeGUI(SerializedProperty property)
        {
            EditorGUI.BeginDisabledGroup(true);
        }
        public override void AfterGUI(SerializedProperty property)
        {
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.HelpBox("Auto injected by script.", MessageType.Info);
        }

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
                var isUdonSharpBehaviour = valueType.IsSubclassOf(typeof(UdonSharpBehaviour));
                var variableName = field.Name;

                if (isArray)
                {
                    var components = isUdonSharpBehaviour
                        ? rootGameObjects.SelectMany(o => o.GetComponentsInChildren(valueType)).ToArray()
                        : rootGameObjects.SelectMany(o => o.GetComponentsInChildren(valueType)).ToArray();
                    var value = field.FieldType.GetConstructor(new[] { typeof(int) }).Invoke(new object[] { components.Length });
                    Array.Copy(components, value as Array, components.Length);
                    field.SetValue(udon, value);
                }
                else
                {
                    if (isUdonSharpBehaviour) field.SetValue(udon, rootGameObjects.SelectMany(o => o.GetComponentsInChildren(valueType)).FirstOrDefault());
                    else field.SetValue(udon, rootGameObjects.SelectMany(o => o.GetComponentsInChildren(valueType)).FirstOrDefault());
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
