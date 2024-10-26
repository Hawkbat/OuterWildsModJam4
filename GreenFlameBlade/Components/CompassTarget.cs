using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class CompassTarget : MonoBehaviour
    {
        public static CompassTarget Make(GameObject obj, CompassFrequency frequency) => Make(obj, frequency, Vector3.zero);
        public static CompassTarget Make(GameObject obj, CompassFrequency frequency, Vector3 targetOffset)
        {
            var target = obj.AddComponent<CompassTarget>();
            target._frequency = frequency;
            target._targetOffset = targetOffset;
            return target;
        }

        [SerializeField] Vector3 _targetOffset;
        [SerializeField] CompassFrequency _frequency;

        public CompassFrequency GetFrequency() => _frequency;

        public Vector3 GetTargetPosition() => transform.TransformPoint(_targetOffset);

        protected void OnEnable()
        {
            GreenFlameBlade.Instance.RegisterCompassTarget(this);
        }

        protected void OnDisable()
        {
            GreenFlameBlade.Instance.UnregisterCompassTarget(this);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            var p = GetTargetPosition();
            Gizmos.DrawLine(p + Vector3.left, p + Vector3.right);
            Gizmos.DrawLine(p + Vector3.down, p + Vector3.up);
            Gizmos.DrawLine(p + Vector3.back, p + Vector3.forward);
        }
    }
}
