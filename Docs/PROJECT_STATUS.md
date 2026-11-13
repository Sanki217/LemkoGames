# Full prototype milestone — 2026-10-18

Project: `C:\Work\Learning Game\LemkoGames`; Unity 6000.3.13f1. User preference: edit project/scenes, do not create player builds.

## Implemented

Village plus eight separate playable activity scenes. Shop preserved. Added Farm (unlimited-copy tray, removable pen animals, explicit quantity/type submission), House (room arrows, predefined replaceable slots, room/item submission), Family Tree (fixed people and connections, highlighted YOU, explicit accepted person IDs), Memory (four picture/word pairs, concealed hints/audio, evaluation lock and safe cancellation), Numbers (configured 1–99 numeral answers), Alphabet (click/drag tiles, correction, repeated Unicode text elements and fixed separators), and Weather (explicit weather criteria and temperature range).

Extended shared session and local progress for per-activity results, points/streaks, hints and completion. Lives/practice persist across scene changes but are session-only. Wrong duplicate submissions are ignored. Zero-life recovery allows fresh attempts or unrestricted practice. All activity objects use world-space sprites/colliders; Canvas is for UI.

Added 23 new placeholder sprites under `Assets/Lemko/Sprites/Placeholders/Activities`; retained previous Shop art. Added seven activity tables and a family person-layout table. Master vocabulary now has 70 entries (original 47 Shop entries preserved). Family terms come from the supplied source and remain unverified; unsupported language content is visibly marked DEMO. No fabricated pronunciation.

Configuration: `Assets/Lemko/Data/Activities`. Shared token prefab: `Assets/Lemko/Prefabs/Words/ActivityToken.prefab`. Setup preserves already implemented activity scenes and backs up original scenes before their first upgrade. CSV reimport retains definition references and session settings while replacing authoring content lists.

## Validation record

- `prototype-setup.log`: compilation/import and seven scene upgrades passed. Content/font validation: 0 errors, 78 development warnings (missing audio/unverified language, including retained demo entries).
- `prototype-data.log`: seven definitions and GUID-preserving reimport, Unicode NFC/repeated-letter/separator policy, invalid settings/answers and eight activity scene reference checks passed.
- Initial `prototype-play.log`: all seven new activities completed; mistakes, duplicate submits, corrections, save roundtrip, reentry, hidden Memory cards, pending Memory cancellation, lives/practice recovery passed.
- Render checks found missing backing sprites and undersized world text. Repaired explicit fallback references, card auto-sizing, Family Tree reference label/scale, placeholder people, and placement rendering. Screenshots are in `Docs/Validation/PrototypeCaptures`.
- Final regression results are recorded in `Docs/Validation/final-validation-status.txt` and the final per-suite logs. See the current project status for the final post-repair checks.

## Limits and next user pass

This is a functional prototype, not a finished language release. Add verified words and recordings, replace art, adjust styling and layouts, then test with physical mouse/touch on target devices. Headless interaction checks and offscreen camera-space UI captures do not establish mobile/WebGL/player behavior. No builds were produced.

Defaults are deliberately small: Farm/House trays support up to four types; Weather up to six choices; Memory 2–6 pairs (default four); Alphabet up to twelve total spelling slots and four extra tiles. Most sessions use five tasks; Memory uses one board. Fine layout algorithms for generated trays remain in their focused controllers, with group offset/scale and prefab styling available in the Inspector. Full layout authoring is not implemented.

Read `Docs/PROTOTYPE_GUIDE.md` for playing, editing/importing content, replacing assets, save behavior, and local testing. Open `Assets/Lemko/Scenes/Village.unity` to start.

## Final outcome

Functional eight-activity prototype implemented and checked in Unity Editor. No player builds created.

- `prototype-completion.log`: final compilation and visual-reference repair succeeded; data/import/scene checks passed; Farm, House, FamilyTree, Numbers, Alphabet, Weather and Memory each passed full session completion. The final play suite additionally exercised actual WorldDrag callbacks and physics drop lookup, invalid drops, corrections, duplicate submissions, leaving Memory during evaluation, zero-life navigation to another activity with a correct HUD, practice and fresh attempts. No Sprite Tiling warnings remained.
- `shop-scene-regression-final.log`: Shop scoring/streaks, duplicate selection/completion protection, navigation through nine scenes, shared lives and practice recovery passed.
- `shop-hints-final.log`: three-second hints, cancellation, touch-release suppression, hint counts/results/reset, pointer raycasts, bounds and hidden product labels passed.
- `prototype-render-final.log`: graphics-enabled offscreen captures of nine scenes at 1280x720 and 960x540, plus revealed Memory cards. Visually inspected all nine smaller captures, revealed cards, and a full-size House capture. Fixed the defects found. Captures use camera-space UI to include Canvas in the render; physical Game-view/input/device acceptance remains a manual check.
- CSV read-back audit passed: original 47 Shop rows unchanged, 70 unique master IDs, known vocabulary references in all eight activity tables. CSV previews and new art were inspected.
- Content/font validation: 0 errors, 78 development warnings. The prototype deliberately lacks final recordings and verified language for new demo content. Final language review and physical platform testing remain required.

