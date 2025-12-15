using Robust.Client.GameObjects;
using Robust.Shared.Map.Components;

namespace Content.Client.Animations.StateMachine;

public sealed class AnimationStateMachineSystem : VisualizerSystem<AnimationStateMachineComponent>
{
    [Dependency] private readonly ILogManager _logger = default!;
    private ISawmill _sawmill = default!;

    private readonly List<EntityUid> _activeStateMachines = [];
    private readonly Dictionary<EntityUid, AnimationState> _activeStates = new();

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logger.GetSawmill("asm");
        _sawmill.Log(LogLevel.Debug, "ASM-System initialized");

        SubscribeLocalEvent<AnimationStateMachineComponent, ComponentShutdown>(OnAnimationStateMachineShutdown);
        SubscribeLocalEvent<AnimationStateMachineComponent, ComponentStartup>(OnAnimationStateMachineInit);
        SubscribeLocalEvent<AnimationPlayerComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    private void OnAnimationCompleted(Entity<AnimationPlayerComponent> ent, ref AnimationCompletedEvent args)
    {
        // TODO: Figure out how to only get here for our own animations.
        if (!TryComp<AnimationStateMachineComponent>(ent, out var comp))
            return;

        if (_activeStates.TryGetValue(ent, out var state) && state.Action.AnimationKey.Equals(args.Key))
            AnimationSystem.Play(ent, state.Action.CreateAnimation(AppearanceSystem, ent), state.Action.AnimationKey);
    }

    private void OnAnimationStateMachineShutdown(Entity<AnimationStateMachineComponent> ent, ref ComponentShutdown args)
    {
        ExitState(ent);
        _activeStateMachines.Remove(ent);
    }

    private void OnAnimationStateMachineInit(EntityUid ent, AnimationStateMachineComponent component, ComponentStartup args)
    {
        _activeStateMachines.Add(ent);
        foreach (var state in component.States)
        {
            foreach (var cond in state.Conditions)
            {
                cond.Initialize(EntityManager);
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        foreach (var ent in _activeStateMachines)
        {
            UpdateStateMachine(ent);
        }
    }

    private void EnterState(EntityUid ent, AnimationState state)
    {
        if (_activeStates.TryGetValue(ent, out var currentState) && currentState == state)
            return;
        ExitState(ent);
        AnimationSystem.Play(ent, state.Action.CreateAnimation(AppearanceSystem, ent), state.Action.AnimationKey);
        _activeStates.Add(ent, state);
    }

    private void ExitState(EntityUid ent)
    {
        if (!_activeStates.TryGetValue(ent, out var state))
            return;

        _activeStates.Remove(ent);

        if (TryComp<AnimationPlayerComponent>(ent, out var animComp) &&
            AnimationSystem.HasRunningAnimation(animComp, state.Action.AnimationKey))
        {
            var animEnt = (ent, animation: animComp);
            AnimationSystem.Stop(animEnt, state.Action.AnimationKey);
            if (!AnimationSystem.HasRunningAnimation(animComp, state.Action.AnimationKey + "_STOP"))
                AnimationSystem.Play(ent, state.Action.StopAnimation(AppearanceSystem, ent), state.Action.AnimationKey + "_STOP");
        }
    }

    private void UpdateStateMachine(EntityUid ent)
    {
        if (_activeStates.TryGetValue(ent, out var currentState))
        {
            if (!EvaluateConditions(ent, currentState))
            {
                ExitState(ent);
            }
        }
        else
        {
            var comp = Comp<AnimationStateMachineComponent>(ent);
            foreach (var state in comp.States)
            {
                // TODO: ONLY FOR TESTING, REMOVE AND USE TRIGGERS/TIMERS INSTEAD!
                if (!EvaluateConditions(ent, state))
                    continue;

                EnterState(ent, state);

                return;
            }
        }
    }

    private bool EvaluateConditions(EntityUid ent, AnimationState state)
    {
        foreach (var cond in state.Conditions)
        {
            if (!cond.Evaluate(ent))
            {
                return false;
            }
        }

        return true;
    }
}
