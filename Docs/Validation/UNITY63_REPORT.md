# Unity 6.3 migration validation — 2026-10-17

Project: `C:\Work\Learning Game\LemkoGames`. Editor: 6000.3.13f1. URP 17.3.0; Input System 1.19.0; uGUI 2.0.0. No packages installed or upgraded by this work.

## Fix and implementation

The copied StarterSetup called `UnityEditor.AssetPackage.Package.Import`, which is unavailable in this Editor. Replaced it with `AssetDatabase.ImportPackage`. Unity 6.3 compilation and the Windows build now succeed.

Added `Assets/Lemko/Scenes/LemkoVillage2D.unity`: orthographic camera, Physics2DRaycaster, regular scene product slots, SpriteRenderer products, BoxCollider2D hit targets, reusable WorldWord prefab and scene text. The existing WordView supports both its UI and sprite presentation. Answer checking remains in ShopController/ShopRound. Shop's Canvas background no longer blocks world clicks. Lists, tooltips, lives and menus remain on Canvas.

The original `Assets/Scenes/Game 1.unity` and its prototype scripts/assets were preserved. The previous generated UI scene was moved to `Assets/Lemko/Scenes/LemkoVillageUI.unity`. Managed data moved into `Assets/Lemko/Data`; prefabs into `Prefabs/Words`; the font into `Fonts`. Unity AssetDatabase moves preserve metadata GUIDs. Existing unrelated folders remain in place.

The supplied carrot PNG was copied unchanged into `Art/Sprites/Products/demo_carrot.png`, configured as a Sprite, and assigned to the existing demo_carrot vocabulary asset. Its white background is part of the source. Language remains explicitly unverified. Missing recordings are expected.

## Verified results

- `unity63-setup-build.log`: `LEMKO_2D_VALIDATION_OK`, `LEMKO_2D_SETUP_OK`, `LEMKO_2D_BUILD_OK`; successful batch exit. This is an actual Windows development build in this project, not the earlier Unity 6.6 build.
- `unity63-checks.log`: `LEMKO_CHECKS_OK: 17` and `LEMKO_2D_RERUN_PRESERVED`; successful batch exit. Existing deterministic round, CSV and save checks pass. Reimport preserved asset identity, and setup reruns preserved scene bytes.
- Scene checks confirm products are outside Canvas, sufficient slots, sprite/collider components, valid Shop content and no missing MonoBehaviour references in the 2D scene.
- `world-player-1280.log`: `LEMKO_WORLD_SMOKE_PASS 1280x720`, process exit 0.
- `world-player-800.log`: `LEMKO_WORLD_SMOKE_PASS 800x600`, process exit 0.
- Both standalone runs verified: one instance per product; actual EventSystem raycasts reach each product without UI interception; duplicate incorrect selection costs only one life; completion; results hide scene objects; duplicate completion does not add results; re-entry has no leftover products; zero-life recovery; unrestricted practice; fresh attempt; village hides Shop objects.
- Standalone smoke uses `-lemkoSmoke -lemkoWorldSmoke` and the separate smoke save key. Normal users do not execute it.

## Limits and remaining checks

These runs use hidden windows and programmatic pointer callbacks. They do not establish complete visible-window visual acceptance, physical mouse/touch behavior, real pronunciation playback, or WebGL/mobile compatibility. Check those locally using the tutorial. No final language content was invented. All seven other activities remain future milestones.

The imported font covers the earlier supplied family source, but final authored language still requires a new coverage and visual check. Development placeholders/unverified entries produce expected content warnings. Unity may reserialize its settings, pipeline assets and TMP fallback during import/build; those changes were not used to introduce new packages or alter the project-wide scene list.

## Reproduce

1. Open LemkoVillage2D in Unity 6000.3.13f1.
2. Run `Lemko > Validate 2D Shop scene`.
3. Run `Lemko > Build Windows 2D Shop`.
4. Launch `Builds/Windows2D/LemkoVillage2D.exe` normally for manual acceptance.
5. Automated run: add `-lemkoSmoke -lemkoWorldSmoke -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile world-test.log`; repeat at 800x600. A successful run logs LEMKO_WORLD_SMOKE_PASS and exits 0.

The original `Run milestone data checks` menu reimports starter data: do not run it after replacing starter vocabulary without backing up your content. Normal scene/content validation does not reimport the starter tables.

## Final preservation and font checks

Content/font validation in Unity 6.3: 0 errors, 8 expected development warnings; see unity63-content.log. Five moved scene/prefab/font/database/configuration GUIDs match their original version-control values. Git diff confirms the original Game 1 scene, Shop prototype folder and Common scripts are unchanged.

