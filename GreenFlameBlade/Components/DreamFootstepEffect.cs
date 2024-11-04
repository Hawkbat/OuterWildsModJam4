using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class DreamFootstepEffect : MonoBehaviour
    {
        ParticleSystem[] _particleSystems;
        bool _inWraithDream;

        void Awake()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
            GlobalMessenger.AddListener(GlobalMessengerEvents.EnterWraithDream, OnEnterWraithDream);
            GlobalMessenger.AddListener(GlobalMessengerEvents.ExitWraithDream, OnExitWraithDream);
        }

        void OnDestroy()
        {
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.EnterWraithDream, OnEnterWraithDream);
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.ExitWraithDream, OnExitWraithDream);
        }

        void LateUpdate()
        {
            transform.position = Locator.GetPlayerTransform().position;
            transform.rotation = Locator.GetPlayerTransform().rotation;
            var isGrounded = Locator.GetPlayerController().IsGrounded();
            var shouldEmit = _inWraithDream && isGrounded;

            foreach (var ps in _particleSystems)
            {
                var emission = ps.emission;
                emission.enabled = shouldEmit;
            }
        }

        void OnEnterWraithDream()
        {
            _inWraithDream = true;
            enabled = true;
        }

        void OnExitWraithDream()
        {
            _inWraithDream = false;
            enabled = false;
        }
    }
}
