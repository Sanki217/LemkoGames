# Engagement and visual polish — 2026-10-28

Open Assets/Lemko/Scenes/Village.unity and press Play. No manual setup is needed.

## Player changes

- Village recommends the first unfinished contribution while every activity remains accessible. Completed contributions use the existing angled map and pop in on return (except Alphabet, which retains the no-visible-signs decision).
- Results show a contribution illustration, independent/supported answers, actually repaired words, vocabulary to revisit and a highlighted next activity. After the gathering is complete, another review activity is still suggested.
- Review prioritizes supported/mistaken words. Independent successes are spaced by 1, 3 and 7 days as the independent streak grows. Sessions mix familiar content with one or two new entries when enough familiar content exists; a new profile still gets a full valid starter session.
- Each tutorial has Show next time on/off, remembered per activity during this play session. Help reopens it without resetting the round. Memory waits at a pending reveal while Help or the menu blocks input.
- Hints remain free learning support, counted once per vocabulary ID per session. No new penalty, timer or activity lock was introduced.

## The eight games

Shop: selected products move into the basket; repeated mistakes give a more specific list reminder. Existing missing/extra checks remain.
Family Tree: successful invitations highlight the connection path from YOU to the selected person. Explicit configured answers remain authoritative.
Farm: placed animals settle and occasionally bob; quantity feedback distinguishes the wrong type from the wrong count. Quantities remain unlocked by Numbers.
House: a translucent placement preview appears over a drop zone; corrections distinguish a right item in a wrong room from a missing item. Furniture remains between tasks during a session, and resets on restart/exit. Free placement and cross-room submission remain.
Memory: matching cards turn mint and pulse, with existing pronunciation where supplied. Review-needing words are prioritized within the configured pool; concealed cards remain concealed.
Numbers: issued ticket previews pulse. Only supplied number forms are used.
Alphabet: placed letters pulse into slots and a correct completed word plays its existing recording, if present. Repeated letters and correction behavior are preserved.
Weather: the selected weather appears near the character; successful plans give a small character reaction. Explicit temperature validation remains.

## Shared presentation

The current white/cream, purple and mint interface is retained and extended. Progress fills smoothly; world pictures gently enlarge on hover without changing their hit areas. Primary actions and the next-activity result button use purple. Redundant explicit hint buttons are hidden; requests/pictures still support hover/hold hints. The results layout separates reward art from statistics.

Reduced motion disables the new pulses, travel, hover enlargement and animal idle, and makes progress changes immediate. No new packages, external services, invented vocabulary or generated pronunciation were added. Existing art is reused; final sprites and recordings can still be replaced normally.

## Editing

- Tutorial text and images: Assets/Lemko/Data/Introductions; layout: Prefabs/UI/ActivityIntroduction.prefab.
- Contribution titles/illustrations: the VillageStoryDefinition assigned to GameSession.
- Results reward: Contribution reward child under each Results panel.
- Shared motion: Runtime/PresentationMotion.cs; animal idle: Runtime/AnimalIdle.cs; review ordering: Runtime/PracticeSelection.cs.
- Existing activity definitions and Inspector layout settings remain the source of gameplay content.
- Recovery/setup menu: Lemko > Setup > Apply engagement polish. It reuses existing generated controls; it deliberately reapplies its documented HUD/result layout settings.

## Validation and limits

Unity 6000.3.13f1 compilation and scene wiring passed, with all nine scenes checked for missing scripts. Static Editor authoring previews of Shop (1280x720 and 960x540), menu, tutorial and results were inspected. They do not execute a game session or read player saves. Final validation evidence is recorded in PROJECT_STATUS.md.

No player builds, browser runs, gameplay simulations or advanced tests were performed. The feel/timing of interactions and touch behavior need ordinary user play feedback. The existing final-art gaps (including the supplied carrot background) remain visible. This pass improves the existing prototype toward a finished game; it does not establish release readiness or learning efficacy.

Suggested short check: Shop basket and results; disable/reopen a tutorial through Help; House preview and correction; Memory reveal plus Help; Farm before/after Numbers; letters and ticket feedback; Weather preview. Confirm reduced motion if you use it.

