using System.Numerics;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client.Animations.StateMachine.AnimationStateActions;

public sealed partial class WaddleAnimationStateAction : AnimationStateAction
{
    ///<summary>
    /// How high should they hop during the waddle? Higher hop = more energy.
    /// </summary>
    [DataField]
    public Vector2 HopIntensity = new(0, 0.25f);

    /// <summary>
    /// How far should they rock backward and forward during the waddle?
    /// Each step will alternate between this being a positive and negative rotation. More rock = more scary.
    /// </summary>
    [DataField]
    public float TumbleIntensity = 20.0f;

    /// <summary>
    /// How long should a complete step take? Less time = more chaos.
    /// </summary>
    [DataField]
    public float AnimationLength = 0.66f;

    /// <summary>
    /// How much shorter should the animation be when running?
    /// </summary>
    [DataField]
    public float RunAnimationLengthMultiplier = 0.568f;

    /// <summary>
    /// Stores which step we made last, so if someone cancels out of the animation mid-step then restarts it looks more natural.
    /// </summary>
    private bool _lastStep;

    private float CalculateAnimationLength()
    {
        return AnimationLength;
    }

    private float CalculateTumbleIntensity()
    {
        return _lastStep ? 360 - TumbleIntensity : TumbleIntensity;
    }

    public override string AnimationKey => "WaddleAnimation";

    public override Animation CreateAnimation(AppearanceSystem appearanceSystem, EntityUid entity)
    {
        return PlayWaddleAnimationUsing(
            CalculateAnimationLength(),
            CalculateTumbleIntensity()
        );
    }

    private Animation PlayWaddleAnimationUsing(float len, float tumbleIntensity)
    {
        _lastStep = !_lastStep;
        var anim = new Animation()
        {
            Length = TimeSpan.FromSeconds(len),
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
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(tumbleIntensity), len/2),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(0), len/2),
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
                        new AnimationTrackProperty.KeyFrame(HopIntensity, len/2),
                        new AnimationTrackProperty.KeyFrame(new Vector2(), len/2),
                    },
                },
            },
        };
        return anim;
    }
}
