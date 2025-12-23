using Robust.Shared.Random;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineTimers;

public sealed partial class AnimationStateMachineRandomTimeRangeTimer : AnimationStateMachineTimer
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
