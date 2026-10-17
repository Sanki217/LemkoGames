# Lemko game: build it together, from setup to delivery

Project: `C:\Work\Learning Game\LemkoGames`  
Editor: **Unity 6000.3.13f1**. Open this inner folder in Unity Hub, not its parent. This project uses URP 17.3, Input System 1.19 and uGUI/TMP 2.0. No new packages are required.

## 1. Open and play the right scene

1. Open Unity Hub, select Add project from disk, and select the folder above. Choose 6000.3.13f1.
2. Wait for import/compilation. Open Window > General > Console. A red compiler error prevents all scripts from running; send its full text if one remains.
3. In the Project panel clear the search, then open `Assets/Lemko/Scenes/LemkoVillage2D.unity`.
4. Press Play, choose Shop, and click products matching the list. Five targets are selected from eight products. Wrong selections cost one life once per product. Choose View results after collecting the list.
5. Try Village and Shop again. Lives remain shared until a fresh attempt or application restart. At zero lives, choose fresh attempt or unlimited practice.

You can also launch `Builds/Windows2D/LemkoVillage2D.exe` when a successful build is recorded in `Docs/Validation/UNITY63_REPORT.md`. Keep the entire build folder together.

The old `Game 1` scene remains a reference. `LemkoVillageUI` preserves the previous Canvas-based prototype. Use **LemkoVillage2D** for new work. Only Shop is currently implemented; seven village locations are planned.

## 2. What lives in the scene and what lives on Canvas

The **Camera** is orthographic and uses `WorldCameraFit` to keep at least 16 x 9 world units visible. `Physics2DRaycaster` sends pointer events to product colliders. The EventSystem uses the existing new Input System.

**Shop World - move slots to arrange products** is a normal scene root, outside every Canvas. Its child slots are ordinary Transforms. Products instantiated into them have SpriteRenderer, BoxCollider2D, a 3D TextMeshPro label rendered in the 2D scene, and WordView. There is no Rigidbody or simulation: products are stationary clickable illustrations. Shelves are SpriteRenderers too.

**Lemko UI** contains only UI: HUD, shopping list, feedback, results, navigation, preferences and the translation tooltip. Its Shop background is transparent and does not intercept product input. The village currently uses placeholder location buttons; a final village illustration can become a scene sprite in a later milestone.

**Session** owns lives, progress, audio and tooltip services. The Shop controller owns answer checking. WordView owns only the word's visual and interaction state. Vocabulary assets never store whether a product was clicked.

## 3. Folder map

| Folder | Put here |
|---|---|
| `Assets/Lemko/Scenes` | Current 2D scene and preserved UI prototype |
| `Assets/Lemko/Prefabs/Words` | Shared `WorldWord` and `UIWord` prefabs |
| `Assets/Lemko/Art/Sprites/Products` | Product sprite PNGs; the supplied carrot is `demo_carrot.png` |
| `Assets/Lemko/Art/Sprites/Backgrounds` | Shop/village/room illustrations |
| `Assets/Lemko/Art/Materials` | Materials specifically used by the game |
| `Assets/Lemko/Audio/Pronunciation` | Verified spoken vocabulary WAV/OGG files |
| `Assets/Lemko/Audio/SFX` | Success, error and button sounds |
| `Assets/Lemko/Fonts` | Font source, license and TMP font asset |
| `Assets/Lemko/Data/Vocabulary` | One ScriptableObject per stable vocabulary ID |
| `Assets/Lemko/Data` | Shared database and activity configuration assets |
| `Assets/Lemko/Runtime` | Runtime C# components |
| `Assets/Lemko/Editor` | Setup, import, validation and build tools |
| `Content` (outside Assets) | Authoring CSVs; not required in a built game |
| `Docs` | This guide, decisions, status and validation evidence |
| `Builds` | Exported players; don't place them under Assets |

Existing unrelated assets and package resources are preserved. Empty old folders can remain: they are harmless. Move Unity assets in the Project panel, which keeps their `.meta` GUIDs; never discard `.meta` files or recreate assets to rename them. Commit Assets, Packages, ProjectSettings, Content and Docs to version control; do not commit Library, Temp or Builds.

## 4. The carrot: your first real sprite

The supplied image is copied unchanged to `Assets/Lemko/Art/Sprites/Products/demo_carrot.png`. It is assigned to `Data/Vocabulary/demo_carrot.asset`. Its current Lemko text is deliberately `[DEMO] Carrot`; replace it only with your verified spelling.

Select the PNG in the Project panel. Inspector settings:

- Texture Type: Sprite (2D and UI).
- Sprite Mode: Single.
- Pixels Per Unit: 512 (the shared WordView also fits the sprite within its configured Picture Bounds).
- Mesh Type: Full Rect is fine for a single static object.
- Filter Mode: Bilinear for this smooth illustration.
- Generate Mip Maps: off.
- Max Size: 2048; development compression is off to avoid visible artifacts.
- Alpha Is Transparency: on; click Apply after changing settings.

