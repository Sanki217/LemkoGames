# One vocabulary sheet for the whole game

Edit **Content/CSV/vocabulary_master.csv**. This is the only spreadsheet required for the normal workflow. It currently contains 344 words and 34 alphabet entries, including reference-only content; not every row is selected for active gameplay.

1. Open the file in Excel using UTF-8 (Data > From Text/CSV if characters look wrong).
2. Edit words, translations, asset keys, tags and the done checklist. Keep established IDs unchanged.
3. Save as CSV UTF-8. The importer supports quoted fields, commas inside values, Unicode, and comma/semicolon Excel exports.
4. In Unity, exit Play mode and choose **Lemko > Content > Import master vocabulary**.
5. To import a downloaded file elsewhere, use **Lemko > Content > Choose master vocabulary CSV...**. This imports that file without copying it over the canonical project CSV.

Columns: id, text_lemko, text_pl, text_en, sprite_id, audio_id, tags, done, letter_uppercase, phonetic_pl. Future translation columns may be added as text_de, text_sk, etc.; the current player language selector still offers Polish/English. The done column is your manual checklist (TRUE/FALSE), initially FALSE. It does not unlock content or mark a translation as verified. CSV cannot store clickable checkbox controls. A spreadsheet app may display those boolean values as checkboxes, but exporting back to CSV retains only the values. Existing internal notes/verification metadata is preserved when the columns are absent; older CSVs containing them remain supported.

## What stays in Unity

The importer updates existing shared vocabulary assets in place. All eight games retain their references. It never rebuilds activity pools, task quantities, weather temperatures, family answers, people/layouts, furniture/room requests or other gameplay settings. Alphabet rows update only the letter definitions, as described below. Tags are descriptive metadata; changing a tag does not silently change gameplay.

New rows create vocabulary assets, but do not automatically appear in games. Assign them in the Inspector to the appropriate definition under Assets/Lemko/Data/Activities (or Assets/Lemko/Data/Shop.asset). Add explicit task criteria where required. Use **Lemko > Content > Edit activity settings in Inspector** as a shortcut. Adding a language word does not invent a relationship, number value or weather criterion.

Removing a CSV row does not delete its asset or remove it from a game. The importer warns and preserves it; explicitly remove its Inspector references if you no longer want it used. This protects existing hand-authored work and saves. Missing optional art/audio remains a warning; invalid/duplicate IDs or missing required translations prevent import before vocabulary assets are changed.

## Former supporting spreadsheets

The old activity tables, alphabet-letter table, family-person table, source mappings, migration table and 29 deferred Shop words are preserved under Content/CSV/Archive/BeforeSingleMaster. They are historical/recovery data, not sheets you need to maintain. The 29 deferred Shop words are not part of the active master or gameplay; move a desired entry into the master and configure its activity when you choose to introduce it. Alphabet letter definitions are now edited in the master CSV using the additional letter columns.

The old normal multi-table menu was removed. Internal legacy recovery code remains explicitly separate; routine imports and older setup import calls use only the master. Imported assets are used at runtime, including WebGL; CSV files are Editor authoring inputs only.

## Validation

SingleMasterChecks.Verify imports a temporary folder containing only the master twice, compares every existing vocabulary GUID and the byte contents of Shop/seven activity assets, then validates the configured activities. No player builds, gameplay simulation or real save operations. See PROJECT_STATUS.md for the execution result.


## Game tags and alphabet rows — 2026-10-29

Tags are game names only: shop, farm, family, house, weather, numbers, alphabet. Rows are grouped in that order. Memory continues to use its configured shared vocabulary pool; it does not need duplicate word rows or a separate tag.

For ordinary word rows, leave letter_uppercase and phonetic_pl empty. For an alphabet row, use tag alphabet, put the lowercase letter in text_lemko, the uppercase letter in letter_uppercase, and the supplied Polish phonetic notation in phonetic_pl. text_pl/text_en and sprite_id can be blank for letter rows. These are letter definitions, not translatable word entries, and are not inserted into vocabulary word pools. IDs/audio keys match the existing letter definitions. Phonetic notation is supplied learning notation, not an audio recording or a verified IPA transcription.

## Release vocabulary and recordings

The November 13 release uses the supplied 344 words and 34 alphabet letters. All 234 supplied WAVs are linked by the exact audio_id in the master. Numbers (110 entries) and alphabet letters (34) await recordings. Words remain playable without audio.

For the next delivery, copy WAV files to Assets/Lemko/Audio/Pronunciation with filenames matching audio_id plus .wav. When replacing a take, overwrite the WAV and keep its .meta file. Choose **Lemko > Content > Import master vocabulary**, then **Lemko > Content > Validate release vocabulary**. Alphabet recordings play on letter placement.

The release build checks the database, recorded pronunciations, alphabet and activity dependencies against the project CSV. If you retire another row, also remove its activity references and its database entry before building. The activities reference vocabulary by stable IDs.

Choose **Lemko > Build > Windows release** to build the Village and eight activities in Builds/WindowsRelease.
