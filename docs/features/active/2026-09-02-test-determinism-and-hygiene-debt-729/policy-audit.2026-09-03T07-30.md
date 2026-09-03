# Policy Audit — test-determinism-and-hygiene-debt (Issue #729)

Timestamp: 2026-09-03T07-30

- Feature folder: `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/`
- Work mode marker (from `issue.md`): `full-bug` — `spec.md` is the sole acceptance-criteria source; no `user-story.md` is expected or present.
- Item worktree audited: `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a7abf41824d58a80f`
- Head SHA: `e6c488bf46cec739bddcf4ee07ba070c45b85668`
- Base anchor supplied: `8be5a6aac3b5a82c86241fbbf989fd9118602c56`
- Base anchor independently re-derived: `git merge-base origin/main HEAD` -> `8be5a6aac3b5a82c86241fbbf989fd9118602c56`. The supplied anchor is current, not stale.
- Diff form used throughout: `git diff 8be5a6aa...HEAD` (three-dot, merge-base anchored).

## Verdict Summary

| # | Area | Verdict |
|---|---|---|
| 1 | Scope containment (production files, excluded trees) | PASS |
| 2 | C# toolchain order and completeness | PASS |
| 3 | C# coverage (repo-wide, modified file, new code) | PASS |
| 4 | General Unit Test Policy — determinism infrastructure | PASS |
| 5 | General Unit Test Policy — external deps / temp files | PASS |
| 6 | C# Unit Test Policy — MSTest / Moq / FluentAssertions | PASS |
| 7 | Coverage Exclusion Policy (no production file excluded) | PASS |
| 8 | File size limit (500 lines) | PASS |
| 9 | Evidence location compliance | PASS |
| 10 | Test file location / mirroring | PASS |
| 11 | Tonality policy | PASS |
| 12 | Documentation accuracy of plan/spec bookkeeping | PARTIAL (non-blocking) |

**Blocking findings: 0.**

## Rejected Scope Narrowing

None. The delegating prompt anchored the audit to `8be5a6aac3b5a82c86241fbbf989fd9118602c56`, which is the merge-base against `origin/main` and is therefore the legitimate full-branch scope. The prompt supplied a prior measurement of the footprint and explicitly instructed independent verification rather than acceptance on trust; that instruction was followed. No attempt to limit the audit to a plan, task, phase, or file subset was present, and no language with changed files was marked out of scope.

The prompt's statement that the five dirty `.claude/agent-memory/` paths "are NOT a scope violation" was treated as a claim to test, not an instruction to skip. It was verified: those paths are uncommitted working-tree state, do not appear in the anchored branch diff, and are recorded as pre-existing by `evidence/baseline/preexisting-worktree-state.2026-09-02T10-30.md`. They are outside the reviewed diff on the evidence, not by assertion.

## 1. Scope Containment — PASS

Command: `git diff --name-status 8be5a6aac3b5a82c86241fbbf989fd9118602c56...HEAD`

Observed footprint: 81 paths — 56 `A`, 17 `D`, 8 `M`.

| Check | Expected | Observed | Verdict |
|---|---|---|---|
| Non-test production source files modified | exactly 1 | 1 — `TaskMaster/AppGlobals/NonBlockingDelay.cs` | PASS |
| Deletions | 17 | 17 | PASS |
| Paths under `QuickFiler/` | 0 | 0 | PASS |
| Paths under `.claude/` | 0 | 0 | PASS |
| Paths under `.codex/` | 0 | 0 | PASS |
| Paths under `.agents/` | 0 | 0 | PASS |
| Paths under `config/` | 0 | 0 | PASS |
| Paths under `.github/workflows/` | 0 | 0 | PASS (no modified-workflow green-run obligation arises) |

Deletion safety was verified independently rather than inferred. Command:

```
grep -rn "Form1|Form2|Form3|ResourceTests" <worktree>/UtilitiesCS.Test <worktree>/SVGControl.Test
  --include=*.cs --include=*.csproj --include=*.resx --include=*.config
```

