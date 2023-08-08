using System;
using TMPro;
using UdonSharp;
using UnityEngine;

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TimeDisplayDriver : UdonSharpBehaviour
    {
        public TextMeshProUGUI timeText, dateText, elapsedTimeText;
        public int updateFrameInterval = 10;

        private void Start()
        {
#if UNITY_ANDROID
            updateFrameInterval *= 2;
#endif
        }

        private void Update()
        {
            if (Time.frameCount % updateFrameInterval != 0) return;

            var now = DateTime.UtcNow;
            if (timeText != null) timeText.text = now.ToLongTimeString();
            if (dateText != null) dateText.text = now.ToShortDateString();
            if (elapsedTimeText != null) elapsedTimeText.text = $"{Mathf.Floor(Time.time / 360)}:{Mathf.Floor(Time.time / 60) % 60:00}";
        }
    }
}
