# Policy Audit — ribbon-engine-toggle-defects (Issue #735)

- Timestamp: 2026-09-03T06-19 (UTC)
- Component: `TaskMaster/Ribbon/` (Explorer CustomUI document, `RibbonController.Intelligence`, `EngineToggleStateCoordinator`) and `TaskMaster.Test/Ribbon/`
- Branch: `bug/ribbon-engine-toggle-defects-735`
- Head: `30e66833e73267327a18e58228f493e8c8e3a4dd`
- Work Mode (from `issue.md`): `full-bug` — acceptance-criteria source is `spec.md` only
- Reviewer: feature-review agent

> Template provenance deviation: `.claude/skills/policy-audit-template-usage/SKILL.md` requires the
> template be resolved through `mcp__drm-copilot__resolve_policy_audit_template_asset` and validated
> through `mcp__drm-copilot__validate_orchestration_artifacts`. Neither MCP tool is exposed in this
> session. This artifact is hand-authored preserving all thirteen canonical major headings rather
> than emitted as BLOCKED. The same unavailability applies to the validation step.

---

## Scope Resolution (authoritative)

The audit scope is the full branch diff against the resolved base branch.

| Item | Value | How resolved |
|---|---|---|
| Base branch tip | `b13d5b7b1a6dd0aa79d51d48a7156ee67377f9d0` | `git rev-parse origin/main` |
| Merge base | `b13d5b7b1a6dd0aa79d51d48a7156ee67377f9d0` | `git merge-base HEAD origin/main` |
| Head | `30e66833e73267327a18e58228f493e8c8e3a4dd` | `git rev-parse HEAD` |
| Diff command used | `git -C <worktree> diff b13d5b7b HEAD` | — |

Because the merge base equals the `origin/main` tip, the two-dot and three-dot forms against
`b13d5b7b` are identical. This is the canonical PR scope.

**Anchor correction applied.** The delegating prompt initially supplied
`git diff a679cd082819af6788cd0fb35f4366786fab87e3...HEAD` and then superseded it. That instruction
was defective: `a679cd08` is an ancestor of `HEAD` after the second `origin/main` merge
(`30e66833`), so the three-dot form degenerates to two-dot and pulls in everything `HEAD` gained
from `main` in the interim. Measured difference: **184 changed paths** under the superseded anchor
versus **78** under the correct anchor, including **18** paths under `.github/`,
`Directory.Build.props`, `scripts/vscode/` and `tests/scripts/vscode/` that belong to sibling items
#730 and #733.

The caller's factual claim was independently verified rather than accepted:
`git diff --name-only b13d5b7b HEAD` returns **zero** paths matching
`^(\.github/|Directory\.Build\.props|scripts/vscode/|tests/scripts/vscode/|artifacts/pr_context|docs/features/active/2026-09-02-(ci-build-infra-debt-730|coverage-cobertura))`.
The correction restores the canonical base rather than narrowing scope, so no finding is withheld as
a result of it.

## Rejected Scope Narrowing

None. No caller instruction attempted to limit the audit to a plan, task, phase, or file subset, or
to declare any language's coverage exempt from evaluation. The one caller correction in this session
increased accuracy by restoring the true merge base; it is recorded above rather than here.

For completeness, `plan.2026-09-02T12-04.md` contains per-task scope fences (write-set clauses,
`[expect-fail]` markers, prohibited-path lists). These are executor guidance, not reviewer scope
instructions, and were not treated as narrowing.

## Evidence Location Compliance

`validate_evidence_locations.py` does not exist in this repository (searched the full worktree). The
equivalent check was performed directly against the branch diff.

| Prohibited prefix | Paths found in `b13d5b7b..HEAD` |
|---|---|
| `artifacts/baselines/` | 0 |
| `artifacts/qa/` | 0 |
| `artifacts/evidence/` | 0 |
| `artifacts/coverage/` | 0 |

All 55 evidence files are under
`docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/<kind>/` with kinds
`baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/`, `other/`. **PASS.**

`EVIDENCE_LOCATION_OVERRIDE_REJECTED`: none required — no delegation instruction specified a
non-canonical evidence path.

---

## Executive Summary

**Verdict: PASS. Zero blocking findings.**

