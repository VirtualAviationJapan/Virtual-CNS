using JetBrains.Annotations;
using TMPro;
using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NavaidTextDriver : UdonSharpBehaviour
    {
        private const int UPDATE_IDENTITY_TEXT = 0;
        private const int UPDATE_DISTANCE_TEXT = 1;
        private const int UPDATE_BEARING_TEXT = 2;
        private const int UPDATE_STEP_COUNT = 4;

        public TextMeshPro identityText;
        public TextMeshProUGUI identityTextUGUI;
        public TextMeshPro distanceText;
        public TextMeshProUGUI distanceTextUGUI;
        [NotNull] public string distanceTextFormat = "0.0 NM";
        public TextMeshPro bearingText;
        public TextMeshProUGUI bearingTextUGUI;
        [NotNull] public string bearingTextFormat = "000 °";
        public int updateIntervalFrames = 10;

        public NavSelector navSelector;
        private float magneticDeclination;
        private int updateIntervalOffsetFrames;

        private Vector3 GetNavaidPosition()
        {
            var t = navSelector.NavaidTransform;
            return (t ? t : transform).position;
        }


        private int GetBearing()
        {
            var direction = Vector3.ProjectOnPlane(GetNavaidPosition() - transform.position, Vector3.up).normalized;
            var bearing = (Mathf.RoundToInt(Vector3.SignedAngle(Vector3.forward, direction, Vector3.up)) + 360) % 360;
            return bearing == 0 ? 360 : bearing;
        }

        private void Start()
        {
            if (!navSelector) navSelector = GetComponentInParent<NavSelector>();
            var navaidDatabaseObj = GameObject.Find(nameof(NavaidDatabase));
            if (navaidDatabaseObj) magneticDeclination = navaidDatabaseObj.GetComponent<NavaidDatabase>().magneticDeclination;
        }

        private void OnEnable()
        {
            updateIntervalOffsetFrames = Random.Range(0, UPDATE_STEP_COUNT);
        }

        private void Update()
        {
            var updateCount = (Time.renderedFrameCount + updateIntervalOffsetFrames) % (UPDATE_STEP_COUNT + updateIntervalFrames);

            switch (updateCount)
            {
                case UPDATE_IDENTITY_TEXT:
                    UpdateText(identityText, identityTextUGUI, navSelector.Identity ?? "---");
                    break;
                case UPDATE_DISTANCE_TEXT:
                    UpdateText(distanceText, distanceTextUGUI, navSelector.HasDME ? (Vector3.Distance(transform.position, GetNavaidPosition()) / 1852.0f).ToString(distanceTextFormat) : null);
                    break;
                case UPDATE_BEARING_TEXT:
                    UpdateText(bearingText, bearingTextUGUI, navSelector.IsVOR ? GetBearing().ToString(bearingTextFormat) : null);
                    break;
            }
        }

        private void UpdateText(TextMeshPro text, TextMeshProUGUI textUGUI, string textValue)
        {
            if (text) text.text = textValue;
            if (textUGUI) textUGUI.text = textValue;
        }
    }
}