The older milestone notes below are historical. Use PROTOTYPE_GUIDE.md for the current workflow. No setup rerun is needed: open Assets/Lemko/Scenes/Village.unity and press Play.

---
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





## Supplied content update — 2026-10-26

Imported six supplied CSVs: 357 vocabulary records plus 34 letters. Master now contains 404 words; 47 Shop entries unchanged. Original sources and row mappings retained in Content/CSV. Existing asset GUIDs preserved. All supplied words are imported; active pools fit the current layouts, with numbers 1–99, six family questions, ten Alphabet words and twelve Memory words. Alphabet now imports uppercase/lowercase/phonetic reference data and draws extra tiles from the supplied alphabet. No new games, difficulty UI, story/reward mechanics, or builds were added.

Validation: Unity 6000.3.13f1 compiled/imported; content/font checks 0 errors, 412 expected missing-art/audio/unverified warnings (including eight retained legacy entries). SuppliedContentChecks and PrototypeDataChecks passed. PrototypePlayChecks completed seven updated activities and checked incorrect/duplicate answers, corrections, saved results, reentry, Memory cancellation and zero-life recovery. ScenePlayChecks passed Shop scoring and navigation through all nine scenes. All 89 prior data asset/folder GUIDs match the external backup. Final logs and CSV review package are outside the project in the chat's LemkoDeliverables folder. Physical device/player testing remains outstanding; English glosses are drafts and language remains unverified.

The design PDF is deliberately outside the game folder. It proposes improvements and a shared story for exactly the existing eight games; these proposals have not been implemented.

## Village gathering milestone — 2026-10-26

Implemented the first stage of the approved design. Exactly eight games remain. VillageGathering.asset supplies shared requests and one persistent contribution per completed activity; replay cannot duplicate contributions, hints/practice remain eligible, and all locations stay accessible. Completing eight contributions unlocks a saved gold/blue gathering cloth. Alphabet adds no visible village signs. No companion or excluded activity options were added.

House now uses empty room images and free pointer-position furniture placement instead of visible slots. Furniture can be moved, removed, and retained while switching rooms within a task; invalid drops do not penalize lives. Submit checks the requested item and room, not coordinates. A new task clears arrangements. Room artwork and placement bounds are Inspector-editable.

Save schema 2 migrates existing schema 1 data in the same storage key, preserving prior results/preferences/history while beginning the new gathering at 0/8. Per-word records distinguish independent answers, supported corrections and later-day recall; Memory matching is not counted as independent recall. Up to two review words receive session priority. Shop feedback identifies the selected item; results include learning support and story contributions. Score multiplier now caps at x5. Existing shared lives/practice remain for this milestone; the optional-challenge learning-flow redesign is later work.

Validation: Unity 6000.3.13f1 compiled and integrated all nine existing scenes. Story data checks passed old-save migration, eight contributions, duplicates, reward gating, partial-save repair, learning records and score cap. Story Play-mode checks passed free placement at exact coordinates, actual drag callbacks, movement, room switching, invalid drops, duplicate Submit, partial sessions, rewards, no Alphabet signs, reentry and saved navigation. Existing seven-activity, Shop/navigation and delayed-hint regressions passed. Captured nine scenes at 1280x720 and 960x540; reviewed all nine smaller captures. Fixed room-name contrast and village subtitle placement; reviewed targeted fresh/completed story, furnished room and results captures. All tests used isolated smoke saves. No builds or package installs. Physical input/device acceptance remains manual.

Next: editable Shop basket and word journal, then approved Farm/Family/Memory/Numbers/Alphabet/Weather refinements. User exclusions: Shop 3/4/5; Farm 3/4; House 2/4 (free placement supersedes slots); Family 5; Alphabet 3; Weather 2/3/4/5. All word recordings will be supplied. Full accepted decisions, editing guidance and exact log names are in the external StoryMilestone/IMPLEMENTATION_NOTES.md beside this chat's deliverables. Do not regenerate starter scenes; open Assets/Lemko/Scenes/Village.unity and Play.

## Editable Shop basket and word journal — 2026-10-26

