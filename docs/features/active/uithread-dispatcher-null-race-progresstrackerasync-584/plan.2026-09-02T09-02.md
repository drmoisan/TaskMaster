# uithread-dispatcher-null-race-progresstrackerasync (Plan)

- **Issue:** #584
- **Work Mode:** full-bug (AC source is `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md`; no `user-story.md` exists or is expected)
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T09-02
- **Status:** Ready for preflight (revision round 8; findings D1-D11 from preflight round 1, B1-B3 and NB1-NB6 from preflight round 2, C1-C3 and N1-N2 from preflight round 3, E1-E3 and N-1/N-2/N-3 from preflight round 4, F1 from preflight round 5, and G1 plus O1-O4 from preflight round 6 applied, revision round 9 (backtick-removal presentation fix for the parallel-scheduling blast-radius harvester) applied)
- **Version:** 1.7
- **Branch:** `bug/uithread-dispatcher-null-race-progresstrackerasync-584`
- **BASE (merge base with `origin/main`):** `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1`

**Fail-closed evidence rule:** Every baseline, QA-gate, regression, and coverage-comparison task below
names its artifact path. If a required artifact is missing or is missing any of `Timestamp:`,
`Command:`, `EXIT_CODE:`, `Output Summary:`, the outcome is BLOCKED or INCOMPLETE, never PASS.

**Evidence location invariant:** all evidence for this item is written under
`docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/` in the
sub-kinds `baseline/`, `regression-testing/`, `qa-gates/`, `issue-updates/`, and `other/`. No evidence
is written under `artifacts/`.

---

## Scope: files this plan's diff writes

Production and test source (exactly five files):

