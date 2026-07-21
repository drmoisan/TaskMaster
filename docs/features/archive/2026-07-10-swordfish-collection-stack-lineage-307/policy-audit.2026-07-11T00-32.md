# Policy Compliance Audit — swordfish-collection-stack-lineage (#307, epic F2)

- Timestamp: 2026-07-11T00-32
- Reviewer: feature-reviewer
- Branch under review: `feature/swordfish-collection-stack-lineage`
- Base (resolved): `origin/epic/swordfish-removal-integration` (epic integration branch, NOT main)
- Merge-base: `0b72b11bb1145dd00f70fe9de8d7a6ed3bef79bb`
- Work Mode: `full-feature` (AC sources: `spec.md` + `user-story.md`)
- Verdict: **PASS** (0 Blocking findings)

## Scope Resolution (authoritative)

The caller supplied a two-dot range `origin/epic/swordfish-removal-integration..HEAD`. That two-dot
diff includes divergence noise: because the feature branch is behind the epic tip, the sibling
feature doc folders for #306/#309/#310 and `ScoSortedDictionary.cs` appear as spurious
additions/deletions that are not this feature's work. Per the Scope Invariant and
`pr-base-branch-merge-base`, the authoritative audit scope is the **three-dot merge-base diff**
`origin/epic/swordfish-removal-integration...HEAD` (standard PR semantics). All verdicts below are
computed against that three-dot scope, which is clean and focused on #307. This is not a narrowing;
it is the correct base resolution (the reviewer used the merge-base, the widest legitimate scope for
this branch's own changes).

Branch commits since merge-base include the #307 feature commits (9169a5b5, 87ada637, cc40bfd8,
e9fbf786, 77fcf06d, 6112adc2, 72a89ebf, 6c5ad999, 6e76d12b) plus merged sibling commits whose
content is common with the epic and therefore cancels in the three-dot diff.

## Rejected Scope Narrowing

None. No caller instruction attempted to narrow scope to a plan/task/phase, to a file subset, or to
mark a changed language as out of scope. The plan trailer and delegation text contained no scope
narrowing directed at feature-review.

## Changed-Language Coverage Verdicts

Only one language has changed files in the branch diff: **C#** (`*.cs`, `*.csproj`). No `*.ts`,
`*.py`, or `*.ps1` production files changed, so those languages have zero changed files and impose
no coverage obligation.

| Language | Changed files? | Coverage verdict |
|---|---|---|
| C# | Yes | **PASS** (see §Coverage) |
| TypeScript | No | N/A (zero changed files) |
| Python | No | N/A (zero changed files) |
| PowerShell | No | N/A (zero changed files) |

### Coverage (C#) — Appendix A checklist

- [x] C# coverage evidence located and parsed — `evidence/qa-gates/vstest-coverage.md` and
      `evidence/qa-gates/coverage-delta.md` (derived from `dotnet-coverage merge --output-format
      cobertura` on the vstest `.coverage` output). Evidence-verification model satisfied without
      re-running coverage generation.
- [x] New/changed-code coverage checked — new-code line coverage **98.0%** (402/410 production
      lines), exceeds the CLAUDE.md `>= 90%` new-module/method bar. Per-file: `ConcurrentObservable
      Collection.cs` 100.0% (57/57); `ConcurrentObservableCollection.Serialization.cs` 96.8%
      (243/251); `SloStack.cs` 100.0% (102/102).
- [x] No-regression on changed lines checked — re-based consumers were type-only re-points with
      unchanged control flow; their existing suites remain green (full suite 4685/0). Changed lines
      did not lose coverage.
- [x] Repo-wide figure recorded and dispositioned — Baseline: 76.59% (106,550/139,120);
      Post-change: 76.61% (106,753/139,344); Change: +0.02 pp; Disposition: **pre-existing,
      out-of-F2-scope, non-blocking** (no regression); Evidence: `evidence/qa-gates/coverage-delta.md`.

