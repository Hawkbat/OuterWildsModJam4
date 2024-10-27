using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine;

namespace GreenFlameBlade
{
    public interface ICommonCameraAPI
    {
        void RegisterCustomCamera(OWCamera OWCamera);
        (OWCamera, Camera) CreateCustomCamera(string name);
        UnityEvent<PlayerTool> EquipTool();
        UnityEvent<PlayerTool> UnequipTool();
        void ExitCamera(OWCamera OWCamera);
        void EnterCamera(OWCamera OWCamera);
    }
}
