# Checklist — Assets/Art

## Sprites/Symbols/
- [ ] Bell.png — imported as Sprite (2D and UI), Texture Type = Sprite
- [ ] Cherry.png — imported as Sprite
- [ ] Scatter.png — imported as Sprite
- [ ] Seven.png — imported as Sprite
- [ ] Wild.png — imported as Sprite
- [ ] All symbol sprites same pixel dimensions (recommended: 256×256 or 512×512)
- [ ] All symbol sprites have transparent backgrounds (PNG, no white fill)
- [ ] Each symbol visually distinct at small sizes (test at 64×64 on screen)
- [ ] Sprites assigned to corresponding `SymbolData` ScriptableObjects

## Sprites/UI/
- [ ] Background.png — resolution matches target aspect ratio (1080×1920 baseline)
- [ ] Button.png — 9-slice borders set in Import Settings for resizable buttons
- [ ] ButtonHighlight.png — 9-slice borders set, alpha correct for pressed state
- [ ] Panel.png — 9-slice borders set for resizable modal backgrounds
- [ ] SpinButton.png — large enough to be tappable (min 120×120 dp)
- [ ] All UI sprites set to Texture Type = Sprite, Filter Mode = Bilinear
- [ ] Sprites packed into a Sprite Atlas (`Assets/Art/Sprites/UIAtlas.spriteatlas`) to reduce draw calls

## Resources/Sprites/ (Reel Skins — loaded at runtime)
- [ ] ClassicSlots.png loaded correctly by `SlotGameLoader` for classic variant
- [ ] JungleJackpot.png loaded correctly for jungle variant
- [ ] PennyParadise.png loaded correctly for penny variant
- [ ] All three reel skin textures same dimensions as symbol sprites
- [ ] Textures in `Resources/` folder are set to Texture Type = Sprite

## General
- [ ] All textures use power-of-2 dimensions where possible (performance on older GPUs)
- [ ] Max texture size set appropriately (2048 for backgrounds, 512 for symbols/buttons)
- [ ] Compression: Android → ASTC 6×6 (quality/size balance)
- [ ] No duplicate or unused textures committed to the repo
- [ ] All `.meta` files committed alongside every `.png`
