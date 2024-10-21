using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class CompassTool : OWItem
    {
        public static ItemType ITEM_TYPE = EnumUtils.Create<ItemType>("Compass");

        CompassFrequency _frequency;

        [SerializeField] MeshRenderer _screenRenderer;
        [SerializeField] OWCamera _snapshotCamera;
        RenderTexture _snapshotTexture;
        ScreenPrompt _scanPrompt;

        public CompassFrequency GetFrequency() => _frequency;

        public override string GetDisplayName() => GreenFlameBlade.Instance.NewHorizons.GetTranslationForUI("CompassTool");

        public override void Awake()
        {
            _type = ITEM_TYPE;
            base.Awake();
        }

        public void Update()
        {
            var isHeld = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ITEM_TYPE;
            var isInItemToolMode = OWInput.IsInputMode(InputMode.Character) && Locator.GetToolModeSwapper().GetToolMode() == ToolMode.Item;
            
            if (!isHeld || !isInItemToolMode) {
                if (_scanPrompt != null) _scanPrompt.SetVisibility(false);
                return;
            }
            
            var isScanning = isInItemToolMode && OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary, InputMode.Character);
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
                _screenRenderer.material.mainTexture = _snapshotTexture;
                Locator.GetPlayerAudioController().PlayEnterLaunchCodes();
            }

            if (_scanPrompt == null) _scanPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, GreenFlameBlade.Instance.NewHorizons.GetTranslationForUI("CompassToolScanPrompt") + "   <CMD>");

            _scanPrompt.SetVisibility(true);
        }
    }
}
