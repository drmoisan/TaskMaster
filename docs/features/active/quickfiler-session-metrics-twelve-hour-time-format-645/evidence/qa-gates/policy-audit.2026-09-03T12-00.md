# Policy Audit — quickfiler-session-metrics-twelve-hour-time-format-645

- Timestamp: 2026-09-03T12-00
- Reviewer: feature-review agent
- Work mode: `full-bug` (per `issue.md` marker; AC source is `spec.md` § Acceptance Criteria, 10 items)
- Branch: `bug/quickfiler-session-metrics-twelve-hour-time-format-645`, HEAD `b3bb8fef`
- Base for diff: `origin/main`, merge-base `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1`, independently
  recomputed with `git -C <worktree> merge-base HEAD origin/main` and confirmed identical to the
  value the delegation prompt supplied. `git fetch origin main` plus a name-only diff of the four
  in-scope files between the merge-base and current `origin/main` returned no commits, confirming
  the merge-base is still current.
- Commits reviewed: `05e4add3`, `494bdb3d`, `9cc37d01`, `fb8f94ca`, `6c1ac1f1`, `8d7935fc`, `b3bb8fef`.

## Rejected Scope Narrowing

None detected. The delegation prompt scopes the review to the full branch diff against the
resolved merge-base and explicitly instructs independent verification of all 10 AC items; it does
not attempt to narrow the audit to a task/phase subset.

## Evidence Location Compliance

All newly added files in the branch diff were enumerated via
`git diff --name-status 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1 HEAD` and cross-checked against
the canonical evidence scheme in `evidence-and-timestamp-conventions`.

- All evidence artifacts are under
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/{baseline,regression-testing,qa-gates,other}/`.
- No path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`,
  or any other non-canonical path appears in the diff (`git diff --name-only <merge-base> HEAD |
  grep '^artifacts/'` returned no matches).
- `validate_evidence_locations.py` was not found anywhere in this worktree
  (`find <worktree> -iname validate_evidence_locations.py` returned no results); the location check
  above was therefore performed manually against the full diff file list rather than via that
  script. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events were needed — the delegation prompt did
  not supply a non-canonical evidence path.

**Verdict: PASS** (evidence *location* is fully compliant). A separate, content-level finding on
two of these evidence files is recorded below under "Evidence Hygiene — Host Path Leak".

## Scope and Forbidden-Path Compliance

- `git diff --name-status 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1 HEAD` lists exactly four
  production/test files plus documentation/evidence files under the feature folder. No file under
  `QuickFiler/Legacy/`, no `TaskVisualization/TaskViewer.Designer.cs`, and no path matching
  `.claude/**`, `.codex/**`, `.agents/**`, `config/blast-radius.json`, or
  `config/orchestration-routing.json` appears anywhere in the diff.
- `494bdb3d` ("de-backtick scope-exclusion paths") touches only `spec.md`/plan prose that *names*
  the excluded paths, not the paths themselves — confirmed by reading the diff of that commit; it
  is not a scope-boundary violation.
- An untracked file, `docs/features/potential/promoted/2026-09-02-quickfiler-date-time-format-missing-invariant-culture.md`,
  is present in the worktree but is not part of any commit on this branch and does not appear in
  the `merge-base..HEAD` diff. It is the orchestrator's promotion record for the known,
  out-of-scope follow-up (issue #742) and requires no action from this review.

**Verdict: PASS.**

## CSharpier Formatting (C#1)

- `dotnet tool run csharpier format` and `check`, scoped to the four in-scope files, both ran with
  `EXIT_CODE: 0` (`evidence/qa-gates/p4-t1-csharpier-format.2026-09-03T11-34.md`,
  `p4-t2-csharpier-check.2026-09-03T11-34.md`). SHA-256 before/after hashes on the format pass are
  identical for all four files (`RewrittenCount: 0`), i.e. the files were already CSharpier-clean.
- Plan deviation: the plan scopes CSharpier to the four in-scope files rather than repo-wide `.`,
  reasoned to avoid an unrelated repo-wide rewrite crossing the scope-lock boundary. This is a
  documented, justified deviation from CLAUDE.md's literal `.` invocation and does not weaken the
  formatting gate for the files actually changed.

**Verdict: PASS.**

## .NET Analyzers (C#1)

- `MSBuild.exe TaskMaster.sln -t:Rebuild ... -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`
  ran with `EXIT_CODE: 0`, 0 errors, 5 warnings (pre-existing `System.Reactive` packages.config
  notices, identical to baseline). 57 `CoreCompile:` entries confirmed a genuine rebuild, not an
  incremental no-op (`p4-t3-analyzer-rebuild.2026-09-03T11-35.md`).

**Verdict: PASS.**

## Nullable / Type-Checking (C#1)

- `MSBuild.exe TaskMaster.sln -t:Rebuild ... -p:TreatWarningsAsErrors=true` ran with `EXIT_CODE: 0`,
  0 errors, 5 pre-existing warnings, 56 `CoreCompile:` entries confirming a genuine rebuild
  (`p4-t4-nullable-rebuild.2026-09-03T11-36.md`). `/p:Nullable=enable` was correctly **not** passed,
  per CLAUDE.md C#1's explicit prohibition (forcing it is documented to produce ~195 unrelated
  errors). No `#nullable enable` directive is present in any of the four changed files, so this
  change does not newly opt into per-file nullable analysis; no CS86xx diagnostics apply.

