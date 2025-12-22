using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client.Animations.StateMachine.AnimationStateActions;
[ImplicitDataDefinitionForInheritors]
[PublicAPI]
public abstract partial class AnimationStateAction
{
    ///<summary>
    /// Should this action be restarted on trigger?
    /// (Useful if an already running animation needs to change immediately after certain events).
    /// </summary>
    [DataField]
    public bool RestartOnTrigger = false;

    /// <summary>
    /// If set to true, the state only executes its action one time instead of restarting it once the animation ends.
    /// </summary>
    [DataField]
    public bool OneShot = false;

    /// <summary>
    /// I couldn't get IoCManager.InjectDependencies to work, use this method to initialize them manually.
    /// </summary>
    /// <param name="entityManager"></param>
    public virtual void Initialize(EntityManager entityManager) { }
    public abstract string AnimationKey { get; }

    internal bool TryAnimationInternal(AppearanceSystem appearanceSystem, EntityUid entity, [NotNullWhen(true)] out Animation? anim, bool restarting)
    {
        anim = TryAnimation(appearanceSystem, entity, restarting);
        return anim != null;
    }

    [PublicAPI]
    protected abstract Animation? TryAnimation(AppearanceSystem appearanceSystem, EntityUid entity, bool restarting);
}
