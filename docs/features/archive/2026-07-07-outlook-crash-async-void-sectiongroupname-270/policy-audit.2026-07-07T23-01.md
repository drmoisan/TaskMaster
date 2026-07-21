# Policy Compliance Audit — Issue #270 (outlook-crash-async-void-sectiongroupname)

- Timestamp: 2026-07-07T23-01
- Reviewer: feature-reviewer
- Work mode: minor-audit (from `issue.md` `- Work Mode: minor-audit`)
- Base branch: `main`
- Merge-base SHA: `82f89f2bd90b6456eb2fd2639eb2d5bc05eec999` (independently recomputed via `git merge-base HEAD origin/main`; matches the caller-supplied value)
- Head SHA: `d3ed469f1e72d37f61ba7089a759e6bcbdd7c337`
- Diff range audited: `82f89f2b..d3ed469f` (full branch diff vs base)

## Executive Summary

This is a C# defect fix. Two `async void` Outlook COM event handlers in
`TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` were changed from
`catch (System.Exception) { throw; }` (which rescheduled a recoverable settings/config
fault onto the ThreadPool and terminated `outlook.exe`) to a log-and-contain pattern via
the existing `logger`. An injectable-delegate seam was added to make the fault-containment
path deterministically unit-testable without a live Outlook process. Two new MSTest
regression tests were added; static test helpers were extracted to a new partial-class file
to stay under the 500-line limit; one pre-existing test that asserted the old rethrow
behavior was corrected; and a `<Compile Include>` entry was added for the new test file.

Overall verdict: PASS. The change adheres to CLAUDE.md, the general code-change and
unit-test rules, and the C# code-change / unit-test policies. The full C# toolchain is
green in committed evidence (format, analyzers, nullable/type-check on touched projects,
MSTest 202/202). Coverage on changed lines does not regress and the new testable methods
exceed the 90% new-code floor. No blocking findings.

One non-blocking process observation: the canonical machine-readable coverage artifact
`artifacts/csharp/coverage.xml` is not present at the SKILL-specified path; C# coverage
was verified from committed Cobertura-derived evidence in the feature `evidence/` folder
(see Section 1.2). This is recorded as a Low-severity recommendation, not a blocker,
because coverage is fully verifiable from committed evidence.

## Data Provenance and PR-Context Correction

`artifacts/pr_context.summary.txt` misclassifies this change. Its "Changed files overview"
reports `Core logic changes: 0 files` and lists only Markdown files under
`Docs/templates/agents/tooling: 24 files`. This is the known recurring C#-as-docs
misclassification in the PR-context summary generator. The real branch diff (from
`git diff --stat 82f89f2b..d3ed469f`) includes C# production and test changes:

- `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` (production, +70/-16 region)
- `TaskMaster.Test/AppGlobals/AppEventsTests.cs` (test, split/refactor + 2 new tests)
- `TaskMaster.Test/AppGlobals/AppEventsTests.Helpers.cs` (new test helper file)
- `TaskMaster.Test/AppGlobals/AppEventsCoverageExpansionTests.cs` (test correction)
- `TaskMaster.Test/TaskMaster.Test.csproj` (compile-include wiring)

Per the caller instruction and the SKILL scope invariant, this audit was authored from the
real git diff, not from the summary overview. The `.cs` changes are treated as changed
files for coverage purposes.

## Rejected Scope Narrowing

None. The caller directed a full feature-vs-base audit with no narrowing and explicitly
instructed disregarding the unreliable summary lines (which broadens, not narrows, scope).
No caller instruction attempted to limit scope to a plan subset, a file subset, or to mark
any language as out of scope.

## Section 1 — Coverage Verification

### 1.1 Changed languages in branch diff

- C#: changed files present (production + test). Coverage verdict required: yes.
- TypeScript / Python / PowerShell: zero changed files on the branch. Not applicable.

### 1.2 C# coverage

Coverage was verified by inspecting committed, execution-produced evidence rather than
rerunning generation:

