# Policy Audit — itemviewer-breadcrumb-lifecycle-defects (Issue #488; also closes #475)

- Timestamp: 2026-08-28T06-44 (UTC)
- Branch: `bug/itemviewer-breadcrumb-lifecycle-defects-488` at `d9ed9eb2`
- Base: `epic/quickfiler-bug-family-integration`, merge base `12465043` (re-verified by this reviewer with `git merge-base`; the caller-supplied base SHA is the true merge base, not stale)
- Work mode: `full-bug` (from `issue.md`) — `spec.md` is the sole acceptance-criteria source; `user-story.md` is absent, as required (verified on disk)
- Audit scope: the full branch diff `12465043..d9ed9eb2` — 106 files (8 code/project paths, 94 evidence/documentation paths under the feature folder, 4 potential entries), 12 commits. PR context artifacts were regenerated at this HEAD before review (`artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`, hand-authored from `git diff --numstat` because the MCP collector is unavailable in this environment).

## Verdict

**PASS — zero Blocking findings.** All eight production/test paths conform to the general and C# code-change policies, the Bugfix Workflow is verified RED-first at commit granularity for all six defect units, all toolchain gates are verified non-vacuous from committed evidence, and the C# coverage verdict is PASS from figures this reviewer re-derived independently from the committed Cobertura documents. Six Non-blocking findings are recorded (§ 8).

## Rejected Scope Narrowing

None detected. The caller briefing supplied the full production diff, directed the audit at the full branch diff against `12465043`, and attempted no narrowing to a plan, task, phase, or file subset. The full diff scope was independently re-derived from `git diff --numstat 12465043..HEAD` and matches the caller's enumeration exactly.

## 1. Scope integrity

- **Production/test path set (8 paths), verified equal to the spec's permitted subset:** `ItemViewer.Breadcrumb.cs` (+110/−4), `BreadcrumbItemViewerLifecycleCoordinator.cs` (+16/−0), `BreadcrumbPopupUiOperations.cs` (+0/−5), `BreadcrumbDropDownHost.cs` (+2/−2), `BreadcrumbItemViewerLifecycleCoordinatorTests.cs` (+38/−1), `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` (+15/−12), new `ItemViewerBreadcrumbLifecycleRegressionTests.cs` (+480), `QuickFiler.Test/QuickFiler.Test.csproj` (+1/−0). Nothing else outside `docs/features/` changed.
- **Every explicitly excluded file is byte-identical to `12465043`** — verified by this reviewer with an empty `git diff` over: the six 489-owned `ItemViewer` partials (including `ItemViewer.cs`, `ItemViewer.Designer.cs`), the four 501-owned coordinator/hub files, all `QfcItemController.*` partials (484-owned), `QuickFiler/QuickFiler.csproj`, `IBreadcrumbDropDownHost.cs`, `IItemViewer.cs`, and `BreadcrumbDropDownIntegrationTests.cs` (which remains at exactly 500 lines).
- **`QuickFiler.Test.csproj` diff shape:** exactly one added `<Compile Include="Viewers\ItemViewerBreadcrumbLifecycleRegressionTests.cs" />` line, inserted immediately after the existing `Viewers\ItemViewerBreadcrumbDropDownContractTests.cs` entry, zero deletions, zero reordering — the eleven merged-sibling entries carried by the base are all intact (verified from the raw diff hunk: context lines only above and below the insertion).
- The working tree at HEAD is clean.

## 2. Toolchain gates (verified from committed evidence; figures re-derived where cheap)

| Gate | Result | Verified how |
|---|---|---|
| csharpier check | EXIT 0; 1554 files, zero unformatted | `evidence/qa-gates/csharpier-check.md`; empty baseline unformatted set makes the gate absolute |
| MSBuild analyzers `/t:Rebuild` | EXIT 0; 0 errors, 5 warnings (= baseline); `Skipping target "CoreCompile"` count 0 against 83 `CoreCompile:` executions | `evidence/qa-gates/msbuild-analyzers.md`; non-vacuity counters recorded per the anti-warm-build rule |
| MSBuild nullable `/t:Rebuild` | EXIT 0; 0 errors; zero CS86xx; `/p:Nullable=enable` correctly not supplied; CoreCompile skip count 0 | `evidence/qa-gates/msbuild-nullable.md` |
| `QuickFiler.Test` vstest | 1201/1201 passed, 0 failed (baseline 1192; +9 = ten added tests minus one deleted) | Counters re-parsed by this reviewer from the committed TRX: `total="1201" passed="1201" failed="0"` |
| Full-suite coverage run | 9 assemblies, 6821/6821 passed, 0 failed (baseline 6812) | `evidence/qa-gates/coverage-final.md`; discovery list excludes sibling worktrees |
| Consecutive clean pass | Four stages at 06-20/06-21/06-22/06-25 UTC, zero restarts; all seven owned files SHA-identical across the pass | `evidence/qa-gates/toolchain-consecutive-pass.md` |

