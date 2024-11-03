using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class SignalBlockerPuzzleDirector : MonoBehaviour
    {
        OWAudioSource _stingerSource;

        void Awake()
        {
            var ringWorld = Locator.GetAstroObject(AstroObject.Name.RingWorld);
            var dreamWorld = Locator.GetAstroObject(AstroObject.Name.DreamWorld);

            // Apply custom owlk portrait textures
            var portraitTex = GreenFlameBlade.Instance.ModHelper.Assets.GetTexture("assets/Decal_IP_GhostPortraitsAlt_d.png");
            var frame = ringWorld.transform.Find("Sector_RingInterior/Sector_Zone4/Sector_BlightedShore/Sector_JammingControlRoom_Zone4/Props_JammingControlRoom_Zone4/OtherComponentsGroup/Prefab_IP_PictureFrame/PictureFrame_Decal");
            frame.GetComponent<MeshRenderer>().material.mainTexture = portraitTex;

            // Wire up antenna repair puzzle
            var brokenAntenna = ringWorld.transform.Find("Sector_RingWorld/SignalBlockerAntenna/AntennaBroken");
            var repairReciever = brokenAntenna.gameObject.AddComponent<GenericRepairReceiver>();
            repairReciever.OnRepaired += RepairReciever_OnRepaired;
            DialogueConditionManager.SharedInstance.SetConditionState("GFB_ANTENNA_REPAIRED", false);

            // Nitpick but this elevator is facing the wrong way
            var wrongFacingElevator = dreamWorld.transform.Find("Sector_DreamWorld/Sector_DreamZone_2/Structure_DreamZone_2/City/HornetHouse/Elevator");
            wrongFacingElevator.transform.localEulerAngles = Vector3.zero;

            // Wire up gear interface to elevator
            var elevator = dreamWorld.transform.Find("Sector_DreamWorld/Sector_DreamZone_2/Structure_DreamZone_2/City/ZoteHouse/Elevator/Prefab_IP_DW_CageElevator").GetComponent<CageElevator>();
            var lowerFloor = elevator._destinations[0];
            var gears = dreamWorld.transform.Find("Sector_DreamWorld/Sector_DreamZone_2/Structure_DreamZone_2/City/ZoteHouse/Elevator/ElevatorDestinations/LowerDestination/Prefab_IP_DW_GearInterface_Standing");
            var gearInterface = gears.GetComponentInChildren<GearInterfaceEffects>();
            var interactReceiver = gears.GetComponentInChildren<InteractReceiver>();
            lowerFloor._gearInterface = gearInterface;
            lowerFloor._interactReceiver = interactReceiver;
            interactReceiver.OnPressInteract += lowerFloor.OnPressInteract;
            interactReceiver.SetPromptText(UITextType.RotateGearPrompt);

            _stingerSource = ringWorld.transform.Find("Sector_RingWorld/StingerSource").GetComponent<OWAudioSource>();
        }

        void RepairReciever_OnRepaired(GenericRepairReceiver target)
        {
            DialogueConditionManager.SharedInstance.SetConditionState("GFB_ANTENNA_REPAIRED", true);
            Locator.GetShipLogManager().RevealFact("GFB_ANTENNA_REPAIRED");
            Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.GearRotate_Light);
            Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.NomaiPowerOn);
            _stingerSource.Play();
        }
    }
}
