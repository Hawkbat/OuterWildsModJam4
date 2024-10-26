using UnityEngine;
using UnityEngine.Events;

namespace GreenFlameBlade.Components
{
    public class WraithDioramaChoiceOption : MonoBehaviour
    {
        [SerializeField] WraithDioramaChoice _nextChoice;
        WraithDiorama _diorama;

        public UnityEvent<WraithDioramaChoiceOption> OnChoiceSelected;
        public bool IsActivated() => _diorama.IsActivated();

        public void SetActivation(bool active) => _diorama.SetActivation(active);

        public WraithDioramaChoice GetNextChoice() => _nextChoice;

        void Awake()
        {
            _diorama = GetComponent<WraithDiorama>();
            _diorama.OnProximityTriggered.AddListener(OnProximityTriggered);
        }

        void OnProximityTriggered(WraithDiorama diorama)
        {
            OnChoiceSelected.Invoke(this);
        }
    }
}
