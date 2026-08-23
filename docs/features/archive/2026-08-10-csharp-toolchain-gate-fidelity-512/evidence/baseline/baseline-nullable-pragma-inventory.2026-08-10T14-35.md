# Baseline — per-file `#nullable enable` opt-in inventory (supports #522 and AC4 design)

Timestamp: 2026-08-10T14-35
Branch: bug/csharp-toolchain-gate-fidelity-512 (from origin/epic/build-ci-coverage-gate-fidelity-integration @ edf3d34c)

Command: `pwsh -NoProfile -File <scratchpad>/find-nullable-files.ps1`
(Enumerates every `*.cs` file outside `obj/`, `bin/`, `packages/` and `.dotnet-sdk/` whose first 40
lines contain a `#nullable enable` directive, and reports line counts.)
EXIT_CODE: 0

## Output Summary

**458** `.cs` files carry a top-of-file `#nullable enable` pragma.

| Top-level project directory | Files with `#nullable enable` |
|---|---|
| `UtilitiesCS` | 390 |
| `QuickFiler` | 22 |
| `UtilitiesCS.Test` | 21 |
| `SVGControl` | 17 |
| `TaskMaster.Test` | 4 |
| `SVGControl.Test` | 3 |
| `TaskMaster` | 1 |
| **Total** | **458** |

## Why this matters for #522

`UtilitiesCS` contains 390 files that have opted in to nullable analysis, and the project passes CI's
actual command (run M3 in `baseline-nullable-gate-vacuity.2026-08-10T14-25.md`) with zero errors. The
195 errors that appear under `/p:Nullable=enable` therefore originate in the *other* `UtilitiesCS`
files — those that have never opted in and were never written for nullable analysis.

This is direct confirmation of the rationale `.github/workflows/ci.yml` states in-line: the repository
enforces nullability through a per-file opt-in convention, and the opted-in population is already
clean. Solution-wide `/p:Nullable=enable` does not tighten enforcement on the opted-in files; it
conscripts the un-opted files, which is why the documented gate can never pass. Removing
`/p:Nullable=enable` from the documented command loses no enforcement over any file that has opted in.

## Candidate files for the AC4 negative-path proof

The proof requires a production file that already carries `#nullable enable`, so that introducing a
violation produces a real diagnostic under the corrected command. Smallest candidates measured:

| Lines | Path | Note |
|---|---|---|
| 10 | `UtilitiesCS\EmailIntelligence\IntelligenceFilters.cs` | `UtilitiesCS` compiles first, so failure is fast |
| 13 | `UtilitiesCS\OutlookObjects\Item\ItemComparer.cs` | concrete class |
| 19 | `UtilitiesCS\Extensions\QueueExtensions.cs` | concrete static extension class |
| 27 | `SVGControl\ISvgResource.cs` | **see correction below** — the file also declares a concrete class and IS suitable |
| 43 | `SVGControl\SvgResourceConverter.cs` | concrete class in a small, fast project |
| 49 | `SVGControl\ToggleSwitch.cs` | concrete class |

## Correction issued 2026-08-10T16-00 (two errors in the table above)

Both errors were found during spec authoring and are corrected here rather than silently amended.

**Error 1 — `SVGControl\ISvgResource.cs` is not interface-only.** The table entry above described it as
"interface only; unsuitable for a CS8603 return-value violation". That was inferred from the filename
and is wrong. Reading the file shows it declares the interface `ISvgResource` at lines 12-16 **and a
concrete `public class SvgResource : ISvgResource` at lines 18-30**, with settable `string? Name` and
`byte[]? Data` properties. It is a viable perturbation site. The research document's selection of this
file was correct and this artifact's dismissal of it was not.

**Error 2 — the line counts in this artifact exclude blank lines.** The generating script used
`Get-Content <file> | Measure-Object -Line`, which does not count empty lines. `ISvgResource.cs` is
reported as 27 above but is **31 lines** on disk (4 blank). Every figure in the two tables above is
therefore a non-blank line count, not a file length. The relative ordering is unaffected, so the
"smallest candidates" ranking remains usable, but no individual figure should be quoted as a file
length.

Neither error affects any conclusion drawn from this artifact about issue #522: the per-project
pragma counts are file counts, not line counts, and are unaffected.

## Selection guidance (revised)

Both `UtilitiesCS\Extensions\QueueExtensions.cs` and `SVGControl\ISvgResource.cs` are viable. Prefer
`QueueExtensions.cs` as primary because `UtilitiesCS` is the foundational dependency and compiles
first, so the perturbed build fails in seconds rather than after a longer dependency chain; this was
confirmed by the executed dry run recorded in
`../regression-testing/negative-path-proof-dry-run.2026-08-10T15-20.md` (EXIT 1 in 3.6 s).
`SVGControl\ISvgResource.cs` is a viable fallback.

Original guidance, retained: prefer a concrete class over an interface, because the intended
violation (returning `null` from a non-nullable reference return type, yielding CS8603) requires a
method body. `UtilitiesCS` is the foundational dependency and is compiled early, so a perturbation
there fails within a few seconds; a perturbation in a leaf project costs a longer build before the
diagnostic appears.

**Non-vacuity requirement.** Whatever file is chosen, the proof is only meaningful if the corrected
command genuinely recompiles that file's project. Under a `/t:Rebuild`-based corrected command every
project is recompiled, so this is satisfied by construction. The proof must additionally record the
paired positive control: the same command returning EXIT 0 on the unperturbed tree, and confirmation
that the perturbation was reverted.
