using JetBrains.Annotations;

namespace Content.Client.Animations.StateMachine.AnimationStateConditions;

[ImplicitDataDefinitionForInheritors]
[PublicAPI]
public abstract partial class AnimationStateCondition
{
    internal bool LastResult = false;

    /// <summary>
    /// I couldn't get IoCManager.InjectDependencies to work, use this method to initialize them manually.
    /// </summary>
    /// <param name="entityManager"></param>
    public virtual void Initialize(EntityManager entityManager) { }

    internal bool EvaluateInternal(Entity<AnimationStateMachineComponent> ent)
    {
        LastResult = Evaluate(ent);
        return LastResult;
    }

    [PublicAPI]
    protected abstract bool Evaluate(EntityUid ent);
}