The change delivers all three consolidated defects in issue #735: five dead CustomUI callback
bindings repaired (four renames plus one element deletion), the unguarded globals dereference in
`ClearSpamManagerAsync` replaced by an extracted and fully tested `SpamManagerResetGate`, and the
toggle-state last-writer race closed by a monotonic-ticket compare-and-apply cache. Two authorized
in-scope extensions (CR-2 canceled-prime completion, CR-3 engines-unavailable guard test) are also
delivered.

Independently reproduced by this review rather than accepted from evidence:

- The pre-fix CustomUI document bound **84** distinct callback names of which exactly **5** resolved
  to no public instance method on `RibbonViewer`; the post-fix document binds **83** with **0**
  unresolved. Reproduced by parsing both documents and reflecting the method set from source.
- **No** `[ExcludeFromCodeCoverage]` attribute is added or removed anywhere in the diff. The
  `RibbonController` type-level exemption at `TaskMaster/Ribbon/RibbonController.cs:36` is present on
  `b13d5b7b` unchanged.
- Red-before-green is genuine for Findings 1 and 3: `p1-t2.trx` shows 2/2 failed pre-fix and
  `p1-t7.trx` 2/2 passed post-fix; `p3-t5.trx` shows exactly the three defect-reproduction tests
  failed pre-fix and `p3-t11.trx` 6/6 passed post-fix.
- Per-file coverage recomputed from the committed Cobertura documents; every threshold is met with
  margin.
- No commit on this branch carries the local account or machine token in the feature folder.

Twenty-four of twenty-five acceptance criteria are satisfied. The single open criterion (F2-AC8)
requires an operator with a live Outlook host and cannot be satisfied by any code change.

Thirteen non-blocking observations are recorded in `## 8. Gaps and Exceptions` and, with
reproduction detail, in the companion code-review artifact.

