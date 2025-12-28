using System.Numerics;
using Content.Client.Buckle;
using Content.Client.Gravity;
using Content.Shared.ActionBlocker;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Physics.Components;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineConditions;

public sealed partial class AnimationStateMachineIsWeightlessCondition : AnimationStateMachineCondition
{
    private GravitySystem _gravitySystem = null!;

    public override void Initialize(EntityManager entityManager)
    {
        base.Initialize(entityManager);

        _gravitySystem = entityManager.System<GravitySystem>();

    }

    protected override bool Evaluate(EntityUid entity)
    {
        if (_gravitySystem.IsWeightless(entity))
            return true;
        return false;
    }
}