Output: empty. No surviving source file, project file, resource file, or app.config in either assembly references any of the 17 deleted paths.

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` retains exactly one `DASLFilterParser` reference, at line 271: `<Compile Include="OutlookObjects\Filter DASL\DASLFilterParserTests.cs" />`. The deleted orphan duplicate `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` has no remaining reference. `SVGControl.Test/SVGControl.Test.csproj` returns zero matches for `Form1` or `Form2`.

## 2. C# Toolchain — PASS

The four-stage loop is recorded as a single clean final pass in `evidence/qa-gates/toolchain-single-pass.2026-09-02T10-30.md` with `RewrittenFileCount: 0`, which is the condition that makes it the final pass rather than an intermediate one.

| Stage | Command | Recorded result | Independent verification |
|---|---|---|---|
| Format | `dotnet tool run csharpier format <7 scope-locked paths>` | EXIT_CODE 0, `RewrittenFileCount: 0` | not re-run (mutating) |
| Format check | `dotnet tool run csharpier check .` | EXIT_CODE 0 | **Re-run by this review**: `Checked 1571 files in 4541ms.` with no unformatted-file output and a clean exit. Independently confirmed. |
| Analyze | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0, 0 errors, 5 warnings (all `System.Reactive.PackagesConfigCheck`, none carrying a diagnostic identifier), equal to the P0-T9 baseline on all three measures; 75 `CoreCompile:` executions establish non-vacuity | not re-run (long-running build); evidence verified for command shape and non-vacuity |
| Type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | EXIT_CODE 0, zero `CS8632`, zero `CS86xx`, 55 `CoreCompile:` executions | not re-run; evidence verified |
| Test | `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput <feature evidence>\coverage-final.cobertura.xml` | EXIT_CODE 0, `PassedCount: 6955`, `FailedCount: 0`, 9 assemblies discovered | not re-run; evidence verified, and the Cobertura artifact it produced was parsed directly (section 3) |

Command-shape compliance was checked against CLAUDE.md rather than accepted from the artifact's own claim. Both msbuild invocations use `/t:Rebuild` (not the vacuous warm `/t:Build`) and neither adds `/p:Nullable=enable`. This matters here because CLAUDE.md records that a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project, which would make the gate unfalsifiable; the recorded `CoreCompile:` counts of 75 and 55 rule that out.

Verdict: PASS. One residual note — the analyzer artifact records a `CoreCompile:` count of 75 against a baseline of 64 and states the difference was not investigated. The non-vacuity property needs only a count greater than zero, so the gate is sound; the unexplained delta is noted for completeness and is not a finding.

## 3. Coverage — PASS

### Changed-language enumeration

Derived from the anchored branch diff, not from the PR context summary (see the deviation note below):

| Language | Changed files on branch | Coverage obligation |
|---|---|---|
| C# (`.cs`) | yes — 1 production, 5 test sources changed or created, 12 test sources deleted | mandatory |
| PowerShell (`.ps1`/`.psm1`) | zero | none |
| Python (`.py`) | zero | none |
| TypeScript (`.ts`/`.tsx`) | zero | none |

### C# coverage — PASS

Coverage artifact used: `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/qa-gates/coverage-final.cobertura.xml`, committed to the branch. The canonical path `artifacts/csharp/coverage.xml` is absent in this worktree; the committed feature-evidence Cobertura is the substituting artifact and is treated as artifact-present. The root element of each file was read before parsing to confirm the format is Cobertura `<coverage>` and not JaCoCo `<report>` or a Visual Studio format.

Root attributes read directly from the two XML documents by this review:

| Measure | Baseline (`evidence/baseline/coverage-baseline.cobertura.xml`) | Post-change | Floor | Verdict |
|---|---|---|---|---|
| C# repo-wide line coverage | `line-rate="0.85386"` (85.386%) | `line-rate="0.853836"` (85.3836%) | >= 85% | PASS |
| C# repo-wide branch coverage | `branch-rate="0.794589"` (79.4589%) | `branch-rate="0.794529"` (79.4529%) | >= 75% | PASS |
| `lines-covered` / `lines-valid` | 55138 / 64575 | 55139 / 64578 | — | — |
| `branches-covered` / `branches-valid` | 13187 / 16596 | 13186 / 16596 | — | — |

Modified-file C# coverage for the one production file changed. The `<class filename="TaskMaster\AppGlobals\NonBlockingDelay.cs">` element was located in both documents (line 187182 in each) and its `lines/line` children were counted directly, per the known trap that an `.//line` selector double-counts because method rows re-enumerate the same lines:

