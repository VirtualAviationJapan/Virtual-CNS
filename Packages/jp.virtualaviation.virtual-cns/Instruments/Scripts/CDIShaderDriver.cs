using UdonSharp;
using UnityEngine;

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(100)] // After NavSelector
    public class CDIShaderDriver : UdonSharpBehaviour
    {
        public Renderer cdiRenderer;
        [MaterialOf("cdiRenderer")] public Material cdiMaterial;
        public Renderer dmeRenderer;
        [MaterialOf("dmeRenderer")] public Material dmeMaterial;

        [Header("Advanced Options")]
        public string locationPropertyName = "_VORLocation";
        public string coursePropertyName = "_OBS";
        public string ilsPropertyName = "_ILS";
        public string localizerDirectionPropertyName = "_LocalizerDirection";
        public string glideSlopeLocationPropertyName = "_GlideSlopeLocation";
        public string dmeEnabledPropertyName = "_DMEEnabled";
        public string dmeLocationPropertyName = "_DMELocation";
        public string markerPropertyName = "_Marker";
        private NavSelector navSelector;

        private int cdiMaterialIndex, dmeMaterialIndex;
        private MaterialPropertyBlock properties;
        private void Start()
        {
            cdiMaterialIndex = FindMaterialIndex(cdiRenderer, cdiMaterial);
            dmeMaterialIndex = FindMaterialIndex(dmeRenderer, dmeMaterial);

            properties = new MaterialPropertyBlock();
            navSelector = GetComponentInParent<NavSelector>();
            navSelector._Subscribe(this);
        }

        public void _NavChanged()
        {
            UpdateCDI();
            UpdateDME();
        }

        private void UpdateCDI()
        {
            var navaidTransform = navSelector.NavaidTransform;
            var navaidPosition = navaidTransform.position;
            var isILS = navSelector.IsILS;
            cdiRenderer.GetPropertyBlock(properties);

            properties.SetVector(locationPropertyName, navaidPosition);
            properties.SetFloat(coursePropertyName, navSelector.Course);

            properties.SetFloat(ilsPropertyName, isILS ? 1.0f : 0.0f);
            if (isILS)
            {
                var glideSlopeTransform = navSelector.GlideSlopeTransform;
                properties.SetVector(localizerDirectionPropertyName, navaidTransform.forward);
                properties.SetVector(glideSlopeLocationPropertyName, glideSlopeTransform.position);
            }


            cdiRenderer.SetPropertyBlock(properties);
        }

        private void UpdateDME()
        {
            var hasDME = navSelector.HasDME;

            dmeRenderer.GetPropertyBlock(properties);

            properties.SetFloat(dmeEnabledPropertyName, hasDME ? 1 : 0);
            if (hasDME)
            {
                var dmeTransform = navSelector.DMETransform;
                properties.SetVector(dmeLocationPropertyName, dmeTransform.position);
            }

            dmeRenderer.SetPropertyBlock(properties);
        }

        private int FindMaterialIndex(Renderer renderer, Material material)
        {
            if (renderer == null) return 0;

            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i] == material)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
