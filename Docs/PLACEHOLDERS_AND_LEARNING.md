# Placeholder art and next design steps — 2026-10-17

## Where the art is

All new temporary artwork is in **Assets/Lemko/Art/Placeholders**:

- Products: ph_apple, ph_pear, ph_bread, ph_cheese, ph_egg, ph_milk, ph_water.
- Backgrounds: ph_shop_background and ph_village_background.

These are original simple geometric PNGs created for the prototype. Product PNGs have transparent backgrounds. Your supplied carrot remains in Art/Sprites/Products/demo_carrot.png, unchanged.

Open **Assets/Lemko/Scenes/LemkoVillage2D.unity**, press Play, and enter Shop. Product pictures are assigned through Data/Vocabulary, so the existing WorldWord prefab automatically displays them. Backgrounds are SpriteRenderers outside Canvas. They have no colliders. UI remains on Canvas. The Shop backdrop is under Shop World and follows its visibility; the village backdrop sits behind the menus.

To inspect Shop art without playing: temporarily enable Shop World in the Hierarchy and disable the Village UI panel so it does not cover the view. Use the Scene view's 2D mode. Products are instantiated during Play; slots remain editable scene Transforms. Restore the panel state afterward or simply enter Play, where Session sets navigation visibility.

Replace temporary product art by assigning a final Sprite to its vocabulary asset. Put final assets in Art/Sprites/Products; do not overwrite placeholder source files unless intentionally editing the placeholders. Replace backgrounds through their scene SpriteRenderer Sprite fields. Current backgrounds are 1600 x 900 pixels at 100 pixels per unit, covering 16 x 9 world units. Match those proportions or adjust scale/camera deliberately.

The Editor menu **Lemko > Art > Apply simple placeholder art (preserve assigned sprites)** imports the files and fills only missing product sprite references. It does not replace your assigned carrot or final product art. Existing background objects are preserved on rerun. A pre-change scene backup is kept outside Assets at Docs/Backups/LemkoVillage2D-before-placeholder-art.unity.txt. The generation script is in Tools/make_placeholder_art.py; it outputs staging PNGs into Tools/PlaceholderArt, not directly over final assets.

No player builds were created for this art pass. Older EXEs do not include this artwork. Test the project in Unity. The art pass compiles/imports and validates scene references; full visual and mouse acceptance still needs the visible Editor.

## Scene recommendation

Keep **one main scene for this small solo game**, with shared Session, audio, tooltip, camera, EventSystem and HUD. Group each activity's world objects and UI separately; use reusable activity prefabs when those activities are implemented. Activate one activity at a time, and give each controller explicit Begin/Close cleanup. Keep answer rules out of a global all-games script.

This keeps shared lives/preferences simple and allows quick layout comparisons. The tradeoff is a larger Hierarchy and all referenced assets potentially loaded together. Eight small static 2D activities are a reasonable starting scope for it, but actual mobile memory use must still be measured.

Separate scenes become useful if activities gain large assets, independent lighting/cameras, or scene editing becomes cumbersome. Then use a small persistent bootstrap scene for shared services and load one activity scene additively. Do not duplicate Session/EventSystem/audio in each scene. Unity supports multi-scene workflows: https://docs.unity.com/en-us/engine/6000.3/manual/working-with-scenes/multi-scene-editing

No multi-scene migration was performed in this art pass.

## Highest-value learning improvements (proposals, not implemented)

1. **Move from copying text to recognizing meaning.** The current Shop labels repeat the shopping-list words, so players can match spelling without knowing the meaning. Keep a guided introduction with words and pictures, then offer picture-only product labels. Keep hover translations and explicit audio available; hints should help rather than punish.
2. **Revisit words later.** Record per-word attempts, first-try correctness and hint use. Bring difficult words back after several other questions and in later sessions. Do not call a word mastered after one correct click. Spacing and retrieval practice have strong evidence for retention; our exact scheduling should start simple and be tested with learners. Review: https://doi.org/10.1038/s44159-022-00089-1
3. **Give each round a small purpose.** A villager needs ingredients, a farmer needs animals, or someone is furnishing a room. Short requests and a visible completed basket make progress understandable. Use only supplied, reviewed Lemko for prompts.
4. **Teach through errors.** On a wrong answer, show which word the selected object represents, allow replaying it, then revisit the target later. Avoid feedback that only says wrong. Keep practice immediately available and avoid timers during introductory learning.
5. **Reward learning with visible village progress.** A small stamp, repaired fence, new flower or friendly thank-you is enough. Reward completing useful practice, not only flawless streaks or speed. Cosmetic progress must not lock other learning activities.
6. **Test the same word in different forms.** Picture-to-word, audio-to-picture and spelling can share entries. Introduce a form first, then mix it into later tasks. Verify that a reward or matching trick cannot substitute for learning the vocabulary.

Recommended next order: first approve the readable Shop layout and real content; then implement guided/picture-only presentation and useful error feedback; then per-word review scheduling. Add decorative rewards after all eight activities are playable. These changes require gameplay/data work and are not part of the placeholder-only edits.
