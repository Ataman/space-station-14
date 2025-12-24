using Content.Client.Animations.StateMachine.AnimationStateMachineConditions;
using Content.Client.Animations.StateMachine.AnimationStateMachineStates;
using Content.Client.Animations.StateMachine.AnimationStateMachineTimers;
using Content.Client.Animations.StateMachine.AnimationStateMachineTriggers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Client.Animations.StateMachine;

[RegisterComponent]
public sealed partial class AnimationStateMachineComponent : Component
{
    [DataField]
    public List<ProtoId<AnimationStateMachinePrototype>> StateMachines;

    /// <summary>
    /// Tracks the next update time for each state machine.
    /// </summary>
    /// <remarks>
    /// TODO: Add AutoPausedField when https://github.com/space-wizards/RobustToolbox/issues/3768 is fixed.
    /// </remarks>
    public Dictionary<ProtoId<AnimationStateMachinePrototype>, TimeSpan> NextUpdates = new();

    internal List<AnimationStateMachineInstance> ActiveStateMachines = [];
}

/// <summary>
/// Prototypes of AnimationStateMachine are (presumably) singletons.
/// Conditions and other types require per-instance data.
/// Thus, a holding struct is required to keep track.
/// </summary>
internal sealed class AnimationStateMachineInstance
{
    internal ProtoId<AnimationStateMachinePrototype> Prototype = default;
    internal AnimationStateMachineState[] States = [];
    internal AnimationStateMachineTimer? Timer = null;
    internal AnimationStateMachineState ActiveState = new NullAnimationStateMachineState();
    internal TimeSpan ActiveStateExitTime = TimeSpan.Zero;

    internal void SwitchState(Entity<AnimationStateMachineComponent> ent, AnimationStateMachineState newState, bool switchedByTrigger)
    {
        if (ActiveState == newState)
            return;

        ActiveState.Exit(ent.Owner);
        newState.Enter(ent.Owner, switchedByTrigger);
        ActiveState = newState;
    }
}
