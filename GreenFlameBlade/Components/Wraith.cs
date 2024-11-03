using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class Wraith : MonoBehaviour
    {
        const float WARP_DURATION = 0.5f;
        static readonly Vector3 WARP_SCALE = new(0f, 3f, 0f);

        Transform _targetPoint;
        bool _warpingOut;
        bool _warpingIn;
        float _warpTime;
        Vector3 _initialScale;
        bool _silentWarpOut;
        bool _silentWarpIn;

        void Start()
        {
            _initialScale = transform.localScale;
        }

        protected virtual void Update()
        {
            if (_warpingIn)
            {
                if (_warpTime == 0f && !_silentWarpIn)
                {
                    Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Exit);
                }
                _warpTime += Time.deltaTime;
                transform.localScale = Vector3.Scale(_initialScale, Vector3.Lerp(WARP_SCALE, Vector3.one, _warpTime / WARP_DURATION));
                if (_warpTime >= WARP_DURATION)
                {
                    _warpingIn = false;
                }
            }
            if (_warpingOut)
            {
                if (_warpTime == 0f && !_silentWarpOut)
                {
                    Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Enter);
                }
                _warpTime += Time.deltaTime;
                transform.localScale = Vector3.Scale(_initialScale, Vector3.Lerp(Vector3.one, WARP_SCALE, _warpTime / WARP_DURATION));
                if (_warpTime >= WARP_DURATION)
                {
                    _warpingOut = false;
                    _warpingIn = true;
                    _warpTime = 0f;
                    if (_targetPoint == null)
                    {
                        gameObject.SetActive(false);
                    }
                    else
                    {
                        transform.parent = _targetPoint;
                        transform.localPosition = Vector3.zero;
                        transform.localEulerAngles = Vector3.zero;
                    }
                }
            }
        }

        public void Warp(Transform targetPoint, bool silentWarpOut = false, bool silentWarpIn = false)
        {
            _targetPoint = targetPoint;
            _warpingOut = true;
            _warpTime = 0f;
            _silentWarpOut = silentWarpOut;
            _silentWarpIn = silentWarpIn;
        }

        public void WarpImmediate(Transform targetPoint)
        {
            _targetPoint = targetPoint;
            transform.parent = targetPoint;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            _warpTime = WARP_DURATION;
        }
    }
}
