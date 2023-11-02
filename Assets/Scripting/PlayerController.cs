using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// The idea here is to implement first, THEN optimize.
// Implement just movement first, refine that, then add more.
public class PlayerController : MonoBehaviour
{
    private const float deltaTimeScalar = 300f;
    private float Delta { get { return Time.fixedDeltaTime; } }

    private readonly float MAX_SPEED = 4f;
    private readonly float ACCELERATION = 25f * deltaTimeScalar; // exists as JammoAcceleration
    private readonly float ROTATION_LERP = 0.08f * deltaTimeScalar;
    private readonly float JUMP_FORCE = 30f;
    private readonly float DRAG = 4f * deltaTimeScalar;
    private readonly float SLIDE_ANGLE = 40f;

    [SerializeField] private Animator _animator;
    private int _speedHash;
    private float _prevSpeedValue;
    private int _jumpHash;
    private int _yVeloHash;
    private int _slidingHash;

    private InputMap _input;
    private Transform _perspective;
    private Rigidbody _rbody;
    private FloatingCollider _floatingCollider;

    private RaycastHit _info;

    private Vector3 _moveInput;
    private bool _isMovePressed;

    private bool _isJumpPressed;
    public bool _grounded; //exposed for testing
    private bool _onIllegalSlope;

    public bool testBool = true;

    private void Awake()
    {
        _input = new InputMap();
        _rbody = GetComponent<Rigidbody>();
        _floatingCollider = GetComponent<FloatingCollider>();
        _perspective = Camera.main.transform;

        _speedHash = Animator.StringToHash("Speed");
        _jumpHash = Animator.StringToHash("Grounded");
        _yVeloHash = Animator.StringToHash("YVelo");
        _slidingHash = Animator.StringToHash("Sliding");
    }

    private void OnEnable()
    {
        _input.CharacterControls.Enable();

        // Goon's Aside:
        // this looks more ugly than just having an exposed hook for a unity event.
        _input.CharacterControls.Move.started += OnMoveContext;
        _input.CharacterControls.Move.performed += OnMoveContext;
        _input.CharacterControls.Move.canceled += OnMoveContext;
        _input.CharacterControls.Jump.started += OnJumpContext;
    }

    private void OnDisable()
    {
        _input.CharacterControls.Move.started -= OnMoveContext;
        _input.CharacterControls.Move.performed -= OnMoveContext;
        _input.CharacterControls.Move.canceled -= OnMoveContext;
        _input.CharacterControls.Jump.started -= OnJumpContext;
        _input.CharacterControls.Disable();
    }

