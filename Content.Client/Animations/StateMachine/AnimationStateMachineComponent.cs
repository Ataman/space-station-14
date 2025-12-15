using Content.Client.Animations.StateMachine.AnimationStateActions;
using Content.Client.Animations.StateMachine.AnimationStateConditions;

namespace Content.Client.Animations.StateMachine;

[RegisterComponent]
public sealed partial class AnimationStateMachineComponent : Component
{
    [DataField]
    public AnimationState[] States;
}

[Serializable]
[DataDefinition]
public sealed partial class AnimationState
{
    [DataField]
    public AnimationStateCondition[] Conditions;

    [DataField]
    public AnimationStateAction Action;
}
