using UnityEngine;
namespace Lemko
{
    [RequireComponent(typeof(Camera))]
    public sealed class WorldCameraFit : MonoBehaviour
    {
        [Tooltip("Minimum visible world area; matches the 1280 x 720 UI reference.")]
        public Vector2 referenceSize = new Vector2(16, 9);
        Camera target;
        void Awake() { target = GetComponent<Camera>(); }
        void LateUpdate() { target.orthographicSize = Mathf.Max(referenceSize.y * .5f, referenceSize.x / (2f * target.aspect)); }
    }
}
