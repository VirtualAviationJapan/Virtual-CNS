using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VirtualCNS
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(1200)] // After FMC, Before CDU
    public class CDUFunction_RadioTuning : UdonSharpBehaviour
    {
        private CDU cdu;

        private float COM1
        {
            get => cdu.fmc ? cdu.fmc._GetComFrequency(0) : 0;
            set
            {
                if (cdu.fmc) cdu.fmc._SetComFrequency(0, value);
            }
        }
        private float COM2
        {
            get => cdu.fmc ? cdu.fmc._GetComFrequency(1) : 0;
            set
            {
                if (cdu.fmc) cdu.fmc._SetComFrequency(1, value);
            }
        }

        private string NAV1
        {
            get => cdu.fmc ? cdu.fmc._GetNavIdentity(0) : null;
            set
            {
                if (cdu.fmc) cdu.fmc._SetNavTargetByIdentity(0, value);
            }
        }
        private string NAV2
        {
            get => cdu.fmc ? cdu.fmc._GetNavIdentity(1) : null;
            set
            {
                if (cdu.fmc) cdu.fmc._SetNavTargetByIdentity(1, value);
            }
        }

        public void _OnPageEnter(CDU cdu)
        {
            this.cdu = cdu;
            UpdateDisplay();
        }

        public void _L1()
        {
            COM1 = cdu._FloatInput(COM1);
            UpdateDisplay();
        }

        public void _R1()
        {
            COM2 = cdu._FloatInput(COM2);
            UpdateDisplay();
        }

        public void _L3()
        {
            NAV1 = cdu._StringInput(NAV1);
            UpdateDisplay();
        }

        public void _R3()
        {
            NAV2 = cdu._StringInput(NAV2);
            UpdateDisplay();
        }

        public void _Refresh() => UpdateDisplay();

        private string FrequencyText(float value) =>  $"{value:0.000}";

#region Display
        private string Color(string color, string text) => $"<color={color}>{text}</color>";
        private string Size(string size, string text) => $"<size={size}>{text}</color>";
        private string DataLine(string label, string value) => $"{Size("75%", label)}\n{Color("#080", string.IsNullOrEmpty(value) ? "[ ]" : value)}";

        private void UpdateDisplay()
        {
            cdu.titleText.text = Color("#08f", "RADIO TUNING");
            cdu.pageText.text = Color("#08f", "1/1");

            cdu.leftLineText[0].text = DataLine("COM 1", FrequencyText(COM1));
            cdu.leftLineText[1].text = DataLine("RCL 1", null);
            cdu.leftLineText[2].text = DataLine("NAV 1", NAV1);
            cdu.leftLineText[3].text = DataLine("NAV 1", $"{Color("#08f", "AUTO")}/{Color("#fff", Size("75%", "MAN"))}");
            cdu.leftLineText[4].text = null;
            cdu.leftLineText[5].text = DataLine("ATC 1", Color("#fff", "1200"));

            cdu.centerLineText[0].text = null;
            cdu.centerLineText[1].text = null;
            cdu.centerLineText[2].text = null;
            cdu.centerLineText[3].text = "-- MODE --";
            cdu.centerLineText[4].text = null;
            cdu.centerLineText[5].text = null;

            cdu.rightLineText[0].text = DataLine("COM 2", FrequencyText(COM2));
            cdu.rightLineText[1].text = DataLine("PRE 2", null);
            cdu.rightLineText[2].text = DataLine("NAV 2", NAV2);
            cdu.rightLineText[3].text = DataLine("NAV 2", $"{Color("#08f", "AUTO")}/{Color("#fff", Size("75%", "MAN"))}");
            cdu.rightLineText[4].text = null;
            cdu.rightLineText[5].text = DataLine("ATC 2", Color("#fff", ""));
        }
#endregion
    }
}