- `UtilitiesCS/Threading/UiThread.cs` — the `Dispatcher` accessor and its backing field (P2-T1).
- `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — one added `using`, one added `[TestClass]`
  (P1-T1, P1-T2).
- `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` — attribute-only addition (P1-T5).
- `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` — attribute-only addition (P1-T5).
- `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` — attribute-only addition (P1-T5).

The last three carry an attribute-only change and nothing else: no assertion, no test body, no
`using`, and no member in those files is added, removed, or reordered. That constraint is what keeps
them compatible with AC4's "unmodified assertions" wording; P1-T5 states the enforceable form of it.

### Why three additional test files are in scope (re-derived this pass)

`UtilitiesCS.Test/Properties/AssemblyInfo.cs` line 18 declares `[assembly: Parallelize(`, so classes
in this assembly run concurrently by default. Exactly three files in `UtilitiesCS.Test` reflect over
the process-global `UiThread._dispatcher` backing field and write it:
`UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` line 144,
`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` line 138, and
`UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` line 422. Re-derived this pass with
`git grep -n -F '"_dispatcher"' -- UtilitiesCS.Test`, which returns exactly those three lines in
exactly those three files. None of them is among the 18 files in `UtilitiesCS.Test` that already
carry `[DoNotParallelize]`.

Marking only the new test class `[DoNotParallelize]` would rest the new class's determinism on an
assumption about how the MSTest adapter orders its parallel and non-parallel buckets — an assumption
this plan cannot verify against the repository tree. This plan instead states the isolation guarantee
in a form that is verifiable against the tree by a single command: **after P1-T5, zero writers of
`UiThread._dispatcher` remain in the parallel bucket**, because every file in `UtilitiesCS.Test` that
names that field carries the `DoNotParallelize` attribute on its test class. That is what P1-T5
asserts and what P0-T13 baselines. It also removes, as a side effect, the concurrent-writer hazard that the new
negative test would otherwise share with the two existing tests that install a real dispatcher and
pump a `DispatcherFrame` (`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` lines 150, 152,
and 162).

### Pre-existing file-size overage recorded, not deepened

`UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` is 514 lines at BASE, already above the
500-line limit in the rule file .claude/rules/general-code-change.md. That overage is pre-existing
and is not caused by this change. To avoid deepening it, P1-T5 adds the attribute to that one file by
extending its existing attribute list on line 14 to `[TestClass, DoNotParallelize]` rather than
adding a line. The other two files have ample headroom (347 and 205 lines) and use the repository's
prevailing two-line idiom, which is `[TestClass]` on one line and `[DoNotParallelize]` on the next
(the existing pattern in UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs, lines 15-16,
re-derived this pass).

Documentation and evidence written by this plan:

- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/issue-updates/`
- `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/`

Explicitly NOT written by this plan (verified this pass, see P0-T3 and P3-T4):
UtilitiesCS/Threading/ProgressTrackerAsync.cs, UtilitiesCS.Test/UtilitiesCS.Test.csproj,
UtilitiesCS/UtilitiesCS.csproj, and anything under the Claude runtime tree at .claude/, the Codex
mirror tree at .codex/, the dot-agents tree at .agents/, config/blast-radius.json, or
config/orchestration-routing.json.

### Test-file placement decision (made this pass, not deferred to the executor)

The new regression test goes into the EXISTING file `UtilitiesCS.Test/Threading/UiThread_Tests.cs`,
as a second `[TestClass]` alongside the existing `SynchronizationContextAwaiter_Tests`. Two
re-derived facts fix this decision:

1. The file is currently 104 lines. The addition specified in P1-T2 is approximately 75 lines, so the
   post-change file is approximately 180 lines — under the 500-line limit in the rule file
   .claude/rules/general-code-change.md. The alternative file
   UtilitiesCS.Test/Threading/UiThread_Dispatcher_Tests.cs is therefore NOT created.
2. UtilitiesCS.Test/UtilitiesCS.Test.csproj line 493 already carries
   `<Compile Include="Threading\UiThread_Tests.cs" />`. This project uses explicit `Compile Include`
   wiring, so reusing the existing file requires no `.csproj` edit, whereas a new file would. The
   three files P1-T5 touches are wired at the same project's lines 476
   (`Threading\ProgressTracker_Tests.cs`), 478 (`Threading\ProgressTrackerAsync_Tests.cs`), and 489
   (`Threading\IdleAsyncQueue_Tests.cs`), all re-derived this pass, so no `.csproj` edit is required
   for them either and UtilitiesCS.Test/UtilitiesCS.Test.csproj stays out of this plan's diff.

---

## Threshold reconciliation (recorded, applied)

`CLAUDE.md` (rank 1 in `policy-compliance-order`) sets repository line coverage `>= 80%` and new
module/class/method coverage `>= 90%`. The rule files .claude/rules/general-unit-test.md and
.claude/rules/quality-tiers.md (rank 3/4) set `>= 85%` line and `>= 75%` branch. This plan applies
the rank-1 `CLAUDE.md` figures (`>= 80%` repository line, `>= 90%` new code) and records the
divergence in P0-T12 rather than silently choosing one. The conflict is pre-existing and is NOT
resolved by this bug fix.

The enforced repository-level gate in this plan is **no regression relative to the P0 baseline**
(P4-T7). The absolute `>= 80%` figure is recorded, not gated, because the baseline is measured, not
assumed: a floor asserted against an unmeasured baseline could be unsatisfiable for reasons this
change did not cause.

---

## Acceptance criteria mapping (source: `spec.md` "## Acceptance Criteria", AC1-AC7)

Each row below lists exactly the same evidence-artifact set as the corresponding `AC-MAPPING:` line
in the Planner Internal Review record at the end of this file. The two lists are one derivation, not
two.

| AC | Implementation task | Test task | Evidence task |
|---|---|---|---|
| AC1 | P2-T1 | P1-T2, P1-T4, P3-T2 | `evidence/regression-testing/p1-t4-expect-fail.md`, `evidence/regression-testing/p3-t2-regression-green.md` |
| AC2 | P2-T1 | P2-T2, P4-T4 | `evidence/qa-gates/p2-t2-nullforgiving-removed.md`, `evidence/qa-gates/p4-t4-nullable-build.md` |
| AC3 | P0-T3 (verification, no edit) | P3-T4 | `evidence/other/p3-t4-progresstrackerasync-unmodified.md` |
| AC4 | P1-T5 (attribute-only, no assertion changed) | P3-T3, P3-T6 | `evidence/qa-gates/p1-t5-donotparallelize.md`, `evidence/regression-testing/p3-t3-at-risk-tests.md`, `evidence/regression-testing/p3-t6-quickfiler-wpfuidispatcher.md` |
| AC5 | P1-T2, P1-T5, P2-T1 | P3-T5 | `evidence/qa-gates/p3-t5-no-timing-tokens.md` |
| AC6 | P4-T1, P4-T3, P4-T4 | P4-T5, P4-T6 | `evidence/qa-gates/p4-t1-format.md`, `evidence/qa-gates/p4-t2-format-check.md`, `evidence/qa-gates/p4-t3-analyzer-build.md`, `evidence/qa-gates/p4-t4-nullable-build.md`, `evidence/qa-gates/p4-t5-utilitiescs-tests.md`, `evidence/qa-gates/p4-t6-quickfiler-tests.md`, `evidence/qa-gates/p4-t8-loop-closure.md` |
| AC7 | P2-T1 | P4-T5, P4-T7 | `evidence/baseline/p0-t10-utilitiescs-tests-coverage.md`, `evidence/qa-gates/p4-t7-coverage-delta.md` |

---

## Exact source text this plan will create (quoted verbatim for gate exoneration)

The `Dispatcher` property region of `UtilitiesCS/Threading/UiThread.cs` (currently lines 135-140)
becomes exactly this, subject only to `csharpier` re-wrapping:

```csharp
        public static Dispatcher Dispatcher
        {
            get
            {
                if (_dispatcher is null)
                {
                    throw new InvalidOperationException(
                        "The UI dispatcher has not been captured. Call UiThread.Init() so that UiThread.Initialize() runs before reading UiThread.Dispatcher."
                    );
                }
                return _dispatcher;
            }
            private set => _dispatcher = value;
        }
        private static Dispatcher? _dispatcher;
```

The literal token asserted by P2-T2 is, verbatim and on one source line:
`private static Dispatcher? _dispatcher;`

The literal token whose disappearance P2-T2 asserts is, verbatim: `null!`

The two MSTest node identifiers created by P1-T2 are, verbatim:

- `UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`
- `UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance`

The attribute text P1-T5 creates is quoted verbatim here so that the gate reads this quotation as the
executor's instruction rather than as a search for a literal that is absent from the tree today.

In `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, lines 28-29 become exactly:

```csharp
    [TestClass]
    [DoNotParallelize]
```

In `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, lines 13-14 become exactly:

```csharp
    [TestClass]
    [DoNotParallelize]
```

In `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, line 14 becomes exactly this single line,
so that the file's length does not grow past its pre-existing 514:

```csharp
    [TestClass, DoNotParallelize]
```

The single-line token asserted by P1-T5 against all four files is, verbatim: `DoNotParallelize`.
That bare token is used deliberately rather than the bracketed spelling, because it matches both the
two-line idiom and the combined attribute list above.

---

## Shell constraints measured in this worktree (binding on every command block below)

Every command block in this plan runs through the executing agent's POSIX (git-bash) shell. Five
constraints of that shell and of the tools it invokes were measured directly in this worktree by the
reviewing executor during preflight rounds 3, 4, 5, and 6, by running the commands rather than by
reading documentation. They are recorded once here and are the reason several command shapes and
several evidence-sourcing rules below are spelled the way they are. They are environment
measurements, not repository-tree citations.

One further rule, the "TRX selection rule", is stated at the end of this section after constraint 5.
It is a plan rule rather than an environment measurement, and it is placed here because it binds on
the same seven tasks constraint 5 binds on and because stating it once centrally is what keeps it
from being present in some of those tasks and absent from others.

1. **A command whose name is `pwsh` is refused outright, in every argument shape.** The refusal is
   keyed on `pwsh` occupying the command position, not on `-Command` versus `-File`. The verbatim
   refusal text is: "this command runs pwsh in a plain command; what it reads or is handed as shell
   text cannot be shown not to run git. Refusing to run it." Consequence: no task in this plan may
   invoke `pwsh`, and the script scripts/vscode/Install-RepoDotNetSdk.ps1 cannot be run from this
   shell in any form. P0-T5 step 1 performs the same download-and-extract with POSIX utilities
   instead. No `pwsh`
   invocation appears anywhere in this plan.

2. **A command whose NAME is a quoted absolute path is refused** ("runs a command whose name is
   computed at runtime in a plain command ... Refusing to run it"). A path used as an ARGUMENT — after
   `--`, or as the value of a parameter — is not refused. Neither `vswhere.exe` nor
   `vstest.console.exe` resolves on `PATH` by bare name. Consequence: every task that runs
   `vstest.console.exe` leads its command line with a `PATH=` prefix so that the command NAME is the
   bare filename `vstest.console.exe`, and the two `dotnet-coverage collect` tasks keep the executable
   as a double-quoted ARGUMENT after `--`, which needs no prefix. The measured proof of the prefix
   form is in P0-T5 step 2.

3. **Bare `msbuild` does not resolve on `PATH` in this shell; `msbuild.exe` does.** Measured:
   `msbuild -version` returns `command not found` with exit 127, and `msbuild.exe -version` exits 0
   printing `MSBuild version 18.9.1+a81b43525 for .NET Framework`. This is a git-bash PATH-resolution
   property — MSYS bash does not append `.exe` when searching `PATH` for a bare name — and not a
   deviation from `CLAUDE.md`. Every switch set below is character-for-character the one `CLAUDE.md`
   mandates: no `/p:Nullable=enable` is ever added, and `/t:Rebuild` is always used in place of
   `/t:Build`.

4. **MSYS path conversion rewrites forward-slash switches, and is disabled per command line with a
   leading `MSYS_NO_PATHCONV=1` assignment.** This constraint supersedes an earlier round's narrower
   claim that only the executable SPELLING had to change; that claim was wrong, and it was wrong in a
   way that reading the commands could not reveal. Measured in this worktree during preflight round 4
   by running the commands:

   - `msbuild.exe` receives mangled switches. A single-letter `/m` is converted to `M:/`, `/t:Rebuild`
     to `t:Rebuild`, and `/p:...` to `p:...`, so MSBuild sees several bare operands and fails with
     `MSB1008: Only one project can be specified.` All six `msbuild.exe` command blocks in this plan
     failed this way when run without the prefix.
   - `vstest.console.exe` receives a mangled switch whenever a multi-letter `/Switch` carries NO
     colon. `/InIsolation` is converted to the msys-root path
     `C:/Program Files/Git/InIsolation`, which vstest then treats as a test source, reporting
     `The test source file ... was not found` and running zero tests. All eight vstest and
     `dotnet-coverage` invocations in this plan failed this way when run without the prefix.
     Colon-bearing switches such as `/Logger:trx`, `/ResultsDirectory:...`, `/Settings:...`, and
     `/TestCaseFilter:...` are NOT converted, which is precisely why three rounds of reading these
     command lines did not surface the defect: the only affected switch in the whole set is the one
     without a colon.
   - `msbuild.exe -version` in P0-T5 step 3 needs no prefix and does not carry one. Its only argument
     begins with `-`, not `/`, so nothing is converted. This was verified separately.

   The remedy applied throughout this plan is a single leading environment-variable assignment,
   `MSYS_NO_PATHCONV=1 `, placed on the same command line immediately before the executable name. It
   changes no switch. Where a command line already carries a `PATH=` prefix, `MSYS_NO_PATHCONV=1`
   is placed BEFORE it; the shell accepts any number of leading assignments on one command. The
   fourteen command blocks carrying the prefix are P0-T8, P0-T9, P1-T3, P3-T1, P4-T3, and P4-T4
   (`msbuild.exe`), and P0-T10, P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, P4-T5, and P4-T6 (vstest and
   `dotnet-coverage`). Because the prefix is part of the recorded command line, the acceptance clauses
   in P0-T9 and P4-T4 assert that the recorded line CONTAINS `msbuild.exe TaskMaster.sln` rather than
   that it begins with it. The substantive clauses those gates enforce — no `/p:Nullable=enable`, and
   `/t:Rebuild` rather than `/t:Build` — are unaffected and unchanged.

5. **A green `vstest.console.exe` run prints no `Failed:` line and no `Skipped:` line on its console
   output.** Measured across four green runs in this worktree during preflight round 5 — of 4783,
   1312, 41, and 2 tests, with and without `/Settings:`, and both under and outside
   `dotnet-coverage collect`. The entire summary block a successful run emits is:

   ```text
   Test Run Successful.
   Total tests: 4783
        Passed: 4783
    Total time: 12.8452 Seconds
   ```

   A search of the captured output of each of those four runs for `Failed:` or `Skipped:` returned
   zero matches. EACH of those two aggregate lines is printed only when its OWN counter is non-zero,
   and not merely when the run has failures: a run with a skip and no failure prints `Skipped:` and
   no `Failed:`, and a run with a failure and no skip prints `Failed:` and no `Skipped:`. Preflight
   round 6 confirmed that the two are independent, by running a three-test probe — one passing, one
   failing, one skipped — whose console printed both `Failed: 1` and `Skipped: 1`. That per-counter
   rule is why P1-T4's `[expect-fail]` acceptance can and does read `Failed: 1` from the console —
   P1-T4's run has a non-zero failure count by construction — while every
   green-run task in this plan cannot read a `Failed` count from there at all. Per-test pass and fail
   lines are a separate matter and ARE printed on a green run, so P3-T6's requirement that two named
   tests appear by name as passing in the console output is unaffected.

   Both omitted counts are recoverable from the TRX file that every affected command in this plan
   already writes through its `/Logger:trx` switch. The TRX carries the run's aggregate counts in a
   single element, for example:

   ```text
   <Counters total="4783" executed="4783" passed="4783" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" ... />
   ```

   **How `Failed` and `Skipped` are sourced from that element, and why `notExecuted` is prohibited.**
   `Failed` is read from the `failed` attribute of the single `<Counters .../>` element. `Skipped` is
   DERIVED from that same element as `total` minus `executed`; record `total`, `executed`, and the
   derived `Skipped` value. The `notExecuted` attribute MUST NOT be used: `vstest.console.exe`'s TRX
   logger populates only `total`, `executed`, `passed`, and `failed` on the `<Counters .../>` element
   and hard-codes every other attribute (`notExecuted`, `error`, `timeout`, `aborted`,
   `inconclusive`, ...) to `0` regardless of the run's actual outcome. Measured in preflight round 6:
   a run whose console printed `Skipped: 1` produced a TRX with `notExecuted="0"`; the derivation
   `total` minus `executed` correctly returned `1` on that run and `0` on an all-passing run. In the
   same file, the skipped test's own `<UnitTestResult ... outcome="NotExecuted">` element was present
   and correct, so it is the aggregate `<Counters .../>` element alone that mis-reports. Sourcing
   `Skipped` from `notExecuted` would therefore record a constant `0` whatever the run did — an
   acceptance value that cannot fail, which is the same defect class this constraint exists to remove.

   This plan therefore sources `Total tests` and `Passed` from the console summary block and `Failed`
   from the `failed` attribute in all seven TRX-reading tasks (P0-T10, P0-T11, P3-T2, P3-T3, P3-T6,
   P4-T5, and P4-T6), and derives `Skipped` as `total` minus `executed` in the four of those that
   record a `Skipped` count (P0-T10, P0-T11, P4-T5, and P4-T6); P3-T2, P3-T3, and P3-T6 record no
   `Skipped` figure and read only `Failed`. No command line changes: the `/Logger:trx` and
   `/ResultsDirectory:` switches every one of those tasks already carries are what produce the file,
   and each task writes to its own results directory, so no TRX can be attributed to the wrong task.
   Repeated runs of the SAME task do leave more than one `.trx` file in that task's own directory,
   which is what the TRX selection rule stated below governs. The TRX files are
   gitignored by `.gitignore` line 39 `[Tt]est[Rr]esult*/` and excluded from the format gate by
   `.csharpierignore` line 8 `*.trx` (both re-derived this pass), so reading them adds nothing to any
   porcelain, diff, or format gate in this plan.

   **Redaction rule binding on all seven of those tasks.** Record the `total`, `executed`, and
   `failed` values and the derived `Skipped` value, and identify the file they came from by its
   repository-relative results directory
   (`TestResults/p0-t10/`, `TestResults/p0-t11/`, `TestResults/p3-t2/`, `TestResults/p3-t3/`,
   `TestResults/p3-t6/`, `TestResults/p4-t5/`, `TestResults/p4-t6/`). Do NOT record the TRX file's own
   name in any evidence artifact: `vstest.console.exe` composes the default TRX filename from the host
   account name and the machine name, so recording it verbatim would place an account-name disclosure
   into committed evidence. Do NOT quote a vstest run's `Results File:` console line in any evidence
   artifact either: that line carries the full absolute host path of the same TRX, so it discloses the
   account name and the machine name as well as the worktree's absolute location. This is the same
   disclosure P4-T7's redaction rule prevents for the
   Cobertura `filename` attribute, applied to the second artifact class this plan now reads. The four
   recorded numeric values themselves carry no path.

   This constraint was invisible to preflight rounds 1 through 4. Until round 5, the `/InIsolation`
   mangling recorded in constraint 4 above made every `vstest.console.exe` invocation in this plan
   execute zero tests, so no round before round 5 had ever observed what a genuinely successful run's
   console output contains. It is the same defect class the `atomic-plan-contract` rule "Observe a
   command's success-case output before asserting over that output" exists to prevent: a value that
   documentation or intuition suggests is printed, but that the tool does not print on a successful
   run.

   Round 5's own first remedy fell into that same class and was corrected in round 6. That remedy
   sourced `Skipped` from the TRX `notExecuted` attribute, which the tool writes but never populates,
   so the recorded value would have been a constant `0` on every run. Round 6 built the three-test
   probe described above and read the attribute rather than assuming it, which is what produced the
   `total` minus `executed` derivation this constraint now states.

**TRX selection rule, binding on all seven TRX-reading tasks (P0-T10, P0-T11, P3-T2, P3-T3, P3-T6,
P4-T5, and P4-T6).** If a task's results directory holds more than one `.trx` file at the moment that
task's evidence artifact is written, read the most recently modified one, and record in that task's
artifact a line beginning `TRX SELECTED: most recently modified .trx in ` and completed by that
task's own results directory — for P4-T5, exactly
`TRX SELECTED: most recently modified .trx in TestResults/p4-t5/` — together with that file's
last-modified timestamp. Do NOT record the selected file's name: the
redaction rule in constraint 5 binds on this line too, because the default TRX filename carries the
host account name and the machine name, and the last-modified timestamp identifies the selection
without disclosing either.

This rule is stated once here rather than repeated inside individual tasks, because it applies to all
seven of them and because the plan explicitly anticipates re-running three. P3-T3's own text treats a
zero-test run as a failure of that task requiring a corrected filter and a re-run, and P4-T8's
loop-closure text restarts the Phase 4 loop from P4-T1 when a step rewrote a tracked file, which
re-runs P4-T5 and P4-T6. `vstest.console.exe` composes each default TRX filename with a timestamp and
does not overwrite an existing one — reported by the preflight round-6 reviewer, and recorded here as
a reported measurement rather than as a repository-tree citation — so a second run of the same task
leaves two `.trx` files in that task's directory. Without this rule those three tasks would have no
stated way to choose between them and the choice would fall to the executor.

### Worktree state assumed by Phase 0

The worktree in which this plan was authored and reviewed already contains `.dotnet-sdk/` (SDK
8.0.205), a completed `packages/` NuGet restore, and built `Debug` output. All three are gitignored
and none of them is committed by this plan. A FRESH worktree has none of them, so Phase 0's
bootstrap and restore steps in P0-T5 are mandatory and are written to run unconditionally where they
are cheap (`nuget restore`, `dotnet tool restore`) and conditionally where they are expensive and
idempotently detectable (the SDK bootstrap, which runs only when the first `dotnet --version` probe
fails). No task in this plan may be skipped on the assumption that a prior run already performed it.

### Phase 0 — Baseline capture, policy reads, and tree re-derivation

Phase 0 does not assume an empty `git status --porcelain`. This worktree already carries modified or
untracked files under the agent-memory tree at .claude/agent-memory/, written by the planning and
preflight delegations that ran in this same preparation cycle. That state is expected and affects no
gate in this plan: every terminal porcelain and diff gate here is pathspec-scoped to `UtilitiesCS`,
`UtilitiesCS.Test`, and the feature folder, and the two deliberately unscoped porcelain spans in
P4-T1 are compared before-against-after rather than asserted empty, precisely so that ambient state
cannot satisfy or falsify them. This plan's own commits never touch the Claude runtime tree at
.claude/, the Codex mirror tree at .codex/, the dot-agents tree at .agents/,
config/blast-radius.json, or config/orchestration-routing.json, and P5-T10 asserts that.

- [ ] [P0-T1] Read the policy files in the order required by `policy-compliance-order`: `CLAUDE.md`, then `.claude/rules/general-code-change.md`, then `.claude/rules/general-unit-test.md`, then `.claude/rules/quality-tiers.md`, then `.claude/rules/csharp.md`, then `.claude/rules/tonality.md`. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:` (the six paths in the order read), and an explicit list of the files read. Acceptance: the artifact exists, lists all six paths, and its `Policy Order:` line matches the order above.

- [ ] [P0-T2] Re-derive the defect site by reading `UtilitiesCS/Threading/UiThread.cs` in full. Record: the file's total line count; the line numbers of the `Dispatcher` property and its backing field; the verbatim backing-field declaration line; whether the file carries a nullable-enable directive on line 1; and the line numbers of the two lazy-initialising sibling properties `UiSyncContext` and `AutoScaleFactor`. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t2-uithread-rederivation.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the artifact records a total line count of 163, records that the backing-field line contains `null!`, records the property at lines 135-139 and the field at line 140, and records the nullable-enable directive on line 1. If any of these five recorded values differs from the value stated here, stop and report BLOCKED rather than editing, because the fix text quoted above was derived from them.

- [ ] [P0-T3] Re-derive the AC3 hypothesis by reading `UtilitiesCS/Threading/ProgressTrackerAsync.cs` in full. Record the verbatim text and line number of the statement that assigns `UiThread.Dispatcher` to the instance field, and the verbatim text and line number of the first statement that dereferences that instance field. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t3-progresstrackerasync-rederivation.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the artifact records that line 33 is `UiDispatcher = UiThread.Dispatcher;` and that line 35 is the first dereference (`await UiDispatcher.InvokeAsync(`), and states the conclusion that a throwing getter raises at line 33 before line 35 executes, so no edit to this file is required. If line 33 is not the property read, record the actual ordering, add `UtilitiesCS/Threading/ProgressTrackerAsync.cs` to the write-target list in the "Scope" section of this plan, and report the overturned conclusion to the caller before proceeding.

- [ ] [P0-T4] Re-derive the test-side facts by reading `UtilitiesCS.Test/Threading/UiThread_Tests.cs` in full, reading the `DispatcherField`/`ForceDispatcherNull`/`RestoreDispatcher` helper region of `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, and reading `UtilitiesCS.Test/Properties/AssemblyInfo.cs`. Record: the total line count of `UtilitiesCS.Test/Threading/UiThread_Tests.cs`; its namespace; its existing `using` directives; the reflection idiom used to reach the private static backing field; and whether the assembly declares class-level parallelisation. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t4-test-rederivation.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the artifact records 104 total lines, namespace `UtilitiesCS.Test.Threading`, absence of a `System.Reflection` using directive, the reflection idiom taking the field by the name `_dispatcher` with non-public static binding flags, and the presence of an assembly-level parallelisation attribute on line 18 of `UtilitiesCS.Test/Properties/AssemblyInfo.cs` (which is the justification for the do-not-parallelize attribute in P1-T2 and P1-T5).

- [ ] [P0-T5] Resolve and record the toolchain entry points. Run these commands from the worktree root, in the order given.

  **1. Probe the .NET SDK before anything that depends on it.**

  ```text
  dotnet --version
  ```

  If this fails with an error containing `The repo-local .NET SDK is missing`, the worktree has no `.dotnet-sdk/` directory, and `global.json` (`"paths": [".dotnet-sdk", "$host$"]`, re-derived this pass) then resolves to no SDK at all, so every `dotnet` command in this plan — `dotnet tool restore` in this task, `dotnet tool run csharpier --version` in this task, and the formatter and format-check commands in P4-T1 and P4-T2 — fails. Bootstrap the SDK by running, from the worktree root:

  ```text
  mkdir -p .dotnet-sdk
  curl -L -o .dotnet-sdk/sdk.zip https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip
  unzip -q -o .dotnet-sdk/sdk.zip -d .dotnet-sdk
  rm -f .dotnet-sdk/sdk.zip
  ```

  then re-run `dotnet --version`. Record every command actually run and every `dotnet --version` reading taken, in the order taken.

  This is deliberately NOT an invocation of `scripts/vscode/Install-RepoDotNetSdk.ps1`, and that script MUST NOT be run here. The script can only be started through `pwsh`, and a command whose name is `pwsh` is refused outright by this worktree's shell in every argument shape — `-File` and `-Command` alike, because the guard keys on `pwsh` occupying the command position. The verbatim refusal text measured in this worktree is: "this command runs pwsh in a plain command; what it reads or is handed as shell text cannot be shown not to run git. Refusing to run it." The four POSIX commands above perform the same download-and-extract the script performs: the URL is character-for-character the one the script builds at `scripts/vscode/Install-RepoDotNetSdk.ps1` line 26 for its default `$Version` of `8.0.205` and default `$Architecture` of `x64` (re-derived this pass), and `.dotnet-sdk` at the worktree root is the same destination the script resolves at line 36 (`Join-Path $PSScriptRoot '..\..\.dotnet-sdk'`, re-derived this pass). Version `8.0.205` is also what `global.json` pins, so the acceptance below is unchanged by the substitution.

  The bootstrap leaves nothing in any porcelain or diff gate later in this plan: `.gitignore` line 350 is `.dotnet*/` (re-derived this pass), which already ignores `.dotnet-sdk/`.

  Fail-closed rule for this step: if any of the four commands exits non-zero — including `curl` failing to download and including `unzip` being unavailable in this shell, which this plan has not measured — record `SDK_BOOTSTRAP: BLOCKED` in the artifact together with the failing command and its verbatim output, and halt. Do not proceed to any later task, because no formatting gate in this plan can run without an SDK and the evidence produced would be unverifiable. Exactly one fallback is authorised, and only for a failure of the `unzip` command specifically: run `PATH="/c/Windows/System32:$PATH" tar.exe -xf .dotnet-sdk/sdk.zip -C .dotnet-sdk`, record it with its own exit code, and declare `SDK_BOOTSTRAP: BLOCKED` if it too exits non-zero. That fallback uses the libarchive `tar.exe` Windows ships in `System32`, which reads zip archives; the GNU `tar` on the shell's default `PATH` does not, which is why the `PATH=` prefix is required to select the right one. No other substitution is authorised for any of the four commands.

  **2. Resolve the `vstest.console.exe` directory** by calling `vswhere.exe` through a `PATH=` prefix, so that the command NAME is the bare filename rather than a quoted absolute path:

  ```text
  PATH="/c/Program Files (x86)/Microsoft Visual Studio/Installer:$PATH" vswhere.exe -latest -products '*' -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
  ```

  This exact command was run in this worktree during preflight round 3 and printed `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. The `PATH=`-prefix spelling is required, not stylistic: a command whose NAME is a quoted absolute path is refused by this worktree's shell with "runs a command whose name is computed at runtime in a plain command ... Refusing to run it", so the previously planned `"/c/Program Files (x86)/.../vswhere.exe" -latest ...` form cannot run at all. A path supplied as an ARGUMENT is not refused, which is why the `-find` pattern and the `dotnet-coverage` operands in P0-T10 and P4-T5 need no such treatment.

  Two quoting details are load-bearing. `-products` is quoted as `'*'` so the shell does not expand it as a filename glob against the worktree root. The `-find` pattern is double-quoted so its backslashes survive word expansion; an unquoted `Common7\IDE\...` would have its backslashes stripped and would match nothing. If the command prints more than one path, take the first.

  From the printed full path, derive the two values every later vstest task substitutes, by stripping the trailing `\vstest.console.exe`:

  - `<resolved-vstest-dir-native>` — the directory exactly as `vswhere.exe` printed it, backslash-spelled, for the example above `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform`. It is used ONLY inside the double-quoted operand after `--` in P0-T10 and P4-T5, where it is an argument handed to `dotnet-coverage` and must be a native Windows path.
  - `<resolved-vstest-dir>` — the same directory in the POSIX spelling this shell uses for `PATH` entries, obtained mechanically by replacing the leading `C:` with `/c` and every `\` with `/`; for the example above `/c/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform`. It is used ONLY in the `PATH="<resolved-vstest-dir>:$PATH"` prefix of P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, and P4-T6. The POSIX spelling is required there because this shell splits `PATH` on `:`, so a native `C:\...` entry would split at the drive-letter colon and resolve nothing.

  Both derivations are mechanical, so a third party re-running this task obtains the same two values from the same printed line.

  **3. Confirm the MSBuild entry point.**

  ```text
  msbuild.exe -version
  ```

  The `.exe` suffix is required and is not optional shorthand. Measured in this worktree during preflight round 3: `msbuild -version` returns `command not found` with exit 127, while `msbuild.exe -version` exits 0 and prints `MSBuild version 18.9.1+a81b43525 for .NET Framework`. MSYS bash does not append `.exe` when searching `PATH` for a bare name, so the bare spelling names nothing. No `vswhere`-based resolution step is added for MSBuild and none is needed: `msbuild.exe` is on `PATH`, and this plan invokes it by that name in P0-T8, P0-T9, P1-T3, P3-T1, P4-T3, and P4-T4 with the switch sets `CLAUDE.md` mandates, unchanged. This probe carries NO `MSYS_NO_PATHCONV=1` prefix and must not be given one: its single argument `-version` begins with `-`, not `/`, so MSYS path conversion has nothing to rewrite here. The six full `msbuild.exe` build commands do carry the prefix, for the reason given in constraint 4 of "Shell constraints measured in this worktree". No switch is added, removed, or altered in any of them.

  **4. Restore NuGet packages for the solution.**

  ```text
  nuget.exe restore TaskMaster.sln
  ```

  This step is required and is not redundant with `dotnet tool restore` in step 5. `dotnet tool restore` restores only the local tool manifest, which in this repository is `dotnet-tools.json` at the worktree root (there is no `.config/` directory, re-derived this pass); it performs no NuGet package restore. A fresh worktree has no `packages/` directory at all, and every project in this solution is `packages.config`-based — 18 `packages.config` files exist across the solution's projects, including `UtilitiesCS/packages.config` and `UtilitiesCS.Test/packages.config`, re-derived this pass. Without this restore the FIRST build task in this plan (P0-T8) fails with 37 errors, comprising `CS0246` type-not-found errors and MSBuild `.targets`-file-not-found errors from the `packages.config` import elements; that failure was reproduced directly in this worktree during preflight round 4.

  The command name is the bare `nuget.exe`, which resolves on `PATH` in this shell and therefore needs no `PATH=` prefix. It MUST NOT be spelled as a quoted absolute path: constraint 2 in "Shell constraints measured in this worktree" records that this shell refuses any command whose NAME is a quoted absolute path. It also needs no `MSYS_NO_PATHCONV=1` prefix, because neither of its two arguments begins with `/`.

  This step is CI parity, not a local deviation: `.github/workflows/_build-analyzers.yml` line 45 runs `nuget restore $env:SOLUTION_PATH` with `SOLUTION_PATH: TaskMaster.sln` (line 17) immediately before its analyzer build, and `.github/workflows/_build-nullable.yml` line 45 and `.github/workflows/_mstest-coverage.yml` line 45 do the same before their respective gates. All three re-derived this pass.

  The restore writes only into `packages/` at the worktree root, which `.gitignore` line 191 ignores as `**/[Pp]ackages/*` (re-derived this pass), so it enters no porcelain or diff gate later in this plan. The one un-ignored path under that pattern is `!**/[Pp]ackages/build/` on line 193; no `packages/build/` directory exists in this worktree after a completed restore, re-derived this pass. If a restore in a fresh worktree does produce one, record it in this artifact and report it, because it would otherwise appear as an untracked path in P4-T1's two unscoped porcelain spans — where, being present in both, it would cancel — and it lies outside every scoped gate in Phase 5.

  **5.** `dotnet tool restore`
  **6.** `dotnet tool run csharpier --version`
  **7.** `dotnet-coverage --version`

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t5-toolchain-resolution.md` with `Timestamp:`, `Command:` (every command actually run, in the order run, including every `dotnet --version` attempt and every bootstrap command when bootstrap was required), `EXIT_CODE:` (per command), and `Output Summary:` recording:

  - a `SDK_BOOTSTRAP:` field, whose value is EITHER the bootstrap outcome (the first `dotnet --version` result, the fact that the four-command POSIX bootstrap was run, the resulting `.dotnet-sdk` path, and the post-bootstrap `dotnet --version` result) OR, when the first probe already succeeded and no bootstrap was performed, the literal value `NOT REQUIRED (first probe already reported a version beginning 8.0.2)`. The second form is the correct one whenever `.dotnet-sdk/` is already present — for example in a worktree where an earlier partial run bootstrapped it — because in that case no bootstrap runs and there is no post-bootstrap reading to record. Recording the literal is not a skip: the first `dotnet --version` command is still run and still recorded under `Command:` and `EXIT_CODE:`;
  - a `NUGET_RESTORE:` field recording the exit code of step 4's `nuget.exe restore TaskMaster.sln` and the restore summary line it printed (for example the count of packages installed, or its statement that all packages are already installed);
  - the verbatim path line `vswhere.exe` printed;
  - the derived `RESOLVED_VSTEST_DIR_NATIVE:` and `RESOLVED_VSTEST_DIR:` values described in step 2;
  - the reported MSBuild version, the CSharpier version, and the `dotnet-coverage` version.

  Acceptance: the last `dotnet --version` reading recorded from step 1 reports a version beginning `8.0.2` and exits 0; `nuget.exe restore TaskMaster.sln` exits 0; `RESOLVED_VSTEST_DIR_NATIVE:` and `RESOLVED_VSTEST_DIR:` are both recorded as non-empty concrete directory paths, and the file `vstest.console.exe` exists inside that directory; `msbuild.exe -version` exits 0; `dotnet tool restore` exits 0; and the reported CSharpier version is `1.2.6` (the version pinned by `dotnet-tools.json`). The two recorded directory values are the substitutions for the `<resolved-vstest-dir-native>` and `<resolved-vstest-dir>` placeholders used in P0-T10, P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, P4-T5, and P4-T6; no task in this plan records or substitutes a full `vstest.console.exe` file path as a command name. If `dotnet-coverage` is absent, record `dotnet-coverage: UNAVAILABLE`, install it with `dotnet tool install --global dotnet-coverage`, re-run the version probe, and record both attempts; do not proceed to P0-T10 with an unresolved collector.

- [ ] [P0-T6] Probe MCP availability by attempting one call to `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"` and `artifact_path: "docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md"`. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t6-mcp-probe.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the artifact records either the validator result or the exact string `MCP VALIDATOR UNAVAILABLE` plus the error text. This task never halts the plan: an unavailable validator is recorded and execution continues.

- [ ] [P0-T7] Capture the format baseline. Run `dotnet tool run csharpier check .` from the worktree root. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t7-csharpier-check.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. The `Output Summary:` MUST enumerate, one per line under the heading `BASELINE_FORMAT_DRIFT_SET:`, every repository-relative path the command reports as unformatted, or the single line `BASELINE_FORMAT_DRIFT_SET: NONE` when it reports none. Acceptance: the artifact exists and carries a `BASELINE_FORMAT_DRIFT_SET:` block. A non-zero exit code here is a recorded baseline fact, not a failure of this task; P4-T2 and P5-T10 are both written to be satisfiable against a non-empty drift set.

- [ ] [P0-T8] Capture the analyzer baseline. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t8-analyzer-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Warning(s)` and `N Error(s)` counts printed by MSBuild. Acceptance: `EXIT_CODE: 0` and the artifact records `0 Error(s)` together with the baseline warning count (referred to below as the baseline analyzer warning count).

- [ ] [P0-T9] Capture the nullable/type-check baseline. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t9-nullable-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Error(s)` count and the quoted command line. Acceptance: `EXIT_CODE: 0`, the artifact records `0 Error(s)`, and the quoted command line contains `msbuild.exe TaskMaster.sln`, contains no `Nullable=enable` substring, and uses `/t:Rebuild` rather than `/t:Build`. The first of those three clauses is worded as `contains` rather than `begins with` because the recorded line begins with the `MSYS_NO_PATHCONV=1 ` assignment required by constraint 4 in "Shell constraints measured in this worktree"; it checks only the executable spelling this shell requires. The two substantive clauses are unchanged from `CLAUDE.md` and are what this gate actually enforces.

- [ ] [P0-T10] Capture the `UtilitiesCS.Test` baseline run with Cobertura coverage, using the native vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 dotnet-coverage collect --output coverage/p0-t10.cobertura.xml --output-format cobertura --settings coverage.config -- "<resolved-vstest-dir-native>\vstest.console.exe" UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults/p0-t10 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  This task does NOT use the `PATH=`-prefix form that P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, and P4-T6 use. Here `vstest.console.exe` is not the command name — `dotnet-coverage` is — and the executable is an ARGUMENT after `--`, which this worktree's shell does not refuse. It DOES carry the `MSYS_NO_PATHCONV=1` prefix, which every vstest invocation in this plan carries regardless of which of the two forms it uses: the prefix suppresses the conversion of the colon-free `/InIsolation` switch, and that switch is passed through to `vstest.console.exe` identically in both forms. Without it, `/InIsolation` arrives as `C:/Program Files/Git/InIsolation`, vstest treats it as a test source, reports `The test source file ... was not found`, and runs zero tests — a run that would produce a coverage file describing nothing and a test count of zero. See constraint 4 in "Shell constraints measured in this worktree". The operand is double-quoted so the backslashes in `<resolved-vstest-dir-native>` survive word expansion and `dotnet-coverage` receives a valid native Windows path; the native spelling rather than the POSIX one is used for that same reason. P4-T5 uses this identical form, which is what keeps the two runs command-identical apart from their `--output` and `/ResultsDirectory` values, as P4-T7's comparison requires.

  Every other path in this command is written with forward slashes deliberately. Every command block in this plan is executed through a POSIX shell, which removes an unquoted backslash inside a word; a backslash-spelled `coverage\p0-t10.cobertura.xml` would therefore be created as `coveragep0-t10.cobertura.xml` at the worktree root, where `.gitignore`'s `coverage/*` rule (line 144, re-derived this pass) does not match it and where P4-T7 could not read it. `msbuild.exe`, `vstest.console.exe`, and `dotnet-coverage` all accept forward-slash paths on Windows. Backslashes survive in exactly three places in this plan, and all three are inside double quotes, which is what preserves them: the `-find` pattern in P0-T5 step 2, and the `"<resolved-vstest-dir-native>\vstest.console.exe"` operands in this task and in P4-T5.

  This command's flag set is deliberately identical to P4-T5's. The two differ only in the `--output` filename and the `/ResultsDirectory` value. In particular neither passes `/EnableCodeCoverage`, because activating the vstest in-process collector underneath `dotnet-coverage collect` changes the loaded-module set and therefore the `lines-valid` denominator, which would make the P4-T7 comparison between these two runs not apples-to-apples.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t10-utilitiescs-tests-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. The `Output Summary:` MUST record the numeric `Total tests`, `Passed`, `Failed`, and `Skipped` counts for this run, each drawn from the source named below, together with the TRX `total` and `executed` values from which the `Skipped` figure is derived, and, read from the root `<coverage>` element of `coverage/p0-t10.cobertura.xml`, the numeric `lines-covered`, `lines-valid`, and `line-rate` attribute values (referred to below as the baseline coverage figures).

  **Where each of the four test counts is read from.** `Total tests` and `Passed` are read from the console summary block this command prints. `Failed` is read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p0-t10/`. `Skipped` is DERIVED from that same element as `total` minus `executed`; record `total`, `executed`, and the derived `Skipped` value. The `notExecuted` attribute MUST NOT be used: `vstest.console.exe`'s TRX logger populates only `total`, `executed`, `passed`, and `failed` on the `<Counters .../>` element and hard-codes every other attribute (`notExecuted`, `error`, `timeout`, `aborted`, `inconclusive`, ...) to `0` regardless of the run's actual outcome. Measured in preflight round 6: a run whose console printed `Skipped: 1` produced a TRX with `notExecuted="0"`; the derivation `total` minus `executed` correctly returned `1` on that run and `0` on an all-passing run. The console/TRX split is required, not stylistic. Measured across four green runs in this worktree during preflight round 5, a successful `vstest.console.exe` run prints no `Failed:` line and no `Skipped:` line at all; the entire summary block it emits is:

  ```text
  Test Run Successful.
  Total tests: 4783
       Passed: 4783
   Total time: 12.8452 Seconds
  ```

  The TRX carries the run's aggregate counts in one element, for example:

  ```text
  <Counters total="4783" executed="4783" passed="4783" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" ... />
  ```

  where `failed` supplies `Failed` and `total` minus `executed` supplies `Skipped`. See constraint 5 in "Shell constraints measured in this worktree", which states this rule once centrally and binds it on all seven TRX-reading tasks. `TestResults/p0-t10/` is written by this task and by no other task in this plan, so the file is located without any choice being left to the executor; if this task has been run more than once, the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection.

  **Redaction rule for the TRX reference.** Identify the TRX in the artifact by its repository-relative results directory `TestResults/p0-t10/`, and do NOT record the file's own name, and do NOT quote the run's `Results File:` console line. `vstest.console.exe` composes the default TRX filename from the host account name and the machine name, and the `Results File:` line prints that filename inside a full absolute host path, so recording either verbatim would place an account-name disclosure into committed evidence — the same disclosure P4-T7's redaction rule prevents for the Cobertura `filename` attribute. Record the `total`, `executed`, and `failed` values and the derived `Skipped` value themselves, which carry no path.

  Acceptance: the artifact records all four test counts and all three numeric coverage attribute values as concrete numbers, not placeholders; it records the `total` and `executed` values from which the `Skipped` figure was derived; and it identifies `TestResults/p0-t10/` as the results directory `Failed` and `Skipped` were read from without recording a TRX filename and without quoting a `Results File:` line. If `Failed` is non-zero, record the failing test names as `BASELINE_FAILURE_SET:` and treat that set, not zero, as the comparison target in P3-T3 and P4-T5; a pre-existing failure is a recorded baseline fact, not a blocker for this plan.

- [ ] [P0-T11] Capture the `QuickFiler.Test` baseline run. Command:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p0-t11 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  The `PATH=`-prefix form is used by every task in this plan that runs `vstest.console.exe` as the command NAME (P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, P4-T6). It is required because this worktree's shell refuses a command whose name is a quoted absolute path, and `vstest.console.exe` does not resolve on the default `PATH` by bare name; see constraint 2 in "Shell constraints measured in this worktree". `<resolved-vstest-dir>` is the POSIX-spelled directory recorded in P0-T5 step 2.

  The `MSYS_NO_PATHCONV=1` assignment ahead of the `PATH=` assignment is separately required and is not decoration. Without it, MSYS path conversion rewrites the colon-free `/InIsolation` switch into `C:/Program Files/Git/InIsolation`; vstest then treats that as a test source, prints `The test source file ... was not found`, and runs zero tests while the colon-bearing switches on the same line pass through untouched. Ordering the two assignments the other way round works identically — the shell accepts any number of leading assignments — but the order shown here is used uniformly across all six tasks so the recorded command lines are comparable. See constraint 4 in "Shell constraints measured in this worktree".

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t11-quickfiler-tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the numeric `Total tests`, `Passed`, `Failed`, and `Skipped` counts, the TRX `total` and `executed` values from which the `Skipped` figure is derived, plus a `BASELINE_FAILURE_SET:` list when `Failed` is non-zero.

  The same sourcing rule P0-T10 states applies here unchanged. `Total tests` and `Passed` are read from the console summary block; `Failed` is read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p0-t11/`; and `Skipped` is DERIVED from that same element as `total` minus `executed`, with `total`, `executed`, and the derived `Skipped` value all recorded. The `notExecuted` attribute MUST NOT be used: `vstest.console.exe`'s TRX logger populates only `total`, `executed`, `passed`, and `failed` on the `<Counters .../>` element and hard-codes every other attribute (`notExecuted`, `error`, `timeout`, `aborted`, `inconclusive`, ...) to `0` regardless of the run's actual outcome. Measured in preflight round 6: a run whose console printed `Skipped: 1` produced a TRX with `notExecuted="0"`; the derivation `total` minus `executed` correctly returned `1` on that run and `0` on an all-passing run. The counts come from the TRX rather than the console because a green `vstest.console.exe` run prints no `Failed:` line and no `Skipped:` line at all (constraint 5 in "Shell constraints measured in this worktree", measured across four green runs in preflight round 5). Only the SOURCE of the `Failed` and `Skipped` values changes; the `BASELINE_FAILURE_SET:` mechanism keyed off a non-zero `Failed` count is unchanged, and the failing test names it lists are still taken from the run's per-test output. `TestResults/p0-t11/` is written by this task and by no other task in this plan; if this task has been run more than once, the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line, because `vstest.console.exe` composes the default TRX filename from the host account name and the machine name and prints it inside a full absolute host path.

  Acceptance: all four counts are recorded as concrete numbers, the `total` and `executed` values from which `Skipped` was derived are recorded, and the artifact identifies `TestResults/p0-t11/` as the results directory `Failed` and `Skipped` were read from without recording a TRX filename and without quoting a `Results File:` line. This assembly is baselined because the sibling audit in P3-T6 found that `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` constructs the parameterless `WpfUiDispatcher`, whose provider closes over `UiThread.Dispatcher`.

- [ ] [P0-T12] Record the coverage-threshold reconciliation. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t12-threshold-reconciliation.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` naming `CLAUDE.md` as the rank-1 authority supplying `>= 80%` repository line coverage and `>= 90%` new-code coverage, naming `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` as the rank-3/rank-4 sources supplying `>= 85%` line and `>= 75%` branch, stating that the rank-1 figures are the ones this plan enforces, and quoting the baseline `line-rate` recorded in P0-T10. Acceptance: the artifact names `CLAUDE.md` explicitly as the superseding authority and quotes the concrete baseline `line-rate` value.

- [ ] [P0-T13] Baseline the parallel-bucket census and the file sizes of the five files this plan writes. Run:

  ```text
  git grep -n -F '"_dispatcher"' -- UtilitiesCS.Test
  git grep -c -F DoNotParallelize -- UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  git grep -n -F "[TestClass" -- UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  wc -l UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  ```

  The counting idiom for every line-count assertion in this plan is `wc -l`, the physical newline count, used identically in P0-T13, P2-T3, and P4-T8. A before/after line-count comparison taken across two different counting idioms is incommensurable, so the idiom is fixed here rather than left to the executor. `Measure-Object -Line` MUST NOT be substituted for it: that cmdlet does not count blank lines and under-reports every file in this set by 17 to 92 lines, confirmed against this tree in this pass (`UiThread_Tests.cs` has 17 blank lines out of 104 and `ProgressTracker_Tests.cs` has 92 out of 514, so the substitution would report 87 and 422 in place of 104 and 514, tripping this task's own BLOCKED clause below and contradicting the pre-existing-overage narrative this plan records for `ProgressTracker_Tests.cs`).

  `wc -l` given more than one file prints one row per file and then a trailing `total` row. The acceptance below reads the five named per-file rows; it does not assert anything about the whole command output as a single value, and the trailing `total` row is expected and is not a defect.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/baseline/p0-t13-parallel-bucket-census.md` with `Timestamp:`, `Command:` (all four), `EXIT_CODE:` (per command), and `Output Summary:` quoting each command's output verbatim under the headings `BASELINE_DISPATCHER_WRITERS:`, `BASELINE_DONOTPARALLELIZE_COUNTS:`, `BASELINE_TESTCLASS_LINES:`, and `BASELINE_LINE_COUNTS:`. Acceptance, each value re-derived by the planner against the working tree in this pass:

  1. The first command prints exactly three lines, one each in `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` at line 144, `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` at line 422, and `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` at line 138.
  2. The second command prints nothing and exits 1, because `git grep` exits 1 on zero matches. This is the false-before half of P1-T5's gate: none of the four files carries the attribute at BASE.
  3. The third command prints exactly four lines, at `UtilitiesCS.Test/Threading/UiThread_Tests.cs:8`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:28`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:13`, and `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:14`.
  4. The fourth command's five per-file rows report 163 for `UtilitiesCS/Threading/UiThread.cs`, 104 for `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, 347 for `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, 205 for `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, and 514 for `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`. These five per-file rows are the recorded baseline counts referred to by P2-T3 and P4-T8; the trailing `total` row is ignored.

  If any of these values differs from the value stated here, stop and report BLOCKED rather than editing, because P1-T5's edit sites and P2-T3's size accounting were derived from them. The artifact MUST additionally carry the line `PRE-EXISTING FILE-SIZE OVERAGE: UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs 514` together with a statement that the overage exists at BASE `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` and is not introduced by this change.

### Phase 1 — Deterministic regression test, red before the fix

- [ ] [P1-T1] In `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, add `using System.Reflection;` to the existing using block, preserving the existing directives and their order (`System`, `System.Reflection`, `System.Threading`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`). Do not add `using System.Windows.Threading;`; the new test refers to the WPF dispatcher type by its fully-qualified name so no new type name enters this file's lookup scope. Acceptance: reading the file shows exactly one added using directive and five using directives total, in the order listed above, with no existing directive removed or reordered. No `git diff` is asserted at this point; the diff-based gates in this plan all use the single-ref working-tree form `git diff 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1 -- <paths>` and are stated in P3-T5 and P4-T7.

- [ ] [P1-T2] In the same file `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, append a second test class inside the existing `UtilitiesCS.Test.Threading` namespace, after the closing brace of `SynchronizationContextAwaiter_Tests`. The class is named `UiThread_Dispatcher_Tests`, carries `[TestClass]` on one line and `[DoNotParallelize]` on the next (justified by the assembly-level parallelisation attribute recorded in P0-T4 and by the fact that both tests mutate the process-global static `UiThread._dispatcher`), carries an XML doc comment containing the literal token `#584` and stating why reflection is used, and contains a private static helper returning the `FieldInfo` for `_dispatcher` plus exactly these two `[TestMethod]`s. The XML doc comment MUST NOT contain the token `DoNotParallelize`, because P1-T5 asserts an exact occurrence count of 1 for that token in this file. The XML doc comment MUST ALSO NOT contain any of the seven tokens `Thread.Sleep`, `Task.Delay`, `SpinWait`, `Retry`, `retries`, `Timeout(`, or `PushFrame`, in any letter case. P3-T5 searches the added lines of this change case-insensitively for exactly those seven tokens, and it reads added lines without distinguishing code from comment, so a doc comment explaining that the test needs no retry or sleep would trip AC5's gate on its own compliant documentation. State the rationale without those words — for example, by saying the test drives the accessor contract directly through the private backing field and is therefore deterministic without any timing construct.

  ```csharp
        [TestMethod]
        public void Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize()
        {
            // Arrange
            var field = DispatcherField();
            field.Should().NotBeNull();
            var prior = field.GetValue(null);
            field.SetValue(null, null);
            try
            {
                // Act
                Action act = () =>
                {
                    _ = UiThread.Dispatcher;
                };

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("*UiThread.Initialize()*");
            }
            finally
            {
                field.SetValue(null, prior);
            }
        }

        [TestMethod]
        public void Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance()
        {
            // Arrange
            var field = DispatcherField();
            var prior = field.GetValue(null);
            var expected = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            field.SetValue(null, expected);
            try
            {
                // Act / Assert
                UiThread.Dispatcher.Should().BeSameAs(expected);
            }
            finally
            {
                field.SetValue(null, prior);
            }
        }
  ```

  The helper is:

  ```csharp
        private static FieldInfo DispatcherField()
        {
            return typeof(UiThread).GetField(
                "_dispatcher",
                BindingFlags.NonPublic | BindingFlags.Static
            );
        }
  ```

  Acceptance: the file contains exactly two `[TestClass]` attributes; the new class contains exactly two `[TestMethod]` attributes; the new code contains no occurrence of `Thread.Sleep`, `Task.Delay`, `Thread.CurrentThread.Join`, `SpinWait`, or `Dispatcher.PushFrame`; both tests restore the captured prior field value in a `finally` block; the file's total line count is under 500; and `git grep -c -F "#584" -- UtilitiesCS.Test/Threading/UiThread_Tests.cs` prints a count of 1 or more, which is the enforceable form of the XML-doc-comment requirement stated above.

- [ ] [P1-T3] Build the solution so the new test compiles against the UNFIXED production code. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p1-t3-build-before-fix.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Warning(s)` and `N Error(s)` counts. Acceptance: `EXIT_CODE: 0` and `0 Error(s)`. This confirms the regression test's red state in P1-T4 is a runtime assertion failure and not a compile failure, which is the property that makes it a genuine fail-before.

- [ ] [P1-T4] [expect-fail] Run the two new tests against the unfixed production code, using the POSIX vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p1-t4 /TestCaseFilter:"FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize|FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance"
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p1-t4-expect-fail.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1`, and `Output Summary:` recording `Total tests`, `Passed`, `Failed`, the name of the failing test, and the verbatim FluentAssertions failure message. Acceptance: the run reports `Total tests: 2`, `Passed: 1`, `Failed: 1`; the single failure is `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`; and the recorded failure message states that no exception was thrown. The positive test passing here is expected and required: it proves the reflection arrangement and the restore path work before the production change, so the red in the negative test is attributable to the defect and not to the test harness.

- [ ] [P1-T5] Move every remaining writer of `UiThread._dispatcher` out of the parallel bucket, by an attribute-only edit to three existing test files. Make exactly these three edits and nothing else in these files — no `using`, no assertion, no test body, no member added, removed, or reordered:

  1. `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`: insert `    [DoNotParallelize]` immediately after the `[TestClass]` on line 28, giving the two-line form quoted verbatim in the "Exact source text this plan will create" section.
  2. `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`: insert `    [DoNotParallelize]` immediately after the `[TestClass]` on line 13, same two-line form.
  3. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`: replace line 14's `    [TestClass]` with the single line `    [TestClass, DoNotParallelize]`. The combined attribute list is used for this one file only, because the file is 514 lines at BASE and already exceeds the 500-line limit in `.claude/rules/general-code-change.md`; the combined form adds the attribute without adding a line, so this change does not deepen a pre-existing overage.

  Then run:

  ```text
  git grep -l -F '"_dispatcher"' -- UtilitiesCS.Test
  git grep -c -F DoNotParallelize -- UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p1-t5-donotparallelize.md` with `Timestamp:`, `Command:` (both), `EXIT_CODE:` (per command), and `Output Summary:` quoting both outputs verbatim. Acceptance:

  - The first command prints exactly these four paths and no other: `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`. The fourth is present because P1-T2 added the new class's `DispatcherField()` helper to it.
  - The second command prints exactly four lines, one per path, each reporting a count of exactly `1`. This is the enforceable form of the isolation guarantee: every file in `UtilitiesCS.Test` that names `UiThread._dispatcher` carries the do-not-parallelize attribute, so zero writers of that field remain in the parallel bucket. P0-T13 recorded the false-before state for this same command (zero matches, exit 1), so the gate is false before this task and true after it.

  `DoNotParallelize` resolves in all three files without a new `using`: each already imports `Microsoft.VisualStudio.TestTools.UnitTesting` at `IdleAsyncQueue_Tests.cs:6`, `ProgressTrackerAsync_Tests.cs:7`, and `ProgressTracker_Tests.cs:8`, re-derived this pass. This task is placed after P1-T4 deliberately: P1-T3's build and P1-T4's expect-fail run are scoped to the two new tests by an explicit `/TestCaseFilter`, so neither depends on these three files, and the first build that compiles these edits is P3-T1.

  This task changes no assertion in any of the three files, which is what keeps it compatible with AC4's "all pass, unmodified assertions" wording. Moving a class from the parallel bucket to the serial bucket can only reduce the concurrency those tests experience, so it cannot introduce a race; the possibility that it exposes a latent ordering dependency is not asserted away here but is verified empirically by P3-T3 and P4-T5.

### Phase 2 — Minimal production fix

- [ ] [P2-T1] In `UtilitiesCS/Threading/UiThread.cs`, replace the `Dispatcher` property and its backing field (lines 135-140 as re-derived in P0-T2) with the text quoted verbatim in the "Exact source text this plan will create" section above: an expression-free `get` accessor that throws `new InvalidOperationException("The UI dispatcher has not been captured. Call UiThread.Init() so that UiThread.Initialize() runs before reading UiThread.Dispatcher.")` when `_dispatcher is null` and otherwise returns `_dispatcher`; the `private set => _dispatcher = value;` accessor unchanged; and the backing field redeclared as `private static Dispatcher? _dispatcher;` with the `null!` initialiser and its trailing comment removed. Change nothing else in this file. Acceptance: the property's declared return type is still the non-nullable `Dispatcher`; the file contains exactly one `throw new InvalidOperationException(`; the file's total line count is 172 or fewer; and no other member of the file is modified.

- [ ] [P2-T2] Verify the null-forgiving suppression is gone and the nullable field declaration is present. Run:

  ```text
  git grep -c -F "null!" -- UtilitiesCS/Threading/UiThread.cs
  git grep -n -F "private static Dispatcher? _dispatcher;" -- UtilitiesCS/Threading/UiThread.cs
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t2-nullforgiving-removed.md` with `Timestamp:`, `Command:` (both), `EXIT_CODE:` (per command), `Output Summary:` quoting each command's output. Acceptance: the first command prints no matching line and exits 1 (`git grep` exits 1 on zero matches), and the second command prints exactly one line whose path is `UtilitiesCS/Threading/UiThread.cs`. Both commands are scoped by pathspec to this one file, so neither is affected by `null!` occurrences elsewhere in the repository. The pre-change state of the first command was exactly one match on line 140, recorded in P0-T2, so this gate is false before the edit and true after it.

- [ ] [P2-T3] Account for the file-size limit in `.claude/rules/general-code-change.md` across all five files this plan writes. Run:

  ```text
  wc -l UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  ```

  This is character-for-character the command P0-T13 ran, for the reason stated there: the before and after counts must come from one counting idiom or the comparison is incommensurable. Read the five named per-file rows and ignore the trailing `total` row.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p2-t3-file-size.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` quoting all five reported per-file line counts alongside the corresponding baseline counts recorded in P0-T13. Acceptance:

  1. The counts for `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, and `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` are each strictly less than 500.
  2. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` is exempt from clause 1 because it is 514 lines at BASE, above the limit before this plan touches it. Its acceptance is instead that its post-change count is less than or equal to its P0-T13 baseline count plus 1. The plan's intent is a count unchanged at 514, achieved by the combined attribute list in P1-T5; the plus-one tolerance exists solely because a later `csharpier format .` pass may split that attribute list onto two lines, which is a formatter decision this plan does not control. The artifact MUST carry the line `PRE-EXISTING FILE-SIZE OVERAGE: UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` and state that the overage exists at BASE and is not introduced by this change. If the post-change count exceeds baseline plus 1, that is a real regression in this file and the task fails.

### Phase 3 — Targeted verification of the fix and its blast radius

- [ ] [P3-T1] Rebuild with analyzers after the fix. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p3-t1-analyzer-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Warning(s)` and `N Error(s)` counts. Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, and the warning count is less than or equal to the baseline analyzer warning count recorded in P0-T8.

- [ ] [P3-T2] Run the two new tests against the fixed code, using the POSIX vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p3-t2 /TestCaseFilter:"FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize|FullyQualifiedName=UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance"
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p3-t2-regression-green.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording `Total tests`, `Passed`, `Failed`, and both test names with their individual outcomes. Acceptance: `EXIT_CODE: 0` as observed from the shell; `Total tests: 2` and `Passed: 2` as observed in the console summary block; and `Failed: 0` read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p3-t2/`.

  The `Failed` value is sourced from the TRX rather than the console because a green `vstest.console.exe` run prints no `Failed:` line at all — measured across four green runs in this worktree during preflight round 5, the whole summary block a successful run emits is `Test Run Successful.` followed by `Total tests`, `Passed`, and `Total time` only. See constraint 5 in "Shell constraints measured in this worktree". Both test names and their individual outcomes remain console-observed, because per-test result lines ARE printed on a green run. This task records no `Skipped` figure, so the `total` minus `executed` derivation stated in constraint 5 does not apply here; the `notExecuted` attribute is not read by this task and MUST NOT be introduced into it. `TestResults/p3-t2/` is written by this task and by no other task in this plan; if this task has been run more than once, the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line, because `vstest.console.exe` composes the default TRX filename from the host account name and the machine name and prints it inside a full absolute host path.

- [ ] [P3-T3] Run the four `UtilitiesCS.Test` classes the spec names as at risk, plus the fifth class P1-T5 modifies, using the POSIX vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p3-t3 /TestCaseFilter:"FullyQualifiedName~UtilitiesCS.Test.Threading.IdleAsyncQueue_Tests|FullyQualifiedName~UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests|FullyQualifiedName~UtilitiesCS.Test.ProgressTracker_Tests|FullyQualifiedName~WpfDispatcherYieldTests|FullyQualifiedName~OutlookFolderTreeServiceConcurrencyTests"
  ```

  `UtilitiesCS.Test.ProgressTracker_Tests` is the fully-qualified name of the class declared in `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`; that file declares `namespace UtilitiesCS.Test` on line 12, not `UtilitiesCS.Test.Threading`, re-derived this pass. It is included here because P1-T5 modifies it, and it is not a prefix of any other class name in this filter.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p3-t3-at-risk-tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording `Total tests`, `Passed`, `Failed`, and the name and outcome of every executed test. `Total tests` and `Passed` are read from the console summary block; `Failed` is read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p3-t3/`, because a green run prints no aggregate `Failed:` line on the console (constraint 5 in "Shell constraints measured in this worktree"). The per-test names and outcomes remain console-observed. This task records no `Skipped` figure, so the `total` minus `executed` derivation stated in constraint 5 does not apply here; the `notExecuted` attribute is not read by this task and MUST NOT be introduced into it. `TestResults/p3-t3/` is written by this task and by no other task in this plan; if this task has been run more than once — which its own clause 1 below anticipates, since a zero-test run requires a corrected filter and a re-run — the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line. Acceptance:

  1. `Total tests` is greater than zero. A zero-test run means the filter matched nothing and proves nothing; treat it as a failure of this task and correct the filter.
  2. The executed set includes, by name, `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`, `YieldAsync_WithoutDispatcher_RemainsStrict`, `InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`, `GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher`, and `Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi` (the last is the `[STATestMethod]` at `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` line 412 that writes `UiThread._dispatcher`, re-derived this pass).
  3. The failing set is empty, or every member of it is also a member of the `BASELINE_FAILURE_SET` recorded in P0-T10. Any failing test that is not in that baseline set fails this task. If one of the five named tests appears in the failing set and also in the baseline set, record `PRE-EXISTING FAILURE: <test name>` in the artifact and report it to the caller before AC4 is marked, because AC4's wording is "all pass".

- [ ] [P3-T4] Re-verify AC3 against the committed tree rather than against the P0 reading. Run:

  ```text
  git add -A -- UtilitiesCS UtilitiesCS.Test
  git status --porcelain -- UtilitiesCS/Threading/ProgressTrackerAsync.cs
  git diff --name-status --cached 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1 -- UtilitiesCS/Threading/ProgressTrackerAsync.cs
  git grep -n -F "UiDispatcher = UiThread.Dispatcher;" -- UtilitiesCS/Threading/ProgressTrackerAsync.cs
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/p3-t4-progresstrackerasync-unmodified.md` with `Timestamp:`, `Command:` (all four), `EXIT_CODE:` (per command), `Output Summary:`. The `Output Summary:` MUST state, in one paragraph, why the fix in `UtilitiesCS/Threading/UiThread.cs` alone converts this consumer's failure mode: the property read on line 33 now throws before the dereference on line 35 is reached, so the consumer receives a self-diagnosing `InvalidOperationException` at the property-access line without a code change. Acceptance: the porcelain status command prints nothing, the `--cached` name-status diff prints nothing, and the grep prints exactly one line whose line number is 33.

  On what each span actually observes: `git status --porcelain` reports both the index and the working tree, so it is the span that observes the staged state produced by the preceding `git add`. A two-dot `git diff A..HEAD` never observes the index at all — it compares two commits — and before this plan's first commit (P5-T9) it would compare BASE against an identical HEAD and print nothing whatever the executor wrote, which is a vacuous pass. The `--cached` form used above compares the index against the named commit, so it observes the staged state directly and reports a real change to this path if one is staged. The two spans are complementary: `--cached` is blind to an unstaged working-tree edit, and porcelain is the span that catches that case.

- [ ] [P3-T5] Verify AC5 across the whole change. Run:

  ```text
  mkdir -p TestResults
  git diff 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1 -- UtilitiesCS UtilitiesCS.Test > TestResults/p3-t5-source.diff
  grep -E '^\+' TestResults/p3-t5-source.diff | grep -E -i 'Thread\.Sleep|Task\.Delay|SpinWait|Retry|retries|Timeout\(|PushFrame'
  ```

  The filter is a plain POSIX `grep` pipeline. It has to be: this worktree's shell refuses any command named for a PowerShell 7 host, in every argument shape (see constraint 1 in "Shell constraints measured in this worktree"), so a PowerShell-based filter would leave this gate with no runnable command at all. `grep` is available in the same shell that runs the two preceding spans. The `-i` flag supplies case-insensitive matching, and the seven-token list is the same list P1-T2's authoring constraint enumerates.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p3-t5-no-timing-tokens.md` with `Timestamp:`, `Command:` (all three), `EXIT_CODE:` (per command), and `Output Summary:` recording the byte size of `TestResults/p3-t5-source.diff` and quoting the last command's output verbatim. Acceptance: `TestResults/p3-t5-source.diff` is non-empty (an empty diff means the gate had nothing to inspect and the result is BLOCKED, not PASS), and the third command prints nothing and its second `grep` exits 1, which is what `grep` returns when it finds no match. If it prints any line, the change violates AC5 and the offending construct must be removed before proceeding.

  Two properties of the diff span are load-bearing and must not be "simplified". First, it is anchored to the BASE SHA rather than left bare, so it cannot silently degrade into a worktree-versus-index comparison. Second, it uses the **single-ref** form `git diff <SHA> -- <paths>`, which compares the working tree against that commit, and NOT the two-dot form `git diff <SHA>..HEAD`. This plan's first commit is P5-T9, so at Phase 3 the branch HEAD is still identical to BASE and a two-dot span would emit an empty diff no matter what the executor wrote — the `grep` filter would then print nothing and the gate would pass vacuously. The single-ref form is blind to untracked files, which is harmless here because all five files this plan writes are already tracked at BASE (re-derived this pass from `UtilitiesCS.Test/UtilitiesCS.Test.csproj` lines 476, 478, 489, and 493, and from the presence of `UtilitiesCS/Threading/UiThread.cs` at BASE).

  The redirection target is written with forward slashes for the reason stated in P0-T10: a backslash-spelled `TestResults\p3-t5-source.diff` would be created as `TestResultsp3-t5-source.diff` at the worktree root, which `.gitignore`'s `[Tt]est[Rr]esult*/` rule on line 39 does not match, which P5-T10's scoped porcelain check does not see, and which the following `grep -E '^\+' TestResults/p3-t5-source.diff` would then fail to open — producing a second, independent vacuous pass of this gate.

- [ ] [P3-T6] Run the `QuickFiler.Test` class that constructs the parameterless `WpfUiDispatcher`, whose provider closes over `UiThread.Dispatcher`. This class is NOT named in `spec.md` or in the research trail; it was found during this plan's adversarial self-review by enumerating `new WpfUiDispatcher(` across the repository. Command:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p3-t6 /TestCaseFilter:"FullyQualifiedName~QuickFiler.Controllers.Tests.WpfUiDispatcherTests"
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/regression-testing/p3-t6-quickfiler-wpfuidispatcher.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording `Total tests`, `Passed`, `Failed`, and each test name with its outcome. Acceptance: `Total tests` is 2 as observed in the console summary block; `Failed: 0` read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p3-t6/`; and both `Construction_YieldsAnIUiDispatcher` and `Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread` are listed by name as passing in the console output.

  The `Failed` value is sourced from the TRX because a green `vstest.console.exe` run prints no `Failed:` line at all, measured across four green runs in this worktree during preflight round 5 (constraint 5 in "Shell constraints measured in this worktree"). The by-name clause is unaffected by that measurement and is unchanged: the console DOES print per-test pass and fail lines on a green run, and the two names above are the two `[TestMethod]`s declared in `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` at lines 24 and 50, re-derived this pass. This task records no `Skipped` figure, so the `total` minus `executed` derivation stated in constraint 5 does not apply here; the `notExecuted` attribute is not read by this task and MUST NOT be introduced into it. `TestResults/p3-t6/` is written by this task and by no other task in this plan; if this task has been run more than once, the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line, because `vstest.console.exe` composes the default TRX filename from the host account name and the machine name and prints it inside a full absolute host path.

  The plan-time expectation is that neither is affected — the constructor only captures the provider lambda without invoking it, and the second test installs a real dispatcher through `UiThreadDispatcherFixture` before any forwarding call — but that expectation is verified by running the tests, not asserted from reading.

### Phase 4 — Final QA loop (format, analyze, type-check, test, coverage)

- [ ] [P4-T1] Format, with the formatter's write scope restricted to the five paths this plan owns. Run, from the worktree root:

  ```text
  git status --porcelain
  dotnet tool run csharpier format UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  git status --porcelain
  ```

  This plan's owned file set is exactly the five paths named on that command line:

  - `UtilitiesCS/Threading/UiThread.cs`
  - `UtilitiesCS.Test/Threading/UiThread_Tests.cs`
  - `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`
  - `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`
  - `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`

  **Option chosen for the repo-wide-drift problem, and why.** The preflight round-2 review identified that `dotnet tool run csharpier format .` formats the ENTIRE repository, so drift it repairs outside `UtilitiesCS`/`UtilitiesCS.Test` — in `QuickFiler/`, `TaskMaster/`, `ToDoModel/`, and elsewhere — would be rewritten, would not be restored by a check scoped to those two directories, would not be committed, and would be invisible to every terminal porcelain gate in this plan, which are all scoped the same way. Two remedies were available: widen the porcelain check and the `git checkout --` restoration to the whole worktree, or restrict the formatter's write scope so the unowned drift is never created. This plan takes the second. The reason is that the first remedy makes the gate depend on the whole worktree's ambient state, including tracked directories this plan has no relationship with (`.claude/agent-memory/` is tracked in this repository), so a concurrent or pre-existing modification anywhere would either fail the gate or force an exclusion list that grows without bound. Restricting the formatter's write scope removes the failure mode at its source: a file the formatter is never given cannot be rewritten.

  Repository policy is preserved by this choice. CSharpier is file-based and formats exactly the paths it is given, so the five owned files receive character-for-character the formatting `csharpier format .` would have applied to them. The repo-wide obligation is discharged on the verification side, unchanged: P4-T2 still runs `dotnet tool run csharpier check .` over the whole tree, which is the same read-only, CI-parity command `.github/workflows/_format-check.yml` line 41 runs (`dotnet csharpier check .`, re-derived this pass). A formatting regression anywhere in the repository therefore still surfaces; what this task no longer does is silently repair a pre-existing one.

  If the multi-path invocation is rejected by the pinned CSharpier 1.2.6 CLI, run `dotnet tool run csharpier format <path>` once per owned path instead and record all five invocations. Both forms have identical write scope, so the acceptance below is unaffected.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t1-format.md` with `Timestamp:`, `Command:` (all three, or all seven if the per-path fallback was used), `EXIT_CODE:` (per command), and `Output Summary:` quoting the formatter's trailing summary line verbatim (CSharpier prints a `Formatted N files in Xms` line, where `N` is the count of files processed rather than the count rewritten, so the number alone is not evidence of a no-op), the unscoped porcelain output taken before the formatter ran, the unscoped porcelain output taken after it ran, and the single line `RESTORED_UNOWNED_FORMAT_DRIFT: NOT APPLICABLE (formatter write scope restricted to the five owned paths)`. Acceptance: `EXIT_CODE: 0` for the formatter; and the two unscoped porcelain outputs differ, if at all, only in entries for the five owned paths above. The porcelain spans are deliberately UNSCOPED here — unlike the terminal gates in P5-T10 and P5-T11, whose pathspecs exist to keep unrelated tracked state from making them unsatisfiable — because the property this task must establish is precisely that no path outside the owned five changed, and a scoped span cannot observe that. Comparing the before and after outputs, rather than asserting an empty one, is what makes the observation independent of whatever ambient modifications already existed when the task began.

  This task records a before-and-after tree observation in addition to the formatter's exit code, because a formatter rewrites tracked source and still exits 0 after rewriting: its exit code alone is identical on a clean run and on a repairing one.

- [ ] [P4-T2] Verify formatting. Run `dotnet tool run csharpier check .`. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t2-format-check.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` enumerating every path the command reports as unformatted. Acceptance: none of this plan's five owned paths — `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` — appears in the reported set, and the reported set is a subset of the `BASELINE_FORMAT_DRIFT_SET` recorded in P0-T7. When that baseline set was `NONE`, this reduces to `EXIT_CODE: 0` with an empty reported set.

  The subset clause is the whole-tree half of this gate and is not slack. P4-T1 restricts the formatter's write scope to the five owned paths, so it repairs no pre-existing drift and creates none: every path this command reports must therefore already have been reported at P0-T7. A path in the reported set that is absent from `BASELINE_FORMAT_DRIFT_SET` is new drift introduced during this plan's execution and fails this task. This command is run over the whole repository (`.`), matching `.github/workflows/_format-check.yml` line 41 exactly, so the check retains full repository scope even though P4-T1's write scope is narrow.

- [ ] [P4-T3] Analyze. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t3-analyzer-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Warning(s)` and `N Error(s)` counts. Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, and the warning count is less than or equal to the baseline analyzer warning count from P0-T8.

- [ ] [P4-T4] Type-check. Run:

  ```text
  MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t4-nullable-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the trailing `N Error(s)` count and the quoted command line. Acceptance: `EXIT_CODE: 0`, `0 Error(s)`, and the quoted command line contains `msbuild.exe TaskMaster.sln`, contains no `Nullable=enable` substring, and uses `/t:Rebuild`. As in P0-T9, that first clause is worded as `contains` because the recorded line begins with the `MSYS_NO_PATHCONV=1 ` assignment, and it records only the executable spelling this shell requires; the `Nullable=enable` and `/t:Rebuild` clauses are the substantive checks and are unchanged. This gate is the one that proves AC2's real value: with the backing field now declared `Dispatcher?` in a file that opts into nullable analysis, a getter that returned the field without narrowing it would raise `CS8603` and fail here.

- [ ] [P4-T5] Test `UtilitiesCS.Test` with coverage, using the native vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 dotnet-coverage collect --output coverage/p4-t5.cobertura.xml --output-format cobertura --settings coverage.config -- "<resolved-vstest-dir-native>\vstest.console.exe" UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults/p4-t5 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  This command's flag set is deliberately identical to P0-T10's; the two differ only in the `--output` filename and the `/ResultsDirectory` value. The executable operand is spelled identically to P0-T10's — the same double-quoted `<resolved-vstest-dir-native>` substitution, supplied as an ARGUMENT after `--` rather than as a command name, and therefore without the `PATH=` prefix the six direct vstest tasks carry. The `MSYS_NO_PATHCONV=1` prefix IS carried here, exactly as in P0-T10, and its presence on both sides of the pair is part of what keeps the two runs command-identical; a prefix on one side only would mean one run executed the suite and the other executed nothing. Keeping the two spellings identical is a precondition of P4-T7's comparison. In particular `/EnableCodeCoverage` is deliberately absent from both. Adding it here alone would activate a second, nested collector underneath `dotnet-coverage collect`, changing the loaded-module set and therefore the `lines-valid` denominator, and P4-T7's baseline-to-post-change comparison would no longer be a comparison of like with like. `dotnet-coverage collect` alone already produces the Cobertura file P4-T7 reads.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t5-utilitiescs-tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording `Total tests`, `Passed`, `Failed`, `Skipped`, the TRX `total` and `executed` values from which the `Skipped` figure is derived, and the `lines-covered`, `lines-valid`, and `line-rate` attribute values read from the root `<coverage>` element of `coverage/p4-t5.cobertura.xml`. `Total tests` and `Passed` are read from the console summary block; `Failed` is read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p4-t5/`; and `Skipped` is DERIVED from that same element as `total` minus `executed`, with `total`, `executed`, and the derived `Skipped` value all recorded. The `notExecuted` attribute MUST NOT be used: `vstest.console.exe`'s TRX logger populates only `total`, `executed`, `passed`, and `failed` on the `<Counters .../>` element and hard-codes every other attribute (`notExecuted`, `error`, `timeout`, `aborted`, `inconclusive`, ...) to `0` regardless of the run's actual outcome. Measured in preflight round 6: a run whose console printed `Skipped: 1` produced a TRX with `notExecuted="0"`; the derivation `total` minus `executed` correctly returned `1` on that run and `0` on an all-passing run. Both counts come from the TRX rather than the console because a green run prints neither aggregate line on the console (constraint 5 in "Shell constraints measured in this worktree"). The failing test names, when there are any, remain console-observed. `TestResults/p4-t5/` is written by this task and by no other task in this plan; if this task has been run more than once — which P4-T8's loop-restart text anticipates — the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line. Acceptance: the failing-test set is empty, or is a subset of the `BASELINE_FAILURE_SET` recorded in P0-T10 with no new member; `Total tests` is greater than or equal to the baseline `Total tests` plus 2; the `total` and `executed` values from which `Skipped` was derived are recorded; and all three coverage attribute values are recorded as concrete numbers.

- [ ] [P4-T6] Test `QuickFiler.Test`, using the POSIX vstest directory resolved in P0-T5:

  ```text
  MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p4-t6 /TestCaseFilter:TestCategory!=LiveOutlook
  ```

  This command's flag set is deliberately identical to P0-T11's, including the `MSYS_NO_PATHCONV=1` and `PATH=` prefixes and the bare `vstest.console.exe` command name; the two differ only in the `/ResultsDirectory` value. `/EnableCodeCoverage` is deliberately absent from both: this task records no coverage figure, so the flag would have no consumer here, and its presence on one side of a baseline-to-post-change pair and not the other is exactly the asymmetry that makes two runs incomparable.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t6-quickfiler-tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording `Total tests`, `Passed`, `Failed`, `Skipped`, and the TRX `total` and `executed` values from which the `Skipped` figure is derived. `Total tests` and `Passed` are read from the console summary block; `Failed` is read from the `failed` attribute of the single `<Counters .../>` element in the TRX file this task's own `/Logger:trx` switch writes under `TestResults/p4-t6/`; and `Skipped` is DERIVED from that same element as `total` minus `executed`, with `total`, `executed`, and the derived `Skipped` value all recorded. The `notExecuted` attribute MUST NOT be used: `vstest.console.exe`'s TRX logger populates only `total`, `executed`, `passed`, and `failed` on the `<Counters .../>` element and hard-codes every other attribute (`notExecuted`, `error`, `timeout`, `aborted`, `inconclusive`, ...) to `0` regardless of the run's actual outcome. Measured in preflight round 6: a run whose console printed `Skipped: 1` produced a TRX with `notExecuted="0"`; the derivation `total` minus `executed` correctly returned `1` on that run and `0` on an all-passing run. Both counts come from the TRX rather than the console because a green run prints neither aggregate line on the console (constraint 5 in "Shell constraints measured in this worktree"). The failing test names, when there are any, remain console-observed. `TestResults/p4-t6/` is written by this task and by no other task in this plan; if this task has been run more than once — which P4-T8's loop-restart text anticipates — the "TRX selection rule" stated immediately after constraint 5 governs which `.trx` in that directory is read and what is recorded about the selection. The TRX reference is subject to the same redaction rule P0-T10 states: identify the file by its repository-relative results directory only, never by its own name and never by quoting the run's `Results File:` console line. Acceptance: the failing-test set is empty, or is a subset of the `BASELINE_FAILURE_SET` recorded in P0-T11 with no new member; the `total` and `executed` values from which `Skipped` was derived are recorded; and `Total tests` equals the baseline `Total tests` from P0-T11 (this plan adds no test to this assembly).

- [ ] [P4-T7] Compute and record the coverage delta and the changed-line coverage. Derive the added-line set as the line numbers of `+` lines produced by:

  ```text
  git diff 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1 -- UtilitiesCS/Threading/UiThread.cs
  ```

  This is the single-ref working-tree form, anchored to BASE, for the same reason stated in P3-T5: this plan's first commit is P5-T9, so a two-dot `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1..HEAD` span would return an empty diff at Phase 4, the added-line set would be empty, and clause (a) below could never be satisfied. `UtilitiesCS/Threading/UiThread.cs` is tracked at BASE, so the single-ref form's blindness to untracked files does not apply to it.

  Read `coverage/p4-t5.cobertura.xml`, locate the class node whose `filename` attribute ends in `Threading\UiThread.cs` or `Threading/UiThread.cs` (Cobertura emits the host path separator in that attribute value; accept either), and intersect its `<line number=...>` elements with the added-line set.

  **Redaction rule for this task's artifact.** `dotnet-coverage` writes the `filename` attribute as a full absolute host path — it begins with the drive letter and includes the user profile directory, so it contains a host account name. When recording the located class node in the evidence artifact, record ONLY the line-hit data: the `<line number=...>` values and their `hits` values. Do NOT record the `filename` attribute's absolute-path value verbatim. Identify the node in the artifact by the repository-relative path `UtilitiesCS/Threading/UiThread.cs` instead. The same rule applies to any other absolute path this task encounters while reading the Cobertura file.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t7-coverage-delta.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording, as concrete numbers: the baseline `lines-covered`, `lines-valid`, and `line-rate` from P0-T10; the post-change `lines-covered`, `lines-valid`, and `line-rate` from P4-T5; the signed difference `post-change lines-valid` minus `baseline lines-valid`; the added-line set; the intersected line numbers with their `hits` values; and the resulting changed-line coverage percentage. Both recorded `line-rate` figures MUST be labelled, verbatim, `raw unstripped dotnet-coverage line-rate for the UtilitiesCS.Test process; not the repository first-party figure CLAUDE.md's 80% refers to`. The word "single-assembly" is deliberately absent from that label: `dotnet-coverage collect` instruments the whole test host process and reports every first-party module loaded into it, not one assembly, so `UtilitiesCS` and `UtilitiesCS.Test` both contribute to `lines-valid` (which is also why P4-T7 clause (c) is a band rather than an equality). The label's purpose — marking the figure as not comparable to the repository-wide first-party percentage `CLAUDE.md` states — is unchanged. Acceptance:

  (a) The intersected set contains at least two line numbers. If it contains fewer, the coverage report did not resolve this file and the result is BLOCKED, not PASS.

  (b) Every intersected line has `hits` of 1 or more, giving 100% changed-line coverage, which satisfies the `>= 90%` new-code target from `CLAUDE.md`.

  (c) The denominator is comparable: the signed `lines-valid` difference is between 0 and 200 inclusive. Because P0-T10 and P4-T5 now run a flag-identical command, the only legitimate source of a `lines-valid` change between them is the source this plan adds — approximately six coverable lines in `UtilitiesCS/Threading/UiThread.cs` and approximately seventy-five in `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, both of which are inside the denominator because `coverage.config` excludes third-party module paths only (re-derived this pass). An exact-equality clause would be unsatisfiable for that reason and is deliberately not used. A difference outside the stated band indicates the collector's loaded-module set differed between the two runs — a mismatch of that kind has moved this denominator by tens of thousands of lines on an unchanged tree in this repository. If the difference falls outside the band, record `COVERAGE DENOMINATOR MISMATCH`, state explicitly that the repository-wide percentage comparison in clause (d) is VOID, and rest this gate on clauses (a) and (b) alone.

  (d) The post-change `line-rate` is greater than or equal to the baseline `line-rate` minus 0.005, the stated tolerance absorbing run-to-run nondeterminism in this suite. This clause is skipped and marked VOID when clause (c) recorded a denominator mismatch.

  Also record the post-change `line-rate` against the `>= 80%` repository figure from `CLAUDE.md` as an observation, not a gate. That observation is explicitly non-comparable to the policy figure: it is the raw, unstripped `dotnet-coverage` line rate for the `UtilitiesCS.Test` process, whereas `CLAUDE.md`'s 80% refers to the repository's first-party testable denominator after third-party stripping. If the post-change figure is below that floor while the baseline figure was also below it, record `PRE-EXISTING FLOOR SHORTFALL` and do not treat it as caused by this change.

- [ ] [P4-T8] Confirm the loop closed in a single clean pass and re-audit file sizes after the formatter ran. Re-run the P2-T3 command:

  ```text
  wc -l UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  ```

  This is character-for-character the command P0-T13 and P2-T3 ran, for the reason stated in P0-T13. Read the five named per-file rows and ignore the trailing `total` row.

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/qa-gates/p4-t8-loop-closure.md` with `Timestamp:`, `Command:` (the re-run command above, plus the string `see P4-T1..P4-T7` for the loop record), `EXIT_CODE:`, and `Output Summary:` listing each of P4-T1 through P4-T7 with its recorded exit code and artifact path, stating explicitly whether any step rewrote a tracked file, and quoting the five post-format per-file line counts. Acceptance: the artifact lists all seven steps in order with their artifacts; it records that no step after P4-T1 rewrote a tracked file (if any did, the loop restarts from P4-T1 and this artifact records both passes); and the five post-format line counts satisfy the same two clauses P2-T3 states, evaluated against the P0-T13 baseline counts. The re-run exists because `csharpier format .` in P4-T1 can change line counts, so a size audit taken only at P2-T3 would describe a pre-format tree.

### Phase 5 — Acceptance criteria, documentation, and handoff

- [ ] [P5-T1] Mark AC1 in `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` (the bullet beginning "AC1: `UiThread.Dispatcher` throws a named `InvalidOperationException`"). Acceptance: that bullet reads `- [x]` and the marking is accompanied by a citation of `evidence/regression-testing/p1-t4-expect-fail.md` (recording `Failed: 1`) and `evidence/regression-testing/p3-t2-regression-green.md` (recording `Passed: 2`).

- [ ] [P5-T2] Mark AC2 in the same file (the bullet beginning "AC2: The `null!` null-forgiving suppression"). Acceptance: that bullet reads `- [x]` and cites `evidence/qa-gates/p2-t2-nullforgiving-removed.md` recording zero `null!` matches in `UtilitiesCS/Threading/UiThread.cs`, and `evidence/qa-gates/p4-t4-nullable-build.md` recording `0 Error(s)`.

- [ ] [P5-T3] Mark AC3 in the same file (the bullet beginning "AC3: UtilitiesCS/Threading/ProgressTrackerAsync.cs is left unmodified"). Acceptance: that bullet reads `- [x]` and cites `evidence/other/p3-t4-progresstrackerasync-unmodified.md`, which must contain the empty `--cached` name-status diff for that path, the empty porcelain status for that path, and the recorded verification paragraph.

- [ ] [P5-T4] Mark AC4 in the same file (the bullet beginning "AC4: No regression in"). Acceptance: that bullet reads `- [x]` and cites `evidence/qa-gates/p1-t5-donotparallelize.md` (recording that the change to `IdleAsyncQueue_Tests.cs` and `ProgressTrackerAsync_Tests.cs` is attribute-only and alters no assertion), `evidence/regression-testing/p3-t3-at-risk-tests.md` (five named tests executed, no failure outside the recorded `BASELINE_FAILURE_SET`), and `evidence/regression-testing/p3-t6-quickfiler-wpfuidispatcher.md` (`Failed: 0`).

- [ ] [P5-T5] Mark AC5 in the same file (the bullet beginning "AC5: No retry, sleep, or timing tolerance"). Acceptance: that bullet reads `- [x]` and cites `evidence/qa-gates/p3-t5-no-timing-tokens.md` recording zero matching added lines in the anchored diff.

- [ ] [P5-T6] Mark AC6 in the same file (the bullet beginning "AC6: Full C# toolchain"). Acceptance: that bullet reads `- [x]` and cites exactly the seven artifacts listed in the AC6 row of the acceptance-criteria mapping table above: `evidence/qa-gates/p4-t1-format.md`, `evidence/qa-gates/p4-t2-format-check.md`, `evidence/qa-gates/p4-t3-analyzer-build.md`, `evidence/qa-gates/p4-t4-nullable-build.md`, `evidence/qa-gates/p4-t5-utilitiescs-tests.md`, `evidence/qa-gates/p4-t6-quickfiler-tests.md`, and `evidence/qa-gates/p4-t8-loop-closure.md`. `evidence/qa-gates/p4-t7-coverage-delta.md` is deliberately not cited here; it is AC7's evidence and is cited by P5-T7.

- [ ] [P5-T7] Mark AC7 in the same file (the bullet beginning "AC7: Repository-wide line coverage does not regress"). Acceptance: that bullet reads `- [x]` and cites exactly the two artifacts listed in the AC7 row of the acceptance-criteria mapping table above — `evidence/baseline/p0-t10-utilitiescs-tests-coverage.md` for the baseline figures and `evidence/qa-gates/p4-t7-coverage-delta.md` for the comparison — quoting the concrete baseline and post-change `line-rate` values, the signed `lines-valid` difference, and the concrete changed-line coverage percentage. If P4-T7 recorded `COVERAGE DENOMINATOR MISMATCH`, this bullet is marked `- [x]` only on the strength of the changed-line clauses and the check-off text must say so explicitly.

- [ ] [P5-T8] Mirror the issue update. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/issue-updates/issue-584.2026-09-02T09-02.md` containing `Timestamp:`, the exact text intended for issue #584, and `PostedAs:` set to `comment`, `body`, or `unknown`. If posting is blocked (for example `gh` is unavailable), begin the file with a `POSTING BLOCKED` header and the reason. Acceptance: the artifact exists and carries a `PostedAs:` line or a `POSTING BLOCKED` header; the plan does not halt on an unavailable `gh`.

- [ ] [P5-T9] Commit the change. Run:

  ```text
  git add -- UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git commit -m "<message>" -- UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  ```

  where `<message>` summarises the accessor contract change and names issue #584. Acceptance: `git log -1 --name-only` lists all five source paths — `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, and `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` — and lists no path under `.claude/`, `.codex/`, `.agents/`, or `config/`.

  The commit carries the same explicit pathspec as the `git add`, rather than being a bare `git commit` over the whole index. A bare commit would commit everything staged, and P3-T4 already ran `git add -A -- UtilitiesCS UtilitiesCS.Test`, which stages every modified or untracked path under those two directories rather than only this plan's five. The explicit-pathspec commit form is what actually bounds the committed footprint to the enumerated paths; the `git add` is retained ahead of it because a pathspec commit only accepts paths already known to git, so the evidence artifacts this plan creates under the feature folder must be staged first. P5-T10's porcelain span remains the backstop that reports anything left behind.

- [ ] [P5-T10] Verify the committed footprint. Run:

  ```text
  git diff --name-status 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1..HEAD
  git status --porcelain -- UtilitiesCS UtilitiesCS.Test docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  ```

  Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/p5-t10-footprint.md` with `Timestamp:`, `Command:` (both), `EXIT_CODE:` (per command), `Output Summary:` quoting both outputs verbatim. Acceptance: the anchored name-status diff lists exactly these five source paths and no other source path — `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` — apart from paths under `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/`; it lists no path in the `BASELINE_FORMAT_DRIFT_SET` recorded in P0-T7, because P4-T1's formatter write scope was restricted to the five owned paths and therefore rewrote no unowned path at all; it lists no path under `.claude/`, `.codex/`, `.agents/`, `config/blast-radius.json`, or `config/orchestration-routing.json`; and the porcelain output lists `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md` (modified by this plan's own check-off of P5-T9) and no path outside `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/`. P5-T11 commits both the plan file and this artifact together.

  The porcelain clause is worded that way because of the state this task actually runs in. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584` is inside this command's pathspec and contains this plan file; P5-T9 commits that whole folder, and the check-off protocol in `acceptance-criteria-tracking` marks P5-T9 `[x]` in this plan file once P5-T9 completes, so by the time this task runs the plan file is modified relative to that commit. `evidence/other/p5-t10-footprint.md` does not yet exist when these two commands run, because this task writes it from their output. An acceptance demanding that the porcelain output list "only this artifact" is therefore unsatisfiable in both directions at once, and is replaced by the clause above.

  The `git status --porcelain` span is the companion the name-status diff needs so that a file created but not yet tracked cannot escape the check. Here the two-dot form is correct and not vacuous, because P5-T9 has already committed, so `HEAD` is no longer identical to BASE.

- [ ] [P5-T11] Commit the footprint artifact and confirm a clean tree. Run:

  ```text
  git add -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git commit -m "docs(584): record committed-footprint evidence" -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git status --porcelain -- UtilitiesCS UtilitiesCS.Test docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  ```

  Acceptance: the final porcelain command prints nothing. The status pathspec is scoped so that unrelated tracked state elsewhere in the worktree, including `.claude/agent-memory/`, cannot make this gate unsatisfiable or falsely satisfied. The `git commit` carries the same explicit pathspec as the `git add`, for the reason stated in P5-T9: P3-T4 ran `git add -A -- UtilitiesCS UtilitiesCS.Test`, so a bare `git commit` here would commit whatever that left staged under those two directories, and P5-T10's name-status diff — the one span that would report it — has already run by this point.

- [ ] [P5-T12] Write the acceptance-criteria status summary. Write `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/other/p5-t12-ac-status-summary.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` listing AC1 through AC7 with, for each, its check state in `spec.md` and its evidence artifact path. Then run:

  ```text
  git add -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git commit -m "docs(584): record acceptance-criteria status summary" -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git status --porcelain -- UtilitiesCS UtilitiesCS.Test docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  ```

  Acceptance: all seven AC identifiers appear exactly once each in the artifact, every one is recorded as checked, every named artifact path exists on disk, and the final porcelain command prints nothing. The commit carries the same explicit pathspec as the `git add`, for the reason given in P5-T11.

- [ ] [P5-T13] Commit the plan's own final check-off state and confirm the tree is clean. Mark every remaining task in this plan file as `[x]`, including this task itself, then run:

  ```text
  git add -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git commit -m "docs(584): record final plan and acceptance-criteria check-off state" -- docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  git status --porcelain -- UtilitiesCS UtilitiesCS.Test docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584
  ```

  Acceptance: the final `git status --porcelain` command prints nothing. The commit carries the same explicit pathspec as the `git add`, matching P5-T9, P5-T11, and P5-T12; without it this commit would sweep anything still staged from P3-T4's `git add -A -- UtilitiesCS UtilitiesCS.Test`.

  This task exists because `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/plan.2026-09-02T09-02.md` is itself inside the pathspec of P5-T11's and P5-T12's porcelain checks, and its own check-off necessarily happens after those checks have already run. Without this task the plan terminates with its own plan file modified and uncommitted, which contradicts the repository expectation that work is not complete until the tree is clean. Marking this task `[x]` before its own commit is deliberate and is what makes the commit capture the plan's terminal state; the ordering is stated here so it is not read as a check-off ahead of evidence.

---

## Planner Adversarial Self-Review

SELF-REVIEW: RE-DERIVED THIS PASS

Citations re-derived directly against the working tree in this pass:

1. `UtilitiesCS/Threading/UiThread.cs` — read in full (163 lines). Line 1 is the nullable-enable directive; lines 135-139 are the `Dispatcher` property; line 140 is `private static Dispatcher _dispatcher = null!;` with the trailing comment. Confirms the defect and the exact replacement region.
2. `UtilitiesCS/Threading/UiThread.cs` — lines 113-125 (`UiSyncContext`) and 147-158 (`AutoScaleFactor`) re-derived as the lazy-initialising siblings, confirming the omission on `Dispatcher` is an inconsistency.
3. `UtilitiesCS/Threading/UiThread.cs` line 61 — `Dispatcher = _syncContextForm.UiDispatcher;` is the sole writer through the private setter; re-derived to confirm the setter's non-nullable parameter type is unaffected by the field retyping.
4. `UtilitiesCS/Threading/SyncContextForm.cs` line 30 — `public Dispatcher UiDispatcher { get; private set; } = null!;` re-derived, confirming the value assigned at `UiThread.cs:61` is statically non-null and introduces no `CS8604` at that assignment.
5. `UtilitiesCS/Threading/ProgressTrackerAsync.cs` — read in full (109 lines). Line 33 is `UiDispatcher = UiThread.Dispatcher;` and line 35 is `await UiDispatcher.InvokeAsync(`. AC3's "no edit required" conclusion is re-derived from the tree, not carried from the research document, and is confirmed.
6. UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs lines 57-67 — the existing `InvalidOperationException` precedent re-derived verbatim; lines 45-46 confirm the default fallback provider is `() => UtilitiesCS.UiThread.Dispatcher`.
7. `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — read in full (104 lines). Namespace `UtilitiesCS.Test.Threading`; four using directives; one `[TestClass]` (`SynchronizationContextAwaiter_Tests`); no `System.Reflection` using. Establishes the 500-line headroom decision and the exact using-block edit.
8. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 493 — `<Compile Include="Threading\UiThread_Tests.cs" />` re-derived, establishing that reusing the existing test file requires no project-file edit.
9. `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` lines 141-186 — `DispatcherField`, `ForceDispatcherNull`, and `RestoreDispatcher` re-derived, giving the exact reflection idiom (`typeof(UiThread).GetField("_dispatcher", BindingFlags.NonPublic | BindingFlags.Static)`) the new test mirrors. Lines 248-289 re-derived: the at-risk test asserts `NotThrow` and `callCount == 0` with no exception-type assertion, so the type change is invisible to it.
10. `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` lines 126-190 — re-derived: this test installs a real `Dispatcher.CurrentDispatcher` into `_dispatcher` before calling `InitializeAsync()` and restores in `finally`, so it exercises only the non-null path and is unaffected.
11. `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` lines 117-142 — re-derived: `YieldAsync_WithoutDispatcher_RemainsStrict` injects two null-returning provider delegates and asserts the exception TYPE only (`ThrowAsync<InvalidOperationException>()`, no `WithMessage`). The real `UiThread.Dispatcher` property is never read by any of this class's tests.
12. `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs` line 55 — re-derived: `new WpfDispatcherYield()` uses the parameterless constructor, so its fallback provider is the real property. Sibling re-check outcome: the message text differs after the fix but the exception type does not, and this test asserts neither; P3-T3 verifies empirically rather than resting on this reading.
13. `UtilitiesCS.Test/Properties/AssemblyInfo.cs` line 18 — the assembly-level `Parallelize(` attribute re-derived, which is the justification for the do-not-parallelize attribute on the new class and on the three existing classes P1-T5 touches.
14. **Sibling finding not present in `spec.md` or the research trail:** `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` lines 26 and 64 construct `new WpfUiDispatcher()`, whose provider closes over `UiThread.Dispatcher` (`UtilitiesCS/Threading/WpfUiDispatcher.cs` lines 24-25 and 37, re-derived). Line 26's constructor only captures the lambda and never invokes it; line 64 runs inside a `UiThreadDispatcherFixture` transaction that installs a real dispatcher first. Neither is expected to change outcome, but the class is now baselined in P0-T11 and verified in P3-T6 rather than left unexamined.
15. `.csharpierignore` — re-derived: it excludes `**/evidence/**`, `*.cobertura.xml`, `*.trx`, `*.csproj`, `*.props`, `*.targets`. Evidence artifacts written by this plan therefore cannot fail the format gate.
16. `.gitignore` lines 39 and 144-145 — re-derived: `[Tt]est[Rr]esult*/` ignores the `TestResults/` subdirectories this plan writes, and `coverage/*` (except `coverage/.gitkeep`) ignores the Cobertura outputs. Neither enters the committed footprint asserted in P5-T10. See citation 30 for why both patterns require the forward-slash spelling to take effect.
17. `coverage.config` — re-derived: it excludes only third-party module paths (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest). No first-party production path is excluded, so `UtilitiesCS/Threading/UiThread.cs` is in the coverage denominator and P4-T7's class-node lookup can resolve.
18. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` lines 249-272 — the AC1..AC7 bullets re-derived verbatim, giving the exact bullet-opening text each P5 check-off task must match. Line range corrected in the revision round 9 pass; see "Citations re-derived in the revision pass of 2026-09-02 (revision round 9, backtick-removal presentation fix)" below for the re-derivation, which supersedes this entry's round-1 "lines 234-257" reading — that reading predated the `## Write Set` section inserted at spec.md lines 77-86.
19. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/issue.md` line 8 — the merge base `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` re-derived as the anchor for every `git diff` in this plan.
20. `CLAUDE.md` "C# Toolchain" section and `.claude/rules/general-unit-test.md` / `.claude/rules/quality-tiers.md` coverage sections — re-derived, producing the recorded 80/90 versus 85/75 conflict and the rank-1 resolution stated in "Threshold reconciliation" above.

### Citations re-derived in the revision pass of 2026-09-02 (revision round 9, backtick-removal presentation fix)

Every citation below was re-derived against the working tree in this revision pass by reading the
named file. The tree is still at BASE `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` with no commit made.
This round changed no command line, no task ID, no write-target file, and no evidence path. It
changed only: the removal of backtick-wrapping around scope-exclusion, precedent, and
context-reference file-path mentions in `spec.md` and this plan (round 9 itself, applied before this
pass); one incomplete backtick-removal left behind by that round at spec.md line 168 (Defect 1,
corrected in this pass); the plan's own status and version metadata; and citation 18 above, which the
`## Write Set` section insertion had already made stale before round 9 and which round 9's edits did
not touch. The prose corrections are enumerated in "Sibling regions re-checked in the revision round
9 pass" below.

58. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` lines 77-86
    — the `## Write Set` section re-derived this pass. It lists exactly five paths, in this order:
    `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`,
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, and
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`. This plan's own "Scope: files this plan's
    diff writes" list (above, lines 23-32) was re-read alongside it and names the same five paths in
    the same order.
59. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` lines
    249-272 — the current position of the `## Acceptance Criteria` block re-derived directly from the
    file: the heading sits at line 249, the AC1-AC7 bullets run through line 272, and line 274 is `##
    Risks & Mitigations`. This is the corrected range that supersedes citation 18's round-1 "lines
    234-257" reading, which predated the `## Write Set` section this pass re-derived as citation 58.
60. `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md` line 259 —
    the AC3 bullet's opening text, "AC3: UtilitiesCS/Threading/ProgressTrackerAsync.cs is left
    unmodified", re-read this pass and compared character-for-character against P5-T3's quotation of
    the same opening text (above). The two match exactly; no correction to P5-T3 was required.

### Citations re-derived in the revision pass of 2026-09-02 (preflight round 2)

Every citation below was re-derived against the working tree in this revision pass. Prior-round
verification of the same files is not relied on, because the file set this plan writes changed in
this pass and a prior pass observed a superseded scope.

21. `git grep -n '"_dispatcher"' -- UtilitiesCS.Test` returns exactly three lines:
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:144`,
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:422`, and
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:138`. These are the complete set of
    files in the assembly that reflect over the process-global backing field, and every one of them
    writes it via `SetValue`. This is the derivation behind P0-T13, P1-T5, and the expanded scope.
22. `[DoNotParallelize]` occurs in 18 files under `UtilitiesCS.Test`, re-derived this pass. None of
    the three files in citation 21 is among them, so all three are in the parallel bucket at BASE.
23. `UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs` lines 15-16 — `[TestClass]` then
    `[DoNotParallelize]` on the following line, re-derived as the repository's prevailing two-line
    idiom that P1-T5 follows for two of the three files.
24. `[TestClass` occurs exactly once in each of the four files P1-T5 and P1-T2 touch, at
    `UtilitiesCS.Test/Threading/UiThread_Tests.cs:8`,
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:28`,
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:13`, and
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:14`. Re-derived this pass, which is what
    makes P0-T13's third acceptance clause a real four-line expectation rather than a lower bound.
25. Line counts re-derived this pass: `UtilitiesCS/Threading/UiThread.cs` 163,
    `UtilitiesCS.Test/Threading/UiThread_Tests.cs` 104,
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` 347,
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` 205, and
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` 514. **Sibling finding surfaced by this
    pass and not present in the prior round:** the last of these already exceeds the 500-line limit
    in `.claude/rules/general-code-change.md` at BASE. A naive "all five files under 500" acceptance
    would have been unsatisfiable. P1-T5 therefore uses the combined attribute list for that one
    file, and P2-T3 and P4-T8 record the overage as pre-existing and gate on non-growth instead.
26. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` line 12 declares `namespace
    UtilitiesCS.Test`, not `UtilitiesCS.Test.Threading`. Re-derived this pass; this is why P3-T3's
    added filter term is `FullyQualifiedName~UtilitiesCS.Test.ProgressTracker_Tests` and not a
    `.Threading.`-qualified spelling that would have matched nothing.
27. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` lines 411-432 —
    `Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi` is the
    `[STATestMethod]` that writes `UiThread._dispatcher` at line 432 and restores it in a `finally`.
    Re-derived this pass as the named test P3-T3 requires in its executed set.
28. `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` lines 150, 152, and 162 — the
    `SetValue` write, the `InitializeAsync()` call that reads `UiThread.Dispatcher` downstream, and
    the `Dispatcher.PushFrame(frame)` pump. Re-derived this pass; this is the concrete mechanism by
    which a concurrent writer could either observe the post-fix `InvalidOperationException` or leave
    the pumped frame without an exit, and it is what P1-T5 removes from the parallel bucket.
29. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` lines 476, 478, 489, and 493 — `Compile Include`
    entries for `Threading\ProgressTracker_Tests.cs`, `Threading\ProgressTrackerAsync_Tests.cs`,
    `Threading\IdleAsyncQueue_Tests.cs`, and `Threading\UiThread_Tests.cs`. Re-derived this pass;
    all four files are already wired and already tracked, so the expanded scope requires no
    `.csproj` edit and the single-ref `git diff` form used in P3-T5 and P4-T7 is not blind to any
    file this plan writes.
30. `.gitignore` line 39 `[Tt]est[Rr]esult*/` and line 144 `coverage/*` — re-derived this pass.
    Both are directory-scoped patterns. A file written to the worktree root as
    `TestResultsp3-t5-source.diff` or `coveragep0-t10.cobertura.xml`, which is what a POSIX shell
    produces from an unquoted backslash-spelled path, matches neither. This is the derivation behind
    the forward-slash rewrite of every path in this plan's command blocks.
31. `coverage.config` lines 12-22 — the `ModulePaths/Exclude` list names Deedle, FSharp,
    Castle.Core, FluentAssertions, Moq, Microsoft.Testing, and MSTest only. Re-derived this pass. No
    first-party assembly is excluded, so both `UtilitiesCS` and `UtilitiesCS.Test` contribute to
    `lines-valid`. That is why P4-T7's denominator clause is a bounded band rather than an equality:
    the production and test lines this plan adds legitimately move the denominator.
32. `UtilitiesCS/Threading/UiThread.cs` lines 135-140 re-read in this pass, unchanged from the prior
    round: `get => _dispatcher;` at line 137 and
    `private static Dispatcher _dispatcher = null!; // set in Initialize() before any access` at
    line 140. The replacement text quoted in this plan still matches the tree it will replace.

### Citations re-derived in the revision pass of 2026-09-02 (preflight round 3)

Every citation below was re-derived against the working tree in this revision pass. The tree is
still clean at BASE `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` with no commit made, so these
observations describe the state the plan's first task will actually run in.

33. Physical line counts re-derived this pass by counting every line of each file:
    `UtilitiesCS/Threading/UiThread.cs` 163, `UtilitiesCS.Test/Threading/UiThread_Tests.cs` 104,
    `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` 347,
    `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` 205, and
    `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` 514. These are the five values P0-T13,
    P2-T3, and P4-T8 assert, and they are the values `wc -l` reports.
34. Blank-line counts re-derived this pass: `UtilitiesCS.Test/Threading/UiThread_Tests.cs` has 17
    blank lines out of 104 and `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` has 92 out of
    514. `Get-Content | Measure-Object -Line` does not count blank lines, so it would have reported
    87 and 422 for those two files against a plan asserting 104 and 514. That is the derivation
    behind the `wc -l` idiom now fixed in P0-T13, P2-T3, and P4-T8, and behind P0-T13's explicit
    prohibition on substituting `Measure-Object -Line`.
35. `global.json` lines 6-10 — `"paths": [".dotnet-sdk", "$host$"]` with the error message `The
    repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the
    repository root...`. Re-derived this pass. `.dotnet-sdk/` does NOT exist in this worktree, also
    re-derived this pass, so every `dotnet` command fails until the bootstrap in P0-T5 runs.
36. `scripts/vscode/Install-RepoDotNetSdk.ps1` — re-derived this pass. Line 3 defaults `$Version` to
    `8.0.205`, which is why P0-T5's acceptance reads "a version beginning `8.0.2`". This script is
    NOT invoked by the plan; the citation records the two values P0-T5's POSIX bootstrap reproduces.
37. `C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe` — confirmed present at
    that path this pass. This is what makes P0-T5 step 2 a real command rather than a documented
    shape.
38. `.github/workflows/ci.yml` line 21-23 delegates format checking to
    `.github/workflows/_format-check.yml`, whose line 41 runs `dotnet csharpier check .` after
    `dotnet tool restore` on line 37. Re-derived this pass. This is the CI-parity command P4-T2
    runs repo-wide, and it is why restricting P4-T1's formatter WRITE scope to the five owned paths
    does not narrow the repository-wide format gate.
39. `git grep -n '"_dispatcher"' -- UtilitiesCS.Test` re-derived again in this pass and unchanged:
    `IdleAsyncQueue_Tests.cs:144`, `ProgressTracker_Tests.cs:422`,
    `ProgressTrackerAsync_Tests.cs:138`. P0-T13's first acceptance clause and P1-T5's edit sites are
    still correct after this round's edits.
40. `UtilitiesCS/Threading/UiThread.cs` lines 135-140 re-read again in this pass and unchanged:
    `get => _dispatcher;` at line 137 and
    `private static Dispatcher _dispatcher = null!; // set in Initialize() before any access` at
    line 140. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` line 14 is still the sole
    `[TestClass]` in that file.

### Citations re-derived in the revision pass of 2026-09-02 (revision round 5, applying preflight findings C1-C3 and N1-N2)

Every citation below was re-derived against the working tree in this revision pass by reading the
named file. The tree is still at BASE `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` with no commit made,
so these observations describe the state the plan's first task will actually run in.

41. `.gitignore` line 350 is `.dotnet*/`. Re-derived this pass by searching the file for `dotnet`,
    which returns exactly two lines: line 143, a comment about `dotnet-coverage` Cobertura output,
    and line 350, the pattern itself. `.dotnet-sdk/` is therefore already ignored, so P0-T5 step 1's
    bootstrap cannot appear in any porcelain or diff gate later in this plan.
42. `scripts/vscode/Install-RepoDotNetSdk.ps1` line 26 builds the download URL
    `https://builds.dotnet.microsoft.com/dotnet/Sdk/$Version/dotnet-sdk-$Version-win-$Architecture.zip`,
    line 3 defaults `$Version` to `8.0.205`, and lines 5-6 default `$Architecture` to `x64`.
    Substituting those defaults gives
    `https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip`, which is
    character-for-character the URL P0-T5 step 1 passes to `curl`. Re-derived this pass by reading the
    file in full.
43. `scripts/vscode/Install-RepoDotNetSdk.ps1` line 36 resolves the default install directory as
    `Join-Path $PSScriptRoot '..\..\.dotnet-sdk'`, which from `scripts/vscode/` is `.dotnet-sdk` at
    the worktree root — the same destination P0-T5 step 1's `unzip -d .dotnet-sdk` writes to.
    Line 56 shows the script's own success marker is `.dotnet-sdk/sdk/8.0.205`, which the extracted
    archive creates. Re-derived this pass.
44. `global.json` lines 2-11 re-derived this pass: `"version": "8.0.205"`, `"rollForward":
    "latestFeature"`, `"paths": [".dotnet-sdk", "$host$"]`, and the `errorMessage` beginning `The
    repo-local .NET SDK is missing`. This is why P0-T5's acceptance clause — post-bootstrap
    `dotnet --version` beginning `8.0.2` and exiting 0 — is unchanged by replacing the PowerShell
    bootstrap with the POSIX one: both install the same pinned version into the same directory.

**Environment measurements applied in this pass, recorded as such and not as tree citations.** The
first three shell behaviours in "Shell constraints measured in this worktree" — the refusal of any
command named `pwsh`, the refusal of any command whose NAME is a quoted absolute path, and the
failure of bare `msbuild` against the success of `msbuild.exe` — were measured by running the
commands in this
worktree during preflight round 3. The fourth (MSYS path conversion of forward-slash switches) and
the missing-NuGet-restore failure recorded in P0-T5 step 4 were measured the same way during
preflight round 4. The fifth (a green `vstest.console.exe` run printing no `Failed:` and no
`Skipped:` line, and the TRX `<Counters .../>` element carrying both counts) was measured the same
way during preflight round 5, across four green runs of 4783, 1312, 41, and 2 tests. The planner has
no shell in its tool surface and therefore did not
re-run them in this pass; they are carried as reported measurements with their verbatim outputs, and
they are labelled as measurements rather than as citations to the repository tree. The same applies
to the `vswhere.exe` PATH-prefix invocation in P0-T5 step 2 and the path it printed. Two consequences
of those measurements that this pass derived rather than measured are stated explicitly in the plan
so a reviewer can check them: that a POSIX shell splits `PATH` on `:` and so requires the
`/c/...` spelling for the `PATH=` prefix, and that `unzip` availability in this shell is unmeasured,
which is why P0-T5 step 1 carries a fail-closed `SDK_BOOTSTRAP: BLOCKED` clause covering a failure of
any of its four commands.

### Citations re-derived in the revision pass of 2026-09-02 (revision round 6, applying preflight round 4 findings E1-E3 and N-1/N-2/N-3)

Every citation below was re-derived against the working tree in this revision pass by reading or
searching the named file. The tree is still at BASE `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` with no
commit made. The worktree now additionally contains gitignored `.dotnet-sdk/`, `packages/`, and
`Debug` build output left in place by the round-4 reviewer; none of them is a tracked file and none of
them changes any citation below.

45. `.github/workflows/_build-analyzers.yml` — line 17 sets `SOLUTION_PATH: TaskMaster.sln`, line 45
    runs `nuget restore $env:SOLUTION_PATH` as the step named "Restore solution", and line 50 runs the
    analyzer `msbuild` immediately after it. Read in full this pass. `.github/workflows/_build-nullable.yml`
    line 45 and `.github/workflows/_mstest-coverage.yml` line 45 carry the identical restore step.
    This is the CI-parity citation for the new P0-T5 step 4: every CI gate that builds this solution
    restores NuGet packages first.
46. `packages.config` exists in 18 project directories across this solution, enumerated this pass:
    `QuickFiler`, `QuickFiler.Test`, `SVGControl`, `SVGControl.Test`, `Tags`, `Tags.Test`,
    `TaskMaster`, `TaskMaster.Test`, `TaskTree`, `TaskTree.Test`, `TaskVisualization`,
    `TaskVisualization.Test`, `ToDoModel`, `ToDoModel.Test`, `UtilitiesCS`, `UtilitiesCS.Test`,
    `VBFunctions`, and `VBFunctions.Test`. Every one of them is a restore target of P0-T5 step 4, and
    the two that matter directly to this plan — `UtilitiesCS/packages.config` and
    `UtilitiesCS.Test/packages.config` — are among them.
47. `.gitignore` line 191 is `**/[Pp]ackages/*` and line 193 is `!**/[Pp]ackages/build/`. Re-derived
    this pass by reading lines 185-216. The restore output under `packages/` is therefore ignored, with
    the single exception of a `packages/build/` directory, which does not exist in this worktree after
    a completed restore (also re-derived this pass). P0-T5 step 4 states what to do if a fresh-worktree
    restore produces one.
48. `dotnet-tools.json` exists at the worktree root and no `.config/` directory exists. Re-derived this
    pass. This is why P0-T5 step 4's rationale names the root manifest rather than the conventional
    `.config/` location.
49. `UtilitiesCS/Threading/UiThread.cs` lines 135-140 re-read again in this pass and still unchanged:
    the property opens at line 135, `get => _dispatcher;` is line 137, `private set => _dispatcher = value;`
    is line 138, and line 140 is
    `private static Dispatcher _dispatcher = null!; // set in Initialize() before any access`.
    The replacement text quoted in "Exact source text this plan will create" still matches the region it
    replaces, and P0-T2's five BLOCKED-clause values are still the values the tree reports.
50. `.dotnet-sdk/dotnet.exe` is present in this worktree. Re-derived this pass. `.gitignore` line 350
    `.dotnet*/` ignores it (citation 41). Its presence is why P0-T5's `SDK_BOOTSTRAP:` field now accepts
    a `NOT REQUIRED` value: in this worktree the first `dotnet --version` probe succeeds and the
    four-command bootstrap never runs, so there is no post-bootstrap reading to record. In a fresh
    worktree the bootstrap does run and the first form of the field applies.

### Citations re-derived in the revision pass of 2026-09-02 (revision round 7, applying preflight round 5 finding F1)

Every citation below was re-derived against the working tree in this revision pass by reading or
searching the named file. The tree is still at BASE `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` with no
commit made. This round changed no command line, no task ID, no write target, and no evidence path; it
changed only how seven tasks source two numeric fields, plus one new constraint entry and its
redaction rule. (The `Skipped` half of that sourcing rule was itself wrong and was corrected in
revision round 8; see the round-8 section below.)

51. `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` — searched this pass for `[TestMethod]` and
    for public method declarations. The file declares exactly two `[TestMethod]` attributes, at lines
    23 and 48, whose methods are `public void Construction_YieldsAnIUiDispatcher()` on line 24 and
    `public async Task Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread()` on line 50.
    This is the derivation behind P3-T6's `Total tests` of 2 and behind the two names its acceptance
    requires to appear as passing in the console output; both names are re-derived here rather than
    carried forward from the round-1 citation, which recorded the two `new WpfUiDispatcher()`
    construction sites (lines 26 and 64) and not the method declarations.
52. `.gitignore` lines 33-50 read this pass. Line 39 is `[Tt]est[Rr]esult*/` and is directory-scoped,
    so `TestResults/p0-t10/`, `TestResults/p0-t11/`, `TestResults/p3-t2/`, `TestResults/p3-t3/`,
    `TestResults/p3-t6/`, `TestResults/p4-t5/`, and `TestResults/p4-t6/` — and every `.trx` file
    inside them — are ignored. Line 44 is the unrelated NUnit pattern `TestResult.xml`. Reading the
    TRX files therefore adds nothing to P5-T10's name-status diff or to any porcelain gate in this
    plan.
53. `.csharpierignore` read in full this pass (15 lines). Line 8 is `*.trx`, alongside
    `**/evidence/**` on line 4, `*.cobertura.xml` on line 5, `*.coverage` on line 6, `*.coveragexml`
    on line 7, and the project-file exclusions on lines 12-14. The TRX files this round's rule reads
    are outside the format gate, so P4-T2's repo-wide `csharpier check .` cannot report them.

### Citations re-derived in the revision pass of 2026-09-02 (revision round 8, applying preflight round 6 finding G1 and non-blocking findings O1-O4)

Every citation below was re-derived against the working tree in this revision pass by reading the
named file. The tree is still at BASE `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` with no commit made.
This round changed no command line, no task ID, no write-target file, and no evidence path. It
changed only the stated SOURCE of the `Skipped` field, the placement of the TRX-collision tie-break
rule, one redaction clause, the plan's own status and version metadata, and the prose corrections
enumerated in "Sibling regions re-checked in the revision round 8 pass" below.

54. `UtilitiesCS/Threading/UiThread.cs` lines 130-145 re-read this pass and still unchanged: the
    `Dispatcher` property opens at line 135, `get => _dispatcher;` is line 137,
    `private set => _dispatcher = value;` is line 138, and line 140 is
    `private static Dispatcher _dispatcher = null!; // set in Initialize() before any access`. The
    replacement text in "Exact source text this plan will create" still matches the region it
    replaces, and P0-T2's five BLOCKED-clause values are still the values the tree reports.
55. `.csharpierignore` read in full again this pass (15 lines). Line 8 is `*.trx`, line 4 is
    `**/evidence/**`, line 5 is `*.cobertura.xml`. Every TRX this round's rewritten sourcing rule
    reads remains outside P4-T2's repo-wide `csharpier check .`, including a second `.trx` left in a
    results directory by a re-run, because the pattern is extension-scoped and not name-scoped.
56. `.gitignore` lines 33-48 re-read this pass. Line 39 is `[Tt]est[Rr]esult*/` and is
    directory-scoped, so every file inside `TestResults/<task>/` is ignored however many `.trx` files
    a re-run leaves there. Line 44 is the unrelated NUnit pattern `TestResult.xml`. The TRX selection
    rule added this round therefore adds nothing to P5-T10's name-status diff or to any porcelain
    gate in this plan.
57. `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` re-searched this pass for `[TestMethod]`
    and for public method declarations: exactly two `[TestMethod]` attributes, at lines 23 and 48,
    on `public void Construction_YieldsAnIUiDispatcher()` at line 24 and
    `public async Task Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread()` at line
    50. P3-T6's `Total tests` of 2 and its two by-name clauses are unaffected by this round's edit to
    that task, which changed only its TRX-sourcing and redaction prose.

**Environment measurements applied in this pass, recorded as such and not as tree citations.** The
`<Counters .../>` behaviour stated in constraint 5 — that `vstest.console.exe`'s TRX logger populates
only `total`, `executed`, `passed`, and `failed` and hard-codes every other attribute to `0`, so that
a run whose console printed `Skipped: 1` produced a TRX carrying `notExecuted="0"` while
`total` minus `executed` returned the correct `1` — was measured by the preflight round-6 reviewer,
who built a three-test probe assembly (one passing, one failing, one skipped) and ran it through this
plan's exact command shape. The related statement that each of the `Failed:` and `Skipped:` console
lines is printed only when its own counter is non-zero comes from the same probe, which printed both.
The statement that a default TRX filename embeds a timestamp and does not overwrite an existing file
is reported by the same reviewer. The planner has no shell in its tool surface and did not re-run any
of them in this pass; they are carried as reported measurements and are labelled as such rather than
as citations to the repository tree.

### Sibling regions re-checked in the revision round 9 pass

- **The spec.md line-168 `WpfDispatcherYield.cs` backtick leftover was found and fixed in this
  round.** Round 9's backtick-removal pass converted every other backtick-wrapped
  scope-exclusion/precedent/context-reference path mention in `spec.md` and this plan to plain text,
  but left the `WpfDispatcherYield.cs:64-66` mention inside spec.md's "Error handling and logging
  updates" subsection (the same file already referenced in plain-text form at spec.md lines 100 and
  133) still backtick-wrapped. It is now corrected to the unbackticked, full-repository-relative-path
  form `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:64-66`, matching the spelling already
  established at those two lines. The surrounding code-identifier backticks on
  `InvalidOperationException`, `Initialize()`, and `IdleAsyncQueue.OnApplicationIdle` in the same
  passage were left untouched, because those are code-identifier citations rather than path mentions.
- **No other task in this plan quotes the AC3 or AC4 bullet text in full, so no further
  quotation-consistency fix was needed.** P5-T1, P5-T2, and P5-T5 through P5-T8 were each re-read this
  pass: every one quotes only a short opening fragment of its own AC bullet (for example P5-T4's "AC4:
  No regression in"), never the AC3 or AC4 bullet in full. P5-T3's opening-fragment quotation of AC3
  was re-derived directly (citation 60 above) and P5-T4's opening-fragment quotation of AC4 ("AC4: No
  regression in") was re-checked against spec.md's current AC4 bullet and still matches.
- **P5-T9's and P5-T10's backtick-wrapped acceptance-criterion path spans were read and confirmed
  unaffected by this round's edits.** Both tasks still assert, in backtick-wrapped form, that the
  committed diff and the anchored name-status diff contain no path under `.claude/`, `.codex/`,
  `.agents/`, or `config/`. Those are enforcement assertions the executor checks against, not
  scope-exclusion/precedent/context-reference prose, so round 9's backtick-removal pass correctly left
  them backtick-wrapped and this pass found no change needed.
- **Constraint 5, the TRX selection rule, and the seven TRX-reading tasks (P0-T10, P0-T11, P3-T2,
  P3-T3, P3-T6, P4-T5, P4-T6) were read and confirmed unaffected by round 9's backtick-removal
  edits.** None of the seven task bodies, and neither the constraint-5 sourcing paragraph nor the TRX
  selection rule that follows it, mentions `WpfDispatcherYield.cs` or any other
  scope-exclusion/precedent/context-reference file path; each cites only its own vstest command,
  `/ResultsDirectory` value, and the `notExecuted`/`Skipped` sourcing rule already re-derived in the
  revision round 8 pass. No command line, task ID, write-target file, or evidence path in any of the
  seven changed as a result of round 9.

### Sibling regions re-checked in the revision round 8 pass

- **Every live occurrence of `notExecuted` in the plan was enumerated before any edit, and again
  after.** The pre-edit set named the attribute as the `Skipped` source in eight places: constraint
  5's sourcing paragraph, constraint 5's redaction rule, P0-T10 (three: its sourcing paragraph, its
  `where failed supplies ...` sentence, and its redaction rule), P0-T11, P4-T5, and P4-T6. All eight
  were corrected. Two further occurrences sit inside the verbatim `<Counters .../>` example blocks in
  constraint 5 and in P0-T10; those quote tool output rather than instruct the executor, and each is
  now immediately followed by the prohibition, so both were left as they stand. After the edits,
  every surviving occurrence of the token is either inside one of those illustrative XML blocks,
  inside an explicit prohibition on using the attribute, or
  inside the measured-evidence sentence recording that the probe's TRX carried `notExecuted="0"`. No
  occurrence anywhere in the plan now names it as a source for any recorded value.
- **The three tasks that read a `Failed` count but record no `Skipped` count were checked
  individually rather than swept.** P3-T2, P3-T3, and P3-T6 read only `failed`, so the `total` minus
  `executed` derivation does not apply to them; each now says so explicitly and forbids introducing
  `notExecuted`, so a later reader cannot mistake the absence for an omission. The delta added no
  `Skipped` field to any of the three, because adding one would have changed a gate rather than its
  documentation.
- **The `Output Summary:` field lists and the acceptance clauses were re-read against each other for
  all four tasks that record `Skipped`.** P0-T10, P0-T11, P4-T5, and P4-T6 now each list `total` and
  `executed` in the field list AND require them in the acceptance clause, so the derived value is
  auditable from the artifact rather than being an unverifiable single number. Recording the two
  operands is what makes the `Skipped` figure falsifiable by a third party, which is the property
  `notExecuted` lacked.
- **The tie-break rule's central form was checked against the redaction rule it now sits beside.**
  The finding's suggested wording would have had each artifact record `TRX SELECTED: <filename>`.
  That directly contradicts the redaction rule added in round 7, because the default TRX filename is
  composed from the host account name and the machine name. The rule as written records the results
  directory plus the selected file's last-modified timestamp instead, which identifies the selection
  uniquely within that directory and discloses neither. This is a sibling-invalidation catch inside
  this round's own delta.
- **The claim that per-task results directories make collisions impossible was corrected where it
  appeared.** Constraint 5's sourcing paragraph and the round-7 changelog both stated that files
  "cannot collide" because each task owns its directory. That is true across tasks and false within
  one task across re-runs, which is exactly the case O1 raised. Both sentences now say the uniqueness
  is per task and point at the selection rule for the re-run case.
- **The seven TRX-reading tasks were re-enumerated after the four per-task tie-break copies were
  replaced.** All seven — P0-T10, P0-T11, P3-T2, P3-T3, P3-T6, P4-T5, P4-T6 — now carry an explicit
  pointer to the central rule instead of a local copy of it, and the rule itself names all seven by
  task ID. Three of those pointers (P3-T3, P4-T5, P4-T6) additionally name the specific in-plan text
  that anticipates that task being re-run. The rule is stated
  in exactly one place and referenced from seven, so it can no longer be present in some tasks and
  silently absent from others. P1-T4, the eighth vstest task, reads no TRX and carries no pointer;
  the reason is stated in the round-7 section.
- **The `Failed:`/`Skipped:` trigger statement was corrected in both places it appeared**, not only
  in the one the finding named: constraint 5's measured-behaviour paragraph and the round-7
  changelog's P1-T4 rationale. Both now state the per-counter rule. P1-T4's own acceptance is
  unchanged and remains satisfiable: its run has a non-zero failure count by construction, so the
  `Failed:` line it reads is printed whatever the skip count is.
- **The "Shell constraints measured in this worktree" section header was re-read against its own new
  contents.** Its opening sentence attributed the five constraints to preflight rounds 3, 4, and 5;
  constraint 5 now also carries a round-6 measurement, so the sentence names rounds 3, 4, 5, and 6.
  The count of five is unchanged, because round 6 corrected constraint 5 rather than adding a sixth.
  The section now also states, before the numbered list, that the TRX selection rule at its end is a
  plan rule rather than an environment measurement, so the section's self-description matches what it
  contains. No existing constraint number moved, so every cross-reference elsewhere in the plan —
  which names constraint 1, 2, 4, or 5 — remains valid; each was re-read to confirm it.
- **The plan's `Status:` and `Version:` metadata were updated to name this round and its findings**,
  so the header does not report a round-7 plan while carrying round-8 text.
- **P4-T7 and P4-T8 were checked for a reason to change and required none.** P4-T7 reads Cobertura
  attributes and class-node line hits, and P4-T8 reads exit codes, artifact paths, and `wc -l` rows.
  Neither reads a TRX or a test count, so neither is affected by this round. P4-T7's own redaction
  rule was re-read and is consistent with the extended TRX redaction rule; both now prohibit
  recording an absolute host path from any artifact class this plan reads.
- **The write-target set was re-read after every edit in this round and is unchanged.** The five
  paths are `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`,
  `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,
  `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, and
  `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`.
  `UtilitiesCS/Threading/ProgressTrackerAsync.cs` remains outside the write-target list. No evidence
  path and no command line changed, so the acceptance-criteria mapping table and the `AC-MAPPING:`
  block below are unaffected and were re-read row by row against each other to confirm they still
  agree.

### Sibling regions re-checked in the revision round 7 pass

- **Every task in the plan that records a `Failed` or a `Skipped` count was enumerated before any
  edit was made, not only the five the finding named.** There are eight: P0-T10, P0-T11, P1-T4,
  P3-T2, P3-T3, P3-T6, P4-T5, and P4-T6. Seven now carry the two-source rule. The eighth, P1-T4, is
  deliberately unchanged, and the reason is stated rather than assumed: P1-T4 is the `[expect-fail]`
  task, its acceptance requires `Failed: 1`, and the measured behaviour is that EACH of the `Failed:`
  and `Skipped:` console lines is printed only when its OWN counter is non-zero — independently of
  the other, as the round-6 three-test probe confirmed by printing both lines for a run that had one
  failure and one skip. A run that satisfies P1-T4's
  acceptance therefore has a non-zero failure count and prints the line P1-T4 reads, so applying the
  TRX rule there would have changed
  a gate that is already satisfiable. Constraint 5 states this explicitly so a later reader does not
  read P1-T4 as an omission.
- **Every one of the seven changed tasks was re-read to confirm it already carries `/Logger:trx` and
  a `/ResultsDirectory:` that no other task shares.** The seven directories are `TestResults/p0-t10`,
  `p0-t11`, `p3-t2`, `p3-t3`, `p3-t6`, `p4-t5`, and `p4-t6`; P1-T4's `p1-t4` is the eighth and is
  likewise unique. Because no two tasks write to the same results directory, a TRX cannot be
  attributed to the wrong TASK, and the rule needs no new command, no `LogFileName` argument, and no
  change to any existing switch. No command line in the plan was modified in this round. Uniqueness
  across tasks does not, however, bound the number of `.trx` files inside one task's own directory
  when that task is re-run; revision round 8 added the "TRX selection rule" after constraint 5 to
  cover that case for all seven tasks.
- **The P0-T10 / P4-T5 and P0-T11 / P4-T6 identity preconditions were re-checked after this round's
  edits.** This round edited only `Output Summary:` and acceptance prose, so both pairs still differ
  only in `--output` and `/ResultsDirectory` (the first pair) and in `/ResultsDirectory` alone (the
  second). P4-T7 clause (c) rests on the first of those and is unaffected.
- **Sibling invalidation caught and repaired inside this round's own delta.** The first draft of this
  round required each affected artifact to "name the TRX file" the counts were read from.
  `vstest.console.exe` composes the default TRX filename from the host account name and the machine
  name, so that requirement would have written an account-name disclosure into committed evidence —
  the same defect class P4-T7's existing redaction rule was added to prevent for the Cobertura
  `filename` attribute. All four occurrences were rewritten to identify the file by its
  repository-relative results directory only, and the rule is stated once centrally in constraint 5
  and bound on all seven tasks. The finding's specified treatment is unaffected: it named the
  directory, not the filename, as the locator.
- **The "Shell constraints measured in this worktree" header sentence was corrected from "Four
  constraints" to "Five", and every cross-reference to a numbered constraint elsewhere in the plan was
  re-read.** The surviving references name constraint 1 (P0-T5 step 1 and P3-T5), constraint 2 (P0-T5
  step 4, P0-T10, and P0-T11), and constraint 4 (P0-T5 step 3, P0-T10, and P0-T11). The new entry was
  appended as constraint 5, so no existing number moved and no reference was invalidated.
- **The tasks that record test counts but were NOT changed were checked for a reason to change
  them.** P4-T7 records only Cobertura root-element attributes and class-node line hits, and P4-T8
  records exit codes, artifact paths, and `wc -l` rows; neither reads a test count, so neither is
  touched. P3-T3's clause 1 (`Total tests` greater than zero) and P4-T5's clause comparing `Total
  tests` to the baseline plus 2 both compare console-sourced values on each side, and P4-T6's clause
  compares its console-sourced `Total tests` to P0-T11's console-sourced one, so no comparison in the
  plan now spans two different sources.
- **The failing-test NAME sets were distinguished from the `Failed` COUNT throughout.** P0-T10's and
  P0-T11's `BASELINE_FAILURE_SET:`, P3-T3's clause 3, and P4-T5's and P4-T6's subset clauses all
  operate on test names taken from per-test output, which is printed on green and red runs alike.
  Only the aggregate count moved to the TRX, so none of those mechanisms changed.
- **The write-target set was re-read after every edit in this round and is unchanged.** The five
  paths are `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`,
  `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,
  `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, and
  `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`.
  `UtilitiesCS/Threading/ProgressTrackerAsync.cs` remains outside the write-target list. No evidence
  path changed, so the acceptance-criteria mapping table and the `AC-MAPPING:` block below are
  unaffected and were re-read row by row against each other to confirm they still agree.

### Sibling regions re-checked in the revision round 6 pass

- **Every `msbuild.exe` occurrence in the whole plan was enumerated, not only the six the finding
  named.** The plan contains seven: the six build command blocks (P0-T8, P0-T9, P1-T3, P3-T1, P4-T3,
  P4-T4), which now all carry the `MSYS_NO_PATHCONV=1 ` prefix, and the P0-T5 step 3 `-version` probe,
  which deliberately does not and must not. The two distinct build command lines were each replaced
  across all their occurrences in one operation — the analyzer line appears four times (P0-T8, P1-T3,
  P3-T1, P4-T3) and the nullable line twice (P0-T9, P4-T4) — so no occurrence could be missed through
  differing surrounding whitespace. The switch sets after the prefix are byte-identical to their
  pre-round-6 form: `/t:Rebuild` in all six, no `/p:Nullable=enable` anywhere, `/t:Build` nowhere.
- **Every `vstest.console.exe` and `dotnet-coverage` occurrence in the whole plan was enumerated.**
  There are eight invocations: six leading `MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe`
  (P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, P4-T6) and two leading `MSYS_NO_PATHCONV=1 dotnet-coverage collect`
  (P0-T10, P4-T5). All eight carry the prefix. The remaining textual occurrences of those names are in
  prose or in the P0-T5 step 2 `-find` pattern, none of which is an executed command line.
- **The P0-T10 / P4-T5 identity precondition was re-checked operand by operand after the prefix was
  added.** Both now begin `MSYS_NO_PATHCONV=1 dotnet-coverage collect`; both use the native directory
  spelling in the quoted operand after `--`; neither carries a `PATH=` prefix; neither carries
  `/EnableCodeCoverage`. They still differ only in `--output` and `/ResultsDirectory`, which is what
  P4-T7 clause (c) rests on. The same check was repeated for the P0-T11 / P4-T6 pair, which still
  differ only in `/ResultsDirectory`. Sibling-invalidation risk addressed here: a prefix added to one
  side of either pair and not the other would have left one run executing the suite and the other
  executing nothing, while both artifacts still recorded an exit code.
- **The acceptance clauses that read the recorded command line were re-checked.** P0-T9 and P4-T4 were
  the only two, and both were reworded from "begins" to "contains". Both retain the two substantive
  clauses verbatim. No other acceptance clause anywhere in the plan asserts a position within a
  recorded command line.
- **P0-T5's step numbering was re-checked against every cross-reference in the plan.** Inserting the
  NuGet restore as step 4 renumbered the former steps 4, 5, and 6 to 5, 6, and 7. Every surviving
  cross-reference in the plan names step 1, step 2, or step 3 — none names a renumbered step — so no
  reference was invalidated. The two references to step 4 both refer to the new restore step.
- **N-3 checked and no change required.** No task in this plan asserts an equality against a specific
  analyzer warning count. P0-T8 records the observed baseline count as data; P3-T1 and P4-T3 each
  require only that the observed count be less than or equal to that recorded baseline. A measured
  baseline of 5 satisfies those clauses exactly as any other value would, so the plan carries no
  number that the environment could falsify.
- **No `pwsh` was reintroduced.** The only occurrences of that token in the plan are constraint 1 of
  "Shell constraints measured in this worktree", P0-T5 step 1's prohibition on running the PowerShell
  bootstrap script, and P3-T5's rationale for using a `grep` pipeline. None is a command. The three new
  or rewritten command lines this round adds — `nuget.exe restore TaskMaster.sln` and the fourteen
  prefixed invocations — introduce no shell host.
- **The write-target set was re-read after every edit in this round and is unchanged.** This round
  touched command prefixes, one new toolchain step, two acceptance wordings, one evidence-redaction
  rule, and prose. It added no file to the diff and removed none.
  `UtilitiesCS/Threading/ProgressTrackerAsync.cs` remains outside the write-target list, and the
  restore step writes only into gitignored `packages/`.
- **P4-T7's new redaction rule was checked against P4-T7's own acceptance clauses.** Clauses (a) and
  (b) read the intersected line numbers and their `hits` values, which the rule explicitly preserves;
  clauses (c) and (d) read root-element attributes, which carry no path. The rule therefore removes
  the account-name disclosure without removing anything the gate reads. It was also checked against
  P5-T7, which cites the artifact and quotes coverage figures rather than paths.
- **The new "Worktree state assumed by Phase 0" section was checked against every Phase 0 task.** It
  states that the authoring worktree's prebuilt state must not be treated as licence to skip a task,
  which is consistent with P0-T5's unconditional restore steps and with its conditional SDK bootstrap.
  No task's acceptance was weakened to accommodate the prebuilt state.

### Sibling regions re-checked in the revision round 5 pass

- Every one of the eight `vstest.console.exe` invocations in the plan was re-read after the C2
  rewrite. Six of them (P0-T11, P1-T4, P3-T2, P3-T3, P3-T6, P4-T6) lead with
  `PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe`, and two (P0-T10, P4-T5) carry
  `-- "<resolved-vstest-dir-native>\vstest.console.exe"` as an argument to `dotnet-coverage`. The
  pre-C2 single placeholder that stood for a full executable path was removed everywhere; every
  vstest invocation in the plan now substitutes one of the two directory placeholders defined in
  P0-T5 step 2, and no command in the plan names a full `vstest.console.exe` path in the
  command-NAME position.
- P0-T10 and P4-T5 were compared operand by operand after the rewrite. They remain identical apart
  from `--output` and `/ResultsDirectory`, which is the precondition P4-T7 clause (c) rests on. Both
  use the native directory spelling; neither carries a `PATH=` prefix; neither carries
  `/EnableCodeCoverage`.
- P0-T11 and P4-T6 were compared the same way and remain identical apart from `/ResultsDirectory`.
  Both now carry the `PATH=` prefix, so P4-T6's "flag set identical to P0-T11's" clause still holds.
- Sibling invalidation caught in this pass: P0-T10's forward-slash rationale asserted that the P0-T5
  `-find` pattern was "the only backslashes left anywhere in this plan's commands". The C2 rewrite
  added two more, inside the double-quoted `dotnet-coverage` operands. The sentence was corrected to
  name all three sites and to state that double quoting is what preserves them in each.
- Every `msbuild` invocation was re-read after the C3 rewrite. All six command blocks (P0-T8, P0-T9,
  P1-T3, P3-T1, P4-T3, P4-T4) and the P0-T5 step 3 probe now spell `msbuild.exe`. The switch sets are
  byte-identical to their pre-C3 form: `/t:Rebuild` in all six, `/p:EnableNETAnalyzers=true
  /p:EnforceCodeStyleInBuild=true` in the four analyzer builds, `/p:TreatWarningsAsErrors=true` in
  the two type-check builds, and no `/p:Nullable=enable` anywhere. The quoted-command-line acceptance
  clauses in P0-T9 and P4-T4 were updated to expect the `msbuild.exe` spelling; both retain their two
  substantive clauses unchanged, and both now state that the spelling clause is not the substantive
  one.
- P0-T5's step 3 justification previously rested on a prior round's review note that bare `msbuild`
  resolved. That claim is now known to be wrong in this worktree and was replaced with the measured
  exit-127 and exit-0 readings.
- P0-T5's acceptance clause previously required "the resolved vstest path is a non-empty existing
  file path". After C2 the recorded artefact value is a directory, so the clause was rewritten to
  require both recorded directory values and the existence of `vstest.console.exe` inside them.
- P3-T5's rationale named a PowerShell-hosted filter as the superseded alternative and cited the
  round-2 refusal of a `-Command` payload specifically. Because the refusal is now known to apply to
  every argument shape, the paragraph was rewritten so it does not imply that some other PowerShell
  invocation shape would have been available.
- Phase 0's new introductory paragraph (finding N1) was checked against every porcelain gate in the
  plan. P4-T1's two porcelain spans are deliberately unscoped but are compared before-against-after,
  so pre-existing `.claude/agent-memory/**` entries appear in both and cancel. P5-T10, P5-T11,
  P5-T12, and P5-T13 are pathspec-scoped to `UtilitiesCS`, `UtilitiesCS.Test`, and the feature
  folder. P3-T4's porcelain span is scoped to a single file. No gate is affected by the dirty
  `.claude/` state, which is what the paragraph asserts.
- P5-T11, P5-T12, and P5-T13 (finding N2) were rewritten to carry the feature-folder pathspec on
  `git commit` as well as on `git add`, matching P5-T9. P5-T12 previously described its commit only
  in prose; it now has an explicit command block, so all four commit tasks in Phase 5 are stated in
  the same form and none of them can sweep residue left staged by P3-T4's `git add -A -- UtilitiesCS
  UtilitiesCS.Test`.
- The five write targets were re-read against the "Scope" section after all of this round's edits.
  The set is unchanged: this round touched only command spellings, commit pathspecs, and prose.
  `UtilitiesCS/Threading/ProgressTrackerAsync.cs` remains outside the write-target list.

### Sibling regions re-checked in the preflight round 3 pass and found consistent

- Every command block in the plan was re-read for a surviving PowerShell-hosted payload. The three
  line-count blocks are `wc -l` and P3-T5's filter is a `grep -E` pipeline. One conditional
  PowerShell-script bootstrap remained in P0-T5 after this round and was removed in the round-5 pass
  recorded below.
- Every occurrence of the string `Measure-Object` was re-read. The one remaining occurrence is
  P0-T13's explicit prohibition on using it, which is prose about the idiom rather than a command.
- P0-T10's forward-slash paragraph was corrected in this pass to say every command block runs
  through a POSIX shell, naming the places where backslashes are deliberately retained and protected
  by double quoting.
- P3-T5's two closing rationale paragraphs named `Select-String` as the consumer of the diff file.
  Both were corrected to name the `grep` pipeline that replaced it, so the stated vacuous-pass
  mechanisms still describe the command the task actually runs.
- P5-T10's rationale asserted that P4-T1 "restored every unowned path the formatter rewrote". After
  the NB1 change P4-T1 restores nothing, because it rewrites nothing outside the owned five. The
  clause was corrected to state the new mechanism. This is the sibling-invalidation case: the NB1
  edit was in Phase 4 and the invalidated sentence was in Phase 5.
- The acceptance-criteria mapping table was re-read row by row against the `AC-MAPPING:` block. NB3
  named AC3's Implementation cell. Two further disagreements of the same class were found in this
  pass and corrected: AC4's cell used a semicolon where `AC-MAPPING:` uses a comma, and AC5's cell
  listed `P2-T1, P1-T2, P1-T5` where `AC-MAPPING:` lists `P1-T2, P1-T5, P2-T1`. All seven rows now
  match their `AC-MAPPING:` line character for character in the Implementation column.
- P1-T2's acceptance already forbade `Thread.Sleep`, `Task.Delay`, `Thread.CurrentThread.Join`,
  `SpinWait`, and `Dispatcher.PushFrame` in the new CODE. The NB2 change extends the prohibition to
  the new DOC COMMENT and adds the two tokens the code clause did not carry, `Retry`/`retries` and
  `Timeout(`, so the P1-T2 constraint is now a superset of what P3-T5 searches for. The verbatim
  test code quoted in P1-T2 was re-read against all seven tokens and contains none of them.
- P0-T7's note that "P4-T2 and P5-T10 are both written to be satisfiable against a non-empty drift
  set" was re-checked against the rewritten P4-T2 acceptance and still holds: P4-T2 now requires the
  reported set to be a subset of `BASELINE_FORMAT_DRIFT_SET`, which a non-empty baseline satisfies.
- P5-T9's `git add` was retained ahead of its new pathspec commit deliberately. A pathspec commit
  accepts only paths already known to git, and this plan creates new untracked evidence files under
  the feature folder, so removing the `git add` would make the commit fail on exactly those files.

### Sibling regions re-checked in the preflight round 2 pass and found consistent

- `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` lines 134-188 — the `DispatcherField` /
  `ForceDispatcherNull` / `RestoreDispatcher` helpers sit far from line 28, so P1-T5's attribute
  insertion cannot disturb them and the new test's mirrored idiom stays valid.
- `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` lines 1-16 — the file's `using` block
  already imports `Microsoft.VisualStudio.TestTools.UnitTesting`, so `DoNotParallelize` resolves
  without a new `using`, and the attribute-only edit adds no directive to a 514-line file.
- The acceptance-criteria mapping table and the `AC-MAPPING:` block below were re-derived from the
  task list in this pass rather than edited independently, which is what removes the AC2 and AC7
  disagreements the prior round reported.
- Every remaining `git diff` span in the plan was re-read in this pass. The two-dot form now appears
  only in P5-T10, which runs after the P5-T9 commit and is therefore not vacuous; every span that
  runs before that commit uses either the single-ref working-tree form or `--cached`.

---

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: UtilitiesCS/Threading/UiThread.cs | lines 135-140, Dispatcher property and `null!` backing field
CITATION: UtilitiesCS/Threading/UiThread.cs | line 1, nullable-enable directive
CITATION: UtilitiesCS/Threading/UiThread.cs | line 61, `Dispatcher = _syncContextForm.UiDispatcher;`
CITATION: UtilitiesCS/Threading/UiThread.cs | lines 113-125 and 147-158, lazy-init sibling properties
CITATION: UtilitiesCS/Threading/ProgressTrackerAsync.cs | line 33, `UiDispatcher = UiThread.Dispatcher;`
CITATION: UtilitiesCS/Threading/ProgressTrackerAsync.cs | line 35, first dereference `await UiDispatcher.InvokeAsync(`
CITATION: UtilitiesCS/Threading/SyncContextForm.cs | line 30, `public Dispatcher UiDispatcher { get; private set; } = null!;`
CITATION: UtilitiesCS/Threading/WpfUiDispatcher.cs | lines 24-25 and 37, parameterless ctor provider over UiThread.Dispatcher
CITATION: UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs | lines 57-67, existing InvalidOperationException precedent
CITATION: UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs | lines 45-46, default fallback provider
CITATION: UtilitiesCS.Test/Threading/UiThread_Tests.cs | 104 lines, namespace UtilitiesCS.Test.Threading, single TestClass
CITATION: UtilitiesCS.Test/UtilitiesCS.Test.csproj | line 493, Compile Include for Threading\UiThread_Tests.cs
CITATION: UtilitiesCS.Test/UtilitiesCS.Test.csproj | lines 476, 478, 489, Compile Include for ProgressTracker_Tests.cs, ProgressTrackerAsync_Tests.cs, IdleAsyncQueue_Tests.cs
CITATION: UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs | lines 141-186, DispatcherField/ForceDispatcherNull/RestoreDispatcher
CITATION: UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs | lines 248-289, AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException
CITATION: UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs | line 28, sole [TestClass] and P1-T5 insertion point; line 144, "_dispatcher" reflection write; 347 total lines
CITATION: UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs | lines 126-190, InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker
CITATION: UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs | line 13, sole [TestClass] and P1-T5 insertion point; lines 138, 150, 152, 162, reflection write then InitializeAsync then Dispatcher.PushFrame; 205 total lines
CITATION: UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs | line 12, namespace UtilitiesCS.Test; line 14, sole [TestClass] and P1-T5 edit site; line 422, "_dispatcher" reflection write; lines 411-432, Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi; 514 total lines, pre-existing file-size overage
CITATION: UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs | lines 15-16, prevailing [TestClass] then [DoNotParallelize] two-line idiom
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs | lines 117-142, YieldAsync_WithoutDispatcher_RemainsStrict
CITATION: UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs | line 55, new WpfDispatcherYield()
CITATION: UtilitiesCS.Test/Properties/AssemblyInfo.cs | line 18, assembly Parallelize attribute
CITATION: QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs | lines 26 and 64, new WpfUiDispatcher()
CITATION: QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs | lines 23-24 and 48-50, the file's only two [TestMethod] declarations, Construction_YieldsAnIUiDispatcher and Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread — the basis for P3-T6's Total tests of 2 and its two by-name clauses
CITATION: .csharpierignore | lines 4-14, evidence and project-file exclusions, including line 8 `*.trx`, which keeps the TRX files P0-T10, P0-T11, P3-T2, P3-T3, P3-T6, P4-T5, and P4-T6 read out of P4-T2's repo-wide format check
CITATION: .gitignore | line 39 `[Tt]est[Rr]esult*/` and lines 144-145 `coverage/*`, both directory-scoped, which is why backslash-stripped root-level paths would escape them and why every `TestResults/<task>/` TRX this plan reads is untracked
CITATION: coverage.config | lines 12-22, third-party-only module excludes; no first-party assembly excluded, so added production and test lines legitimately move lines-valid
CITATION: docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/spec.md | lines 249-272, AC1-AC7 (corrected in revision round 9; supersedes citation 18's round-1 "lines 234-257" reading, which predated the ## Write Set section insertion)
CITATION: docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/issue.md | line 8, merge base SHA
CITATION: global.json | lines 6-10, sdk paths [".dotnet-sdk", "$host$"] and the missing-repo-local-SDK error message; `.dotnet-sdk/` absent from this worktree
CITATION: scripts/vscode/Install-RepoDotNetSdk.ps1 | line 3 default version 8.0.205; line 26 download URL https://builds.dotnet.microsoft.com/dotnet/Sdk/$Version/dotnet-sdk-$Version-win-$Architecture.zip; line 36 install dir Join-Path $PSScriptRoot '..\..\.dotnet-sdk' — the three values P0-T5's POSIX bootstrap reproduces without running the script
CITATION: .gitignore | line 350 `.dotnet*/`, which already ignores the `.dotnet-sdk/` directory P0-T5's bootstrap creates
CITATION: .gitignore | line 191 `**/[Pp]ackages/*` and line 193 `!**/[Pp]ackages/build/`, which ignore the NuGet restore output P0-T5 step 4 creates, with the single named exception
CITATION: .github/workflows/_build-analyzers.yml | line 17 `SOLUTION_PATH: TaskMaster.sln`, line 45 `nuget restore $env:SOLUTION_PATH`, line 50 the analyzer msbuild that follows it — the CI-parity basis for P0-T5 step 4
CITATION: .github/workflows/_build-nullable.yml | line 45, `nuget restore $env:SOLUTION_PATH` preceding the nullable build
CITATION: .github/workflows/_mstest-coverage.yml | line 45, `nuget restore $env:SOLUTION_PATH` preceding the coverage run
CITATION: UtilitiesCS/packages.config | one of 18 packages.config files in this solution, which is why a missing NuGet restore fails the first build with CS0246 and missing-.targets errors
CITATION: UtilitiesCS.Test/packages.config | the test-assembly half of the same packages.config-based restore requirement
CITATION: dotnet-tools.json | repository-root local tool manifest pinning CSharpier 1.2.6; no `.config/` directory exists, so `dotnet tool restore` reads this file and performs no NuGet package restore
CITATION: .github/workflows/ci.yml | lines 21-23, format-check delegated to _format-check.yml
CITATION: .github/workflows/_format-check.yml | line 37 `dotnet tool restore`, line 41 `dotnet csharpier check .` repo-wide, the CI-parity command P4-T2 runs
CITATION: UtilitiesCS.Test/Threading/UiThread_Tests.cs | 17 blank lines of 104 total, and UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs 92 blank of 514, the measurement that disqualifies Measure-Object -Line as the counting idiom
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7
AC-MAPPING: AC1 | IMPLEMENTATION: P2-T1 | TESTS: P1-T2, P1-T4, P3-T2 | EVIDENCE: evidence/regression-testing/p1-t4-expect-fail.md, evidence/regression-testing/p3-t2-regression-green.md
AC-MAPPING: AC2 | IMPLEMENTATION: P2-T1 | TESTS: P2-T2, P4-T4 | EVIDENCE: evidence/qa-gates/p2-t2-nullforgiving-removed.md, evidence/qa-gates/p4-t4-nullable-build.md
AC-MAPPING: AC3 | IMPLEMENTATION: P0-T3 (verification, no edit) | TESTS: P3-T4 | EVIDENCE: evidence/other/p3-t4-progresstrackerasync-unmodified.md
AC-MAPPING: AC4 | IMPLEMENTATION: P1-T5 (attribute-only, no assertion changed) | TESTS: P3-T3, P3-T6 | EVIDENCE: evidence/qa-gates/p1-t5-donotparallelize.md, evidence/regression-testing/p3-t3-at-risk-tests.md, evidence/regression-testing/p3-t6-quickfiler-wpfuidispatcher.md
AC-MAPPING: AC5 | IMPLEMENTATION: P1-T2, P1-T5, P2-T1 | TESTS: P3-T5 | EVIDENCE: evidence/qa-gates/p3-t5-no-timing-tokens.md
AC-MAPPING: AC6 | IMPLEMENTATION: P4-T1, P4-T3, P4-T4 | TESTS: P4-T5, P4-T6 | EVIDENCE: evidence/qa-gates/p4-t1-format.md, evidence/qa-gates/p4-t2-format-check.md, evidence/qa-gates/p4-t3-analyzer-build.md, evidence/qa-gates/p4-t4-nullable-build.md, evidence/qa-gates/p4-t5-utilitiescs-tests.md, evidence/qa-gates/p4-t6-quickfiler-tests.md, evidence/qa-gates/p4-t8-loop-closure.md
AC-MAPPING: AC7 | IMPLEMENTATION: P2-T1 | TESTS: P4-T5, P4-T7 | EVIDENCE: evidence/baseline/p0-t10-utilitiescs-tests-coverage.md, evidence/qa-gates/p4-t7-coverage-delta.md
UNRESOLVED-GAPS: NONE

DIRECTIVE: PREFLIGHT VALIDATION ONLY
