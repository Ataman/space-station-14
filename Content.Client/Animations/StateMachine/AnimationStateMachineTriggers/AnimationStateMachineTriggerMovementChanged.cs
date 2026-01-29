using Content.Shared.Movement.Components;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineTriggers;

public sealed partial class AnimationStateMachineTriggerMovementChanged : AnimationStateMachineTriggerBase
{
    private EntityManager _entities = null!;
    private InputMoverComponent? _inputMoverComponent;

    private bool _wasWalking = false;
    private bool _wasSprinting = false;

    protected override void Initialize(EntityManager entityManager)
    {
        base.Initialize(entityManager);
        _entities = entityManager;
    }

    public override void TriggerIfNecessary(EntityUid entity)
    {
        base.TriggerIfNecessary(entity);
        if (_inputMoverComponent == null)
        {
            if (!_entities.TryGetComponent<InputMoverComponent>(entity, out var input))
            {
                return;
            }
            _inputMoverComponent = input;
        }

        if (_inputMoverComponent.HasDirectionalMovement && !_wasWalking ||
            _inputMoverComponent.Sprinting && !_wasSprinting)
        {
            Trigger();
        }

        _wasWalking = _inputMoverComponent.HasDirectionalMovement;
        _wasSprinting = _inputMoverComponent.Sprinting;
    }
}
