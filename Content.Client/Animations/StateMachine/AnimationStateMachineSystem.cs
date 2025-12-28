using System.Linq;
using System.Reflection;
using Content.Client.Animations.StateMachine.AnimationStateMachineConditions;
using Content.Client.Animations.StateMachine.AnimationStateMachineStates;
using Content.Client.Animations.StateMachine.AnimationStateMachineTriggers;
using Content.Shared.Mobs.Events;
using Robust.Client.GameObjects;
using Robust.Client.Timing;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;

namespace Content.Client.Animations.StateMachine;

public sealed class AnimationStateMachineSystem : VisualizerSystem<AnimationStateMachineComponent>
{
    [Dependency] private readonly ILogManager _logger = default!;
    [Dependency] private readonly IClientGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;
    private ISawmill _sawmill = default!;

    private const float UpdateInterval = 0.1f;

    private Dictionary<EntityUid, List<(Type, string)>> _actingComponentProperties = new();

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logger.GetSawmill("asm");

        SubscribeLocalEvent<AnimationStateMachineComponent, ComponentInit>(OnAnimationStateMachineComponentInit);
        SubscribeLocalEvent<AnimationPlayerComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    internal void OnTrigger(AnimationStateMachineState state, Entity<AnimationStateMachineComponent> entity)
    {
        foreach (var stateMachine in entity.Comp.ActiveStateMachines)
        {
            if (stateMachine.ActiveState == state && EvaluateConditions(entity, state, TimeSpan.Zero))
            {
                stateMachine.SwitchState(entity, state, true);
            }
        }
    }

    internal void RegisterEntityAnimationProperty(EntityUid uid, Type type, string prop)
    {
        if (!_actingComponentProperties.ContainsKey(uid))
            _actingComponentProperties.Add(uid, []);
        if (_actingComponentProperties[uid]
            .Any(x => x.Item1 == type && x.Item2 == prop))
        {
            _sawmill.Error($"An animation using the {prop} property on {type.Name} has already been registered for entity {uid}");
            return;
        }
        _actingComponentProperties[uid].Add((type, prop));
    }

    internal void DeregisterEntityAnimationProperty(EntityUid uid, Type type, string prop)
    {
        if(!_actingComponentProperties.ContainsKey(uid))
            _actingComponentProperties.Add(uid, []);
        var tuple = _actingComponentProperties[uid].Single(x => x.Item1 == type && x.Item2 == prop);
        _actingComponentProperties[uid].Remove(tuple);
    }

    private void OnAnimationCompleted(Entity<AnimationPlayerComponent> ent, ref AnimationCompletedEvent args)
    {
        // TODO: Figure out how to only get here for our own animations.
        if (!TryComp<AnimationStateMachineComponent>(ent, out var comp))
            return;

        AnimationStateMachineInstance? instance = null;
        foreach (var machine in comp.ActiveStateMachines)
        {
            if (machine.ActiveState is AnimationStateMachineAnimationState animState && animState.RunningAnimationKey == args.Key)
            {
                instance = machine;
            }
        }

        if (instance == null)
            return;

        if (instance.ActiveState.OneShot)
        {
            instance.SwitchState((ent, comp), _prototypeManager.Index(instance.Prototype).DefaultState, false);
        }
        else if (EvaluateConditions((ent, comp), instance.ActiveState, comp.NextUpdates[instance.Prototype]))
        {
            instance.ActiveState.Update(ent, args.Finished);
        }
    }

    private void OnAnimationStateMachineComponentInit(Entity<AnimationStateMachineComponent> ent, ref ComponentInit args)
    {
        foreach (var stateMachine in ent.Comp.StateMachines)
        {
            InitStateMachine(ent, stateMachine);
        }
    }

