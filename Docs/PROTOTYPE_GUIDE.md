# Full prototype guide

Open `C:\Work\Learning Game\LemkoGames` in Unity 6000.3.13f1. Open `Assets/Lemko/Scenes/Village.unity` and press Play. Each location opens its own scene under `Assets/Lemko/Scenes/Activities`.

## Play each activity

| Activity | Interaction |
|---|---|
| Shop | Read the Lemko list and click matching product pictures. Correct streaks increase points; wrong products cost one life. |
| Farm | Drag unlimited animal copies into the pen, or click a tray animal to add it. Click a placed animal to remove it. Submit checks type and quantity. |
| House | Switch rooms with arrows. Drag furniture to one of the slots; clicking a tray item puts it in the first slot. Click placed furniture to remove it. Submit checks the current room and requested item. |
| Family Tree | YOU is highlighted. Follow the connections and click the requested relative. Valid people are explicit task data, so multiple acceptable people can be configured. |
| Memory | Match one picture card and one Lemko word card. Four pairs by default. A mismatch does not cost a life. Face-down cards have no hints or audio. |
| Numbers | Read/listen to the supplied number word and click the numeral. The prototype uses clearly marked demo number text until verified Lemko is supplied. |
| Alphabet | Use the picture and Polish meaning to spell the Lemko word. Click tiles or drag them into slots. Click a filled slot to return its tile. Submit checks the spelling. |
| Weather | Choose a weather picture and set the temperature slider. Submit checks the explicit weather and temperature range. |

Most sessions have five tasks. After a correct answer, press Next; after the last task, press Results. Memory is one four-pair board per session. Results record points, mistakes and hints. Lives are shared across scenes; zero lives offers a fresh attempt or unlimited practice.

Starting lives are configured in the existing `Assets/Lemko/Data/Shop.asset` and apply to the whole session. Tray layout limits are four animal/furniture types and six weather choices; Memory supports 2–6 pairs. Larger content pools need a tray paging/layout extension rather than placing answers offscreen.

Hold a picture or Lemko prompt for three seconds to reveal a translation. The hint counter increases once per reveal. Audio controls are present, but recordings are not required for the prototype. No difficulty selector is implemented.

## Replace content

`Content/CSV/vocabulary_master.csv` is the master vocabulary table. The original 47 Shop entries are retained. New entries are either sourced from your family sheet and marked unverified, or visibly marked `[DEMO: ...]`. Do not treat demo words as Lemko translations.

Keep `id` stable. Translation columns are `text_lemko`, `text_pl`, `text_en`; additional `text_<language-code>` columns are stored too. The current hint selector exposes Polish and English only. `sprite_id` and `audio_id` match filenames without their extensions.

Each activity has an `activity_<name>.csv` table referencing those IDs. Family Tree also uses `family_people.csv` for person IDs, positions, and up to two parent connections. The active tables are directly under `Content/CSV`; older templates and source files are reference/archive material.

Choose **Lemko > Import all prototype CSV tables** after changes. This imports the shared vocabulary and all activity definitions. It rejects unknown IDs, missing required translations, duplicate task IDs, unavailable answers, and invalid ranges. Missing recordings and unverified language are development warnings. CSV is Editor-only; the game runs from imported Unity assets.

CSV content lists are authoritative on reimport. Inspector session lengths and other settings are retained; Inspector task/list edits are replaced by CSV values when you reimport. Existing ScriptableObject references are retained. No assets are silently deleted.

## Assets and layout

- `Assets/Lemko/Sprites/Placeholders/Activities`: new animal, furniture, family, room, number and weather pictures.
- `Assets/Lemko/Sprites/Placeholders/Products`: temporary Shop pictures.
- `Assets/Lemko/Sprites/Products`: final supplied pictures, including the original carrot.
- `Assets/Lemko/Audio/Pronunciation`: add matching `audio_*.wav` or `.ogg` files here.
- `Assets/Lemko/Data/Activities`: activity definitions, including session length, pools, criteria, family positions, Memory delay/pairs and spelling limits.
- `Assets/Lemko/Prefabs/Words/ActivityToken.prefab`: shared world-object presentation. Change backing, image sizing and text styling here.
- Each activity scene has an **Activity UI** object with its controller and a separate **[Activity] World** root. Canvas contains prompts/buttons/HUD only; the playable tokens and placement areas are actual scene-world objects.
- Controller **Content Offset** and **Content Scale** position the generated task layout as a group. Family positions are directly editable data. Fine placement algorithms for other generated trays are in their focused controller scripts; the prototype does not yet provide a full visual layout authoring tool.

When replacing a placeholder, retain its `.meta` file/GUID and asset key. Move it through Unity's Project panel. Avoid two assets with the same filename/key. Import sprites as Single sprites, then test their visual size. The original carrot still has its supplied white background.

## Rules and persistence

Spelling uses Unicode NFC text elements, preserving case, accents and apostrophe distinctions. Spaces, apostrophes and hyphens are fixed slots. Repeated letters use distinct reusable tiles. Words are limited to 12 total slots in this prototype; start short.

Wrong editable answers cost a life only on Submit. Repeating an unchanged wrong answer is ignored. Correct answers lock until Next. Farm starts with an empty pen; House starts with empty rooms for each task. Ordinary Memory mismatches never consume lives.

Session-only: lives, practice mode, the current task, placed objects, flipped cards, current score/streak and hint count. Persisted: audio/language preferences, encountered vocabulary, completion counts, last/best points and simple result details. Old Shop saves remain compatible. No accounts or network services are used.

## Safe setup and validation

The setup tool creates gameplay only in activity scenes that do not already contain a LearningActivity. It preserves existing gameplay scenes on rerun. It keeps original pre-upgrade scene/code/content backups under `Docs/Backups/BeforeFullPrototype`. Do not run older milestone/import tools on the current content; those are retained for historical reference.

See `Docs/PROJECT_STATUS.md` for exact current validation results and remaining limitations. No player builds are made unless requested. Before replacing all content, test every location once, both a correct and wrong answer, return to Village midway, reopen it, test hints, and check a smaller Game view. Hardware mouse/touch and platform testing remain necessary even when automated Editor checks pass.
