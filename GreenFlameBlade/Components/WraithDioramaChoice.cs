using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class WraithDioramaChoice : MonoBehaviour
    {
        const float OPTION_ACTIVATION_DELAY = 0.5f;

        [SerializeField] Transform _wraithTargetPoint;
        WraithDioramaChoiceOption[] _options;
        bool _active;
        float _transitionTime;
        WraithDioramaChoiceOption _selected;

        public bool IsActivated() => _active;

        public void SetActivation(bool active)
        {
            if (active != _active)
            {
                _active = active;
                _transitionTime = 0f;
                _selected = null;
                enabled = true;
                gameObject.SetActive(true);
                if (_wraithTargetPoint != null)
                {
                    DreamWraith.Get().Warp(_wraithTargetPoint);
                }
            }
        }

        void Awake()
        {
            _options = GetComponentsInChildren<WraithDioramaChoiceOption>();
            foreach (var option in _options)
            {
                option.OnChoiceSelected.AddListener(OnChoiceSelected);
            }
        }

        void Update()
        {
            if (_active)
            {
                for (int i = 0; i < _options.Length; i++)
                {
                    if (_transitionTime >= OPTION_ACTIVATION_DELAY * i)
                    {
                        _options[i].SetActivation(true);
                    }
                }
            }
            else
            {
                var delay = 0f;
                for (int i = 0; i < _options.Length; i++)
                {
                    if (_options[i] != _selected) delay += OPTION_ACTIVATION_DELAY;
                    if (_options[i] == _selected || _transitionTime >= delay)
                    {
                        _options[i].SetActivation(false);
                    }
                }
            }
            if (_transitionTime >= OPTION_ACTIVATION_DELAY * (_options.Length - 1))
            {
                enabled = false;
                if (!_active)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        void OnChoiceSelected(WraithDioramaChoiceOption option)
        {
            SetActivation(false);
            var next = option.GetNextChoice();
            if (next != null)
            {
                next.SetActivation(true);
            }
        }
    }
}
