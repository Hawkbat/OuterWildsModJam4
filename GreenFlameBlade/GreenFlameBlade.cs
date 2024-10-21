using GreenFlameBlade.Components;
using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using System.Reflection;

namespace GreenFlameBlade
{
    public class GreenFlameBlade : ModBehaviour
    {
        public static GreenFlameBlade Instance;
        public INewHorizons NewHorizons;

        public List<CompassTarget> CompassTargets = [];

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            ModHelper.Console.WriteLine($"My mod {nameof(GreenFlameBlade)} is loaded!", MessageType.Success);

            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            new Harmony("Hawkbar.GreenFlameBlade").PatchAll(Assembly.GetExecutingAssembly());

            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen);
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            if (newScene != OWScene.SolarSystem) return;

        }
    }

}
