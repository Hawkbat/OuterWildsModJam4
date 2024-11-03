using UnityEngine;

namespace GreenFlameBlade.Components
{
    [ExecuteInEditMode]
    public class DebugPoseSequencer : MonoBehaviour
    {
        int _index = 0;
        OWCamera _owCamera;
        bool _allowCamera;
        bool _usingCamera;

        void Awake()
        {
            if (!Application.isPlaying) return;
            
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            transform.GetChild(_index).gameObject.SetActive(true);
            (_owCamera, _) = GreenFlameBlade.Instance.CommonCameraUtility.CreateCustomCamera("GFB_Pose_Debug");
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    if (child.name != i.ToString()) child.name = i.ToString();
                }
                return;
            }

            if (GreenFlameBlade.Instance.DebugMode && OWInput.IsPressed(InputLibrary.flashlight))
            {
                if (OWInput.IsNewlyPressed(InputLibrary.toolOptionRight))
                {
                    Forward();
                }
                if (OWInput.IsNewlyPressed(InputLibrary.toolOptionLeft))
                {
                    Back();
                }
                if (OWInput.IsNewlyPressed(InputLibrary.autopilot))
                {
                    _allowCamera = !_allowCamera;
                    UpdateCamera();
                }
            }
        }

        void Forward()
        {
            if (_index == transform.childCount - 1)
            {
                Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.GearRotate_Fail);
                return;
            }
            Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.GearRotate_Light);
            Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.Projector_Next);
            transform.GetChild(_index).gameObject.SetActive(false);
            _index++;
            transform.GetChild(_index).gameObject.SetActive(true);
            UpdateCamera();
        }

        void Back()
        {
            if (_index == 0)
            {
                Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.GearRotate_Fail);
                return;
            }
            Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.GearRotate_Light);
            Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.Projector_Prev);
            transform.GetChild(_index).gameObject.SetActive(false);
            _index--;
            transform.GetChild(_index).gameObject.SetActive(true);
            UpdateCamera();
        }

        void EnableCamera()
        {
            if (_usingCamera) return;
            GreenFlameBlade.Instance.CommonCameraUtility.EnterCamera(_owCamera);
            _usingCamera = true;
            Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.ToolProbeEquip);
        }

        void DisableCamera()
        {
            if (!_usingCamera) return;
            GreenFlameBlade.Instance.CommonCameraUtility.ExitCamera(_owCamera);
            _usingCamera = false;
            Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.ToolProbeUnequip);
        }

        void UpdateCamera()
        {
            if (!_allowCamera)
            {
                DisableCamera();
                return;
            }

            var activePose = transform.GetChild(_index);
            var cameraTarget = activePose.GetComponentInChildren<DebugCameraTarget>();
            if (cameraTarget != null)
            {
                EnableCamera();
                _owCamera.transform.parent = cameraTarget.transform;
                _owCamera.transform.localPosition = Vector3.zero;
                _owCamera.transform.localEulerAngles = Vector3.zero;
            }
            else
            {
                DisableCamera();
            }
        }
    }
}
