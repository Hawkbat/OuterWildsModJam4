using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class WraithDreamDirector : MonoBehaviour
    {
        [SerializeField] DreamWraith _dreamWraith;
        [SerializeField] Transform _wraithInitialPoint;
        [SerializeField] WraithDioramaChoice[] _initialChoices;
        [SerializeField] WraithDiorama[] _initialDioramas;
        [SerializeField] OWAudioSource _musicSource;

        WraithDiorama[] _dioramas;
        WraithDioramaChoice[] _choices;
        bool _inWraithDream;

        void Awake()
        {
            _dioramas = GetComponentsInChildren<WraithDiorama>();
            _choices = GetComponentsInChildren<WraithDioramaChoice>();
            foreach (var choice in _choices)
            {
                choice.OnActivated += OnChoiceActivated;
            }
            GlobalMessenger.AddListener(GlobalMessengerEvents.EnterWraithDream, OnEnterWraithDream);
            GlobalMessenger.AddListener(GlobalMessengerEvents.ExitWraithDream, OnExitWraithDream);
            GlobalMessenger.AddListener(GlobalMessengerEvents.EnterFakeEyeSequence, OnEnterFakeEyeSequence);
        }

        void OnDestroy()
        {
            foreach (var choice in _choices)
            {
                choice.OnActivated -= OnChoiceActivated;
            }
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.EnterWraithDream, OnEnterWraithDream);
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.ExitWraithDream, OnExitWraithDream);
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.EnterFakeEyeSequence, OnEnterFakeEyeSequence);
        }

        void Update()
        {
            if (_inWraithDream && Vector3.Distance(transform.position, Locator.GetPlayerTransform().position) > 200f)
            {
                Locator.GetPlayerBody().WarpToPositionRotation(transform.position + transform.up, transform.rotation);
            }
        }

        void OnEnterWraithDream()
        {
            _inWraithDream = true;
            _dreamWraith.Warp(_wraithInitialPoint, true);
            foreach (var choice in _initialChoices)
            {
                if (choice.AreRequirementsMet())
                {
                    choice.SetActivation(true);
                }
            }
            foreach (var diorama in _initialDioramas) diorama.SetActivation(true);
            _musicSource.FadeIn(0.25f);
        }

        void OnExitWraithDream()
        {
            _inWraithDream = false;
            _dreamWraith.WarpImmediate(_wraithInitialPoint);
            foreach (var diorama in _dioramas)
            {
                diorama.SetImmediateActivation(false);
            }
            foreach (var choice in _choices)
            {
                choice.SetImmediateActivation(false);
            }
            _musicSource.FadeOut(0.25f);
        }

        void OnEnterFakeEyeSequence()
        {
            _musicSource.FadeOut(0.25f);
        }

        void OnChoiceActivated(WraithDioramaChoice choice)
        {
            if (choice.GetWraithTarget() != null)
            {
                _dreamWraith.Warp(choice.GetWraithTarget());
            }
        }
    }
}