---

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence — order-independent | PASS | Every new fixture constructs its own harness or gate per test method. No static or shared mutable state; no `[AssemblyInitialize]`/`[ClassInitialize]` added. |
| Isolation — one unit per test | PASS | 27 new tests each target one method or one interleaving. |
| Fast execution | PASS | Ribbon-scoped run: 134 tests. Full first-party suite: 6982 tests in 31.49 s (`qa-gates/vstest-coverage-run`). |
| Determinism | PASS | Every asynchronous outcome driven by a held `TaskCompletionSource<bool>`. Direct scan of the four new/changed test files found zero occurrences of `Thread.Sleep`, `Task.Delay`, `Task.Run`, `DateTime.Now`, `DateTime.UtcNow`, `.Wait()`, `.Result`, `Path.GetTempPath`. |
| Readability | PASS | Arrange/Act/Assert comments in every test; each FluentAssertions call carries a `because` string. |
| Line coverage floor 85% | PASS | Repo-wide 85.41%. Per-file figures in section 5. |
| Branch coverage floor 75% | PASS | Repo-wide 79.50%. Per-file figures in section 5. |
| No regression on changed lines | PASS | `EngineToggleStateCoordinator.cs` 98.52% -> 100% line, 93.33% -> 97.37% branch. |
| No production file excluded from measurement | PASS for this change | No exclusion added or widened. The pre-existing `RibbonController` type-level exemption is unchanged; see section 8, NB-13. |
| Test file location mirrors production tree | PASS | `TaskMaster/Ribbon/X.cs` -> `TaskMaster.Test/Ribbon/XTests.cs` for both new types. |
| No temporary files in tests | PASS | Zero filesystem writes in the new fixtures. |
| No external dependencies | PASS | `Mock<IAppItemEngines>(MockBehavior.Strict)`, `Mock<IAppAutoFileObjects>`, `Mock<IApplicationGlobals>`. The one concrete boundary type, `ManagerAsyncLazy`, is constructed over a mocked globals object; its constructor assigns an `AsyncLazy` without running the factory. |
| Scenario completeness | PASS | Positive, negative (three null states), boundary (equal ticket rejected), error propagation (fault escapes unwrapped) and concurrency (three interleavings) are all covered. |
| Banned APIs in test code | PASS | See determinism row. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | The race fix is a ticket plus a compare-and-swap loop; the gate is a three-line decision. No new abstraction layer. |
| Reusability | PASS | The versioned cache is extracted as its own type with a single consumer rather than duplicated. |
| Separation of concerns | PASS | Both new types are host-neutral: no `Microsoft.Office.*`, no `System.Windows.Forms`, no logger field, no COM. Presentation stays behind injected delegates in the exempt shim. |
| Fail fast and explicitly | PASS | `ArgumentNullException` for each gate dependency and for a null reset delegate, checked before any accessor runs. `InvalidOperationException` on the direct toggle path when engines are unavailable. |
| No silent broad catch | PASS | `SpamManagerResetGate` contains zero `catch` clauses by construction; `EngineToggleStateCoordinator` retains exactly one, at the click boundary. |
| File size limit 500 lines | PASS | Largest changed file is `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` at 496 lines. Counts verified with `awk 'END{print NR}'`, not `Measure-Object -Line`. See section 8, NB-5. |
| Toolchain loop order and single clean pass | PASS | Format (P4-T1/P4-T4) -> analyzers (P4-T5) -> nullable type-check (P4-T6) -> tests with coverage (P4-T7). All exit 0. |
| No policy documents modified | PASS | Zero paths under `.claude/rules/` or `.github/instructions/` in the diff. |
| Dependencies unchanged | PASS | No `packages.config` or `PackageReference` edit. The two `.csproj` edits add three and two `<Compile Include>` entries respectively and nothing else. |
| Public API compatibility | PASS | Both new types are `internal sealed`. No public signature changed. The user-visible surface change is the intended one: four check boxes begin working and one non-functional button is removed. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `dotnet tool run csharpier check .` exit 0, 1576 files checked, no unformatted path (`qa-gates/csharpier-check-final`). Baseline was 1571 files; the +5 are exactly this change's five new C# files. |
| CSharpier applied to the CustomUI XML | PASS | `RibbonExplorer.xml` is not in `.csharpierignore` and is attribute-per-line wrapped; the format step ran after the edit (`qa-gates/csharpier-xml-format`). |
| .NET analyzers with `/t:Rebuild` | PASS | Exit 0; 5 warnings, 0 errors, equal to baseline. The 5 are the System.Reactive `packages.config` advisory, one per consuming project; none carries an analyzer rule ID. |
| Nullable type-check with `/t:Rebuild`, no `/p:Nullable=enable` | PASS | Exit 0; 5 warnings, 0 errors, equal to baseline. `/p:Nullable=enable` correctly omitted, matching `.github/workflows/ci.yml`. |
| `/t:Rebuild` actually rebuilt | PASS | Elapsed 12.75 s and 14.96 s against 1.5–5 s for the incremental `/t:Build` gates earlier in the plan. |
| Strong contracts, explicit types at boundaries | PASS | `internal SpamManagerResetGate(Func<IAppAutoFileObjects>, Func<IAppItemEngines>, Action<string>)`; `internal Task RunAsync(Func<ManagerAsyncLazy, IAppItemEngines, Task>)`. |
| Async and resource safety | PASS | `ConfigureAwait(false)` on every await in the coordinator. `RunAsync` returns the reset task directly, so a fault propagates without re-wrapping. |
| XML documentation on non-obvious behavior | PASS | Both new types carry full `<summary>`/`<remarks>` explaining the freshness invariant, the reference-type comparand choice, and the deliberate absence of the coverage attribute. |
| Naming conventions | PASS | `PascalCase` types and members, `_camelCase` private fields. |
| Language features vs target framework | PASS | `??=` and the null-forgiving `!` require C# 8. `TaskMaster.csproj` sets `<LangVersion>preview</LangVersion>`, `TaskMaster.Test.csproj` sets `<LangVersion>latest</LangVersion>`; both target `v4.8.1`. Both builds are green. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting` in all four new/changed test files. No xUnit or NUnit introduced. |
| Moq for mocking | PASS | `Mock<T>`, `SetupSequence`, `MockBehavior.Strict`, `Verify(..., Times.Never())`. |
| FluentAssertions for assertions | PASS | `Should().BeTrue/BeEmpty/ContainSingle/BeSameAs/NotBeSameAs/ThrowAsync/BeAssignableTo/WithParameterName/Equal`. No bare MSTest `Assert` in the new code. |
| Test-run command | PASS | `Invoke-MSTestWithCoverage.ps1` wraps `vstest.console.exe` with `/EnableCodeCoverage` and applies `/TestCaseFilter:TestCategory!=LiveOutlook`, so no external Outlook process is started. |
| Partial-class test split follows in-repo precedent | PASS | `RibbonControllerTests.cs` / `RibbonControllerTests.Engines.cs` is the existing precedent in the same directory. |

## 5. Test Coverage Detail

Coverage is verified by inspecting pre-existing artifacts, not by rerunning generation.

