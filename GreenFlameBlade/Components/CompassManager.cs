using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class CompassManager : MonoBehaviour
    {
        void Awake()
        {
            foreach (var campfire in FindObjectsOfType<Campfire>())
            {
                if (campfire.transform.root.name == "DreamWorld_Body" || campfire.transform.root.name == "Ship_Body") continue;
                CompassTarget.Make(campfire.transform.parent.gameObject, campfire is DreamCampfire ? CompassFrequency.DreamFire : CompassFrequency.Campfire);
            }
            foreach (var reel in FindObjectsOfType<SlideReelItem>())
            {
                CompassTarget.Make(reel.gameObject, CompassFrequency.SlideReel);
            }
            foreach (var signal in FindObjectsOfType<AudioSignal>())
            {
                if (signal._frequency == SignalFrequency.Quantum)
                {
                    CompassTarget.Make(signal.transform.parent.gameObject, CompassFrequency.Quantum);
                }
            }
            foreach (var lightSensor in FindObjectsOfType<SingleLightSensor>())
            {
                if (lightSensor.transform.root.name == "RingWorld_Body")
                {
                    CompassTarget.Make(lightSensor.gameObject, CompassFrequency.RingWorldMetal);
                }
            }
            foreach (var fuelTank in FindObjectsOfType<PlayerRecoveryPoint>())
            {
                if (!fuelTank._DLCFuelTank) continue;
                CompassTarget.Make(fuelTank.transform.parent.gameObject, CompassFrequency.DreamFire);
            }
            foreach (var lantern in FindObjectsOfType<SimpleLanternItem>())
            {
                CompassTarget.Make(lantern.gameObject, CompassFrequency.DreamFire);
            }
            CompassTarget.Make(Locator.GetAstroObject(AstroObject.Name.Sun).gameObject, CompassFrequency.Campfire);
        }
    }
}