- `evidence/qa-gates/test-final.2026-07-07T22-50.md` (VSTest `/EnableCodeCoverage`, EXIT 0,
  202/202 pass; `.coverage` merged to Cobertura via `dotnet-coverage merge -f cobertura`).
- `evidence/qa-gates/coverage-delta.2026-07-07T22-50.md` (baseline-vs-post-change delta).
- Baseline reference: `evidence/baseline/test-baseline.md`.

Per-scope figures:

| Scope | Baseline | Post-change | Change | Disposition |
|---|---|---|---|---|
| `TaskMaster` production package (line) | 63.64% | 64.07% | +0.43 pt | PASS — improved, no regression; COM/VSTO exemption applies (below) |
| `AppEvents.ReadinessHookup.cs` (file line) | 66.67% | 65.52% | -1.15 pt | PASS — movement is added-denominator lines, not previously-covered lines becoming uncovered |
| `HandleInboxItemAddAsync` (new core method) | n/a (new) | 100.00% | new | PASS — >= 90% new-code floor |
| `HandleToDoItemChangeAsync` (new core method) | n/a (new) | 92.86% | new | PASS — >= 90%; sole uncovered line is the production default-collaborator COM lambda |
| `OlInboxItems_ItemAdd` (thin async-void wrapper) | — | 100.00% | — | PASS |
| `OlToDoItems_ItemChange` (thin async-void wrapper) | 0.00% | 0.00% | 0 | Host-bound wrapper, COM-exempt; no regression |

C# coverage verdict: **PASS**.

Rationale on the repo/package line figure (64.07%, below the nominal 80%/85% floor): the
`TaskMaster` assembly is a VSTO Outlook add-in and `AppEvents` is an Outlook Interop event
handler class, which is explicitly within the ratified COM/VSTO/WinForms coverage exemption
in CLAUDE.md (testable-denominator floor; Interop event handler classes in `TaskMaster` are
formally exempted). The exemption is authoritative in CLAUDE.md, which sits first in the
policy-compliance order. This change does not degrade the package figure — it improves it by
+0.43 pt. The operative, non-exempt requirement (AC5 changed-line no-regression and the
>= 90% new-code floor for the two new testable methods) is satisfied: 100% and 92.86%.

New/changed-code coverage: 100.00% (`HandleInboxItemAddAsync`) and 92.86%
(`HandleToDoItemChangeAsync`); both above the 90% new-code line floor.

Coverage-artifact-location note (Low): the canonical `artifacts/csharp/coverage.xml`
(SKILL-specified path; also the path the `validate-feature-review-coverage.ps1` hook reads
for the numeric C# gate) is absent. The generated `.coverage` binary referenced in
`test-final` is a local, uncommitted TestResults file. Recommendation: emit the merged
Cobertura to `artifacts/csharp/coverage.xml` so the machine-readable C# gate has an artifact
to parse. This is not a blocker because coverage is fully verified from the committed
evidence above.

## Section 2 — C# Toolchain Compliance (order: format -> analyze -> type-check -> test)

Verified from committed QA-gate evidence (2026-07-07T22-50 final set):

| Stage | Command (evidence) | Result | Verdict |
|---|---|---|---|
| Format | `dotnet tool run csharpier format .` | EXIT 0; no reformatting churn on touched files | PASS |
| Analyze | `msbuild TaskMaster.sln -t:Build ... -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` | EXIT 0; zero new warnings on touched files | PASS |
| Type-check (nullable) | `msbuild TaskMaster.sln -t:Rebuild ... -p:Nullable=enable -p:TreatWarningsAsErrors=true` | Solution EXIT 1 = pre-existing vendored debt only (84 errors, byte-identical to baseline: SVGControl 34, UtilitiesSwordfish 50); touched-project incremental nullable build EXIT 0, zero new diagnostics on touched files | PASS |
| Test | `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /EnableCodeCoverage` | EXIT 0; 202/202 pass | PASS |

The solution-level nullable Rebuild EXIT 1 is the documented pre-existing baseline of vendored
errors (SVGControl, UtilitiesSwordfish) unrelated to this change. Attribution in
`typecheck-final.2026-07-07T22-50.md` shows zero errors cite `AppEvents.ReadinessHookup.cs`
or any `AppEventsTests` file; the two CS8625 diagnostics that do cite the production file are
in the pre-existing `Unhook()` method (lines 18-23), outside this change's diff (hunk begins
at line 60). The `?`-annotated seam properties are wrapped in a narrow
`#nullable enable annotations` / `#nullable restore annotations` context, which keeps them
CS8632-clean under the analyzer build and correct under the nullable build.