The supplied image has a white background. **Alpha Is Transparency does not remove white pixels.** Export a transparent PNG from your drawing application to get a cutout. Crop excess transparent margins consistently so different products fill their slots similarly. Replace the existing file with the same filename; retain its `.meta`. No prefab editing is necessary to update its picture.

Now select `demo_carrot.asset`: Sprite should point to that PNG. Add a verified pronunciation clip to Pronunciation when ready. Set Polish/English translations and Lemko text. Keep Verified off until you have reviewed the language. Do not rename its ID after it has been used in saved progress or activity tables; an asset filename can differ from its stable ID.

## 5. Add the next product

For an existing demo word:

1. Export `demo_apple.png` (or the corresponding existing stable ID) into the Products folder.
2. Set the same sprite import settings and Apply.
3. Select its vocabulary asset; drag the sprite into Sprite. Drag a recording into Pronunciation if available.
4. Play the 2D scene. Products are randomized into slots, so look for the word rather than expecting a fixed position.

For a genuinely new word:

1. Add one row to the master CSV using a permanent unique ID such as `food_cabbage`. Supply verified Lemko plus Polish and English. Do not use spaces or accented characters in IDs; language text can contain them.
2. Add the ID to `activity_shop.csv` if it should appear in Shop.
3. Import using **Lemko > Import vocabulary and Shop CSV...** and choose the folder containing both tables.
4. Select `Data/Shop.asset`. Its Products list controls the pool. Task Count controls the number requested; Starting Lives controls a fresh session. Current supported pool: task count + at least one distractor, maximum 12.
5. Every product needs a configured scene slot. The generated scene has 12. The first N slots are used, where N is product count.

Missing sprite/audio is allowed during development: the word stays playable with a colored placeholder; the Play control is hidden when pronunciation is absent. One AudioSource prevents recordings overlapping.

## 6. Arrange and style the 2D Shop

Do this with Play mode stopped; edits made during Play are normally discarded.

1. Open LemkoVillage2D, select Shop World and enable its checkbox temporarily to see the layout while editing. Session hides/shows it appropriately at runtime.
2. Expand its Product Slot children. Move each slot using the Move tool in the Scene view's **2D** mode. Product instances appear during Play; their positions come from these slots.
3. To preview a product while editing, drag WorldWord into a temporary slot and assign a vocabulary entry and the scene tooltip/audio references in its Inspector. Delete the temporary preview before saving; the controller creates the actual products at runtime.
4. Edit `Prefabs/Words/WorldWord` in Prefab Mode to change all products. Picture Bounds controls automatic sprite fit. Adjust the root BoxCollider2D to cover the visible product, without overlapping neighbors. Adjust the child Word label for label styling and the Speak child for the pronunciation hit area.
5. Put a background sprite under a separate scene GameObject. Set its SpriteRenderer Sorting Order below products (products use 2, labels 3). Keep it outside Canvas. Do not add a collider to a decorative background.
6. Edit UI text sizes/positions under Lemko UI. Keep UI Images' Raycast Target off when decorative, especially full-screen transparent panels. Otherwise they can block clicks reaching products.
7. Test Game view at 1280x720, 800x600 and a portrait aspect. WorldCameraFit preserves the reference area, but portrait polish and touch acceptance are still future work.

Save the scene with Ctrl+S. Setup preserves existing scenes and prefabs, so rerunning setup will not reset your hand edits. Duplicate the scene before experimenting. Build commands target the named 2D scene; if you rename it, update the build tool or use a Build Profile explicitly.

## 7. Vocabulary workbook and eight activity sheets

Name the workbook **lemko_game_content.xlsx**. Use one header row, one record per row, no merged cells or section titles. Export sheets separately as CSV UTF-8:

| Export | Essential columns |
|---|---|
| vocabulary_master.csv | id, lemko, polish, english, sprite_key, audio_key, tags, verified |
| activity_shop.csv | vocabulary_id |
| activity_family_tree.csv | task_id, vocabulary_id, reference_person_id, accepted_person_ids, difficulty |
| activity_farm.csv | task_id, vocabulary_id, quantity, tray_ids |
| activity_house.csv | task_id, vocabulary_id, room_id, allowed_slot_ids, tray_ids |
| activity_memory.csv | vocabulary_id, starter |
| activity_numbers.csv | task_id, vocabulary_id, numeral, option_numerals |
| activity_alphabet.csv | task_id, vocabulary_id, extra_tiles |
| activity_weather.csv | task_id, vocabulary_id, weather_id, min_temperature, max_temperature |

Only master + Shop import are currently implemented; the other seven files are proposed templates. Use `|` for list values. Explicit asset keys can be paths such as `Assets/Lemko/Art/Sprites/Products/demo_carrot.png`. Blank keys retain existing Inspector assignments. Reimport updates existing assets and preserves references; removed rows do not delete assets.

