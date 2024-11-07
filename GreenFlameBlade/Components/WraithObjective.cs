using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class WraithObjective : MonoBehaviour
    {
        const float WARP_DELAY = 1f;

        [SerializeField] string _conditionName;
        Wraith _wraith;
        bool _inDream;
        bool _warpingIn;
        float _warpDelay;

        void Awake()
        {
            _wraith = GetComponentInChildren<Wraith>();
            GlobalMessenger.AddListener(GlobalMessengerEvents.EnterWraithDream, OnEnterWraithDream);
            GlobalMessenger.AddListener(GlobalMessengerEvents.ExitWraithDream, OnExitWraithDream);
            GlobalMessenger.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);
        }

        void OnDestroy()
        {
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.EnterWraithDream, OnEnterWraithDream);
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.ExitWraithDream, OnExitWraithDream);
            GlobalMessenger.RemoveListener("DialogueConditionChanged", OnDialogueConditionChanged);
        }

        void Update()
        {
            if (_warpingIn)
            {
                _warpDelay -= Time.deltaTime;
                if (_warpDelay < 0f)
                {
                    _wraith.Warp(transform, true, false);
                    _warpingIn = false;
                }
            }
        }

        void UpdateVisibility()
        {
            var visible = _inDream && DialogueConditionManager.SharedInstance.GetConditionState(_conditionName);
            if (visible)
            {
                _warpingIn = true;
                _warpDelay = WARP_DELAY;
            }
            else
            {
                _warpingIn = false;
                _wraith.WarpImmediate(null);
            }
        }

        void OnEnterWraithDream()
        {
            _inDream = true;
            UpdateVisibility();
        }

        void OnExitWraithDream()
        {
            _inDream = false;
            UpdateVisibility();
        }

        void OnDialogueConditionChanged()
        {
            if (_inDream)
            {
                UpdateVisibility();
            }
        }
    }
}
