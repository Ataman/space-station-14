using Content.Shared.Humanoid;
using Robust.Client.GameObjects;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineActions.SpriteChanges;

public sealed partial class AnimationStateMachineActionSpriteChangeEyeBlink : AnimationStateMachineActionSpriteChangeBase
{
    [DataField]
    public float BlinkSkinColorMultiplier = 0.9f;

    private HumanoidAppearanceComponent? _humanoid;
    private SpriteSystem _spriteSystem;

    public override void Initialize(Entity<SpriteComponent> entity, EntityManager manager)
    {
        _spriteSystem = manager.System<SpriteSystem>();
        if (!manager.TryGetComponent<HumanoidAppearanceComponent>(entity.Owner, out var humanoid))
            return;
        _humanoid = humanoid;
    }

    public override void ExecuteSpriteChange(Entity<SpriteComponent> entity)
    {
        if (_humanoid == null)
            return;
        var blinkFade = BlinkSkinColorMultiplier;
        var blinkColor = new Color(
            _humanoid.SkinColor.R * blinkFade,
            _humanoid.SkinColor.G * blinkFade,
            _humanoid.SkinColor.B * blinkFade);
        entity.Comp[_spriteSystem.LayerMapReserve((entity.Owner, entity.Comp), HumanoidVisualLayers.Eyes)].Color = blinkColor;
    }

    public override void ResetSpriteChange(Entity<SpriteComponent> entity)
    {
        if (_humanoid == null)
            return;

        entity.Comp[_spriteSystem.LayerMapReserve((entity.Owner, entity.Comp), HumanoidVisualLayers.Eyes)].Color = _humanoid.EyeColor;
    }
}