Implemented the next approved milestone in the existing Shop and Village scenes. Products can be added/removed freely, including by clicking their smaller world-sprite basket copies. Check basket validates the complete set; an empty basket is not penalized, each distinct incorrect basket costs at most one life, and rechecking the same rejected set is ignored. The list does not reveal item correctness before submission. Correct baskets award the existing capped-multiplier points once; View results completes the gathering contribution. Basket position, spacing and scale are Inspector-editable. Existing three-second hints and hidden product names remain.

Village now has a Word journal showing encountered vocabulary only, with six-row pagination, Lemko/Polish/English search, a practice filter, translated meanings, per-word learning history and pronunciation when supplied. Browsing earns no score or recall credit. The journal is unavailable inside activities, preserves hidden Memory answers, and closes on navigation. Missing recordings and unverified language remain labelled. The supplied CSVs and final asset assignments were not replaced.

Validation: Unity 6000.3.13f1 compiled; setup rerun preserved existing generated objects. Basket pure-data and Editor integration checks passed (editing, exact sets, duplicate guards, world copies, journal search/paging/filter/language/history, navigation and save reload). Updated ScenePlayChecks and HintPlayChecks passed. PrototypePlayChecks passed all seven other activities, errors/duplicates, corrections, hidden Memory, save roundtrip, reentry and zero-life/practice. Journal and basket captures were reviewed at 1280x720 and 960x540. All checks used isolated smoke saves. No builds/packages. Physical device/input acceptance remains manual.

Exact final evidence and editing guide: external BasketJournal/HANDOFF.md, setup-final.log, integration-final.log, ScenePlayChecks.log, HintPlayChecks.log and PrototypePlayChecks.log beside this chat's deliverables. Earlier development-test errors were fixed before final validation. Backups of edited scripts and scenes are in BasketJournal/Backup.

Next: explicit in-activity hint controls beyond existing hover/long press, approved Farm repair arrangements and Family Tree invitation/album presentation; later Memory/Numbers/Alphabet/Weather and optional challenge refinements remain in the external roadmap. This milestone does not finish the full design plan. Test by opening Assets/Lemko/Scenes/Village.unity and pressing Play; no regeneration required.

## Farm repairs, invitations and explicit hints — 2026-10-26

Implemented the next agreed milestone. Farm alternates empty pens with repair arrangements every second task (Inspector-configurable). Repairs cycle missing/extra/wrong-species arrangements; all remain solvable within the twelve-animal capacity, and initial arrangements/free edits never cost lives or score. A standard five-task session shows missing and extra variants; longer sessions reach wrong-species repairs. The existing tray, drag/drop, removal and Submit logic remain.

Family Tree now asks the player to deliver invitations. Correct recipients are highlighted and a temporary session album lists the relationships invited. Explicit valid-person answers remain authoritative. Duplicates/incorrect answers cannot deliver invitations; replay/exit clears the album, while existing completion/gathering progress persists. The tree was resized slightly to make space for the album without overlap.

Explicit mouse/touch Hold for hint (3s) controls were added to six non-Memory activities, and ? 3s controls beside Shop list words. They use the shared tooltip timer, reveal counter and supported-answer recording. Release/exit/task changes cancel pending hints. Existing hover/long-press hints remain. Memory has no new hint control or hidden-answer exposure. Full keyboard/controller hold access remains future accessibility work.

Validation: Unity 6000.3.13f1 compiled and applied scene/prefab changes. FarmFamilyHintChecks passed all repair variants in a six-task session, solvability, free edits, duplicate Submit, explicit hint timing/cancellation/count/support, invitations/duplicates/restart, Shop hints and Memory exclusion. Captured repairs, invitation success and Shop hint at 1280x720 and 960x540; reviewed and fixed album overlap/contrast. Existing HintPlayChecks, PrototypePlayChecks (all seven non-Shop games) and ScenePlayChecks (Shop plus nine-scene navigation/shared lives) all passed. Isolated smoke saves only. No builds/packages or supplied-language/asset replacements. Physical input/device acceptance remains manual.

Evidence and guide: external FarmFamilyHints/HANDOFF.md, setup.log, layout.log, integration-final.log, hint-regression.log, activity-regression.log, scene-regression.log. Original edited code, scenes and list-row prefab are backed up under FarmFamilyHints/Backup. Existing scenes are ready: open Village and Play; no regeneration or wiring required.

Next: approved Memory association checks, contextual Numbers, Alphabet support/repair, then Weather outing feedback and optional challenge versus ordinary learning. Keep all user exclusions and exactly eight activities. Optional two-pen Farm tasks are not implemented in this milestone.

## Memory associations, number tickets and Alphabet labels — 2026-10-26

Memory now follows pair matching with two short picture-to-Lemko association checks, using the matched words in shuffled positions. Ordinary mismatches and association mistakes cost no lives. Matching alone earns no independent recall record; association responses are explicitly supported practice. Duplicate answers are guarded, hidden cards retain privacy, completion waits for the configured checks, and results show both pair/check outcomes. MemoryController Association Checks allows 0–2, default 2.

