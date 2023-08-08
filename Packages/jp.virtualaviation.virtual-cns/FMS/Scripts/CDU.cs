
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using VRC.SDKBase;
using VRC.Udon;

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(1300)] // After FMC, CDUFunction
    public class CDU : UdonSharpBehaviour
    {
        public UdonSharpBehaviour startFunction;

        [NonSerialized] public FMC fmc;
        private void Start()
        {
            fmc = GetComponentInParent<FMC>();
            fmc._Subscribe(this);

            UpdateInputText();

            _SetFunction(startFunction);
            function._OnPageEnter(this);
        }

        public void _FMC_Updated()
        {
            function._Refresh();
        }

#region Function
        [Header("Function")]
        private CDUFunction function;
        [NonSerialized] public string input = "";

        public void _SetFunction(UdonSharpBehaviour value)
        {
            function = (CDUFunction)value;
        }

        public void _L1() => function._L1();
        public void _L2() => function._L2();
        public void _L3() => function._L3();
        public void _L4() => function._L4();
        public void _L5() => function._L5();
        public void _L6() => function._L6();
        public void _R1() => function._R1();
        public void _R2() => function._R2();
        public void _R3() => function._R3();
        public void _R4() => function._R4();
        public void _R5() => function._R5();
        public void _R6() => function._R6();

        public string _StringInput(string currentValue)
        {
            if (_HasInput())
            {
                var value = input;
                _ClearInput();
                return value;
            }

            _SetInput(currentValue);
            return currentValue;
        }

        public float _FloatInput(float currentValue)
        {
            if (_HasInput())
            {
                float value;
                if (float.TryParse(input, out value))
                {
                    _ClearInput();
                    return value;
                }
            }
            else
            {
                _SetInput($"{currentValue}");
            }

            return currentValue;
        }
#endregion

#region Display
        [Header("Display")]
        public TextMeshProUGUI titleText, pageText;
        public TextMeshProUGUI[] leftLineText = {}, centerLineText = {}, rightLineText = {};
#endregion

#region Input
        [Header("Scratch Pad")]
        public TextMeshProUGUI scratchPadText;

        public void _Input(char value)
        {
            input += value;
            UpdateInputText();
        }
        public void _ClearInput()
        {
            input = "";
            UpdateInputText();
        }
        public void _SetInput(string value)
        {
            input = value;
            UpdateInputText();
        }
        public bool _HasInput()
        {
            return !string.IsNullOrEmpty(input);
        }
        public void _OnInput()
        {
            UpdateInputText();
        }

        private void UpdateInputText()
        {
            scratchPadText.text = input;
        }
#endregion
    }
}
