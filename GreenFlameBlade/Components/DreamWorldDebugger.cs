using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class DreamWorldDebugger : MonoBehaviour
    {
        bool _inDreamWorld;
        bool _debugging;
        DreamLanternItem _lantern;
        float _intensity;
        float _range;

        OWLight2 GetAmbientLight() => _lantern._lights[1];

        void OnEnable()
        {
            GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
            GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
        }

        void OnDisable()
        {
            GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
            GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
        }

        void OnEnterDreamWorld()
        {
            _inDreamWorld = true;
            _lantern = Locator.GetDreamWorldController().GetPlayerLantern();
            var ambLight = GetAmbientLight();
            _intensity = ambLight._gameplayIntensity;
            _range = ambLight.range;
        }

        void OnExitDreamWorld()
        {
            _inDreamWorld = false;
            EndDebug();
        }

        void StartDebug()
        {
            _debugging = true;
            var ambLight = GetAmbientLight();
            ambLight.SetIntensity(1f);
            ambLight.range = 2000f;
            Locator.GetPlayerAudioController().PlayTurnOnFlashlight();
        }

        void EndDebug()
        {
            _debugging = false;
            var ambLight = GetAmbientLight();
            ambLight.SetIntensity(_intensity);
            ambLight.range = _range;
            Locator.GetPlayerAudioController().PlayTurnOffFlashlight();
        }

        void Update()
        {
            if (_inDreamWorld)
            {
                if (GreenFlameBlade.Instance.DebugMode && OWInput.IsNewlyPressed(InputLibrary.flashlight))
                {
                    if (_debugging) EndDebug();
                    else StartDebug();
                }
            }
        }
    }
}