Numbers now frames numeral choices as issuing gathering tickets, with a bounded world-sprite preview of recently issued tickets. Minimum/Maximum Number fields select an authored content range (default 1–20, available 1–99); invalid/inverted/single-form ranges are rejected. No number words are generated. Supplied CSVs and verification flags are unchanged. Preview is session-only, resets on exit/replay, and duplicate choices cannot issue extra tickets.

Alphabet alternates full spelling with damaged labels (default every second task, two missing letters). Prefilled letters remain editable and automatically mark the task supported. Help: one letter fills/corrects a position, counts one requested hint and marks support; it conserves repeated-letter tiles and retrieves a misplaced occupied tile before disturbing a correct one. NFC and fixed separator policy remain. Submit still checks spelling; no signs appear in Village. Translation hold controls remain three seconds; intentional letter help is a separate immediate support action.

Validation: Unity 6000.3.13f1 compilation/setup passed. Targeted Editor tests passed Memory phase/completion/privacy/duplicate/support/reset, ticket range validation/issuance, repeated letters/apostrophes/occupied tile help/repair/support and cleanup. PrototypePlayChecks passed all seven non-Shop activities including the new Memory flow; HintPlayChecks and ScenePlayChecks passed delayed hints, Shop score/duplicates, nine-scene navigation, shared lives and recovery. Captured all three changed activities at 1280x720/960x540; reviewed the smaller captures and Alphabet at the larger size. Tests used isolated smoke saves and temporary spelling fixtures only. No player builds, package installs or supplied asset replacements. Physical device/input acceptance remains manual.

Exact evidence: external MemoryNumbersAlphabet/setup.log, integration-final.log, activity-regression.log, hint-regression.log and scene-regression.log. Editing/test guide: MemoryNumbersAlphabet/HANDOFF.md. Backups: MemoryNumbersAlphabet/Backup. Existing scenes are wired; open Village and Play.

Next approved milestone: Weather outing feedback and ordinary learning versus optional challenge. Recordings/final art, full keyboard/controller access, audience testing and optional advanced activity variants remain later work. Preserve all user exclusions and exactly eight games.

## Weather outing feedback and learning/challenge choice — 2026-10-27

Ordinary Learning is now the default (also for older saves without the new additive challengeMode preference). Mistakes still produce feedback and reset the score multiplier, but never consume lives or block learning. Village has Start challenge / Switch to learning and explanatory text. Starting challenge explicitly refills starting lives; changing activities/replaying does not. At zero challenge lives, Fresh challenge or Continue learning restarts the activity in the selected mode. Both modes retain hints, points, history and gathering rewards. Mode preference persists in the existing save schema/key; lives and temporary task state remain session-only. Scores are not split by mode. Existing progress/preferences are preserved.

Weather retains its choices and temperature slider. Request framing now describes an outing-planning puzzle; correction identifies missing/wrong weather, too cold or too warm. Success colours the character/blanket, reveals an existing apple placeholder as prepared supplies, and locks the slider until the next task. Outing Illustration and Preview Position are Inspector fields. No simulation, forecast cards, clothing, combined requests or sentence-building were introduced. Slider callbacks now remove only the controller's own listener. Vocabulary and recordings are unchanged.

Validation policy changed at the user's request: no simulation or advanced testing needed; user will report runtime issues. This milestone compiled with Unity 6000.3.13f1 and the scene setup/reference check exited 0 with LEMKO_WEATHER_LEARNING_MODE_WIRING_PASS (nine scenes, Village mode controls and Weather references). No Play-mode simulations, regression suites, new render captures or player builds ran. Runtime/visual acceptance is therefore not claimed. Existing ScenePlayChecks/PrototypePlayChecks were adjusted for explicit challenge entry/HUD wording but not executed. No packages installed.

Guide/evidence: external WeatherLearningMode/HANDOFF.md and compile-setup.log. Backups of modified scripts/scenes and this status document are in WeatherLearningMode/Backup. Open Village and Play; no manual wiring required. The earlier approval-review usage-limit interruption performed no project edits; work resumed after user continuation.

Current core enhancement milestones are implemented. Remaining: user play feedback/fixes, final recordings/art, layouts/UI polish, keyboard/controller accessibility, audience learning/enjoyment feedback and optional advanced activity variants. Maintain exactly eight activities and all recorded exclusions. Continue lightweight checks unless the user requests broader validation.

## Feedback and session recap polish — 2026-10-27

