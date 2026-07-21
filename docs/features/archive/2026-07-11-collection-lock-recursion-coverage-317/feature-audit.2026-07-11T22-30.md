# Feature Audit — collection-lock-recursion-coverage-317 (Issue #317)

- Timestamp: 2026-07-11T22-30
- Branch: `test/collection-lock-recursion-coverage-317` vs base `main` (merge-base `5ecbc4c6`, verified via `git merge-base HEAD main`)
- Work mode: `full-bug` (per `plan.2026-07-11T19-27.md`'s persisted Work Mode marker)

## Scope and Baseline

- AC source (per `full-bug` work mode): `spec.md` only — this feature has no `user-story.md`, consistent with the bug work mode's AC-source rule.
- Baseline: `main` at `5ecbc4c61bd87ac09b75d52a8913d7e53b410343`, which this branch was cut from directly (no divergence since — the merge-base equals `main`'s current tip).
- Diff under audit: `git diff --name-only main HEAD` — 2 code files (`UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs`, new; `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, +1 line) plus 22 documentation/evidence files under `docs/features/active/2026-07-11-collection-lock-recursion-coverage-317/`. No production `.cs` file is in the diff.

## Acceptance Criteria Inventory

Source: `spec.md`, `## Acceptance Criteria` section, 5 items (AC-1 through AC-5), all currently checked `- [x]` in the source file with inline evidence references.

1. AC-1: `ConcurrentObservableCollectionLockRecursionTests.cs` exists at its original path, containing both named `[TestMethod]`s, both passing.
2. AC-2: The restored file's namespace is `UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection`, matching its two living siblings.
3. AC-3: `UtilitiesCS.Test/UtilitiesCS.Test.csproj` carries the matching `<Compile Include>` entry.
4. AC-4: No production file is modified; a repo-wide diff against `main` shows only the two files above touched.
5. AC-5: Full C# toolchain passes in a single final pass (csharpier → analyzers → nullable/`TreatWarningsAsErrors` → MSTest via vstest), with zero test regressions and no coverage regression on changed lines.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence (independently re-verified by this reviewer unless noted) |
|---|---|---|
| AC-1 | **PASS** | File exists at the exact path (confirmed via `Read`). Contains both named `[TestMethod]`s verbatim (confirmed by direct file read). Targeted run: 2/2 passed (`evidence/regression-testing/restored-tests-pass.2026-07-11T20-07.md`). Full-suite run: 4213/4213 passed, 0 failed, including the 2 new tests (`evidence/qa-gates/post-change-test-coverage.2026-07-11T20-25.md`). |
| AC-2 | **PASS** | `grep -n "^namespace"` (run independently by this reviewer) confirms `UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection` in the restored file, identical to both `ConcurrentObservableCollection_Tests.cs` and `ConcurrentObservableCollectionSerialization_Tests.cs` in the same folder. |
| AC-3 | **PASS** | `git diff main HEAD -- UtilitiesCS.Test/UtilitiesCS.Test.csproj` (run independently) shows exactly one `<Compile Include>` line added, positioned immediately after `ConcurrentObservableCollection_Tests.cs`'s entry, as planned. |
| AC-4 | **PASS** | `git diff --stat main HEAD` (run independently, no path filter) lists 24 files total, of which exactly 2 are code files (the test file and the csproj) and the remaining 22 are markdown files under the feature's own `docs/features/active/2026-07-11-collection-lock-recursion-coverage-317/` folder (plan, spec, research, and evidence artifacts this same delivery produced). No other production or test file, and no file outside the feature folder besides the two named code files, appears in the diff. This satisfies AC-4's literal wording ("no production file is modified... only the two files above touched" — read as "the only *code* files touched," which is the criterion's evident intent given the feature folder's own documentation is expected to grow). |
| AC-5 | **PARTIAL** | CSharpier: PASS (after one documented loop-restart). Analyzer build: PASS (0 errors). MSTest via vstest: PASS (4213/4213, 0 regressions, coverage on changed lines held/improved — see `policy-audit.2026-07-11T22-30.md` §5.1). **Nullable/`TreatWarningsAsErrors` build: FAILS with exit code 1** (34 pre-existing, unrelated `SVGControl.csproj` errors), independently confirmed by this reviewer to be identical in count and diagnostic codes to the pre-restoration baseline capture (`evidence/baseline/baseline-nullable-build.2026-07-11T19-52.md`) and to reference neither of the two files this PR touches. AC-5 as literally worded ("Full C# toolchain passes in a single final pass") is **not fully earned** — one toolchain step does not pass — even though the failure is demonstrably pre-existing and unrelated to this change's scope. This is a wording/evidence mismatch in the spec's AC-5 checkbox (checked `[x]` in `spec.md` even though the linked nullable-build evidence itself records exit code 1), not a defect in the delivered test code. |

## Independent Re-Verification Log (this review)

- `git merge-base HEAD main` → `5ecbc4c61bd87ac09b75d52a8913d7e53b410343`, matching the caller-supplied base and `main`'s current tip.
- `git show 0ec111b29923cfadd63c26908e41e069924d4ea5~1:<path>` → confirmed the recovered pre-deletion content is byte-identical to the restored file except for the `namespace` line, corroborating the spec's Root Cause Analysis and the plan's P1-T1/P1-T2 claims.
- `grep -n "^namespace"` across all three sibling files → confirmed AC-2.
- `git diff --numstat main HEAD` and `git diff --stat main HEAD` → confirmed AC-4's file-scope claim.
- `awk 'END{print NR}'` on both changed code files → 88 lines (new test file) and 901 lines (csproj, pre-existing size, +1 line only) — no 500-line-limit violation.
- Read `evidence/baseline/baseline-nullable-build.2026-07-11T19-52.md` and `evidence/qa-gates/post-change-nullable-build.2026-07-11T20-20.md` in full → confirmed the AC-5 nullable-build shortfall is pre-existing and unchanged by this PR (see AC-5 row above).
- Independently parsed `artifacts/csharp/coverage.xml` (Python, per-package `<line hits>` aggregation) → reproduced the evidence's claimed `UtilitiesCS` package figures (88.3x%) and the raw repo-wide figure (60.69%); full detail in `policy-audit.2026-07-11T22-30.md` §5.1.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-07-11-collection-lock-recursion-coverage-317/spec.md`
- Total AC items: 5
- Checked off (delivered) in source file: 5 (all already `- [x]` prior to this review)
- This review's independent verdicts: 4 PASS (AC-1, AC-2, AC-3, AC-4), 1 PARTIAL (AC-5)
- Remaining (unchecked): 0
- Items remaining: none unchecked in the source file; AC-5 is flagged here as PARTIAL despite being checked, because its own cited evidence (`post-change-nullable-build.2026-07-11T20-20.md`) records a toolchain step failure. This review does not un-check AC-5 in `spec.md` (per the AC check-off protocol, reviewers document gaps rather than reformatting source text they did not author), but records the gap here and in the policy audit for visibility.

## Verdict

**PASS on substance, with one AC (AC-5) evaluated as PARTIAL** due to a pre-existing, unrelated nullable-build failure that AC-5's own wording does not account for. This does not block the change: the failure is demonstrably identical to the pre-restoration baseline, references neither file this PR touches, and the actual deliverable (restored regression test coverage, correctly namespaced, correctly wired, passing, with no coverage regression) is fully realized.
