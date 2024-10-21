using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class CompassTarget : MonoBehaviour
    {
        [SerializeField] CompassFrequency _frequency;

        public CompassFrequency GetFrequency() => _frequency;

        public void SetFrequency(CompassFrequency frequency)
        {
            _frequency = frequency;
        }
    }
}
