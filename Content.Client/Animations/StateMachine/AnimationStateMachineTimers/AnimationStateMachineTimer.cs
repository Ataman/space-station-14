using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineTimers;

[ImplicitDataDefinitionForInheritors]
[PublicAPI]
public abstract partial class AnimationStateMachineTimer
{
    public abstract TimeSpan GetNextPeriod(IRobustRandom random);
}