### Changed languages in the branch diff

| Language | Changed files in `b13d5b7b..HEAD` |
|---|---|
| C# | 8 `.cs`, plus 1 `.xml` embedded resource and 2 `.csproj` |
| TypeScript | 0 |
| Python | 0 |
| PowerShell | 0 |

Only C# has changed files. TypeScript, Python and PowerShell have zero changed files on this branch,
so no verdict is owed for them.

### Coverage artifact location

The canonical path `artifacts/csharp/coverage.xml` is absent. The Cobertura documents committed under
`<FEATURE>/evidence/` are used instead, which is the accepted substitution for this repository when
the executor writes coverage into the canonical feature-evidence tree:

- Baseline: `evidence/baseline/coverage-baseline.2026-09-02T12-04.cobertura.xml` (root element `<coverage>`, Cobertura)
- Final: `evidence/qa-gates/coverage-final.2026-09-02T12-04.cobertura.xml` (root element `<coverage>`, Cobertura)

Both were produced in the same session by the same script with the same Koverage post-processing, so
they are comparable on equal terms. All figures below were recomputed by this review directly from
those two documents, using `lines/line` child elements rather than a descendant axis, to avoid the
double-counting that a `.//line` selector produces under a `<class>` element.

### C# coverage verdicts

| Scope | Line | Branch | Floor | Verdict |
|---|---|---|---|---|
| C# repo-wide coverage, final | **85.41%** (55225/64658) | **79.50%** (13219/16628) | 85% / 75% | **PASS** |
| C# repo-wide coverage, baseline | 85.39% (55141/64578) | 79.46% (13188/16596) | — | improved |
| C# new file coverage — `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` | **94.87%** (37/39) | **80.00%** (8/10) | 85% / 75%, plus 90% new-module | **PASS** |
| C# new file coverage — `TaskMaster/Ribbon/SpamManagerResetGate.cs` | **100%** (33/33) | **100%** (14/14) | 85% / 75%, plus 90% new-module | **PASS** |
| C# modified file coverage — `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | **100%** (143/143), was 98.52% | **97.37%** (37/38), was 93.33% | 85% / 75%, no regression | **PASS** |
| C# modified file coverage — `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | no measurable lines in either document | no measurable lines in either document | — | recorded below |

`RibbonController.Intelligence.cs` is a partial of `RibbonController`, which carries a type-level
`[ExcludeFromCodeCoverage]` at `TaskMaster/Ribbon/RibbonController.cs:36`. That attribute is present
unchanged on `b13d5b7b`; this change neither adds nor widens it. The file contributes zero lines to
the denominator in both the baseline and the final document, so no regression is arithmetically
possible and no coverage credit is claimed for it. The residual lines are validated by the recorded
operator procedure in `evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md`
instead. This row is a measurement property of a pre-existing ratified exemption, not a shortfall
introduced by this change; the C# language verdict above stands at PASS on the repo-wide, new-file
and modified-file rows that are measurable.

### C# uncovered residual, enumerated

`EngineTogglePressedStateCache.cs` lines **109** and **127** are uncovered, and the same two sites
are the 50% partial branches at lines **104** and **117**. Both are compare-and-swap retry paths:
line 109 is the `continue` after a lost `TryAdd`, line 127 is the loop-back after a lost `TryUpdate`.
Reaching either requires a genuine thread race inside the CAS window, which no deterministic test may
manufacture under the repository determinism rules. The file clears both floors with the residual in
place, so no exemption is sought.

`EngineToggleStateCoordinator.cs` line **362** is a 50% partial branch on the pre-existing
`RenderEngineName` ternary; the file's branch figure of 97.37% is above the floor.

### Threshold basis

`CLAUDE.md` states an 80% repo-wide floor and a 90% new-module target; `.claude/rules/quality-tiers.md`
and `.claude/rules/general-unit-test.md` state a uniform 85% line and 75% branch floor across T1–T4.
That conflict is unreconciled in the repository. Every figure above is reported against the stricter
uniform rule and additionally against the 90% new-module target, and clears both, so the conflict has
no effect on any verdict here.

## 6. Test Execution Metrics

