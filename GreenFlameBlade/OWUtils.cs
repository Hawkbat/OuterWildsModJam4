using UnityEngine;

namespace GreenFlameBlade
{
    public static class OWUtils
    {
        public static bool ShipLogFactKnown(string factID)
        {
            return PlayerData.GetShipLogFactSave(factID)?.revealOrder >= 0;
        }

        public static ReferenceFrameVolume AddReferenceFrame(GameObject obj, float radius, float minTargetRadius, float maxTargetRadius)
        {
            var go = new GameObject("RFVolume");
            go.transform.parent = obj.transform;
            go.transform.localPosition = Vector3.zero;
            go.layer = LayerMask.NameToLayer("ReferenceFrameVolume");
            go.SetActive(false);

            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = radius;
            var rf = new ReferenceFrame(obj.GetAttachedOWRigidbody())
            {
                _minSuitTargetDistance = minTargetRadius,
                _maxTargetDistance = maxTargetRadius,
                _autopilotArrivalDistance = radius,
                _autoAlignmentDistance = radius * 0.75f,
                _hideLandingModePrompt = false,
                _matchAngularVelocity = true,
                _minMatchAngularVelocityDistance = 70,
                _maxMatchAngularVelocityDistance = 400,
                _bracketsRadius = radius * 0.5f
            };

            var rfv = go.AddComponent<ReferenceFrameVolume>();
            rfv._referenceFrame = rf;
            rfv._minColliderRadius = minTargetRadius;
            rfv._maxColliderRadius = radius;
            rfv._isPrimaryVolume = false;
            rfv._isCloseRangeVolume = false;

            rf._useCenterOfMass = false;
            rf._localPosition = Vector3.zero;
            go.transform.localPosition = Vector3.zero;

            go.SetActive(true);

            return rfv;
        }
    }
}
