using Content.Client.Animations.StateMachine.AnimationStateMachineActions;
using Robust.Client.GameObjects;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineStates;

public sealed partial class AnimationStateMachineStateSpriteChange : AnimationStateMachineStateBase
{
    private static readonly AnimationStateMachineActionSpriteChangeBase NullAction = new AnimationStateMachineSpriteChangeStateActionNull();

    [DataField]
    public AnimationStateMachineActionSpriteChangeBase Action = NullAction;

    private EntityManager _entityManager;

    public override void Initialize(EntityUid ent, EntityManager entityManager)
    {
        _entityManager = entityManager;
        if (!entityManager.TryGetComponent<SpriteComponent>(ent, out var spriteComponent))
            return;
        Action.Initialize((ent, spriteComponent), entityManager);
    }

    public override void Enter(EntityUid ent, bool enteredByTrigger)
    {
        if (!_entityManager.TryGetComponent<SpriteComponent>(ent, out var spriteComponent))
            return;
        Action.ExecuteSpriteChange((ent, spriteComponent));
    }

    public override void Update(EntityUid ent, bool finished)
    {

    }

    public override void Exit(EntityUid ent)
    {
        if (!_entityManager.TryGetComponent<SpriteComponent>(ent, out var spriteComponent))
            return;
        Action.ResetSpriteChange((ent, spriteComponent));
    }
}