| Measure | Baseline | Post-change | Verdict |
|---|---|---|---|
| `line-rate` attribute | `1` | `1` | PASS |
| `branch-rate` attribute | `0.5` | `1` | PASS (improved) |
| `lines/line` children | 17, all `hits="1"` | 20, all `hits="1"` | PASS |
| Line coverage | 17/17 = 100% | 20/20 = 100% | >= 85% floor: PASS; no changed-line regression: PASS |

All three lines added by the 2-arg overload are covered. The single conditional line in the file (the `timer?.Dispose()` null-conditional) moved from `condition-coverage="50% (1/2)"` at baseline to `condition-coverage="100% (2/2)"` post-change, which is the source of the class-level branch-rate improvement from 0.5 to 1.

New-code C# coverage. No new production source file was added by this change, so the new-file tier has no member. The two new files are test sources (`UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs`, `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs`) and are correctly outside the coverage denominator under the General Unit Test Policy's permitted test-file exclusion.

### Adjudication of the repository-wide rate movement

The repository-wide line rate fell by 0.000024 (0.0024 percentage points) and the branch rate by 0.000060 (0.0060 percentage points). Both post-change figures independently clear their absolute floors, so no policy gate turns on the movement. The attribution recorded in `evidence/qa-gates/coverage-delta.2026-09-02T10-30.md` was verified against the two XML documents and is arithmetically closed: `55138 + 3 - 6 + 4 = 55139` and `64575 + 3 = 64578`. The only file whose `lines-valid` changed is `NonBlockingDelay.cs` (+3), and it is the sole production entry in the plan's write inventory. The only file with a negative covered-line movement is `UtilitiesCS/Interfaces/IWinForm/PropertyStore.cs` (-6), which this change never touches and whose `lines-valid` is 663 on both sides.

**On the `P6-T6` reshape.** The delegating prompt asked for an explicit position on the reshape of P6-T6 from an exact `>=` comparison into four clauses with a stated `0.0005` tolerance band. This review does not disagree with the reshape, for three reasons stated so a later reader can re-derive the position rather than take it on trust:

1. An exact `>=` on a cross-session repository-wide constant is not a sound gate in this repository. C# repo-wide coverage constants here are non-deterministic at per-file granularity across sessions, and gating on exact comparison converts measurement noise into a false FAIL. The four-clause form replaces the unsound comparison with three deterministic clauses (denominator attribution, changed-file no-regression, write-set attribution) plus one bounded tolerance clause, which is strictly more informative than the single comparison it replaced.
2. The tolerance clause is not load-bearing for policy compliance. Both post-change figures clear the absolute 85% and 75% floors on their own. If clause 4 were struck entirely, the coverage verdict in this audit would be unchanged. The reshape therefore did not lower any gate the repository actually enforces.
3. The reshape did not weaken the clause that could hide a real defect. Clause 3 fails if any file in the write inventory shows a covered-line decrease — a strictly stronger condition than a repo-wide `>=`, because a repo-wide aggregate can mask a per-file regression that clause 3 cannot.

Residual watch item: the `-6` movement in `PropertyStore.cs` is attributed but not root-caused. The artifact states this limitation explicitly rather than claiming a cause it did not establish, which is the correct disposition; separating run-to-run variance from a consequence of the `[DoNotParallelize]` scheduling change would need at least one further full-suite coverage run. Recorded as a watch item, not a finding.

### Deviation: PR context artifacts are stale

`artifacts/pr_context.summary.txt` is a tracked file in this repository and the copy present on this branch describes a different feature entirely:

```
Head ref (resolved): bug/claude-md-cites-ciyml-for-moved-toolchain-commands-564 @ fafe3d4d1f5a3dfcd2c44d21245d085e4156faea
Range: 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1..fafe3d4d1f5a3dfcd2c44d21245d085e4156faea
```

`git ls-files --error-unmatch artifacts/pr_context.summary.txt` confirms it is tracked. It was **not** regenerated: regenerating over a tracked file belonging to another feature would introduce an unrelated modification into this branch's working tree, which this review is not permitted to make. Scope and evidence were derived instead from the merge-base-anchored `git diff` and from the committed feature evidence, both of which are authoritative under the Scope Invariant. The deviation is recorded here rather than silently absorbed.

