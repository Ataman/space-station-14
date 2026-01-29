using System.Numerics;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineActions.Animations;

public sealed partial class AnimationStateMachineActionAnimationHopping : AnimationStateMachineActionAnimationBase
{
    ///<summary>
    /// How high should they hop? Higher hop = more energy.
    /// </summary>
    [DataField]
    public float HopIntensity = 0.35f;

    /// <summary>
    /// How long should the hop take?
    /// </summary>
    [DataField]
    public float AnimationLength = 0.3f;

    protected override Animation GetNextAnimation(AppearanceSystem appearanceSystem, EntityUid entity, bool restarting)
    {
        var anim = new Animation()
        {
            Length = TimeSpan.FromSeconds(AnimationLength),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(), 0),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0, HopIntensity), AnimationLength/3),
                        new AnimationTrackProperty.KeyFrame(new Vector2(), AnimationLength/3*2),
                    },
                },
            },
        };
        return anim;
    }

    protected override Animation? GetResetAnimation(AppearanceSystem appearanceSystem, EntityUid entity)
    {
        return StopAnimation;
    }
}