| Run | Task | Total | Passed | Failed | Purpose |
|---|---|---|---|---|---|
| `regression-testing/p1-t2/p1-t2.trx` | P1-T2 | 2 | 0 | **2** | Finding 1 red-before-green (expected failure, exit 1) |
| `regression-testing/p1-t7/p1-t7.trx` | P1-T7 | 2 | 2 | 0 | Finding 1 green after fix |
| `regression-testing/p1-t8/p1-t8.trx` | P1-T8 | 109 | 109 | 0 | Ribbon regression after Finding 1 |
| `regression-testing/p2-t8/p2-t8.trx` | P2-T8 | 9 | 9 | 0 | Finding 2 gate fixture |
| `regression-testing/p3-t5/p3-t5.trx` | P3-T5 | 6 | 3 | **3** | Finding 3 red-before-green; the three failures are exactly the prime-after-toggle race, the toggle-versus-toggle race, and the canceled-prime logging case |
| `regression-testing/p3-t11/p3-t11.trx` | P3-T11 | 6 | 6 | 0 | Finding 3 green after fix |
| `regression-testing/p3-t12/p3-t12.trx` | P3-T12 | 24 | 24 | 0 | Coordinator fixture regression |
| `qa-gates/p4-t3/p4-t3.trx` | P4-T3 | 134 | 134 | 0 | Ribbon namespace after the cache extraction |
| `qa-gates/vstest-coverage-run` | P4-T7 | **6982** | **6982** | **0** | Full first-party suite with coverage |

Test population moved from 6955 to 6982, a delta of 27, which equals the count of new `[TestMethod]`
declarations verified directly from source: 2 XML-consistency + 9 gate + 6 race + 10 cache. The
ribbon directory `[TestMethod]` count moved from 85 to 112, also a delta of 27. No test was removed
or skipped.

Red-before-green disposition per finding:

- **Finding 1 — proven.** Both new tests failed pre-fix on the real defect and pass post-fix.
- **Finding 3 — proven.** The three defect-reproduction tests failed pre-fix; the three
  guard/coverage tests correctly passed pre-fix because they pin behavior that already held.
- **Finding 2 — structurally impossible, and correctly documented.** The defective statements sit
  behind a modal `MessageBox.Show`, a `WindowsFormsSynchronizationContext`, and disk-backed
  classifier serialization, and inside a type-level coverage exemption. A failing pre-fix run would
  have required a message pump, an answered modal dialog and filesystem access, each prohibited by
  the unit-test policy. `evidence/regression-testing/fail-before-exception.2026-09-02T12-04.md`
  records the impossibility and the substituted proof — the three not-ready gate tests map one-to-one
  onto the three null states that produced the `NullReferenceException` — rather than fabricating a
  run. This review accepts that disposition.

## 7. Code Quality Checks

| Check | Command | Exit | Result |
|---|---|---|---|
| Format (apply) | `dotnet tool run csharpier format <in-scope paths>` | 0 | 4 of 8 rewritten on pass 1; 2 of 10 on the branch-B re-run |
| Format (verify) | `dotnet tool run csharpier check .` | 0 | 1576 files, no unformatted path |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | 5 warnings, 0 errors — equal to baseline |
| Nullable type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | 5 warnings, 0 errors — equal to baseline |
| Tests with coverage | `Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration 'Debug' -CoverageOutput <feature evidence path>` | 0 | 6982/6982 passed |
| Exemption-attribute diff | anchored `^[+-]\s*\[ExcludeFromCodeCoverage\]` | 0 | 0 added, 0 removed — independently reproduced |
| File-size ceiling | `awk 'END{print NR}'` per changed file | 0 | max 496 lines |
| Host/account token sweep | case-insensitive grep across all six branch commits | 0 | 0 matching files in the feature folder at every commit |

The toolchain loop closed in a single pass. The mid-loop branch-B extraction (P4-T3) is correctly
accounted for: it changed tracked source after the first format step, and the plan's own branch-B
terms required P4-T1 and P4-T2 to be re-run, which they were, so every gate from the repository-wide
format check onward observed the final tree.

## 8. Gaps and Exceptions

No blocking findings. Thirteen non-blocking observations, each with a file-and-location citation.
Reproduction detail is in the companion code-review artifact.

