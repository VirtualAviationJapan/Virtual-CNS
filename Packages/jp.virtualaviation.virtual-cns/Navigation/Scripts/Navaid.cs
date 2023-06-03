using UnityEngine;
using System;
using System.Linq;
using UdonSharp;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Navaid : UdonSharpBehaviour
    {

        public string identity;
        public float frequency = 108.00f;
        public NavaidCapability capability;
        public Transform glideSlope;
        public bool hideFromMap;

        public bool IsNDB => ((int)capability & (int)NavaidCapability.NDB) != 0;
        public bool IsVOR => ((int)capability & (int)NavaidCapability.VOR) != 0;
        public bool IsILS => ((int)capability & (int)NavaidCapability.ILS) != 0;
        public bool HasDME => ((int)capability & (int)NavaidCapability.DME) != 0;
        public bool IsTACAN => ((int)capability & (int)NavaidCapability.TACAN) != 0;
        public bool IsVORTAC => ((int)capability & (int)NavaidCapability.VORTAC) != 0;

        public Transform DmeTransform
        {
            get
            {
                if (!HasDME) return null;
                if (IsILS && glideSlope != null) return glideSlope;
                return transform;
            }
        }

        private void OnValidate()
        {
            gameObject.name = identity;
        }
    }

    [Flags]
    public enum NavaidCapability
    {
        NDB = 1,
        VOR = 2,
        DME = 4,
        ILS = 8,
        VORDME = VOR | DME,
        ILSDME = ILS | DME,
        TACAN = 16 | DME,
        VORTAC = TACAN | VOR,
    }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(NavaidEditor))]
    public class NavaidEditor : Editor
    {
        public enum FrequencyMode
        {
            VOR,
            Channel,
            DME,
        };

        public static string[] Channels => Enumerable.Range(1, 126)
            .Zip("XYZ".ToCharArray(), (n, c) => $"{n}{c}")
            .ToArray();

        public static string VORToChannel(float frequency)
        {
            var normalizedFrequency = (frequency - 108.00f) * 10 + 17.0f;
            var channelNumber = Mathf.FloorToInt(normalizedFrequency);
            var channelSuffix = Mathf.Approximately(normalizedFrequency - channelNumber, 0.5f) ? 'Y' : 'X';
            return $"{channelNumber}{channelSuffix}";
        }

        public static float ChannelToVOR(string channel)
        {
            var channelNumber = int.Parse(new string(channel.TakeWhile(char.IsDigit).ToArray()));
            var channelSuffix = channel.Last();

            return (channelNumber - 17.0f) * 0.1f + 108.0f + channelSuffix == 'Y' ? 0.5f : 0.0f;
        }

        public static int VORToDME(float frequency)
        {
            var normalizedFrequency = (frequency - 108.00f) * 10 + 17.0f;
            var channelNumber = Mathf.FloorToInt(normalizedFrequency);
            var y = Mathf.Approximately(normalizedFrequency - channelNumber, 0.5f);

            return channelNumber + (y ? 1087 : 961);
        }

        public static float DMEToVOR(int frequency)
        {
            var y = frequency >= 1088;
            var channelNumber = y ? frequency - 1087 : frequency - 961;
            return (channelNumber - 17.0f) * 0.1f + 108.0f + (y ? 0.5f : 0.0f);
        }

        public static FrequencyMode FrequencyField(SerializedProperty property, FrequencyMode mode, bool hideLabel = false)
        {
            if (!hideLabel) EditorGUILayout.PrefixLabel(property.displayName);

            var nextMode = (FrequencyMode)EditorGUILayout.EnumPopup(mode);

            switch (mode)
            {
                case FrequencyMode.VOR:
                    break;
                case FrequencyMode.Channel:
                    var channels = Channels;
                    var channel = VORToChannel(property.floatValue);
                    var channelIndex = Array.IndexOf(channels, channel);
                    if (channelIndex < 0) break;

                    var nextChannelIndex = EditorGUILayout.Popup(channelIndex, channels);
                    if (nextChannelIndex != channelIndex)
                    {
                        property.floatValue = ChannelToVOR(channels[nextChannelIndex]);
                    }
                    return nextMode;
                case FrequencyMode.DME:
                    var dme = VORToDME(property.floatValue);
                    var nextDme = EditorGUILayout.IntField(dme);
                    if (nextDme != dme)
                    {
                        property.floatValue = DMEToVOR(nextDme);
                    }
                    return nextMode;
            }

            EditorGUILayout.PropertyField(property, GUIContent.none);

            return nextMode;
        }

        private FrequencyMode frequencyMode;
        public override void OnInspectorGUI()
        {
            var property = serializedObject.GetIterator();
            property.NextVisible(true);

            do
            {
                switch (property.propertyPath)
                {
                    case nameof(Navaid.frequency):
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            frequencyMode = FrequencyField(property, frequencyMode);
                        }
                        break;
                    default:
                        EditorGUILayout.PropertyField(property);
                        break;
                }
            } while (property.NextVisible(false));
        }
    }
#endif
}