Keep your family draft's 45 rows for review. Fill missing English, choose reviewed main Lemko forms, and keep slash alternatives in notes until variant support is implemented. Person IDs such as `you`, `maternal_grandmother`, `paternal_grandmother` are separate from vocabulary IDs. A general grandmother question must accept both grandmothers. Do not infer answers from the Polish note at runtime.

Do not run starter data tests on your final custom vocabulary: the original Milestone data checks deliberately reimport Content/Starter. Use the non-destructive imported-content/font validator after authoring.

## 8. How we finish the game together

Each milestone should end with a playable scene, explicit validation results and an updated status file. You supply approved words, art and recordings; I implement and verify the mechanics. Send the actual project path whenever it changes and keep one Unity version pinned. Include full Console errors and exact reproduction steps when something breaks.

1. **Shop foundation (current):** test this 2D interaction, approve layout and replace demo art/content. Tell me which screen size and orientation should be the mobile default.
2. **Farm and House:** I add shared world-object drag/drop, tray spawning, pen removal, room arrows, fixed placement slots and explicit Submit. You supply animal words, counts, room/furniture vocabulary and illustrations. Editing an answer must not deduct lives.
3. **Family and Memory:** you approve a fixed person tree and explicit accepted-person lists. I add questions and four picture/word pairs, hidden-card protection, evaluation locking and safe exits.
4. **Alphabet and Numbers:** you supply short verified spellings and explicit number forms/numerals. I implement NFC-aware letter tiles, repeated letters, corrections and numeral options without inventing language forms.
5. **Weather:** you supply character requests, weather IDs and acceptable temperature ranges. I add buttons, slider and explicit submission criteria.
6. **Village art and audio:** you supply an illustration and eight location positions. I replace placeholder navigation with scene objects, integrate final sprites/recordings, review font coverage, preferences and completion records across all eight activities.
7. **Delivery:** I compile/build installed targets and report exact tests. You run visible desktop and real-device acceptance. We fix blockers, then export release builds. Android/iOS/WebGL modules and signing/device requirements are handled explicitly when needed, never installed silently.

For each content batch, provide the updated CSV folder plus assets named by stable ID. Tell me which entries are approved, which are drafts, and which scene changes you made. A useful next request is: “Use this project and implement Farm/House with scene sprites, preserving my current Shop layout.”

## 9. Test checklist before approving a milestone

- Enter, complete, exit and re-enter the activity without duplicated objects/listeners.
- Correct answers succeed; wrong answers deduct once; invalid intermediate edits do not penalize.
- Leave while a tooltip/audio/animation is active; it must stop cleanly.
- Exhaust lives across rounds; fresh attempt and practice must both remain available.
- Replace one sprite and one audio clip, then reimport CSV; references must remain valid.
- Quit and relaunch normally: preferences and completion persist; lives restart. Automated runs use a separate save key.
- Check actual Lemko/Polish glyphs, not just placeholder English.
- Check large and smaller windows; then actual touch devices/browser builds.

Build through **Lemko > Build Windows 2D Shop**. The build scene list is explicit; the existing project-wide list is not replaced. Do not claim all eight activities or mobile/WebGL are complete until their checks pass.

## 10. Troubleshooting

**AssetPackage namespace error:** Unity 6.6's package API was copied into a Unity 6.3 project. The tool now uses `AssetDatabase.ImportPackage`, supported by this Editor.

**No products:** open LemkoVillage2D, not Game 1 or LemkoVillageUI. Enter Shop; check Shop configuration and slot count. Inspect Console errors.

**Products visible but not clickable:** inspect EventSystem/InputSystemUIInputModule, Camera/Physics2DRaycaster, product BoxCollider2D and UI raycast blocking. Avoid overlapping colliders. The automated smoke checks raycasts as well as selection callbacks.

**White rectangle around carrot:** the source is opaque. Supply a PNG with a real transparent background; Unity cannot recover alpha simply by changing its importer.

**Pink artwork:** use a SpriteRenderer-compatible shader for the project's URP pipeline; do not reuse 3D prototype materials indiscriminately.

**Changes disappear on Stop:** edit the scene/prefab outside Play mode and save. Vocabulary is shared data; round selection state is deliberately temporary.

**A file or scene is missing:** check the actual project folder and the Project panel search filter. Never copy Library/Temp from the old Unity version. Read Docs/Validation/UNITY63_REPORT.md for what was actually generated and validated.

## Added placeholder artwork

See [Placeholder art and learning design](PLACEHOLDERS_AND_LEARNING.md). The current Unity scene has seven additional product pictures and Shop/village backgrounds under Assets/Lemko/Art/Placeholders. Test in Unity; the older standalone build has not been updated, as requested.