    private void FixedUpdate()
    {
        // this must be calculated first, because otherwise, the floating collider spring force overrides jump input
        Physics.SphereCast(new Ray(transform.position + Vector3.up, Vector3.down), 0.5f, out _info, 0.7f);

        bool didInitialSphereCastMiss = _info.collider == null;

        // we're on an illegal slope if we didn't miss but also if the slope is too slanted for us to stand on.
        float normalAngleFromUp = Vector3.Angle(Vector3.up, _info.normal);

        //_onIllegalSlope = !didInitialSphereCastMiss && angle > SLIDE_ANGLE;

        // if we aren't on a flat surface, we need to make sure we're not hanging on a corner.
        // to do this, we'll use a simple raycast offset by the spherecast's collision point to check if we're 
        // really on a slant. If we don't hit something, that means the place the spherecast hit is the edge of
        // a vertical face that doesn't slant toward us
        if (!didInitialSphereCastMiss && Mathf.Abs(normalAngleFromUp) > 1f) // if the normal is not nigh-perfectly vertical
        {
            Vector3 position = transform.position;
            Vector3 point = _info.point;

            float mixer = 0.8f;

            // ray origin is halfway between point and position on XZ.
            Vector3 origin = new Vector3(Mathf.Lerp(position.x, point.x, mixer), position.y + 1f, Mathf.Lerp(position.z, point.z, mixer));

            // NOTE:
            // used to have a !didInitialSphereCastMiss. The weird thing is that this entire block shouldnt run if there is no ground.
            // while Vector3's default is a 0 magnitude vector, i didnt want to take a chance of this somehow activating, so I added the miss check
            // to the If Statement.
            _onIllegalSlope = normalAngleFromUp > SLIDE_ANGLE && Physics.Raycast(origin, Vector3.down, out RaycastHit check, 1.5f) && Vector3.Dot(check.normal, _info.normal) > 0.9f;

            // Debug.DrawRay(origin, Vector3.down * 1.5f, Color.blue, 0.15f);

            // if the second conditional isnt there, we slide down legal slopes due to this force.
            if (!_onIllegalSlope && normalAngleFromUp > SLIDE_ANGLE)
            {
                // note that this force is also applied for a very brief period during the below mentioned "reassignment window"
                // it doesn't really matter because it's applying a subtle force in a direction you are already moving in, but 
                // it is important to document anyway.
                var duduoe = _info.normal;
                duduoe.y = 0f;
                _rbody.AddForce(duduoe.normalized * 35f, ForceMode.Force);
            }

        }
        else
        {
            // this needs to be here because there is a case where we transition from a slope to grounded too fast and miss the angle period where
            // we reassign onIllegalSlope (1>), but also where the slope is legal (SLIDE_ANGLE<). In our case, that makes the window 1 - 45 degrees.
            // As such, running into the slope can sometimes leave you sliding on the floor as you miss the reassignment-to-false window.
            _onIllegalSlope = false;
        }

        // sets us to grounded if we didn't miss a collision and the hit was within "grounded" range.
        // We are also grounded if we're on an illegal slope, but that has a caveat in that we can't jump.
        _grounded = (!didInitialSphereCastMiss && _info.distance < 0.6f) || _onIllegalSlope; // .6

        // saves on calculations; normalizes the current lateral velocity to 0 (no speed) and 1 (max speed)
        float normalizedLateralVelocityMagnitude = SquaredLateralVelocity() / Mathf.Pow(MAX_SPEED, 2f);

        // transforms directional input to camera space
        Vector3 perspectiveAlignedInput = FlatAlignFromPerspective(_moveInput);

        // how aligned the movement vector is with the current velocity direction.
        // used for amplifying unique movements for snappy controls.
        float inputSimilarityToVelocity = Vector3.Dot(perspectiveAlignedInput, _rbody.velocity.normalized);

        float slopeSimilarityToPerspInput = 1f;

        if (_onIllegalSlope)
        {
            // in reality, i'd want some sort of "sliding" animation for when the player is on an illegal slope and has depleted all their velocity
            // in the direction of the slope.

            // calculates the similarity between the normal's lateral direction vs the player's desired move direction this frame.
            //
            // PULLED TO SLOPE KEYSTONE --
            Vector3 flatNormalOfSlope = _info.normal;
            flatNormalOfSlope.y = 0f;
            slopeSimilarityToPerspInput = (Vector3.Dot(perspectiveAlignedInput, flatNormalOfSlope.normalized) + 1) / 2;
            // PULLED TO SLOPE KEYSTONE -- 
            //

            Vector3 downSlopeDirection = -(transform.forward - Vector3.Dot(transform.forward, _info.normal) * _info.normal).normalized;

            // if velo is moving downwards, invert the force direction. Since the velo is moving down, the trans.forward is facing
            // downwards as well, meaning our "downslope" vector is actually up-slope and therefore needs to be flipped.
            if (Vector3.Angle(downSlopeDirection, Vector3.down) > 89f)
            {
                downSlopeDirection *= -1f;
            }

            _animator.SetBool(_slidingHash, _rbody.velocity.y < -1f || Vector3.Dot(flatNormalOfSlope, transform.forward) > -0.1f); // TODO flatnormal normalized???

            // slope force to push player away from slope
            _rbody.AddForce(downSlopeDirection * 35f, ForceMode.Force);

            // slope behavior brain-dump:
            // this isn't totally correct behavior; i want jammo to not be able to jump if he's on an illegal surface, but i dont
            // want to make him "airborne" if he runs into a slope. I'd rather the slope just push back in that case.
            // however, this is a bit weird because the only way i can detect if im "running into" a slope versus being on it
            // is if the result of a direct down raycast hits something. I might be able to do something with the collision point,
            // though. Probably not, actually. In both cases, the collision point would be in the same spot. The only difference is that
            // in the first "on a slope" scenario, there is only one collision point: with the slope. In the other scenario, "running against
            // an illegal slope," there are two collision points, theoritically: the ground and the slope. 
        }
        else 
        {
            _animator.SetBool(_slidingHash, false); // bad
        }

        if (_isMovePressed && (inputSimilarityToVelocity < 0.1f || normalizedLateralVelocityMagnitude < 1f))
        {
            float inputSimilarityScalar = Mathf.Pow(Mathf.Abs(inputSimilarityToVelocity - 2), 1.5f);

            _rbody.AddForce(ACCELERATION * Delta * inputSimilarityScalar * slopeSimilarityToPerspInput 
                 * perspectiveAlignedInput);
        }

        if (_isJumpPressed)
        {
            // TODO REMOVE
            testBool = false; // TESTING ONLY
            // this mimics a state transition by totally locking out the floating collider.


            _isJumpPressed = false;

            float instantVelocity = Mathf.Sqrt(2f * 9.81f * 2.25f); // replace with pre-calculated value?

            // velocity change is just better than other types because it is the most consistent.
            // additionally, it allows me to concretely specify what the vertical velocity will always
            // be, unlike Impulse where I had to guess a little.
            _rbody.AddForce((instantVelocity - _rbody.velocity.y) * Vector3.up, ForceMode.VelocityChange);

        } 
        else
        {
            // if placed outside else block, more inconsistent results arise in regards to jump height.
            // if "true" is hardcoded into the argument, player gets stuck to ground for obvious reasons.
            // therefore, the spring force must be applied once the player is in contact with the ground.
            //
            // this must also be skipped when on a slope, as otherwise, the player is dragged into the ground
            // and shoved out again, resulting in rapid grounded/not-grounded switches and slow movement.
            _floatingCollider.UpdateForce(_grounded && !_onIllegalSlope);
            // TESTING ONLY
            //_floatingCollider.UpdateForce(testBool); // this seems to work. I think im finally ready?
            // the only bug im seeing is the sliding thing but that i can work out later.
            // Determine if Jammo really needs 2000 as his spring constant.
        }


        var vec = _rbody.velocity;
        vec.y = 0f;
        RotateTowardsMovement(vec, Delta);

        // high damp, low constant test
        // fails at higher values when always active, but at lower values is also not very good
        // it seems that having it always active (at low values) leads to a lot of rejumping, but
        // also rather consistent jump heights. They're lower, ofc, bc the spring is still active,
        // but they are sorta consistent. More consistent than earlier iterations (excluding current)
        // that is.
        // floatingCollider.UpdateForce(true);

        // apply drag
        Vector3 velo = _rbody.velocity;
        velo.y = 0f;
        _rbody.AddForce(DRAG * Delta * -velo);

        UpdateGroundedAnimator();
        UpdateYVeloAnimator(_rbody.velocity.y); // not accurate due to floating collider, but it seeems good enough..?
        UpdateAnimator(_moveInput.magnitude); // normalizedLateralVelocityMagnitude
    }

