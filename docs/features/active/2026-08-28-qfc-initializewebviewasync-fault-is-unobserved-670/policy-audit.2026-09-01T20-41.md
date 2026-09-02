# Policy Audit — issue #670 (`bug/qfc-initializewebviewasync-fault-is-unobserved-670`)

- **Timestamp:** 2026-09-01T20-41
- **Feature folder:** `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670`
- **Branch:** `bug/qfc-initializewebviewasync-fault-is-unobserved-670`
- **HEAD:** `bb4dbaade9c9a90c0e1e5c61ea78041aa0c1892f`
- **Base (merge base with `origin/main`):** `988d35a8f8eb7436cc46a9f6424db917ed93807a`
- **Work mode:** `full-bug` (marker read at `issue.md:12`) — acceptance-criteria source is `spec.md` only
- **Overall verdict:** PASS — 0 blocking findings

## Base Resolution and Scope

`git merge-base HEAD origin/main` was recomputed in-session and returned
`988d35a8f8eb7436cc46a9f6424db917ed93807a`, matching the anchor supplied to this review. All diffs
below use the three-dot form against that SHA. The plan text pins an older SHA
(`2b85134b42872e405602e6064e02dc9cda6c319b`); that is a previously reported plan defect and is not
re-raised here as a new finding.

The audited scope is the full branch diff against the resolved base. It is not narrowed to any plan,
task, or phase.

### Changed-file inventory (full branch diff, 5 source paths)

| Status | Path |
| --- | --- |
| A | `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` (+41/-0) |
| M | `QuickFiler/QuickFiler.csproj` (+1/-0) |
| M | `QuickFiler/Controllers/QfcItemController.Initialization.cs` (+3/-3) |
| M | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` (+100/-0) |
| M | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` (+52/-0) |

Plus the feature folder (requirements, plan, research, evidence) and ten `.claude/agent-memory/**`
paths. The agent-memory paths originate from the upstream preparation run and the orchestrator's
merge-conflict resolution; they are not raised as scope creep. They were checked for host
identifiers (see Artifact Hygiene below) and are clean.

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and `QuickFiler.Test/QuickFiler.Test.csproj`
have zero changed lines, confirmed by `git diff --stat` returning empty output for both.

## Rejected Scope Narrowing

None. The delegating prompt directed a full-branch audit against the correct merge base, supplied
the complete five-path source footprint, and explicitly instructed that all 14 acceptance criteria be
evaluated. No instruction attempted to limit the audited file set, skip a toolchain check, or mark a
language's coverage as out of consideration. Nothing was rejected under the scope invariant.

The prompt's statement that the ten `.claude/agent-memory/**` paths "are NOT this change's work" is a
provenance attribution, not a scope narrowing: it directs that they not be raised as scope creep
while simultaneously directing that they be inspected for host identifiers. They were inspected.

## Evidence Location Compliance

The branch diff was scanned for files written under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/`, or `artifacts/coverage/`. **Zero occurrences.** All 54 delivery-run evidence
files are under `<FEATURE>/evidence/<kind>/` using canonical kinds only: `baseline`,
`regression-testing`, `qa-gates`, `other`.

The executor recorded a location override in the prescribed form at
`evidence/other/p4-t26-ac14-path-override.md`:

    EVIDENCE_LOCATION_OVERRIDE_REJECTED: .../evidence/coverage/ replaced with .../evidence/baseline/
    (Phase 0 coverage) and .../evidence/qa-gates/ (Phase 4 coverage)

This override is **correct and is endorsed by this audit.** AC14's criterion text names
`evidence/coverage/`, which is not among the canonical kinds enumerated at
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md:15-20` (`baseline`,
`regression-testing`, `qa-gates`, `issue-updates`, `other`, `remediation-baseline`). I verified that
list directly against the skill file. `Test-Path` on the non-canonical directory returns `False`; it
was never created. The criterion text was correctly left unedited, per
`acceptance-criteria-tracking` rule 3.

## Coverage Verification

Coverage was verified by inspecting the committed Cobertura documents. No coverage run was
re-executed and no `artifacts/csharp/coverage.xml` was emitted, per the review directive. Figures
below were parsed independently by this reviewer from the committed XML, not copied from the
executor's records.

### Languages with changed files

| Language | Changed files | Verdict |
| --- | --- | --- |
| C# | 4 `.cs` files (1 added, 3 modified) | see rows below |
| PowerShell | zero `.ps1`/`.psm1` files in the branch diff; no coverage obligation arises | — |
| Python | zero `.py` files in the branch diff; no coverage obligation arises | — |
| TypeScript | zero `.ts`/`.tsx` files in the branch diff; no coverage obligation arises | — |

