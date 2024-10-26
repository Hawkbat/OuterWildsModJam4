using GreenFlameBlade.Components;
using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GreenFlameBlade
{
    public class GreenFlameBlade : ModBehaviour
    {
        public static GreenFlameBlade Instance;
        public INewHorizons NewHorizons;

        List<CompassTarget> _compassTargets = [];

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            ModHelper.Console.WriteLine($"{nameof(GreenFlameBlade)} is loaded!", MessageType.Success);

            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            new Harmony($"Hawkbar.{nameof(GreenFlameBlade)}").PatchAll(Assembly.GetExecutingAssembly());

            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen);
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            if (newScene != OWScene.SolarSystem) return;

            ModHelper.Events.Unity.FireInNUpdates(() =>
            {
                // Attach compass targets to existing objects
                foreach (var campfire in FindObjectsOfType<Campfire>())
                {
                    if (campfire.transform.root.name == "DreamWorld_Body" || campfire.transform.root.name == "Ship_Body") continue;
                    CompassTarget.Make(campfire.transform.parent.gameObject, campfire is DreamCampfire ? CompassFrequency.DreamFire : CompassFrequency.Campfire);
                    campfire.transform.parent.gameObject.AddComponent<CampfireCompassTargetController>();
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
                foreach (var fuelTank in FindObjectsOfType<PlayerRecoveryPoint>())
                {
                    if (!fuelTank._DLCFuelTank) continue;
                    CompassTarget.Make(fuelTank.transform.parent.gameObject, CompassFrequency.DreamFire);
                }
                CompassTarget.Make(Locator.GetAstroObject(AstroObject.Name.Sun).gameObject, CompassFrequency.Campfire);

                var directors = new GameObject("GreenFlameBlade Puzzle Directors");
                directors.AddComponent<SignalBlockerPuzzleDirector>();
                directors.AddComponent<SimulationControlPuzzleDirector>();

                Locator.GetPlayerBody().gameObject.AddComponent<DreamWorldDebugger>();
            }, 2);
        }

        public IEnumerable<CompassTarget> GetCompassTargets(CompassFrequency frequency) => _compassTargets.Where(t => t.GetFrequency() == frequency);

        public IEnumerable<CompassTarget> GetCompassTargets() => _compassTargets;

        public void RegisterCompassTarget(CompassTarget compassTarget) => _compassTargets.Add(compassTarget);

        public void UnregisterCompassTarget(CompassTarget compassTarget) => _compassTargets.Remove(compassTarget);
    }

}
