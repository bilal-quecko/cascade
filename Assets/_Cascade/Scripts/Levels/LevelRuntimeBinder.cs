using UnityEngine;

namespace Cascade.Levels
{
    /// <summary>
    /// Contract component placed on every level prefab root.
    /// The shared Gameplay scene binds its systems to these authored roots/anchors.
    /// </summary>
    public sealed class LevelRuntimeBinder : MonoBehaviour
    {
        [Header("Prefab Contract")]
        public Transform environmentRoot;
        public Transform machineRoot;
        public Transform placementRoot;
        public Transform objectivesRoot;
        public Transform cameraRoot;
        public Transform vfxRoot;
        public Transform lightingRoot;

        [Header("Camera")]
        public Transform observationAnchor;
        public Transform simulationInterestRoot;
        public Transform resultAnchor;

        public Rigidbody[] GetRuntimeRigidbodies()
        {
            return machineRoot != null
                ? machineRoot.GetComponentsInChildren<Rigidbody>(true)
                : GetComponentsInChildren<Rigidbody>(true);
        }
    }
}
