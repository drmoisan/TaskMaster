# P4-T1 — Nullable Opt-In Re-Grep and Candidate Selection

Timestamp: 2026-07-20T04-10 (corrected 2026-07-20T04-25)

## Correction note

An initial pass of this task used bash/GNU `grep -rl "^#nullable"`, which produced an inaccurate
count (156) and an incorrect non-opted-in candidate (`UtilitiesCS/Dialogs/ActionButton.cs`).
Investigation found that most `UtilitiesCS/**/*.cs` files carry a UTF-8 BOM before their first
line; GNU `grep`'s `^` anchor does not match immediately after a BOM, silently under-counting
opted-in files and misclassifying BOM-prefixed opted-in files (including `ActionButton.cs`,
confirmed opted-in at line 11: `#nullable enable`) as non-opted-in. Ripgrep (the `Grep` tool)
correctly skips the BOM and was used to produce the corrected results below. This correction was
caught before P4-T5 (defect introduction in the non-opted-in candidate) proceeded.

## Command (corrected, ripgrep-based)

`Grep pattern="^#nullable" path="UtilitiesCS" glob="**/*.cs"` and the same for `SVGControl`.

## Result

385 files under `UtilitiesCS/**/*.cs` and 15 files under `SVGControl/**/*.cs` currently carry a
`#nullable` pragma (400 total). This reflects the full fan-in from all 12 epic children plus this
capstone's own Phase 1/Phase 2 remediation batches (which did not add any new `#nullable` pragma
to a previously-non-opted-in file — remediation was annotation/pragma-suppression-only within
already-opted-in files, plus the SVGControl CS0649 pragma bracket, which does not add `#nullable`
either). The 385/15 figures are substantially higher than the plan's illustrative ~62-file Phase 2
estimate because most of `UtilitiesCS` was already opted in by the 12 sibling children before
this capstone began; Phase 2 only remediated the diagnostics in the subset of already-opted-in
files under its two declared scope trees.

## Selected candidates

- **Opted-in candidate**: `UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs` — confirmed
  line 1 is `#nullable enable` (ripgrep-confirmed). Still representative; unaffected by this
  feature's remediation.
- **Non-opted-in candidate**: `UtilitiesCS/EmailIntelligence/Bayesian/Obsolete/BayesianClassifier.cs`
  — ripgrep-confirmed zero `#nullable` matches anywhere in the file. Substituted for the
  originally-illustrative `ActionButton.cs`, which is confirmed (ripgrep) to be opted-in
  (`#nullable enable` at line 11) and therefore not a valid non-opted-in candidate. The entire
  `UtilitiesCS/Interfaces/**` tree (0 matches across all files) is also confirmed non-opted-in and
  would be an alternative candidate, but its files are interfaces without method bodies, unsuitable
  for hosting the P4-T5 defect statement; a concrete class with a constructor body
  (`BayesianClassifier`) was chosen instead.
