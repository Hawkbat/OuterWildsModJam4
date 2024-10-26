using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class CampfireCompassTargetController : MonoBehaviour
    {
        CompassTarget _target;
        Campfire _campfire;

        public void SetCampfire(Campfire campfire)
        {
            _campfire = campfire;
            _target.enabled = campfire.GetState() == Campfire.State.LIT;
            campfire.OnCampfireStateChange += OnCampfireStateChange;
        }

        void Awake()
        {
            _target = GetComponent<CompassTarget>();
        }

        void OnDestroy()
        {
            if (_campfire != null)
            {
                _campfire.OnCampfireStateChange -= OnCampfireStateChange;
                _campfire = null;
            }
        }

        void OnCampfireStateChange(Campfire fire)
        {
            _target.enabled = _campfire.GetState() == Campfire.State.LIT;
        }
    }
}
