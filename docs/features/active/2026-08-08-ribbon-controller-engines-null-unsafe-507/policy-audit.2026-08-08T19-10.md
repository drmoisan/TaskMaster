# Policy Audit — ribbon-controller-engines-null-unsafe (#507) — Remediation Cycle 1 Exit

Timestamp: 2026-08-08T19-10
Work Mode: `minor-audit`
Scope: full branch diff, `git diff 003c5715055d7d1933db68a742531332756e30b2...HEAD` (branch
`bug/ribbon-controller-engines-null-unsafe-507` vs merge base
`003c5715055d7d1933db68a742531332756e30b2`, head `4fea8d6d`). Two commits under review since the
cycle-1 audit: `e589fad7` (fix, already reviewed in `policy-audit.2026-08-08T17-45.md`) and
`4fea8d6d` (remediation: split `RibbonControllerTests.cs`, new this cycle).

## Executive Summary

This is the cycle-1 remediation exit re-audit. Cycle 1 (`policy-audit.2026-08-08T17-45.md`,
`code-review.2026-08-08T17-45.md`, `feature-audit.2026-08-08T17-45.md`) raised 2 Blocking findings:

- **B1 (file-size cap)**: `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` was 513 lines, 13 over
  the repository's 500-line cap. **Verified remediated this cycle**: commit `4fea8d6d` applies the
  repository's `partial class` convention, moving the two #507 regression tests verbatim into a new
  `TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` (73 lines), registered in
  `TaskMaster.Test.csproj`. `RibbonControllerTests.cs` is now 452 lines (matches the pre-#507
  baseline exactly); `RibbonControllerTests.Engines.cs` is 73 lines. Both are under 500. See § 3.
- **B2 (unguarded call sites)**: `Engines` returning `null` relocates rather than eliminates the
  reachable `NullReferenceException` at all 11 real call sites in `TaskMaster/Ribbon/RibbonViewer.cs`
  (out of scope for #507, untouched by this branch). **Disposition this cycle: promoted to tracked
  issue #518** (`docs/features/potential/promoted/2026-08-08-ribbon-engines-callers-unguarded-null-deref.md`)
  and accepted as non-blocking for this PR. This auditor independently concurs with that disposition
  (reasoning in § 5); it is not treated as an open blocker.

**Total Blocking count this cycle: 0.**

## Rejected Scope Narrowing

None detected requiring rejection. The re-audit prompt asked this review to "treat B2 as
dispositioned-and-tracked, not as an open blocker" unless this auditor independently disagrees. This
is not a scope-narrowing instruction under the Scope Invariant: it does not ask any file, language,
or toolchain check to be skipped, and it does not assert that a language with changed files is "not
applicable." It is a disposition claim about one specific finding, which this audit evaluated on its
own merits (§ 5) rather than accepting on say-so. The audit scope used throughout this document
remains the full `git diff 003c5715055d7d1933db68a742531332756e30b2...HEAD`, independently
re-derived via `git diff --numstat`/`--stat`, not any narrower plan- or task-scoped subset.

No other caller text in the re-audit prompt met the narrowing criteria (coverage-artifact-path
guidance, toolchain-rerun waiver, and standing-context reminders all point at existing evidence
rather than instructing a skipped check).

## Evidence Location Compliance

`validate_evidence_locations.py` remains absent from this repository's tree (`find . -iname
"validate_evidence_locations.py"` returns nothing), consistent with the cycle-1 finding. Manual scan
of the full branch diff for `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or
`artifacts/coverage/` paths:

```
git diff 003c5715055d7d1933db68a742531332756e30b2...HEAD --name-only | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"
```

returns **zero** matches (exit code 1 / no match). All evidence, including the two new agent-memory
files and the new promoted-issue doc, sits under the canonical
`docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/{baseline,
regression-testing,qa-gates,remediation-baseline,other}/` tree or the canonical
`docs/features/potential/promoted/` promotion path. No violation.

`artifacts/pr_context.summary.txt` was found stale at commit `e589fad7` (one commit behind HEAD
`4fea8d6d`; missing the split file, the csproj entry, and the two new agent-memory files) and was
regenerated in place at the start of this cycle to reflect the current head, per the "regenerate if
stale" instruction. No `artifacts/csharp/coverage.xml` was created (per the reviewed feature's
explicit instruction; also confirmed genuinely absent by directory listing), consistent with cycle 1.

## 1. Coverage Verification

