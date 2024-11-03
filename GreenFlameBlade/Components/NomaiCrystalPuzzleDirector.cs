using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class NomaiCrystalPuzzleDirector : MonoBehaviour
    {
        public void Awake()
        {
            // Increase probe lock-on range
            var probe = FindObjectOfType<OrbitalProbeLaunchController>()._probeBody;
            var rfVolume = probe.GetComponentInChildren<ReferenceFrameVolume>();
            rfVolume._referenceFrame.SetMaxTargetDistance(100_000f);
        }
    }
}
