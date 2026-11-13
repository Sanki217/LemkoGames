# Clean interface and unique hints — 27 October 2026

Open Village and press Play; all nine scenes are already updated.

## Behaviour

Shop list entries now show only the Lemko word. Brackets, `[OK]` text and the per-row Hint controls have been removed from the reusable ShoppingListRow prefab. Hover a list word or picture to get its hint; touch still uses a hold. The existing 0.5-second stillness plus one-second ring policy is unchanged.

Hints are counted once per vocabulary ID **within an activity session**. Picture, shopping-list entry and basket copy of the same word share that count. Repeated reveals remain available but add nothing. Alphabet's letter-help action uses the same word accounting, so repeatedly helping one spelling does not inflate the counter. Hints still mark answers as supported learning. Starting/replaying another session resets this session's unique-word set; previous saved result totals are not rewritten.

## Visual changes

- Warm white/light grey surfaces, purple primary actions, mint selection and basket area, rounded controls and restrained shadows.
- Compact top bar with stats, Menu and Village. Audio and hint language remain available inside Menu. The inactive original control references are retained safely for preferences.
- A shared progress bar: activity accepted-task completion, Shop basket count relative to the requested count, or Village gathering contributions. Shop's bar describes basket filling, not proof that its contents are correct; Submit still validates the answer.
- Shop uses an eight-card product area, separate basket and uncluttered shopping list. Short instruction, clear Check basket action, more spacing. Product drawings remain SpriteRenderers/world objects, not Canvas images. Selected products get a mint backing rather than a blue tint over their artwork.
- Menu, save slots, results, tooltip and journal share the lighter styling. Redundant gathering instructions are hidden on activity screens. Family Tree connectors and Memory card captions have contrast appropriate to the new palette.

This is an original friendly learning UI. No Duolingo art, branding, mascot or fonts were copied. The supplied vocabulary and final-art placeholders remain unchanged; the carrot's white background is part of its supplied image.

## Editing

- Scene UI is editable in the Canvas. Root top bar, Session progress, panels and controls retain their references.
- Shared menu: `Assets/Lemko/Prefabs/UI/GameMenu.prefab`.
- List rows: `Assets/Lemko/Prefabs/Words/ShoppingListRow.prefab`.
- Product selection highlight: `WorldWord.prefab > Selection highlight` and WordView's reference. ActivityToken also has a reusable optional selection marker.
- Rounded UI sprites: `Assets/Lemko/Sprites/UI/ui_card.png` and `ui_button.png` (sliced borders).
- Shop panel artwork: `Assets/Lemko/Sprites/Placeholders/Backgrounds/sprite_shop_clean.png`. Product positions are separate scene slot transforms. If changing the arrangement, move slots and update/replace this panel artwork together.
- `Tools/make_clean_ui_art.py` regenerates only these original UI images. Run deliberately; it overwrites those generated images.
- The **Lemko > Setup > Apply clean interface** menu deliberately reapplies layout/styling. It is not needed to play. Do not rerun it after hand-editing layout unless you want to reapply this preset.

## Validation

Unity 6000.3.13f1 compiled and saved nine scenes/prefabs with `LEMKO_CLEAN_UI_SETUP_PASS`; missing-script checks passed. Focused hint-accounting checks passed first use, duplicate ID, another ID, null ID and new-session reset (`LEMKO_UNIQUE_HINT_DATA_PASS`).

Editor authoring previews of Shop at 1280×720 and 960×540 plus Menu at 1280×720 were rendered and visually inspected. These are deliberately populated sample layouts, not a live gameplay run. They do not execute rounds, read/save progress or enter Play mode. Preview-only objects and camera changes were not saved into scenes. Two spacing issues were corrected after the first preview. A final compilation also checks the inactive audio/language-label binding fix.

No player build, gameplay simulation or WebGL run was performed. Other activity layouts received the shared style and contrast changes, but were not each rendered in this pass. Physical pointer/touch behaviour and complete activity acceptance remain for your normal play-through.

Suggested check: open a product hint, open that same word from its list/basket, and confirm Hints stays at 1; hint a different word and confirm 2. Start a fresh session and confirm 0. Try Menu audio/language controls, both Shop outcomes and one light-background activity.

Evidence and backups: external CleanUI folder (`setup-final.log`, `preview-final.log`, `final-compile.log`, `PreviewFinal/`, `Backup/`). All real save data was preserved.
