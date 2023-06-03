using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VirtualCNS
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class MaterialOfAttribute : PropertyAttribute
    {
        public string rendererPropertyName;

        public MaterialOfAttribute(string rendererPropertyName)
        {
            this.rendererPropertyName = rendererPropertyName;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MaterialOfAttribute))]
    public class MaterialOfAttributeDrawer : PropertyDrawer
    {
        private static float lineHeight => EditorStyles.objectField.lineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var propertyName = (attribute as MaterialOfAttribute).rendererPropertyName;
            var renderer = property.serializedObject.FindProperty(propertyName)?.objectReferenceValue as Renderer;
            if (renderer)
            {
                var materials = renderer.sharedMaterials;
                var index = materials.Select((m, i) => m == property.objectReferenceValue ? i : -1).FirstOrDefault(i => i >= 0);
                index = EditorGUI.Popup(position, label, index, materials.Select(m => new GUIContent(m.name)).ToArray());
                property.objectReferenceValue = materials.Skip(index).FirstOrDefault();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return lineHeight;
        }
    }
#endif
}
