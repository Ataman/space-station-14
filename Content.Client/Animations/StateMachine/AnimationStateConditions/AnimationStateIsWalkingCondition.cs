using System.Numerics;
using Content.Client.Buckle;
using Content.Client.Gravity;
using Content.Shared.ActionBlocker;
using Content.Shared.Buckle;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
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
    private Robust.Client.Physics.PhysicsSystem _physics = null!;
    private ISawmill _sawmill;

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
        _physics = entityManager.System<Robust.Client.Physics.PhysicsSystem>();
        _sawmill = Logger.GetSawmill("asm");
    }

    protected override bool Evaluate(EntityUid entity)
    {
        //if (!_entities.TryGetComponent<InputMoverComponent>(entity, out var mover))
        //    return false;

        if (_physicsComponent == null)
        {
            if (!_entities.TryGetComponent<PhysicsComponent>(entity, out var physics))
            {
                return false;
            }
            _physicsComponent = physics;
        }

        //_sawmill.Debug($"HasDirectionalMovement = {mover.HasDirectionalMovement}, CanMove = {mover.CanMove}");

        //if (_sharedMoverController.GetVelocityInput(mover).Sprinting == Vector2.Zero)
        //    return false;

        //if (!mover.HasDirectionalMovement || !mover.CanMove)
        //    return false;


        if (_physicsComponent.LinearVelocity == Vector2.Zero)
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
