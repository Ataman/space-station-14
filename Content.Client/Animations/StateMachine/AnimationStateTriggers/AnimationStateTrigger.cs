using JetBrains.Annotations;

namespace Content.Client.Animations.StateMachine.AnimationStateTriggers;

[ImplicitDataDefinitionForInheritors]
[PublicAPI]
public abstract partial class AnimationStateTrigger
{
    private AnimationStateMachineSystem _animationStateMachineSystem;
    private Entity<AnimationStateMachineComponent> _entity;
    private AnimationState _parentState;
    private bool _triggered = false;

    internal void InitializeInternal(EntityManager entityManager, Entity<AnimationStateMachineComponent> entity, AnimationState state)
    {
        _animationStateMachineSystem = entityManager.System<AnimationStateMachineSystem>();
        _entity = entity;
        _parentState = state;
        Initialize(entityManager);
    }

    [PublicAPI]
    protected virtual void Initialize(EntityManager entityManager) { }

    public virtual void TriggerIfNecessary(EntityUid entity) { }

    internal bool TriggerIfNecessaryInternal(EntityUid entity)
    {
        _triggered = false;
        TriggerIfNecessary(entity);
        return _triggered;
    }

    [PublicAPI]
    protected void Trigger()
    {
        _triggered = true;
        _animationStateMachineSystem.OnTrigger(_parentState, _entity);
    }
}
