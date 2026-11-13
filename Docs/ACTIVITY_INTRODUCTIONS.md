# Activity introductions

Each of the eight activity scenes shows a story and tutorial before starting its round. Click or tap anywhere to continue; Enter and Space also work. The dismissal is deferred by one frame so the click cannot select a game item. The intro blocks gameplay and the pause menu while open.

Introductions appear whenever an activity scene is entered, including opening that scene directly. Retry/Play again within the same scene starts immediately. Village has no introduction. Farm explains identification before Numbers completion and quantities afterward. No progress or vocabulary encounters are recorded by merely viewing an introduction.

## Edit in Unity

- Story, title, instructions and optional illustration: Assets/Lemko/Data/Introductions. Select the corresponding activity asset in the Inspector.
- Farm's Before Numbers field holds its introductory-mode instructions.
- Shared appearance: Assets/Lemko/Prefabs/UI/ActivityIntroduction.prefab. Edit this prefab to update all eight scenes.
- The panel is initially inactive in the prefab; runtime opens it. Temporarily activate its child panel to inspect the layout in Prefab Mode, then restore it before saving.
- Setup recovery menu: Lemko > Setup > Add activity introduction screens. Existing definitions and prefab are preserved.

## Quick manual check

Open Assets/Lemko/Scenes/Village.unity and press Play. Enter an activity: its tutorial should cover the screen and the round should not yet have started. Click anywhere: the tutorial disappears and the round begins. Return to Village and enter another activity. Repeat for all eight. Check tapping on your target touch device separately.

## Validation — 2026-10-28

Unity 6000.3.13f1 batch compilation/import and ActivityIntroductionSetup.Apply completed successfully, exit code 0. The setup validated the introduction references, matching activity IDs and missing scripts in all eight activity scenes and saved them. Log: external ActivityIntroductions/setup.log (LEMKO_ACTIVITY_INTRODUCTIONS_PASS).

No player build, gameplay simulation, or device testing was performed for this change. Click/tap behavior and final visual layout still need the short manual check above.
