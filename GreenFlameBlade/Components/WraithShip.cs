using System.Collections.Generic;
using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class WraithShip : MonoBehaviour
    {
        const float SCAN_DURATION = 2f;
        const float SCAN_DISTANCE = 3000f;
        const float SCAN_EXIT_DISTANCE = 4000f;
        const float SCAN_REF_SCALE = 5f;
        const float ESCAPE_DISTANCE = 1500f;
        const float WARP_DURATION = 0.25f;
        static readonly Vector3 WARP_SCALE = new(0f, 3f, 0f);

        [SerializeField] DreamCampfire _dreamCampfire;
        [SerializeField] DreamArrivalPoint _dreamArrivalPoint;
        [SerializeField] DreamLanternItem _dreamLantern;
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
        Vector3 _defaultScale;
        RelativeLocationData _relativeShipLocation;
        AstroObject _currentBody;
        GameObject _dimensionBody;
        OWItem _previousHeldItem;

        void Awake()
        {
            _defaultScale = transform.localScale;
            GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
            GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
        }

        void OnDestroy()
        {
            GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
            GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
        }

        void OnEnterDreamWorld()
        {
            if (_playerAbducted)
            {
                AbductionStartCompleted();
            }
        }

        void OnExitDreamWorld()
        {
            if (_playerAbducted)
            {
                EndAbduction();
            }
        }

        void Start()
        {
            _dreamCampfire.transform.parent = Locator.GetShipTransform();
            _dreamCampfire.transform.localPosition = Vector3.zero;
            _dreamCampfire.transform.localEulerAngles = Vector3.zero;
            _dreamCampfire.transform.localScale = Vector3.one;
            _dreamCampfire._sector = Locator.GetShipTransform().GetComponentInChildren<Sector>();

            _dimensionBody = GreenFlameBlade.Instance.NewHorizons.GetPlanet("Ringed Planet");
            _dreamArrivalPoint.transform.parent = _dimensionBody.transform;
            _dreamArrivalPoint.transform.localPosition = Vector3.zero;
            _dreamArrivalPoint.transform.localEulerAngles = Vector3.zero;
            _dreamArrivalPoint.transform.localScale = Vector3.one;
            _dreamArrivalPoint._sector = _dimensionBody.GetComponentInChildren<Sector>();

            _dreamLantern.transform.parent = _dimensionBody.transform;
            _dreamLantern.transform.localPosition = Vector3.zero;
            _dreamLantern.transform.localEulerAngles = Vector3.zero;
            _dreamLantern.transform.localScale = Vector3.one;

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
            if (!_scanning && _playerScanned && !_playerAbducted && playerDist > SCAN_EXIT_DISTANCE)
            {
                _playerScanned = false;
            }
            if (_scanning)
            {
                _scanProgress = Mathf.Clamp01(_scanProgress + Time.deltaTime / SCAN_DURATION);
                UpdateScan();
                if (_scanProgress >= 1f)
                {
                    EndScan();
                    StartAbduction();
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
        }

        void StartScan()
        {
            _scanning = true;
            _scanProgress = 0f;
            _scanBeamFade.FadeTo(1f, 0.25f);
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.VisionTorch_ProjectionOn);
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
            Locator.GetPlayerBody().GetComponentInChildren<PlayerResources>().ToggleInvincibility();
            if (PlayerState.IsInsideShip())
            {
                _playerWasInShip = true;
                var shipBody = Locator.GetShipBody();
                shipBody.GetComponentInChildren<ShipDamageController>().ToggleInvincibility();
                _relativeShipLocation = new RelativeLocationData(shipBody, GetComponentInParent<OWRigidbody>(), transform);
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
            }

            _previousHeldItem = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
            if (_previousHeldItem != null)
            {
                Locator.GetToolModeSwapper().GetItemCarryTool().DropItemInstantly(null, transform);
            }

            var relativeLocation = new RelativeLocationData(Locator.GetPlayerBody(), _dreamCampfire.GetComponentInParent<OWRigidbody>(), _dreamCampfire.transform);

            var dwc = Locator.GetDreamWorldController();
            dwc._dreamCampfire = _dreamCampfire;
            dwc._dreamCampfire.OnDreamCampfireExtinguished += dwc.OnDreamCampfireExtinguished;
            dwc._dreamArrivalPoint = _dreamArrivalPoint;
            dwc._relativeSleepLocation = relativeLocation;
            dwc._cachedCamDegreesY = dwc._playerCamera.GetComponent<PlayerCameraController>().GetDegreesY();
            dwc._playerLantern = _dreamLantern;
            dwc._enteringDream = true;
            ReticleController.Hide();
            Locator.GetPromptManager().SetPromptsVisible(false);
            Locator.GetPlayerCameraDetector().GetComponent<AudioDetector>().DeactivateAllVolumes(0f);
            Locator.GetAudioMixer().MixDreamWorld();
            dwc.UpdateSimulationSphereRadius(10000f);
        }

        void AbductionStartCompleted()
        {
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.VisionTorch_EnterVision);
            Locator.GetPlayerBody().GetComponentInChildren<PlayerResources>().ToggleInvincibility();
            if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() == _dreamLantern)
            {
                Locator.GetToolModeSwapper().GetItemCarryTool().DropItemInstantly(null, transform);
            }
        }

        void EndAbduction()
        {
            _playerAbducted = false;
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.VisionTorch_ExitVision);
            var dwc = Locator.GetDreamWorldController();
            dwc.UpdateSimulationSphereRadius(0f);
            Locator.GetCloakFieldController()._exclusionSector.RemoveOccupant(Locator.GetPlayerSectorDetector());
            if (_playerWasInShip)
            {
                var spawner = Locator.GetPlayerBody().GetComponent<PlayerSpawner>();
                spawner.DebugWarp(spawner.GetSpawnPoint(SpawnLocation.Ship));
                var shipBody = Locator.GetShipBody();
                shipBody.MoveToRelativeLocation(_relativeShipLocation, GetComponentInParent<OWRigidbody>(), transform);
                if (!Physics.autoSyncTransforms)
                {
                    Physics.SyncTransforms();
                }
                shipBody.GetComponentInChildren<ShipDamageController>().ToggleInvincibility();
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
            if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() == _dreamLantern)
            {
                Locator.GetToolModeSwapper().GetItemCarryTool().DropItemInstantly(null, transform);
            }
            if (_previousHeldItem != null)
            {
                Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(_previousHeldItem);
            }
        }

        void StartWarpOut()
        {
            _warpingOut = true;
            _warpProgress = 0f;
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Enter);
        }

        void UpdateWarpOut()
        {
            transform.localScale = Vector3.Scale(_defaultScale, Vector3.Lerp(Vector3.one, WARP_SCALE, _warpProgress));
        }

        void EndWarpOut()
        {
            _warpingOut = false;
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
