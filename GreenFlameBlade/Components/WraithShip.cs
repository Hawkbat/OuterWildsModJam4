using System.Collections.Generic;
using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class WraithShip : MonoBehaviour
    {
        public static WraithShip Instance;

        const float SCAN_DURATION = 2f;
        const float SCAN_DISTANCE = 2000f;
        const float SCAN_EXIT_DISTANCE = 3000f;
        const float SCAN_REF_SCALE = 5f;
        const float ESCAPE_DISTANCE = 1000f;
        const float WARP_DURATION = 0.25f;
        static readonly Vector3 WARP_SCALE = new(0f, 3f, 0f);

        [SerializeField] OWRendererFadeController _scanBeamFade;
        [SerializeField] ReferenceFrameVolume _referenceFrameVolume;
        bool _scanning;
        float _scanProgress;
        bool _playerScanned;
        bool _warpingIn;
        bool _warpingOut;
        float _warpProgress;
        bool _playerAbducted;
        bool _playerWasInShip;
        bool _playerWasAtFlightConsole;
        bool _abductionEnding;
        Vector3 _defaultScale;
        RelativeLocationData _relativeAbductionLocation;
        AstroObject _currentBody;
        GameObject _dimensionBody;
        OWItem _previousHeldItem;
        bool _inFakeEyeSequence;

        void Awake()
        {
            _defaultScale = transform.localScale;
            GlobalMessenger.AddListener(GlobalMessengerEvents.EnterWraithDream, OnEnterWraithDream);
            GlobalMessenger.AddListener(GlobalMessengerEvents.ExitWraithDream, OnExitWraithDream);
            Instance = this;
        }

        void OnDestroy()
        {
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.EnterWraithDream, OnEnterWraithDream);
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.ExitWraithDream, OnExitWraithDream);
            Instance = null;
        }

        void OnEnterWraithDream()
        {
            if (!_playerAbducted)
            {
                StartAbduction();
            }
        }

        void OnExitWraithDream()
        {
            if (_playerAbducted)
            {
                EndAbduction();
            }
        }

        void Start()
        {
            _dimensionBody = GreenFlameBlade.Instance.NewHorizons.GetPlanet("Ringed Planet");

            EndScan();
            WarpAway(AstroObject.Name.GiantsDeep);
        }

        void Update()
        {
            var playerDist = Vector3.Distance(transform.position, Locator.GetPlayerTransform().position);
            if (!_scanning && !_playerScanned && !_playerAbducted && playerDist < SCAN_DISTANCE)
            {
                _playerScanned = true;
                StartScan();
            }
            if (_playerScanned && !_playerAbducted && !_abductionEnding && playerDist > SCAN_EXIT_DISTANCE)
            {
                GreenFlameBlade.Instance.ModHelper.Console.WriteLine($"Scan exit dist: {playerDist}");
                _playerScanned = false;
            }
            if (_scanning)
            {
                _scanProgress = Mathf.Clamp01(_scanProgress + Time.deltaTime / SCAN_DURATION);
                UpdateScan();
                if (_scanProgress >= 1f)
                {
                    EndScan();
                    if (PlayerData.GetShipLogFactSave("GFB_CRASH_VISION") != null)
                    {
                        GlobalMessenger.FireEvent(GlobalMessengerEvents.EnterWraithDream);
                    }
                    else
                    {
                        StartWarpOut();
                    }
                }
            }
            if (!_warpingOut && !_warpingIn && !_playerAbducted && playerDist < ESCAPE_DISTANCE)
            {
                if (_scanning) EndScan();
                StartWarpOut();
            }
            if (_warpingOut)
            {
                _warpProgress = Mathf.Clamp01(_warpProgress + Time.deltaTime / WARP_DURATION);
                UpdateWarpOut();
                if (_warpProgress >= 1f)
                {
                    EndWarpOut();
                    WarpAway();
                    StartWarpIn();
                }
            }
            if (_warpingIn)
            {
                _warpProgress = Mathf.Clamp01(_warpProgress + Time.deltaTime / WARP_DURATION);
                UpdateWarpIn();
                if (_warpProgress >= 1f)
                {
                    EndWarpIn();
                }
            }
            if (_playerAbducted)
            {
                UpdateAbduction();
            }
        }

        void StartScan()
        {
            _scanning = true;
            _scanProgress = 0f;
            _scanBeamFade.FadeTo(1f, 0.25f);
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.VisionTorch_ProjectionOn);
            Locator.GetShipLogManager().RevealFact("GFB_WRAITH_SHIP_SCAN");
        }

        void UpdateScan()
        {
            var camT = Locator.GetPlayerCamera().transform;
            var dir = (camT.position - transform.position).normalized;
            var target = camT.position - dir * 2f;
            target += camT.up * 50f;
            target -= camT.up * _scanProgress * 100f;

            var scale = Vector3.Distance(transform.position, target) / (SCAN_REF_SCALE * _defaultScale.z);
            _scanBeamFade.transform.localScale = Vector3.one * scale;
            _scanBeamFade.transform.rotation = Quaternion.LookRotation(dir, camT.up);
        }

        void EndScan()
        {
            _scanning = false;
            _scanBeamFade.FadeTo(0f, 0.25f);
        }

        void StartAbduction()
        {
            _playerAbducted = true;
            if (PlayerState.IsInsideShip())
            {
                _playerWasInShip = true;
                var shipBody = Locator.GetShipBody();
                shipBody.GetComponentInChildren<ShipDamageController>().ToggleInvincibility();
                _relativeAbductionLocation = new RelativeLocationData(shipBody, GetComponentInParent<OWRigidbody>(), transform);
                shipBody.WarpToPositionRotation(new Vector3(0f, 1000000f, 0f), Quaternion.identity);
                foreach (var volume in shipBody.GetComponentsInChildren<OWTriggerVolume>())
                {
                    volume.RemoveObjectFromVolume(Locator.GetPlayerDetector());
                    volume.RemoveObjectFromVolume(Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>().gameObject);
                }
                if (PlayerState.AtFlightConsole())
                {
                    _playerWasAtFlightConsole = true;
                    var cockpit = shipBody.GetComponentInChildren<ShipCockpitController>();
                    cockpit.ExitFlightConsole();
                    cockpit.CompleteExitFlightConsole();
                }
            }
            else
            {
                _playerWasInShip = false;
                _relativeAbductionLocation = new RelativeLocationData(Locator.GetPlayerBody(), GetComponentInParent<OWRigidbody>(), transform);
            }

            _previousHeldItem = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
            if (_previousHeldItem != null)
            {
                Locator.GetToolModeSwapper().GetItemCarryTool().DropItemInstantly(null, transform);
            }

            Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>().OpenEyes(1f, true);

            Locator.GetShipLogManager().RevealFact("GFB_WRAITH_SHIP_ENTER");

            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.VisionTorch_EnterVision);

            var relativeLocation = new RelativeLocationData(Vector3.up, Quaternion.identity, Vector3.zero);
            Locator.GetPlayerBody().MoveToRelativeLocation(relativeLocation, _dimensionBody.GetAttachedOWRigidbody(), _dimensionBody.transform);
            GlobalMessenger.FireEvent("WarpPlayer");
            if (!Physics.autoSyncTransforms)
            {
                Physics.SyncTransforms();
            }

            var antennaRepaired = DialogueConditionManager.SharedInstance.GetConditionState("GFB_ANTENNA_REPAIRED");
            var wraithsFreed = DialogueConditionManager.SharedInstance.GetConditionState("GFB_CONTROL_ROOM_DISABLED");
            var hasCrystal = _previousHeldItem != null && _previousHeldItem is NomaiCrystalItem;

            var isEndOfMod = antennaRepaired && wraithsFreed && hasCrystal;
            if (isEndOfMod || OWInput.IsPressed(InputLibrary.flashlight))
            {
                _inFakeEyeSequence = true;
                GlobalMessenger.FireEvent(GlobalMessengerEvents.EnterFakeEyeSequence);
            }
        }

        void UpdateAbduction()
        {
            if (_playerAbducted && Vector3.Distance(_dimensionBody.transform.position, Locator.GetPlayerTransform().position) > 100f)
            {
                Locator.GetPlayerBody().WarpToPositionRotation(_dimensionBody.transform.position + _dimensionBody.transform.up, _dimensionBody.transform.rotation);
                Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Exit);
            }
        }

        void EndAbduction()
        {
            _playerAbducted = false;
            _abductionEnding = true;
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.VisionTorch_ExitVision);
            if (_playerWasInShip)
            {
                var spawner = Locator.GetPlayerBody().GetComponent<PlayerSpawner>();
                spawner.DebugWarp(spawner.GetSpawnPoint(SpawnLocation.Ship));
                var shipBody = Locator.GetShipBody();
                shipBody.MoveToRelativeLocation(_relativeAbductionLocation, GetComponentInParent<OWRigidbody>(), transform);
                if (!Physics.autoSyncTransforms)
                {
                    Physics.SyncTransforms();
                }
                shipBody.GetComponentInChildren<ShipDamageController>().ToggleInvincibility();
            }
            else
            {
                Locator.GetPlayerBody().MoveToRelativeLocation(_relativeAbductionLocation, GetComponentInParent<OWRigidbody>(), transform);
                GlobalMessenger.FireEvent("WarpPlayer");
                if (!Physics.autoSyncTransforms)
                {
                    Physics.SyncTransforms();
                }
            }
            if (_previousHeldItem != null)
            {
                Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(_previousHeldItem);
            }

            Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>().OpenEyes(1f, true);

            Locator.GetShipLogManager().RevealFact("GFB_WRAITH_SHIP_EXIT");

            GreenFlameBlade.Instance.ModHelper.Events.Unity.FireInNUpdates(() =>
            {
                _abductionEnding = false;
                if (_playerWasInShip)
                {
                    var shipBody = Locator.GetShipBody();
                    foreach (var volume in shipBody.GetComponentsInChildren<OWTriggerVolume>())
                    {
                        volume.AddObjectToVolume(Locator.GetPlayerDetector());
                        volume.AddObjectToVolume(Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>().gameObject);
                    }
                    if (_playerWasAtFlightConsole)
                    {
                        var cockpit = shipBody.GetComponentInChildren<ShipCockpitController>();
                        cockpit.OnPressInteract();
                    }
                }
                GlobalMessenger.FireEvent("PlayerRepositioned");
            }, 2);
        }

        void StartWarpOut()
        {
            _warpingOut = true;
            _warpProgress = 0f;
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Enter);

            Locator.GetShipLogManager().RevealFact("GFB_WRAITH_SHIP_FLEE");
        }

        void UpdateWarpOut()
        {
            transform.localScale = Vector3.Scale(_defaultScale, Vector3.Lerp(Vector3.one, WARP_SCALE, _warpProgress));
        }

        void EndWarpOut()
        {
            _warpingOut = false;
            _playerScanned = false;
        }

        void StartWarpIn()
        {
            _warpingIn = true;
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Exit);
        }

        void UpdateWarpIn()
        {
            transform.localScale = Vector3.Scale(_defaultScale, Vector3.Lerp(WARP_SCALE, Vector3.one, _warpProgress));
        }

        void EndWarpIn()
        {
            _warpingIn = false;
        }

        void WarpAway(AstroObject.Name forcedPlanet = AstroObject.Name.None)
        {
            AstroObject body;
            Vector3 dir;
            if (forcedPlanet != AstroObject.Name.None)
            {
                body = Locator.GetAstroObject(forcedPlanet);
                dir = body.transform.up;
            }
            else
            {
                var bodies = new List<AstroObject>()
                {
                    Locator.GetAstroObject(AstroObject.Name.DarkBramble),
                    Locator.GetAstroObject(AstroObject.Name.BrittleHollow),
                    Locator.GetAstroObject(AstroObject.Name.GiantsDeep),
                    Locator.GetAstroObject(AstroObject.Name.TimberHearth),
                };
                bodies.Remove(_currentBody);
                body = bodies[Random.Range(0, bodies.Count)];
                dir = Random.onUnitSphere;
            }

            var radius = body._gravityVolume.GetAlignmentRadius() + 1000f;
            var pos = body.transform.position + dir * radius;
            transform.parent = body.GetRootSector().transform;
            transform.position = pos;
            transform.up = pos - body.transform.position;

            _currentBody = body;

            _referenceFrameVolume._attachedOWRigidbody = body.GetOWRigidbody();
            _referenceFrameVolume._referenceFrame._attachedOWRigidbody = body.GetOWRigidbody();
            _referenceFrameVolume._referenceFrame._attachedAstroObject = null;
            _referenceFrameVolume._referenceFrame._localPosition = body.transform.InverseTransformPoint(transform.position);

            var rfTracker = Locator.GetPlayerTransform().GetComponent<ReferenceFrameTracker>();
            if (_referenceFrameVolume._referenceFrame == rfTracker._currentReferenceFrame)
            {
                rfTracker.UntargetReferenceFrame();
            }
        }
    }
}
