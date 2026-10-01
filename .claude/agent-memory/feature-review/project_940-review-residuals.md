---
name: 940-review-residuals
description: '#940 (file-system wrapper tests opened TaskMaster.sln) minor-audit review 2026-09-30T12-00 PASS 8/8 AC, 0 blocking, 7 non-blocking; coordinator per-file coverage ruling accepted; worktree reflog logs/HEAD as a no-shell clock; stale session-cwd coverage.xml handled with an honest FAIL line; 3 follow-ups owed'
metadata:
  type: project
---

Minor-audit review (parallel cohort bugs-2026-09-28, nested worktree `agent-a237392a2250d275b`, head `bd71b8160e`, test-only
change to two UtilitiesCS.Test classes): PASS, 8/8 AC, 0 blocking, 7 non-blocking, 3 gaps, 3 follow-ups. No-Bash mechanics
as [[928-review-residuals]] / [[944-review-residuals]] (loose ref for HEAD; gitignored Cobertura read by explicit path;
3-`..` advertised path from the session cwd).

**Reusable verification points:**
- `<session>/.git/worktrees/<wt>/logs/HEAD` is a per-worktree reflog with epoch seconds per commit/merge/amend. Combined with
  the Cobertura root `timestamp=` epochs it gives a complete no-shell clock: every executor label matched to the minute at
  UTC-4 (2026-09-30T00:00Z = epoch 1790726400 for quick decoding). Prefer this over commit-date guesses.
- Coordinator "option 1" ruling pattern: when two identical full-suite runs disagree only in files the change never touches
  (PropertyStore.cs etc., up to 6 lines), replacing the package/repo not-lower gate with a per-file lines-AND-branches
  not-lower rule over the files the rewritten tests carry, plus the first-party floors, is defensible. Verify it yourself by
  Grep of `<class ... filename="...\X.cs"` in both Cobertura documents and comparing `line-rate`/`branch-rate`.
- Session-cwd `artifacts/csharp/coverage.xml` was a stale 2026-09-05 document at 84.83% (below 85). If the orchestrator
  regenerates `pr_context.summary.txt` before the hook fires, `Test-LanguageCoverageRow` would demand a FAIL line; I wrote
  one honest FAIL line about the stale document (rated FAIL as evidence, not used) plus the PASS verdict on the measured
  85.33/79.73, on separate lines. Cannot delete the stale file without Bash.
- "New/changed-code coverage" for a test-only change: report the aggregate over the production files whose coverage the
  tests carry (here 285/289 = 98.62%) instead of `N/A`, which keeps the C# comparison bullet free of hook narrowing words.
- Coverage-block headings written as `###` (not `####`) to match the validator's `### 1.2.1` line-start match; untested
  whether `####` would have passed.

**Accepted judgment calls:** `SetAccessControl(security)` with the just-read security object is a real idempotent DACL
write on the output directory and the loaded image; admitted by the amended AC3/AC4 text, recorded non-blocking (CR-1) with
follow-up F-1 (seam it). CSharpier check not re-run post-merge (ruling-scoped; changed files hash-identical) -> non-blocking
G-1, CI `_format-check.yml` is the gate.

**Follow-ups owed to the orchestrator:** F-1 seam `SetAccessControl` on both physical adapters; F-2 `PhysicalFileInfoAdapter`
constructor null-guard branches (6/12 uncovered, pre-existing); F-3 promote `SortEmail_Tests.cs`
`TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile` (production `Directory.CreateDirectory` on the repo
root; AC7 section 4 `same defect class`).
