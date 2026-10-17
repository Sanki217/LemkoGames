# Shop content and hints

Project: `C:\Work\Learning Game\LemkoGames`, Unity 6000.3.13f1.

## Open the updated game

The migration has already been applied. You do not need to run setup again. For recovery only: save scenes, exit Play mode, then choose **Lemko > Content > Apply English IDs and delayed hints**. This updates existing vocabulary assets through Unity while retaining their GUIDs, updates the managed scenes and WorldWord prefab, and creates ShoppingListRow. It preserves assigned art and archives the previous CSV tables. Wait for `LEMKO_ENGLISH_HINT_MIGRATION_OK` in Console. If there is an error, stop and share that error before continuing.

Open `Assets/Lemko/Scenes/Village.unity`, press Play, then choose Shop. `Assets/Lemko/Scenes/Activities/Shop.unity` can also run directly. The other seven activity scenes remain placeholders.

## What to test

- Products show pictures without names. Click products to answer; the shopping list still displays Lemko words.
- Hover a product or an individual shopping-list word continuously for three seconds to reveal its translation. Leaving early cancels the hint.
- On touch devices, press and hold for three seconds; releasing after the hint should not select the product.
- Each reveal adds one to **Hints used**. Staying over the same word does not keep adding hints. Leaving and revealing it again counts again.
- Hints do not deduct points or lives. The count resets for each new round; the results and last saved Shop result record it.
- Leave Shop during a pending hint, re-enter, and check that no old tooltip remains.

## Folder and naming conventions

| Content | Location | Example |
|---|---|---|
| Final sprites | `Assets/Lemko/Sprites/Products` | `sprite_carrot.png` |
| Temporary sprites | `Assets/Lemko/Sprites/Placeholders/Products` | `sprite_bread.png` |
| Temporary backgrounds | `Assets/Lemko/Sprites/Placeholders/Backgrounds` | `ph_shop_background.png` |
| Pronunciation | `Assets/Lemko/Audio/Pronunciation` | `audio_bread.wav` |
| Vocabulary assets | `Assets/Lemko/Data/Vocabulary` | `food_bread.asset` |
| CSV authoring files | `Content/CSV` | `vocabulary_master.csv` |

Keep IDs stable and English. Translations remain in their original languages. Old IDs occur in the migration mapping and archived source tables so previous saved encounters can be recovered. Do not remove that mapping merely to remove Polish strings.

## Edit vocabulary

Open `Content/CSV/vocabulary_master.csv` as UTF-8 in Excel. It has 47 active Shop entries. Columns:

`id,text_lemko,text_pl,text_en,sprite_id,audio_id,tags,verified,notes`

`id` identifies the meaning, while `sprite_id` and `audio_id` identify asset filenames without extensions. `food_bread`, for example, points to `sprite_bread` and `audio_bread`. Avoid duplicate asset filenames anywhere under Assets. Preserve the supplied text and have language content verified before setting `verified` to true.

Add a future translation as a new `text_<language-code>` column, for example `text_de`. The importer stores additional translations and VocabularyEntry supports lookup by language code. The current hint button still switches only Polish/English; adding another selectable UI language requires a small follow-up code change.

`activity_shop.csv` references vocabulary IDs and controls the active Shop pool. `vocabulary_shop_later.csv` contains 29 deferred entries and is not imported into the active pool. Its source-level metadata is authoring-only: there is no difficulty selection in gameplay.

Use the existing CSV import menu, select `Content/CSV` if prompted, then run the content validator. Runtime uses imported Unity assets; CSV files are not required in the player. Reimport updates assets by stable ID, preserving references; changing an established ID requires an explicit migration.

## Replace art and audio

1. In Unity's Project panel, move a placeholder sprite and its metadata to `Sprites/Products` when replacing it. Keep the filename and asset GUID. Avoid leaving two sprites with the same asset key.
2. Replace the PNG file contents with your final transparent PNG. Check Texture Type = Sprite (2D and UI), Sprite Mode = Single. Use the Inspector preview to confirm orientation and transparency.
3. Add recordings under Audio/Pronunciation using the matching `audio_*` filename. No fabricated recordings are provided; absent recordings are allowed during development.
4. Reimport CSV and inspect the associated vocabulary asset's sprite/audio fields. Test pronunciation in Play mode.

The supplied carrot is preserved, including its white background. The 68 new drawings are simple temporary shapes, supplementing the eight existing pictures, and include the 29 deferred entries. Similar placeholder foods may need final art to be unambiguous.

## Validation boundary

No builds were requested or produced for this update. See PROJECT_STATUS.md and Docs/Validation for the exact Editor checks completed. Automated pointer callbacks do not replace visible mouse/touch testing on your target devices.