Added AnswerFeedback to all eight activity sessions. Accepted correct answers show awarded points and briefly pulse existing feedback/HUD text; corrections use warm colour emphasis. Memory feedback never fabricates points for unscored pairs/checks. Presentation cannot score, validate answers or block input. Animation cancels/restores on next task, restart and exit. Duration, scale, colours and Animate are Inspector-editable; disabling Animate retains static colour feedback. No sounds, assets, vocabulary, life rules or save schema changed.

Results now show up to three practised words with the current translation language and a direct Next activity button for an unfinished gathering contribution. It falls back to the current activity if that is the only unfinished one, and hides when all eight are complete. Existing replay/Village controls remain. Recap words are session-only, accrue no learning credit, and refresh when the language changes. New UI: Words and next step, Next place under each Results panel.

Validation: Unity 6000.3.13f1 compilation and scene wiring succeeded, process exit 0, LEMKO_FEEDBACK_RECAP_WIRING_PASS for eight scenes. Per user request, no play simulations, advanced suites, fresh captures or builds. User will judge animation feel and layout. Evidence/editing guide: external FeedbackRecap/compile-setup.log and HANDOFF.md. Backups: FeedbackRecap/Backup. All scenes are ready to open from Village without manual wiring.

Next work should follow user play feedback and final asset/layout priorities. Maintain lightweight checks, eight activities, existing exclusions and no builds unless requested.


## Video review improvements — 2026-10-27

Implemented the accepted recap plan: House submission searches stored placement across rooms and accepts matching furniture without excluding unrelated decorations; unchanged arrangements cannot be penalized again simply by switching the visible room. Separate requested-room and furniture hints, padded current-room hit target, larger furniture and wider free-placement area are wired. Alphabet picture/meaning/hold controls now request Lemko spelling through a per-binding hint content mode. Tile surfaces and hit areas are separated. All nine scenes have the shared three-second hint progress ring; hint counting still occurs only on reveal.

Farm identification is always accessible; quantity and repair tasks require a saved completed Numbers session. Existing Numbers completions qualify. First-time Numbers uses supplied 1–5 forms by default; Farm requests default to at most five. Settings remain Inspector-only. Numbers results announce the unlock. No save reset/schema change. Journal selection now stays on the visible page, highlights its row, and stops stale audio; demo entries are filtered without deleting assets/history. All eight demo assets are absent from active gameplay definitions.

Village now has an original elevated placeholder map with eight world-space collider destinations, labels and completion/hover rings, paths, trees and a central gathering table. Contributions moved from the detached strip to their locations; no Alphabet signs. Old grid/background are disabled for recovery. Canvas remains for HUD/menus. Final art/audio was not fabricated or substituted; unavailable activity recordings are labelled clearly. Map source is Tools/make_village_placeholder.py; art is under Sprites/Placeholders/Village.

Validation: Unity 6000.3.13f1 runtime/Editor compilation succeeded; scene setup logged LEMKO_REVIEW_IMPROVEMENTS_WIRING_PASS (nine scenes, required references and missing-script check), return code 0. Static inspection found nine hint indicators and no demo references in active definitions. Generated village PNG inspected. Initial licensing handshake/access-token warnings recovered; no C# compile errors. Per user instruction no play simulations, advanced tests, runtime captures or player builds. Physical input and final in-game layouts require the short user play-through.

Guide: Docs/REVIEW_IMPROVEMENTS.md. External evidence/backups: C:/Users/micha/.codex/visualizations/2026/10/15/01a141c6-6ce4-7612-87ab-db241350669d/ReviewImplementation (compile-setup.log, Backup, HANDOFF.md). Scenes already wired; open Village and Play. Next: user acceptance of fixes, final room/map/object art and recordings, UI/layout refinement and accessibility. Keep exactly eight activities and existing scope exclusions.


## Save/load and first production-quality pass — 2026-10-27

Added one shared GameMenu prefab to all nine scenes (Menu button or Escape). Three manual local progress checkpoints are independent of autosave, show timestamps/progress, and confirm replacement/load. Loading returns to Village and starts a fresh attempt; a recovery checkpoint preserves pre-load progress. Unreadable checkpoints cannot replace the current profile. Saved scope: completed results, word history/encounters, gathering and preferences; current round/streak/placements/lives remain session-only. Old autosaves are preserved. Reduced motion is an additive preference, suppressing decorative answer pulse and tooltip fade. Menu overlay blocks answer input; an already-running Memory reveal animation can finish beneath it.

Hint timing is now 0.5 seconds of pointer stillness, then a one-second ring fill. Movement beyond four pixels restarts it. Existing mouse-hover/touch-hold/explicit-hold semantics, contextual spelling/translation and reveal-only counting remain. Scene and Shop-row labels no longer advertise three seconds. Village journal shortcut moved away from introduction. Shared GamePresentation asset controls Alphabet tile size/spacing/colours; menu palette is used on initial prefab generation, with later menu styling edited in the prefab. Refactored manual storage out of menu/session ownership; explicit parsing failure for manual loads; reduced progress-ring mesh allocation.

