# Game menu, save slots and presentation — 27 October 2026

Open `Assets/Lemko/Scenes/Village.unity` and press Play. No setup action is needed: all nine scenes are wired.

## Save and load

Click **Menu** in the top bar, or press **Escape**. Resume/Escape closes the menu without ending the current activity. Escape cancels an open confirmation first.

Three manual slots are separate from autosave. Save a checkpoint in an empty slot; overwriting an occupied slot asks for confirmation. Each slot shows its timestamp, gathering contributions and encountered-word count. Loading asks for confirmation, ends the current round and returns to the village. **Recover progress before last load** restores the recovery checkpoint made immediately before that load. An unreadable slot is identified and cannot replace current progress.

Saved: activity results, vocabulary learning history, encountered words, gathering progress, audio/hint-language/reduced-motion preferences and learning/challenge mode. Not saved: temporary answers, drag positions, current round, score streak and remaining challenge lives. Loading starts a fresh attempt with the configured starting lives. Manual slots and recovery are local to this device/browser, using PlayerPrefs; this is not file export or cloud sync. Existing autosave and existing player progress remain intact.

## Hints

Keep the mouse still over a hintable word or object. After **0.5 seconds** the circle appears and fills over **1 second**, then the hint is shown. Mouse hover does not require pressing a button. Explicit hint controls and touch use press-and-hold/release.

Movement beyond a small four-pixel tolerance restarts the delay and hides a revealed tooltip. Leaving, dragging, disabling or rebinding the target cancels. Only the reveal counts as a used hint. Alphabet still reveals Lemko spelling; ordinary targets reveal the selected translation. Face-down Memory cards remain private.

On each scene's TooltipService, `Still Seconds` controls the first wait, `Hold Seconds` controls the ring fill, and `Movement Tolerance` controls pointer jitter tolerance.

## Quality and refactoring

- One reusable **GameMenu.prefab** now owns menu layout across all nine scenes, with Save/Load, audio, translation language and reduced-motion controls. Scene instances only supply their GameSession reference.
- Reduced motion disables decorative answer pulses and tooltip fade. The functional hint-progress circle remains. The preference persists and is included in checkpoints.
- **GamePresentation.asset** exposes Alphabet tile dimensions, spacing and colours. Menu palette fields are used when the setup tool first creates the menu prefab; to restyle an existing menu, edit the prefab itself. The setup preserves existing prefab/layout edits.
- Manual checkpoint storage is separated from session/navigation and menu presentation. Save parsing reports failure to the manual-load path instead of silently loading a fresh profile. Old version-1/2 autosaves remain supported.
- Ring mesh generation avoids per-segment temporary arrays. The Village journal shortcut no longer sits over its introduction. Obsolete three-second labels are removed.

Useful assets:

- `Assets/Lemko/Prefabs/UI/GameMenu.prefab`
- `Assets/Lemko/Data/GamePresentation.asset`
- `Assets/Lemko/Runtime/ManualProgressStore.cs`
- `Assets/Lemko/Runtime/GameMenu.cs`
- `Assets/Lemko/Runtime/TooltipService.cs`

## Quick acceptance

1. Open Menu in Village and an activity, then Resume. Confirm the current activity remains available and menu input does not select objects behind it.
2. Save Slot 1, finish another activity, then load Slot 1. Confirm the earlier progress returns. Use recovery to restore the state before loading. Cancel an overwrite/load once too.
3. Restart and check the slot remains. Do not clear PlayerPrefs or delete your save to test this.
4. Hover while moving: no hint should finish loading. Stop: wait 0.5 seconds, observe a one-second fill, then check the hint count rises once. Repeat with Alphabet and an explicit hold control.
5. Toggle reduced motion; check answer feedback remains readable without pulsing. Inspect the menu in your normal window and a smaller landscape window.

## Exact validation

Unity 6000.3.13f1 compiled and saved all nine scenes, reported `LEMKO_GAME_QUALITY_WIRING_PASS`, and exited with code 0. The setup checks menu/session references and missing scripts. A separate small save-data check reported `LEMKO_MANUAL_SAVE_DATA_PASS` and exited with code 0: isolated checkpoint and recovery round trips, preservation of results/preferences, legacy migration, and corrupt/unsupported-data rejection. Its temporary smoke keys were restored/removed in `finally`; real user save keys were not used. Runtime and Editor staged compilation had also passed without warnings before deployment.

No player build, Play-mode simulation, advanced suite or in-game screenshot capture ran. Menu interaction, physical pointer/touch behaviour and visual acceptance still require your short play-through. Initial Unity licensing warnings and a shutdown network callback warning did not prevent successful completion.

Logs and pre-edit backups: external `QualityPass/compile-setup.log`, `save-data-check.log`, `CompilerCheck/`, and `Backup/` under the chat's visualization directory.

## What comes next

This is a first production-quality refactor, not a declaration that the game is release-ready. Next passes should focus on (1) a consistent art/UI direction and final recordings, (2) audience-specific onboarding and better activity pacing/rewards, (3) keyboard/controller and mobile accessibility, and (4) content/device acceptance. Keep the existing eight activities. Avoid replacing working systems wholesale; move remaining hardcoded presentation into prefabs/settings as each screen is refined.
