﻿using OWML.Utils;
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
        NotificationData _notificationData;
        Quaternion _needleTargetRot;
        CompassTarget _potentialTarget;
        CompassTarget _currentTarget;
        bool _heldInShip;

        public CompassFrequency GetFrequency() => _frequency;

        public override string GetDisplayName() => GreenFlameBlade.Instance.NewHorizons.GetTranslationForUI("GFB_CompassTool");

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

            var heldInShip = PlayerState.AtFlightConsole();
            if (heldInShip != _heldInShip)
            {
                _heldInShip = heldInShip;
                if (heldInShip)
                {
                    transform.localPosition = new Vector3(0.1f, 0.65f, -0.1f);
                    transform.localEulerAngles = new Vector3(330f, 30f, 330f);
                }
                else
                {
                    transform.localPosition = new Vector3(0f, 0.3f, 0f);
                    transform.localEulerAngles = new Vector3(345f, 10f, 345f);
                }
            }

            _potentialTarget = null;

            var collider = Locator.GetToolModeSwapper()._firstPersonManipulator._lastHitCollider;
            if (collider != null)
            {
                _potentialTarget = collider.GetComponentInParent<CompassTarget>();
                if (_potentialTarget != null && !_potentialTarget.enabled)
                {
                    _potentialTarget = null;
                }
            }

            if (_scanPrompt == null)
            {
                _scanPrompt = new ScreenPrompt(InputLibrary.toolActionSecondary, "<CMD> " + GreenFlameBlade.Instance.NewHorizons.GetTranslationForUI("GFB_CompassToolScanPrompt"));
                Locator.GetPromptManager().AddScreenPrompt(_scanPrompt, PromptPosition.Center);
            }

            _scanPrompt.SetVisibility(isInItemToolMode && isInSuitMode && _potentialTarget != null);

            var isScanning = _potentialTarget != null && isInItemToolMode && OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary, InputMode.Character);
            if (isScanning)
            {
                _frequency = _potentialTarget.GetFrequency();
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
                Locator.GetPlayerAudioController().PlayProbeSnapshot();
                Locator.GetPlayerAudioController().PlayLockOn();
                var msgTemplate = GreenFlameBlade.Instance.NewHorizons.GetTranslationForUI("GFB_CompassToolNotification");
                var msgText = string.Format(msgTemplate, GreenFlameBlade.Instance.NewHorizons.GetTranslationForUI($"GFB_{nameof(CompassFrequency)}_{EnumUtils.GetName(_frequency)}"));
                if (_notificationData == null)
                {
                    _notificationData = new NotificationData(NotificationTarget.All, msgText);
                }
                _notificationData.displayMessage = msgText;
                NotificationManager.SharedInstance.PostNotification(_notificationData);
            }

            var targets = CompassManager.Get().GetTargets(_frequency);
            var minDist = float.PositiveInfinity;

            _currentTarget = null;

            foreach (var t in targets)
            {
                var dist = Vector3.Distance(t.GetTargetPosition(), transform.position) * t.GetFudgeFactor();
                if (dist < minDist)
                {
                    minDist = dist;
                    _currentTarget = t;
                }
            }

            if (_currentTarget != null)
            {
                _needleTargetRot = Quaternion.LookRotation(_currentTarget.GetTargetPosition() - _needle.transform.position, transform.up);
            }
            else
            {
                _needleTargetRot = Quaternion.LookRotation(transform.forward, transform.up);
            }
            _needle.transform.rotation = Quaternion.RotateTowards(_needle.transform.rotation, _needleTargetRot, 360f * Time.deltaTime);
        }
    }
}