### 1.1 Changed languages

`git diff --numstat` (re-derived independently this cycle) shows the following `.cs` files touched:
`TaskMaster/Ribbon/RibbonController.Intelligence.cs` (production, unchanged since `e589fad7`),
`TaskMaster.Test/Ribbon/RibbonControllerTests.cs` (test, modified again this cycle to remove the
moved tests), and `TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` (new test file this
cycle). No `.ts`/`.tsx`/`.py`/`.ps1`/`.psm1` files are in the diff. **CSharp** remains the only
language requiring a coverage verdict.

### 1.2 CSharp coverage row

C# coverage verdict: **FAIL** — repo-wide raw `dotnet-coverage`/Cobertura coverage remains below the
85%/80% floors on the same pre-existing, non-blocking basis established in cycle 1; no coverage
regeneration was performed or required this cycle.

- **Artifact used**: `artifacts/csharp/coverage.xml` (canonical hook path) remains intentionally
  absent (unchanged from cycle 1, per the reviewed feature's explicit instruction and to avoid a
  false floor-check against a partial artifact). Verification continues to rely on the feature's own
  committed Cobertura evidence from cycle 1:
  `evidence/baseline/phase0-baseline-coverage.cobertura.xml` (baseline) and
  `evidence/qa-gates/phase2-final-coverage.cobertura.xml` (post-fix, pre-split).
- **Baseline**: repo-wide raw `line-rate` = **74.43%** (unchanged from cycle 1;
  `evidence/baseline/phase0-baseline-vstest-coverage.md`).
- **Post-change**: repo-wide raw `line-rate` = **61.66%** (unchanged from cycle 1;
  `evidence/qa-gates/phase2-final-vstest-coverage.md`). This cycle's remediation commit (`4fea8d6d`)
  is a test-only move (verbatim relocation of two already-existing `[TestMethod]`s between two
  `partial class` files); it adds no new production code and touches no coverage-instrumented class,
  so it has no independent effect on this figure. No new coverage run was required to re-verify this
  cycle's change: the moved tests exercise the identical `RibbonController.Engines` property already
  covered by cycle 1's evidence, and `RibbonController` remains `[ExcludeFromCodeCoverage]`.
- **Change**: unchanged from cycle 1 — a denominator artifact (`lines-valid` grew 213,002 ->
  259,906 while `lines-covered` increased 158,543 -> 160,251), investigated and dispositioned in
  `evidence/qa-gates/phase2-coverage-comparison.md`, not a genuine loss attributable to this feature.
- **New/changed-code coverage**: no new production files were added this cycle; the new test file
  (`RibbonControllerTests.Engines.cs`) is correctly excluded from the coverage denominator per policy
  (coverage tooling excludes test files).
- **Disposition**: this repo-wide raw figure is a pre-existing, non-blocking condition, unchanged by
  this cycle's remediation, carried forward unmodified from cycle 1's disposition
  (`policy-audit.2026-08-08T17-45.md` § 1.2): raw `dotnet-coverage` merges undercount due to
  vendor/third-party assembly inclusion, and a first-party-only figure has previously been shown to
  clear the floor elsewhere in this repository's coverage history. Not a new remediation trigger for
  this minor-audit bugfix.

### 1.3 Other languages

TypeScript, Python, PowerShell: zero changed files in the branch diff, confirmed via `git diff
--numstat`. No coverage row required for these languages.

## 2. Toolchain Verification (C#)

All four stages were re-run by the orchestrator after the remediation commit landed, in a single
clean pass:

| Stage | Command | EXIT_CODE | Result |
|---|---|---|---|
| Format | `csharpier check .` | 0 | 1489 files, 0 reformatted |
| Analyzers | `msbuild TaskMaster.sln /t:Build /m ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | 0 errors |
| Nullable | `msbuild TaskMaster.sln /t:Rebuild /m ... /p:TreatWarningsAsErrors=true` | 0 | 0 errors |
| Test | `vstest.console.exe <9 assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"` | 0 | 6295 total, 6295 passed, 0 failed |

Both #507 regression tests were confirmed passing by name in the same run:
`Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing` (3 ms),
`Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines` (1 ms). This is reported by the orchestrator as
a verified, independently-run result; this audit did not re-execute the toolchain (not required per
task instructions) but did independently re-verify the file-size and scope-boundary claims that
depend on it (§ 3).

## 3. General Code Change Policy

