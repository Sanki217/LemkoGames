# Latest milestone: English IDs, complete Shop pictures, delayed hints — 2026-10-17

Applied successfully in Unity 6000.3.13f1. Open Assets/Lemko/Scenes/Village.unity and press Play. No setup rerun is required. Current guide: CONTENT_AND_HINTS.md. Earlier milestone notes below are historical.

- World product labels hidden; all 47 active Shop words have pictures. Added 68 temporary illustrations, including the 29 deferred words (76 food pictures total). Preserved supplied carrot and existing artwork.
- Individual Lemko shopping-list rows and product pictures reveal translations after three seconds of hover or touch hold. Leaving/disabling/changing scenes cancels pending hints. Touch release after a revealed hint does not select a product.
- Each reveal counts once; re-hovering counts a new reveal. No points/lives penalty. Hints reset each round and appear in results; last result persists.
- English food_* vocabulary IDs and sprite_*/audio_* asset keys. Moved assets through AssetDatabase, retaining GUIDs. Saved encounter IDs migrate and deduplicate. Old source tables and mappings remain for traceability.
- Content/CSV is the authoring folder; Assets/Lemko/Sprites holds final and placeholder sprite folders; Assets/Lemko/Audio/Pronunciation holds recordings. Additional text_<code> CSV columns are stored; current hint UI remains Polish/English.

Exact validation (all Editor-only, no builds):

1. Docs/Validation/english-hints-migration.log: LEMKO_ENGLISH_HINT_MIGRATION_OK and LEMKO_ENGLISH_DATA_CHECKS_PASS. Compilation succeeded, all 76 pictures resolved, migration checked GUID preservation. Content/font validation: 0 errors, 55 development warnings for missing recordings/unverified language across active and retained demo entries.
2. Docs/Validation/hint-play.log: LEMKO_HINT_PLAY_CHECKS_PASS. Automated Play-mode callbacks verified hidden product labels, 8 pictures/5 list rows, list UI raycasts, timing, cancellation, reveal counts, translated text, touch suppression, screen bounds, results, round reset and scene-exit cleanup.
3. Docs/Validation/scene-regression.log: LEMKO_EDITOR_SCENE_PLAY_CHECKS_PASS. Verified scoring/streak reset, duplicate answer/completion protection, nine-scene navigation, shared lives, zero-life practice and fresh attempt.
4. Docs/Validation/english-reimport.log: LEMKO_ENGLISH_REIMPORT_CHECKS_PASS. All 47 object references and GUIDs retained on reimport; Unicode/quoted CSV parsing, save migration, persisted hint count and invalid-save fallback passed. Font/content checks again 0 errors, 55 development warnings.
5. Static inspection: no shop_*.asset files remain in the vocabulary folder. 79 sprite PNGs: 76 food pictures, 2 backgrounds, 1 fallback. CSV preview and placeholder contact sheet visually inspected.

Limitations: headless Editor tests do not verify final visual layout, physical pointer/touch input or device/platform rendering. Test visibly at your usual resolution and a smaller window. No final recordings supplied; language remains unverified. Shop is playable; the other seven activities still have scene placeholders. No player builds were made.

---