Repo-wide coverage disposition: the raw ~76.6% repo-wide figure includes vendored code
(UtilitiesSwordfish/SVGControl) and pre-exists this feature. This repo's authoritative coverage
policy is CLAUDE.md's 80% floor on the **testable (production-only, COM/VSTO/WinForms-exempt)
denominator**, not an unconditional 85% repo-wide floor. F2 does not regress the repo-wide line rate
(essentially flat, +0.02 pp), and the F2-scoped obligations (new-code >= 90%, no changed-line
regression) are met. No C# coverage artifact was emitted at `artifacts/csharp/coverage.xml` by
design (the caller and CLAUDE.md coverage policy direct that an unconditional 85%-floor artifact
must not be manufactured); the feature's `evidence/` artifacts supply the numeric verification.

## Evidence Location Compliance

All evidence for this feature is written under the canonical
`docs/features/active/2026-07-10-swordfish-collection-stack-lineage-307/evidence/<kind>/` tree
(`baseline/`, `qa-gates/`, `regression-testing/`). The three-dot diff contains **no** files written
under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. No
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` events occurred. Result: **PASS**.

## Toolchain Compliance (CLAUDE.md §8 / csharp.md CUT3)

| Stage | Command (evidence) | Result |
|---|---|---|
| Format (csharpier) | `csharpier check .` — `evidence/qa-gates/csharpier.md` | EXIT 0; 0 files require formatting — **PASS** |
| Analyzers | `msbuild ... /p:EnableNETAnalyzers /p:EnforceCodeStyleInBuild` — `evidence/qa-gates/msbuild-analyzers.md` | EXIT 0; 0 errors, 0 warnings — **PASS** |
| Nullable/TWAE | `msbuild -t:Rebuild ... /p:Nullable=enable /p:TreatWarningsAsErrors` — `evidence/qa-gates/msbuild-nullable.md` | EXIT 1; **84 vendored-only errors** (UtilitiesSwordfish 50 + SVGControl 34); **0 first-party** — **PASS (no regression)** |
| Tests (MSTest/vstest) | canonical runsettings — `evidence/qa-gates/vstest-coverage.md` | 4685 passed / 0 failed (baseline 4680/0) — **PASS** |

Nullable-gate disposition: the nullable `TreatWarningsAsErrors` build returns EXIT 1, but the
84-error set is byte-for-byte the Phase 0 vendored-only baseline
(`evidence/baseline/nullable-baseline-errors.txt`), with **zero** first-party diagnostics. Per
csharp.md the operative first-party type-safety gate is the analyzer build (green). This is a
documented pre-existing baseline, not a regression introduced by F2. **Not a Blocking finding.**

## Policy Sections

### General Code Change Policy

| Requirement | Finding | Verdict |
|---|---|---|
| Separation of concerns (pure logic vs I/O) | Collection/stack logic is host-neutral; filesystem/prompt isolated behind `IConcurrentObservableCollectionFileSystem`/`...Prompt` injectable seams | PASS |
| Fail-fast error handling | `Pop()`/`Peek()` throw `InvalidOperationException` on empty; `NodeAt` throws `IndexOutOfRangeException`; `Subscribe` guards null observer | PASS |
| File size <= 500 lines (new files) | Largest new file `SloStack.cs` 260; `SloStack_Tests.cs` 402; `ConcurrentObservableCollection_Tests.cs` 398; all new files < 500 | PASS |
| File size <= 500 lines (touched files) | `AppToDoObjects.cs` = 503 (baseline 502, net +1 from a type-only re-point). Pre-existing >500 condition, not introduced by F2 | PARTIAL (non-blocking; pre-existing — see §Observations) |
| Logging pattern | Uses existing `log4net` logger; no ad-hoc console output added | PASS |
| No new dependencies | csproj diffs add/remove `<Compile>` entries only; no new package/ProjectReference | PASS |
| Public API compatibility | Return types re-pointed at interface members `IAppAutoFileObjects.Filters/MovedMails`, `IToDoObjects.PrefixList/LoadPrefixList`, `IQfc*`; in-repo callers updated; behavior preserved | PASS |

### General / C# Unit Test Policy

| Requirement | Finding | Verdict |
|---|---|---|
| Framework = MSTest | New tests use `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`; no NUnit/xUnit introduced | PASS |
| Assertions = FluentAssertions | New round-trip/undo/SloStack/collection tests use `.Should()...` | PASS |
| Mocking = Moq | Injectable seams (`IConcurrentObservableCollectionFileSystem`) enable Moq-based FS tests in `ConcurrentObservableCollectionSerialization_Tests`; no framework substitution | PASS |
| No temp files in tests | Round-trip fixtures are in-memory strings; grep for `GetTempPath`/`GetTempFileName`/`File.Create*` in new tests returned none | PASS |
| Determinism | No `Thread.Sleep`/`Task.Delay`/`DateTime.Now`/`DateTime.UtcNow` in new tests; deterministic inputs; AAA structure | PASS |
| Independence/isolation | Each new test constructs its own fixtures; no shared mutable state | PASS |
| Coverage exclusion of production files | Two default seam classes carry `[ExcludeFromCodeCoverage]` (File.* I/O passthrough; MyBox WinForms prompt) | PASS under CLAUDE.md exemption (see §Observations for rule-file tension) |

### C# Code Change Policy

| Requirement | Finding | Verdict |
|---|---|---|
| Nullable enabled, no new first-party warnings | 0 first-party nullable diagnostics (evidence) | PASS |
| Strong contracts / explicit APIs | Positional stack members carry XML docs stating ordinal semantics; `Subscribe` documents replay semantics | PASS |
| Cohesive types / focused responsibility | `SloStack<T>` isolates O(n) positional members from Recents-shared `SloLinkedList<T>`; serialization split into a partial | PASS |
| csharpier formatting | EXIT 0 | PASS |

## Observations (non-blocking)

1. **`AppToDoObjects.cs` = 503 lines.** Baseline was 502 (already over the 500 limit); F2 added a net
   +1 line via a type-only re-point (`ScoCollection<IPrefix>` → `ConcurrentObservableCollection
   <IPrefix>`). Pre-existing condition, not materially introduced by F2. Recommend a follow-up split
   outside this feature's scope. Non-blocking.
2. **`ConcurrentObservableCollection(byte[] file)` constructor discards the `DeserializeJson(byte[])`
   return value** (`ConcurrentObservableCollection.Serialization.cs:33-37`), so it constructs an
   empty instance. This is a latent no-op **ported verbatim** from the legacy `ScoCollection` byte[]
   constructor (confirmed against merge-base) and is documented in
   `ConcurrentObservableCollectionSerialization_Tests.cs:34-35`. No first-party consumer uses the
   byte[] constructor to populate. Behavior-preservation mandate honored. Info-level.
3. **Coverage-policy tension between `CLAUDE.md` and `.claude/rules/general-unit-test.md`.** The rule
   file states no production file may be excluded from coverage and uses an 85% floor; CLAUDE.md
   ratifies `[ExcludeFromCodeCoverage]` for WinForms/host-bound/I/O-passthrough classes and an 80%
   testable-denominator floor. Per `policy-compliance-order`, CLAUDE.md has precedence (#1). The two
   excluded seam classes are the thinnest-possible host-bound wiring (one-line File.*/MyBox
   passthroughs), which is exactly the refactor the rule file recommends; testable logic sits in the
   collection at 100%/96.8% via injectable seams. Compliant under CLAUDE.md. Documented tension only.

## Appendix A — Coverage Verification Detail

- Language with changed files requiring coverage: C# only.
- Evidence source (not re-run): `evidence/qa-gates/coverage-delta.md`, `evidence/qa-gates/vstest-coverage.md`.
- New-code line coverage: **98.0%** (>= 90% bar) — PASS.
- Repo-wide line rate: Baseline 76.59% → Post-change 76.61% (+0.02 pp) — no regression; pre-existing
  vendored-inclusive figure dispositioned non-blocking per CLAUDE.md testable-denominator floor.
- Branch-rate note: the `.coverage`→Cobertura conversion emits branch rate 1.0 and is not reliable
  for per-branch data; line-coverage figures are authoritative for both the no-regression and
  new-code assessments (documented in `vstest-coverage.md`).
