using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using JetBrains.Annotations;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineActions;
[ImplicitDataDefinitionForInheritors]
[PublicAPI]
public abstract partial class AnimationStateMachineAnimationAction
{
    ///<summary>
    /// Should this action be restarted on trigger?
    /// (Useful if an already running animation needs to change immediately after certain events).
    /// </summary>
    [DataField]
    public bool RestartOnTrigger;

    protected string AnimationKey => GetType().Name;

    public virtual void Initialize(EntityManager entityManager) { }

    internal bool TryNextAnimation(AppearanceSystem appearanceSystem, EntityUid entity, [NotNullWhen(true)] out Animation? anim, bool restarting)
    {
        anim = GetNextAnimation(appearanceSystem, entity, restarting);
        return anim != null;
    }

    internal bool TryResetAnimation(AppearanceSystem appearanceSystem, EntityUid entity, [NotNullWhen(true)] out Animation? anim)
    {
        anim = GetResetAnimation(appearanceSystem, entity);
        return anim != null;
    }

    [PublicAPI]
    protected abstract Animation? GetNextAnimation(AppearanceSystem appearanceSystem, EntityUid entity, bool restarting);

    protected abstract Animation? GetResetAnimation(AppearanceSystem appearanceSystem, EntityUid entity);

    protected static readonly Animation StopAnimation = new()
    {
        Length = TimeSpan.FromSeconds(0),
        AnimationTracks =
        {
            new AnimationTrackComponentProperty()
            {
                ComponentType = typeof(SpriteComponent),
                Property = nameof(SpriteComponent.Rotation),
                InterpolationMode = AnimationInterpolationMode.Linear,
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(0), 0),
                },
            },
            new AnimationTrackComponentProperty()
            {
                ComponentType = typeof(SpriteComponent),
                Property = nameof(SpriteComponent.Offset),
                InterpolationMode = AnimationInterpolationMode.Linear,
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(new Vector2(), 0),
                },
            },
        },
    };
}

public sealed partial class NullAnimationStateMachineAnimationAction : AnimationStateMachineAnimationAction
{
    protected override Animation? GetNextAnimation(AppearanceSystem appearanceSystem, EntityUid entity, bool restarting)
    {
        return null;
    }

    protected override Animation? GetResetAnimation(AppearanceSystem appearanceSystem, EntityUid entity)
    {
        return null;
    }
}