### C# coverage rows

- **C# repo-wide line coverage: 85.3771% — PASS.** Parsed from
  `evidence/qa-gates/postchange.cobertura.xml` root attributes: `lines-covered=54988`,
  `lines-valid=64406`. Clears the 80% floor (CLAUDE.md §UT2, `.claude/rules/csharp.md`) and the 85%
  floor (`.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`). Because the figure
  clears both, the unresolved divergence between those two authorities does not require settling for
  this issue.
- **C# repo-wide branch coverage: 79.3997% — PASS.** `branches-covered=13120`,
  `branches-valid=16524`. Clears the 75% branch floor.
- **C# new-file line coverage, `QfcItemController.WebViewFaultBoundary.cs`: 92.3077% — PASS.**
  12 of 13 class-level `<line>` nodes covered; the file's own `line-rate` attribute reads
  `0.923077`, corroborating the count. Clears the 90% new-module rule (CLAUDE.md §UT2) and the 85%
  uniform floor. Branch coverage on the file is 100% (`branch-rate=1`).
- **C# modified-file coverage: no regression on changed lines — PASS.** The three substituted call
  expressions at `Initialization.cs:192`, `:288` and `:324` sit inside members carrying explicit
  "#230: de-exempted" comments and no coverage attribute; they are executed by existing tests before
  and after the change.
