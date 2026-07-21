# Percentage-Obscuring Defect — Reproduction Analysis (P1-T2)

Timestamp: 2026-07-18T08-53

Confirmed hypothesis: DOSSIER-ASSUMED — Hypothesis 1 (theme color contrast). No runtime capture
was possible (see `fail-before-exception.2026-07-18T08-52.md`); per that dossier the strongest
hypothesis is selected on code evidence: the production source itself carries the maintainer TODO
"Override the draw function because these colors do not work as expected" on the exact lines that
assign the `CboFolders` theme colors (`Theme.Rendering.cs:96-98`), the owner-draw paints with the
selection-state-dependent `e.ForeColor` rather than the themed color pair, and the identical
text-present-but-unreadable failure mode has a documented precedent (issue #269). Hypotheses 2
(scrollbar overlay of the flush-right 46 px column) and 3 (DPI clipping of un-scaled pixel
constants) remain plausible secondary contributors.

Mapping to the planned Phase 4 CSS structure (eliminates all three hypotheses by construction):

The breadcrumb page renders each suggestion as a flexbox row in which the percentage cell is
`flex: 0 0 auto; margin-left: auto; flex-shrink: 0;` with a `ch`-based `min-width`, and the
segment container is `flex: 1 1 auto; min-width: 0; overflow: hidden; text-overflow: ellipsis;
white-space: nowrap;`. This eliminates Hypothesis 1 by construction because all colors come from
CSS custom properties switched atomically by the `themeChange` bridge message (and defaulting via
`prefers-color-scheme`) — there is no owner-draw path where an unthemed `e.ForeColor` can pair
with a themed background, and foreground/background are always assigned from the same theme
variable set. It eliminates Hypothesis 2 because an HTML flex row has no list scrollbar painting
over item content — the scroll container's scrollbar occupies layout space outside the row's
content box, and `flex-shrink: 0` plus `min-width` guarantee the percentage cell can never be
compressed or overlapped by siblings. It eliminates Hypothesis 3 because CSS layout is
DPI-independent (CSS pixels scale with the device scale factor) and the `ch`-based `min-width`
scales with the font, so a "100%" string always fits its cell at any display scaling; long paths
truncate in the middle segments instead of pushing the percentage out of view.

Gate note (G9): this artifact exists before any Phase 4 CSS/fix work begins.
