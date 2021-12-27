namespace VirtualAviationJapan
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
        public float updateInterval = 0.3f;

        private new Camera camera;
        private Vector2Int renderedTile;
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

            Render();
        }

        private void Update()
        {
            var position = origin.position;
            var tilePosition = new Vector2((position.x / TileSize) % 1.0f, (position.z / TileSize) % 1.0f);

            if (GetTileIndex() != renderedTile) Render();

            transform.localPosition = Vector3.right * (position.x % TileSize) + Vector3.up * (position.z % TileSize);

            enabled = false;
            SendCustomEventDelayedSeconds(nameof(_Enable), updateInterval);
        }

        public void _Enable() => enabled = true;

        private void Render()
        {
            var position = origin.position;
            transform.parent = null;
            camera.transform.position = new Vector3(Mathf.FloorToInt(position.x / TileSize) * TileSize, position.y + 100.0f, Mathf.FloorToInt(position.z / TileSize) * TileSize);
            camera.transform.rotation = Quaternion.AngleAxis(90, Vector3.right);
            camera.orthographicSize = TileSize;
            camera.enabled = true;

        }

        private void OnPostRender()
        {
            renderedTile = GetTileIndex();
            camera.enabled = false;

            transform.parent = initialParent;
        }

        private Vector2Int GetTileIndex()
        {
            var position = origin.position;
            return new Vector2Int(Mathf.FloorToInt(position.x / TileSize), Mathf.FloorToInt(position.y / TileSize));
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void Reset()
        {
            GetComponent<Camera>().enabled = false;
        }
#endif
    }
}
