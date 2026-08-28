# Policy Audit — efc-controller-surface-defects-464

- **Timestamp:** 2026-08-28T02-29 (UTC, `date -u` run immediately before this write)
- **Reviewer:** feature-review agent
- **Branch:** `bug/efc-controller-surface-defects-464` @ `7c9b02ee3c404862afeddcd216412ec1708599a3`
- **Base:** `origin/epic/quickfiler-bug-family-integration` @ `38f097898639b054428188c9c5e266e54972c259` (merge base equals base tip; branch is 0 behind / 16 ahead)
- **Scope:** full branch diff `origin/epic/quickfiler-bug-family-integration..HEAD` — 12 non-doc paths (8 modified/created C# files, 1 test csproj, 3 deleted `EfcViewer3.*` files) plus the feature documentation tree
- **Work mode:** `full-bug` (`issue.md:6`); AC source is `spec.md` only

## Rejected Scope Narrowing

None. The delegation prompt directed review of the full branch diff against the resolved epic base, which matches the scope invariant. No caller text attempted to narrow scope to a plan, task, phase, file subset, or language carve-out. The plan-base-drift addendum's `DIRECTIVE`-style guidance is executor handoff text, not reviewer scope narrowing.

## 1. Verdict Summary

| Policy area | Verdict |
|---|---|
| Formatting (CSharpier) | PASS |
| Linting / analyzers | PASS (evidence-verified, non-vacuous) |
| Type check / nullable | PASS (evidence-verified, non-vacuous) |
| Tests (MSTest/Moq/FluentAssertions) | PASS |
| Test purity (no sleeps, temp files, live Outlook, shown forms) | PASS |
| File-size gates | PASS |
| Ownership / sibling path constraints | PASS |
| Coverage — C# repo-wide | PASS |
| Coverage — C# canonical artifact presence | FAIL (procedural; dispositioned non-blocking, see §7) |
| Coverage — new-code 90% floor | FAIL (dispositioned non-blocking, see §7) |
| Evidence location compliance | PASS |
| Artifact hygiene (TRX sanitization, no scripts, no raw XML) | PASS |

**Blocking findings: 0.** Non-blocking findings: 6 (§9).

## 2. Toolchain Gates

1. **Formatting.** Executor evidence `evidence/qa-gates/toolchain-consecutive-pass.md` records `[P10-T1]` format (0 of 8 rewritten by SHA-256) and `[P10-T3]` check (1549 files, 0 unformatted, exit 0). Independently re-verified by this reviewer: `dotnet tool run csharpier check` over the eight owned files → `Checked 8 files in 3012ms.`, exit 0.
2. **Analyzers.** `[P10-T4]` `msbuild /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exit 0, 0 errors, 0 new diagnostic ids vs the Phase 0 baseline (`evidence/qa-gates/msbuild-analyzers.md`). Non-vacuity proven: 0 `Skipping target "CoreCompile"` lines against 36 `csc.exe` invocations. Not re-executed by this reviewer (a rebuild would mutate the executor worktree's build outputs); accepted on the strength of the recorded non-vacuity proof.
3. **Nullable / type check.** `[P10-T5]` CI-exact command (no `/p:Nullable=enable`, `/t:Rebuild`) exit 0, 0 new errors (`evidence/qa-gates/msbuild-nullable.md`), same non-vacuity proof. Accepted on the same basis as the analyzer gate.
4. **Tests.** `[P10-T6]` `vstest.console.exe` (vswhere-resolved, `/InIsolation`, repo runsettings): 1169/1169 passed, 0 failed. Independently re-verified by this reviewer against the committed TRX: `grep -c 'outcome="Passed"'` → 1169; `outcome="Failed"` → 0. Passed-count arithmetic reconciles: 1099 baseline + 26 from the mid-flight integration merge (`25924673`) + 44 added by this feature = 1169.
5. **Loop discipline.** The five Phase 10 stages ran in strictly increasing timestamp order with SHA-256 proof that no owned file changed between format and test; 0 restarts.

## 3. Test Policy Compliance

Verified directly against the four test files this feature wrote or extended:

- Frameworks: MSTest attributes, Moq mocks, FluentAssertions throughout; zero MSTest `Assert.*` calls in the three created files (grep-verified).
- Banned tokens: `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `BackgroundWorker`, `.Show(`, `.ShowDialog(`, `Application.Run`, `Outlook.Application`, `Path.GetTempFileName`, `Path.GetTempPath`, `new Random()` — zero hits across all four files (grep-verified).
- No temporary files, no live Outlook, no shown WinForms form. `EfcViewerTests.cs` constructs no `Form` (grep-verified; AC 999). The `AsyncExpansionPath_OnOffOn` test constructs an `ItemViewer` user control and a parentless `FastObjectListView` without a message pump or `Show()` — compliant.
- Timer determinism follows the 484 technique verbatim: `Timeout.Infinite` timer injected by reflection, disposal observed as `ObjectDisposedException` state, never a race.
- Pre-existing tests: the only pre-existing test file touched is `EfcFormControllerTests.cs`, whose diff is 317 added / **0 deleted** lines — no pre-existing `[TestMethod]` deleted or renamed, no assertion weakened (AC 1009 verified structurally).

## 4. File-Size Gates

| File | Base | Now | Gate | Verdict |
|---|---|---|---|---|
| `EfcFormController.cs` | 1073 | 1189 | <= 1193 (stricter addendum gate; spec's 1204 also satisfied) | PASS |
| `EfcItemController.cs` | 1170 | 1117 | strictly < 1170 | PASS |
| `QfcItemController.ViewerSetup.cs` | 499 | 499 | unchanged; 1 added / 1 deleted | PASS |
| `EfcItemController.CleanupTests.cs` (created) | — | 260 | < 500 | PASS |
| `EfcItemControllerTests.cs` (created) | — | 470 | < 500 | PASS |
| `EfcViewerTests.cs` (created) | — | 164 | < 500 | PASS |
| `EfcFormControllerTests.cs` (modified) | 168 | 485 | < 500 | PASS (15-line headroom; see finding NB-5) |

Counts measured with `awk 'END{print NR}'` (not `Measure-Object -Line`, which undercounts).

## 5. Ownership and Sibling-Path Constraints (all verified by this reviewer)

- `git diff --name-only` intersected with the forbidden set is **empty**: zero hits for `EfcSelectionGuard`, `BreadcrumbRowBuilder`, `KeyboardHandler`, `QfcFormViewer`, `QfcFormKeyHandler`, `KbdActions`, `QuickFiler/Interfaces/`, `QuickFiler/QuickFiler.csproj`.
- `QuickFiler.Test/QuickFiler.Test.csproj`: exactly 3 added / 0 deleted, all three `Controllers\Efc*` entries inserted contiguously after `Controllers\EfcHomeControllerTests.cs`. The pre-existing `Controllers\EfcFormControllerTests.cs` entry (base `:112`) is reused, which is why only three entries were added for four test files. Sibling-owned entries (`QfcCollectionController*` from 444, `QfcItemController.UiThreadDispatcherFixture*` from 493) untouched — the csproj diff contains no other hunk.
- `QfcItemController.ViewerSetup.cs`: exactly one changed line — `"–incognito "` (U+2013) → `"--incognito "` (two U+002D bytes, byte-verified with `cat -A` and an `\xe2\x80\x93` grep returning zero hits repo-file-wide).
- `EfcFormController.cs` `new WebView2BreadcrumbHost(` construction (feature #476 dependency): byte-identical — a 21-line window around it diffs empty against the base; it shifted from `:834` to `:918` solely by uniform offset from insertions above, and no diff hunk touches it.
- Deletions: `EfcViewer3.cs` / `.Designer.cs` / `.resx` deleted wholesale; they were orphans with no `QuickFiler.csproj` reference (which is why that project file has a zero-line diff). `EfcViewer.cs` −12 lines are the dead `SetController`/`_formController`/`EditFiltersMenuItem_Click` surface required by #466 A. All four pure-deletion paths are spec-required, not accidental.

## 6. Coverage Verification (mandatory, per changed language)

Languages with changed files in the branch diff: **C# only.** The diff contains zero `.ps1`/`.psm1`, zero `.py`, zero `.ts`/`.tsx` files, and no helper script of any language is retained under `evidence/` (find over the evidence tree returns only `.md` and `.trx`).

| Language | Changed files in diff | Repo-wide line coverage | Repo-wide branch coverage | Verdict |
|---|---|---|---|---|
| C# | 12 (8 changed + 1 csproj + 3 deleted) | 85.25% (54667/64124) | 79.19% (13001/16418) | PASS — line coverage 85.25% >= 85% floor and branch coverage 79.19% >= 75% floor |
| PowerShell | 0 changed `.ps1`/`.psm1` files on the branch | no gate evaluated — zero changed files | no branch metric exists for Pester | PASS — no PowerShell coverage obligation arises because the branch changes no PowerShell file |
| Python | 0 changed `.py` files on the branch | no gate evaluated — zero changed files | — | PASS — no Python coverage obligation arises because the branch changes no Python file |
| TypeScript | 0 changed `.ts`/`.tsx` files on the branch | no gate evaluated — zero changed files | — | PASS — no TypeScript coverage obligation arises because the branch changes no TypeScript file |

C# figures source: `evidence/qa-gates/coverage-final.md` transcribes the Cobertura root element verbatim. Reviewer corroboration: 54667/64124 = 0.852520 and 13001/16418 = 0.791875 — both match the transcribed `line-rate`/`branch-rate` exactly; the test-count delta (6789 = 6719 + 70 = 26 merge-imported + 44 feature-added) reconciles with the independent TRX count of 1169 for `QuickFiler.Test`.

**Baseline comparison caveat (endorsed as honest disclosure, not a defect):** baseline 70.32% (57714/82070) vs final 85.25% (54667/64124). The `lines-valid` denominators differ by 17,946 lines against a ~150-net-line production diff, so the +14.93-point difference is dominated by what was measured (the baseline aggregate run had 15 load-driven test-host timeouts; the instrument has known denominator instability), not by coverage this feature gained. The executor explicitly declined to claim the improvement (`evidence/qa-gates/coverage-delta.md`). The required assertion — post-change rate not lower than baseline — holds, and both delivered floors are cleared on the delivered measurement's own denominator.

## 7. Coverage Findings Requiring Disposition

### 7.1 Canonical artifact absence — FAIL, dispositioned non-blocking

No `artifacts/csharp/coverage.xml` was emitted, and both raw Cobertura files (baseline 17.9 MB, final 10.7 MB) were deliberately deleted after their root elements were transcribed. Under the artifact-absence rule this is a FAIL-level C# coverage-artifact finding: coverage verification for C# is mandatory and the reviewer could not re-parse per-file rates from a retained XML.

Disposition — non-blocking, for these reasons:
- The root-element figures are transcribed verbatim and are arithmetically self-consistent (both quotients recomputed by this reviewer match to six decimal places).
- The repo-wide figures clear every floor on record (85.25% >= 85 line; 79.19% >= 75 branch; also >= the 80% CLAUDE.md floor).
- The per-member table in `coverage-delta.md` is internally consistent with the delivered source spans and the delivered tests.
- The review model verifies from existing evidence and does not rerun coverage generation; regeneration is an executor/orchestrator obligation.

Corrective action (handoff, not remediation): the epic fan-in should regenerate and retain a canonical C# coverage artifact before the integration→main PR, per the standing pre-review artifact expectation.

### 7.2 New-code 90% floor — FAIL, dispositioned non-blocking

Of the eleven measured new/changed members in `EfcFormController.cs`, five reach 90% (four at 100%); six do not: `WithTrashRow` 81.8%, `BindSourceFolderRows` 0%, `ButtonCancelClickAsync`/`ButtonOkClickAsync`/`ButtonRefreshClickAsync` 81.8% each, `ButtonCreateClickAsync` 18.0%, `ButtonDeleteClickAsync` 88.9%. Additionally, new members in `EfcItemController.cs` (`ThrowInitializationFailure`, the RC1 guards) and `EfcViewer.cs` (`ClaimsAltChord`) are NOT MEASURED because both types carry **pre-existing** class-level `[ExcludeFromCodeCoverage]` attributes (`EfcItemController.cs:25`, `EfcViewer.cs:20`) that predate this feature and are unchanged by it (`evidence/qa-gates/exemption-audit.md`; independently confirmed — zero added `ExcludeFromCodeCoverage` lines in the `.cs`-scoped diff).

Disposition — non-blocking, for these reasons:
- Every sub-90 member's uncovered lines are itemised and each is COM-bound, dialog-bound, or a live-viewer/router path the headless test policy forbids driving (`MessageBox.Show`, `MAPIFolder` operations, `_formViewer.Close()`, a private member reachable only with a live form viewer and breadcrumb router). The boundary logic RC3 actually added (the `catch` sinks) is covered in every one of the five extracted members.
- `ButtonCreateClickAsync`'s 18% is dominated by pre-existing relocated code (the folder-creation body moved from the old `async void` handler into the extracted member), not by newly authored untested logic.
- The unmeasured members are behaviourally asserted by 26 passing tests; only the measurement is unavailable, and creating it would require removing pre-existing exemptions this feature does not own.
- `spec.md` (the sole AC source for this `full-bug` feature) asserts no per-member threshold; the CLAUDE.md-vs-rules threshold discrepancy is recorded as unresolved and is promoted as follow-up item 6 (`evidence/other/followup-promotions.md`).

No remediation-inputs artifact is produced: there is no code defect to remediate, the delivered floors that are unambiguous (repo-wide line and branch) both pass, and the residual threshold question is a policy decision owed to the maintainer via the promoted follow-up, not an executor work item.

## 8. Evidence Location and Artifact Hygiene

- **Evidence locations: PASS.** Every evidence file in the diff lives under `docs/features/active/efc-controller-surface-defects-464/evidence/<kind>/`. `git diff --name-only` contains zero paths under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. No `validate_evidence_locations.py` exists in this repository (checked); the equivalent scan was performed directly with the diff path list.
- **No retained scripts: PASS.** The evidence tree contains 87 `.md` and 22 `.trx` files and nothing else — zero `.ps1`/`.py`/`.pl`/`.sh`/`.xml`.
- **No raw coverage XML committed: PASS** (see §7.1 for the flip side of that fact).
- **TRX sanitization: PASS.** Across all 22 committed TRX files: `computerName="<host>"`, `runUser="<host>\<user>"`, and `codeBase`/`storage` paths rendered as `<repo-root>\...`. No account name, machine name, or `C:\Users\...` path appears anywhere in the evidence tree (grep-verified over the full tree).
- **EVIDENCE_LOCATION_OVERRIDE_REJECTED:** none — no caller instruction supplied a non-canonical evidence path.

## 9. Findings Register

| ID | Severity | Finding |
|---|---|---|
| NB-1 | Non-blocking (process) | Canonical C# coverage artifact absent; raw Cobertura XMLs deleted after transcription (§7.1). Fan-in should regenerate before the integration→main PR. |
| NB-2 | Non-blocking (coverage) | New-code 90% floor unmet for 6 of 11 measured members; several new members unmeasured under pre-existing class-level exemptions (§7.2). Resolution vehicle is promoted follow-up item 6 (threshold discrepancy). |
| NB-3 | Non-blocking (handoff) | Seven follow-up promotions are recorded but NOT created (`evidence/other/followup-promotions.md`), including the RC7 `EfcSelectionGuard` three-`=` `BannerPrefix` residual and its stale companion comment (now `EfcFormController.cs:318-320`). The promotion MCP tool was unavailable to the executor and writing potential docs would have breached the scope gates. The orchestrator must run all seven through the promotion lifecycle at fan-in; prose in a feature folder does not survive merge as a tracked obligation. |
| NB-4 | Non-blocking (docs) | Truncated comment at `EfcFormController.cs:1142`: `// #465 D (RC7): single classification owner. StartsWith, never Substring, over the` — the sentence ends mid-clause. Cosmetic; fix opportunistically at fan-in. |
| NB-5 | Non-blocking (hygiene) | `EfcFormControllerTests.cs` is at 485/500 lines and `QfcItemController.ViewerSetup.cs` at 499/500. Neither violates the limit; both have near-zero headroom for future additions. |
| NB-6 | Non-blocking (environment) | The Phase 0 aggregate coverage run recorded 15 load-driven `QfcItemController.*` failures (~60 s timeouts under instrumentation) owned by live sibling #489. All 15 passed in the final aggregate run (6789/6789) and the isolated baseline recorded `BASELINE_FAILED` as the empty set. Environment flakiness under load, not a defect of this feature. |

## 10. Policy Conflicts Observed

The CLAUDE.md coverage figures (80% repo-wide / 90% new code) and `.claude/rules/general-unit-test.md` figures (85% line / 75% branch, uniform tiers) disagree. The delivered repo-wide measurement clears both readings, so the conflict is not load-bearing for this review's verdicts; it is already promoted as follow-up item 6 rather than resolved here. No other policy conflict was encountered.
