using UnityEngine;

namespace GreenFlameBlade.Components
{
    [RequireComponent(typeof(OWAudioSource))]
    public class WraithDioramaSound : MonoBehaviour
    {
        [SerializeField] WraithDiorama _diorama;
        [SerializeField] bool _playOnActivate;
        [SerializeField] bool _playOnDeactivate;
        [SerializeField] bool _playOnProximityTrigger;
        OWAudioSource _audioSource;

        void Awake()
        {
            _audioSource = GetComponent<OWAudioSource>();
            _diorama.OnActivated += OnActivated;
            _diorama.OnDeactivated += OnDeactivated;
            _diorama.OnProximityTriggered += OnProximityTriggered;
        }

        void OnDestroy()
        {
            _diorama.OnActivated -= OnActivated;
            _diorama.OnDeactivated -= OnDeactivated;
            _diorama.OnProximityTriggered -= OnProximityTriggered;
        }

        void OnActivated(WraithDiorama target)
        {
            if (_playOnActivate) _audioSource.Play();
        }

        void OnDeactivated(WraithDiorama target)
        {
            if (_playOnDeactivate) _audioSource.Play();
        }

        void OnProximityTriggered(WraithDiorama target)
        {
            if (_playOnProximityTrigger) _audioSource.Play();
        }
    }
}