**Verdict: PASS.**

## MSTest / Moq / FluentAssertions Conventions (CUT1–CUT2)

- Both test files use `[TestClass]`/`[TestMethod]` (MSTest), `Moq` (`groups.Verify(...)` on a
  `Mock<...>`), and `FluentAssertions` (`.Should().Equal(...)`), consistent with the rest of each
  file and with repo convention. The two touched assertions in
  `QfcHomeControllerMetricsTests.cs` build `expectedDataLineBeg` dynamically from the same
  `FakeTimeProvider`-sourced `expectedLocal` the production code consumes, so the test literal
  change is a pure format-string mirror, not a hand-picked value. The one touched fixed literal in
  `EfcHomeControllerMetricsTests.cs` (`01:05` → `13:05`) was independently verified against the
  fixture's `MetricsNow = new DateTime(2026, 7, 4, 13, 5, 0)`: `13:05` is the correct `HH:mm`
  rendering of that fixture.
- Both touched clock-seam tests continue to source time via `FakeTimeProvider`
  (`QfcHomeControllerMetricsTests.cs`) or a fixed fixture `DateTime` consumed through the
  production method's parameter (`EfcHomeControllerMetricsTests.cs`), not the wall clock.

**Verdict: PASS.**

## General Code Change Policy (`.claude/rules/general-code-change.md`)

- Simplicity / minimal diff: the fix is a literal three-character format-string substitution at
  each of three call sites, with no control-flow, signature, or logic change. Matches spec.md's
  "Design summary."
- File size limit (500 lines): `QfcHomeController.Metrics.cs` 245 lines, `EfcHomeController.Metrics.cs`
  124 lines, `QfcHomeControllerMetricsTests.cs` 477 lines, `EfcHomeControllerMetricsTests.cs`
  490 lines (all measured at HEAD via `git show HEAD:<path> | wc -l`). All four are within the
  500-line limit.
- No new dependencies, no I/O boundary changes, no public API changes.

**Verdict: PASS.**

## General Unit Test Policy (`.claude/rules/general-unit-test.md`)

- Independence/isolation/determinism: unchanged from the pre-existing tests; both touched test
  methods already drive their fixed/injected clock rather than the wall clock (verified above).
  No new temp files, no new external dependencies.
- Coverage — repo-wide floor: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws below the
  repository's 80% floor at 23.8225% both at baseline (`p0-t15-coverage-baseline...md`) and at
  final (`p4-t5-coverage-final...md`), Delta = 0.0000 pp (`p4-t6-coverage-delta...md`). This is a
  documented, pre-existing, repository-wide condition unrelated to this change (identical figure
  before and after), per the task's known-environment-defect carve-out. It is **not** attributable
  to this branch and is **not** treated as a blocking finding here, consistent with that carve-out.
- Coverage — changed lines: the three changed production lines and the four dependent test-literal
  changes are exercised by the scoped pre- and post-edit regression runs
  (`p0-t14-scoped-regression-baseline...md`: 27/27 pass under the pre-fix literal;
  `p3-t3-scoped-regression-postedit...md`: 27/27 pass under the fixed literal) and by the full-assembly
  run (`p4-t5-coverage-final...md`: 1312/1312 pass). No changed line is uncovered or regressed.
- AAA structure and documentation: both touched test methods already use Arrange/Act/Assert
  comments and descriptive names; the two touched XML doc comments were corrected to describe
  `"HH:mm"` accurately (previously referenced the pre-fix `"hh:mm"` literal), which is itself a
  comment-accuracy improvement, not a regression.

**Verdict: PASS** (repo-wide coverage floor breach is a pre-existing, non-regressed, environment
condition and is excluded from this verdict per the task's documented carve-out; see the
"Coverage Verification" section below for the mandatory per-language reporting).

