using System.Linq;
using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class WraithDioramaChoice : MonoBehaviour
    {
        const float ACTIVATION_WARP_DELAY = 0.5f;
        const float SELECTED_DEACTIVATION_DELAY = 0.5f;
        const float OPTION_ACTIVATION_DELAY = 0.25f;

        public delegate void DioramaChoiceEvent(WraithDioramaChoice target);

        public event DioramaChoiceEvent OnActivated;
        public event DioramaChoiceEvent OnDeactivated;

        [SerializeField] Transform _wraithTargetPoint;
        [SerializeField] string[] _requiredKnownFacts;
        [SerializeField] string[] _requiredUnknownFacts;
        WraithDiorama[] _options;
        bool _active;
        float _transitionTime;
        WraithDiorama _selected;

        public Transform GetWraithTarget() => _wraithTargetPoint;

        public bool IsActivated() => _active;

        public void SetActivation(bool active)
        {
            if (active != _active)
            {
                _active = active;
                _transitionTime = 0f;
                if (active)
                {
                    _selected = null;
                }
                enabled = true;
                if (active && OnActivated != null) OnActivated(this);
                if (!active && OnDeactivated != null) OnDeactivated(this);
            }
        }

        public void SetImmediateActivation(bool active)
        {
            if (active != _active)
            {
                _active = active;
                _transitionTime = OPTION_ACTIVATION_DELAY * _options.Length;
                if (active)
                {
                    _selected = null;
                }
                enabled = false;
                if (active && OnActivated != null) OnActivated(this);
                if (!active && OnDeactivated != null) OnDeactivated(this);
            }
        }

        public bool AreRequirementsMet()
        {
            if (_requiredKnownFacts.Length > 0 && _requiredKnownFacts.Any(f => !OWUtils.ShipLogFactKnown(f))) return false;
            if (_requiredUnknownFacts.Length > 0 && _requiredUnknownFacts.Any(f => OWUtils.ShipLogFactKnown(f))) return false;
            return true;
        }

        void Awake()
        {
            _options = GetComponentsInChildren<WraithDiorama>();
            foreach (var option in _options)
            {
                option.OnProximityTriggered += OnChoiceSelected;
            }
        }

        void OnDestroy()
        {
            foreach (var option in _options)
            {
                option.OnProximityTriggered -= OnChoiceSelected;
            }
        }

        void Update()
        {
            _transitionTime += Time.deltaTime;
            if (_active)
            {
                for (int i = 0; i < _options.Length; i++)
                {
                    if (_transitionTime >= ACTIVATION_WARP_DELAY + OPTION_ACTIVATION_DELAY * i)
                    {
                        _options[i].SetActivation(true);
                    }
                }

                if (_options.All(o => o.IsActivated()))
                {
                    enabled = false;
                }
            }
            else
            {
                if (_selected != null)
                {
                    _selected.SetActivation(false);
                }

                var delay = _selected != null ? SELECTED_DEACTIVATION_DELAY : 0f;
                for (int i = 0; i < _options.Length; i++)
                {
                    if (_options[i] == _selected) continue;
                    if (_transitionTime >= delay)
                    {
                        _options[i].SetActivation(false);
                    }
                    delay += OPTION_ACTIVATION_DELAY;
                }

                if (_options.All(o => !o.IsActivated()))
                {
                    enabled = false;
                }
            }
        }

        void OnChoiceSelected(WraithDiorama option)
        {
            if (!option.IsChoice()) return;
            _selected = option;
            SetActivation(false);
            var next = option.GetNextChoice();
            if (next != null && next.AreRequirementsMet())
            {
                next.SetActivation(true);
            }
        }
    }
}
