using Content.Client.Animations.StateMachine.AnimationStateMachineStates;
using Robust.Client.GameObjects;
using Robust.Client.Timing;
using Robust.Shared.Random;

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


        // Current situation:
        // To run multiple state machines on an entity: Turn the state machine into a prototype and turn the component into a state machine collection.
        // Dictionary<AnimationStateMachine, TimeSpan> actually works with the [AutoPausedField] attribute so we use that for timers.
        // The EyeBlink example still needs a timer/trigger to exit the state as well.
        
    }

    internal void OnTrigger(AnimationStateMachineState state, Entity<AnimationStateMachineComponent> entity)
    {
        if (EvaluateConditions(entity, state, true))
            SwitchState(entity, state, true);
    }

    private void OnAnimationCompleted(Entity<AnimationPlayerComponent> ent, ref AnimationCompletedEvent args)
    {
        // TODO: Figure out how to only get here for our own animations.
        if (!TryComp<AnimationStateMachineComponent>(ent, out var comp))
            return;

        if (comp.ActiveState.OneShot)
        {
            SwitchState((ent, comp), comp.DefaultState, false);
        }
        else if (EvaluateConditions((ent, comp), comp.ActiveState, false))
        {
            comp.ActiveState.Update(ent, args.Finished);
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

            state.Initialize(ent.Owner, EntityManager);

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

    private void SwitchState(Entity<AnimationStateMachineComponent> ent, AnimationStateMachineState state, bool switchedByTrigger)
    {
        if (ent.Comp.ActiveState == state)
            return;

        ent.Comp.ActiveState.Exit(ent.Owner);
        state.Enter(ent.Owner, switchedByTrigger);
        ent.Comp.ActiveState = state;
    }

    private void UpdateStateConditions(Entity<AnimationStateMachineComponent> ent)
    {
        var currentState = ent.Comp.ActiveState;
        var nextState = ent.Comp.DefaultState;

        // Return if currentState has conditions that are still fulfilled.
        if (currentState is { Conditions.Length: > 0, OneShot: false } && EvaluateConditions(ent, currentState, false))
            return;

        foreach (var state in ent.Comp.States)
        {
            if (!EvaluateConditions(ent, state, false))
                continue;

            nextState = state;
            break;
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

    private bool EvaluateConditions(Entity<AnimationStateMachineComponent> ent, AnimationStateMachineState state, bool immediately)
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

    private void EvaluateTriggers(Entity<AnimationStateMachineComponent> ent, AnimationStateMachineState state)
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
