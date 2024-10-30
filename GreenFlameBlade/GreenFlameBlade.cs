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
        public ICommonCameraAPI CommonCameraUtility;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            ModHelper.Console.WriteLine($"{nameof(GreenFlameBlade)} is loaded!", MessageType.Success);

            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            CommonCameraUtility = ModHelper.Interaction.TryGetModApi<ICommonCameraAPI>("xen.CommonCameraUtility");

            new Harmony($"Hawkbar.{nameof(GreenFlameBlade)}").PatchAll(Assembly.GetExecutingAssembly());

            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen);
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            if (newScene != OWScene.SolarSystem) return;

            ModHelper.Events.Unity.FireInNUpdates(() =>
            {
                var directors = new GameObject("GreenFlameBlade Managers");
                directors.AddComponent<CompassManager>();
                directors.AddComponent<SignalBlockerPuzzleDirector>();
                directors.AddComponent<SimulationControlPuzzleDirector>();
                directors.AddComponent<FakeEyeSequenceDirector>();

                Locator.GetPlayerBody().gameObject.AddComponent<DreamWorldDebugger>();
            }, 1);
        }
    }
}
