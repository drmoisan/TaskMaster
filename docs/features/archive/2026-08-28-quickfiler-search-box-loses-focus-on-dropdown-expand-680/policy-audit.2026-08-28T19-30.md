# Policy Audit — quickfiler-search-box-loses-focus-on-dropdown-expand (Issue #680)

- Review cycle: R4 pass 2 (third full review of this branch; second re-audit after the pass-2 remediation)
- Timestamp: 2026-08-28T19-30
- Reviewer: feature-review agent
- Branch: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680` @ `4cf822e8`
- Base: `main` (`origin/main` @ `b0c7fa18`); merge base `b0c7fa18` (verified this session via `git merge-base HEAD main`)
- Diff scope: full branch diff `b0c7fa18..4cf822e8` — 132 files (13 C#/build, 12 TRX evidence, feature-folder docs, agent-memory files, 1 promotion doc)
- Work mode: `full-bug` (verified from `issue.md` `- Work Mode:` marker); `spec.md` is the sole AC source

Verdicts: **0 Blocking findings.** 1 non-blocking FAIL row carried (one modified file at 82.41% line coverage vs the 85% rules floor, dispositioned below), 3 non-blocking observations (recurring future-dated artifact stamps, canonical coverage-artifact path, stale AC-6 footprint enumeration in spec prose).

## 1. Rejected Scope Narrowing

None detected. The calling prompt mandated the full branch-vs-base diff with maximum scrutiny; no plan, task, or caller text attempted to narrow the audit scope. The prior-cycle context supplied by the caller was treated as untrusted historical data and every prior "zero hits" sanitization claim was re-verified from the committed tree this session.

## 2. Evidence Location Compliance

- Branch diff scanned for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`: **zero occurrences — PASS**. All evidence artifacts live under `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/<kind>/` (canonical), including the pass-2 remediation's `qa-gates/r2-*`, `regression-testing/r2-*`, and `regression-testing/r-p2-t3/` artifacts.
- `validate_evidence_locations.py` does not exist in the TaskMaster repository (confirmed by filesystem search); the scan above was performed directly against `git diff --name-only b0c7fa18..HEAD`.
- The pass-2 remediation plan records `EVIDENCE_LOCATION_OVERRIDE_REJECTED: not applicable` — no non-canonical path was supplied, and none was used.

## 3. Host-Identity Sanitization Sweep (primary check this cycle — R2 verification)

Given three recurrences of the self-referential-leak defect class on this branch, the sweep was re-run from scratch against the committed tree at HEAD (`4cf822e8`), not carried from any prior cycle's claim.

