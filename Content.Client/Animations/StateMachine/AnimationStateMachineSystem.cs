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
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Client.Animations.StateMachine;

public sealed class AnimationStateMachineSystem : VisualizerSystem<AnimationStateMachineComponent>
{
    [Dependency] private readonly ILogManager _logger = default!;
    [Dependency] private readonly IClientGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    private ISawmill _sawmill = default!;

    private const float UpdateInterval = 0.1f;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logger.GetSawmill("asm");

        SubscribeLocalEvent<AnimationStateMachineComponent, ComponentInit>(OnAnimationStateMachineComponentInit);
        SubscribeLocalEvent<AnimationPlayerComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    internal void OnTrigger(AnimationState state, Entity<AnimationStateMachineComponent> entity)
    {
        if (EvaluateConditions(entity, state, true))
            SwitchState(entity, state, true);
    }

    private void OnAnimationCompleted(Entity<AnimationPlayerComponent> ent, ref AnimationCompletedEvent args)
    {
        _sawmill.Debug("OnAnimationCompleted called");
        // TODO: Figure out how to only get here for our own animations.
        if (!TryComp<AnimationStateMachineComponent>(ent, out var comp))
            return;

        if (comp.ActiveState.Action.AnimationKey != args.Key)
            return;

        if (comp.ActiveState.Action.OneShot)
        {
            SwitchState((ent, comp), comp.DefaultState, false);
        }
        else if (comp.ActiveState != comp.DefaultState &&
                 EvaluateConditions((ent, comp), comp.ActiveState, false) &&
                 comp.ActiveState.Action.TryAnimationInternal(AppearanceSystem, ent, out var animation, args.Finished))
        {
            AnimationSystem.Play(ent, animation, comp.ActiveState.Action.AnimationKey);
        }
    }

    private void OnAnimationStateMachineComponentInit(Entity<AnimationStateMachineComponent> ent, ref ComponentInit args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + TimeSpan.FromSeconds(UpdateInterval);
        foreach (var state in ent.Comp.States)
        {
            // TODO: Add more info to error message.
            if (state.Conditions.Length == 0)
                _sawmill.Error("Every AnimationState must have at least one condition.");

            state.Action.Initialize(EntityManager);

            foreach (var cond in state.Conditions)
            {
                cond.Initialize(EntityManager);
            }

            foreach (var trigger in state.Triggers)
            {
                trigger.InitializeInternal(EntityManager, ent, state);
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AnimationStateMachineComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            UpdateTriggers((ent, comp));

            if (comp.NextUpdate > _timing.CurTime)
                continue;

            UpdateStateConditions((ent, comp));

            if (comp.Timer != null)
            {
                comp.NextUpdate += comp.Timer.GetNextPeriod(_random);
            }
            else
            {
                comp.NextUpdate += TimeSpan.FromSeconds(UpdateInterval);
            }
        }
    }

    private void SwitchState(Entity<AnimationStateMachineComponent> ent, AnimationState state, bool switchedByTrigger)
    {
        if (ent.Comp.ActiveState == state && (!switchedByTrigger || !state.Action.RestartOnTrigger))
            return;

        if (!TryComp<AnimationPlayerComponent>(ent, out var animComp))
        {
            _sawmill.Error($"Entity {ent.Owner.Id} is running an AnimationStateMachine without the AnimationPlayerComponent.");
            return;
        }

        if(AnimationSystem.HasRunningAnimation(animComp, ent.Comp.ActiveState.Action.AnimationKey))
            AnimationSystem.Stop((ent, animComp), ent.Comp.ActiveState.Action.AnimationKey);

        _sawmill.Debug($"Entering state for animation {state.Action.AnimationKey}");
        if (!AnimationSystem.HasRunningAnimation(animComp, state.Action.AnimationKey) &&
            state.Action.TryAnimationInternal(AppearanceSystem, ent, out var animation, false))
            AnimationSystem.Play(ent, animation, state.Action.AnimationKey);
        {
            ent.Comp.ActiveState = state;
        }
    }

    private void UpdateStateConditions(Entity<AnimationStateMachineComponent> ent)
    {
        var currentState = ent.Comp.ActiveState;
        var nextState = ent.Comp.DefaultState;

        // Return if currentState has conditions that are still fulfilled.
        if (currentState is { Conditions.Length: > 0, Action.OneShot: false } && EvaluateConditions(ent, currentState, false))
            return;

        foreach (var state in ent.Comp.States)
        {
            if (EvaluateConditions(ent, state, false))
            {
                nextState = state;
                break;
            }
        }

        SwitchState(ent, nextState, false);
    }

    private void UpdateTriggers(Entity<AnimationStateMachineComponent> ent)
    {
        foreach (var state in ent.Comp.States)
        {
            if(_timing.IsFirstTimePredicted)
                EvaluateTriggers(ent, state);
        }
    }

    private bool EvaluateConditions(Entity<AnimationStateMachineComponent> ent, AnimationState state, bool immediately)
    {
        foreach (var cond in state.Conditions)
        {
            if (!_timing.IsFirstTimePredicted || !immediately && ent.Comp.NextUpdate > _timing.CurTime)
                return cond.LastResult;
            if (!cond.EvaluateInternal(ent))
            {
                return false;
            }
        }

        return true;
    }

    private void EvaluateTriggers(Entity<AnimationStateMachineComponent> ent, AnimationState state)
    {
        foreach (var trigger in state.Triggers)
        {
            if (trigger.TriggerIfNecessaryInternal(ent))
            {
                return;
            }
        }
    }
}
