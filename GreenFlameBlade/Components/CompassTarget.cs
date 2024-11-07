using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class CompassTarget : MonoBehaviour
    {
        [SerializeField] Vector3 _targetOffset;
        [SerializeField] CompassFrequency _frequency;
        [SerializeField] float _fudgeFactor = 1f;
        bool _cloaked;
        Campfire _campfire;
        SimpleLanternItem _simpleLantern;
        SlideReelSocket _slideReelSocket;
        bool _registerWhenReady;

        public CompassFrequency GetFrequency() => _frequency;

        public void SetFrequency(CompassFrequency frequency) => _frequency = frequency;

        public float GetFudgeFactor() => _fudgeFactor;

        public void SetFudgeFactor(float fudgeFactor) => _fudgeFactor = fudgeFactor;

        public Vector3 GetTargetOffset() => _targetOffset;

        public void SetTargetOffset(Vector3 targetOffset) => _targetOffset = targetOffset;

        public Vector3 GetTargetPosition() => transform.TransformPoint(_targetOffset);

        public bool IsCloaked() => _cloaked;

        void Awake()
        {
            _cloaked = transform.root.name == "RingWorld_Body";
            _campfire = GetComponentInChildren<Campfire>();
            if (_campfire != null)
            {
                _campfire.OnCampfireStateChange += OnCampfireStateChange;
                enabled = enabled && _campfire.GetState() == Campfire.State.LIT;
            }
            _simpleLantern = GetComponent<SimpleLanternItem>();
            if (_simpleLantern != null)
            {
                _simpleLantern.OnLanternExtinguished += OnLanternExtinguished;
                enabled = enabled && _simpleLantern.IsLit();
            }
            _slideReelSocket = GetComponent<SlideReelSocket>();
            if (_slideReelSocket != null)
            {
                _slideReelSocket.OnSocketablePlaced += OnSocketablePlaced;
                _slideReelSocket.OnSocketableRemoved += OnSocketableRemoved;
                enabled = enabled && _slideReelSocket.IsSocketOccupied();
            }
        }

        void OnDestroy()
        {
            if (_campfire != null)
            {
                _campfire.OnCampfireStateChange -= OnCampfireStateChange;
            }
            if (_simpleLantern != null)
            {
                _simpleLantern.OnLanternExtinguished -= OnLanternExtinguished;
            }
            if (_slideReelSocket != null)
            {
                _slideReelSocket.OnSocketablePlaced -= OnSocketablePlaced;
                _slideReelSocket.OnSocketableRemoved -= OnSocketableRemoved;
            }
        }

        void OnEnable()
        {
            if (!CompassManager.Get())
            {
                _registerWhenReady = true;
                return;
            }
            CompassManager.Get().RegisterTarget(this);
        }

        void OnDisable()
        {
            _registerWhenReady = false;
            CompassManager.Get().UnregisterTarget(this);
        }

        void Update()
        {
            if (_registerWhenReady && CompassManager.Get())
            {
                CompassManager.Get().RegisterTarget(this);
                _registerWhenReady = false;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            var p = GetTargetPosition();
            Gizmos.DrawLine(p + Vector3.left, p + Vector3.right);
            Gizmos.DrawLine(p + Vector3.down, p + Vector3.up);
            Gizmos.DrawLine(p + Vector3.back, p + Vector3.forward);
        }

        void OnCampfireStateChange(Campfire fire)
        {
            enabled = fire.GetState() == Campfire.State.LIT;
        }

        void OnLanternExtinguished()
        {
            enabled = _simpleLantern.IsLit();
        }

        void OnSocketablePlaced(OWItem item)
        {
            enabled = true;
        }

        void OnSocketableRemoved(OWItem item)
        {
            enabled = false;
        }
    }
}