- **C# baseline-to-post comparison — PASS.** Baseline `lines-covered=54983`, `lines-valid=64393`
  (85.3866%). Absolute covered lines rose by 5. The ratio moved by −0.0095 percentage points, which
  is arithmetic dilution from the denominator growing by 13 (the new file's measurable lines), not a
  loss of covered code. Both documents record `POSTPROCESSED: yes`, so the denominators are
  comparable.

The sole uncovered line in the new file is **line 29**, the closing brace of the `try` block, which
is reached only when `InitializeWebViewAsync()` returns without throwing. The spec states plainly
(Risks, and Test Strategy §Edge cases) that the guard's success path requires a live CoreWebView2
runtime and is therefore not reachable in a unit test. The uncovered line is the documented,
anticipated limitation rather than an untested behavior. Both `catch` arms are covered: line 30
(`catch (OperationCanceledException)`) has `hits=1`, and lines 35-37 (`catch (Exception ex)` and the
sink invocation) have `hits=1`.

Independent-derivation note: an initial aggregation over `.//line` returned 16/17 for the new file.
That figure double-counted line 17, which appears once at class level and again under each of four
`.ctor` method rows. The class-level `lines/line` set is the correct denominator and yields 12/13,
which the file's own `line-rate` attribute independently confirms.

## Toolchain Compliance (CLAUDE.md / `.claude/rules/csharp.md`)

| Stage | Command | Evidence | Verdict |
| --- | --- | --- | --- |
| 1 Format | `dotnet tool run csharpier format .` | `evidence/qa-gates/p4-t1-csharpier-format.md` | PASS |
| 1 Verify | `dotnet tool run csharpier check .` | `evidence/qa-gates/p4-t2-csharpier-check.md` | PASS |
| 2 Analyze | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `evidence/qa-gates/p4-t3-msbuild-analyzers.md` | PASS |
| 3 Type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | `evidence/qa-gates/p4-t4-msbuild-nullable.md` | PASS |
| 4 Test | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (substituted, see PA-1) | `evidence/qa-gates/p4-t5-vstest-coverage.md` | PASS |

Command-fidelity requirements are met: `/t:Rebuild` is used in both msbuild stages, and
`/p:Nullable=enable` is absent from both. Both prohibitions are stated in CLAUDE.md and both were
observed.

### Independent re-verification by this reviewer

I re-ran the read-only formatting gate myself rather than relying on the record:

    dotnet tool restore        -> Tool 'csharpier' (version '1.2.6') was restored.
    dotnet tool run csharpier check .   -> Checked 1567 files in 4686ms.  (exit 0)

The file count matches the executor's 1567 exactly, and no file was named as needing formatting.

### Non-vacuity of the gates

Each gate was checked for whether it is *capable of failing*, not merely whether it reported success:

- **Format:** the same `csharpier check .` command exited 1 earlier in the run against the
  unformatted new file (`evidence/other/p1-t5-new-file-format.md`), which demonstrates the gate
  discriminates. The file count rising from 1566 to 1567 corroborates the added-file set
  independently of the diff.
- **Analyzers:** the build log records 75 `CoreCompile:` target executions, establishing that
  compilation — and therefore analyzer execution — actually occurred. The record correctly notes
  that a warm `/t:Build` would have skipped `CoreCompile` and produced a structurally unfailable
  gate, and correctly used the anchored pattern `: error [A-Z]+[0-9]+:` rather than a bare-word
  `error` search that returns non-zero on every successful build.
- **Type-check:** 67 `CoreCompile:` executions recorded. The five residual warnings are the
  pre-existing uncoded System.Reactive `packages.config` target diagnostic, identical in count to the
  Phase 0 baseline; being uncoded, it is outside the reach of `/p:TreatWarningsAsErrors=true`, which
  is why a five-warning count coexists with zero errors.
- **Test:** the run enumerates three admissible outcomes and identifies which occurred. The summary
  count (6938 passed) is corroborated by an independent per-line count from a different part of the
  output. A control query against a non-existent test name returned 0, establishing that the
  per-test extraction discriminates rather than always matching.

## Artifact Hygiene — Host Identifiers and XML Validity

The plan's sanitisation tasks (P3-T14, P4-T28) mandate angle-bracket placeholders that the test
runner writes into XML attribute values. An absence assertion and a validity assertion fail on
disjoint inputs, so both were run independently.

**Assertion 1 — absence.** Case-insensitive `grep` for `danmoisan`, `megalodon4`, `C:\Users` and
`C:/Users` across the entire feature folder returned **exit 1, zero matches**. The same sweep across
the ten changed `.claude/agent-memory/**` paths also returned **zero matches**. (Pre-existing,
unchanged agent-memory files elsewhere in that tree do contain such tokens; none are modified by this
branch and they are out of scope.)

**Assertion 2 — validity.** All six committed XML documents were loaded via
`System.Xml.XmlDocument.Load`, which rejects any malformed document:

| Document | Result |
| --- | --- |
| `evidence/baseline/baseline.cobertura.xml` | PARSE-OK, root `<coverage>` |
| `evidence/qa-gates/postchange.cobertura.xml` | PARSE-OK, root `<coverage>` |
| `evidence/regression-testing/p3-t10-new-tests.trx` | PARSE-OK, root `<TestRun>` |
| `evidence/regression-testing/p3-t11-pinned.trx` | PARSE-OK, root `<TestRun>` |
| `evidence/regression-testing/p3-t4-green.trx` | PARSE-OK, root `<TestRun>` |
| `evidence/regression-testing/p3-t5-red.trx` | PARSE-OK, root `<TestRun>` |

Both assertions pass on the same inputs because the sanitiser XML-escaped the placeholders. Raw
bytes of `p3-t10-new-tests.trx`:

    <TestRun id="..." name="&lt;user&gt;@&lt;host&gt; 2026-09-01 20:06:59"
             runUser="&lt;host&gt;\&lt;user&gt;" xmlns="...">

Writing a literal `<user>` into an attribute value would have made the document non-well-formed and
Assertion 2 would have failed; leaving the tokens unsubstituted would have failed Assertion 1. The
entity-escaped form satisfies both. Notably `runUser` — the attribute that sanitisers most commonly
miss — is substituted here, as are the `Deployment/@runDeploymentRoot` value and the lowercase
`storage`/`codeBase` path attributes (all covered by the case-insensitive sweep).

## Policy Conformance Summary

| Policy area | Authority | Verdict | Evidence |
| --- | --- | --- | --- |
| Policy reading order observed | `policy-compliance-order` | PASS | `evidence/baseline/phase0-instructions-read.md` |
| 500-line file ceiling | `.claude/rules/general-code-change.md` | PASS | measured, all 5 files ≤ 500 |
| Error handling — no silent swallow | `.claude/rules/csharp.md:27` | PASS | see Broad-Catch Assessment |
| Logging via project pattern | CLAUDE.md §3 | PASS | log4net `ILog.Error(string, Exception)` |
| MSTest + Moq + FluentAssertions | CLAUDE.md §CUT1-2 | PASS | all four new tests |
| No temporary files in tests | `.claude/rules/general-unit-test.md` | PASS | zero filesystem writes in new tests |
| Determinism — no banned API | `.claude/rules/general-unit-test.md` | PASS | see Determinism Assessment |
| Test file location mirrors source | `.claude/rules/general-unit-test.md` | PASS | tests under `QuickFiler.Test/Controllers/` |
| No production file excluded from coverage | `.claude/rules/general-unit-test.md` | PASS | no `[ExcludeFromCodeCoverage]` added, removed, or moved |
| No policy document modified | review constraint | PASS | no `.claude/rules/**` path in diff |
| Tone policy | `.claude/rules/tonality.md` | PASS | requirement and evidence documents are factual and measured |

### File-size measurements (AC11)

| File | Lines | Ceiling | Verdict |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` | 41 | 500 | PASS |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | 489 | 500 | PASS |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 498 | 500 | PASS |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | 261 | 500 | PASS |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (unmodified) | 499 | 500 | PASS |

Counted with `awk 'END{print NR}'` rather than `Measure-Object -Line`, which undercounts.

## Broad-Catch Assessment

`.claude/rules/csharp.md:27` states: "Fail fast with explicit exceptions. Avoid broad
`catch (Exception)` unless **at a defined boundary with added context**."

`InitializeWebViewGuardedAsync` satisfies both conditions of the exemption:

1. **Defined boundary.** The member exists solely to be a fault boundary. The file is named
   `QfcItemController.WebViewFaultBoundary.cs`, the member is named `...GuardedAsync`, and its XML
   documentation states the contract explicitly ("This member contains the fault instead of returning
   it: the task it returns never transitions to Faulted"). It sits at the outermost point of three
   fire-and-forget dispatches, which is the last place a fault can be observed before the task is
   discarded — the definition of a boundary.
2. **Added context.** The catch does not swallow. It routes to
   `WebViewInitializationErrorSink("WebView2 initialization failed.", ex)`, adding a message that
   identifies the failing subsystem and preserving the exception instance. The default sink writes
   through the repository's existing log4net logger using the message-first
   `ILog.Error(string, Exception)` overload that this type already uses elsewhere.

The narrower `catch (OperationCanceledException)` arm is silent, but that is correct rather than a
swallow: cooperative cancellation during teardown is not a fault, the arm carries a comment
explaining why, and the behavior is pinned by a dedicated test. This mirrors the ratified precedent
`EfcFormController.BindBreadcrumbRowsAsync` (issue #464).

**Verdict: PASS.** This is a defined boundary with added context, not an over-broad catch. Catching
`Exception` rather than a narrower type is also the correct choice on the merits, because the fault
sources are heterogeneous — `ObjectDisposedException` from the #488 D5 disposal guard,
`InvalidOperationException` from the D3/D4 guards, `NullReferenceException` from an absent
CoreWebView2 runtime, and WebView2 runtime failures — and enumerating them would risk reinstating the
unobserved-fault defect for any type omitted.

## Determinism Assessment

A sweep of the two changed test files and the new production file for
`Thread.Sleep`, `Task.Delay`, `SpinWait`, `SpinUntil`, `DateTime.Now`, `DateTime.UtcNow`,
`Stopwatch`, `.Wait()`, `.Result` and `WaitOne` returned **zero hits**.

- No new test starts a live Outlook worker or a real WebView2 runtime. `HarnessController` exposes
  the protected parameterless constructor and injects private fields by reflection specifically so
  the members can be exercised "without live WinForms/Outlook infrastructure"
  (`QfcItemController.TestSupport.cs:22-26`). The WebView2 seam is a Moq double raising
  `WebViewSentinelException`.
- Tests 1, 2 and 4 are effectively synchronous: they await the guard directly, with no dispatcher and
  no pump.
- The pump-hosted test's only wait is `await observed.Task`, where `observed` is a
  `TaskCompletionSource<Exception>` constructed with `TaskCreationOptions.RunContinuationsAsynchronously`
  and completed from the sink callback. There is no polling loop and no wall-clock wait. The
  `[Timeout(PumpTimeoutMs)]` attribute is a deadlock backstop, not a wait mechanism, and follows the
  shape of six pre-existing pump tests in the same file.
- `WinFormsPumpHost` was itself swept for `Thread.Sleep`, `Task.Delay`, `SpinWait`, `SpinUntil`,
  `DoEvents` and `while (true)`: **zero hits**.
- The sink is installed during Arrange, before `Initialize(async: false)` is dispatched, which
  forecloses the race the spec's Risks section identifies (a dispatched operation completing before
  `host.InvokeAsync` returns would otherwise miss a sink installed after the Act).

**Verdict: PASS.**

## Bugfix-Workflow Compliance (RED-first)

CLAUDE.md's bugfix workflow requires a failing regression test first. The literal form is
unsatisfiable here: a test referencing `InitializeWebViewGuardedAsync` and
`WebViewInitializationErrorSink` before those members exist fails to *compile*, and a non-compiling
assembly is not a usable red signal. The spec anticipated this and specified a substitute at
Test Strategy §"Bugfix-workflow sequencing (RED step)".

The substitute was delivered as a genuine discriminating pair
(`evidence/regression-testing/p3-t4-green-run.md`, `p3-t5-red-run.md`, `p3-t6-restored-green-run.md`):

| Run | Guard's `catch (Exception ex)` arm | vstest exit | TRX outcome |
| --- | --- | --- | --- |
| P3-T4 | `WebViewInitializationErrorSink("WebView2 initialization failed.", ex);` | 0 | 1 passed |
| P3-T5 | `_ = ex;` | 1 | 1 failed |
| P3-T6 | restored | 0 | 1 passed |

Two properties make this a sound RED step rather than a formality:

- **The mutation is behavioural, not structural.** The failure message is
  `Expected captured to be ...WebViewSentinelException ... but found <null>`. The first assertion
  (`NotThrowAsync`) still *passes* under the mutation, because the `try`/`catch` is still present.
  Only the sink-invocation assertion fails. This proves the test is sensitive to the observation
  behaviour specifically, not merely to the presence of a `catch`.
- **Staleness was excluded.** `BUILD_START_UTC` (2026-09-02T00:01:21.99Z) precedes `DLL_WRITE_UTC`
  (2026-09-02T00:01:30.16Z), so the assembly under test carried the mutation. Without this check the
  red result would be equally consistent with an unrelated failure against an old binary.

**Verdict: PASS.**

## Findings

No blocking findings. Non-blocking items are recorded in
`code-review.2026-09-01T20-41.md` (CR-1 through CR-4) and below (PA-1, PA-2). No
`remediation-inputs` artifact is produced, because remediation is not required.

### PA-1 — AC10 stage-4 command substitution (Non-blocking, disclosed)

AC10 names stage 4 as `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. The delivery
substituted `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which invokes `dotnet-coverage collect`
around that same `vstest.console.exe` binary over the same nine assemblies.

Assessment: the substitution is necessary rather than convenient. `/EnableCodeCoverage` emits a
binary `.coverage` file, from which neither AC13's per-file line figure nor AC14's comparable
repository-wide counter set can be read without a conversion step; the runner produces the Cobertura
document both criteria depend on. The substitution is disclosed in the evidence record with that
rationale rather than presented as literal compliance. The substance of AC10 — one uninterrupted
four-stage pass, in order, with no stage failing and no stage rewriting a file — is satisfied.
Recorded as a deviation, not a defect.

### PA-2 — Fourth test placed outside the spec's enumerated file table (Non-blocking)

The spec's "In scope" section and its Files/modules table name **three** new tests, all in
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`. The delivery adds
**four** tests; the fourth,
`InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink`, plus the shared
`BuildGuardedWebViewTarget` arrange helper, landed in
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` — a file absent from the
spec's table and from AC11's enumeration.

Assessment: the *test* is authorized in substance. The spec's Test Strategy §"Edge cases and negative
scenarios" explicitly contemplates it: "`OperationCanceledException` — swallowed without reaching the
sink; ... if the planner elects to add that arm." It covers the `catch (OperationCanceledException)`
branch that AC3 requires and that no other test exercises; the Cobertura data confirms line 30 has
`hits=1` as a result. Only the *file placement* falls outside the spec's enumeration, and the
practical reason is visible in the measurements: `Part3.cs` finished at 498 of 500 lines, leaving no
room for a fourth test. The un-enumerated file is at 261 lines and comfortably satisfies the ceiling,
so AC11's substantive requirement holds on it as well. Coverage-positive and within the spec's stated
intent; recorded for transparency rather than as a defect.

## Assumptions and Deviations by This Review

- **PR context artifacts were not regenerated.** `artifacts/pr_context.summary.txt` and
  `artifacts/pr_context.appendix.txt` do not exist in this worktree. Rather than synthesise them,
  scope was derived directly from `git diff 988d35a8...HEAD`, which is the authoritative legitimate
  scope source named in this agent's scope invariant (the resolved base branch) and is strictly more
  reliable than a generated summary — the summary format is known to misclassify `.cs` paths. The
  five-path source footprint derived this way matches the directive exactly.
- Per the review directive, no `artifacts/csharp/coverage.xml` was emitted and no coverage run was
  re-executed; coverage was verified from the committed Cobertura documents.
- No helper script was written under `<FEATURE>/evidence/`; the three parsing scripts used for the
  XML and coverage assertions were written to the session scratchpad.
- `.git/info/exclude` was not edited. `artifacts/orchestration/orchestrator-state.json` was not
  modified. No push, PR, or merge was performed.
