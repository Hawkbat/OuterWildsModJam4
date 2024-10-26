using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class Wraith : MonoBehaviour
    {
        const float WARP_DURATION = 0.5f;
        static readonly Vector3 WARP_SCALE = new(0f, 3f, 0f);

        Transform _targetPoint;
        bool _warpingIn;
        bool _warpingOut;
        float _warpTime;

        protected virtual void Update()
        {
            if (_warpingIn)
            {
                if (_warpTime == 0f)
                {
                    Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Exit);
                }
                _warpTime += Time.deltaTime;
                transform.localScale = Vector3.Lerp(WARP_SCALE, Vector3.one, _warpTime / WARP_DURATION);
                if (_warpTime >= WARP_DURATION)
                {
                    _warpingIn = false;
                }
            }
            if (_warpingOut)
            {
                if (_warpTime == 0f)
                {
                    Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Enter);
                }
                _warpTime += Time.deltaTime;
                transform.localScale = Vector3.Lerp(Vector3.one, WARP_SCALE, _warpTime / WARP_DURATION);
                if (_warpTime >= WARP_DURATION)
                {
                    _warpingOut = false;
                    _warpingIn = true;
                    _warpTime = 0f;
                    transform.parent = _targetPoint;
                    transform.localPosition = Vector3.zero;
                    transform.localEulerAngles = Vector3.zero;
                }
            }
        }

        public void Warp(Transform targetPoint, bool immedate)
        {
            _targetPoint = targetPoint;
            if (immedate)
            {
                transform.parent = targetPoint;
                transform.localPosition = Vector3.zero;
                transform.localEulerAngles = Vector3.zero;
                _warpTime = WARP_DURATION;
            }
            else
            {
                _warpingOut = true;
                _warpTime = 0f;
            }
        }
    }
}
