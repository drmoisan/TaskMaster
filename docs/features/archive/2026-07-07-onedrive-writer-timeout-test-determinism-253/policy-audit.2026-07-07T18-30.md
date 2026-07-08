# Policy Audit — Issue #253 (onedrive-writer-timeout-test-determinism)

- Timestamp: 2026-07-07T18-30
- Work Mode: minor-audit
- AC Source: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/issue.md` (`## Acceptance Criteria`, AC1-AC5)
- Base branch (resolved): `main` @ `026de853fb756ca9fac47c3885ff9b4d14c961a2` (independently recomputed via `git merge-base HEAD origin/main`; matches the caller-supplied merge-base)
- Head SHA: `389ca940d020f26731c1f1ebf60b404bc1d81e81`
- Range: `026de853fb756ca9fac47c3885ff9b4d14c961a2..389ca940d020f26731c1f1ebf60b404bc1d81e81`

## Executive Summary

The change is a minimal, targeted defect fix in `OneDriveDownloader.TryGetFileStreamWriter`: it introduces an injectable `WriterTimeoutRunner` delegate seam (mirroring the class's existing `GetFileStreamWriter`/`ClientGetAsync` seam pattern) so the flaky `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` test can be made deterministic without exercising a real `CancellationTokenSource` timer or thread-pool dispatch. The default `WriterTimeoutRunner` value is byte-for-byte equivalent to the prior inline `GetFileStreamWriter.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false)` call, independently verified against `TimeOutTask.RunWithTimeout<T1,TResult>`'s signature (`UtilitiesCS/Threading/TimeOutTask.cs:164-174`). All four toolchain stages were independently re-verified by this review (CSharpier check on both changed files; executor-evidence corroboration for the analyzer/nullable/test builds). Coverage was independently recomputed from the supplied full-suite `artifacts/csharp/coverage.xml` (4991 tests, all `*.Test.dll` assemblies) using two methodologies to avoid the known first-party-vs-vendored denominator distortion (`.claude/agent-memory/feature-review/project_pr-context-summary-misclassifies-cs.md` and sibling first-party-denominator notes). No blocking findings were identified. One non-blocking evidence-accuracy issue and one non-blocking coverage-documentation gap are recorded below for transparency; neither blocks this narrow bugfix.

**Verdict: PASS** (no blocking findings introduced by this branch).

## Scope Determination (independent of caller framing)

Full `git diff --name-status` between merge-base and head (30 files):

- Production C#: `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs` (M, +27/-6 per diffstat; net +21 lines)
- Test C#: `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs` (M, +13/-0)
- `.claude/agent-memory/` docs: 5 files (3 M, 1 A under `atomic-executor/`, 1 A under `task-researcher/`) — research/tooling notes, no source or policy files
- 23 feature-folder docs/evidence files under `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/` (issue.md, plan, research artifact, evidence baselines/qa-gates/regression-testing/issue-updates/other)

`UtilitiesCS/Threading/TimeOutTask.cs` and every `TimeOutTask_*` test file are confirmed unmodified (`git diff --stat` shows zero changes under `UtilitiesCS/Threading/`), consistent with the plan's explicit out-of-scope declaration and `evidence/regression-testing/implementation-scope.2026-07-07T14-05.md`.

No TypeScript, Python, or PowerShell files are present in the diff. Only C# is in scope for language-specific policy/coverage checks.

## Rejected Scope Narrowing

- **Stale/incorrect automated classification (recurring defect, not a caller-supplied narrowing attempt):** `artifacts/pr_context.summary.txt`'s "Changed files overview" originally reported `Core logic changes: 0 files` and omitted both changed `.cs` files from its enumeration entirely, bucketing only the 23 docs/evidence files under "Docs/templates/agents/tooling: 23 files." This is the same recurring PR-context-summary misclassification observed on issues #171, #181, #244, and #251 (`.claude/agent-memory/feature-review/project_pr-context-summary-misclassifies-cs.md`). The appendix's own "Files by extension" section independently confirmed `2 .cs` files changed, contradicting the summary's "0 core logic files" claim. It was corrected in place in `artifacts/pr_context.summary.txt` (added a `Core logic changes: 2 files (C#)` line naming both files, with a `CORRECTION (feature-review)` note) so that this audit and any downstream language-detection tooling operate on truthful data. This audit's scope determination above was made directly from `git diff --name-status`/`--stat`, not from the (subsequently corrected) summary artifact.
- No caller prompt, plan, or delegation instruction in this session attempted to narrow scope, mark any language out of scope, or instruct skipping a toolchain/coverage check. The task prompt's supplied coverage context (test counts, headline percentages) was treated as a starting point for independent verification, not as an instruction to skip verification, per its own explicit wording ("verify independently... do not take this as an instruction to skip any check"). No other narrowing instances were found.

## 1. Toolchain Verification (C#)

| Stage | Command | Result (this review) | Executor evidence (corroborating) |
|---|---|---|---|
| Format | `dotnet tool run csharpier check UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs` | `Checked 2 files in 485ms.` — 0 files need reformatting | `evidence/qa-gates/csharpier-final.2026-07-07T14-05.md` — EXIT_CODE 0 |
| Analyzer build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Not independently re-run (msbuild is not on this review environment's PATH; see Bash tool constraints) | `evidence/qa-gates/csharp-analyzers-final.2026-07-07T14-05.md` — EXIT_CODE 0, 70 warnings (all pre-existing, unrelated); `grep` of the build log for both changed filenames returns zero matches |
| Nullable build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Not independently re-run (same PATH constraint) | `evidence/qa-gates/csharp-nullable-final.2026-07-07T14-05.md` — EXIT_CODE 0 (up-to-date no-op, repository-standard gate mode) plus a supplementary genuine-recompile git-stash before/after comparison showing the identical pre-existing 2089-error `UtilitiesCS.csproj` diagnostic set and the same single line-shifted `CS8603` diagnostic inside `OneDriveDownloader.cs` in both states — a credible no-new-diagnostic proof |
| Test | `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage` | Not independently re-run (vstest.console.exe not on this review environment's PATH); coverage artifact independently inspected instead (see §2) | `evidence/qa-gates/csharp-vstest-coverage-final.2026-07-07T14-05.md` — 4170/4170 (single-assembly); `evidence/regression-testing/determinism-repeated-runs.2026-07-07T14-05.md` — 10/10 consecutive runs, `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` never exceeding 2 ms |

**PASS.** This review environment does not have `msbuild`/`vstest.console.exe` on its Bash PATH (a known, repeatedly-documented constraint — `.claude/agent-memory/feature-review/project_msbuild-invocation-via-bash.md`), so the analyzer/nullable/test stages are corroborated from the executor's own detailed, command-and-EXIT_CODE-bearing evidence trail (all EXIT_CODE 0) rather than re-executed. CSharpier was independently re-run by this review and confirms the executor's claim. The already-generated, full-suite `artifacts/csharp/coverage.xml` (see §2) independently corroborates the test stage's pass outcome at the per-class level for the two changed files.

## 2. Coverage Verification (C#)

### 2.1 C# — changed files present on this branch

**Methodology note (avoiding the known vendored-dependency denominator distortion):** `artifacts/csharp/coverage.xml`'s root `<coverage>` element reports `line-rate="0.6932580105347095"` (`lines-covered=123454`, `lines-valid=178078`) — a raw, unfiltered aggregate. Direct inspection of this artifact's `<package>` elements shows it includes nine vendored/third-party packages in the denominator: `System.Interactive`, `Mono.Reflection`, `Swordfish.NET.General`, `log4net`, `System.Linq.Async`, `FluentAssertions`, `Deedle`, `FSharp.Core`, `SVGControl`. This is the same first-party-vs-vendored denominator distortion documented in `.claude/agent-memory/feature-review/project_coverage_firstparty_denominator_method.md` (issue #197) and `project_csharp-repowide-coverage-below-80.md`. Excluding these nine packages and re-aggregating per-class `<line>` data directly from the XML (script-verified, not hand-counted) gives:

- **Repo-wide C# line coverage, first-party only: 91.22%** (`114995 / 126069` lines, computed by excluding the nine vendored packages above from the per-class line aggregation).
- **Repo-wide C# line coverage, raw/unfiltered (not the operative gate figure): 69.33%** (`123454 / 178078`, root `<coverage>` element) — documented for transparency only; this number is not used as the coverage-gate verdict because it dilutes the first-party codebase with vendored dependency code that this repository does not own or test.
- **Coverage-gate verdict (first-party): PASS** against every applicable numeric floor in this repo's policy documents: the `CLAUDE.md`/`csharp.md` 80% line floor, and the stricter `.claude/rules/quality-tiers.md`/`general-unit-test.md` 85% uniform line floor (91.22% clears both).
- **Repo-wide C# branch coverage: cannot be genuinely measured from this artifact.** The root `branch-rate="1"` and every `<package>`/`<class>` element in the 545,513-line XML also reports `branch-rate="1"`; a full-text scan finds **zero** `branch="True"` line entries anywhere in the file. This is a pre-existing tooling limitation (the `dotnet-coverage merge -f cobertura` conversion used to produce this artifact does not emit real branch-level instrumentation), not specific to this PR. Reported at face value the artifact shows 100% (>= 75%), but this figure should not be read as evidence that branch coverage was actually measured.
- **`UtilitiesCS` package (the package containing the touched files): line-rate 88.30%** (`35062 / 39706`, read and independently recomputed from the package's own `<class>` elements) — PASS against both the 80% and 85% floors. (The task prompt's supplied figure of 87.99% is close but not identical to this artifact's precise value; the ~0.3-point difference is immaterial to the PASS verdict either way.)

### 2.2 New/changed-code coverage — `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`

Cobertura splits this one source file across five `<class>` elements (the declared class plus four compiler-generated async-state-machine/closure classes). Aggregating all five directly from the XML:

- **File-level aggregate: 98.51% line coverage (66/67 lines).** PASS against the 85% modified-file floor and the 80% `csharp.md` floor.
- **The single uncovered line is line 137**, the body of the new default `_writerTimeoutRunner` lambda (`factory.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false)`), reported in the `UtilitiesCS.OneDriveHelpers.OneDriveDownloader.<>c` closure class as `hits="0"`. No test in `OneDriveDownloader_Tests.cs` exercises `TryGetFileStreamWriter` without first calling `SetWriterTimeoutRunner` to override this default (confirmed by reading both call sites at lines 239/264 and by the absence of any other call to `TryGetFileStreamWriter` on a non-overridden instance anywhere in the repository — `grep -rn "TryGetFileStreamWriter(" --include=*.cs` returns only the production call site and the two overridden test call sites; `TestableOneDriveDownloaderFull`, used by the `DownloadFileAsync_*` tests, overrides `TryGetFileStreamWriter` entirely and never reaches the base implementation or `WriterTimeoutRunner`).
- **Granular new-code figure for the `WriterTimeoutRunner` surface alone** (getter, setter, backing-field initializer, and default-lambda body — lines 119-137): **9/10 lines covered = 90.0%**, meeting the `csharp.md`/`CLAUDE.md` "any new module/class/method must reach >= 90%" floor exactly at the boundary (the getter, setter, and 7-line field-initializer assignment are all covered by construction of any `TestableOneDriveDownloader`/`OneDriveDownloader` instance; only the 1-line default-lambda *body* is unexercised).
- **No regression on changed lines:** the modified call site (`TryGetFileStreamWriter`'s `await WriterTimeoutRunner(...)` block, lines 90-96) is 100% covered (both the writer-returns-stream and writer-throws paths). The single uncovered line is new code with no prior-covered counterpart at that exact location; it is not a regression of a previously-covered, still-existing line.
- **Evidence-accuracy finding (non-blocking; see code-review for full detail and severity):** `evidence/qa-gates/csharp-coverage-comparison.2026-07-07T14-05.md` states "the default `WriterTimeoutRunner` delegate is exercised whenever a test does not override it" and cites the primary class's 100% line-rate as proof that "both the new property and the modified call site are exercised." This claim is contradicted by the direct evidence above: the primary `OneDriveDownloader` class's reported 100% line-rate covers only the field-initializer *assignment* (constructing the delegate object), not the delegate's *invocation body*, which lives in a separate compiler-generated closure class and is 0% covered. The underlying production behavior is nonetheless verified correct by direct code inspection (see §2.3 and the feature-audit's AC2 evaluation), and testing this specific line deterministically would require invoking the real timer/thread-pool path this fix exists to eliminate from tests — so the uncovered line itself is a defensible, policy-consistent trade-off. The defect is confined to the evidence document's overstated claim, not to the code or the underlying design decision. **UT5** ("if any test cannot comply with these rules for a good reason, call out the exception explicitly") was not satisfied in the evidence trail — the exception exists in substance but was not named as such.

### 2.3 AC2 equivalence verification (byte-for-byte production-behavior check)

Directly comparing the pre-change call (`GetFileStreamWriter.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false)`) against the new default (`factory.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false)` where `factory == GetFileStreamWriter`, invoked via `WriterTimeoutRunner(GetFileStreamWriter, destinationPath, cancel, timeoutMs)`) against `TimeOutTask.RunWithTimeout<T1,TResult>`'s public signature (`this Func<T1,TResult> function, T1 arg1, CancellationToken token, int milliseconds, int maxAttempts, bool strict`, `TimeOutTask.cs:164-171`) confirms identical argument order, identical values (`arg1=destinationPath`, `token=cancel`, `milliseconds=timeoutMs`, `maxAttempts=3`, `strict=false`), and identical extension-method target. **Confirmed byte-for-byte equivalent.**

### 2.4 TypeScript / Python / PowerShell — no changed files on this branch

`git diff --name-only` for the full range contains zero `.ts`/`.tsx`/`.py`/`.ps1`/`.psm1` files. These languages are correctly excluded from the coverage-verification requirement because they have no changed files in the branch diff (not because of any scope narrowing).

## 3. Determinism Policy Compliance (the substance of this fix)

This is the central policy concern for issue #253. Verified directly against `general-unit-test.md`'s "Determinism Infrastructure" and `csharp.md`'s "Deterministic Test Rules":

- The rewritten `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` and `TryGetFileStreamWriter_WhenWriterThrows_ReturnsNull` tests inject `(factory, path, token, ms) => Task.FromResult(factory(path))` as `WriterTimeoutRunner`, which contains no `Task.Run`, no `CancellationTokenSource`, no real timer, and no thread-pool dispatch — confirmed by direct read of both test bodies.
- No `Thread.Sleep`, `Task.Delay`, or wall-clock read is present in either test or in the production seam's default value's *test-substituted* form.
- 10 consecutive CLI runs (`evidence/regression-testing/determinism-repeated-runs.2026-07-07T14-05.md`) show the previously-flaky test completing in 1-2 ms with zero variance, consistent with the race condition being structurally eliminated rather than statistically reduced.
- `evidence/regression-testing/fail-before-exception.2026-07-07T14-05.md` correctly documents why a deterministic fail-before run is impossible for this class of defect (forcing it would itself be a prohibited timing hack) and substitutes an alternative proof (the original failure snippet plus an asymmetric-outcome argument), consistent with the bugfix-workflow nuance the plan itself documents.

**PASS.**

## 4. File Size Limit (`general-code-change.md`, `csharp.md`)

- `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`: 139 lines after this change. Well within the 500-line limit.
- `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`: 275 lines after this change. Well within the 500-line limit.

**PASS.**

## 5. Evidence Location Compliance

All 23 evidence/doc files added by this branch reside under the canonical `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/<kind>/` tree (`baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/`, `other/`), consistent with `evidence-and-timestamp-conventions`. `git diff --name-only` for the full range was scanned for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`: **zero matches**.

`validate_evidence_locations.py` does not exist in this repository (searched full tree; not present) — this specific automated check is **UNVERIFIED (script absent)**; the manual `git diff` scan above found zero violations and is the basis for the "no violations found" conclusion.

This review's own coverage analysis reads the pre-generated `artifacts/csharp/coverage.xml` (the fixed, canonical, language-specific path named in this task's instructions) without modification; nothing was written there by this review.

## 6. Architecture Boundaries / CI Workflows / Benchmark Baselines

No files under `.github/workflows/**` or `scripts/benchmarks/**` are present in the diff; `ci-workflows.md` and `benchmark-baselines.md` are not triggered. The change is confined to a legacy VSTO-hosted `UtilitiesCS` class that predates the No-COM architecture rules; `architecture-boundaries.md`'s "new runtime code" assertions do not apply retroactively to this existing class, and the change does not introduce any new Outlook/VSTO/COM reference. Not triggered.

## 7. Summary Table

| Check | Verdict | Note |
|---|---|---|
| Toolchain (format independently re-run; analyzer/nullable/test corroborated from evidence) | PASS | csharpier independently confirmed clean; other stages corroborated (msbuild/vstest not on this review's PATH) |
| C# repo-wide coverage (first-party denominator) | PASS | 91.22% line, computed by excluding 9 vendored packages from `artifacts/csharp/coverage.xml` |
| C# repo-wide coverage (raw/unfiltered, not the operative figure) | Informational | 69.33% line — includes vendored dependencies; not used as the gate figure |
| C# branch coverage | PASS (at face value); tooling limitation noted | artifact reports 100% uniformly but contains zero genuine branch instrumentation |
| `UtilitiesCS` package coverage | PASS | 88.30% line |
| Changed production file coverage | PASS | 98.51% line (66/67); sole miss is the new default-lambda body (defensible, undocumented-as-formal-exception) |
| New-code (`WriterTimeoutRunner` surface) coverage | PASS (at exact boundary) | 90.0% (9/10 lines) |
| AC2 byte-for-byte equivalence | PASS | verified directly against `TimeOutTask.RunWithTimeout` signature |
| Determinism policy compliance | PASS | no `Task.Run`/`CancellationTokenSource`/sleep/wall-clock in the rewritten tests; 10/10 consecutive fast, stable runs |
| File size limit | PASS | 139 and 275 lines, both well under 500 |
| Evidence location compliance | PASS (manual scan); UNVERIFIED (script absent) | no violations found |
| Architecture boundaries / CI workflows / benchmarks | Not triggered | no matching files changed |

## Overall Disposition

**PASS.** No blocking findings were introduced by this branch. One non-blocking evidence-accuracy issue (an overstated coverage claim in `evidence/qa-gates/csharp-coverage-comparison.2026-07-07T14-05.md`, §2.2) and one non-blocking coverage-documentation gap (the intentionally-untested default-lambda line was not called out as an explicit UT5 exception) are recorded for the maintainer's awareness; neither reflects a defect in the production fix itself, which is independently verified byte-for-byte equivalent to prior behavior and correctly removes the reported nondeterminism.