Validation: Unity 6000.3.13f1 compilation and setup exited 0 with LEMKO_GAME_QUALITY_WIRING_PASS for nine scenes, required menu references and missing-script checks. Isolated save-data check exited 0 with LEMKO_MANUAL_SAVE_DATA_PASS (checkpoint/recovery round trips, results/preferences preserved, old-version migration, invalid-data rejection). Smoke keys restored/removed; no real saves used. Staged runtime/Editor compiler checks had passed without warnings. Shop-row label received a final text-only update. No player builds, Play-mode simulations, advanced suites or runtime captures. Physical input and layout still require user acceptance.

Open Village and Play; no manual wiring. Guide: Docs/GAME_MENU_AND_SAVES.md. Evidence/backups: C:/Users/micha/.codex/visualizations/2026/10/15/01a141c6-6ce4-7612-87ab-db241350669d/QualityPass (compile-setup.log, save-data-check.log, CompilerCheck, Backup). Next: consistent visual direction/final audio and art, onboarding/pacing, device/accessibility acceptance. This pass starts production-quality work; it does not establish release readiness. Keep all eight activities and prior scope exclusions.


## Save reset tools and WebGL preparation — 2026-10-27

Added Lemko > Saved Data commands for Editor autosave-only reset and all Editor game saves reset. Commands confirm and are disabled during Play mode to avoid stale runtime state writing back. Added a confirmed Reset local data button to the shared GameMenu prefab, covering standalone/browser storage as well as Editor Play mode. It deletes this game's exact autosave/three manual slots/recovery keys, then returns to a fresh Village profile. No DeleteAll and no automatic reset on build/start. Existing real saves were not cleared. LocalSaveData centralizes the original key names and avoids command-line inspection on WebGL players.

Configured WebGL Gzip, decompression fallback, 128 MB initial growing heap; existing caching enabled and threading disabled retained. Company/product IDs, scene order and other settings preserved. Added Lemko > Web > Configure WebGL (no build). Unity 6000.3.13f1 installation has only Windows standalone support: WebGL Build Support is MISSING. User must add it through Unity Hub, switch to a Web build profile and later run browser acceptance. No module/package installed or player build made.

Validation: Unity compilation/setup exited 0 with LEMKO_RESET_WEB_SETUP_PASS; Village-first and all eight activity scene registrations checked. Isolated reset test exited 0 with LEMKO_RESET_DATA_PASS: autosave-only/all-game reset and unrelated-key preservation, restoring temporary smoke keys afterward. Prefab reset reference verified. Settings diff limited to compression, fallback and initial heap. No gameplay simulation or browser target compilation. WebGL compatibility remains unverified until the module and a served browser build are available.

Guide: Docs/WEBGL_AND_RESET.md. Evidence/backups: C:/Users/micha/.codex/visualizations/2026/10/15/01a141c6-6ce4-7612-87ab-db241350669d/WebSupport (setup.log, reset-check.log, Backup). PlayerPrefs live outside builds; repeated local builds reuse platform-specific saves. Editor reset does not clear standalone/browser data; use in-game Reset local data in those environments. Keep no-build/lightweight-validation preference.


## Clean learning interface and unique hints — 2026-10-27

Removed Shop list brackets/status prefixes and per-row Hint controls from the reusable prefab. Words/pictures remain hintable. Central SessionHintUsage counts stable vocabulary IDs once per activity session across all visual copies and includes Alphabet letter help; repeated reveals remain available and supported-learning classification remains intact. BeginLearningSession resets the set; saved historical results unchanged.

Applied an original light learning UI across nine scenes and shared menu/word prefabs: compact HUD, Menu/Village navigation, settings inside Menu, progress bar, white rounded cards, purple actions, mint selection. Shop has separate products/basket/list panels, shorter instructions, clearer Check basket action and no selection tint over drawings. Items remain world sprites. Journal/results/tooltips/menu styled consistently; Family connectors and Memory card captions adjusted for contrast. Hidden top-bar preference labels are accessed with includeInactive to avoid null references. Supplied art/language and real saves preserved.

Validation: Unity 6000.3.13f1 setup/compilation passed with LEMKO_CLEAN_UI_SETUP_PASS; nine-scene missing-script/reference pass. LEMKO_UNIQUE_HINT_DATA_PASS verifies first/duplicate/distinct/null ID and session reset. Rendered and visually inspected Editor authoring previews: Shop 1280x720 and 960x540, Menu 1280x720; corrected instruction/basket spacing and preview camera sorting. Preview objects were temporary, no scenes saved from the preview. Final compile includes hidden-control binding fix. No gameplay simulation, build or browser run; physical interactions and other activity layouts need ordinary user acceptance.