- **Simplicity/minimal diff (production)**: PASS, unchanged from cycle 1. The sole production line
  (`Globals?.Engines`) is unmodified since `e589fad7`.
- **Scope boundary**: PASS, re-verified independently. `git diff --name-only
  003c5715055d7d1933db68a742531332756e30b2...HEAD` lists exactly: `.claude/agent-memory/**` (2
  files, atomic-executor + feature-review), `TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs`
  (new), `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` (modified),
  `TaskMaster.Test/TaskMaster.Test.csproj` (modified), `TaskMaster/Ribbon/RibbonController.Intelligence.cs`
  (modified), plus the feature's own `docs/features/active/.../` evidence/audit files and one new
  `docs/features/potential/promoted/` doc. `TaskMaster/Ribbon/RibbonViewer.cs` is confirmed **absent**
  from the diff (`git diff --name-only ... | grep -i RibbonViewer` returns no match).
- **File size limit (`CLAUDE.md` § 4.1 / `.claude/rules/general-code-change.md` § File Size Limit)**:
  **PASS — remediated.** `wc -l TaskMaster.Test/Ribbon/RibbonControllerTests.cs` = 452 (was 513 at
  cycle-1 head, `e589fad7`). `wc -l TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` = 73.
  Both `<= 500`. Verified independently in this audit, not merely taken from
  `evidence/remediation-baseline/post-split-line-counts.2026-08-08T17-45.md` (which agrees).
- **Behavior-preserving split**: PASS. `git diff e589fad7 4fea8d6d -- TaskMaster.Test/Ribbon/RibbonControllerTests.cs`
  shows the two `[TestMethod]`s removed from `RibbonControllerTests.cs` are byte-for-byte identical
  (including doc comments, blank lines, and indentation) to the two `[TestMethod]`s added in
  `RibbonControllerTests.Engines.cs` — a pure cut-and-paste move, confirmed by direct diff comparison
  in this audit, not merely the executor's claim. No test logic, assertion, or comment was altered
  during the move.
- **`partial class` convention correctness**: PASS. `RibbonControllerTests.cs` retains the sole
  `[DoNotParallelize]`/`[TestClass]` attribute pair and gained `partial` on the class declaration
  (`public partial class RibbonControllerTests`); `RibbonControllerTests.Engines.cs` declares only
  `public partial class RibbonControllerTests` with no duplicated class-level attributes — attributes
  are correctly placed on exactly one part, matching MSTest's requirement that `[TestClass]` be
  declared once per (partial) class. The moved test that calls the private
  `static RibbonController CreateController()` helper (declared in `RibbonControllerTests.cs`)
  compiles and executes correctly from the sibling partial file because private members are visible
  across all parts of the same partial class within the same assembly — confirmed by the orchestrator's
  green build/test result (§ 2), not merely asserted.
- **csproj registration**: PASS. `TaskMaster.Test.csproj` (legacy non-SDK style) gained exactly one
  `<Compile Include="Ribbon\RibbonControllerTests.Engines.cs" />` line immediately after the existing
  `RibbonControllerTests.cs` entry; no other `<Compile>` entries were altered or removed.
- **No test lost from discovery**: the orchestrator's post-remediation filtered run reports
  6295/6295 total/passed with the two #507 tests confirmed passing by name (§ 2). This audit notes,
  and reconciles, a test-count discrepancy across the various evidence artifacts in § 6 below; the
  reconciliation confirms no test was silently dropped by the split.
- **Naming**: PASS, unchanged from cycle 1.

## 4. General + C# Unit Test Policy

- **Framework/mocking/assertions**: PASS, unchanged. Both tests still use `[TestMethod]` (MSTest),
  `Moq`, and FluentAssertions; the move did not alter the test bodies.
- **Arrange-Act-Assert / independence / determinism / no external dependencies**: PASS, unchanged
  from cycle 1 — the tests were moved, not rewritten.
- **Test file location**: PASS. `TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` mirrors
  the production `TaskMaster/Ribbon/` path convention already used by its sibling
  `RibbonControllerTests.cs`; it is not colocated with production source.
- **Coverage exemption compliance**: PASS, unchanged. Neither file touches
  `[ExcludeFromCodeCoverage]` on `RibbonController`.

## 5. B2 Disposition Review — `RibbonViewer.cs` Unguarded Call Sites (Promoted to #518)

This auditor independently evaluated the promoted-issue disposition rather than accepting it as
given, per the re-audit prompt's own instruction to state a disagreement explicitly if one exists.
Findings supporting non-blocking disposition for **this** PR:

