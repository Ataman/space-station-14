using System.Numerics;
using Content.Client.Buckle;
using Content.Client.Gravity;
using Content.Shared.ActionBlocker;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Physics.Components;

namespace Content.Client.Animations.StateMachine.AnimationStateConditions;

public sealed partial class AnimationStateIsWalkingCondition : AnimationStateCondition
{
    private EntityManager _entities = null!;
    private GravitySystem _gravitySystem = null!;
    private ActionBlockerSystem _actionBlockerSystem = null!;
    private BuckleSystem _buckleSystem = null!;
    private MobStateSystem _mobStateSystem = null!;
    private SharedMoverController _sharedMoverController = null!;

    private InputMoverComponent? _inputMoverComponent;
    private PhysicsComponent? _physicsComponent;

    public override void Initialize(EntityManager entityManager)
    {
        base.Initialize(entityManager);
        _entities = entityManager;
        _gravitySystem = entityManager.System<GravitySystem>();
        _actionBlockerSystem = entityManager.System<ActionBlockerSystem>();
        _buckleSystem = entityManager.System<BuckleSystem>();
        _mobStateSystem = entityManager.System<MobStateSystem>();
        _sharedMoverController = entityManager.System<SharedMoverController>();
    }

    protected override bool Evaluate(EntityUid entity)
    {
        if (_inputMoverComponent == null)
        {
            if (!_entities.TryGetComponent<InputMoverComponent>(entity, out var physics))
            {
                return false;
            }
            _inputMoverComponent = physics;
        }

        if (_physicsComponent == null)
        {
            if (!_entities.TryGetComponent<PhysicsComponent>(entity, out var input))
            {
                return false;
            }
            _physicsComponent = input;
        }

        var velocity = _sharedMoverController.GetVelocityInput(_inputMoverComponent);
        if (velocity.Walking == Vector2.Zero && velocity.Sprinting == Vector2.Zero)
            return false;

        if (!_inputMoverComponent.HasDirectionalMovement || !_inputMoverComponent.CanMove)
            return false;

        if (_physicsComponent.LinearVelocity.EqualsApprox(Vector2.Zero, 0.1f))
            return false;

        if (_gravitySystem.IsWeightless(entity))
            return false;

        if (!_actionBlockerSystem.CanMove(entity, _inputMoverComponent))
            return false;

        if (_buckleSystem.IsBuckled(entity))
            return false;

        if (_mobStateSystem.IsIncapacitated(entity))
            return false;

        return true;
    }
}