Guide: Docs/CLEAN_INTERFACE.md. Art source: Tools/make_clean_ui_art.py. Evidence/backups: C:/Users/micha/.codex/visualizations/2026/10/15/01a141c6-6ce4-7612-87ab-db241350669d/CleanUI (setup-final.log, preview-final.log, final-compile.log, PreviewFinal, Backup). Shop progress bar reflects basket count, not answer correctness; other activities reflect accepted task completion. Next: user play feedback, final art/audio and deeper individual-screen polish. Exactly eight activities retained.

## Activity entry story/tutorial screens — 2026-10-28

Implemented eight editable introduction definitions and one shared UI prefab, wired into all eight activity scenes. Every scene entry waits for click/tap anywhere (or Enter/Space) before starting the round. Same-scene retries skip the introduction. Farm instructions adapt to Numbers completion. Dismissal is deferred one frame and guarded against duplicates; menu and answer input are blocked while the introduction is open. Save format unchanged.

Validation: Unity 6000.3.13f1 compiled/imported and executed ActivityIntroductionSetup.Apply successfully (exit code 0); all eight introduction references/activity IDs and scene missing-script checks passed. Evidence: external ActivityIntroductions/setup.log, LEMKO_ACTIVITY_INTRODUCTIONS_PASS. No build or gameplay simulation performed. Manual mouse/touch and layout checks remain. Editing and verification instructions: Docs/ACTIVITY_INTRODUCTIONS.md.

## Engagement and presentation pass — 2026-10-28

Implemented the approved improvement pass across the existing eight games. Reused the angled village map, shared gathering story and contribution assets; added a suggested unfinished location and animated contribution reveal. Results now show contribution art, accurate repaired-word counts, independent/supported totals, words to revisit and a primary next-activity action, including practice suggestions after all eight contributions are complete.

Review ordering mixes familiar material with a small amount of new vocabulary and prioritizes words needing support; independent streaks space review at 1/3/7 days. Adds backward-compatible per-word streak/last-attempt fields and per-activity skipped-introduction preferences. Tutorials have Show next time and a persistent Help entry; Help does not restart rounds. Pending Memory evaluation waits while Help/menu blocks input. No existing player saves reset.

Shop basket travel and actionable corrections; Family connection-path highlight; Farm animal settling/idle and type/count corrections; House placement ghost, targeted correction and furniture retained between tasks within a session; Memory mint matched pairs and supplied-audio feedback with review-aware selection; Numbers ticket pulses; Alphabet letter placement pulses and supplied pronunciation on success; Weather selected-weather preview and success reaction. Shared picture hover feedback, smooth progress and reduced-motion handling. Hidden redundant hint controls preserve hover/hold support. Final art/audio still user-supplied.

Validation: Unity 6000.3.13f1 final compilation/import and EngagementSetup.Apply exited 0, LEMKO_ENGAGEMENT_PASS; all nine scenes saved with missing-script checks and tutorial/progress references. Evidence: external EngagementPass/final.log. Static Editor authoring captures inspected: Shop 1280x720 and 960x540, menu, introduction and results; marker LEMKO_ENGAGEMENT_AUTHORING_PREVIEW_PASS in preview.log. Captures are synthetic layout previews, not gameplay tests or save access. The first setup emitted a preview-scene cleanup warning; corrected prefab cleanup to UnloadPrefabContents and subsequent setup completed without that warning. Final result-button colour and House persistence refinement compiled after the captures.

No builds, Play-mode simulation, browser/device runs or advanced tests. Physical interaction/timing and all eight runtime layouts remain subject to ordinary user play feedback. Existing supplied carrot background and other placeholder art remain unchanged. WebGL module availability/release readiness are unchanged from the earlier status. No packages or external services added.

Guide: Docs/ENGAGEMENT_POLISH.md. Safe pre-edit source/scene/prefab backups and staged changes: C:/Users/micha/.codex/visualizations/2026/10/15/01a141c6-6ce4-7612-87ab-db241350669d/EngagementPass. Open Village and Play; no manual wiring required. Next: user acceptance, final assets/recordings, accessibility/device checks and learning feedback from real players.

## Single master vocabulary workflow — 2026-10-28

Normal authoring now requires only Content/CSV/vocabulary_master.csv (404 existing entries, text unchanged). Added Lemko > Content > Import master vocabulary, Choose master vocabulary CSV..., and Edit activity settings in Inspector. Folder-based compatibility calls also import vocabulary only. Vocabulary imports no longer require or read activity_shop.csv and no longer rebuild Shop or any other activity settings. Existing stable asset GUIDs, hand-assigned art/audio when no matching replacement exists, and omitted assets are retained. Omitted rows generate warnings; they are not deleted or removed from gameplay automatically. Tags remain descriptive, not implicit pool changes.

