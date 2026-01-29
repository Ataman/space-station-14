using Content.Client.Animations.StateMachine.AnimationStateMachineStates;
using Content.Client.Animations.StateMachine.AnimationStateMachineTimers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Client.Animations.StateMachine;

[Prototype("animationStateMachine")]
public sealed partial class AnimationStateMachinePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// A collection of possible states for this component.
    /// </summary>
    [DataField]
    public List<AnimationStateMachineStateBase> States = [];

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
    public AnimationStateMachineStateBase DefaultState = new NullAnimationStateMachineStateBase();

}
