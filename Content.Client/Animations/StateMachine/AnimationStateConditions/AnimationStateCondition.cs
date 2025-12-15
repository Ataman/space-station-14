namespace Content.Client.Animations.StateMachine.AnimationStateConditions;

[ImplicitDataDefinitionForInheritors]
public abstract partial class AnimationStateCondition
{
    /// <summary>
    /// I couldn't get IoCManager.InjectDependencies to work, use this method to initialize them manually.
    /// </summary>
    /// <param name="entityManager"></param>
    public virtual void Initialize(EntityManager entityManager) { }

    public abstract bool Evaluate(EntityUid entity);
}