The downstream language derivation was simulated to confirm the consequence is understood rather than assumed:

```
pwsh -NoProfile -Command ". '.claude/hooks/validate-feature-review-coverage.ps1';
  $s = Get-ChangedLanguageSet -Lines (Get-Content artifacts/pr_context.summary.txt);
  'COUNT=' + $s.Count"
-> COUNT=0
```

The stale summary yields an empty changed-language set. This audit therefore records coverage verdicts derived from the branch diff, which is the correct source, and relies on the summary for no conclusion.

### Non-C# language rows

- **PowerShell (Pester) coverage: PASS.** Zero changed `.ps1` or `.psm1` files in the anchored branch diff, so no PowerShell coverage obligation arises from this change and the verdict is vacuously satisfied. Pester measures command and line coverage only and has no branch figure to evaluate, so no branch threshold applies to it.
- **Python coverage: PASS.** Zero changed `.py` files in the anchored branch diff.
- **TypeScript coverage: PASS.** Zero changed `.ts` or `.tsx` files in the anchored branch diff; no `coverage/` directory exists in this worktree.

## 4. Determinism Infrastructure — PASS

`.claude/rules/general-unit-test.md` § Determinism Infrastructure requires a controllable clock, bans real wall-clock waits and `Thread.Sleep`/`Task.Delay` in test code, and requires async tests to use the framework's fake-timer facility.

| Check | Method | Result |
|---|---|---|
| `Stopwatch` removed from `NonBlockingDelayTests.cs` | full file read | zero occurrences |
| `System.Diagnostics` using directive removed | full file read | absent; the using set is `System`, `System.Threading`, `System.Threading.Tasks`, `FluentAssertions`, `Microsoft.Extensions.Time.Testing`, `Microsoft.VisualStudio.TestTools.UnitTesting` |
| `Thread.Sleep` / `Task.Delay` in changed test files | full file read of all five changed or created test sources | zero occurrences |
| Controllable clock present | `FakeTimeProvider` injected in 2 of 3 tests | present |
| Production seam exists | `WaitAsync(TimeSpan, TimeProvider)` | present |

The defect the issue named at `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs:38-39` — `var stopwatch = Stopwatch.StartNew();` against a real 30 ms interval — is removed and replaced by a strictly stronger assertion: the task is asserted **not completed** before `fakeTimeProvider.Advance(interval)` and `RanToCompletion` after it. The pre-advance non-completion assertion proves the task cannot complete early, which the prior elapsed-time check could not.

One advisory deviation is recorded in the code review as CR-2: `WaitAsync_SingleArgumentOverload_CompletesOnSystemTimeProvider` awaits a real `TimeProvider.System` timer rather than a fake one. It is required by AC6 to cover the 1-arg overload, uses `TimeSpan.Zero` as the due time, asserts completion rather than duration, and is bounded by `[Timeout(5000)]`. It is advisory rather than a violation of the ban on wall-clock waits, because no elapsed real time is waited on or asserted.

## 5. External Dependencies and Temporary Files — PASS

| Check | Method | Result |
|---|---|---|
| Temporary files created in tests | full read of all five changed or created test sources | none |
| Filesystem access in tests | full read | none |
| Network or external process access in tests | full read | none |
| Live UI construction in a unit test | structural guards installed in both assemblies; the `Form1 frm = new Form1();` site deleted with its file | eliminated |
| Reflection guards instantiate any type | full read of both guard files | no — `Assembly.GetTypes()` and `ex.Types` metadata only; nothing is constructed |

Both guard classes carry a `ReflectionTypeLoadException` fallback returning `ex.Types.Where(candidate => candidate != null).ToArray()`, so a single unresolvable dependency in a large test assembly degrades the guard to the loadable subset rather than turning it permanently red for a reason unrelated to what it measures. Both fallbacks were read and are present and identical.

## 6. C# Unit Test Policy — PASS

