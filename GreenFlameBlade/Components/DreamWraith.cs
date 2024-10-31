using UnityEngine;

namespace GreenFlameBlade.Components
{
    public class DreamWraith : Wraith
    {
        const float BOB_SCALE = 0.1f;
        const float BOB_SPEED = 0.1f;

        float _bobTime;

        protected override void Update()
        {
            base.Update();

            _bobTime += Time.deltaTime;
            transform.localPosition = Vector3.up * Mathf.Sin(_bobTime * Mathf.PI * 2 * BOB_SPEED) * BOB_SCALE;

            var plane = new Plane(transform.parent ? transform.parent.up : transform.up, transform.position);
            var lookAtPos = plane.ClosestPointOnPlane(Locator.GetPlayerTransform().position);
            var lookDir = (lookAtPos - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(lookDir, transform.parent.up);
        }
    }
}
