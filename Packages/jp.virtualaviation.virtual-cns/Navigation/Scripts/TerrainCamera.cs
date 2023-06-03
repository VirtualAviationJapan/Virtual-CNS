namespace VirtualCNS
{
    using UdonSharp;
    using UnityEngine;
    using UnityEngine.UI;
    using VRC.Udon;

    [RequireComponent(typeof(Camera))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TerrainCamera : UdonSharpBehaviour
    {
        private const float NM = 1820.0f;

        public RawImage rawImage;
        public float range = 10;
        public int updateInterval = 10;
        public float cameraAltitude = 1000.0f;

        private float updateIntervalOffset;
        private new Camera camera;
        private Vector2Int renderedTileIndex;
        private float TileSize => range * NM;
        private Transform origin;
        private Transform initialParent;

        private void OnEnable()
        {
            if (!initialParent)
            {
                initialParent = transform.parent;
                var vehicleRigidbody = GetComponentInParent<Rigidbody>();
                if (vehicleRigidbody) origin = vehicleRigidbody.transform;
                if (origin == null) origin = transform.parent;

                camera = GetComponent<Camera>();
                rawImage.texture = camera.targetTexture;

                Debug.Log($"[Virtual-CNS][{this}:{GetHashCode():X8}] Enabled", gameObject);
            }

            updateIntervalOffset = Random.Range(0, updateInterval);

            Render();
        }

        private void Update()
        {
            if ((Time.frameCount + updateIntervalOffset) % updateInterval != 0) return;

            var worldPosition = origin.position;
            var tileIndex = ToTileIndex(worldPosition);

            if (tileIndex != renderedTileIndex) Render();

            var positionInTile = ToPositionInTile(worldPosition, tileIndex);

            transform.localPosition = Vector3.right * positionInTile.x + Vector3.up * positionInTile.y;
        }

        private void Render()
        {
            transform.parent = null;

            var worldPosition = origin.position;
            var tileIndex = ToTileIndex(worldPosition);
            var tileCenter = GetTileCenter(tileIndex);
            var wroldTileCenter = FromXZPosition(tileCenter);

            camera.transform.position = wroldTileCenter + Vector3.up * cameraAltitude;
            camera.transform.rotation = Quaternion.AngleAxis(90, Vector3.right);
            camera.orthographicSize = TileSize;
            camera.enabled = true;

            Debug.Log($"[Virtual-CNS][{this}:{GetHashCode():X8}] Rendering", gameObject);
        }

        private void OnPostRender()
        {
            renderedTileIndex = ToTileIndex(transform.position);
            transform.parent = initialParent;
            SendCustomEventDelayedFrames(nameof(_DisableCamera), 1);

            Debug.Log($"[Virtual-CNS][{this}:{GetHashCode():X8}] Rendered", gameObject);
        }

        public void _DisableCamera()
        {
            camera.enabled = false;
        }

        private Vector2 ToXZPosition(Vector3 worldPosition)
        {
            return Vector2.right * worldPosition.x + Vector2.up * worldPosition.z;
        }
        private Vector3 FromXZPosition(Vector2 xzPosition)
        {
            return Vector3.right * xzPosition.x + Vector3.forward * xzPosition.y;
        }

        private Vector2Int ToTileIndex(Vector3 worldPosition)
        {
            return new Vector2Int(Mathf.RoundToInt(worldPosition.x / TileSize), Mathf.RoundToInt(worldPosition.y / TileSize));
        }

        private Vector2 GetTileCenter(Vector2Int tileIndex)
        {
            return ((Vector2)tileIndex) * TileSize;
        }

        private Vector2 ToPositionInTile(Vector3 worldPosition, Vector2Int tile)
        {
            return ToXZPosition(worldPosition) - GetTileCenter(tile);
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void Reset()
        {
            GetComponent<Camera>().enabled = false;
        }
#endif
    }
}
