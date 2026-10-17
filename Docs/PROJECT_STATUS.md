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
# Project status — 2026-10-17

Current project: C:\Work\Learning Game\LemkoGames; Unity 6000.3.13f1. Supersedes the earlier Unity 6.6 workspace for ongoing development.

## Implemented

- Unity 6.3-compatible TMP resource importer; reported CS0234 resolved by actual Unity compilation/build.
- New scene: Assets/Lemko/Scenes/LemkoVillage2D.unity. Shop products are actual 2D scene objects, not Canvas Images.
- Shared WorldWord prefab, sprite fitting/placeholders, collider pointer events, explicit pronunciation interaction, world placement slots and camera aspect fitting.
- Existing Shop rules, shared lives, progress, tooltips and audio reused.
- Managed asset folders organized through Unity with GUID preservation. Original Game 1 remains untouched as a reference.
- Supplied carrot imported and assigned; its opaque white background remains unchanged.
- Complete practical tutorial: Docs/START_HERE.md.
- Windows2D build passed; 17 Editor assertions and standalone state/raycast checks at two sizes passed. See Docs/Validation/UNITY63_REPORT.md for exact scope and limits.

## Next work

1. User tests visible 2D Shop and approves layout; supplies transparent sprites, verified vocabulary and recordings.
2. Shared scene drag/drop, Farm and House with explicit Submit and editable intermediate states.
3. Family Tree with explicit valid persons, then Memory with concealed cards.
4. Alphabet and Numbers using verified content.
5. Weather with explicit temperature/weather criteria; completion records for all activities.
6. Illustrated scene village, final content, mobile layout and platform builds.
7. Final regression, actual mouse/touch/browser acceptance and release delivery.

## Decisions

Canvas is reserved for UI; products use SpriteRenderer/Collider2D outside it. Existing scenes are preserved rather than overwritten. No packages or external services added. Only Shop is playable at this stage. Lives/practice remain session-only; preferences, encounters and Shop results persist. The CSV schemas for the other seven activities are templates until those importers/controllers are implemented.

Use Unity AssetDatabase or the Project panel for future asset moves. Keep stable vocabulary IDs and .meta files. Do not use the old Unity 6.6 project as the source for edits unless explicitly comparing a reference.

## Placeholder art pass — 2026-10-17

Added seven product sprites and two SpriteRenderer backgrounds under Assets/Lemko/Art/Placeholders; assigned missing vocabulary artwork and preserved the supplied carrot. Updated LemkoVillage2D in place, with a pre-change scene backup in Docs/Backups. Unity 6000.3.13f1 compiled/imported successfully and scene validation passed (Docs/Validation/placeholder-art.log). Source artwork was visually inspected. Visible Editor/play-mode acceptance remains a local check. No builds were made; previous EXEs are now older than the scene. User preference: edit project/scenes without building unless requested. See Docs/PLACEHOLDERS_AND_LEARNING.md for replacing placeholders, one-scene recommendation, and proposed learning improvements. These learning features are recommendations, not implemented mechanics.


## Scoring, separate scenes and source vocabulary — 2026-10-17

User requested separate scenes, superseding the previous one-scene choice. New entry: Assets/Lemko/Scenes/Village.unity. Eight activity scenes are under Scenes/Activities; only Shop has gameplay. Previous scenes remain intact. Scene registration changed EditorBuildSettings (nine new entries first; existing entries preserved); no build was performed.

Shop now awards 100 times the correct-answer streak, resets the multiplier on mistakes, guards duplicate scoring, and records last/best points. Shared lives/practice/preferences survive scene transitions. There is no difficulty selector or difficulty code. Content/Shop contains the cleaned master with 47 level-1 entries, Shop ID table and 29 future-only words with source-level provenance. Original file copied unchanged to Content/Source. No main word fields contained slash alternatives; notes retain alternatives and uncertainties.

Validation: Unity compilation, scoring/old-save/data and nine-scene missing-reference checks passed. Content/font validator: 0 errors, 55 expected warnings across retained demo and source vocabulary. Editor Play-mode scoring and navigation through all nine scenes passed, including duplicate clicks, multiplier reset, shared lives, practice and fresh attempts (separate-scenes-setup.log, separate-scenes-play.log). No standalone builds or physical/device checks. CSV round trips checked 76 IDs and visually inspected sample previews. Next: review real content/art and implement Farm/House in their own scenes. Current setup guide: Content/Shop/README.md.



