using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class CompassTool : OWItem
    {
        [SerializeField] CompassFrequency _frequency;
        [SerializeField] Transform _needle;
        [SerializeField] MeshRenderer _screenRenderer;
        [SerializeField] OWCamera _snapshotCamera;
        RenderTexture _snapshotTexture;
        ScreenPrompt _scanPrompt;
        Quaternion _needleTargetRot;

        public CompassFrequency GetFrequency() => _frequency;

        public override string GetDisplayName() => GreenFlameBlade.Instance.NewHorizons.GetTranslationForUI("CompassTool");

        public override void Awake()
        {
            _type = Enums.ItemType_Compass;
            base.Awake();
        }

        public override void PickUpItem(Transform holdTranform)
        {
            base.PickUpItem(holdTranform);
            transform.localPosition = new Vector3(0f, 0.3f, 0f);
            transform.localEulerAngles = new Vector3(345f, 10f, 345f);
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.Lantern_Pickup);
        }

        public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
        {
            base.DropItem(position, normal, parent, sector, customDropTarget);
            Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.Lantern_Drop);
        }

        protected void Update()
        {
            var isHeld = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == Enums.ItemType_Compass;
            var isInItemToolMode = OWInput.IsInputMode(InputMode.Character) && Locator.GetToolModeSwapper().GetToolMode() == ToolMode.Item;
            var isInSuitMode = Locator.GetToolModeSwapper().GetToolGroup() == ToolGroup.Suit;

            if (_scanPrompt != null) _scanPrompt.SetVisibility(isHeld && isInItemToolMode && isInSuitMode);
            
            if (!isHeld) return;

            if (Locator.GetToolModeSwapper().IsInToolMode(ToolMode.None, ToolGroup.Ship))
            {
                Locator.GetToolModeSwapper().EquipToolMode(ToolMode.Item);
            }

            if (Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item, ToolGroup.Ship))
            {
                if (OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.All))
                {
                    var cockpitController = Locator.GetShipBody().GetComponentInChildren<ShipCockpitController>();
                    if (cockpitController != null)
                    {
                        cockpitController.ExitFlightConsole();
                    }
                }
            }

            CompassTarget target = null;

            var collider = Locator.GetToolModeSwapper()._firstPersonManipulator._lastHitCollider;
            if (collider != null)
            {
                target = collider.GetComponentInParent<CompassTarget>();
            }

            if (_scanPrompt == null)
            {
                _scanPrompt = new ScreenPrompt(InputLibrary.toolActionSecondary, GreenFlameBlade.Instance.NewHorizons.GetTranslationForUI("CompassToolScanPrompt") + "   <CMD>");
                Locator.GetPromptManager().AddScreenPrompt(_scanPrompt, PromptPosition.Center);
            }
            _scanPrompt.SetVisibility(target != null);

            var isScanning = target != null && isInItemToolMode && OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary, InputMode.Character);
            if (isScanning)
            {
                if (_snapshotTexture == null)
                {
                    _snapshotTexture = new RenderTexture(512, 512, 16);
                    _snapshotTexture.name = "CompassToolSnapshot";
                    _snapshotTexture.hideFlags = HideFlags.HideAndDontSave;
                    _snapshotTexture.Create();
                }
                _snapshotCamera.targetTexture = _snapshotTexture;
                _snapshotCamera.Render();
                _screenRenderer.material.SetTexture("_EmissionMap", _snapshotTexture);
                _frequency = target.GetFrequency();
                Locator.GetPlayerAudioController().PlayProbeSnapshot();
                Locator.GetPlayerAudioController().PlayLockOn();
            }

            target = null;

            var targets = GreenFlameBlade.Instance.GetCompassTargets(_frequency);
            var minDist = float.PositiveInfinity;

            foreach (var t in targets)
            {
                var dist = Vector3.Distance(t.GetTargetPosition(), transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    target = t;
                }
            }

            if (target != null)
            {
                _needleTargetRot = Quaternion.LookRotation(target.GetTargetPosition() - _needle.transform.position, transform.up);
            }
            else
            {
                _needleTargetRot = Quaternion.LookRotation(transform.forward, transform.up);
            }
            _needle.transform.rotation = Quaternion.RotateTowards(_needle.transform.rotation, _needleTargetRot, 360f * Time.deltaTime);
        }
    }
}
