using Robust.Shared.Random;

namespace Content.Client.Animations.StateMachine.AnimationStateTimers;

public sealed partial class AnimationStateRandomTimeRangeTimer : AnimationStateTimer
{
    [DataField]
    public TimeSpan MinTime;

    [DataField]
    public TimeSpan MaxTime;

    public override TimeSpan GetNextPeriod(IRobustRandom random)
    {
        return random.Next(MinTime, MaxTime);
    }
}