**Bugfix Workflow (RED-first) verified at commit granularity for all six defect units.** The branch is structured as baseline commit `b61faddc` first, then one commit per unit (D1 `26b91875`, D2 `b210ceb2`, D3 `a52fb2ba`, D4 `5c841d1f`, D5 `fbdd78d1`, #475 all three parts together `9a0bd44e`). Every fail-before TRX was re-parsed by this reviewer and shows a genuine failure: D1 1/1 failed ("no exception was thrown" on the disposal assertion), D2 1/1, D3 1 of 2 failed (negative case red, positive green — the correct pre-fix shape), D4 2/2, D5 1/1, #475 lazy 1/1 (throws pre-laziness), #475 ctor 1/1 (ArgumentNullException instead of the fail-fast throw in the reverted state). Every pass-after TRX is green, and the failing test names are exactly the delivered test names.

## 3. Coverage (C#)

C# coverage verdict: **PASS** — repository-wide line coverage 85.2830 percent raw and 85.0799 percent on the CLAUDE.md § UT2 testable denominator (both at or above the 85 percent uniform floor and the 80 percent § UT2 floor), branch coverage 79.2255 percent raw (above the 75 percent floor), re-parsed by this reviewer from the root `coverage` element of the committed `evidence/qa-gates/coverage-final.cobertura.xml` (`line-rate="0.85283"`, `branch-rate="0.792255"`, 54692/64130 lines, 13012/16424 branches).

- **No-regression:** baseline (`evidence/baseline/coverage-baseline.cobertura.xml`, root `line-rate="0.852607"`) to final moved +0.000223 raw and +0.000237 testable — both denominators rose. Verified from both committed documents, not from the markdown alone.
- **Per-file coverage for the three measured owned production files, independently re-aggregated by this reviewer** (deduplicating `(filename, line)` across `class` elements, the executor's stated method, which this reviewer's parse reproduces exactly): `BreadcrumbItemViewerLifecycleCoordinator.cs` 300/330 = 0.909091 (baseline 288/318 = 0.905660, rose); `BreadcrumbPopupUiOperations.cs` 229/231 = 0.991342 (uncovered-line count unchanged at 2; the −0.000111 delta is the arithmetic of deleting three fully-covered lines, not a regression); `BreadcrumbDropDownHost.cs` 279/281 = 0.992883 (identical uncovered lines 318, 360 in both documents). All three are at or above the 90 percent new/changed-member target, and none of D2's or #475's added measured lines appears in the uncovered-line sets.
- **`ItemViewer.Breadcrumb.cs` matches zero `class` elements in both Cobertura documents** — independently confirmed by this reviewer's parse. `ItemViewer.cs:18-20` carries `[ExcludeFromCodeCoverage]` on the partial type (re-read at HEAD), so D1, D3, D4, D5, and #475 part 3 are coverage-exempt by construction; their regression tests are required by the Bugfix Workflow, and flat coverage there is not a testing gap. No new `[ExcludeFromCodeCoverage]` is introduced and none is removed (zero occurrences of the token in the full branch diff).
- **Coverage artifact status:** the C# coverage artifact is present at the canonical evidence location, `evidence/qa-gates/coverage-final.cobertura.xml` (with its baseline counterpart), and verification was performed from it. The hook-conventional path `artifacts/csharp/coverage.xml` was deliberately not materialized: `artifacts/` is gitignored so a file there is not evidence, and a stale or shape-mismatched file at that path is a known source of false gate failures in this repository. The committed feature-evidence Cobertura is the authoritative artifact for this review.
- TypeScript, Python, and PowerShell each have zero changed files on this branch (verified from the regenerated PR context summary and `git diff --numstat`), so no coverage row is mandatory for them.

## 4. Evidence Location Compliance

- All 94 evidence/documentation paths in the branch diff live under `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/` (evidence in `baseline/`, `qa-gates/`, `regression-testing/`, `other/`) or under `docs/features/potential/` (the three follow-up entries plus the promoted #670 entry) — the canonical locations. Zero violations.
- The branch diff was scanned for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, and `artifacts/coverage/`: none exist. The repository does not carry `validate_evidence_locations.py`; the scan was performed manually against `git diff --name-only 12465043..HEAD`.
- No helper script (`.ps1`, `.py`, `.sh`) exists anywhere under `evidence/` — verified by extension search; none forces a spurious validator outcome.
- No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events: no instruction supplied a non-canonical evidence path.

## 5. General code change policy

- **Bugfix Workflow:** followed for all six units — failing regression test first with committed failing TRX, minimal targeted fix second, full toolchain loop third (one iteration, zero restarts). #475's three parts land in a single commit as the spec requires.
- **Minimality:** each production change is exactly the spec's design: D1 +4 core lines (type-tested concrete dispose with a fresh pattern variable), D2 field + assignment + guarded replay in the newly-adopted branch only, D3 field + reference-equality fail-fast + disposal clear, D4 one private guard called from four entry points, D5 a two-property disposal throw, #475 deletion + two identifier swaps + the lazy `Func<>` parameter. No opportunistic refactor. The three genuinely out-of-scope defects were promoted as potential entries instead of absorbed (D-14).
- **File size:** every touched file is at or under 500 lines at HEAD, re-measured by this reviewer: `ItemViewer.Breadcrumb.cs` 425, `BreadcrumbItemViewerLifecycleCoordinator.cs` 497, `BreadcrumbPopupUiOperations.cs` 489, `BreadcrumbDropDownHost.cs` 463, `BreadcrumbItemViewerLifecycleCoordinatorTests.cs` 419, `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` 483, new regression file 480. The constrained coordinator absorbed +16 against its 19-line headroom (R1 respected); `BreadcrumbDropDownIntegrationTests.cs` untouched at exactly 500.
- **Error handling:** all three new failure modes are fail-fast diagnostics (`InvalidOperationException` naming the operation for D3/D4, `ObjectDisposedException` for D5); #475 restores a fail-fast contract the production dispatcher already documented. No silent path is introduced; no log statement added or removed, matching the spec.
- **Public surface:** no public member added, removed, or re-signed. The deleted selector and the re-signed lazy helper are `internal`/`private` respectively. The one observable behavior change (the public 7-param host constructor now failing fast without an ambient context) is the spec-mandated #475 outcome with no in-repo production caller.

## 6. C# and unit test policy

- All ten added tests use MSTest attributes, Moq (`MockBehavior.Strict` where a collaborator must not be touched), and FluentAssertions, with explicit Arrange/Act/Assert sections and doc comments stating scenario and expected outcome, including the D3 negative test's deliberate `NotBeOfType<ObjectDisposedException>()` exclusion (ODE derives from IOE) and both D4 tests' explicit "proves the guard fires and does not prove the race is absent" limitation.
- Determinism: the new `DrainableSynchronizationContext` makes "posted but not drained" a first-class state with no second thread and no timing construct; the #475 seam test nulls `_context` reflectively with the documented justification. No `Thread.Sleep`, `Task.Delay`, wall-clock wait, or temporary file appears anywhere in the branch diff (scanned). One pre-existing `Task.Run` is retained in the controlled-context half of the replacement boundary test — adjudicated acceptable in § 7 ruling 3.
- No `Interlocked`, `lock`, `Monitor`, `Volatile`, or `Mutex` construct is introduced (scanned; the D4 criterion requires their absence).
- Test placement follows the repository's established MSTest project layout (`QuickFiler.Test/Viewers/`), matching every sibling.

## 7. Rulings requested by the caller

1. **Committed raw Cobertura (2 × 10.7 MB): acceptable to retain for this cycle; Non-blocking (PA-2).** The approved plan's P0-T14/P8-T6 named those exact evidence paths, so the executor was plan-faithful, and the retained documents are what made this review's independent verification possible — the per-file and repo-wide figures above were re-derived from them, not taken from the markdown. The cost is 21.4 MB of permanent history growth against a maintainer preference for compact summaries. Disposition: raise at epic fan-in for a maintainer decision. If removal is chosen, the replacement is (a) the already-committed derived documents `coverage-final.md` and `coverage-delta.md`, plus (b) a compact extract holding the root `coverage` element and the per-`(filename, line)` hit tables for the three measured owned files — that is the exact subset this review consumed. Note that deleting the XMLs in a later commit does not shrink history; the decision is really about whether future features keep committing full raw documents.
2. **`EfcItemController.CleanupTests.cs:41` (C6 site 18) left unedited: correct.** Constraint C6's escalation rule mandates record-and-report, not edit; the file is outside this feature's seven owned paths, so editing it would itself have been a scope violation. The executor's risk bounding is sound and was spot-checked: the file names no guarded member, the guard's null escape covers the null-ambient case, and the empirical settlement (full `QuickFiler.Test` at 1198/1198 during Phase 4 and 6821/6821 repo-wide at the end) is green. The executor's independent re-derivation of 19 executable sites against C6's 13, and the Part 4 correction of C6's stale reachability premise (the constructor no longer calls `TaskScheduler.FromCurrentSynchronizationContext()` after 489's consolidation), are exactly the conduct the constraint asked for. Residual: the site's ambient-context dependence is a latent test-hygiene wrinkle in a file owned elsewhere — recorded as PA-3, promotion recommended, no action in this branch.
3. **The retained `Task.Run`: acceptable under `.claude/rules/general-unit-test.md`.** The banned-API list is `setTimeout`, `Thread.Sleep`, `Task.Delay`, real wall-clock waits, and `Date.now()` outside a clock interface; `Task.Run` is not on it, and the relevant acceptance criterion bans exactly the four constructs verified absent. The construct is carried over verbatim from the pre-change test as plan task P6-T3 instructs, and its completion is signal-based (`context.Drain(post)` pumps until the posted work completes) with no timeout and no wall-clock dependence; it passed in every recorded run including the baseline. Recorded as CR-4 (Info) only.
4. **D4 proxy honesty: confirmed, no overclaim.** The statement that the test "proves the guard fires and does not prove the race is absent" appears verbatim in the spec (Test Strategy, D4 design rationale, and the D4 acceptance criterion), in `evidence/other/change-description.md` (six occurrences including a dedicated section), and in both D4 test methods' own `<remarks>` — all verified by direct read. No acceptance criterion asserts race elimination; the D4 criterion set explicitly says so.
5. **AC honesty: no criterion is checked without support.** Findings of the spot-check are in `feature-audit.2026-08-28T06-44.md`; the named high-risk claims all verify: the byte-identical set (including `QuickFiler.csproj`, `ItemViewer.Designer.cs`, and the 500-line `BreadcrumbDropDownIntegrationTests.cs`) produces empty diffs against `12465043`; the >= 90 percent per-measured-member criterion holds on this reviewer's independent re-aggregation; and all six defect units have genuine committed fail-before TRX with the delivered test names. The one criterion an executor could not close alone (the #670 issue for the D5 faulted-task finding) was closed by the orchestrator through the MCP promotion path, honestly recorded in `evidence/other/ac-reconciliation.md` as remediation-then-supersession rather than papered over, and #670 is verified OPEN on GitHub with the recorded title.

## 8. Findings

**Blocking: 0.**

### Non-blocking

- **PA-1 (Minor) — host tokens in committed TRX evidence.** All 19 committed `.trx` files embed the absolute assembly path `c:\users\<account>\repos\taskmaster\...` in their `storage`/`codeBase` attributes, and `runUser` retains the machine token (a `<machine><user>` domain token), although the run name, user, and `computerName` were sanitized to placeholder form. This is partial compliance with the artifact-hygiene rule the sibling 489 evidence meets fully (489's committed evidence carries zero such tokens). Adjudicated Non-blocking because the identical tokens are already present throughout the integration branch in previously merged sibling evidence (501, 608, 439), so this branch neither introduces the exposure class nor can cure it alone. Recommendation: one mechanical sanitization pass over `evidence/**/*.trx` before fan-in, and a repo-wide cleanup entry for the pre-existing instances.
  **PA-1 REMEDIATED by the orchestrator at 2026-08-28T07-03**, before fan-in, exactly as this finding
  recommends. One mechanical pass over all 19 `evidence/**/*.trx` replaced the absolute worktree path
  with `REDACTED-WORKTREE-ROOT` and the `runUser` domain token with `REDACTED-DOMAIN-USER`. A
  case-insensitive sweep of the feature folder for the account and machine names now returns **0**
  files. The sweep also confirmed that the only remaining occurrences anywhere in the repository sit in
  four previously-merged promoted potential entries and in untracked `bin/Debug` build outputs, none of
  which is in this branch's 108-file diff — consistent with this finding's assessment that the branch
  neither introduced the class nor could cure it alone. The repo-wide cleanup entry this finding
  recommends is still owed.

  **A second, undetected defect was found and fixed during that pass.** The executor's original scrub
  substituted the placeholder `<worktree-root>` into `storage` and `codeBase` XML *attribute values*.
  A raw `<` is not legal in an XML attribute value, so **all 19 committed TRX files were not
  well-formed XML** and would not parse — verified against the committed blobs at `HEAD`, so the defect
  predates this remediation and was not introduced by it. This audit did not catch it because the
  coverage figures were re-parsed from the Cobertura documents rather than from the TRX. Replacing the
  7,306 occurrences with the bracket-free `REDACTED-WORKTREE-ROOT` restored well-formedness: **all 19
  files now parse, 0 failures.** Only path and identity attributes changed; every counter is preserved,
  and re-parsing them independently corroborates the delivered result — baseline `total=1192 passed=1192
  failed=0`, final `total=1201 passed=1201 failed=0`, the D4 full-suite run `1198/1198`, and genuine red
  fail-before runs for all six defect units (`488-d1-fail` 0/1, `488-d2-fail` 0/1, `488-d3-fail` 1 of 2,
  `488-d4-fail` 0/2, `488-d5-fail` 0/1, `475-lazy-fail` 0/1, `475-ctor-fail` 0/1). The evidence is
  strictly better after remediation than before: previously unparseable, now machine-readable.

  **The repo-wide half of PA-1, and PA-2, are now tracked as issue
  [#671](https://github.com/drmoisan/TaskMaster/issues/671)** — "Bug:
  trx-evidence-host-tokens-and-malformed-xml" — covering the pre-existing tokens in features 501, 608
  and 439, the angle-bracket placeholder trap that breaks TRX well-formedness, and the raw-Cobertura
  size convention. It is filed rather than left as prose because a finding recorded only in a feature
  folder stops being visible once the feature merges. **#671 is a follow-up and is deliberately NOT in
  this pull request's `Closes` list.**

- **PA-2 (Info) — 21.4 MB of committed raw Cobertura.** Ruling 1 above; maintainer decision owed at fan-in.
- **PA-3 (Info) — C6 enumeration stale; one construction site installs no context.** Ruling 2 above; recorded and reported per the constraint's own rule; recommend promoting the `EfcItemController.CleanupTests.cs` ambient-context dependence as a test-hygiene note to that surface's owner.
- **PA-4 (Info) — repository threshold-wording conflict persists.** CLAUDE.md § UT2 states 80 percent repo-wide / 90 percent new-code line floors while `.claude/rules/quality-tiers.md` states uniform 85 line / 75 branch. This branch clears the stricter of each pair (85.28 line, 79.23 branch, per-measured-file >= 90), so no gate outcome depends on the conflict. Carried from the sibling reviews; owed to the governance sync, not this feature.
- **CR-4 (Info) — retained `Task.Run`** (ruling 3; recorded in the code review).
- **CR-5 (Info) — coordinator file at 497/500** (three lines of headroom remain; recorded in the code review).

## 9. Acceptance criteria cross-check

All 54 `spec.md` criteria are checked (`- [x]` 54, `- [ ]` 0, re-counted at HEAD), the spec diff against base consists solely of the 54 checkbox flips plus the nine-line dated D5-discharge amendment (verified by filtering the diff), and no criterion text was modified. Detail in `feature-audit.2026-08-28T06-44.md`. No criterion was checked off or altered by this reviewer.
