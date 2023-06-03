using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor.BuildPipeline;
#endif

namespace VirtualCNS
{
    public class TailNumberManager : MonoBehaviour
    {
        public string tailNumber = "VN0000";
        public string callsign = null;
        public bool autoIncrement = true;

        [ExecuteInEditMode]
        private void Start() => Apply();

        private void OnValidate() => Apply();

        private void Reset()
        {
            hideFlags = HideFlags.DontSaveInBuild;
        }

        private void Apply()
        {
            foreach (var tn in GetComponentsInChildren<TailNumber>(true))
            {
                tn.Text = tailNumber;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
                EditorUtility.SetDirty(tn);
#endif
            }
            foreach (var cs in GetComponentsInChildren<Callsign>(true))
            {
                cs.Text = callsign;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
                EditorUtility.SetDirty(cs);
#endif
            }
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private static void AutoIncrement(Scene scene)
        {
            var numbers = new Regex("[0-9]+", RegexOptions.RightToLeft);

            var replacements = scene.GetRootGameObjects()
                .SelectMany(o => o.GetComponentsInChildren<TailNumberManager>())
                .GroupBy(t => t.tailNumber)
                .SelectMany(group =>
                {
                    return group.Select((manager, i) => (manager, numbers.Replace(group.Key, match => (int.Parse(match.Value) + i).ToString(new string (Enumerable.Repeat('0', match.Value.Length).ToArray())), 1))).Skip(1);
                });
            foreach (var (manager, tailNumber) in replacements)
            {
                Debug.Log($"[{manager}] AutoIncrement: {manager.tailNumber} -> {tailNumber}", manager);
                manager.tailNumber = tailNumber;
                manager.Apply();
                EditorUtility.SetDirty(manager);
            }

            var callsignReplacements = scene.GetRootGameObjects()
                .SelectMany(o => o.GetComponentsInChildren<TailNumberManager>())
                .Where(t => !string.IsNullOrEmpty(t.callsign))
                .GroupBy(t => t.callsign)
                .SelectMany(group =>
                {
                    return group.Select((manager, i) => (manager, numbers.Replace(group.Key, match => $"{int.Parse(match.Value) + i}", 1))).Skip(1);
                });

            foreach (var (manager, callsign) in callsignReplacements)
            {
                Debug.Log($"[{manager}] AutoIncrement: {manager.callsign} -> {callsign}", manager);
                manager.callsign = callsign;
                manager.Apply();
                EditorUtility.SetDirty(manager);
            }
        }

        [InitializeOnLoadMethod]
        public static void RegisterCallbacks()
        {
            EditorApplication.playModeStateChanged += (PlayModeStateChange e) => {
                if (e == PlayModeStateChange.EnteredPlayMode) AutoIncrement(SceneManager.GetActiveScene());
            };
        }

        public class BuildCallback : IVRCSDKBuildRequestedCallback
        {
            public int callbackOrder => 10;

            public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
            {
                AutoIncrement(SceneManager.GetActiveScene());
                return true;
            }
        }
#endif
    }
}