Archived 13 supporting CSVs under Content/CSV/Archive/BeforeSingleMaster, preserving all contents, including 29 deferred Shop words and alphabet letter metadata already represented in the Inspector. Only vocabulary_master.csv remains directly in Content/CSV. Removed the normal multi-table menu; explicit internal legacy recovery code reads archived supporting tables. New vocabulary rows still need assignment to relevant Unity pools/tasks. Required translations and duplicate/invalid IDs are checked before mutation. No spreadsheet language content was invented or changed.

Validation: Unity 6000.3.13f1 compiled and SingleMasterChecks.Verify exited 0, LEMKO_SINGLE_MASTER_PASS. A temporary folder containing ONLY the master was imported twice; existing vocabulary GUIDs were unchanged; byte comparison confirmed Shop and all seven ActivityDefinition assets unchanged; all activity configurations validated. No player builds, gameplay simulation or real save access. Existing missing-media/unverified-language warnings remain development warnings.

Guide: Docs/MASTER_VOCABULARY.md. Evidence/backups/download: external SingleMaster folder (validation.log, Backup, vocabulary_master.csv). Download CSV SHA-256 matches the project's canonical file. Old multi-sheet instructions elsewhere in historical docs are superseded by this guide. Current data, content and code were backed up before import.

## Simplified master and grouped animals — 2026-10-28

Grouped all 33 supplied animals together in the user's order, preserving existing English IDs and the first supplied Lemko variant. These were already present, so no duplicates or invented language were added. Numbers 1–5 now appear with the later number section, and numeric entries are contiguous in numeric order. Master remains 404 unique vocabulary entries.

Removed notes and verified from the CSV, added done (FALSE initially) as a manual TRUE/FALSE authoring checklist. CSV cannot retain clickable checkbox controls. done has no gameplay/verification effect. Importer now accepts a master without verified, preserves existing asset notes/verification when absent, and remains compatible with older CSV columns. Guide updated in Docs/MASTER_VOCABULARY.md.

Validation: independent CSV checks confirmed counts, group contiguity, initial done values, preserved translations/asset keys and exact byte equality of downloadable/project files. Unity 6000.3.13f1 compiled; two consecutive master-only imports passed with vocabulary GUIDs and all eight activity configurations unchanged (LEMKO_SINGLE_MASTER_PASS, exit 0). No builds, gameplay simulation or real save access. Evidence/backups/download: external AnimalMaster folder, validation.log and vocabulary_master.csv. The earlier automatic approval usage-limit rejection made no project changes; deployment succeeded after the user's continuation.

## Game-only master tags and integrated alphabet — 2026-10-29

Used the user's Downloads/Untitled spreadsheet - vocabulary_master (1).csv as authoritative input, preserving all 344 word rows, IDs, translation strings, asset keys and done flags. Simplified category tags and grouped rows in requested order: shop (42), farm (25), family (44), house (74), weather (49), numbers (110), alphabet (34). Added all 34 supplied upper/lowercase letters and Polish phonetic notations, reusing existing stable letter IDs/audio keys. Master now contains 378 rows. Removed input rows were not reintroduced into the sheet; pre-existing Unity vocabulary assets remain retained by the established non-deleting import policy.

Added letter_uppercase and phonetic_pl columns. Lowercase goes in text_lemko. Letter rows intentionally have no invented Polish/English word translations; they update the Alphabet activity's existing letter definitions rather than becoming vocabulary-word assets. Normal word imports still require translations. Alphabet import prevalidates matching case pairs, single Unicode text elements, phonetic notation and duplicate letters before changes. Pools, tasks, placements, life rules and other activity settings remain unchanged. Updated the older fixed-404-word content check to use actual word-row count and exclude letter metadata rows.

Validation: CSV checks passed for 378 unique English-style IDs, valid asset-key names, required word translations, supplied letter pairs, game-only tags and requested grouping. Unity 6000.3.13f1 compilation and two full master imports passed: LEMKO_GAME_TAGS_MASTER_PASS, return code 0, in external GameTagsMaster/validation-closed.log. Vocabulary GUIDs/gameplay-rule snapshots preserved; all 34 imported letter definitions matched the CSV. Earlier batch attempts exited before checks because the interactive Editor was open/shutting down; validation succeeded after closure. No build, gameplay simulation or independent linguistic certification. User-supplied language and pronunciation notation preserved.

Guide updated: Docs/MASTER_VOCABULARY.md. Download and backups: external GameTagsMaster/vocabulary_master.csv and Backup. The done column remains a manual authoring flag only. Final media and independent language verification remain user work.
