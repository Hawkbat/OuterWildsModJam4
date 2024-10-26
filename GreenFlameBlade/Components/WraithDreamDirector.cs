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
        [SerializeField] WraithDioramaChoice _initialChoice;

        WraithDiorama[] _dioramas;
        WraithDioramaChoice[] _choices;
        bool _inWraithDream;

        void Awake()
        {
            _dioramas = GetComponentsInChildren<WraithDiorama>();
            _choices = GetComponentsInChildren<WraithDioramaChoice>();
            GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
            GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
        }

        void OnDestroy()
        {
            GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
            GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
        }

        void Update()
        {
            if (_inWraithDream && Vector3.Distance(transform.position, Locator.GetPlayerTransform().position) > 100f)
            {
                Locator.GetPlayerBody().WarpToPositionRotation(transform.position + transform.up, transform.rotation);
            }
        }

        void OnEnterDreamWorld()
        {
            _inWraithDream = Locator.GetDreamWorldController()._dreamArrivalPoint.transform.root.name == "RingedPlanet_Body";
            if (_inWraithDream)
            {
                DreamWraith.Get().Warp(_wraithInitialPoint, false);
                _initialChoice.SetActivation(true);
            }
        }

        void OnExitDreamWorld()
        {
            if (_inWraithDream)
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
}
