using Content.Client.Buckle;
using Content.Client.Gravity;
using Content.Shared.ActionBlocker;
using Content.Shared.Buckle;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;

namespace Content.Client.Animations.StateMachine.AnimationStateConditions;

public sealed partial class AnimationStateIsWalkingCondition : AnimationStateCondition
{
    private EntityManager _entities = null!;
    private GravitySystem _gravitySystem = null!;
    private ActionBlockerSystem _actionBlockerSystem = null!;
    private BuckleSystem _buckleSystem = null!;
    private MobStateSystem _mobStateSystem = null!;

    private InputMoverComponent? _inputMoverComponent;

    public override void Initialize(EntityManager entityManager)
    {
        base.Initialize(entityManager);
        _entities = entityManager;
        _gravitySystem = entityManager.System<GravitySystem>();
        _actionBlockerSystem = entityManager.System<ActionBlockerSystem>();
        _buckleSystem = entityManager.System<BuckleSystem>();
        _mobStateSystem = entityManager.System<MobStateSystem>();
    }

    public override bool Evaluate(EntityUid entity)
    {
        if (_inputMoverComponent == null)
        {
            if (!_entities.TryGetComponent<InputMoverComponent>(entity, out var mover))
            {
                return false;
            }
            _inputMoverComponent = mover;
        }

        if (!_inputMoverComponent.HasDirectionalMovement)
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
