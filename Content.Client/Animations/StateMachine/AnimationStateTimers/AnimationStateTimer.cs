using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Client.Animations.StateMachine.AnimationStateTimers;

[ImplicitDataDefinitionForInheritors]
[PublicAPI]
public abstract partial class AnimationStateTimer
{
    public abstract TimeSpan GetNextPeriod(IRobustRandom random);
}
