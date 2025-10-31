using System.Runtime.CompilerServices;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineConditions;

[ImplicitDataDefinitionForInheritors]
public abstract partial class AnimationStateMachineConditionBase
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

    protected abstract bool Evaluate(EntityUid ent);
}
