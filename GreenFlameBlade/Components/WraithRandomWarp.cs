using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class WraithRandomWarp : MonoBehaviour
    {
        [SerializeField] Wraith _wraith;
        [SerializeField] Transform[] _parents;
        [SerializeField] float _minDistance;
        [SerializeField] float _maxDistance;
        [SerializeField] float _minDelay;
        [SerializeField] float _maxDelay;
        [SerializeField] bool _spherizeRotation;
        [SerializeField] bool _randomizeRotation;

        Transform _currentTarget;
        Transform _nextTarget;
        float _timer;
        int _parentIndex;

        void Update()
        {
            _timer = Mathf.Max(0f, _timer - Time.deltaTime);
            if (_timer <= 0f)
            {
                if (_nextTarget == null)
                {
                    _nextTarget = new GameObject("Random Warp Target").transform;
                }

                var parent = _parents[_parentIndex];
                _nextTarget.transform.parent = parent.transform.parent;
                _nextTarget.transform.position = parent.position + Random.onUnitSphere * Random.Range(_minDistance, _maxDistance);
                _nextTarget.transform.localScale = Vector3.one;
                if (_spherizeRotation)
                {
                    _nextTarget.transform.rotation = Quaternion.LookRotation((_nextTarget.position - parent.position).normalized, parent.up);
                }
                else if (_randomizeRotation)
                {
                    _nextTarget.transform.rotation = Random.rotationUniform;
                }
                else
                {
                    _nextTarget.transform.localRotation = Quaternion.identity;
                }

                _wraith.Warp(_nextTarget, false);

                (_nextTarget, _currentTarget) = (_currentTarget, _nextTarget);

                _parentIndex = (_parentIndex + 1) % _parents.Length;
                _timer = Random.Range(_minDelay, _maxDelay);
            }
        }
    }
}
