using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client.Animations.StateMachine.AnimationStateActions;
[ImplicitDataDefinitionForInheritors]
public abstract partial class AnimationStateAction
{
    public abstract string AnimationKey { get; }
    public abstract Animation CreateAnimation(AppearanceSystem appearanceSystem, EntityUid entity);
    public abstract Animation StopAnimation(AppearanceSystem appearanceSystem, EntityUid entity);
}