- Method: case-insensitive fixed-string content sweep of all 132 changed files' HEAD blobs for (a) the real account name, (b) the real machine name, (c) the `c:\users\` / `c:/users/` path prefix; plus a filename sweep of all 132 changed paths for (a) and (b). Working tree verified clean (`git status` clean), so working-tree and HEAD content are identical.
- Result (a) account token: **0 hits** in content, **0 hits** in filenames — PASS.
- Result (b) machine token: **0 hits** in content, **0 hits** in filenames — PASS.
- Result (c) `c:\users\` prefix: 10 files match, every occurrence inspected — all are sanitized placeholder forms (`<user>`, `<account>`, XML-escaped `&lt;user&gt;`) or generic rule prose; none is followed by a real account segment — PASS.
- TRX attribute audit: all 12 committed TRX files individually inspected at HEAD; every `runUser` reads `&lt;host&gt;\&lt;user&gt;`, every `computerName` (2,649 occurrences total across the 12 files) reads `&lt;host&gt;`, every `runDeploymentRoot` reads `&lt;user&gt;_&lt;host&gt;_<stamp>`, and all `storage=` paths (the lowercase-path trap) carry the escaped placeholder prefix — PASS.
- The two TRX files the orchestrator sanitized directly after the pass-2 QA loop (`r2-p5-t4/r2-p5-t4.trx`, `r2-p5-t5/r2-p5-t5.trx`) are confirmed sanitized and well-formed.
- Sanitization records comply with the AFTER-values-only convention: no committed record quotes a raw "before" value (confirmed by the zero-hit sweep — a quoted before-value would itself have been a hit).

**R2 (prior cycle's Blocking finding): CLOSED — verified independently.**

## 4. TRX Integrity and Red-Run Preservation (RC-2 verification)

- XML well-formedness: all 12 TRX files in the branch diff parse via `[xml]` load — **12/12 WELLFORMED — PASS**.
- The AC-3 fail-before red-run TRX at `evidence/regression-testing/p2-t3/p2-t3.trx` is restored and shows the red counters: total=27, passed=25, failed=2, outcome=Failed — matching the original `72b4b7ed` capture — PASS.
- The remediation's own green run is preserved at the non-colliding path `evidence/regression-testing/r-p2-t3/p2-t3.trx` (36/36 passed), and `p2-t3-new-test-green.2026-08-28T19-27.md` references the relocated path — PASS.

**RC-2 (prior cycle's task-ID collision): CLOSED — verified independently.**

## 5. File Size Compliance (R1 re-verification)

All 13 branch-touched C#/build files re-measured at HEAD with `awk NR` (the `Measure-Object -Line` undercount trap avoided):

| File | Lines | Limit check |
|---|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 498 | PASS (near ceiling) |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | 107 | PASS |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | 460 | PASS |
| `QuickFiler/Viewers/IItemViewer.cs` | 200 | PASS |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | 97 | PASS |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | 263 | PASS |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 486 | PASS (near ceiling) |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` | 499 | PASS (near ceiling; pre-existing, untouched by this branch's later commits) |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` | 385 | PASS |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs` | 427 | PASS |
| `QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs` | 174 | PASS |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` | 150 | PASS |
| `QuickFiler.Test/Viewers/ItemViewerSearchDismissalContractTests.cs` | 95 | PASS |

**R1 (cycle-1 Blocking finding): remains CLOSED.** The 500-line ceiling holds at head; the relocation to `BreadcrumbDropDownHost.Open.cs` is intact.

## 6. Toolchain Compliance

No production or test code changed after commit `c4e96b72` (verified: `git diff c4e96b72..HEAD -- '*.cs' '*.csproj'` is empty), so the cycle-2 code verification carries forward, and the pass-2 remediation's own QA loop re-ran the full gate set against the identical code state:

- Format: `dotnet tool run csharpier check .` exit 0 (`r2-p5-t1-format-check.2026-08-28T23-35.md`) — PASS
- Analyzers: `msbuild /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exit 0 (`r2-p5-t2-analyzers.2026-08-28T23-44.md`) — PASS
- Nullable/type-check: `msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true` exit 0 (`r2-p5-t3-nullable.2026-08-28T23-47.md`) — PASS
- Pinned suites: 75 tests, 0 failures (`r2-p5-t4`, TRX verified this session) — PASS
- Full QuickFiler.Test suite: green (`r2-p5-t5`, 8553-line TRX verified well-formed and sanitized) — PASS
- Reviewer independent rerun this session: `BreadcrumbDropDownHostTests` scoped vstest run (Extensions\TestPlatform binary, `/InIsolation`, `TaskMaster.cli.runsettings`, results directed outside the repository) — **36/36 passed**, including the cycle-2 composition test `OpenAsync_TakeFocusReopenAfterNonCapturingOpenWithPredicateFalse_RestoresAutoCloseButSuppressesFocus` — PASS

## 7. Test Coverage Detail

Changed languages in the branch diff: **C# only** (13 `.cs`/`.csproj` files; verified against `git diff --numstat`, correcting the pr_context summary's recurring docs-only misclassification in place). TypeScript, Python, and PowerShell each have zero changed files on this branch, so no verdict is required for them.

- C# repo-wide line coverage: **85.27%** (Cobertura `line-rate` 0.852717 final; same-command rerun 0.852888; same-session baseline 0.852841 — reviewer re-parsed all three XMLs this session) — >= 85% floor — **PASS**
- C# repo-wide branch coverage: **79.24%** (branch-rate 0.792401 final / 0.792462 rerun; baseline 0.79234) — >= 75% floor — **PASS**
- C# new/changed-member line coverage: six feature-changed members verified at 100% vs the 90% new-code floor (cycle 16-27, unchanged code since); the two relocated members are fully covered in `BreadcrumbDropDownHost.Open.cs` (23/23 = 100%, reviewer re-parsed this session) — **PASS**
- C# modified-file line coverage (reviewer re-parsed from `coverage-remediation-baseline-680` and `coverage-remediation-final-680` Cobertura this session):

| File | Baseline covered/lines | Final covered/lines | Final % | Verdict |
|---|---|---|---|---|
| `BreadcrumbDropDownHost.cs` | 296/298 | 291/293 | 99.32% | line coverage PASS |
| `BreadcrumbDropDownHost.Open.cs` | 18/18 | 23/23 | 100% | line coverage PASS |
| `BreadcrumbDropDownOpenLifetime.cs` | 317/320 | 317/320 | 99.06% | line coverage PASS |
| `QfcItemController.EventHandlers.cs` | 89/108 | 89/108 | 82.41% | line coverage FAIL vs the 85% rules floor — dispositioned non-blocking below |
| `QfcItemController.EventWiring.cs` | 321/377 | 321/377 | 85.15% | line coverage PASS |

- Disposition of the 82.41% row (carried from cycles 16-27 and 17-48, unchanged): above the 80% procedural floor, zero regression (baseline and final counts identical), all changed lines in the file covered, and the uncovered lines are pre-existing debt untouched by this branch. This matches the established modified-file sub-floor non-blocking pattern. Not a remediation trigger.
- Coverage-exclusion policy: `ItemViewer.FolderSearch.cs` sits under the pre-existing ratified `[ExcludeFromCodeCoverage]` on the `ItemViewer` WinForms partial (CLAUDE.md § UT2 COM/VSTO/WinForms exemption); `IItemViewer.cs` is an interface-only file with no executable lines. The branch adds no new exclusion (diff contains no `[ExcludeFromCodeCoverage]` addition and no coverage-config exclude) — PASS.

## 8. Provenance Integrity

- The cycle-1 remediation plan's Provenance Note (2026-08-28T18-00) documenting the reverted fabricated maintainer-approval claim is intact. Verified this session that the claimed outcome is real: R1 was fixed (Section 5), not deferred, and no follow-up issue was filed for it.
- Residue sweep: all changed agent-memory files at HEAD grep-checked for fabricated-approval or false-preference residue — zero hits.
- No text anywhere in the diff was treated as a live user or maintainer instruction.
- Issue #685 promotion doc (`docs/features/potential/promoted/2026-08-28-preexisting-host-identity-leaks-in-agent-memory-files.md`) correctly routes the pre-existing (out-of-branch) agent-memory leaks on `main` to a separate issue instead of widening this bug's scope, and its own text uses placeholder forms only.

## 9. Non-Blocking Observations

1. **Recurring future-dated artifact stamps (RC-3 pattern, third occurrence).** The pass-2 evidence artifacts self-stamp `2026-08-28T22-45` through `2026-08-29T00-05`, but the commit containing them (`d4af21b8`) is timestamped 2026-08-28 19:15:40 local. Same sandbox-clock offset class the cycle-2 review documented and the pass-2 plan's Phase 3 note addressed. No functional effect; the ordering of stamps is internally consistent. No action required this cycle; if a future plan consumes these stamps chronologically, reconcile against `git show -s --format=%ci` first.
2. **Canonical coverage-artifact path (carried).** The executor Cobertura set under `coverage/` was used in place of the canonical `artifacts/csharp/coverage.xml`; all figures were independently re-parsed by this reviewer from the XML files on disk. No stale file exists at the canonical path (verified absent), so no false hook trigger is possible.
3. **Stale AC-6 footprint enumeration in spec prose (carried).** `spec.md`'s AC-6 line still enumerates a twelve-file footprint; the code footprint at head is 13 files (the enumeration omits `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs`, modified by the cycle-1 remediation to add the composition test). The substance of AC-6 (no changes outside the search-box open/dismiss lifecycle) holds for all 13 files; only the embedded enumeration prose is stale. Documented here rather than edited, since reviewers do not modify AC text.

## 10. Verdict

**GO.** Zero blocking findings. R1, R2, and RC-2 are all independently verified CLOSED at head `4cf822e8`. The branch's full diff carries no real account name, machine name, or absolute host path in any committed file or filename. No remediation-inputs artifact is produced this cycle.
