using System.Numerics;
using Content.Client.Buckle;
using Content.Client.Gravity;
using Content.Shared.ActionBlocker;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Physics.Components;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineConditions;

public sealed partial class AnimationStateMachineHasMobStateCondition : AnimationStateMachineCondition
{
    [DataField]
    public MobState State = MobState.Invalid;

    private MobStateSystem _mobStateSystem = null!;

    public override void Initialize(EntityManager entityManager)
    {
        base.Initialize(entityManager);
        _mobStateSystem = entityManager.System<MobStateSystem>();
    }

    protected override bool Evaluate(EntityUid entity)
    {
        return _mobStateSystem.HasState(entity, State);
    }
}