    /// <summary>
    /// Initialize state machine from prototype and add to list of active instances.
    /// </summary>
    private void InitStateMachine(Entity<AnimationStateMachineComponent> ent, ProtoId<AnimationStateMachinePrototype> protoId)
    {
        List<AnimationStateMachineState> states = [];

        var proto = _prototypeManager.Index(protoId);

        ent.Comp.NextUpdates[proto] = _timing.CurTime + TimeSpan.FromSeconds(UpdateInterval);
        foreach (var state in proto.States)
        {
            // TODO: Add more info to error message.
            if (state.Conditions.Length == 0)
                _sawmill.Error("Every AnimationState must have at least one condition.");

            var stateCopy = _serializationManager.CreateCopy(state, null, false, false);
            stateCopy.Initialize(ent.Owner, EntityManager);

            foreach (var cond in stateCopy.Conditions)
            {
                cond.Initialize(EntityManager);
            }

            foreach (var trigger in stateCopy.Triggers)
            {
                trigger.InitializeInternal(EntityManager, ent, stateCopy);
            }

            states.Add(stateCopy);
        }

        var instance = new AnimationStateMachineInstance()
        {
            Prototype = proto,
            States = states.ToArray(),
            Timer = _serializationManager.CreateCopy(proto.Timer, null, false),
        };

        ent.Comp.ActiveStateMachines.Add(instance);
    }

    private void UpdateStateMachine(Entity<AnimationStateMachineComponent> ent, AnimationStateMachineInstance stateMachine)
    {
        UpdateTriggers(ent, stateMachine);
        ExitActiveStateIfExitTimerReached(ent, stateMachine);

        if (ent.Comp.NextUpdates[stateMachine.Prototype] > _timing.CurTime)
            return;

        CheckStateMachineConditionsAndUpdateState(ent, stateMachine);

        if (stateMachine.Timer != null)
        {
            ent.Comp.NextUpdates[stateMachine.Prototype] += stateMachine.Timer.GetNextPeriod(_random);
        }
        else
        {
            ent.Comp.NextUpdates[stateMachine.Prototype] += TimeSpan.FromSeconds(UpdateInterval);
        }
    }

    private void ExitActiveStateIfExitTimerReached(Entity<AnimationStateMachineComponent> ent, AnimationStateMachineInstance stateMachine)
    {
        if (!_timing.IsFirstTimePredicted)
            return;
        if (stateMachine.ActiveStateExitTime == TimeSpan.Zero)
            return;
        if (stateMachine.ActiveStateExitTime > _timing.CurTime)
            return;

        stateMachine.SwitchState(ent, _prototypeManager.Index(stateMachine.Prototype).DefaultState, false);
        stateMachine.ActiveStateExitTime = TimeSpan.Zero;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AnimationStateMachineComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            foreach (var stateMachine in comp.ActiveStateMachines)
            {
                UpdateStateMachine((ent, comp), stateMachine);
            }
        }
    }

    private void CheckStateMachineConditionsAndUpdateState(Entity<AnimationStateMachineComponent> ent, AnimationStateMachineInstance stateMachine)
    {
        var currentState = stateMachine.ActiveState;
        var nextState = _prototypeManager.Index(stateMachine.Prototype).DefaultState;

        // Return if currentState has conditions that are still fulfilled.
        if (currentState is { Conditions.Length: > 0, OneShot: false } &&
            EvaluateConditions(ent, currentState, ent.Comp.NextUpdates[stateMachine.Prototype]) &&
            stateMachine.ActiveStateExitTime == TimeSpan.Zero)
            return;

        foreach (var state in stateMachine.States)
        {
            if (!EvaluateConditions(ent, state, ent.Comp.NextUpdates[stateMachine.Prototype]))
                continue;

            nextState = state;
            break;
        }

        stateMachine.SwitchState(ent, nextState, false);
        if (nextState.ExitPeriod != TimeSpan.Zero)
            stateMachine.ActiveStateExitTime = _timing.CurTime + nextState.ExitPeriod;
    }

    private void UpdateTriggers(Entity<AnimationStateMachineComponent> ent, AnimationStateMachineInstance stateMachine)
    {
        foreach (var state in stateMachine.States)
        {
            if(_timing.IsFirstTimePredicted)
                EvaluateTriggers(ent, state);
        }
    }

    private bool EvaluateConditions(Entity<AnimationStateMachineComponent> ent, AnimationStateMachineState state, TimeSpan nextUpdate)
    {
        foreach (var cond in state.Conditions)
        {
            // TODO: Actually update NextUpdates if we're evaluating.
            if ((!_timing.IsFirstTimePredicted || nextUpdate > _timing.CurTime) && !cond.LastResult)
                return false;
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
