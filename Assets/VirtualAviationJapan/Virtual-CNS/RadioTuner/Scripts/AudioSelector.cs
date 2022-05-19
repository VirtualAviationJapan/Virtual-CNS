using UdonSharp;
using UnityEngine;
using UdonToolkit;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
#endif

namespace VirtualAviationJapan
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [OnAfterEditor(nameof(AudioSelector.OnAfterEditor))]
    public class AudioSelector : UdonSharpBehaviour
    {
        public RadioTuner[] comTuners = { };
        public RadioTuner[] navTuners = { };

        public RadioTuner SelectedMic => comTuners[selectedMic];
        public bool Mic => SelectedMic.Mic;
        public RadioTuner SelectedNavTuner => navTuners[selectedNav];
        public bool NavListen => SelectedNavTuner.Listen;

        private int selectedMic, selectedNav;

        public void _ToggleComListen(int index)
        {
            comTuners[index]._ToggleListen();
        }
        public void _ToggleComListen1() => _ToggleComListen(0);
        public void _ToggleComListen2() => _ToggleComListen(1);
        public void _ToggleComListen3() => _ToggleComListen(2);
        public void _ToggleComListenAndMic(int index)
        {
            comTuners[index]._ToggleListenAndMic();
        }
        public void _ToggleComListenAndMic1() => _ToggleComListenAndMic(0);
        public void _ToggleComListenAndMic2() => _ToggleComListenAndMic(1);
        public void _ToggleComListenAndMic3() => _ToggleComListenAndMic(2);

        public void _SelectMic(int index)
        {
            if (selectedMic == index) return;
            if (Mic) SelectedMic._ToggleMic();
            selectedMic = index;
        }
        public void _SelectMic1() => _SelectMic(0);
        public void _SelectMic2() => _SelectMic(1);
        public void _SelectMic3() => _SelectMic(2);

        public void _ToggleMic(int index)
        {
            _SelectMic(index);
            comTuners[index]._ToggleMic();
        }
        public void _ToggleMic1() => _ToggleMic(0);
        public void _ToggleMic2() => _ToggleMic(1);
        public void _ToggleMic3() => _ToggleMic(2);
        public void _StartPTT() => SelectedMic._StartPTT();
        public void _EndPTT() => SelectedMic._EndPTT();

        public void _ToggleNavListen(int index)
        {
            navTuners[index]._ToggleListen();
        }
        public void _ToggleNavListen1() => _ToggleNavListen(0);
        public void _ToggleNavListen2() => _ToggleNavListen(1);
        public void _ToggleNavListen3() => _ToggleNavListen(2);
        public void _ToggleSelectedNavListen() => _ToggleNavListen(selectedNav);

        public void _SelectNav(int index)
        {
            if (index == selectedNav) return;
            var listen = NavListen;
            if (navTuners[index].Listen != listen) navTuners[index]._ToggleListen();
            if (listen) SelectedNavTuner._ToggleListen();
            selectedNav = index;
        }
        public void _SelectNav1() => _SelectNav(0);
        public void _SelectNav2() => _SelectNav(1);
        public void _SelectNav3() => _SelectNav(2);

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public static void OnAfterEditor(SerializedObject serializedObject)
        {
            var audioSelector = serializedObject.targetObject as AudioSelector;
            var coms = EditorGUILayout.ObjectField("Load COMs", null, typeof(RadioTunerDemultiplexer), true) as RadioTunerDemultiplexer;
            if (coms) audioSelector.comTuners = coms.tuners.ToArray();

            var navs = EditorGUILayout.ObjectField("Load NAVs", null, typeof(RadioTunerDemultiplexer), true) as RadioTunerDemultiplexer;
            if (navs) audioSelector.navTuners = navs.tuners.ToArray();

            if (coms || navs)
            {
                audioSelector.ApplyProxyModifications();
                EditorUtility.SetDirty(UdonSharpEditorUtility.GetBackingUdonBehaviour(audioSelector));
            }
        }
#endif
    }
}
