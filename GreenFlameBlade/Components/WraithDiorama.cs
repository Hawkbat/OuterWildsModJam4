using UnityEngine;
using UnityEngine.Events;

namespace GreenFlameBlade.Components
{
    public class WraithDiorama : MonoBehaviour
    {
        const float TRANSITION_DURATION = 1f;
        static readonly Vector3 TRANSITION_SCALE = new(0f, 3f, 0f);

        public UnityEvent<WraithDiorama> OnProximityTriggered;

        [SerializeField] float _proximityTriggerRadius = 15f;
        bool _active;
        float _transitionTime;
        bool _proximityTriggerFired;

        public bool IsActivated() => _active;

        public void SetActivation(bool active)
        {
            if (active != _active)
            {
                _active = active;
                _transitionTime = 0f;
                if (active)
                {
                    _proximityTriggerFired = false;
                    Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Exit);
                }
                else
                {
                    Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Enter);
                }
                enabled = true;
                gameObject.SetActive(true);
            }
        }

        void Start()
        {
            enabled = false;
            gameObject.SetActive(false);
            transform.localScale = TRANSITION_SCALE;
        }

        void Update()
        {
            if (_transitionTime < 1f)
            {
                _transitionTime = Mathf.Clamp01(_transitionTime + Time.deltaTime / TRANSITION_DURATION);
                transform.localScale = Vector3.Lerp(Vector3.one, TRANSITION_SCALE, _active ? _transitionTime : 1f - _transitionTime);
                if (!_active && _transitionTime >= 1f)
                {
                    enabled = false;
                    gameObject.SetActive(false);
                }
            }
            if (!_proximityTriggerFired)
            {
                var playerT = Locator.GetPlayerTransform();
                if (Vector3.Distance(playerT.position, transform.position) < _proximityTriggerRadius)
                {
                    _proximityTriggerFired = true;
                    OnProximityTriggered.Invoke(this);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _proximityTriggerRadius);
        }
    }
}
