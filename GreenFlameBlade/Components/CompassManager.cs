using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class CompassManager : MonoBehaviour
    {
        static CompassManager _instance;

        public static CompassManager Get() => _instance;

        readonly List<CompassTarget> _activeTargets = [];

        public CompassTarget MakeTarget(GameObject obj, CompassFrequency frequency) => MakeTarget(obj, frequency, Vector3.zero);
        public CompassTarget MakeTarget(GameObject obj, CompassFrequency frequency, Vector3 targetOffset)
        {
            var target = obj.AddComponent<CompassTarget>();
            target.SetFrequency(frequency);
            target.SetTargetOffset(targetOffset);
            return target;
        }

        public IEnumerable<CompassTarget> GetTargets(CompassFrequency frequency) => GetTargets().Where(t => t.GetFrequency() == frequency);

        public IEnumerable<CompassTarget> GetTargets() => _activeTargets.Where(t => PlayerState.InCloakingField() == t.IsCloaked());

        public void RegisterTarget(CompassTarget target) => _activeTargets.Add(target);

        public void UnregisterTarget(CompassTarget target) => _activeTargets.Remove(target);

        void Awake()
        {
            _instance = this;
            foreach (var campfire in FindObjectsOfType<Campfire>())
            {
                if (campfire.transform.root.name == "DreamWorld_Body" || campfire.transform.root.name == "Ship_Body") continue;
                MakeTarget(campfire.transform.parent.gameObject, campfire is DreamCampfire ? CompassFrequency.DreamFire : CompassFrequency.Campfire);
            }
            foreach (var reel in FindObjectsOfType<SlideReelItem>())
            {
                MakeTarget(reel.gameObject, CompassFrequency.SlideReel);
            }
            foreach (var signal in FindObjectsOfType<AudioSignal>())
            {
                if (signal._frequency == SignalFrequency.Quantum)
                {
                    MakeTarget(signal.transform.parent.gameObject, CompassFrequency.Quantum);
                }
            }
            foreach (var lightSensor in FindObjectsOfType<SingleLightSensor>())
            {
                if (lightSensor.transform.root.name == "RingWorld_Body")
                {
                    MakeTarget(lightSensor.gameObject, CompassFrequency.RingWorldMetal);
                }
            }
            foreach (var fuelTank in FindObjectsOfType<PlayerRecoveryPoint>())
            {
                if (!fuelTank._DLCFuelTank) continue;
                MakeTarget(fuelTank.transform.parent.gameObject, CompassFrequency.DreamFire);
            }
            foreach (var lantern in FindObjectsOfType<SimpleLanternItem>())
            {
                MakeTarget(lantern.gameObject, CompassFrequency.DreamFire);
            }
            MakeTarget(Locator.GetAstroObject(AstroObject.Name.Sun).gameObject, CompassFrequency.Campfire);
        }

        void OnDestroy()
        {
            _instance = null;
        }
    }
}
