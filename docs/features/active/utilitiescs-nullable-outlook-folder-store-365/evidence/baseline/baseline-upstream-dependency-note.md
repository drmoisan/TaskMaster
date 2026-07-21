# Baseline Upstream Wave-0 Dependency Status (P0-T8)

Timestamp: 2026-07-19T10-53

Command: for each of the 6 upstream files, `grep -q "#nullable enable" <file>`.

## Findings

| Upstream file | `#nullable enable`? |
| --- | --- |
| `UtilitiesCS/Extensions/StringExtensions.cs` | ENABLED |
| `UtilitiesCS/Extensions/LazyExtension.cs` | ENABLED |
| `UtilitiesCS/Extensions/IEnumerableExtensions.cs` | ENABLED |
| `UtilitiesCS/HelperClasses/Tokenizer.cs` | ENABLED |
| `UtilitiesCS/HelperClasses/Logging/VerboseLogger.cs` | ENABLED |
| `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` | ENABLED |

## Interpretation

All 6 upstream files carry `#nullable enable` in this worktree. This differs from the plan's draft-time
research note (which recorded both `#363` and `#364` as `Status: Draft` and not-yet-landed). This feature
branch was cut from the epic integration branch tip (commit `dffadd5a`, PR #382 merged), on which the
Wave-0 siblings `#363` (`utilitiescs-nullable-extensions`) and `#364` (`utilitiescs-nullable-helperclasses`)
have already landed their pragmas.

Consequence: this feature's annotation decisions at the upstream-consuming call sites (§3.2/§3.3 of the
research) are made against the real (non-oblivious) upstream contract shape, not an oblivious one. The
Epic Dependency Note caveat that baseline CS86xx figures might not reflect the final upstream contract
shape does not apply here, because the upstream contracts are present. P12-T11 re-verifies this at final QC.
