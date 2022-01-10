using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace VirtualAviationJapan
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
            foreach (var tn in GetComponentsInChildren<TailNumber>(true)) tn.Text = tailNumber;
            foreach (var cs in GetComponentsInChildren<Callsign>(true)) cs.Text = callsign;
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private static void AutoIncrement(Scene scene)
        {
            var numbers = new Regex("[0-9]+", RegexOptions.RightToLeft);

            var replacements = scene.GetRootGameObjects()
                .SelectMany(o => o.GetComponentsInChildren<TailNumberManager>())
                .GroupBy(t => t.tailNumber)
                .SelectMany(group => {
                    return group.Select((manager, i) => (manager, numbers.Replace(group.Key, match => $"{int.Parse(match.Value) + i}", 1))).Skip(1);
                });
            foreach (var (manager, tailNumber) in replacements)
            {
                Debug.Log($"[{manager}] AutoIncrement: {manager.tailNumber} -> {tailNumber}");
                manager.tailNumber = tailNumber;
                manager.Apply();
            }

            var callsignReplacements = scene.GetRootGameObjects()
                .SelectMany(o => o.GetComponentsInChildren<TailNumberManager>())
                .Where(t => !string.IsNullOrEmpty(t.callsign))
                .GroupBy(t => t.callsign)
                .SelectMany(group => {
                    return group.Select((manager, i) => (manager, numbers.Replace(group.Key, match => $"{int.Parse(match.Value) + i}", 1))).Skip(1);
                });

            foreach (var (manager, callsign) in callsignReplacements)
            {
                Debug.Log($"[{manager}] AutoIncrement: {manager.callsign} -> {callsign}");
                manager.callsign = callsign;
                manager.Apply();
            }
        }

        [InitializeOnLoadMethod]
        public static void RegisterCallbacks()
        {
            EditorSceneManager.sceneSaving += (scene, _) => AutoIncrement(scene);
            SceneManager.sceneLoaded += (scene, _) => AutoIncrement(scene);
        }
#endif
    }
}