| ID | Severity | Location | Summary |
|---|---|---|---|
| NB-1 | Non-blocking | `TaskMaster/Ribbon/SpamManagerResetGate.cs:132-140` | `string.Format(CultureInfo.CurrentCulture, <literal>)` with zero format placeholders. Every sibling `string.Format` in the ribbon layer has placeholders; this one copies the shape without the substance. |
| NB-2 | Non-blocking | `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs:269-277` and `:341-353` | `CompletePrime`'s `_primeTasks.TryRemove` runs outside `lock (_primeGate)` while `StartPrimeIfNeeded` registers the marker inside it, and `StartObservedPrime(...)` is evaluated before the dictionary assignment. Pre-existing ordering hazard, made reachable on one additional path by the CR-2 change. |
| NB-3 | Non-blocking | `evidence/qa-gates/vstest-coverage-run.2026-09-02T12-04.md` | Records "9 cache tests"; the actual count is 10. The resulting off-by-one is then explained by attributing the 27th test to a pre-existing baseline-filter artifact, which reconciles an arithmetic error rather than recording an observation. Conclusions are unaffected. |
| NB-4 | Non-blocking | `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs:316` and `:332` | The new `RibbonControlTypeName` constant was added but the pre-existing inline literal at line 316 was not replaced, so the file now holds both. The spec's Test Strategy said the literal would be hoisted. |
| NB-5 | Non-blocking | `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 496 lines — four lines of headroom under the 500-line ceiling. |
| NB-6 | Non-blocking | `spec.md` `## Write Set` and Cross-cutting AC 1 | Not amended when the branch-B contingency was taken. The Write Set lists 10 paths and the criterion says "All three new source files"; the delivered footprint is 12 paths and 5 new files. The amendment is recorded only in `evidence/qa-gates/coordinator-size-contingency`. |
| NB-7 | Non-blocking | `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs:338-339` | `GetViewerCallbackSurface()` returns `GetMethods(Public \| Instance)`, which includes the inherited `System.Object` members, so a callback bound to `ToString`, `Equals`, `GetHashCode` or `GetType` would resolve falsely. |
| NB-8 | Non-blocking | `TaskMaster/Ribbon/RibbonController.Intelligence.cs:206 and :220-230` | `_spamManagerResetGate ??= ...` is not thread-safe. It is safe in practice because ribbon callbacks are serialized on the Outlook STA, but the XML documentation does not state that invariant. |
| NB-9 | Non-blocking (repo hygiene) | `evidence/baseline/coverage-baseline...cobertura.xml`, `evidence/qa-gates/coverage-final...cobertura.xml` | Two Cobertura documents totalling roughly 21.6 MB are committed. Once merged they cannot be removed without leaving reachable blobs in history. Squash-merge is the mitigation. |
| NB-10 | Non-blocking | `evidence/qa-gates/coverage-final...cobertura.xml` | Produced at 01:55 local, before the final `origin/main` merge at 02:06 (`30e66833`). The repository-wide figure therefore describes the pre-merge tree. The branch's own contribution is measured correctly and the repository-wide gate is CI's. |
| NB-11 | Non-blocking (operator action) | `spec.md` F2-AC8 | The manual Outlook verification is unperformed and correctly reported as `OPERATOR-ACTION-REQUIRED`. No code change can satisfy it. |
| NB-12 | Non-blocking (process) | `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt` | Both describe issue #730 (`bug/ci-build-infra-debt-730`, base `8be5a6aa`), not this branch. They are tracked files inherited from `main`, so regenerating them would overwrite another item's committed content. Scope was derived from `git merge-base` and `git diff` instead; the deviation is recorded in Scope Resolution above. |
| NB-13 | Context, not a finding | `TaskMaster/Ribbon/RibbonController.cs:36` | `.claude/rules/general-unit-test.md` states no production file may be excluded from coverage measurement, while `CLAUDE.md` ratifies a COM/VSTO type-level exemption. The `RibbonController` exemption predates this branch and is unchanged by it, so no finding is raised against this change. |

## 9. Summary of Changes

Twelve source and project paths, plus the feature folder and four `.claude/agent-memory/` paths.

