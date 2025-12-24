using Content.Client.Animations.StateMachine.AnimationStateMachineActions;
using Robust.Client.GameObjects;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineStates;

public sealed partial class AnimationStateMachineAnimationState : AnimationStateMachineState
{
    private static readonly AnimationStateMachineAnimationAction NullAction = new NullAnimationStateMachineAnimationAction();

    private AnimationPlayerSystem _animationPlayerSystem;
    private AppearanceSystem _appearanceSystem;

    [DataField]
    public AnimationStateMachineAnimationAction Action = NullAction;

    internal string RunningAnimationKey => Action.GetType().Name + "_RUNNING";
    internal string StopAnimationKey => Action.GetType().Name + "_STOP";

    /// <summary>
    /// I couldn't get IoCManager.InjectDependencies to work, use this method to initialize them manually.
    /// </summary>
    public override void Initialize(EntityUid ent, EntityManager entityManager)
    {
        _animationPlayerSystem = entityManager.System<AnimationPlayerSystem>();
        _appearanceSystem = entityManager.System<AppearanceSystem>();
        Action.Initialize(entityManager);
    }

    public override void Enter(EntityUid ent, bool enteredByTrigger)
    {
        if ((!enteredByTrigger || !Action.RestartOnTrigger) &&
            !_animationPlayerSystem.HasRunningAnimation(ent, RunningAnimationKey) &&
            Action.TryNextAnimation(_appearanceSystem, ent, out var animation, false))
        {
            _animationPlayerSystem.Play(ent, animation, RunningAnimationKey);
        }
    }

    public override void Update(EntityUid ent, bool finished)
    {
        if (!_animationPlayerSystem.HasRunningAnimation(ent, RunningAnimationKey) &&
            Action.TryNextAnimation(_appearanceSystem, ent, out var animation, finished))
        {
            _animationPlayerSystem.Play(ent, animation, RunningAnimationKey);
        }
    }

    public override void Exit(EntityUid ent)
    {
        // Stop the running animation of this state.
        if (_animationPlayerSystem.HasRunningAnimation(ent, RunningAnimationKey))
        {
            _animationPlayerSystem.Stop(ent, RunningAnimationKey);
        }

        // Stop the reset animation of this state.
        if (!_animationPlayerSystem.HasRunningAnimation(ent, StopAnimationKey) &&
            Action.TryResetAnimation(_appearanceSystem, ent, out var animation))
        {
            _animationPlayerSystem.Play(ent, animation, StopAnimationKey);
        }
    }
}