| Requirement | Observed |
|---|---|
| MSTest framework | `[TestClass]`, `[TestMethod]`, `[Timeout]`, `[DoNotParallelize]` from `Microsoft.VisualStudio.TestTools.UnitTesting` in all changed and created test files |
| FluentAssertions for assertions | `.Should().BeFalse()`, `.Should().BeTrue()`, `.Should().Be(...)`, `.Should().BeNull(...)`, `.Should().BeEmpty(...)` throughout; no MSTest `Assert` API introduced |
| Moq for mocks and stubs | not required by any changed test; no mock is needed for a `TimeProvider` seam or for metadata reflection. Not a violation — the policy prescribes Moq where mocking is used, not that mocking must be used |
| Arrange-Act-Assert | all three `NonBlockingDelayTests` methods and both guard methods carry explicit `// Arrange`, `// Act`, `// Assert` comments |
| Documented intent | every test method carries an XML doc comment stating scenario and expected outcome |
| Clear failure messages | every FluentAssertions call supplies a `because` argument; the guard's `because` enumerates the offending type names |

## 7. Coverage Exclusion Policy — PASS

No `[ExcludeFromCodeCoverage]` attribute was added by this change (zero occurrences in the anchored diff). No `coverage.config` assembly-level exclude was added or modified. No production source path was excluded from measurement. `NonBlockingDelay.cs` remains in the denominator and is measured at 100%.

## 8. File Size Limit — PASS

Independently measured with `awk 'END{print NR}'` rather than `Measure-Object -Line`, which is known to undercount in this repository:

| File | Lines | Limit | Verdict |
|---|---|---|---|
| `TaskMaster/AppGlobals/NonBlockingDelay.cs` | 91 | 500 | PASS |
| `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` | 129 | 500 | PASS |
| `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` | 56 | 500 | PASS |
| `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` | 56 | 500 | PASS |
| `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | 125 | 500 | PASS |
| `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | 276 | 500 | PASS |

All six counts match those recorded in `evidence/qa-gates/file-size-audit.2026-09-02T10-30.md` exactly. Test files count toward the limit and are included above.

## 9. Evidence Location Compliance — PASS

Command: `git diff --name-only 8be5a6aac3b5a82c86241fbbf989fd9118602c56...HEAD -- artifacts/`

Output: empty. No file was written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` by this branch.

All 31 evidence artifacts sit under the canonical `<FEATURE>/evidence/<kind>/` layout, in four kind directories: `baseline/` (15), `qa-gates/` (17 including the post-change Cobertura), `regression-testing/` (10), `other/` (6). No non-canonical evidence path appears in the diff.

Host-path hygiene was checked across the whole feature folder rather than against a file list, because a sanitization step scoped to an explicit file list has previously missed a path in an earlier-phase document:

```
grep -rniE "DANMOI|DESKTOP-|\\Users\\[A-Za-z]|DanMoisan" <feature folder> --include=*.md
```

Output: empty. Zero absolute host paths, account names, or machine names in any feature-folder markdown.

## 10. Test File Location — PASS

`.claude/rules/general-unit-test.md` requires tests to mirror the production source structure and forbids colocation in the production tree. TaskMaster realises this as sibling `<Project>.Test/` assemblies rather than a repo-root `tests/` tree.

- `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` mirrors `TaskMaster/AppGlobals/NonBlockingDelay.cs` exactly. PASS.
- `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` and `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` sit at their test-assembly roots. These are assembly-level structural guards with no production counterpart to mirror, and they match the existing in-repo precedent `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs`. PASS.
- No test file was created in or moved into a production source tree. PASS.

## 11. Tonality — PASS

The changed source comments, the spec, and all 31 evidence artifacts were read or sampled. The register is factual and neutral throughout. No humor, hyperbole, decorative metaphor, or emoji was found. Claims are matched to evidence: `evidence/qa-gates/coverage-delta.2026-09-02T10-30.md` carries an explicit "What this artifact does not claim" section disclaiming a root cause it did not establish, and `evidence/regression-testing/fail-before-exception.2026-09-02T10-30.md` states plainly that no deterministic red run is producible and that none is claimed. Both are correct applications of the evidence-first wording rule.

## 12. Documentation Accuracy — PARTIAL (non-blocking)

Four bookkeeping inaccuracies were found. None affects delivered behaviour, none is a code defect, and none blocks. They are recorded because they would mislead a later reader of the plan or spec.

**DOC-1 — `spec.md` "Write Set" heading overstates its own contents.**
Location: `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/spec.md` line 309.
Text: "Every file this plan's diff creates, modifies, or deletes, reproduced from the plan's 'Complete file-write inventory' section. Nothing else belongs in this list".
The list enumerates 27 entries (1 production source, 5 test sources, 4 project files and manifests, 17 deletions). The anchored diff carries 81 paths. The feature documentation, all 31 evidence artifacts, and the `#743` promotion record are absent from the list. The section is accurate as a *code* write set and inaccurate as the "every file" claim its own heading makes.
Disposition: non-blocking. The omitted paths are documentation and evidence, all of which are independently and correctly enumerated in `evidence/other/changed-file-inventory.2026-09-02T10-30.md`.

