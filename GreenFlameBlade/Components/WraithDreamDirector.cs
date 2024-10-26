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

        void OnEnterDreamWorld()
        {
            DreamWraith.Get().Warp(_wraithInitialPoint);
            _initialChoice.SetActivation(true);
        }

        void OnExitDreamWorld()
        {
            DreamWraith.Get().Warp(_wraithInitialPoint);
            foreach (var diorama in _dioramas) {
                diorama.SetActivation(false);
            }
            foreach (var choice in _choices)
            {
                choice.SetActivation(false);
            }
        }
    }
}
