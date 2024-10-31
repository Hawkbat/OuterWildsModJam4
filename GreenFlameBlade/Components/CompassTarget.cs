using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace GreenFlameBlade.Components
{
    public class CompassTarget : MonoBehaviour
    {
        static List<CompassTarget> _activeTargets = [];

        public static CompassTarget Make(GameObject obj, CompassFrequency frequency) => Make(obj, frequency, Vector3.zero);
        public static CompassTarget Make(GameObject obj, CompassFrequency frequency, Vector3 targetOffset)
        {
            var target = obj.AddComponent<CompassTarget>();
            target._frequency = frequency;
            target._targetOffset = targetOffset;
            return target;
        }

        public static IEnumerable<CompassTarget> GetTargets(CompassFrequency frequency) => GetTargets().Where(t => t.GetFrequency() == frequency);

        public static IEnumerable<CompassTarget> GetTargets() => _activeTargets.Where(t => PlayerState.InCloakingField() == t._cloaked);

        [SerializeField] Vector3 _targetOffset;
        [SerializeField] CompassFrequency _frequency;
        bool _cloaked;
        Campfire _campfire;
        SimpleLanternItem _simpleLantern;

        public CompassFrequency GetFrequency() => _frequency;

        public Vector3 GetTargetPosition() => transform.TransformPoint(_targetOffset);

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
        }

        void OnEnable()
        {
            _activeTargets.Add(this);
        }

        void OnDisable()
        {
            _activeTargets.Remove(this);
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
    }
}
