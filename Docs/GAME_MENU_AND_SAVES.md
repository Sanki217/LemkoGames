# Session settings

Click **Menu** or press **Escape** in the village or an activity. Use the menu to toggle audio, hint language and reduced motion. Choose **Resume** or press **Escape** to continue playing.

A new launch or a new Editor Play session starts with default settings and an empty journal. During play, the village and activities share progress, preferences, challenge lives and learning history in memory. Returning to the village keeps them available for the rest of that session.

## Hints

Keep the mouse still over a hintable word or object. After **0.5 seconds** the circle appears and fills over **1 second**, then the hint is shown. Mouse hover does not require pressing a button. Explicit hint controls and touch use press-and-hold/release.

Movement beyond a small four-pixel tolerance restarts the delay and hides a revealed tooltip. Leaving, dragging, disabling or rebinding the target cancels. Only the reveal counts as a used hint. Alphabet still reveals Lemko spelling; ordinary targets reveal the selected translation. Face-down Memory cards remain private.

On each scene's TooltipService, `Still Seconds` controls the first wait, `Hold Seconds` controls the ring fill, and `Movement Tolerance` controls pointer jitter tolerance.
