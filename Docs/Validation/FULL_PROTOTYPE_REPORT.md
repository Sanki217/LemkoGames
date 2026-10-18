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

