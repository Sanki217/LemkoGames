# Reviewed improvements — 27 October 2026

Open `C:/Work/Learning Game/LemkoGames` in Unity 6000.3.13f1. Open `Assets/Lemko/Scenes/Village.unity` and press Play. All scene wiring is already saved. Do not regenerate the starter scenes.

## What changed

- House accepts the requested furniture stored in the requested room, even when another room is visible. Extra decoration is allowed. Its submission signature no longer includes the viewed room, so changing the view cannot charge another mistake for an unchanged arrangement. Placement stays freeform; each task still starts fresh.
- House has separate furniture and requested-room hints. The current-room heading has a padded mouse/touch hit area. Placed furniture is larger and the usable room area is slightly wider.
- Alphabet picture, displayed meaning and explicit hold control reveal the exact Lemko spelling. Other activities retain translation hints. No vocabulary text was altered. Letter tiles and slots now have separated, differently coloured surfaces and matching hit areas; repeated-letter and separator rules are unchanged.
- All nine scenes have a circular hint-progress display with a short cue. It appears immediately, fills during the existing three-second timer, then gives way to the hint. A hint is counted only on reveal. Exit, release, drag, disable, rebinding and scene changes clear it through the existing shared cancellation path. Hidden Memory cards still have no hints.
- Farm is animal identification until a Numbers session has been completed. Choose/place one animal; choosing another replaces it without a penalty before Submit. No quantity requests or repair tasks appear before the unlock. After Numbers, quantity and existing repair tasks are enabled, initially with requested quantities up to five. Existing saves with a Numbers completion qualify automatically. No new save schema, reset or migration of player results is required.
- First-time Numbers uses supplied forms in the configured small introductory range, default 1–5. Subsequent sessions use the existing range. Completing Numbers announces the Farm unlock. `Introduce Small Numbers`, `Introductory Maximum`, and Farm `Maximum Quantity` are Inspector settings; these are not player difficulty menus.
- Journal page changes now select a visible word and stop pronunciation from the previous selection. The selected row is highlighted. Eight legacy `[DEMO]` entries are hidden from this view, while their assets and stored history remain intact. None is referenced by the active activity/Shop definitions. No unverified alias migration was guessed.
- Village uses an original elevated placeholder illustration, with eight scene colliders and world-space labels. Hover and completion highlights identify locations. Existing contribution artwork sits near its destination; the completed gathering cloth is at the central table. Alphabet does not add signs. The old background and button grid remain disabled for recovery, rather than being deleted.
- Shared activity pronunciation buttons explicitly say when recordings are not yet supplied. Existing pronunciation assets and language preferences remain unchanged.

## Short manual play-through

1. Enter House. Hold both room labels and the furniture request. Put the requested item in the requested room, switch rooms and Submit. Try an incorrect arrangement and submit twice without changing it; it should count once.
2. In Alphabet, hold the picture, meaning and explicit hint control. Each should reveal the Lemko spelling after the ring fills. Cancel a hold early; the hint count should not rise. Drag and click repeated letters, remove one, then submit.
3. A profile without Numbers completion should see Farm identification only. Complete Numbers and return to Farm for quantities. Your existing save may already qualify; do not erase it to test the locked state. Both Learning and Challenge completions unlock quantities.
4. Open the journal and change pages. The highlighted row, detail and pronunciation should describe the same visible word. Demo entries should not appear.
5. Click all eight buildings. Check their labels and highlights at your usual window size and one smaller landscape size. Completion markers should appear near the corresponding building.
6. Leave an activity during a hint or drag, then re-enter. Confirm that no old hint/progress ring remains.

## Editing the presentation

- Map art: `Assets/Lemko/Sprites/Placeholders/Village/sprite_village_map.png`. Replace the Sprite on **Illustrated village map** when final art is ready. Its current transform deliberately fits the illustration below the HUD.
- Move **Village destinations - move colliders and labels here** children to match a replacement map. Each destination has its activity ID, collider, label and highlight in the Inspector. Keep all eight IDs unchanged.
- Map source: `Tools/make_village_placeholder.py`. Running it deliberately regenerates the two map placeholder PNGs; do not run it over final art.
- Hint presentation: **Hint wait indicator** under each scene Canvas. Adjust ring colour, cue and size there; delay remains on TooltipService.
- House: `Requested Room Label`, room heading, `Placement Area`, `Furniture Scale`, and per-room artwork on HouseController. Final room images can remain empty; furniture is a separate world object.
- Alphabet: `StyleLetter` in AlphabetController currently defines tile dimensions/colours; positions remain in BuildTask. This is a focused prototype layout change, not a complete theme editor.
- Final recordings/art, touch-device acceptance and further layout styling remain your next content pass. Exactly eight activities and separate scenes are retained.

## Validation and recovery

Unity 6000.3.13f1 compiled the runtime and Editor assemblies. The setup run reported `LEMKO_REVIEW_IMPROVEMENTS_WIRING_PASS` for all nine scenes and exited with return code 0. It checked new references and missing scripts. Static inspection confirmed nine serialized hint indicators and no active definition references to legacy demo assets. The original generated village PNG was visually inspected.

No player builds, play simulations, advanced suites or runtime screenshot captures were run. Physical pointer/touch behaviour and final in-game layout are therefore not claimed as verified. The log contains initial licensing handshake/access-token warnings; licensing recovered sufficiently for compilation/setup and successful exit.

Evidence: `compile-setup.log` beside this guide. `Backup/` contains pre-edit runtime scripts, Editor scripts, scenes, prefabs and project status, including prior uncommitted work. The setup is additive and skips already-wired presentation so a later rerun does not reset hand-edited layouts. New source/art files were delivered from `Stage/`; Unity generated their metadata.
