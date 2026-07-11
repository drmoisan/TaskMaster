# Policy Compliance Audit — swordfish-interface-project-teardown (#308), epic child F5

- **Timestamp:** 2026-07-11T13-36
- **Feature:** swordfish-interface-project-teardown (Issue #308), epic swordfish-removal child F5 (epic-terminal)
- **Work mode:** full-feature (`issue.md` line 11)
- **Base / diff baseline:** epic/swordfish-removal-integration @ `1b65f7a7`
- **Head:** feature/swordfish-interface-project-teardown-308 @ `0ec111b2`
- **Audit scope:** full branch diff `git diff 1b65f7a7..0ec111b2` (90 files: 44 `.cs`, 11 `.csproj`, 2 `.sln`, 26 `.md`, plus 4 `.xaml`/`.resx`/`.config`/`.settings` inside the deleted `UtilitiesSwordfish.Test` folder).
- **Policy sources read fresh from this worktree:** `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`.
- **CI note:** child→integration PRs have no CI configured; this audit is the sole quality gate. Merge proceeds only at blocking_count == 0.

## Executive Summary

F5 is a pure structural teardown that removes the vendored `UtilitiesSwordfish` project and its
test project after F1–F4 eliminated all first-party Swordfish type usage. The branch adds no new
executable production code. All changes are deletions, one dead-method removal
(`QfcExplorerController.UpdateForMove`), and comment-only rewordings. The full C# toolchain passes
in a single final pass (format/analyzers/nullable green; MSTest shows only pre-existing
environmental failures). No policy violation was found. Verdict: **PASS**. Blocking findings: **0**.

## Rejected Scope Narrowing

None. The caller prompt correctly directs a full-branch audit against `1b65f7a7` and, in its
"points requiring scrutiny," asks for a broader (not narrower) evaluation of the sibling-feature
comment rewordings under AC-13. No instruction attempted to narrow scope to a subset of files,
plan, phase, or to mark any changed-file language as out of scope. No narrowing to reject.

## Evidence Location Compliance

- All feature evidence artifacts are written under the canonical
  `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/<kind>/` tree
  (baseline, qa-gates, regression-testing, other). Confirmed by scanning the branch diff.
- Branch-diff scan for files written under `artifacts/baselines/`, `artifacts/qa/`,
  `artifacts/evidence/`, or `artifacts/coverage/`: **zero matches**. No non-canonical evidence path.
- Verdict: **PASS** (no evidence-location violation).

## Toolchain Compliance (C# — CLAUDE.md C#1 / CUT3)

Evidence: `evidence/qa-gates/finalqc-single-pass.md`, `finalqc-build-green.md`, `finalqc-csharpier.md`,
`finalqc-analyzers.md`, `finalqc-nullable.md`, `finalqc-vstest-coverage.md`.

| Stage | Command | Result | Verdict |
|---|---|---|---|
| 1. Format (csharpier) | `dotnet csharpier check .` | 0 files need reformatting (idempotent, no auto-fix) | PASS |
| 2. Analyzers | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Build succeeded, 0 errors, 74 pre-existing warnings (CS8632/CS0067 in test projects, not F5-introduced) | PASS |
| 3. Nullable | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Build succeeded, 0 errors, 0 warnings | PASS |
| 4. Test (vstest via dotnet-coverage) | 8 `*.Test.dll`, `/TestCaseFilter:TestCategory!=LiveOutlook` | 5324 passed / 22 failed / 5346 total; the 22 are pre-existing environmental Deedle `eng`-model failures, byte-identical to baseline | PASS (F5-attributable failures: 0) |

Single-pass requirement (CLAUDE.md §8.1 / CUT3): satisfied — no stage auto-fixed files; steps 1–3
fully green in one pass. Step 4 EXIT 1 is caused solely by 22 environmental failures that are
identical at baseline (`evidence/baseline/baseline-vstest-coverage.md`) and post-change
(`evidence/qa-gates/finalqc-vstest-coverage.md`); test count dropped 5358→5346 (the 12 removed
WI-4 test methods) and passed dropped 5336→5324 by the same 12; failed count is unchanged at 22.
The sorted failing-name set is asserted byte-identical (empty diff). F5 introduces zero new
failures. Independent re-verification of build/grep facts below corroborates the toolchain claims.

## Coverage Verdicts (per language with changed files)

Changed-file language enumeration from the branch diff (authoritative) and
`artifacts/pr_context.summary.txt`: **C# only.** TypeScript, Python, and PowerShell have zero
changed files on this branch.

Coverage policy applied is CLAUDE.md (this worktree): repo-wide line coverage `>= 80%` on the
**testable denominator** (COM/VSTO/WinForms/vendored exemption), and `>= 90%` for changed/new code.
CLAUDE.md is the highest-precedence policy document; where `.claude/rules/general-unit-test.md`
states a uniform 85%/75% floor, the CLAUDE.md figures govern per the policy-compliance order and
per the caller's explicit direction. This is recorded as an assumption; both the 80% testable-floor
reading and the no-regression rule are satisfied regardless.

Coverage evidence: `evidence/baseline/baseline-vstest-coverage.md`,
`evidence/qa-gates/finalqc-vstest-coverage.md`, `evidence/qa-gates/coverage-delta-verification.md`.

| Language | Metric | Measurement and disposition | Verdict |
|---|---|---|---|
| C# (.cs) | Line coverage | Repo-wide raw line coverage rose Baseline 68.93% → Post-change 69.44% (delta +0.51pp); raw figure includes untestable COM/VSTO/WinForms and vendored code and is not the testable-denominator figure; per-package comparison shows no surviving first-party production package regressed (UtilitiesCS 88.32%→88.34%, all others held); changed/new executable production lines = 0 (pure teardown), so the >=90% new-code threshold is met with no lines to cover; no coverage backfill owed (removed Swordfish tests dropped numerator and denominator together). | PASS |
| C# (.cs) | Changed-line regression | No surviving first-party production line lost coverage; the only executable removal (dead `UpdateForMove`) was never-called and slightly raised UtilitiesCS coverage. | PASS |
| TypeScript | Line/branch | Zero changed `.ts`/`.tsx` files on this branch. | N/A (no changed files) |
| Python | Line/branch | Zero changed `.py` files on this branch. | N/A (no changed files) |
| PowerShell | Line/branch | Zero changed `.ps1`/`.psm1` files on this branch. | N/A (no changed files) |

- Baseline: 68.93% (128086/185819). Post-change: 69.44% (127873/184152). Disposition: PASS (positive delta; denominator and numerator dropped together with the deleted vendored package).

### Coverage evidence-completeness observation (non-blocking)

The canonical `artifacts/csharp/coverage.xml` and the raw `*.cobertura.xml` files that back the
per-package table were not committed to the feature evidence folder; only the derived Markdown
summaries are present. Independent re-parse of the raw XML was therefore not possible; the C#
coverage verdict rests on the executor's committed Markdown evidence plus independent
build/grep re-verification. This is a documentation-completeness gap, not a coverage regression or
a floor breach. Recommendation: commit the Cobertura XML alongside future coverage summaries for
auditability. Not counted as blocking.

## Independent Re-Verification (reviewer-run, no-mutation)

- `git grep "Swordfish" -- "*.cs" "*.csproj" "*.sln"` → exit 1 (zero matches). Confirms AC-13.
- `git grep -E "IScoCollection2?\b|ISubjectMapSco" -- "*.cs"` → exit 1 (zero). Confirms AC-3/4/5.
- `git grep "UpdateForMove" -- "*.cs"` → only unrelated `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` (a distinct private method with a real call site); the `QfcExplorerController.UpdateForMove` referencing `ISubjectMapSco` is gone. Confirms AC-6 with no dangling symbol.
- `git grep -E "F2E1680E-...|9A04D222-..." -- "*.sln"` → exit 1 (zero). Confirms AC-8/AC-9.
- `git grep "UtilitiesSwordfish" -- "*.csproj" "*.sln"` → exit 1 (zero). Confirms AC-7/AC-10.
- `.csproj` diff additions filter returned empty: all project-file changes are pure removals (no substituted or rewritten build elements).
- Modified-`.cs` diff inspection: `QfcExplorerController.cs` (dead method removal), `AppAutoFileObjects.cs`, `ConcurrentObservableCollection.cs`, `SloStack.cs`, and three test files — every non-deletion `.cs` change other than the dead-method removal is a comment/XML-doc reword ("Swordfish-free"→"vendored-dependency-free" and dangling `<c>`-reference removal) with no behavioral change.
- GitHub issue #317 confirmed OPEN via `gh issue view 317` (AC-12 follow-up correctly filed).

## General Code Change Policy

- **File-size limit (500 lines):** No file gained lines toward the limit; all `.cs` edits are deletions or single-line comment rewords. No violation.
- **Design principles / Simplicity first:** Disposition is removal (not migration) of interfaces with zero surviving implementers/consumers; this matches "Simplicity first" and avoids authoring interfaces no type consumes. Compliant.
- **Public API / breaking changes:** The removed interfaces (`IScoCollection<T>`, `IScoCollection2<T>`, `ISubjectMapSco`) had zero first-party consumers/implementers (verified by build green and repo-wide grep). No consumer break.
- **Comment-only sibling-feature edits (F1/F2 files):** Confirmed comment/XML-doc only, no code path change. Within F5's cross-cutting AC-13 scope ("epic completes here"), which requires the literal code-glob `Swordfish` search to return zero. Acceptable and necessary to satisfy AC-13.

## Unit Test Policy

- WI-4 removes three tests whose subject types are deleted wholesale; no surviving first-party
  production behavior loses its only test (UtilitiesCS held/rose). Compliant with "no regression on
  changed lines."
- AC-12: sender-identity regression coverage confirmed present on the clean base
  (`ConcurrentObservableCollection_Tests.cs`); lock-recursion behavioral coverage absent post-removal
  and correctly deferred to a new issue (#317) rather than authored against a clean type F5 does not
  own. Consistent with the general and C# unit-test policies and the spec Non-Goals.
- No temp-file usage introduced; no new external-dependency tests. Compliant.

## Tone

All feature artifacts reviewed use neutral, factual language. No tone-policy issue.

## Summary

| Policy area | Verdict |
|---|---|
| Scope integrity (full-branch audit) | PASS |
| Evidence location | PASS |
| C# toolchain (format/analyzers/nullable/test) | PASS |
| C# coverage (no regression, no backfill owed, testable-floor policy) | PASS |
| General code-change policy (file size, design, API) | PASS |
| Unit-test policy (WI-4 removals, AC-12 disposition) | PASS |
| Tone | PASS |

**Overall policy verdict: PASS. Blocking findings: 0.** One non-blocking observation
(uncommitted raw Cobertura XML) recorded above.
