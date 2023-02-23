using UdonSharp;
using UnityEngine;
using UdonToolkit;
using TMPro;
using URC;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
using System.Linq;
#endif

namespace VirtualCNS
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [OnAfterEditor(nameof(StandbyFrequencySwitcher.OnAfterEditor))]
#endif
    public class StandbyFrequencySwitcher : UdonSharpBehaviour
    {
        public RadioTuner active, standby;

        public void _TakeOwnership()
        {
            active._TakeOwnership();
            standby._TakeOwnership();
        }

        public void _RequenstSerialization()
        {
            active.RequestSerialization();
            standby.RequestSerialization();
        }

        public void _TransferFrequency()
        {
            _TakeOwnership();

            var tmp = standby.Frequency;
            standby.Frequency = active.Frequency;
            active.Frequency = tmp;

            _RequenstSerialization();
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public static void OnAfterEditor(SerializedObject serializedObject)
        {
            var active = serializedObject.FindProperty(nameof(StandbyFrequencySwitcher.active)).objectReferenceValue as RadioTuner;
            var standby = serializedObject.FindProperty(nameof(StandbyFrequencySwitcher.standby)).objectReferenceValue as RadioTuner;
            SetupHelperGUI(active, standby);
        }

        public static void SetupHelperGUI(RadioTuner active, RadioTuner standby)
        {
            if (active)
            {
                EditorGUILayout.LabelField($"{active.gameObject.name} (Active)");
                using (new EditorGUI.IndentLevelScope())
                {
                    using (var tunerChange = new EditorGUI.ChangeCheckScope())
                    {
                        active.frequencyDisplay = EditorGUILayout.ObjectField("Frequency Display", active.frequencyDisplay, typeof(TextMeshPro), true) as TextMeshPro;
                        active.listeningIndiator = EditorGUILayout.ObjectField("Listening Indicator", active.listeningIndiator, typeof(GameObject), true) as GameObject;
                        if (!active.navMode) active.micIndicator = EditorGUILayout.ObjectField("Mic Indicator", active.micIndicator, typeof(GameObject), true) as GameObject;
                        active.identityDisplay = EditorGUILayout.ObjectField("Identity Display", active.identityDisplay, typeof(TextMeshPro), true) as TextMeshPro;

                        if (active.navMode)
                        {
                            active.navSelector = EditorGUILayout.ObjectField("Nav Selector", active.navSelector, typeof(NavSelector), true) as NavSelector;
                            active.identityPlayer = EditorGUILayout.ObjectField("Identity Player", active.identityPlayer, typeof(IdentityPlayer), true) as IdentityPlayer;
                        }
                        else
                        {
                            active.receiver = EditorGUILayout.ObjectField("Receiver", active.receiver, typeof(Receiver), true) as Receiver;
                            active.transmitter = EditorGUILayout.ObjectField("Transmitter", active.transmitter, typeof(Transmitter), true) as Transmitter;
                        }

                        active.animator = EditorGUILayout.ObjectField("Animator", active.animator, typeof(Animator), true) as Animator;
                        active.listenBool = EditorGUILayout.TextField("Listen Bool", active.listenBool);
                        active.micBool = EditorGUILayout.TextField("Listen Bool", active.micBool);
                    }
                }
            }

            if (standby)
            {
                EditorGUILayout.LabelField($"{standby.gameObject.name} (Standby)");
                using (new EditorGUI.IndentLevelScope())
                {
                    using (var tunerChange = new EditorGUI.ChangeCheckScope())
                    {
                        standby.frequencyDisplay = EditorGUILayout.ObjectField("Frequency Display", standby.frequencyDisplay, typeof(TextMeshPro), true) as TextMeshPro;
                    }
                }
            }
        }
#endif
    }
}