| # | Path | Status | Lines |
|---|---|---|---|
| 1 | `TaskMaster/Ribbon/RibbonExplorer.xml` | M | +5 / -6 |
| 2 | `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | M | 444 total |
| 3 | `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | M | 415 total |
| 4 | `TaskMaster/Ribbon/SpamManagerResetGate.cs` | A | 141 |
| 5 | `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` | A | 157 |
| 6 | `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | M | 496 total |
| 7 | `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | M | 459 total; exactly one added `partial` keyword |
| 8 | `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | A | 277 |
| 9 | `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` | A | 326 |
| 10 | `TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs` | A | 213 |
| 11 | `TaskMaster/TaskMaster.csproj` | M | +2 compile items |
| 12 | `TaskMaster.Test/TaskMaster.Test.csproj` | M | +3 compile items |

The four `.claude/agent-memory/` paths (`atomic-planner/MEMORY.md`,
`atomic-planner/project_735_evidence_content_sanitization_seams.md`,
`task-researcher/MEMORY.md`, `task-researcher/project_ribbon_engine_toggle_defects_735.md`) were
audited rather than excluded. They originate from the preparation commit `044551f0`, contain no host
paths, no credentials and no policy-document edits, and their factual claims were spot-checked: the
"84 distinct callback names, 5 dead" figure matches this review's independent parse exactly.

## 10. Compliance Verdict

| Section | Verdict |
|---|---|
| 1. General Unit Test Policy | **PASS** |
| 2. General Code Change Policy | **PASS** |
| 3. C# Code Change Policy | **PASS** |
| 4. C# Unit Test Policy | **PASS** |
| 5. Test Coverage Detail — C# coverage | **PASS** |
| 6. Test Execution Metrics | **PASS** |
| 7. Code Quality Checks | **PASS** |
| 8. Gaps and Exceptions | 0 blocking, 13 non-blocking |
| Evidence Location Compliance | **PASS** |

**Overall: PASS.** No remediation-inputs artifact is produced, because there are zero blocking
findings and the one open acceptance criterion (F2-AC8) is an operator action that no code change
can satisfy.

## Appendix A: Test Inventory

**`TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`** — 2 added (8 -> 10)

1. `RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod`
2. `RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters`

**`TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs`** — 9 new

1. `Constructor_WithNullAutoFileAccessor_ThrowsArgumentNullException`
2. `Constructor_WithNullEnginesAccessor_ThrowsArgumentNullException`
3. `Constructor_WithNullNotifyDelegate_ThrowsArgumentNullException`
4. `RunAsync_WithNullReset_ThrowsArgumentNullExceptionBeforeProbingAccessors`
5. `RunAsync_WhenAutoFileAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset`
6. `RunAsync_WhenManagerIsNull_NotifiesOnceAndDoesNotInvokeReset`
7. `RunAsync_WhenEnginesAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset`
8. `RunAsync_WhenAllDependenciesAvailable_InvokesResetWithResolvedManagerAndEngines`
9. `RunAsync_WhenResetFaults_PropagatesUnchangedAndDoesNotNotify`

**`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs`** — 6 new

1. `ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult` (failed pre-fix)
2. `ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult` (failed pre-fix)
3. `ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce`
4. `ExecuteToggleAsync_WithNullEngines_ThrowsInvalidOperationExceptionWithoutTogglingEngine` (CR-3)
5. `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` (CR-2, failed pre-fix)
6. `GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked` (CR-2)

**`TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs`** — 10 new

1. `NextSequence_OnSuccessiveCalls_ReturnsStrictlyIncreasingTickets`
2. `NextSequence_IsSharedAcrossKeys_SoTicketsAreGloballyOrdered`
3. `TryGetActive_ForKeyWithNoObservation_ReturnsFalseAndFalse`
4. `TryGetActive_AfterAppliedWrite_ReturnsTheStoredValue`
5. `TryGetActive_IsOrdinalAndCaseSensitive`
6. `TryApplyState_OnFirstObservationForAKey_AppliesAndReportsApplied`
7. `TryApplyState_WithNewerTicket_OverwritesAndReportsApplied`
8. `TryApplyState_WithOlderTicket_IsRejectedAndLeavesTheCachedValue`
9. `TryApplyState_WithEqualTicket_IsRejected`
10. `TryApplyState_KeepsKeysIndependent`

Total new tests: **27**.

## Appendix B: Toolchain Commands Reference

```
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration 'Debug' -CoverageOutput <feature evidence path>
```

Reviewer verification commands (read-only, no mutation):

```
git -C <worktree> rev-parse origin/main
git -C <worktree> merge-base HEAD origin/main
git -C <worktree> diff --name-status b13d5b7b HEAD
git -C <worktree> diff b13d5b7b HEAD | grep -n "^[+-].*ExcludeFromCodeCoverage"
git -C <worktree> show b13d5b7b:TaskMaster/Ribbon/RibbonController.cs | grep -n ExcludeFromCodeCoverage
git -C <worktree> grep -c -i -I -E "<account token>" <each of the six branch commits> -- <feature folder>
awk 'END{print NR}' <each changed file>
```