Toolchain verdict: **PASS** (touched-file scope; no new diagnostics; ordered gates green).

## Section 3 — General Code Change Policy

| Rule | Assessment | Verdict |
|---|---|---|
| Simplicity first | Minimal, targeted fix; thin async-void wrappers delegate to host-neutral core methods holding the try/catch | PASS |
| Separation of concerns | Fault-containment logic extracted into host-neutral `HandleInboxItemAddAsync` / `HandleToDoItemChangeAsync`; COM wiring isolated to the thin wrappers and default-collaborator lambdas | PASS |
| Error handling — fail fast / boundary catch | The `catch (Exception)` is at a defined boundary (top of a COM async-void event handler) and adds context (descriptive message + full exception object) rather than silently swallowing. Correct boundary-catch usage per general-code-change and csharp.md | PASS |
| Logging | Uses the existing `logger.Error(message, ex)` pattern; no ad-hoc console output | PASS |
| File size <= 500 lines | Production file 141 lines; `AppEventsTests.cs` reduced from 500 (baseline, at the ceiling) to 329; new `AppEventsTests.Helpers.cs` 255; `AppEventsCoverageExpansionTests.cs` 176. All under 500 | PASS |
| Naming / XML docs | XML doc comments added to seam properties and core methods explaining why (issue #270, ThreadPool termination); "why" comments present | PASS |
| Public surface minimal | Seam members are `internal`, not `public` | PASS |

## Section 4 — Bugfix Workflow

| Step | Assessment | Verdict |
|---|---|---|
| Failing regression test first | `evidence/regression-testing/fail-before.2026-07-07T22-18.md` shows the two new tests fail against the pre-fix `catch { throw; }`; `pass-after.2026-07-07T22-20.md` shows they pass after the fix | PASS |
| Minimal, targeted fix | Production change confined to the single handler file; RibbonViewer handlers and the proximate config trigger left as documented follow-ups | PASS |
| Verify locally (full toolchain in order) | Section 2 above | PASS |

## Section 5 — Unit Test Policy (General + C#)

| Rule | Assessment | Verdict |
|---|---|---|
| Framework: MSTest | `[TestClass]`/`[TestMethod]`/`[TestMethod] async Task` used | PASS |
| Mocking: Moq | `Mock<IApplicationGlobals>` (Strict) via helpers | PASS |
| Assertions: FluentAssertions | `.Should().NotThrowAsync()`, `.Should().ContainSingle(...)`, `.Should().BeNull()` | PASS |
| Determinism | In-memory `MemoryAppender`; `ReferenceEquals` on injected exception; no wall-clock, no RNG | PASS |
| No external deps / no temp files | Injected throwing delegate; no COM, network, filesystem, or temp files (verified by grep — no `Thread.Sleep`/`Task.Delay`/`DateTime.Now`/temp-file APIs in added test lines) | PASS |
| Arrange-Act-Assert | Explicit Arrange/Act/Assert comments in the two new tests | PASS |
| Tests as spec | Pre-existing `OlInboxItemsItemAdd_..._RethrowsThroughSynchronizationContext` correctly renamed/updated to `..._ContainsAndDoesNotRethrow` to encode the new (correct) contract; it fails against the old rethrow behavior | PASS |
| Byte-equivalent helper move | `AppEventsTests.Helpers.cs` is a verbatim extraction of the private static helpers; partial class in the same namespace with the required usings | PASS |

## Section 6 — DI Seam Standard (csharp.md)

The change uses an injectable-delegate seam (`internal Func<object, Task>?` collaborator
properties, null-coalesced to the production call) — option 2 in the csharp.md DI-seam
preference order, with a safe deterministic default. This is the smallest seam that enables
reliable unit testing of the fault-containment path without a full interface. **PASS.**

## Section 7 — Architecture Boundaries and Evidence Location

- No-COM architecture assertions: these apply to new backend/host-neutral runtime code. This
  change is a bug fix inside pre-existing legacy VSTO code that already depends on Outlook
  Interop; it introduces no new COM dependency (the host-neutral core methods take `object`
  and a delegate). Not a violation. Verdict: PASS (not applicable to legacy fix scope).
- `modified-workflow-needs-green-run`: no paths under `.github/workflows/**`,
  `scripts/benchmarks/**`, or `.github/actions/**` were modified. Rule does not fire.
- Evidence location compliance: see Section 8.

## Section 8 — Evidence Location Compliance

Scanned the branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/`, or `artifacts/coverage/`:

- `git diff --name-only 82f89f2b..HEAD | grep -E '^artifacts/(baselines|qa|evidence|coverage)/'`
  returned NONE.

All feature evidence is written under the canonical
`docs/features/active/2026-07-07-outlook-crash-async-void-sectiongroupname-270/evidence/<kind>/`
tree (baseline, qa-gates, regression-testing, issue-updates). No non-canonical evidence
paths detected. (`scripts/dev_tools/validate_evidence_locations.py` is not present in this
repository; the scan was performed directly against the diff file list.) Verdict: PASS.

## Section 9 — Verdict Summary

| Area | Verdict |
|---|---|
| C# coverage (changed-line, new-method, no-regression) | PASS |
| C# toolchain (format/analyze/type-check/test, in order) | PASS |
| General code-change policy | PASS |
| Bugfix workflow | PASS |
| Unit-test policy (general + C#) | PASS |
| DI seam standard | PASS |
| Architecture boundaries / workflow rule | PASS |
| Evidence location compliance | PASS |

Blocking findings: 0. Non-blocking recommendations: 1 (emit `artifacts/csharp/coverage.xml`).

## Appendix A — Coverage Checklist

- TypeScript coverage: N/A — zero `.ts`/`.tsx` files changed on the branch.
- Python coverage: N/A — zero `.py` files changed on the branch.
- PowerShell coverage: N/A — zero `.ps1`/`.psm1` files changed on the branch.
- C# coverage: PASS — verified from committed Cobertura-derived evidence; changed-line and
  new-method coverage above floors, repo-package figure improved and under the ratified
  COM/VSTO exemption. Baseline: 63.64%; Post-change: 64.07%; Disposition: PASS (no regression).

## Appendix B — Command Reference

Review was evidence-verification (no mutation). Commands used by this reviewer:

- `git merge-base HEAD origin/main` -> `82f89f2bd90b6456eb2fd2639eb2d5bc05eec999`
- `git diff --stat 82f89f2b..HEAD`
- `git diff 82f89f2b..HEAD -- TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs`
- `git diff 82f89f2b..HEAD -- TaskMaster.Test/...`
- `awk 'END{print NR}'` for line counts (head vs `git show <base>:<path>`)
- `git diff --name-only 82f89f2b..HEAD | grep -E '^artifacts/(baselines|qa|evidence|coverage)/'`
- grep for banned/nondeterministic test APIs in added test lines

Executor toolchain commands (referenced from committed evidence, not rerun by reviewer):

- `dotnet tool run csharpier format .` (format)
- `msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` (analyze)
- `msbuild TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true` (type-check)
- `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /EnableCodeCoverage` (test + coverage)
