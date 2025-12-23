using Content.Client.Animations.StateMachine.AnimationStateMachineStates;
using Content.Client.Animations.StateMachine.AnimationStateMachineTimers;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Client.Animations.StateMachine;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class AnimationStateMachineComponent : Component
{
    private static readonly AnimationStateMachineState NullState = new NullAnimationStateMachineState();
    /// <summary>
    /// A collection of possible states for this component.
    /// </summary>
    [DataField]
    public List<AnimationStateMachineState> States = [];

    /// <summary>
    /// Time for the next update.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// Optional timer for executing an update.
    /// Setting this disables periodic condition checks.
    /// </summary>
    [DataField]
    public AnimationStateMachineTimer? Timer;

    /// <summary>
    /// The default state to enter when no other state matches their conditions.
    /// </summary>
    [DataField]
    public AnimationStateMachineState DefaultState = NullState;

    /// <summary>
    /// The currently active state.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public AnimationStateMachineState ActiveState = NullState;
}
