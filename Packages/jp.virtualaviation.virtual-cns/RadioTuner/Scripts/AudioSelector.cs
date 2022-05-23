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
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [OnAfterEditor(nameof(AudioSelector.OnAfterEditor))]
#endif
    public class AudioSelector : UdonSharpBehaviour
    {
        public RadioTuner[] comTuners = { };
        public RadioTuner[] navTuners = { };
        public MarkerReceiver markerReceiver;
        public bool xmtrMode = false;

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
            SelectedMic._SetMic(false);
            comTuners[index]._ToggleListenAndMic();
        }
        public void _ToggleComListenAndMic2() => _ToggleComListenAndMic(1);
        public void _ToggleComListenAndMic3() => _ToggleComListenAndMic(2);

        public void _SetAllComListen(bool value)
        {
            foreach (var tuner in comTuners) tuner._SetListen(value);
        }
        public void _MuteAllCom() => _SetAllComListen(false);
        public void _ListenAllCom() => _SetAllComListen(true);

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

        public void _ActivateMic(int index, bool value)
        {
            _SelectMic(index);
            if (SelectedMic.Mic != value) SelectedMic._ToggleMic();
        }
        public void _ActivateMic1() => _ActivateMic(0, true);
        public void _ActivateMic2() => _ActivateMic(1, true);
        public void _ActivateMic3() => _ActivateMic(2, true);

        public void _SelectCom(int index)
        {
            _ActivateMic(index, SelectedMic.Mic);
        }
        public void _SelectCom1() => _SelectCom(0);
        public void _SelectCom2() => _SelectCom(1);
        public void _SelectCom3() => _SelectCom(2);

        public void _SetSelectedComListen(bool value)
        {
            SelectedMic._SetListen(value);
        }
        public void _ListenSelectedCom() => _SetSelectedComListen(true);
        public void _MuteSelectedCom() => _SetSelectedComListen(false);
        public void _ToggleSelectedComListen() => _SetSelectedComListen(!SelectedMic.Listen);

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

        public void _SetNavListen(int index, bool value)
        {
            if (value != navTuners[index].Listen) navTuners[index]._ToggleListen();
        }
        public void _ListenNav1() => _SetNavListen(0, true);
        public void _ListenNav2() => _SetNavListen(1, true);
        public void _ListenNav3() => _SetNavListen(2, true);
        public void _MuteNav1() => _SetNavListen(0, false);
        public void _MuteNav2() => _SetNavListen(1, false);
        public void _MuteNav3() => _SetNavListen(2, false);

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

        private void SetMarkerMute(bool value)
        {
            if (!markerReceiver) return;
            markerReceiver._SetMute(value);
        }
        private bool GetMarkerMute() => markerReceiver ? markerReceiver.Mute : false;
        public void _ToggleMarkerMute() => SetMarkerMute(!GetMarkerMute());
        public void _UnmuteMarker() => SetMarkerMute(false);
        public void _MuteMarker() => SetMarkerMute(true);

        #region C172 Style ACU
        public bool _IsAllMicMuted()
        {
            foreach (var tuner in comTuners)
            {
                if (tuner.Mic) return false;
            }
            return true;
        }

        public bool _IsAuto()
        {
            if (!SelectedMic.Mic) return false;
            for (var i = 0; i < comTuners.Length; i++)
            {
                if (i != selectedMic && comTuners[i].Listen) return false;
            }
            return true;
        }
        public void _SetAuto()
        {
            for (var i = 0; i < comTuners.Length; i++)
            {
                comTuners[i]._SetListen(i == selectedMic);
            }
        }

        public bool _IsBoth()
        {
            foreach (var tuner in comTuners)
            {
                if (!tuner.Listen) return false;
            }
            return true;
        }
        public void _SetBoth()
        {
            _ListenAllCom();
        }

        public void _SelectXMTR(int index)
        {
            for (var i = 0; i < comTuners.Length; i++)
            {
                comTuners[i]._SetMic(i == index);
            }

            var isAuto = _IsAuto();
           selectedMic = index;
           if (isAuto) _SetAuto();
        }
        public void _SelectXMTR1() => _SelectXMTR(0);
        public void _SelectXMTR2() => _SelectXMTR(1);
        public void _SelectXMTR3() => _SelectXMTR(2);
        #endregion

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
