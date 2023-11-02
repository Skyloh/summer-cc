using RobotPlatformer.Variables;
using UnityEngine;

namespace RobotPlatformer.Player.Behavior
{
    public sealed class DragBehavior : ABehavior
    {
        [Header("Contextual Variables")]
        [SerializeField] FloatVariable _frameDelta;

        [Header("Exposed Constants")]
        [SerializeField] float DragCoeff = 1200f;

        public override void Affect(Rigidbody body)
        {
            Vector3 velo = Utils.Flatten(body.velocity, false); // doesn't produce a linear drag bc some information is lost. No one will notice, though :)

            body.AddForce(DragCoeff * _frameDelta.Get() * -velo);
        }
    }
}