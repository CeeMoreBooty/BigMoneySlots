# Checklist — Assets/Audio

## Sound Files
- [ ] spin.wav — plays when reels begin spinning
- [ ] win.wav — plays for a small/medium win
- [ ] bigwin.wav — plays for jackpot or large win (distinct, celebratory)
- [ ] click.wav — plays on button taps (short, snappy, < 0.2 s)
- [ ] coins.wav — plays when coins are credited to balance

## Unity Import Settings (each file)
- [ ] Audio Clip import settings reviewed in Unity Inspector
- [ ] Load Type: `Decompress On Load` for short SFX (< 1 s), `Compressed In Memory` for longer clips
- [ ] Compression Format: ADPCM (best for SFX), Vorbis (for music/long clips)
- [ ] Force To Mono: ✅ enabled for all SFX (saves memory on mobile)
- [ ] Normalize: enabled (consistent volume levels)
- [ ] Sample Rate Setting: Preserve Sample Rate

## Integration
- [ ] All five clips assigned to the appropriate `AudioSource` components in the scene
- [ ] Master volume slider in settings saves to `PlayerPrefs` and applies on load
- [ ] SFX volume and BGM volume are separate mixer groups (Unity Audio Mixer)
- [ ] Audio does not play during `OnApplicationPause(true)` (paused correctly)
- [ ] No missing audio references — all `AudioSource.clip` fields assigned
- [ ] Spin sound loops while reels are spinning and stops when all reels stop

## General
- [ ] No copyrighted audio — all clips either original, licensed, or royalty-free with attribution
- [ ] All `.meta` files committed alongside every `.wav`
- [ ] Total audio asset size reviewed — keep under 5 MB for fast download
