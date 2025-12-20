using Content.Client.Animations.StateMachine.AnimationStateActions;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Components.Effects;
using Content.Shared.Trigger.Components.Triggers;
using Robust.Client.GameObjects;
using Robust.Client.Timing;
using Robust.Shared.GameStates;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Client.Animations.StateMachine;

public sealed class AnimationStateMachineSystem : VisualizerSystem<AnimationStateMachineComponent>
{
    [Dependency] private readonly ILogManager _logger = default!;
    [Dependency] private readonly IClientGameTiming _timing = default!;
    private ISawmill _sawmill = default!;

    private const float UpdateInterval = 0.1f;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logger.GetSawmill("asm");

        SubscribeLocalEvent<AnimationStateMachineComponent, ComponentInit>(OnAnimationStateMachineComponentInit);
        SubscribeLocalEvent<AnimationStateMachineComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AnimationPlayerComponent, AnimationCompletedEvent>(OnAnimationCompleted);

        SubscribeLocalEvent<InputMoverComponent, MoveInputEvent>(OnMoveInputEvent);
    }

    private void OnMoveInputEvent(Entity<InputMoverComponent> ent, ref MoveInputEvent args)
    {
        _sawmill.Debug($"Ent[{ent.Owner.Id}] - MoveInputEvent: HasDirectionalMovement={args.HasDirectionalMovement}, OldMovement={args.OldMovement}");
    }

    private void OnMapInit(EntityUid uid, AnimationStateMachineComponent comp, MapInitEvent init)
    {
        comp.NextUpdate = _timing.CurTime + TimeSpan.FromSeconds(UpdateInterval);
    }

    internal bool OnTrigger(AnimationState state, Entity<AnimationStateMachineComponent> entity)
    {
        if (!EvaluateConditions(entity, state))
            return false;
        SwitchState(entity, state);
        return true;
    }

    private void OnAnimationCompleted(Entity<AnimationPlayerComponent> ent, ref AnimationCompletedEvent args)
    {
        // TODO: Figure out how to only get here for our own animations.
        if (!TryComp<AnimationStateMachineComponent>(ent, out var comp))
            return;

        // Action without conditions is the default state, don't replay it.
        if (comp.ActiveState.Action.AnimationKey == args.Key &&
            comp.ActiveState != comp.DefaultState &&
            EvaluateConditions((ent, comp), comp.ActiveState))
            AnimationSystem.Play(ent, comp.ActiveState.Action.CreateAnimation(AppearanceSystem, ent), comp.ActiveState.Action.AnimationKey);
    }

    private void OnAnimationStateMachineComponentInit(Entity<AnimationStateMachineComponent> ent, ref ComponentInit args)
    {
        foreach (var state in ent.Comp.States)
        {
            // TODO: Add more info to error message.
            if (state.Conditions.Length == 0)
                _sawmill.Error("Every AnimationState must have at least one condition.");

            foreach (var cond in state.Conditions)
            {
                cond.Initialize(EntityManager);
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AnimationStateMachineComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            if (comp.NextUpdate > _timing.CurTime)
                continue;

            UpdateStateMachine((ent, comp));

            comp.NextUpdate += TimeSpan.FromSeconds(UpdateInterval);
        }
    }

    private void SwitchState(Entity<AnimationStateMachineComponent> ent, AnimationState state)
    {
        if (ent.Comp.ActiveState == state)
            return;

        if (!TryComp<AnimationPlayerComponent>(ent, out var animComp))
            return;

        if(AnimationSystem.HasRunningAnimation(animComp, ent.Comp.ActiveState.Action.AnimationKey))
            AnimationSystem.Stop((ent, animComp), ent.Comp.ActiveState.Action.AnimationKey);

        _sawmill.Debug($"Entering state for animation {state.Action.AnimationKey}");
        if (!AnimationSystem.HasRunningAnimation(animComp, state.Action.AnimationKey))
            AnimationSystem.Play(ent, state.Action.CreateAnimation(AppearanceSystem, ent), state.Action.AnimationKey);
        ent.Comp.ActiveState = state;
    }

    private void UpdateStateMachine(Entity<AnimationStateMachineComponent> ent)
    {
        var currentState = ent.Comp.ActiveState;
        var nextState = ent.Comp.DefaultState;

        // Return if currentState has conditions that are still fulfilled.
        if (currentState is { Conditions.Length: > 0 } && EvaluateConditions(ent, currentState))
            return;

        foreach (var state in ent.Comp.States)
        {
            if (EvaluateConditions(ent, state))
            {
                nextState = state;
                break;
            }
        }

        SwitchState(ent, nextState);
    }

    private bool EvaluateConditions(Entity<AnimationStateMachineComponent> ent, AnimationState state)
    {
        foreach (var cond in state.Conditions)
        {
            if (ent.Comp.NextUpdate > _timing.CurTime)
                return cond.LastResult;
            if (!cond.EvaluateInternal(ent))
            {
                return false;
            }
        }

        return true;
    }
}
