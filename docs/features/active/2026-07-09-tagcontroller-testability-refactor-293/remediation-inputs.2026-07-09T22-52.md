# Remediation Inputs — Issue #293 (tagcontroller-testability-refactor)

- Feature folder: `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/`
- Base branch: `epic/winforms-testability-refactor-integration` @ merge-base `3f04d50f6544f084323e5d7a9a563facb9d579df`
- Head: `55a4835659f977a0dce9e1f5f872b121b659167d`
- Timestamp: 2026-07-09T22-52
- Source artifacts: `policy-audit.2026-07-09T22-52.md`, `code-review.2026-07-09T22-52.md`, `feature-audit.2026-07-09T22-52.md`

## Remediation-Required Findings

### R1 (Blocking) — Test file exceeds 500-line limit

- File: `Tags.Test/TagControllerSeamTests.cs`
- Measured size: 579 lines (`awk 'END{print NR}'` = 579; `wc -l` = 579)
- Violated rule: `.claude/rules/general-code-change.md` "File Size Limit" — "No production code, test code, or reusable script file may exceed 500 lines." Also CLAUDE.md §4.1 (General Code Change Policy) and §C#5.1. No listed exception applies (this is not a throwaway script, a language-processing text fixture, or a Markdown doc).
- Why it was missed: the executor's `evidence/qa-gates/file-size-compliance.md` measured production `Tags/*.cs` files only and did not include `Tags.Test/*.cs` files.
- Required change: split `TagControllerSeamTests.cs` into two or more cohesive test files each `<= 500` lines. A natural seam is by concern group, e.g. dialog-seam / auto-assign tests in one file and navigation/rendering-seam tests in another. Preserve all existing `[TestMethod]` coverage; do not weaken or drop any assertion.
- Verification after fix:
  1. Re-run `awk 'END{print NR}'` on every `Tags.Test/*.cs` file; confirm all `<= 500`.
  2. Re-run the full C# toolchain in order (csharpier -> analyzers -> nullable -> vstest with coverage) and confirm 64/64 tests still pass and Tags.dll line coverage remains `>= 80%` (92.63% baseline for this branch).
  3. Update `evidence/qa-gates/file-size-compliance.md` to include the test-file measurements.

## Non-Blocking Observations (optional, not required for merge)

- Low: `Tags/LauncherAutoAssign.cs` `AutoFindAsync` (L81-91) — redundant `try { ... } catch (Exception) { throw; }` adds no context and cannot observe faults from the returned task. Suggest returning `Task.Run(...)` directly. Not required.
- Low: `Tags/TagController.Rendering.cs` `RemoveControls` (L94-102) — `_colColorbox.Remove(i)` index/element confusion. Pre-existing, latent (collection empty in current flows), explicitly report-only per spec `## Non-Goals`. Track as a separate follow-up; not in scope for #293.

## Handoff

Route R1 to the atomic planner/executor for a minimal remediation cycle: split the oversized test file and re-run the toolchain. All nine acceptance criteria already pass; no functional change is required. After R1 is resolved and the toolchain is re-verified green, the branch is expected to be READY TO MERGE.
