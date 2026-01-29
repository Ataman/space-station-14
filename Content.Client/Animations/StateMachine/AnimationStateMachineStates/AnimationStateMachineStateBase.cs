using Content.Client.Animations.StateMachine.AnimationStateMachineConditions;
using Content.Client.Animations.StateMachine.AnimationStateMachineTriggers;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineStates;

[Serializable, ImplicitDataDefinitionForInheritors]
public abstract partial class AnimationStateMachineStateBase
{
    /// <summary>
    /// A collection of conditions that must be true for this state to activate.
    /// </summary>
    [DataField]
    public AnimationStateMachineConditionBase[] Conditions = [];

    /// <summary>
    /// A collection of triggers that cause a conditions check.
    /// </summary>
    [DataField]
    public AnimationStateMachineTrigger[] Triggers = [];

    /// <summary>
    /// If set to true, the state only executes its action one time instead of restarting it once the animation ends.
    /// </summary>
    [DataField]
    public bool OneShot;

    /// <summary>
    /// If set to non-zero, state exits automatically after the defined timespan.
    /// </summary>
    [DataField]
    public TimeSpan ExitPeriod = TimeSpan.Zero;

    /// <summary>
    /// Called once after creation of the state machine.
    /// Initialize your dependencies here.
    /// </summary>
    public abstract void Initialize(EntityUid ent, EntityManager entityManager);
    public abstract void Enter(EntityUid ent, bool enteredByTrigger);
    public abstract void Update(EntityUid ent, bool finished);
    public abstract void Exit(EntityUid ent);
}

public sealed partial class NullAnimationStateMachineStateBase : AnimationStateMachineStateBase
{
    public override void Initialize(EntityUid ent, EntityManager entityManager) { }
    public override void Enter(EntityUid ent, bool enteredByTrigger) { }
    public override void Update(EntityUid ent, bool finished) { }
    public override void Exit(EntityUid ent) { }
}
