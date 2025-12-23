using Content.Shared.Humanoid;
using Robust.Client.GameObjects;

namespace Content.Client.Animations.StateMachine.AnimationStateMachineActions.SpriteChanges;

public sealed partial class EyeBlinkAnimationStateMachineSpriteChangeAction : AnimationStateMachineSpriteChangeStateAction
{
    [DataField]
    public float BlinkSkinColorMultiplier = 0.9f;

    private HumanoidAppearanceComponent? _humanoid;
    private Color _originalColor = Color.White;
    private EntityManager _entities;
    private SpriteSystem _spriteSystem;

    public override void Initialize(Entity<SpriteComponent> entity, EntityManager manager)
    {
        _entities = manager;
        _spriteSystem = manager.System<SpriteSystem>();
        if (!manager.TryGetComponent<HumanoidAppearanceComponent>(entity.Owner, out var humanoid))
            return;
        _humanoid = humanoid;
        _originalColor = humanoid.EyeColor;
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

        entity.Comp[_spriteSystem.LayerMapReserve((entity.Owner, entity.Comp), HumanoidVisualLayers.Eyes)].Color = _originalColor;
    }
}