## Coverage Verification (mandatory reporting)

- Languages with changed files in this branch: **C#** only (the two production `.cs` files and two
  test `.cs` files). No TypeScript, Python, or PowerShell files are changed.
- Coverage artifact: `artifacts/csharp/coverage.xml` (canonical path) is absent in this worktree;
  however, the branch instead committed a Cobertura report directly under the feature's evidence
  tree — `evidence/qa-gates/coverage-final.cobertura.xml` (final) and
  `evidence/baseline/coverage-baseline.cobertura.xml` (baseline) — which is the accepted
  substitute artifact under this repo's established convention for feature-scoped coverage evidence.
- Repo-wide line coverage (post-processed, thrown value): **23.8225%**, below both the 80% floor
  cited in CLAUDE.md/General Unit Test Policy and the 85%/75% uniform floor cited in
  `.claude/rules/quality-tiers.md`. This is unchanged from baseline (Delta = 0.0000 pp) and is a
  pre-existing, repository-wide condition per the task's documented environment-defect carve-out,
  not introduced or worsened by this branch.
- New files: none. This is a pure modification of four pre-existing files; no new production or
  test file was added.
- Modified files: the two production files' relevant classes were spot-checked directly in
  `coverage-final.cobertura.xml` (`QuickFiler.Controllers.QfcHomeController` class entries,
  line-rate 0.699 and 0.839 for the async-state-machine class covering `WriteMetricsAsync`). Both
  changed lines are within methods that the scoped regression run (27/27 pass) directly exercises;
  no changed-line regression is evident.

**Coverage verdict: FAIL** (repo-wide C# line coverage 23.8225% is below both the CLAUDE.md 80%
floor and the `.claude/rules` 85%/75% uniform floor cited in "80-vs-85 coverage floor doc
conflict" from prior review history). **Disposition: non-blocking.** This FAIL is a pre-existing,
repository-wide condition identical at baseline and final (Delta = 0.0000 pp), documented in the
task's own known-environment-defect carve-out and confirmed independently against the committed
Cobertura artifacts in this review. The three changed production lines and their four dependent
test-literal updates are covered by passing tests (27/27 scoped, 1312/1312 full-assembly). No
remediation of the repo-wide floor is required of this branch; the floor is owed by a separate,
repository-wide coverage-uplift effort.

## Evidence Hygiene — Host Path Leak (BLOCKING)

Both committed Cobertura evidence files —
`docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml`
and
`docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml`
— embed the reviewing account name and the absolute item-worktree path in every `<class
filename="...">` attribute. Verified by direct count:

```
git grep -c "DanMoisan" <both files>
  .../evidence/baseline/coverage-baseline.cobertura.xml:2007
  .../evidence/qa-gates/coverage-final.cobertura.xml:2007
```

Each file carries 2,007 occurrences of the literal path
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a6cd1c774527c71c3\...` (4,014
occurrences total across the two files), introduced in commits `9cc37d01` (baseline) and `6c1ac1f1`
(final). This is a host-identifying data leak: it discloses the operator's Windows account name and
local directory layout in a file intended to be merged into shared repository history. Notably,
the plan's own "Plan-Level Decisions and Deviations" §5 explicitly reasoned about *avoiding*
host-identifying data in evidence (choosing not to log raw TRX output because it "would also embed
the account and machine name in its default filename"), but the same concern was not applied to
the much larger Cobertura evidence committed two tasks later in the same phase.

This does not affect the correctness of the production fix itself (which is otherwise sound and
independently verified above), but it is a policy-hygiene defect in the branch's own committed
evidence artifacts.

**Verdict: FAIL — BLOCKING.** See `remediation-inputs.2026-09-03T12-00.md`.

## Tonality Policy

All evidence artifacts and plan/spec documents reviewed use neutral, factual, professional
language consistent with `.claude/rules/tonality.md`. No hyperbole, humor, or motivational
phrasing was observed in the reviewed documents.

**Verdict: PASS.**

## Summary Table

| Area | Verdict |
|---|---|
| Scope / forbidden paths | PASS |
| Evidence location (path scheme) | PASS |
| CSharpier formatting | PASS |
| .NET analyzers | PASS |
| Nullable / type-check | PASS |
| MSTest/Moq/FluentAssertions conventions | PASS |
| General Code Change Policy | PASS |
| General Unit Test Policy (changed lines) | PASS |
| Coverage — C# repo-wide | FAIL (non-blocking, pre-existing, Delta = 0) |
| Evidence hygiene — host path leak | FAIL (BLOCKING) |
| Tonality | PASS |