    private float SquaredLateralVelocity()
    {
        Vector3 velo = _rbody.velocity;
        return velo.x * velo.x + velo.z * velo.z;
    }

    private void UpdateAnimator(float value)
    {
        float v = Mathf.Lerp(_prevSpeedValue, value, 0.1f);
        _animator.SetFloat(_speedHash, v);
        _prevSpeedValue = v;
    }

    private void UpdateGroundedAnimator()
    {
        _animator.SetBool(_jumpHash, _grounded); // add onIllegalSlope here?
    }

    private void UpdateYVeloAnimator(float value)
    {
        _animator.SetFloat(_yVeloHash, value);
    }

    private void RotateTowardsMovement(Vector3 desiredPos, float deltaTime)
    {
        if (desiredPos == Vector3.zero)
        {
            return;
        }

        _rbody.MoveRotation(
            Quaternion.Slerp(
                transform.rotation, 
                Quaternion.LookRotation(desiredPos, Vector3.up), 
                ROTATION_LERP * deltaTime)); // not the "right way" to lerp, but it's rotation. who cares.

        // in the refactoring, make sure to have two types of rotation:
        // rotation of the model, and rotation of the rigidbody.
        // the first is used for purely visual rotation, whereas the second is used for
        // updating the Transform's cardinal vectors.
        //
        // see if you can find a way around this, though. It might be possible to "fake"
        // the rigidbody's rotation if you just store the information you need so that 
        // you don't need to keep track of the rotation of a parent-child gameobject scenario.
        // perhaps use a Quaternion field as the "internal" rotation and set the "visual" model
        // rotation to that internal field?
    }

    private void OnMoveContext(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        _moveInput = new Vector3(input.x, 0f, input.y);
        _isMovePressed = !context.canceled;
    }

    // only hooks into the STARTED invocation.
    private void OnJumpContext(InputAction.CallbackContext context)
    {
        _isJumpPressed = _grounded && !_onIllegalSlope; // pedantic, i think
    }

    private Vector3 FlatAlignFromPerspective(Vector3 vector)
    {
        Quaternion rotation = Quaternion.Euler(0f, _perspective.eulerAngles.y, 0f);
        return rotation * vector;
    }
}
