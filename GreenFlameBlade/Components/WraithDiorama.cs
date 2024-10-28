using UnityEngine;
using UnityEngine.Events;

namespace GreenFlameBlade.Components
{
    public class WraithDiorama : MonoBehaviour
    {
        const float TRANSITION_DURATION = 0.5f;
        static readonly Vector3 TRANSITION_SCALE = new(0f, 3f, 0f);

        public delegate void ProximityEvent(WraithDiorama target);

        public event ProximityEvent OnProximityTriggered;

        [SerializeField] bool _proximityTriggerEnabled;
        [SerializeField] float _proximityTriggerRadius = 5f;
        [SerializeField] bool _isChoice;
        [SerializeField] WraithDioramaChoice _nextChoice;
        [SerializeField] bool _exitDream;
        [SerializeField] string[] _revealOnActivate;
        [SerializeField] string[] _revealOnProximityTrigger;

        bool _active;
        float _transitionProgress;
        bool _proximityTriggerFired;
        Vector3 _initialScale;

        public bool IsChoice() => _isChoice;

        public WraithDioramaChoice GetNextChoice() => _nextChoice;

        public bool IsDreamExit() => _exitDream;

        public bool IsActivated() => _active;

        public void SetActivation(bool active)
        {
            if (active != _active)
            {
                _active = active;
                _transitionProgress = 0f;
                _proximityTriggerFired = false;
                if (active)
                {
                    Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Exit);
                    foreach (var factID in _revealOnActivate)
                    {
                        Locator.GetShipLogManager().RevealFact(factID);
                    }
                }
                else
                {
                    Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.LoadingZone_Enter);
                }
                enabled = true;
                gameObject.SetActive(true);
            }
        }

        public void SetImmediateActivation(bool active)
        {
            _active = active;
            _transitionProgress = 1f;
            _proximityTriggerFired = false;
            enabled = false;
            transform.localScale = active ? _initialScale : Vector3.Scale(_initialScale, TRANSITION_SCALE);
            gameObject.SetActive(active);
            if (active)
            {
                foreach (var factID in _revealOnActivate)
                {
                    Locator.GetShipLogManager().RevealFact(factID);
                }
            }
        }

        void Start()
        {
            _initialScale = transform.localScale;
            SetImmediateActivation(false);
        }

        void Update()
        {
            if (_transitionProgress < 1f)
            {
                _transitionProgress = Mathf.Clamp01(_transitionProgress + Time.deltaTime / TRANSITION_DURATION);
                transform.localScale = Vector3.Scale(_initialScale, Vector3.Lerp(TRANSITION_SCALE, Vector3.one, _active ? _transitionProgress : 1f - _transitionProgress));
                if (!_active && _transitionProgress >= 1f)
                {
                    enabled = false;
                    gameObject.SetActive(false);
                }
            }
            if (_proximityTriggerEnabled && !_proximityTriggerFired)
            {
                var playerT = Locator.GetPlayerTransform();
                if (Vector3.Distance(playerT.position, transform.position) < _proximityTriggerRadius)
                {
                    _proximityTriggerFired = true;
                    foreach (var factID in _revealOnProximityTrigger)
                    {
                        Locator.GetShipLogManager().RevealFact(factID);
                    }
                    if (OnProximityTriggered != null)
                    {
                        OnProximityTriggered(this);
                    }
                    if (_exitDream)
                    {
                        GlobalMessenger.FireEvent(GlobalMessengerEvents.ExitWraithDream);
                    }
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!_proximityTriggerEnabled) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _proximityTriggerRadius);
        }
    }
}
