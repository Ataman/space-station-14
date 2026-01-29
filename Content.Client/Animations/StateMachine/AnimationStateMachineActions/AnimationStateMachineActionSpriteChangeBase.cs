using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using JetBrains.Annotations;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineActions;
[ImplicitDataDefinitionForInheritors]
[PublicAPI]
public abstract partial class AnimationStateMachineActionSpriteChangeBase
{
    public abstract void Initialize(Entity<SpriteComponent> entity, EntityManager entityManager);
    public abstract void ExecuteSpriteChange(Entity<SpriteComponent> entity);
    public abstract void ResetSpriteChange(Entity<SpriteComponent> entity);
}

public sealed partial class AnimationStateMachineSpriteChangeStateActionNull : AnimationStateMachineActionSpriteChangeBase
{
    public override void Initialize(Entity<SpriteComponent> entity, EntityManager entityManager) { }

    public override void ExecuteSpriteChange(Entity<SpriteComponent> entity) { }

    public override void ResetSpriteChange(Entity<SpriteComponent> entity) { }
}
