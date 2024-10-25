using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class DreamWraith : MonoBehaviour
    {
        const float BOB_SCALE = 0.25f;
        const float BOB_SPEED = 1f;
        const float WARP_DURATION = 0.5f;
        static readonly Vector3 WARP_SCALE = new(0f, 3f, 0f);

        static DreamWraith _instance;

        public static DreamWraith Get() => _instance;

        Transform _targetPoint;
        bool _warpingIn;
        bool _warpingOut;
        float _warpTime;
        float _bobTime;

        void Awake()
        {
            _instance = this;
        }

        void OnDestroy()
        {
            _instance = null;
        }

        void Update()
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
                }
            }

            transform.localPosition = Vector3.up * Mathf.Sin(_bobTime * Mathf.PI * 2 * BOB_SPEED) * BOB_SCALE;

            var plane = new Plane(transform.parent.up, transform.position);
            var lookAtPos = plane.ClosestPointOnPlane(Locator.GetPlayerTransform().position);
            var lookDir = (transform.position - lookAtPos).normalized;
            transform.rotation = Quaternion.LookRotation(lookDir, transform.parent.up);
        }

        public void Warp(Transform targetPoint)
        {
            _targetPoint = targetPoint;
            _warpingOut = true;
        }
    }
}
