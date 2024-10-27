using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class WraithDreamDirector : MonoBehaviour
    {
        [SerializeField] Transform _wraithInitialPoint;
        [SerializeField] WraithDioramaChoice[] _initialChoices;
        [SerializeField] WraithDiorama[] _initialDioramas;

        WraithDiorama[] _dioramas;
        WraithDioramaChoice[] _choices;
        bool _inWraithDream;

        void Awake()
        {
            _dioramas = GetComponentsInChildren<WraithDiorama>();
            _choices = GetComponentsInChildren<WraithDioramaChoice>();
            GlobalMessenger.AddListener(GlobalMessengerEvents.EnterWraithDream, OnEnterWraithDream);
            GlobalMessenger.AddListener(GlobalMessengerEvents.ExitWraithDream, OnExitWraithDream);
        }

        void OnDestroy()
        {
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.EnterWraithDream, OnEnterWraithDream);
            GlobalMessenger.RemoveListener(GlobalMessengerEvents.ExitWraithDream, OnExitWraithDream);
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
            DreamWraith.Get().Warp(_wraithInitialPoint, false);
            foreach (var choice in _initialChoices) choice.SetActivation(true);
            foreach (var diorama in _initialDioramas) diorama.SetActivation(true);
        }

        void OnExitWraithDream()
        {
            _inWraithDream = false;
            DreamWraith.Get().Warp(_wraithInitialPoint, true);
            foreach (var diorama in _dioramas)
            {
                diorama.SetImmediateActivation(false);
            }
            foreach (var choice in _choices)
            {
                choice.SetImmediateActivation(false);
            }
        }
    }
}
