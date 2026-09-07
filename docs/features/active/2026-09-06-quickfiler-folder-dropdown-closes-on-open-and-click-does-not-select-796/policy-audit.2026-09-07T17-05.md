# Policy Audit — issue #796 (QuickFiler folder drop-down closes on open; row click does not select)

- Timestamp: 2026-09-07T17-05
- Issue: #796
- Work Mode: full-bug (marker read from `issue.md` line 12: `- Work Mode: full-bug`)
- Feature folder: `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796`
- Base ref: `a6b259160f9ac1fbe251708d897fd4721486259e` (origin/main, ancestry verified by the executor at P9-T10)
- Branch head reported by the caller: `8e427fe1`
- Reviewed worktree: `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-af8210acca019debc`
- Policies applied, in order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`

## Template Provenance

The MCP tool `mcp__drm-copilot__resolve_policy_audit_template_asset` is not available in this
session, so this artifact was hand-authored preserving every canonical major heading required by
`.claude/skills/policy-audit-template-usage/SKILL.md`. `mcp__drm-copilot__validate_orchestration_artifacts`
is likewise unavailable and was not run. This is a tooling limitation, recorded rather than
concealed; the structural requirements of the skill are met.

## Review Method and Its Limits (recorded as an assumption)

The caller imposed a binding tooling constraint: the Bash tool must not be used in this worktree
because an unattended `git` invocation from a review agent hangs indefinitely. All evidence below
was therefore gathered with read-only file inspection (Read, Grep, Glob) plus the pre-materialised
diff the caller supplied at
`C:/Users/DANMOI~1/AppData/Local/Temp/claude/C--Users-DanMoisan-repos-TaskMaster-wt-2026-09-06T17-16/b3e58737-6e95-4aae-b188-50cc8e7cf80a/scratchpad/796-code-diff.patch`,
which is `git diff a6b25916..HEAD -- QuickFiler QuickFiler.Test`.

Consequences, stated so no reader over-reads this audit:

- The supplied patch is pathspec-limited to `QuickFiler` and `QuickFiler.Test`. It is therefore the
  complete CODE footprint but not the complete path footprint.
- The claim that the full branch diff contains 84 paths — 15 code, 68 feature-folder, 1 promoted
  potential record — and zero paths under `UtilitiesCS/`, `UtilitiesCS.Test/`, `.claude/`, `.codex/`,
  `.agents/`, `config/`, `.github/` or `TaskMaster.sln` could not be re-derived by this reviewer with
  `git`. It is corroborated by second-party evidence at
  `evidence/qa-gates/p9-t10-scope-boundary.md`, which records the anchored two-dot `--name-status`
  diff, its ancestry check, and a mechanical classification of all 84 paths. This audit treats that
  as the scope record and marks the independent re-derivation UNVERIFIED-BY-TOOLING, not absent.
- `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` do not exist in this
  worktree (verified by glob over `artifacts/**`; only `pr_body_*` and `artifacts/orchestration/**`
  are present). They could not be regenerated without shell access. The caller-supplied full-code
  patch plus the scope-boundary artifact were used in their place.

## Rejected Scope Narrowing

No caller instruction attempted to narrow the audit to a plan, task, phase, or file subset, and none
declared any language "out of scope", "informational only", or "not applicable". Two caller
statements were examined against the scope invariant and neither is a narrowing:

1. "Do NOT use the Bash tool at all." This constrains the TOOL, not the scope. The full branch code
   diff was supplied in file form precisely so the full-footprint audit could still be performed. It
   is recorded above as a method limitation with its consequences enumerated.
2. "Do not raise the pre-existing absolute repository coverage level as a blocking finding against
   this item." This directs the DISPOSITION of a finding, not its existence. The finding is recorded
   below with an explicit FAIL verdict against the policy floor and a non-blocking disposition, which
   is the treatment this repository has applied to the same condition before. No coverage check was
   skipped and no language verdict was suppressed.

Neither statement was acted on as a narrowing. The audit scope is the full branch diff against
`a6b25916`.

## Evidence Location Compliance

`validate_evidence_locations.py` could not be executed (no shell). Compliance was established by
enumerating the feature folder with Glob and classifying every path.

- Every evidence artifact in this feature lies under `<FEATURE>/evidence/<kind>/` with `<kind>` in
  {`baseline`, `qa-gates`, `regression-testing`, `issue-updates`, `other`}. All five are canonical
  sub-paths per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
- `<FEATURE>/research/` and `<FEATURE>/runbooks/` are feature documents, not `evidence/` artifacts,
  and are outside the evidence scheme.
- Zero files were written under `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`,
  `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`
  or `artifacts/post-change/`. The only `artifacts/` content in this worktree is
  `artifacts/orchestration/` (an allowed non-evidence sub-path) and pre-existing `pr_body_*` files
  belonging to other issues.
- Raw coverage XML, TRX files and MSBuild logs were written to the gitignored `coverage/` and
  `TestResults/` directories and are not committed. That is correct: none of them is an evidence
  artifact path, and a TRX embeds host and account names.

No evidence-location violation was found. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` entry is required.

## Executive Summary

The change is a bug fix in `QuickFiler` delivering four production mechanisms (an AC2 self-inflicted
deactivation seam, an AC3 pending-commit latch, an AC4 search-owned dismissal latch, and AC6 debug
instrumentation) plus five test changes. Fifteen code paths changed; all fifteen are declared write-set
members.

The mandated four-stage C# toolchain completed a single clean pass in order: CSharpier check clean over
1608 files, `/t:Rebuild` analyzer gate 0 warnings / 0 errors with 36 compiler invocations, `/t:Rebuild`
nullable gate 0 warnings / 0 errors with 36 compiler invocations and no `/p:Nullable=enable` token, and
1380 of 1380 `QuickFiler.Test` tests passing against a baseline of 1370 with an empty failure set.

Coverage: changed-code line coverage is 97.5610 percent (40/41), corroborated at 95.2381 percent with no
exclusion taken and 97.2222 percent with all comment lines removed; all three clear the 90 percent
new-code floor. Repository-wide line coverage is 24.1857 percent, which FAILS the policy floor. That
figure is pre-existing (24.1387 percent at baseline), was moved upward by this item, and is recorded
below as a FAIL with a non-blocking disposition.

Verdict: PASS. Blocking findings: 0.

## 1. General Unit Test Policy Compliance

Source: `.claude/rules/general-unit-test.md`.

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence — tests run in any order | PASS | The three added suites construct all state per test. `CloseOrderingHostHarness` and `ItemViewerDropDownHarness` save and restore `SynchronizationContext.Current` in constructor/`Dispose`, so no ambient state leaks across tests. |
| Isolation — one unit per test | PASS | Each added test drives one seam: one formatter, one close branch, one leave branch, one open path. |
| Fast execution | PASS | Whole assembly of 1380 tests runs in 13.4 s (`evidence/qa-gates/p9-t5-full-assembly-tests.md`). |
| Determinism — no flakiness, no wall-clock | PASS | `InlineSynchronizationContext.Post` runs callbacks inline so the popup lifecycle settles synchronously; `OpenAndSettle` asserts `opening.IsCompleted` before acting. Grep over `QuickFiler.Test` for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `new Random(` returns no hit in any file changed by this item. |
| Readability, documented intent | PASS | Every added `[TestMethod]` carries a `<summary>` stating Scenario and Expected outcome, and the two whose proof is limited carry a `<remarks>` stating the limit (`BreadcrumbDropDownCloseOrderingTests.cs` lines 305-310 and 359-367). |
| Arrange–Act–Assert structure | PASS | All added tests carry literal `// Arrange`, `// Act`, `// Assert` comments in that order. |
| Clear failure messages | PASS | The `CancelCount` assertions carry FluentAssertions `because` reasons ("a close racing an in-flight commit must not cancel the selection"; "a close with no commit in flight still cancels the selection"). |
| No external dependencies (DB, network, process, live Outlook) | PASS | The surface factory seam returns a plain `Panel` and a stub messenger; `CoreWebView2Environment` is obtained via `FormatterServices.GetUninitializedObject` rather than a real environment. No window is shown. The full run uses `TestCategory!=LiveOutlook`. |
| No temporary files in tests | PASS | Grep of the three changed/added test files finds no `Path.GetTempPath`, `Path.GetTempFileName`, `File.Create` or equivalent. |
| No mutable global state | PASS | The only ambient state touched is `SynchronizationContext.Current`, which both harnesses restore in `Dispose` (the `CloseOrderingHostHarness` constructor also restores it on a throwing construction path). |
| Scenario completeness — positive, negative, boundary | PASS | Every new mechanism is landed as a polarity PAIR: AC2 genuine/self-inflicted, AC3 commit-pending/no-commit-pending, AC4 search-driven/mouse-driven. |
| Coverage floors | PARTIAL | Changed-code and new-file figures pass; repo-wide line and branch rates fail the floor. See section 5. |
| No production file excluded from coverage measurement | PASS (with recorded policy conflict) | This diff adds no coverage `exclude` entry and no `[ExcludeFromCodeCoverage]` attribute. The pre-existing class-level attributes on `QfcFormViewer` (line 17) and `ItemViewer` (line 20) are WinForms form-derived types, formally exempted by the ratified COM/VSTO/WinForms carve-out in `CLAUDE.md` § UT2. That carve-out and the flat prohibition in `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy are in unreconciled conflict repo-wide; this item neither creates nor widens the conflict. Recorded, not charged to this change. |
| Test file location | PASS (with recorded convention conflict) | `.claude/rules/general-unit-test.md` requires a `tests/` tree. This repository's C# convention is a sibling `<Project>.Test` assembly, used by every existing C# test in the tree. The new files follow the repository convention and mirror the production namespace. Pre-existing repo-wide divergence, not introduced here. |
| Determinism infrastructure (banned APIs) | PASS | No banned timing API appears in any changed test file. |

## 2. General Code Change Policy Compliance

Source: `.claude/rules/general-code-change.md` and `CLAUDE.md` § General Code Change Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow — failing regression test first | PASS | Documented RED-first evidence for every behavioural criterion: `evidence/regression-testing/p4-t5-ac2-fail-before.md`, `p5-t3-ac3-fail-before.md`, `p6-t4-ac4-fail-before.md`. P6-T4 records EXIT_CODE 1 with `ExpectedExitCode: 1`, Total 2 / Passed 1 / Failed 1, and the exact Moq message proving the failure is the intended one, not a harness defect. |
| Minimal, targeted fix | PASS | Fifteen code paths, all write-set members. Two conditional write-set paths (`FolderBreadcrumb.html`, `BreadcrumbDropDownOpenCoordinator.cs`) were deliberately NOT changed after the evidence refused to justify them; recorded at `evidence/other/close-ordering-decision.md` (`AC3-HTML-POINTERDOWN: NOT REQUIRED`, `AC3-ENFORCEMENT-SITE: HOST`). |
| Simplicity first | PASS | Each mechanism is a single boolean latch or a single guarded early return. No new abstraction, no new interface beyond one property. |
| Separation of concerns | PASS | The three diagnostic formatters are pure static string builders taking primitives, so the AC6 evidence rests on deterministic managed-seam assertions rather than a source-text scan. Logging is separated from the close handler's control flow. |
| Extensibility / no breaking public API | PASS | One member added to the internal add-in interface `IQfcFormViewer`; its only implementor, `QfcFormViewer`, is updated in the same diff. `IBreadcrumbDropDownHost` and `IQfcItemController` are unchanged, so mock hosts and the hand-written test implementor are undisturbed. Host constructor arity is unchanged; `IsCommitPending` is a settable internal property, matching the #677 `MayTakeFocus` precedent. |
| Error handling — fail fast, no silent swallow | PASS | No new catch is introduced. The pre-existing per-item boundary catch in the cancel loop is preserved with its rationale comment intact (`QfcFormController.Deactivate.cs` lines 135-146). |
| Logging uses the project pattern | PASS | Controllers use `logger`, viewers use `log`; the new `BreadcrumbDropDownHost.Diagnostics.cs` declares `log4net.LogManager.GetLogger(typeof(BreadcrumbDropDownHost))`. Both new lines are Debug level and neither alters control flow. The `OnDropDownClosed` line is emitted at entry, ahead of the guard return, as the spec requires. |
| Comment why, not what | PASS with one exception | Comments are consistently rationale-bearing. One pre-existing comment is now falsified by the change — see CR-1 in the code review. |
| File size ≤ 500 lines | PASS | 15 paths measured post-format with the baseline-commensurable idiom `(Get-Content -LiteralPath $_).Count`; maximum 496 at `BreadcrumbDropDownHost.cs`, which FELL from 498 because `OnDropDownClosed` was relocated to the new part. `evidence/qa-gates/p9-t8-file-size-audit.md`. Test files are included in the audit, as this repository requires. |
| Toolchain loop in exact order, restart on failure | PASS | `evidence/qa-gates/p9-t11-final-loop.md` records the RESTART branch honestly: P9-T3 attempt 1 failed on three `MSB3061` file-lock warnings caused by a running Outlook process, the cause was timestamped to a 42-second window outside this item's diff, the gate was NOT reinterpreted to accommodate it, and the loop was rerun end to end after the cause was cleared. |
| Dependencies | PASS | No package added or changed. |
| I/O boundaries | PASS | The new logic touches no disk, network or COM. `Form.ActiveForm` is read only for a diagnostic field that the code explicitly does not act on. |

## 3. Language-Specific Code Change Policy Compliance (C#)

Source: `CLAUDE.md` § C# Code Change Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier via `dotnet tool run`, manifest-pinned | PASS | `dotnet tool run csharpier check .` EXIT_CODE 0, "Checked 1608 files", empty unformatted list (`evidence/qa-gates/p9-t2-csharpier-check.md`); reproduced in the final loop. |
| `dotnet format` NOT used | PASS | No artifact records a `dotnet format` invocation; every formatting command in the evidence tree is `dotnet tool run csharpier`. |
| Analyzers with `/t:Rebuild` (never `/t:Build`) | PASS | Command reproduced verbatim in `evidence/qa-gates/p9-t3-analyzer-rebuild.md`; `/t:Rebuild` present, `CscTaskCount=36` and `CscToolCount=36` read back from the detailed log, so the gate is not vacuous. 0 warnings, 0 errors, matching the P0-T8 baseline of 0/0. |
| Nullable gate with `/t:Rebuild` and `/p:TreatWarningsAsErrors=true` | PASS | `evidence/qa-gates/p9-t4-nullable-rebuild.md`: EXIT_CODE 0, 0/0, 36/36 invocations, both touched assemblies rewritten after `RunStartedUtc`. |
| No solution-wide `/p:Nullable=enable` | PASS | Absent from the command text and corroborated mechanically: `NullableEnableTokenCount=0` over the detailed-verbosity log, which reproduces every project's csc command line. |
| Per-file nullable opt-in respected | PASS | The one new production file, `BreadcrumbDropDownHost.Diagnostics.cs`, opens with `#nullable enable` and its handler signature uses `object? sender`, matching the part it was moved from. |
| Strong contracts, explicit types at boundaries | PASS | Every added member has an explicit return type and XML documentation. `SetBreadcrumbPopupOwner` validates both arguments and returns without side effect when either is null. |
| `var` only where obvious | PASS | The added production code uses explicit types throughout. |
| Naming conventions | PASS | `PascalCase` members (`IsCommitPending`, `IsDeactivationSelfInflictedByOwnPopup`, `FormatDropDownClosedDiagnostics`), `_camelCase` private fields (`_searchOwnedDismissal`, `_breadcrumbPopupOwners`). Names are descriptive rather than abbreviated. |
| XML documentation on non-obvious public/internal API | PASS | Notably strong: the `IQfcFormViewer` member documents that its polarity is load-bearing and must not be inverted, and the `activeFormIsNull` parameter documents that the discriminator it was introduced for was measured to run inverted and is therefore retained as data only. |
| Non-SDK-style projects need explicit `<Compile Include>` | PASS | Three entries added — one in `QuickFiler.csproj` for the diagnostics part, two in `QuickFiler.Test.csproj` for the new test files. Effectiveness is proved rather than assumed: P6-T4 reports Total 2 for the new latch class, which is non-zero only if the compile entry took effect. |
| Internal over public where possible | PASS | Every new member except the interface property and its implementation is `internal` or `private`. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

Source: `CLAUDE.md` § C# Unit Test Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]` / `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting` in all changed test files. No xUnit or NUnit reference introduced. |
| Moq for mocking | PASS | `Mock<IItemViewer>`, `Mock<IFolderSearchHandler>`, `Mock<IBreadcrumbDropDownHost>`, `Mock<IQfcItemController>`, `MockBehavior.Strict` where a provider must not be called. |
| FluentAssertions for assertions | PASS | Every new assertion is `Should()`-based. No MSTest `Assert.*` call was added. |
| Test commands | PASS | `vstest.console.exe` resolved through `vswhere` to `Common7\IDE\Extensions\TestPlatform\vstest.console.exe`, run with `/InIsolation` and the repository runsettings. |
| Deliberate updates to tests that pin changed contracts, not weakening | PASS | Two updates, both disclosed and both minimal. See section 8. |

## 5. Test Coverage Detail

Coverage artifacts inspected (not regenerated): `coverage/p9-t6-final.cobertura.xml` and
`coverage/p0-t11-baseline.cobertura.xml`. The canonical path `artifacts/csharp/coverage.xml` is
absent; per the precedent that a feature-run Cobertura document counts as the artifact, the two
documents above were read directly and their document-level attributes were re-verified by this
reviewer with a literal grep, not taken from the executor's summary.

Independent verification performed by this reviewer:

- `coverage/p9-t6-final.cobertura.xml` carries `line-rate="0.241857"`, `lines-covered="14925"`,
  `lines-valid="61710"` — confirmed present.
- `coverage/p0-t11-baseline.cobertura.xml` carries `line-rate="0.241387"`, `lines-covered="14867"`,
  `lines-valid="61590"` — confirmed present.
- `QfcFormViewer.cs` appears in ZERO class nodes of the final document, corroborating the
  class-level `[ExcludeFromCodeCoverage]` at source line 17 and the executor's NOT MEASURABLE row.
- `BreadcrumbDropDownHost.Diagnostics.cs` is present in the final document, so the new part is
  instrumented rather than silently absent.

Coverage verdicts by language. Every language with changed files carries an explicit PASS or FAIL.

| Language and scope | Measured | Threshold | Verdict |
|---|---|---|---|
| C# (csharp) repo-wide line coverage 24.1857% | 24.1857% | >= 85% (`.claude/rules/quality-tiers.md`), >= 80% (`CLAUDE.md`) | FAIL — non-blocking disposition |
| C# (csharp) repo-wide branch coverage 23.0082% | 23.0082% | >= 75% | FAIL — non-blocking disposition |
| C# (csharp) repo-wide no-regression: 24.1387% -> 24.1857% coverage | +0.0470 pp | must not decrease | PASS |
| C# (csharp) changed-code line coverage 97.5610% (40/41) | 97.5610% | >= 90% new code, >= 85% modified | PASS |
| C# (csharp) changed-code corroboration, no exclusion taken, coverage 95.2381% (40/42) | 95.2381% | >= 90% | PASS |
| C# (csharp) changed-code corroboration, comment lines removed, coverage 97.2222% (35/36) | 97.2222% | >= 90% | PASS |
| C# (csharp) new file BreadcrumbDropDownHost.Diagnostics.cs coverage 100.0000% (28/28) | 100.0000% | >= 85% | PASS |
| PowerShell (Pester) — 0 changed `.ps1` files in the branch diff, so 0 files enter coverage; coverage obligation nil | 0 files | line >= 85%, no branch gate | PASS |
| Python — 0 changed `.py` files in the branch diff; coverage 0 files measured | 0 files | >= 85% / >= 75% | PASS |
| TypeScript — 0 changed `.ts`/`.tsx` files in the branch diff; coverage 0 files measured | 0 files | >= 85% / >= 75% | PASS |

Disposition of the two repo-wide FAIL rows. Both are recorded as FAIL because the measured figures
are below the policy floors and a below-floor figure is never recorded as PASS. Both carry a
NON-BLOCKING disposition and no remediation-inputs artifact, on these grounds, each of which is
independently checkable:

1. The condition is pre-existing. The baseline document, produced before any change on this branch,
   records 24.1387 percent from the same single-assembly-scoped runner.
2. The item improved it. Numerator +58, denominator +120, ratio +0.0470 pp.
3. The changed-code floor is met three separate ways, none of which was chosen after seeing which
   one passed.
4. The figure is a whole-solution document rate produced by a run scoped to one test assembly, so it
   is not a measurement of repository quality by any single item's authorship.

Per-file rows for the five named production files, baseline versus final (executor measurement, from
`evidence/qa-gates/p9-t6-coverage-final.md`):

| File | Final covered / valid | Baseline covered / valid |
|---|---|---|
| QfcFormController.Deactivate.cs | 48 / 48 | 25 / 25 |
| BreadcrumbDropDownHost.cs | 287 / 289 | 291 / 293 |
| BreadcrumbDropDownHost.Open.cs | 24 / 24 | 23 / 23 |
| QfcItemController.EventHandlers.cs | 97 / 118 | 89 / 108 |
| BreadcrumbDropDownOpenCoordinator.cs | 234 / 238 | 234 / 238 |

No changed file regressed on its changed lines. The one changed-line exclusion taken
(`QfcItemController.EventHandlers.cs` line 282, `IsBreadcrumbSelectorOpen`) is individually named
with a reason that is mechanically checkable: its only reader casts an `IQfcItemController` to the
concrete `QfcItemController`, and every deactivate test injects Moq mocks of the interface, so the
cast yields null and the member is never evaluated. One of an allowance of three was spent.

The second uncovered changed line, `EventHandlers.cs` line 209, was deliberately left in the
denominator rather than excluded. That is the correct choice and is adjudicated as F1 in section 8.

## 6. Test Execution Metrics

| Metric | Value | Source |
|---|---|---|
| Assembly | `QuickFiler.Test.dll` | `evidence/qa-gates/p9-t5-full-assembly-tests.md` |
| Total | 1380 | run summary and TRX (`TotalResultNodes=1380`) |
| Passed | 1380 | run summary |
| Failed | 0 | `NonPassedCount=0` over all 1380 TRX result nodes |
| Baseline total | 1370 | `evidence/baseline/p0-t10-quickfiler-test-baseline.md` |
| Baseline failure set | EMPTY | same |
| Net new tests | +10 | 1380 − 1370 |
| Filter | `TestCategory!=LiveOutlook`, `/InIsolation` | command recorded verbatim |
| Duration | 13.4 s | run summary |
| Final-loop re-run | 1380 / 1380, EXIT_CODE 0 | `evidence/qa-gates/p9-t11-final-loop.md` |
| Expect-fail inventory | 4 rows, all Passed after the fix | resolved by `testName` against the TRX |

The "Failed: 0" reading is a measurement, not an omission: the console prints no `Failed:` line on a
passing run, so the executor corroborated it against the TRX result nodes. The inventory count of 4
rather than 5 is correctly derived from the recorded `AC2-PARK-FOCUS-SUPPRESSED: NO` branch, and the
executor recorded that a literal token search of the P4-T7 artifact returns the OPPOSITE answer
because the single hit is the artifact declaring the token absent. That is the kind of trap that
usually produces a wrong count; it was caught.

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier format .` then `check .` | EXIT_CODE 0, 1608 files, empty unformatted list |
| Lint / analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0, 0 warnings, 0 errors, 36/36 compiler invocations |
| Type check / nullable | `msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | EXIT_CODE 0, 0 warnings, 0 errors, 36/36 compiler invocations, 0 `Nullable=enable` tokens |
| Test | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:... /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook"` | EXIT_CODE 0, 1380 / 1380 |

Gate anti-vacuity was checked in both build gates: `/t:Rebuild` was used rather than `/t:Build`, the
compiler-invocation counts were read back from a detailed log and equal the baseline's 36, and both
touched assemblies carry a `LastWriteTimeUtc` later than the run start. A warm `/t:Build` returning
exit 0 with `CoreCompile` skipped — the documented failure mode for these gates — is excluded.

Tonality (`.claude/rules/tonality.md`): the diff's comments, the XML documentation, and the evidence
artifacts are factual and measured throughout. Several places state a limit rather than overstating a
result — for example `BreadcrumbDropDownCloseOrderingTests.cs` lines 359-367 explicitly record which
half of AC1 the test does NOT prove. No humour, hyperbole, or decorative metaphor was found. PASS.

## 8. Gaps and Exceptions

### F1 — Dead internal accessor `SearchOwnsDropDownDismissal` (adjudicated: NOT blocking)

`QuickFiler/Controllers/QfcItemController.EventHandlers.cs` line 209:

```csharp
internal bool SearchOwnsDropDownDismissal => _searchOwnedDismissal;
```

Verified independently by this reviewer: a Grep for the identifier across every `.cs` file in the
worktree returns exactly one hit, the declaration. The property has no reader in production or test
code.

The distinction the caller asked to be made precisely, re-verified here rather than accepted:

- The underlying FIELD `_searchOwnedDismissal` is fully live. Grep places it at line 203
  (declaration), written at 183, 220, 234, 257 and 265, and READ at 263 in `if (!_searchOwnedDismissal) return;`
  — which is the AC4 latch itself. The AC4 mechanism is delivered and exercised.
- Only the exposing property is dead. The re-pinned test reaches the state by reflection
  (`QfcItemControllerTestSupport.SetField(controller, "_searchOwnedDismissal", true)` at
  `QfcItemController.SearchDismissalTests.cs` line 85), by field name, which is why the property
  never acquired a reader.

Adjudication: NOT blocking, stated plainly. It is an `internal`, get-only, side-effect-free
expression-bodied member on an internal partial class. It changes no public surface, cannot be called
by any consumer outside the assembly, introduces no branch, and cannot alter behaviour. Its entire
cost is one uncovered line, and that line was deliberately left IN the changed-code denominator
rather than excluded, so the 97.5610 percent figure is reported against it rather than around it.
It is a tidiness defect, not a correctness or policy defect. Under
`.claude/rules/general-code-change.md` ("make the public surface area small and intentional") it is a
minor violation with no consequence.

Recommended, not required: either delete the property, or make the re-pinned test set the state
through the property's sibling path instead of reflecting on a field name — which would also remove
the string-literal field-name coupling noted as CR-6 in the code review. Neither is a condition of
merge.

### F2 — Falsified plan premise about comment lines and Cobertura (adjudicated: adequately handled)

The plan reasoned that "a comment line carries no Cobertura `line` node at all", so comment-only
changed lines could not enter the coverage denominator. The executor measured that this holds for 139
of 144 comment-only changed lines, and fails for five: `BreadcrumbDropDownHost.cs` lines 442-446 each
carry a `line` node with `hits=1`, while lines 251-254 in the same file carry none, so the behaviour
is not even uniform within one file.

Adjudication: the disclosure and the three-way reporting are an adequate response, and better than
adequate in three specific respects.

1. The departure was disclosed in the direction that DISADVANTAGES the executor's own figure. Those
   five lines are covered, so including them raises the changed-code percentage; the executor said so
   explicitly ("those five lines sit in the denominator and are all covered, so including them raises
   the changed-code figure") rather than letting the favourable arithmetic pass silently.
2. The result is shown not to depend on the disputed mechanism. 97.5610 percent (reported), 95.2381
   percent (no exclusion taken), 97.2222 percent (all comment lines removed from both numerator and
   denominator, which neutralises the departure entirely). All three clear 90 percent.
3. The artifact records "Neither computation was selected after seeing which one passed; both are
   reported", and the third computation is precisely the one that removes the benefit of the
   departure. Post-hoc metric selection is the failure mode this disclosure forecloses.
4. The proposed mechanism ("the conversion maps the full source span preceding a mapped statement")
   is labelled as unestablished and explicitly not relied upon. That is the correct epistemic
   handling of an unverified explanation.

No further action is required. NOT blocking.

### E1 — Human-interaction exception HI-796-1 (AC6 runtime observation)

Recorded as a permitted exception, following the #400 and #438 precedent. The runbook at
`runbooks/confirm-dropdown-close-ordering.runbook.md` was executed by the maintainer against a Debug
build from `ec674e0c`, whose parent `0dfcb402` is the instrumentation commit; the observation states
that `ec674e0c` changes no compiled source relative to its parent, so the instrumentation is present
in the observed build. The transcript is at
`evidence/other/2026-09-07T12-19-dropdown-close-ordering-observation.md` and its conformance check at
`evidence/other/p2-t2-manual-observation-conformance.md`.

Quality notes on the exception, since a manual artifact is the weakest evidence class in this
repository and deserves scrutiny rather than deference:

- It carries an explicit Elision notice stating exactly which line runs were elided and guaranteeing
  that no `SelectorWasOpen=True` line, no `entered.` line and no `OnDropDownClosed` line is elided,
  so file order among decision-carrying lines is intact.
- It carries a Redaction note: no absolute host paths, user names, mailbox addresses or personal
  folder names appear in the excerpt. Verified by reading the artifact.
- It records values that CONTRADICT the predictions of the spec and the research artifact
  (`ActiveFormNull` inverted on all four observations; `WebView2Focused` reversed on both gestures)
  and follows the observation rather than the prediction. A manual artifact that only confirmed its
  own predictions would warrant more suspicion than this one does.
- It states its own blind spot rather than filling it: candidate 3 is recorded as NOT DIRECTLY
  OBSERVABLE, with the reason that a handler carrying no logging site emits nothing whether it ran or
  not, and the decision record refuses to read that silence as refutation.

Outlook is deliberately closed and was not relaunched by this review. No gate here depends on
relaunching it.

### E2 — Deliberate test update, not a weakening (confirmed by re-derivation, not assumption)

`QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs`, method
`TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent`. The caller asked that this
reasoning be confirmed rather than assumed. It was, on five independent points:

1. The method name is retained. Verified in the current file at line 80.
2. The `Times.Once()` assertion on `SetFolderDroppedDown(false)` is retained. Verified at line 91.
3. Exactly one Arrange line was added (line 85) plus a `<para>` in the doc comment. The diff hunk
   confirms no other line changed in the method.
4. The class's `[TestMethod]` count is unchanged at 6. Verified by a Grep count over the current
   file, matching the spec's stated requirement.
5. The case the method no longer covers is covered elsewhere with the opposite assertion:
   `SearchLeaveAfterMouseDrivenOpen_DoesNotCloseDropDown` asserts `Times.Never()` on the same call
   (`QfcItemController.SearchLeaveLatchTests.cs` line 178). Coverage of the mouse-driven case is
   moved, not lost.

The provenance claim was also checked against a second source. `evidence/other/phase7-blocking-finding-out-of-write-set-test.md`
records that the executor STOPPED at this test rather than editing it, on the ground that the file was
not then a write-set member and that narrowing the AC4 fix to keep it green would mean not delivering
AC4. The plan delta, the seventeenth write-set path and the spec's Test Strategy section were authored
in response. That is the correct handling of a pre-existing test that pins a contract an AC deliberately
changes, and it is the opposite of quietly weakening a test to make a gate pass.

### E3 — Two write-set paths deliberately not changed

`QuickFiler/Resources/FolderBreadcrumb.html` and `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`
are write-set members that the diff does not touch. This is correct and is evidence-driven, not an
omission: the write set is an upper bound on what may change, and both paths were adjudicated in the
decision record from measured field values (`PendingClose=False` on all three gestures refutes a
coordinator-sited latch; the transcript's silence on activation cannot meet the admissibility condition
for the HTML change). The decision record additionally records the limitation that this "states the
evidence does not support the page change, not that the page change has been shown unnecessary".

### E4 — Producer side of the AC2 seam is not covered by any automated test

`QfcFormViewer.IsDeactivationSelfInflictedByOwnPopup` and the `ItemViewer.Breadcrumb.cs` registration
call both sit in types carrying a class-level `[ExcludeFromCodeCoverage]`, and no test exercises
either. The AC2 tests mock `IQfcFormViewer`, so what is proven is the CONSUMER's gating, not the
producer's derivation. This is permitted by the ratified WinForms exemption in `CLAUDE.md` § UT2 and
is consistent with the spec's own automation-feasibility table, but the spec's claim that AC2 is
automatable "Yes, fully" is slightly overstated: fully at the seam, not end to end. Recorded as a
limitation, not charged as a violation.

## 9. Summary of Changes

Fifteen code paths, all write-set members.

Production — modified (7):
- `QuickFiler/Controllers/QfcFormController.Deactivate.cs` — AC6 entry and per-item diagnostics via two pure static formatters; AC2 guard scoped to the cancel loop.
- `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` — AC4 `_searchOwnedDismissal` provenance latch with two producers and one consumer; AC6 `IsBreadcrumbSelectorOpen`.
- `QuickFiler/Interfaces/IQfcFormViewer.cs` — AC2 seam declaration with load-bearing polarity documented.
- `QuickFiler/Viewers/QfcFormViewer.cs` — AC2 popup-owner registry and predicate implementation.
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` — AC2 registration beside the existing `MayTakeFocus` assignment (4 lines).
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` — AC3 latch set on `ExplicitCommit`; AC3 conditional cancel in `FinishClose`; `OnDropDownClosed` relocated out.
- `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` — AC3 `IsCommitPending` property; cleared in `ShowPopup`.

Production — created (1):
- `QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs` — AC6 host-side instrumentation, the log4net field, and the relocated native-close handler.

Test — modified (3) and created (2):
- `QfcFormControllerDeactivateTests.cs` (+1 explicit genuine-case Arrange, +2 tests), `BreadcrumbPendingOpenCloseTests.cs` (+2 tests, all five existing tests retained), `QfcItemController.SearchDismissalTests.cs` (deliberate re-pin), `BreadcrumbDropDownCloseOrderingTests.cs` (new, 4 tests), `QfcItemController.SearchLeaveLatchTests.cs` (new, 2 tests).

Compile entries — modified (2): `QuickFiler.csproj` (1 entry), `QuickFiler.Test.csproj` (2 entries).

## 10. Compliance Verdict

**PASS. Blocking findings: 0.**

Section verdicts: §1 PASS (coverage sub-row PARTIAL, dispositioned), §2 PASS, §3 PASS, §4 PASS,
§5 PASS for every changed-code and new-file threshold with two pre-existing repo-wide FAIL rows
carrying a non-blocking disposition, §6 PASS, §7 PASS, §8 two adjudicated findings, both NOT blocking.

No remediation-inputs artifact is produced, because no finding requires remediation before merge. The
non-blocking findings recorded in `code-review.2026-09-07T17-05.md` (CR-1 through CR-3 in particular)
are recommended for promotion to follow-up issues through the potential-to-issue lifecycle rather
than being fixed in this branch, so that they survive the merge of this feature folder.

## Appendix A: Test Inventory

Tests added by this change (10 net new):

| # | Test | File | Criterion |
|---|---|---|---|
| 1 | FormatDeactivationDiagnostics_IncludesEveryDiscriminatingField | QfcFormControllerDeactivateTests.cs | AC6 |
| 2 | FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector | QfcFormControllerDeactivateTests.cs | AC2 (expect-fail row) |
| 3 | FormatDropDownClosedDiagnostics_IncludesEveryDiscriminatingField | BreadcrumbDropDownCloseOrderingTests.cs | AC6 |
| 4 | NativeCloseWhileCommitPending_DoesNotCancelSelection | BreadcrumbDropDownCloseOrderingTests.cs | AC3 (expect-fail row) |
| 5 | NativeCloseWithNoCommitPending_StillCancelsSelection | BreadcrumbDropDownCloseOrderingTests.cs | AC3 scoping guard |
| 6 | GestureOpen_ResolvesOpenAndLeavesHostOpenWithoutClose | BreadcrumbDropDownCloseOrderingTests.cs | AC1 |
| 7 | CloseWhilePendingOpenAndCommitPending_DoesNotCancelSelection | BreadcrumbPendingOpenCloseTests.cs | AC3 |
| 8 | RowSetRefreshWhileOpen_NeverClosesHost | BreadcrumbPendingOpenCloseTests.cs | AC5 (#438 AC-3 guard) |
| 9 | SearchLeaveAfterMouseDrivenOpen_DoesNotCloseDropDown | QfcItemController.SearchLeaveLatchTests.cs | AC4 (expect-fail row) |
| 10 | SearchLeaveAfterSearchDrivenOpen_ClosesDropDown | QfcItemController.SearchLeaveLatchTests.cs | AC4 paired positive |

Tests deliberately updated (2): `FormDeactivated_CancelsSelectorOnEveryItemController` (+1 explicit
genuine-case Arrange line; `Times.Once()` on both controllers retained) and
`TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent` (+1 Arrange line; name and
`Times.Once()` retained).

Tests deliberately NOT changed, serving as scoping guards: the two `CancelCount.Should().Be(1)`
assertions at `BreadcrumbPendingOpenCloseTests.cs` lines 48 and 79, which prove the AC3 cancel
suppression is conditional rather than global, and `FormDeactivated_WebView2Focused_ParksFocusOnce`,
whose non-modification is the recorded `AC2-PARK-FOCUS-SUPPRESSED: NO` decision.

## Appendix B: Toolchain Commands Reference

```
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook"
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput coverage\p9-t6-final.cobertura.xml
```

Read-only commands this reviewer would have run had shell access been permitted, listed so the gap is
auditable: `git diff --name-status a6b25916..HEAD`, `git status --porcelain --untracked-files=all`,
`python scripts/dev_tools/validate_evidence_locations.py --root .`.
