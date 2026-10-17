# Shop CSV authoring and scene update — 2026-10-17

## Start here

Open **Assets/Lemko/Scenes/Village.unity** in Unity 6000.3.13f1 and press Play. The village now opens separate scenes in `Assets/Lemko/Scenes/Activities`: Shop, FamilyTree, Farm, House, Memory, Numbers, Alphabet and Weather. Shop is playable; the other seven are explicitly labelled scene placeholders. The older LemkoVillage2D and Game 1 scenes are preserved as references.

Scenes were added to Editor Build Settings for navigation, with existing entries preserved. No player builds were made. Old EXEs do not contain these changes. Directly opening Shop and pressing Play also works: it starts a fresh session if none exists.

## Points and multiplier

Each new Shop round starts at 0 points. Consecutive correct selections award **100, 200, 300, ...**, matching the original Game 1 rule. A wrong selection costs one life, keeps already earned points, and resets the next correct award to 100. Duplicate clicks award nothing and do not deduct additional lives.

The HUD shows points and the multiplier for the **next** correct selection. Results show the round total and best total. Last and best Shop points persist alongside previous progress; old saves without these fields remain readable. Best points include practice rounds. Streak and current round points reset when starting another round or leaving/re-entering Shop. Lives and practice mode are session-only and survive scene changes; quitting starts a fresh life session next launch.

## The three CSV files

- **vocabulary_master.csv:** 47 level-1 vocabulary entries used now. One row per word, one header, UTF-8 with BOM for Excel. All supplied main word forms and translations are retained.
- **activity_shop.csv:** those same 47 stable IDs, defining the active Shop vocabulary pool. The controller draws eight products per round and requests five of them. Large pools no longer require displaying every product simultaneously.
- **vocabulary_shop_later.csv:** 29 entries from source levels 2 and 3, retained for future work. `source_level` is authoring provenance only. This file is not imported or used by gameplay.

There is **no difficulty selector, difficulty configuration or difficulty logic** in the game. Only source level 1 is active. Keep the later-level file separate until we implement those requirements together.

The original source is preserved unchanged at `Content/Source/shop_original.csv`; your Downloads file was not modified. Its command `Што треба купити:` is used as the Shop list heading. There were no slash-separated alternatives in the main word fields. The main form `сметана` was retained; the alternate `верхнина` stays in notes. The potato variants and other original notes are also retained. `??` for buns remains visible for review. No Lemko spelling or translation was silently corrected or invented, including source English spellings in the future-only file.

## Master columns

| Column | What to enter |
|---|---|
| id | Permanent unique ASCII identifier, e.g. shop_chleb. Never change it when correcting a translation. |
| lemko | One chosen main Lemko form. Do not put a section heading or several slash-separated spellings here. |
| polish | Polish meaning; preserve singular/plural intentionally. |
| english | English meaning. Required by the current importer. |
| sprite_key | Exact Unity asset path or an unambiguous filename stem; blank retains an existing Inspector assignment. |
| audio_key | Exact recording asset path or an unambiguous filename stem; blank retains existing audio. |
| tags | Optional tags separated with a vertical bar, e.g. shop\|food. |
| verified | true after language review, otherwise false. Currently false because this conversion is not linguistic verification. |
| notes | Editorial/context notes; imported into the vocabulary Inspector, not used as answer logic. |

Current artwork is linked for bread, eggs, milk, cheese, water, apples, pears and the supplied carrot. Other products use the existing colored placeholders and word labels until you supply their images. Keep `shop_jajka` etc. even if the picture filename is still `ph_egg.png`; a stable ID does not require renaming existing art. For new final files, using the vocabulary ID as the filename is easiest: `shop_chleb.png`, `shop_chleb.wav`.

## Import and preview

1. Open vocabulary_master.csv in Excel using UTF-8 if encoding is requested. With Polish Excel, use Data > From Text/CSV and choose comma if double-clicking puts everything in one column. Do not rename the file to XLSX.
2. Review the words and notes. Add assets or adjust translations. Keep the existing IDs.
3. Save as CSV UTF-8. Quotes around commas or multiline notes are expected and supported.
4. In Unity choose **Lemko > Import vocabulary and Shop CSV...** and select `Content/Shop`.
5. Run **Lemko > Validate imported content and font**. Missing optional art/audio and unverified words are warnings; missing translations and unknown IDs are errors.
6. Open Village and play Shop. `Data/Shop.asset` has Task Count 5 and Displayed Product Count 8. These control round size and shelf capacity, not difficulty. The configured scene supports at most 12 displayed products.

The active file has already been imported. Reimport updates managed assets in place, preserving references. The old demo assets are retained but are not part of the Shop pool. Do not run the legacy **Run milestone data checks** menu on your authored content: it intentionally reimports the old starter CSVs.

## Do the other activities the same way

Keep one combined master vocabulary table as the project grows. Append new words, or reuse an existing ID if the exact same word/meaning is used by another activity. IDs beginning `shop_` can still be reused outside Shop; the prefix is not a gameplay restriction.

Each activity table references IDs from the master and adds only its own rule data. For example, Family Tree needs `reference_person_id` and `accepted_person_ids`, Farm needs an animal vocabulary ID and quantity, and Numbers needs a supplied number word plus numeral. Keep person/slot/room identifiers distinct from vocabulary IDs. The templates in Content/Templates describe proposed future schemas; only Shop import/gameplay is implemented today. Do not assume creating a CSV implements an activity.

For new sheets: one header, one record per row, no merged cells, blank section separators or repeated headings. Put comments in notes, never extra heading rows. Keep all required translations filled. Submit the CSV and matching assets together for each next milestone.

## Validation performed for this update

- Spreadsheet CSV round-trip checks: 47 active rows, 29 future rows, 76 distinct IDs; no slash variants required selection. Word/translation and original note preservation checked against the source.
- Unity 6000.3.13f1 compiled and generated/registered nine scenes. Static score checks, old-save compatibility, pool validity and missing-script checks passed.
- Content/font validation: 0 errors and 55 expected development warnings (47 source words plus eight retained demo assets, all still missing some optional media or marked unverified).
- Editor Play-mode checks passed for 100/200/300 scoring, duplicate submissions, streak reset, results, all nine scene transitions, shared lives, round reset, zero-life practice and fresh attempts. Logs: Docs/Validation/separate-scenes-setup.log and separate-scenes-play.log.
- No standalone build, browser/device check or physical mouse acceptance was performed in this update. Check visible text/layout and real pronunciation locally as you add media.

Scene structure supersedes the previous one-scene recommendation. The shared session data lives in memory across scene changes; each loaded scene owns its local HUD/audio/tooltip objects. Scene transitions unload the previous scene, preventing duplicated cameras and EventSystems.