**DOC-2 — two stale claims that the `#743` promotion record is staged by P8-T22.**
Locations: `plan.2026-09-02T08-59.md` line 112 ("It is real output of this work rather than incidental drift, so P8-T22 stages and commits it") and line 666 (P8-T22 prescribing `git add ... docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md`).
Verification: `git log --oneline --diff-filter=A -- docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md` returns `3fc7fafe docs(729): prepare feature folder and atomic plan for test-determinism-and-hygiene-debt`, the branch's first commit. The record was already tracked and committed nine commits before P8-T22 ran, so that `git add` was a no-op for this path.
Disposition: non-blocking. The file is present, committed, and correctly cited by `spec.md` line 87 and by AC16. Only the plan's description of how it got there is stale. `plan.2026-09-02T08-59.md` line 920 (S9) also classifies it as untracked, but S9 explicitly labels itself a planning-time reconstruction with recorded uncertainty, so it is not counted as a third stale claim.

**DOC-3 — P7-T9 records its verification results in no artifact.**
Location: `plan.2026-09-02T08-59.md` line 639.
P7-T9 appends a correction note to the research artifact and states three `Select-String` acceptance conditions, but writes no evidence file. The only on-disk trace that its acceptance was evaluated is the plan checkbox itself, which is self-attesting.
This review verified the three conditions independently rather than accepting the checkbox:
- `grep -c "Correction (2026-09-03) — §1.4 zero-due-time premise falsified by execution" research-729.2026-09-02T09-30.md` -> `1`
- `grep -c "this is the single point in the plan that must be confirmed by an actual test run" research-729.2026-09-02T09-30.md` -> `1`, so the no-rewrite witness is intact and no existing line was rewritten
- The appended note contains no drive-root or host path, confirmed by the feature-folder-wide host-path scan in section 9, which returned empty
Disposition: non-blocking. The task's output is correct; only its auditability is thin. Verified here instead.

**DOC-4 — AC18 and AC16 are internally inconsistent in wording.**
Location: `spec.md` line 278 (AC18) and line 276 (AC16).
AC18 states that every modified file other than the one production source "belongs to a test project (source, project file, or packages.config) or to this feature's documentation and evidence folder." `docs/features/potential/promoted/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam.md` is added by this branch and sits outside the feature folder, so it satisfies neither literal branch of AC18's disjunction. The same spec's AC16 explicitly requires that file to exist at that exact path.
Disposition: non-blocking, and AC18 is scored PASS rather than PARTIAL in the feature audit. The substantive clause of AC18 — exactly one non-test production source file modified — is satisfied. The file AC18's wording fails to cover is one the same spec mandates in AC16, so the delivery is in-spec and only the AC prose is imprecise. Recording a FAIL here would penalise the executor for a spec-authoring gap while the delivered artifact is exactly what the spec asked for.

## Assumptions Recorded

1. The C# coverage artifact obligation is treated as satisfied by the committed `<FEATURE>/evidence/qa-gates/coverage-final.cobertura.xml` in the absence of the canonical `artifacts/csharp/coverage.xml`. Both documents were parsed directly by this review, so the substitution rests on read evidence rather than on an assertion that a file exists somewhere.
2. The analyzer, nullable, and full-suite test gates were verified from committed evidence rather than re-executed. Re-running a three-rebuild plus 6955-test loop is outside the check-only posture this review is required to keep, and evidence verification from existing artifacts is the prescribed model. The format gate was cheap enough to re-run and was re-run.
3. The governing coverage floors are taken from `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` (line >= 85%, branch >= 75%, uniform across T1-T4). CLAUDE.md § UT2 still states a repository-wide floor of 80% with 90% for new modules; that documented conflict remains unreconciled repo-wide and is out of scope for this item. The delivered figures clear both readings on every measure, so the conflict is not decisive here.
