using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class NomaiCrystalPuzzleDirector : MonoBehaviour
    {
        public void Awake()
        {
            // Increase probe lock-on range
            var probeBody = FindObjectOfType<OrbitalProbeLaunchController>()._probeBody;
            OWUtils.AddReferenceFrame(probeBody.gameObject, 500f, 5f, 100_000f);
        }
    }
}