1. **Pre-review scope boundary, not post-hoc narrowing.** `issue.md`'s "Dependencies / Risks"
   section explicitly named `RibbonViewer.cs` as out of scope and forbade modifying it, *before*
   cycle-1's review began. The disposition does not narrow an audit-time scope; it enforces a
   scope the issue itself set at authoring time.
2. **Concurrency conflict is real and independently verifiable.** `issue.md` states an unmerged
   sibling branch, `bug/ribbon-engine-readiness-guard-503`, is concurrently relocating the exact
   `#region Spam Manager` / `#region Triage` blocks containing all 11 call sites into a partial
   class. Modifying those 11 call sites in this PR would create a direct merge conflict with that
   branch's restructuring.
3. **Not a regression introduced by this change.** The sibling `SB` property (same file,
   `RibbonController.Intelligence.cs:190-202`) already returns `null` via the identical
   `Globals?.` pattern, and its own callers (`TrainSpam_Click`, `TrainHam_Click` in
   `RibbonViewer.cs`) are equally unguarded on `main`, independent of this branch. The unguarded-
   caller pattern is a pre-existing codebase convention that #507 does not worsen; #507 makes
   `Engines` consistent with that existing (imperfect) convention rather than introducing a new one.
4. **Policy directly supports deferral over scope creep.** `CLAUDE.md`'s Bugfix Workflow states:
   "If you uncover deeper design problems, open a new issue instead of widening scope." Fixing 11
   call sites in a file explicitly excluded from this issue's declared scope, and concurrently owned
   by another in-flight branch, is exactly the "deeper design problem" this clause anticipates.
5. **The promotion itself is verifiably complete.** Issue #518
   (`docs/features/potential/promoted/2026-08-08-ribbon-engines-callers-unguarded-null-deref.md`) was
   independently read for this audit: it names all 11 call sites with line numbers and exact
   expressions, records the `#503` sequencing dependency, and cross-references #505/#506. This is not
   a bare deferral — it is a fully specified, trackable follow-up.

**Conclusion: this auditor concurs with the non-blocking disposition.** B2 is not counted toward
this cycle's Blocking total. It remains factually correct that `Engines` returning `null` relocates
rather than eliminates the reachable `NullReferenceException` for real callers — that finding is not
retracted — but blocking merge of #507 on a defect this PR cannot fix without violating its own
declared scope boundary and colliding with a concurrent branch would not improve the codebase; it
would only delay a correct, narrow, evidenced fix while the actual hazard (unguarded callers) remains
open and tracked at #518.

## 6. Evidence Consistency Note (Informational) — Test Count Reconciliation

Cycle 1 recorded three test counts that appeared to disagree: unfiltered baseline 6294/6294,
unfiltered post-fix (executor) 6296/6296, and filtered post-fix (orchestrator reconciliation,
`/TestCaseFilter:"TestCategory!=LiveOutlook"`) 6295/6295. This cycle's orchestrator-run toolchain
(§ 2) used the same `TestCategory!=LiveOutlook` filter for both sides of the comparison and reports
6293 filtered-baseline vs 6295 filtered-final — a delta of **+2**, exactly matching the two #507
tests, and self-consistent with cycle 1's numbers: 6293 = 6294 (unfiltered baseline) − 1
(`LiveOutlook`-tagged test excluded by the filter); 6295 = 6296 (unfiltered post-fix) − 1 (same
exclusion). All four recorded counts across both cycles are mutually consistent once the filter
difference is accounted for, and every recorded run shows `total == passed`, `failed == 0`. This
resolves cycle 1's § 5 open question about the 6296-vs-6295 discrepancy. **Informational, not
blocking; no test was lost or gained outside the two intentional #507 additions.**

## 7. Summary of Findings by Severity

| Severity | Count | Findings |
|---|---|---|
| Blocking | 0 | — (B1 remediated and independently re-verified this cycle; B2 promoted to #518 and dispositioned non-blocking, concurred with independently) |
| Non-blocking | 0 | — |
| Informational | 3 | CLAUDE.md/ci.yml nullable command divergence (pre-existing, carried from cycle 1, reported separately); test-count reconciliation across cycles (§ 6, resolved); pre-existing sub-floor repo-wide C# coverage (dispositioned non-blocking, carried from cycle 1) |

**Total Blocking count: 0.**
