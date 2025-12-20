namespace Content.Client.Animations.StateMachine.AnimationStateTriggers;

[ImplicitDataDefinitionForInheritors]
public abstract partial class AnimationStateTrigger
{
    private AnimationStateMachineSystem _animationStateMachineSystem;
    private Entity<AnimationStateMachineComponent> _entity;
    private AnimationState _parentState;

    public virtual void Initialize(EntityManager entityManager, Entity<AnimationStateMachineComponent> entity, AnimationState state)
    {
        _animationStateMachineSystem = entityManager.System<AnimationStateMachineSystem>();
        _entity = entity;
        _parentState = state;
    }

    protected void Trigger()
    {
        _animationStateMachineSystem.OnTrigger(_parentState, _entity);
    }
}
