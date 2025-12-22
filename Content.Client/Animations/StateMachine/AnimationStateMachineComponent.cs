using Content.Client.Animations.StateMachine.AnimationStateActions;
using Content.Client.Animations.StateMachine.AnimationStateConditions;
using Content.Client.Animations.StateMachine.AnimationStateTimers;
using Content.Client.Animations.StateMachine.AnimationStateTriggers;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Client.Animations.StateMachine;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class AnimationStateMachineComponent : Component
{
    /// <summary>
    /// A collection of possible states for this component.
    /// </summary>
    [DataField]
    public List<AnimationState> States = [];

    /// <summary>
    /// The server time at which the next sound will play.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// Optional timer for executing an update.
    /// Setting this disables periodic condition checks.
    /// </summary>
    [DataField]
    public AnimationStateTimer? Timer;

    [DataField]
    public AnimationState DefaultState = AnimationState.StopAnimationState;

    [ViewVariables(VVAccess.ReadOnly)]
    public AnimationState ActiveState = AnimationState.StopAnimationState;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsWalking = false;
}

[Serializable]
[DataDefinition]
public sealed partial class AnimationState
{
    /// <summary>
    /// A collection of conditions that must be true for this state to activate.
    /// </summary>
    [DataField]
    public AnimationStateCondition[] Conditions = [];

    /// <summary>
    /// A collection of triggers that cause a conditions check.
    /// </summary>
    [DataField]
    public AnimationStateTrigger[] Triggers = [];

    /// <summary>
    /// The action (animation) that should be used when this state is entered/running.
    /// </summary>
    [DataField]
    public AnimationStateAction Action;

    public static AnimationState StopAnimationState = new AnimationState() { Action = new StopAnimationStateAction() };
}
