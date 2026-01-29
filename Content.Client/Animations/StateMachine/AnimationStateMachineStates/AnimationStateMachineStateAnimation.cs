using Content.Client.Animations.StateMachine.AnimationStateMachineActions;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineStates;

public sealed partial class AnimationStateMachineStateAnimation : AnimationStateMachineStateBase
{
    private static readonly AnimationStateMachineAnimationAction NullAction = new NullAnimationStateMachineAnimationAction();

    private AnimationPlayerSystem _animationPlayerSystem;
    private AnimationStateMachineSystem _animationStateMachineSystem;
    private AppearanceSystem _appearanceSystem;

    private readonly List<(Type, string)> _animationCompProps = [];

    [DataField]
    public AnimationStateMachineAnimationAction Action = NullAction;

    public string RunningAnimationKey => Action.GetType().Name + "_RUNNING";
    public string StopAnimationKey => Action.GetType().Name + "_STOP";

    /// <summary>
    /// I couldn't get IoCManager.InjectDependencies to work, use this method to initialize them manually.
    /// </summary>
    public override void Initialize(EntityUid ent, EntityManager entityManager)
    {
        _animationPlayerSystem = entityManager.System<AnimationPlayerSystem>();
        _animationStateMachineSystem = entityManager.System<AnimationStateMachineSystem>();
        _appearanceSystem = entityManager.System<AppearanceSystem>();
        Action.Initialize(entityManager);
    }

    public override void Enter(EntityUid ent, bool enteredByTrigger)
    {
        if (enteredByTrigger && Action.RestartOnTrigger ||
            _animationPlayerSystem.HasRunningAnimation(ent, RunningAnimationKey) ||
            !Action.TryNextAnimation(_appearanceSystem, ent, out var animation, false))
            return;

        UpdateAnimationCompProps(ent, animation);
        _animationPlayerSystem.Play(ent, animation, RunningAnimationKey);
    }

    public override void Update(EntityUid ent, bool finished)
    {
        if (_animationPlayerSystem.HasRunningAnimation(ent, RunningAnimationKey) ||
            !Action.TryNextAnimation(_appearanceSystem, ent, out var animation, finished))
            return;

        UpdateAnimationCompProps(ent, animation);
        _animationPlayerSystem.Play(ent, animation, RunningAnimationKey);
    }

    public override void Exit(EntityUid ent)
    {
        // Stop the running animation of this state.
        if (_animationPlayerSystem.HasRunningAnimation(ent, RunningAnimationKey))
            _animationPlayerSystem.Stop(ent, RunningAnimationKey);

        // TODO: The stopping animation can still clash with registered component properties but to fix this
        // a state queue would be required first.
        // For now, we'll test and see if any glitches would even occur due to this.
        ClearAnimationCompProps(ent);

        // Stop the reset animation of this state.
        if (_animationPlayerSystem.HasRunningAnimation(ent, StopAnimationKey) ||
            !Action.TryResetAnimation(_appearanceSystem, ent, out var animation))
            return;

        // Play the stopping animation.
        _animationPlayerSystem.Play(ent, animation, StopAnimationKey);
    }

    private void ClearAnimationCompProps(EntityUid ent)
    {
        foreach (var compProp in _animationCompProps)
        {
            _animationStateMachineSystem.DeregisterEntityAnimationProperty(ent,
                compProp.Item1,
                compProp.Item2);
        }
        _animationCompProps.Clear();
    }

    private void UpdateAnimationCompProps(EntityUid ent, Animation anim)
    {
        ClearAnimationCompProps(ent);
        foreach (var track in anim.AnimationTracks)
        {
            if (track is not AnimationTrackComponentProperty { ComponentType: not null, Property: not null } propTrack)
                continue;

            _animationCompProps.Add((propTrack.ComponentType, propTrack.Property));
            _animationStateMachineSystem.RegisterEntityAnimationProperty(ent,
                propTrack.ComponentType,
                propTrack.Property);
        }
    }
}
