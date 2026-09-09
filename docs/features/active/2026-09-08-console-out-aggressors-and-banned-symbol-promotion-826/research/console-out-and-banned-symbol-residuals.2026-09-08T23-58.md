# Research — Console.Out aggressors and banned-symbol promotion (issue #826)

- **Issue:** #826
- **Feature folder:** `docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/`
- **Researcher:** task-researcher
- **Timestamp:** 2026-09-08T23-58
- **Tooling available to this session:** Read / Grep / Glob only. No Bash, no PowerShell, no
  msbuild, no vstest. Every statement about what a build or a test run *would* produce is labelled
  **PREDICTION** and is not a measurement.

---

## 0. Corrections to the delegation brief

Three items in the brief disagree with the current tree. All three change plan content.

### 0.1 `Directory.Build.props` exists (brief and CLAUDE.md both say it does not)

`CLAUDE.md` §C#1.3 states "no project in this repository carries a `<Nullable>` element and there is
no `Directory.Build.props`". The second clause is **false as of this tree**:

- `Directory.Build.props` (18 lines) exists at the repository root and sets exactly one property,
  `<RxUseUnsupportedPackagesConfig>true</RxUseUnsupportedPackagesConfig>` (line 16), added under
  issue #730.
- `Directory.Build.targets` (30 lines) also exists and conditions `SignManifests` / `SignAssembly`
  on `$(CI)` for the `TaskMaster` project.

The `<Nullable>` half of the CLAUDE.md sentence remains accurate (no `.csproj` carries a `<Nullable>`
element — verified by the grep in §1.4). Only the "no `Directory.Build.props`" clause is stale.

**Why this matters for #826:** `Directory.Build.props` is a real, already-present, solution-wide
MSBuild property injection point. It is the only mechanism in this repository that could carry a
`WarningsNotAsErrors` property without editing 18 legacy `.csproj` files. §1.5 evaluates it and
rejects it, but the plan must reject it *on the merits*, not on the false premise that the file does
not exist.

### 0.2 Five affected test projects, not six

The brief asks me to "name the six affected projects". The 33 in-scope files span **five** projects:

| Project | In-scope files |
|---|---|
| `UtilitiesCS.Test` | 22 |
| `QuickFiler.Test` | 6 |
| `ToDoModel.Test` | 3 |
| `TaskMaster.Test` | 1 |
| `VBFunctions.Test` | 1 |
| **Total** | **33** |

Derivation in §3.1. `spec.md` line 14 says "roughly 24 test classes"; the verified figure is 33 files
/ 34 live install statements. Both the `spec.md` estimate and the brief's project count need
correcting.

### 0.3 `spec.md` cites lines 78 and 96; the current tree has 79 and 97

`spec.md` line 57 says `OlTableExtensions.TableAccess.cs` lines **78 and 96**. The brief says **79
and 97**. The brief is correct against the current tree:

- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:79` —
  `Console.WriteLine($"Task timed out on try {counter}");`
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:97` — identical text.

**These line numbers are volatile and must not be written into the plan as anchors.**
`docs/features/epics/review-residuals-2026-09-08/epic.md:129-133` records that feature **825** owns
the 2000 ms deadline window and timeout mechanics of the same method (`GetTableInViewAsync`,
lines 32-119), that 826 carries the epic's single `depends_on` edge, and that 826 "executes in wave 1
against a `TableAccess.cs` that already contains 825's changes." The plan must locate these two sites
by their exact text (`Console.WriteLine($"Task timed out on try {counter}");`) and by their enclosing
`catch` clause, not by line number.

---

## 1. Q1 — Is promoting RS0030 above `suggestion` reachable?

### 1.1 Answer

**No. Not without either (a) clearing the pre-existing usages first, or (b) using a scoping mechanism
that this repository does not have and that its own policy forbids introducing.** Both (a) and (b)
are out of reach inside this bugfix. The largest reachable subset is stated in §1.7.

### 1.2 The failing gate and the precise mechanism

The breaking step is **toolchain step 3**, not step 2:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

Mechanism, in order:

1. `BannedSymbols.txt` is supplied to the compiler as an `AdditionalFiles` item in 16 of 18 projects
   (`<AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />`, e.g.
   `UtilitiesCS/UtilitiesCS.csproj:1316`, `QuickFiler/QuickFiler.csproj:603`).
2. `Microsoft.CodeAnalysis.BannedApiAnalyzers` reports **RS0030** once per call site that resolves to
   a listed DocID.
3. `.editorconfig:548` currently sets `dotnet_diagnostic.RS0030.severity = suggestion`. A
   `suggestion` is emitted at **info** (`IDE`/message) level. `TreatWarningsAsErrors` promotes
   *warnings*; it does not promote info-level diagnostics. So today RS0030 cannot fail any build.
4. Changing that line to `= warning` makes every RS0030 report a compiler *warning*.
   `/p:TreatWarningsAsErrors=true` (with no `WarningsNotAsErrors` anywhere — see §1.4) promotes every
   warning to an error. The build fails at the first project that contains a banned call site.

**PREDICTION (not measured):** the failure count would be on the order of the ~143 figure recorded in
the `.editorconfig:545-547` comment. I could not run msbuild, so I cannot state the exact diagnostic
count. My raw textual counts are in §1.3 and are deliberately *not* presented as diagnostic counts —
they differ (a `using static` alias, a commented line, or a same-line double call all break the
one-hit-per-diagnostic assumption in both directions).

### 1.3 Textual banned-symbol surface (informational, not a diagnostic count)

Independently re-derived; agrees with the brief.

| Banned DocID | Textual `*.cs` hits |
|---|---|
| `P:System.DateTime.Now` | 54 |
| `P:System.DateTime.UtcNow` | 20 |
| `P:System.Random.Shared` | 5 |
| `M:System.Threading.Thread.Sleep(*)` | 15 |
| `M:System.Threading.Tasks.Task.Delay(*)` | 58 |
| **Total textual** | **152** |

152 textual hits versus the recorded ~143 diagnostics. The gap is expected and is exactly why a
textual count cannot substitute for a build measurement.

### 1.4 Which CI gate breaks

- **`.github/workflows/_build-nullable.yml`** — this is the gate that breaks. Its step "Build with
  nullable warnings treated as errors" (lines 63-76) runs
  `msbuild $env:SOLUTION_PATH /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  and exits non-zero on `$LASTEXITCODE`. It uses `/t:Rebuild`, so there is no incremental escape.
- **`.github/workflows/_build-analyzers.yml`** — this gate does **not** break. Its step (lines 66-69)
  runs `/t:Build /m ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` with **no**
  `TreatWarningsAsErrors`. Warnings there are reported and ignored; the step exits 0.

So the analyzer gate is not the constraint. The nullable gate is. This inverts the intuitive reading
and the plan should state it explicitly.

### 1.5 Scoping mechanisms — availability and approval status

| Mechanism | Present in repo? | Can scope RS0030 to new code? | Approved here? |
|---|---|---|---|
| Path-scoped `.editorconfig` sections | **No.** Exactly one `.editorconfig` (repo root; `Glob **/.editorconfig` → 1 result). It has exactly three sections: `[*.cs]` (line 1), `[*.vb]` (line 590), `[*.{cs,vb}]` (line 612). All are language-scoped; **none** is path-scoped. | Only by *directory*, never by "new code". Would have to enumerate every clean directory and would silently stop protecting a directory the moment a legacy file moved into it. | Not used anywhere today. Introducing path-scoped severity is a new governance primitive. |
| `WarningsNotAsErrors` / `NoWarn` | **No.** Grep across all 18 `.csproj` for `WarningsNotAsErrors\|NoWarn\|AnalysisLevel\|AnalysisMode\|TreatWarningsAsErrors` returns **zero** property hits (only the `AdditionalFiles`/comment lines shown in §1.2). `Directory.Build.props` sets only `RxUseUnsupportedPackagesConfig`. | Technically yes — `WarningsNotAsErrors=RS0030` in `Directory.Build.props` would make RS0030 a non-fatal warning solution-wide. | **No.** This is precisely the "bulk-suppress the pre-existing usages" move the brief prohibits, wearing an MSBuild hat. It also directly contradicts `.claude/rules/csharp.md:87`, which records that the SecurityCodeScan deferral was taken specifically so that **"no `<WarningsNotAsErrors>` containing CS8032"** had to be introduced. The repository has an explicit, documented precedent of *declining* this exact mechanism. |
| `.globalconfig` | **No.** `Glob **/.globalconfig` and `**/*.globalconfig` both return zero files, despite `CLAUDE.md` §C#1.2 naming it as an analyzer-config input. | A `.globalconfig` offers no scoping `.editorconfig` lacks; it is strictly weaker (no per-path sections at all, only `global_level` ordering). | Not present; would be a new file with no precedent. |
| Per-project `<AdditionalFiles>` scoping of a *different* BannedSymbols file | Partially. `Glob **/BannedSymbols*.txt` returns exactly **one** file. But the wiring is already per-project (16 explicit `<AdditionalFiles>` items), so a second file is mechanically possible. | Yes, at *project* granularity: a clean project could point at a stricter list. | Weak fit. There is no clean first-party project — the banned symbols are spread across `UtilitiesCS`, `QuickFiler`, `TaskMaster` and all the `.Test` projects. It also forks the source of truth, which `.claude/rules/csharp.md:79` describes as a single "repo-root `BannedSymbols.txt`". |
| `AnalysisLevel` / `AnalysisMode` | **No** (zero hits, same grep as row 2). | **No.** These govern the built-in CA rules shipped with the .NET SDK. RS0030 comes from a third-party NuGet analyzer wired by explicit `<Analyzer Include>` items; `AnalysisLevel` does not reach it. | Irrelevant. |
| Repository-local mechanism | **None found.** No custom MSBuild target, no diff-scoped analyzer runner, no baseline/suppression file (`Glob` found no `*.GlobalSuppressions*` baseline for RS0030 and no `AnalyzerReleases` file). | — | — |

### 1.6 The severity-first ordering invariant (verbatim)

`.claude/rules/csharp.md:81-83`:

> ### Severity-first ordering invariant
>
> All new analyzer rule severities are configured in `.editorconfig` at `severity = suggestion`
> (never `warning`/`error`) BEFORE any `<Analyzer Include>` item is wired into a project. This is
> required because the type-check toolchain step runs `msbuild ... /p:TreatWarningsAsErrors=true`,
> which promotes any `warning`-severity analyzer diagnostic to a build error. Keeping new analyzer
> diagnostics at `suggestion` (message level) prevents the analyzer adoption from breaking the
> protected nullable gate.

And `.claude/rules/csharp.md:79`, on RS0030 specifically:

> RS0030 is held at `severity = suggestion` for initial rollout (existing call sites are not
> build-broken); promotion to `warning` after legacy cleanup is documented follow-up work.

The rule text is unambiguous: the promotion is gated on "**after legacy cleanup**". Clearing ~143
call sites is not a bugfix; it is a `TimeProvider` migration across five production assemblies. It is
out of scope for #826 under the General Code Change Policy's "Change only what is needed" bugfix
rule, and `spec.md:139-140` already concedes the sequencing ("the cleanup must land before the
promotion or the nullable gate breaks").

### 1.7 Recommendation for Q1 — the largest reachable subset

**Do not change `dotnet_diagnostic.RS0030.severity`.** State plainly in the plan that severity
promotion is unreachable in this feature and why.

The largest reachable subset is **content-only, severity-unchanged**:

1. Add the safe DocID subset from §2 to `BannedSymbols.txt` at the unchanged `suggestion` severity.
2. Amend the `.editorconfig:545-547` comment to record (i) the current verified textual surface, and
   (ii) the concrete precondition for promotion, so the next reader does not re-derive it.
3. File the cleanup-then-promote work as a follow-up issue rather than attempting it here.

**Honesty requirement the plan must carry:** adding a DocID while severity is `suggestion` produces
**zero build enforcement**. It changes IDE squiggles and nothing else. It does not make `AC5`-style
timing-hack constraints build-enforced, which is the stated motivation in `spec.md:68-70`. If the
plan claims otherwise it is wrong. The honest value of the addition is that it pre-stages the list so
the eventual promotion is a one-line severity change rather than a list-design exercise — and that
value is real but modest. If the feature owner wants build enforcement in *this* feature, the only
truthful answer is that it is not available.

---

## 2. Q2 — Exact DocID strings and current usage counts

BannedApiAnalyzers resolves an invocation to its **original definition**, so a call through a derived
type (e.g. `AutoResetEvent.WaitOne()`) is matched by the DocID of the *declaring* type
(`System.Threading.WaitHandle`). Each overload needs its own line; there is no wildcard syntax.
Format is `<DocID>;<message>`.

### 2.1 `CancelAfter` — SAFE, trivial

Complete overload family on `System.Threading.CancellationTokenSource`:

```
M:System.Threading.CancellationTokenSource.CancelAfter(System.Int32);Do not use CancelAfter to bound a test or an operation. Inject System.TimeProvider (FakeTimeProvider in tests) so the deadline is deterministic.
M:System.Threading.CancellationTokenSource.CancelAfter(System.TimeSpan);Do not use CancelAfter to bound a test or an operation. Inject System.TimeProvider (FakeTimeProvider in tests) so the deadline is deterministic.
```

Current usage: **1 call site, test-only, `int` overload.**
`QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs:169` — `tokenSource.CancelAfter(25);`
Zero production usages. Zero `TimeSpan`-overload usages.

### 2.2 `WaitOne` — SAFE, all test-only

Complete overload family on `System.Threading.WaitHandle` (.NET Framework 4.8.1):

```
M:System.Threading.WaitHandle.WaitOne;Do not block on a WaitHandle in a test. Await a Task or use an injected TimeProvider so the wait is deterministic and cannot hang the run.
M:System.Threading.WaitHandle.WaitOne(System.Int32);<same message>
M:System.Threading.WaitHandle.WaitOne(System.TimeSpan);<same message>
M:System.Threading.WaitHandle.WaitOne(System.Int32,System.Boolean);<same message>
M:System.Threading.WaitHandle.WaitOne(System.TimeSpan,System.Boolean);<same message>
```

(The parameterless overload's DocID carries no parentheses — that is the correct DocID form for a
zero-arity method and is a common authoring error.)

Current usage: **13 call sites across 12 files, 100% test code, 0 production.**

| Receiver type | Sites |
|---|---|
| `AutoResetEvent` (→ `WaitHandle.WaitOne()`) | 11 |
| `SemaphoreSlim.AvailableWaitHandle` (→ `WaitHandle.WaitOne()`) | 1 (`QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:397`) |
| `CancellationToken.WaitHandle` (→ `WaitHandle.WaitOne(Int32)`) | 1 (`QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:57`, `WaitOne(0)`) |

The 11 `AutoResetEvent` sites are all the `private readonly AutoResetEvent _ready = new AutoResetEvent(false);`
pattern (declarations at e.g. `UtilitiesCS.Test/Threading/UiThread_Tests.cs:321,431`,
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyReaderTests.cs:404`). No `Monitor`,
`ManualResetEvent`, or `Mutex` receivers exist.

### 2.3 `new CancellationTokenSource(int)` — SAFE, and cheaper than the brief feared

Constructor DocIDs:

```
M:System.Threading.CancellationTokenSource.#ctor(System.Int32);Do not construct a CancellationTokenSource with a wall-clock deadline. Inject System.TimeProvider and use the TimeProvider-aware overload so the deadline is deterministic.
M:System.Threading.CancellationTokenSource.#ctor(System.TimeSpan);<same message>
```

The parameterless `#ctor` is **not** a candidate and must **not** be listed.

**Breakdown of the 170 textual hits (this is the number the brief asked for):**

| Form | Hits | Files | Location |
|---|---|---|---|
| `new CancellationTokenSource()` — parameterless, NOT banned | **157** | 80 | mixed test/production |
| `new CancellationTokenSource(<arg>)` — banned candidate | **13** | 3 | **100% production, 0 test** |
| **Total** | **170** | — | matches the brief's 170 exactly |

All 13 parameterized sites bind to the **`Int32`** overload; **zero** bind to `TimeSpan`:

| File | Sites | Argument |
|---|---|---|
| `UtilitiesCS/Threading/TimeOutTask.cs` | 10 (lines 53, 119, 200, 274, 358, 436, 506, 588, 670, 752) | `ms` / `milliseconds` (`int`) |
| `QuickFiler/Controllers/QfcQueue.cs` | 2 (lines 50, 101) | `timeout`, declared `int timeout` at `QfcQueue.cs:48` and `:88` |
| `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs` | 1 (line 295) | `timeout`, declared `int timeout` at `ConversationHelper.cs:281` |

So the addition is **cheap in count (13) but concentrated in the timeout infrastructure itself**. Ten
of the thirteen are inside `TimeOutTask.cs`, which is the repository's own timeout primitive — the
one place where constructing a deadline source is the legitimate job of the code. Banning the symbol
would flag the implementation of the abstraction that the ban exists to promote.

**Consequence for the plan:** if this DocID is ever promoted to `warning`, `TimeOutTask.cs` will need
either a `#pragma warning disable RS0030` with a documented rationale at each of its 10 sites, or a
`RS0031`-style allow-list. That is a real, foreseeable cost and the plan should record it now rather
than discover it at promotion time. At `suggestion` severity it costs nothing today.

### 2.4 `TimeoutAfter` — **CANNOT BE BANNED. Repository-local, and it is the prescribed remedy.**

This is the most consequential finding in Q2 and it contradicts the framing of the issue.

**`TimeoutAfter` is not a BCL member.** It is a repository-local extension method defined in
`UtilitiesCS/Threading/TimeOutTask.cs`, in `namespace UtilitiesCS` (line 11), on
`public static class TimeOutTask` (line 13). Four overloads:

| Line | Signature |
|---|---|
| 824 | `public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout, int repeatAttempts)` |
| 862 | `public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout, TimeProvider? timeProvider = null)` |
| 924 | `public static Task TimeoutAfter(this Task task, int millisecondsTimeout, int repeatAttempts)` |
| 949 | `public static Task TimeoutAfter(this Task task, int millisecondsTimeout, TimeProvider? timeProvider = null)` |

**Can BannedApiAnalyzers express it?** Yes, mechanically — DocIDs are namespace-qualified and work
for first-party symbols. A generic-arity-1 method takes a `` `1 `` suffix and generic parameters are
positional (`` `` `0 `` ``), e.g.
`M:UtilitiesCS.TimeOutTask.TimeoutAfter``1(System.Threading.Tasks.Task{``0},System.Int32,System.Int32)`.
Getting all four of those DocID strings right by hand is error-prone and unverifiable without a
build, which this session cannot run.

**But it must not be banned, for a substantive reason.** The two `TimeProvider`-accepting overloads
(lines 862 and 949) carry this XML doc (lines 858-861 / 945-948):

> production; tests pass a `FakeTimeProvider` so the timeout fires only when the fake clock is
> advanced, making the timeout-versus-completion race deterministic.

`TimeoutAfter` **is** the injected-time-abstraction seam that `.claude/rules/csharp.md:55-63`
prescribes and that every message in the current `BannedSymbols.txt` points callers *toward*. Banning
it would ban the cure. Its 62 textual occurrences across 11 files break down as ~11 definition and
recursive-call lines inside `TimeOutTask.cs` itself, ~44 in the four `TimeOutTask*` test files that
exist to cover it, and the remainder in `DfDeedle*`, `OlTableExtensions.Etl.cs` and
`OlTableExtensions.cs` — i.e. the surface is large precisely because the seam is being adopted.

**Recommendation: drop `TimeoutAfter` from the candidate list and record the reason.** The issue text
lists it, so the plan must explicitly close it out rather than silently omit it, and `spec.md:138`
should be corrected — note that `spec.md:138` already omits `TimeoutAfter` from its own shortlist
("consider adding `CancelAfter`, `WaitOne` and `new CancellationTokenSource(int)`"), so the spec and
the issue body already disagree. This research resolves that disagreement in the spec's favour.

### 2.5 Q2 recommendation

Add **9 lines** covering **3 symbol families**, at unchanged `suggestion` severity:

- `CancelAfter` — 2 overload lines. Introduces 1 suggestion (test-only).
- `WaitHandle.WaitOne` — 5 overload lines. Introduces 13 suggestions (test-only).
- `CancellationTokenSource.#ctor(Int32)` + `#ctor(TimeSpan)` — 2 lines. Introduces 13 suggestions
  (production-only, 10 of them in `TimeOutTask.cs`).

Total new suggestions: **27**. Total new build failures: **0** (PREDICTION — severity is `suggestion`,
so no warning is emitted and `TreatWarningsAsErrors` has nothing to promote; §1.2 mechanism).

Exclude `TimeoutAfter` per §2.4.

Repeat the honesty caveat from §1.7 in the plan: these 27 diagnostics are IDE-only and enforce nothing.

---

## 3. Q3 — Is removing the console-writer install behaviour-preserving?

### 3.1 Population (see §7 for the full derivation evidence)

- `Console.SetOut(` appears in **35 distinct `.cs` files**, 38 occurrences.
- Minus `TaskMaster/ThisAddIn.cs:103` — production, explicitly out of scope.
- Minus `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs:31` —
  commented out (`//Console.SetOut(new DebugTextWriter());`), installs nothing.
- Leaves **33 test files** carrying **34 live install statements** (`ObsoleteBayesianClassifier_Tests.cs`
  has two, at lines 61 and 476, because the file contains two `[TestClass]` types each with its own
  `[TestInitialize] public void TestInitialize()`).
- Two further **commented** installs exist inside commented-out `[ClassInitialize]` blocks at
  `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs:22` and
  `.../TreeNodeTests_UnfinishedStubs.cs:20`. These are dead text; deleting them is optional tidy-up,
  not part of the behavioural change.

Note: `ToDoModel.Test/Data Model/` contains a **space** in the directory name. Any tooling or
`<Compile Include>` reference in the plan must quote it.

### 3.2 What `DebugTextWriter` actually does

`UtilitiesCS/HelperClasses/Logging/DebugTextWriter.cs` (50 lines, `namespace UtilitiesCS`):

- `public class DebugTextWriter : StreamWriter` (line 12), constructed as
  `base(new DebugOutStream(), Encoding.Unicode, 1024)` with `AutoFlush = true` (lines 14-18).
- The nested `sealed class DebugOutStream : Stream` (line 20) implements exactly one meaningful
  operation: `Write(byte[], int, int)` → `Debug.Write(Encoding.Unicode.GetString(buffer, offset, count))`
  (lines 22-25). `Flush()` → `Debug.Flush()` (line 31).
- **It is write-only and non-readable.** `CanRead => false`, `CanSeek => false` (lines 27-28).
  `Read`, `Seek`, `SetLength`, `Length` and `Position` all `throw bad_op`
  (`new InvalidOperationException()`, lines 33-47).

**Therefore anything written to it goes to `System.Diagnostics.Debug` and is unrecoverable.** There
is no buffer, no backing store, and no API by which a test could read back what was written. This is
the decisive fact for Q3: *no test can possibly assert on content routed through a `DebugTextWriter`,
because the type physically cannot return it.* Under a Release build with `DEBUG` undefined,
`Debug.Write` compiles away entirely and the output goes nowhere at all.

Two nested test-local re-declarations exist at `UtilitiesCS.Test/DeedleTests.cs:28` and
`UtilitiesCS.Test/Extensions/DeedleTests.cs:27`. Neither file calls `Console.SetOut`; they are
definitions only and are out of scope.

### 3.3 Is removal behaviour-preserving? Yes.

- **No test asserts on console content.** `spec.md:37-38` records that #811 removed the last four
  capture-and-assert sites and that "after #811 no test asserts on `Console.Out` content". I
  re-verified the one remaining suspicious site: `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs`
  has exactly one `Console.` reference, at line 1640, and it is a **comment**
  (`// The null-writer path resolving to Console.Out; this file is at its line ceiling.`) above an
  assertion that only checks `NotThrow()` and `MoveToStart()` (lines 1641-1642). Nothing reads console
  content.
- **No class holds the writer in a field it later reads.** Only two of the 33 files use a field at
  all: `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs:16` and
  `.../TreeNodeTests_UnfinishedStubs.cs:14`, both `private DebugTextWriter tw;`. A grep for
  `DebugTextWriter|\btw\b` across all of `ToDoModel.Test` returns **only** the declaration, the
  commented block, the assignment, and the `SetOut` call — **`tw` is never read**. The field is
  write-only in both files.
- **The exercised production code writes nothing that matters.** The redirect exists, per
  `TaskMaster/ThisAddIn.cs:101` ("Redirect the console output to the debug window for Deedle
  df.Print() calls"), to make Deedle's `Print()` land in the VS Debug window during interactive
  debugging. That is a developer-convenience behaviour, not a test assertion.

**Conclusion: removal is behaviour-preserving.** The only observable difference is that any
`Console.Write` performed by code under test lands on the real console (or the vstest captured-output
stream) instead of the Debug window.

### 3.4 Removal versus a restoring scope — recommendation

**Recommend removal, and reject the restoring scope.** Reasons, strongest first:

1. **A restoring scope does not fix the defect; it multiplies it.** `TaskMaster.runsettings:4-7` and
   `scripts/vscode/TaskMaster.cli.runsettings` both set `<Workers>0</Workers>` and
   `<Scope>ClassLevel</Scope>` — class-level parallelism with unbounded workers. `Console.SetOut` is
   **process-global**. A save/restore pair in a `[TestInitialize]`/`[TestCleanup]` running
   concurrently across 33 classes interleaves: class A saves the writer that class B just installed,
   then restores B's writer as if it were the original. That is strictly worse than the current
   unrestored install, which at least converges to a single stable writer. `spec.md:14` records that
   #811 already "removed the propagating save/restore in `NLogTraceWriter_Test`" — the repository has
   already rejected this pattern once, for this reason.
2. **The install serves no test purpose** (§3.2, §3.3).
3. **Removal is strictly smaller.** It deletes lines; it adds none.

### 3.5 Initializers that become empty (delete the method) versus must be kept

**11 of the 34 install statements are the only executable statement in their initializer.** Deleting
the statement leaves an empty method body, which should be deleted with it.

| # | File | Init method | Line | Post-removal state |
|---|---|---|---|---|
| 1 | `VBFunctions.Test/ComputerInfo_Test.cs` | `[TestInitialize] Initialize()` | 15 | empty → delete method |
| 2 | `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs` | `[TestInitialize] TestInitialize()` | 12 | empty → delete |
| 3 | `UtilitiesCS.Test/Extensions/Frexp_Test.cs` | `[TestInitialize] TestInitialize()` | 16 | empty → delete |
| 4 | `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs` | `[TestInitialize] TestInitialize()` | 16 | empty → delete |
| 5 | `UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs` | `[TestInitialize] TestInitialize()` | 21 | empty → delete |
| 6 | `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs` | `[TestInitialize] TestInitialize()` | 34 | empty → delete |
| 7 | `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs` | `[TestInitialize] TestInitialize()` (class 1) | 61 | empty → delete |
| 8 | `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs` | `[TestInitialize] TestInitialize()` (class 2) | 476 | empty → delete |
| 9 | `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs` | `[TestInitialize] TestInitialize()` | 21 | leaves only comment line 22 → delete method + comment |
| 10 | `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs` | `[TestInitialize] TestInitialize()` | 24 | leaves only comment line 25 → delete method + comment |
| 11 | `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` | `[TestInitialize] TestInitialize()` | 23 | leaves only comment line 24 → delete method + comment |

Rows 9-11 leave a comment-only body. An empty-but-for-a-comment `[TestInitialize]` is dead weight;
delete the method and the orphaned comment together.

**The remaining 23 install statements sit in initializers that do other work and MUST be kept** —
they construct `MockRepository` instances, mocks, and fixtures. Representative examples:
`UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs:23` (followed by `mockRepository`
construction at :24), `QuickFiler.Test/Controllers/QfcFormControllerTests.cs:92` (followed by
`_mockGlobals`/`_mockAF` setup at :93-96),
`UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs:38` (followed by
`mockRepository`/`mockGlobals`/`mockMailItem` at :39-41). In these, delete **only** the one line.

### 3.6 Declarations that become unused — the compiler-warning risk

This is the part of Q3 with a real gate consequence, and it is narrower than the brief anticipated.

**Unused `using` directives: NOT a risk.** IDE0005 ("Remove unnecessary using directive") is only
reported by the command-line compiler when a documentation file is generated. A grep for
`GenerateDocumentationFile` across all 18 `.csproj` returns **zero hits** (the same grep returns 13
`LangVersion` hits, confirming the grep itself works). Therefore IDE0005 is not emitted by either
msbuild step for any project in this solution, and a `using System;` or `using UtilitiesCS;` left
unused after removal **cannot fail the build**. `.editorconfig` additionally contains no `IDE0005`
entry at all (grep for `IDE0005|CS0169|CS0414|CS0649|IDE0051` → no matches), so the catch-all
`dotnet_analyzer_diagnostic.severity = suggestion` at line 27 would govern it in the IDE anyway.
Leaving the usings in place is safe; removing them is cosmetic and optional.

**Unused private fields: A REAL RISK, in exactly two files.** `CS0169` ("field is never used") and
`CS0414` ("field is assigned but its value is never used") are **compiler** warnings, not analyzer
diagnostics. They are unaffected by the `.editorconfig` severity ceiling and are **promoted to errors
by `/p:TreatWarningsAsErrors=true`** — toolchain step 3 and the `_build-nullable.yml` CI gate.

The two affected files:

- `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs` — field `private DebugTextWriter tw;` (line 16),
  assigned `tw = new DebugTextWriter();` (line 29), consumed only by `Console.SetOut(tw);` (line 30).
- `ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs` — same shape at lines 14, 27, 28.

**PREDICTION:** removing only line 30 (resp. 28) leaves the field assigned but never read → **CS0414**
→ error under step 3. Removing lines 29-30 (resp. 27-28) but keeping the field leaves it never used →
**CS0169** → error under step 3. **Both the field declaration and its assignment must be deleted
together with the `Console.SetOut` call.** This is the single most likely way for a careless
implementation of item 1 to break the nullable gate, and the plan must call it out as a hard
constraint on those two files.

No other file among the 33 declares a `DebugTextWriter`-typed field (verified: the
`DebugTextWriter|\btw\b` grep over `ToDoModel.Test` returns only these two files, and the other 31
files all use the inline `Console.SetOut(new DebugTextWriter())` form with no field).

### 3.7 Line counts — no file is near the 500-line limit *because of* this change

Removal only ever **reduces** line count, so no file can cross the 500-line ceiling as a result.
Recorded for completeness (current line counts, all 33 files):

| File | Lines |
|---|---|
| `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs` | 1680 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` | 1456 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianPerformanceMeasurement_Tests.cs` | 1213 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests.cs` | 786 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs` | 708 |
| `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs` | 681 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs` | 562 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianSerializationHelper_Tests.cs` | 559 |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs` | 497 |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | 497 |
| `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | 496 |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs` | 491 |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 792 |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs` | 394 |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` | 394 |
| `ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs` | 350 |
| `QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs` | 345 |
| `UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs` | 318 |
| `UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs` | 306 |
| `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs` | 285 |
| `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` | 276 |
| `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs` | 270 |
| `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs` | 167 |
| `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs` | 150 |
| `UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs` | 126 |
| `UtilitiesCS.Test/NewtonsoftHelpers/ScDictionaryConverter_Tests.cs` | 124 |
| `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs` | 109 |
| `UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs` | 80 |
| `VBFunctions.Test/ComputerInfo_Test.cs` | 80 |
| `UtilitiesCS.Test/Threading/AppGlobalsConverterTests_Unfinished.cs` | 77 |
| `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs` | 50 |
| `UtilitiesCS.Test/Extensions/Frexp_Test.cs` | 41 |
| `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs` | 40 |

**Pre-existing observation, not caused by this change:** thirteen of these files already exceed the
500-line limit in `.claude/rules/general-code-change.md` ("No production code, test code, or reusable
script file may exceed 500 lines"). That is pre-existing debt outside the scope of #826; the plan
should note it as a report-only finding and not attempt to split files.

---

## 4. Q4 — The right logging call for the two production diagnostics

### 4.1 The logger seam

- Declared in the **sibling partial-class file**, not the file being edited:
  `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs:25-27` —
  `private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);`
- **The logging framework is log4net, not NLog.** The brief asks "(NLog?)"; the answer is log4net.
  `OlTableExtensions.cs:15` also carries `using log4net.Repository.Hierarchy;`.
- Both files declare `public static partial class OlTableExtensions` in `namespace UtilitiesCS`
  (`OlTableExtensions.cs:23`, `OlTableExtensions.TableAccess.cs:17`), so `logger` is already in scope
  at lines 79 and 97 with **no new `using` and no new field**. It is already used from
  `TableAccess.cs` at lines 141, 167, 210, 217, 238, 245, 286, 311, 318.
- `OlTableExtensions.TableAccess.cs` is **432 lines** — comfortably under the 500-line ceiling, with
  room for the change. (Caveat: feature 825 lands in this file first; the plan should re-measure.)

### 4.2 `LogTableTiming` is the wrong vehicle

`OlTableExtensions.cs:39-46`:

```csharp
private static void LogTableTiming(string phase, string? details = null)
{
    var detailSegment = string.IsNullOrWhiteSpace(details) ? string.Empty : $" | {details}";
    var phaseLabel = phase.StartsWith("[Table timing]", StringComparison.Ordinal)
        ? phase
        : $"[Table timing] {phase}";
    logger.Debug($"{phaseLabel} | {BuildTableTimingContext()}{detailSegment}");
}
```

Three reasons it is the wrong vehicle:

1. **It is hard-wired to `Debug` level.** A timeout is not a trace event; routing it through
   `LogTableTiming` would bury it below the default log threshold.
2. **It prefixes `[Table timing]`** and appends `BuildTableTimingContext()` (thread id +
   synchronization context, `OlTableExtensions.cs:34-37`). That framing is for the instrumentation
   channel that feature 825 owns; mixing a fault message into it conflates two channels.
3. Its two existing uses in this method (lines 41 and 66) bracket the **success** path
   ("acquisition start", "acquisition complete"). The timeout message is the failure counterpart and
   belongs on the fault channel.

### 4.3 Recommended level: `logger.Warn`

**Use `logger.Warn`, at both sites.** Justification:

- The condition is a **genuine fault** — a 2000 ms deadline on a COM call expired. It is not routine
  progress, so `Debug`/`Info` understate it.
- It is **recoverable and retried** — both sites sit inside the `counter < 2` bounded retry
  (lines 80-88 and 98-109), so the operation may still succeed. That rules out `Error`.
- `Warn` is exactly how this file already reports *the same class of event*: `TableAccess.cs:217`
  logs `$"{nameof(GetTableAsync)} failed after {maxAttempts} attempts. Returning null"` at `Warn`,
  and `TableAccess.cs:238` logs a `COMException` before a retry at `Warn`, with the follow-on
  "Retrying N times ..." at `Info` (line 245). Matching the established local convention is required
  by the General Code Change Policy §7.1 ("Where the repo already has a clear style, match that
  style").

**Suggested message shape** — retain the existing information, add the method name for grep-ability,
matching the `nameof` convention used at lines 141/167/218/239:

```csharp
logger.Warn($"{nameof(GetTableInViewAsync)} timed out on try {counter}");
```

The two sites are in different `catch` clauses (`TaskCanceledException` at :71 and `TimeoutException`
at :95) and currently carry identical text. Consider differentiating them so the log distinguishes
the two failure modes — that is a small, in-scope improvement, but it is a judgement call for the
plan author, and keeping them identical is also defensible as the minimal change.

### 4.4 Existing test coverage of the two catch branches: NONE

Searched `UtilitiesCS.Test/OutlookObjects/Table/` (7 files). Only
`UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` references `GetTableInViewAsync`.
`OlTableExtensionsRetryTests.cs` does **not** — a grep for
`Console\.|TimeoutException|TaskCanceledException|counter|GetTableInView` in that file returns **no
matches**, so despite its name it does not test this retry path at all.

The four tests that do touch the method, and which branch each reaches:

| Test method | Line | Branch reached | Reaches line 79 / 97? |
|---|---|---|---|
| `GetTableInViewAsync_NullTableView_ThrowsInvalidOperationException` | 1238 | throws at the `view is null` guard (:47-53), before the `try` | **No** |
| `GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry` | 1267 | success path; the in-code comment at :1277-1283 states `Task.Run` "completes RanToCompletion, so there is no synthetic retry", and asserts `callCount.Should().Be(1)` (:1320) | **No** |
| `GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException` | 1324 | asserts `ThrowAsync<OperationCanceledException>()` (:1350) — the exception **escapes**, so no `catch` clause matched | **No** |
| `GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot` | 1646 | success path; asserts `callCount.Should().Be(1)` (:1680) | **No** |

**Conclusions:**

- **No existing test asserts on console output** for this method. (The only `Console.` token in the
  1846-line file is the comment at :1640.)
- **No existing test asserts on the retry behaviour** of these two branches.
- **Therefore no existing test can break** from this change. (PREDICTION, based on branch analysis;
  not measured by a test run.)
- **A new regression test is required** to satisfy the bugfix workflow's "failing regression test
  first" rule and the repo's ">= 90% coverage for changed lines" rule, because the changed lines are
  currently uncovered.

### 4.5 Is the change testable? Yes — but not via the `writer` seam

- **The `TextWriter` seam at line 384 is NOT applicable.** `var target = writer ?? Console.Out;` is
  inside `EnumerateTable(this Outlook.Table table, TextWriter? writer = null)` (line 382), a
  completely different method that formats a table for display. `GetTableInViewAsync` (lines 32-119)
  has no `TextWriter` parameter and there is no reason to add one.
- **The `logger` seam makes the change testable** in the sense that matters: after the change, the
  message is observable through log4net rather than through process-global console state. log4net
  supports an in-memory appender attached at runtime, which is the standard way to assert on it
  without touching the filesystem (and therefore without violating the temp-file prohibition in
  `.claude/rules/general-unit-test.md`).
- **The genuinely useful regression test is the branch test, not the message test.** The existing
  `timeoutSourceFactory` injection seam (`GetTableInViewAsync(..., Func<int, CancellationTokenSource>? timeoutSourceFactory = null)`,
  line 37) is already used deterministically by
  `GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry` (:1300) to drive
  timeout behaviour without any wall-clock wait. A new test should use that same seam to force entry
  into `catch (TimeoutException)` and assert the retry occurs (`callCount == 2`) — and, if a log
  appender is wired, that a `Warn` record was emitted. **Coordinate with feature 825**, which owns
  this seam per `epic.md:129-131`.
- **Important constraint on any new test in this class:** `OlTableExtensions_Tests.cs` is **1846
  lines** and its own comment at :1640 says "this file is at its line ceiling". A new test must go in
  a **new file**, which requires a `<Compile Include>` entry in `UtilitiesCS.Test.csproj` (a legacy
  non-SDK project). `epic.md:136-143` explicitly anticipates this and instructs that `.csproj`
  contention be resolved by unioning item lists at merge, never by a `depends_on` edge.

---

## 5. Q5 — Toolchain and coverage implications

### 5.1 Removing the 33 installs does not change any coverage figure. Confirmed.

Basis — three independent configuration facts:

1. **Test assemblies are excluded from instrumentation at run time.**
   `scripts/vscode/Invoke-MSTestWithCoverage.ps1:99` defines
   `$testAssemblyPattern = '.*\.Test\.dll$'` and lines 109-112 append it as a `ModulePath` under
   `/Configuration/CodeCoverage/ModulePaths/Exclude` before invoking `dotnet-coverage`. Lines 82-86
   note the canonical settings file is never written — the exclusion is injected into an in-memory
   copy. All 33 files compile into `*.Test.dll` assemblies, so they are outside the denominator.
2. **`coverage.config`** (24 lines) excludes only third-party module paths (`Deedle`, `FSharp`,
   `Castle.Core`, `FluentAssertions`, `Moq`, `Microsoft.Testing`, `MSTest`). **No production source
   path is excluded**, which is compliant with the Coverage Exclusion Policy.
3. **`TaskMaster.runsettings`** (30 lines) carries the same seven third-party module excludes under
   its Code Coverage data collector (lines 14-24) plus MSTest parallelization (lines 3-8). No
   production exclusions.

Since the change deletes only test-project lines, and test assemblies are not measured, **no coverage
numerator or denominator moves.** No `[ExcludeFromCodeCoverage]` attribute is involved anywhere in
this change.

### 5.2 The two production line changes DO touch coverage — and improve the situation

`OlTableExtensions.TableAccess.cs:79` and `:97` are inside `catch (TaskCanceledException)` and
`catch (TimeoutException)` respectively. Per §4.4, **neither branch is reached by any existing test.**

- Replacing `Console.WriteLine(...)` with `logger.Warn(...)` is a **1-for-1 statement substitution**,
  so the line count of the method is unchanged and the covered/uncovered ratio is unchanged if no
  test is added. (PREDICTION.)
- However, the repository rule "Code changes or refactors must not reduce coverage for the lines that
  were changed" plus "Any new modules, classes, or methods added must target >= 90% coverage" means a
  **changed** line that is uncovered is a finding. Adding the §4.5 regression test moves both changed
  lines (or at least the `TimeoutException` one) from uncovered to covered, which is a **net
  improvement**.
- **Recommendation:** the plan should require at least one new test entering `catch (TimeoutException)`,
  so the two changed production lines are not left as newly-touched-but-uncovered.

### 5.3 No `.csproj` change is needed for item 1. Confirmed.

All 33 files already have `<Compile Include>` entries (they compile today). Editing an existing file
adds no project-file entry. The five affected projects — **`UtilitiesCS.Test`, `QuickFiler.Test`,
`ToDoModel.Test`, `TaskMaster.Test`, `VBFunctions.Test`** — require no `.csproj` edit for item 1.

**But a `.csproj` change IS required if the §4.5 regression test is added in a new file**, because
`OlTableExtensions_Tests.cs` is at its 1846-line ceiling. That would add a `<Compile Include>` entry
to `UtilitiesCS.Test.csproj` (which already has ~470 such entries per `epic.md:138`). Handle per
`epic.md:136-143`: contention, not a dependency; union at merge.

### 5.4 Toolchain-order note

`spec.md:139-140` says "the cleanup must land before the promotion or the nullable gate breaks."
Given §1.7's recommendation not to promote at all, that sequencing constraint is moot for this
feature — but the plan should still state that no ordering hazard exists among the three items as
scoped: item 1 (test files), item 2 (one production file), and item 3 (one text file + one comment)
are disjoint and can land in any order.

---

## 6. Recommended scope for the atomic plan

| Item | Recommendation | Confidence |
|---|---|---|
| 1. Console writer installs | **Do it.** Remove all 34 live install statements across 33 files in 5 projects. Delete the 11 initializer methods that become empty. Delete the `tw` field + assignment in the two `TreeNode` files (§3.6 — mandatory, CS0169/CS0414 risk). | High — behaviour-preservation is proven by `DebugTextWriter` being unreadable (§3.2). |
| 2. Two production diagnostics | **Do it.** Replace both `Console.WriteLine` calls with `logger.Warn($"{nameof(GetTableInViewAsync)} timed out on try {counter}")`. Locate by text, not line number (§0.3). Add one regression test in a NEW file entering `catch (TimeoutException)` via the existing `timeoutSourceFactory` seam. | High — logger is in scope, level matches local convention, no existing test breaks. |
| 3a. RS0030 severity promotion | **Do NOT do it.** Unreachable without either clearing ~143 usages or a prohibited bulk suppression (§1). Record the finding and the precondition; file the cleanup as a follow-up issue. | High — mechanism verified against `_build-nullable.yml` and `.claude/rules/csharp.md:81-83`. |
| 3b. BannedSymbols additions | **Do a reduced set.** Add 9 lines: `CancelAfter` ×2, `WaitHandle.WaitOne` ×5, `CancellationTokenSource.#ctor` ×2. **Exclude `TimeoutAfter`** (§2.4 — it is repo-local and is the prescribed remedy, not a hazard). State explicitly that at `suggestion` severity this buys zero build enforcement. | High on the exclusion; medium on the exact DocID strings, which are unverified by a build (§8). |

### Rejected alternatives (brief)

- **Restoring `Console.Out` scope instead of removal** — rejected in §3.4: process-global state under
  `ClassLevel` parallelism makes save/restore interleave and corrupt, and #811 already removed this
  exact pattern from `NLogTraceWriter_Test`.
- **`WarningsNotAsErrors=RS0030` in `Directory.Build.props`** — rejected in §1.5: it is bulk
  suppression by another name, and `.claude/rules/csharp.md:87` records an explicit precedent of
  declining this mechanism for CS8032.
- **Path-scoped `.editorconfig` severity for new directories** — rejected in §1.5: scopes by
  directory, not by newness; silently degrades; no precedent in the single repo-root `.editorconfig`.
- **A second, stricter `BannedSymbols.txt` for a "clean" project** — rejected in §1.5: no first-party
  project is clean, and it forks a documented single source of truth.
- **Routing the two diagnostics through `LogTableTiming`** — rejected in §4.2: hard-wired to `Debug`,
  and its `[Table timing]` framing belongs to feature 825's instrumentation channel.
- **Adding a `TextWriter` seam to `GetTableInViewAsync`** (mirroring `EnumerateTable`) — rejected in
  §4.5: the `logger` seam already exists and is already used nine times in this file; a second
  output seam would be redundant indirection.

---

## 7. Numeric Derivation Evidence

Required before any numeric claim is proposed for a `spec.md` acceptance criterion.

### 7.1 Claim: 33 in-scope test files carrying 34 live console-writer install statements

- **Complete Family:** every statement in the repository that installs a writer into `Console.Out`
  via `System.Console.SetOut`, in any syntactic form (inline `new`, field-held, `this.`-qualified,
  fully-qualified `System.Console.SetOut`), across all `*.cs` files.
- **Exhaustive Search Scope:** all `*.cs` files in the worktree root
  `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a176c8956f6e15150`, all projects, no
  directory filter. `Console.SetOut` has exactly one overload
  (`public static void SetOut(TextWriter newOut)`), so a single method name covers the whole family;
  the form variation is in the *receiver expression and argument*, which both queries below capture
  because neither anchors on the argument.
- **Inclusion Rules:** statement is uncommented; file is a test-project file (compiles into a
  `*.Test.dll`); the call installs a writer.
- **Exclusion Rules:** production code; commented-out lines (leading `//`); definitions of
  `DebugTextWriter` that perform no install.
- **Primary Search Strategy:** `Grep` pattern `Console\.SetOut\(`, glob `*.cs`, `output_mode=content`,
  `head_limit=0`, with `-B 8 -A 4` context so each hit's comment status and enclosing method are
  directly readable.
- **Primary Member Set (35 files, 38 occurrences):** `VBFunctions.Test/ComputerInfo_Test.cs:15`;
  `TaskMaster/ThisAddIn.cs:103`; `UtilitiesCS.Test/Threading/AppGlobalsConverterTests_Unfinished.cs:19`;
  `UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs:27`;
  `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs:12`;
  `UtilitiesCS.Test/Extensions/Frexp_Test.cs:16`;
  `ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs:20,28`;
  `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs:22,30`;
  `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs:29`;
  `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs:38`;
  `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs:16`;
  `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs:27`;
  `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs:26`;
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs:61,476`;
  `.../BayesianSerializationHelper_Tests.cs:31`; `.../BayesianPerformanceMeasurement_Tests.cs:35`;
  `.../BayesianClassifierTests_UnfinishedStubs.cs:31`; `.../BayesianClassifierTests.cs:22`;
  `.../BayesianClassifierSharedTests.cs:23`; `.../BayesianClassifierGroupTests.cs:24`;
  `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs:34`;
  `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs:21`;
  `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs:25`;
  `.../WrapperScDictionaryTest.cs:25`; `.../WrapperPeopleScoDictionaryNew_Tests.cs:21`;
  `.../ScoDictionaryConverterTests.cs:25`; `.../ScDictionaryConverter_Tests.cs:21`;
  `.../PeopleScoConverter_Tests.cs:23`; `.../FilePathHelperConverterTests.cs:22`;
  `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:38`;
  `.../QfcHomeControllerRunAsyncTests.cs:48`; `.../QfcHomeControllerPropertyTests.cs:46`;
  `.../QfcHomeControllerIterationTests.cs:38`; `.../QfcFormControllerTests.cs:92`;
  `.../QfcFormControllerSeamTests.cs:99`.
- **Primary Count:** 38 occurrences / 35 distinct files.
- **Cross-check Search Strategy (distinct expression):** `Grep` pattern `SetOut` — bare method name,
  **no receiver qualifier and no opening parenthesis**. This is a strictly broader query: it would
  additionally match `System.Console.SetOut`, a `using static System.Console;` unqualified `SetOut(`,
  a line-broken `Console.\n    SetOut(`, and any `SetOut` on another type. Run with
  `output_mode=count`, `head_limit=0`.
- **Cross-check Member Set (35 files):** `VBFunctions.Test/ComputerInfo_Test.cs` (1);
  `UtilitiesCS.Test/Threading/AppGlobalsConverterTests_Unfinished.cs` (1);
  `UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs` (1);
  `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` (1); `.../QfcFormControllerTests.cs` (1);
  `.../QfcHomeControllerIterationTests.cs` (1); `.../QfcHomeControllerPropertyTests.cs` (1);
  `.../QfcHomeControllerTests.cs` (1); `.../QfcHomeControllerRunAsyncTests.cs` (1);
  `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs` (1); `UtilitiesCS.Test/Extensions/Frexp_Test.cs` (1);
  `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs` (1); `TaskMaster/ThisAddIn.cs` (1);
  `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs` (1);
  `.../MailItemHelperTests.cs` (1); `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs` (1);
  `.../Triage/Triage_OlLogicTests.cs` (1);
  `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs` (1);
  `.../Bayesian/ObsoleteBayesianClassifier_Tests.cs` (**2**);
  `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs` (1);
  `.../Bayesian/BayesianSerializationHelper_Tests.cs` (1);
  `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs` (1);
  `.../Bayesian/BayesianPerformanceMeasurement_Tests.cs` (1);
  `.../WrapperPeopleScoDictionaryNew_Tests.cs` (1);
  `.../Bayesian/BayesianClassifierTests_UnfinishedStubs.cs` (1); `.../BayesianClassifierTests.cs` (1);
  `.../BayesianClassifierSharedTests.cs` (1); `.../BayesianClassifierGroupTests.cs` (1);
  `.../ScoDictionaryConverterTests.cs` (1); `.../ScDictionaryConverter_Tests.cs` (1);
  `.../PeopleScoConverter_Tests.cs` (1); `.../FilePathHelperConverterTests.cs` (1);
  `ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs` (**2**);
  `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs` (**2**);
  `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs` (1).
- **Cross-check Count:** 38 occurrences / 35 distinct files.
- **Member-set Comparison:** the two normalized file sets are **identical** — 35 files, same members,
  and the same three files carry 2 occurrences each (`ObsoleteBayesianClassifier_Tests.cs`,
  `TreeNodeTests_UnfinishedStubs.cs`, `TreeNodeTests.cs`). The broader cross-check found **no**
  additional syntactic form, which establishes that the inline `Console.SetOut(` form is exhaustive
  for this repository. Reduction to the in-scope figure: 35 files − 1 production
  (`TaskMaster/ThisAddIn.cs`) − 1 commented-only (`BayesianClassifierTests_UnfinishedStubs.cs:31`)
  = **33 files**. Statements: 38 − 1 production − 3 commented
  (`BayesianClassifierTests_UnfinishedStubs.cs:31`, `TreeNodeTests.cs:22`,
  `TreeNodeTests_UnfinishedStubs.cs:20`) = **34 live install statements**. **Counts agree; assertion
  released.**

### 7.2 Claim: 5 affected test projects

- **Complete Family:** the set of MSBuild projects owning at least one of the 33 files in §7.1.
- **Exhaustive Search Scope:** the 33-file member set established and cross-checked in §7.1.
- **Inclusion / Exclusion Rules:** include the project whose directory is the first path segment of
  each file path; exclude `TaskMaster` (production, out of scope).
- **Primary Search Strategy:** partition the §7.1 primary member set by leading path segment.
- **Primary Member Set:** `UtilitiesCS.Test` (22), `QuickFiler.Test` (6), `ToDoModel.Test` (3),
  `TaskMaster.Test` (1), `VBFunctions.Test` (1).
- **Primary Count:** 5 projects; 22+6+3+1+1 = 33 files.
- **Cross-check Search Strategy (distinct expression):** three independent `Grep ^ output_mode=count`
  line-count sweeps, each scoped to a *different* directory root with an explicit brace-glob file
  list — (a) `UtilitiesCS.Test/` with the 22 expected filenames, (b) worktree root with the 5
  filenames expected in `ToDoModel.Test`/`TaskMaster.Test`/`VBFunctions.Test`, (c)
  `QuickFiler.Test/Controllers/` with the 6 expected filenames. A file assigned to the wrong project
  would fail to resolve in its sweep and the returned file count would fall short.
- **Cross-check Member Set:** sweep (a) returned exactly 22 files, all under `UtilitiesCS.Test/`;
  sweep (b) returned exactly 5 files — `VBFunctions.Test/ComputerInfo_Test.cs`,
  `ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs`,
  `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs`,
  `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs`,
  `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs` (→ ToDoModel.Test 3,
  TaskMaster.Test 1, VBFunctions.Test 1); sweep (c) returned exactly 6 files, all under
  `QuickFiler.Test/Controllers/`.
- **Cross-check Count:** 5 projects; 22+5+6 = 33 files.
- **Member-set Comparison:** both partitions name the identical 5 projects with identical per-project
  file membership and both total 33. **Counts agree; assertion released.** This also establishes that
  the brief's "six affected projects" is incorrect (§0.2).

### 7.3 Claim: 13 `new CancellationTokenSource(<arg>)` call sites, all production, all `Int32`

- **Complete Family:** every invocation of a `System.Threading.CancellationTokenSource` constructor
  overload that accepts a deadline. The complete constructor family is
  `#ctor()`, `#ctor(System.Int32)`, `#ctor(System.TimeSpan)` — three overloads; the parameterless one
  is out of family for the ban but must be counted to prove the partition is exhaustive.
- **Exhaustive Search Scope:** all `*.cs` files in the worktree root.
- **Inclusion Rules:** an object-creation expression naming `CancellationTokenSource` with at least
  one argument.
- **Exclusion Rules:** the parameterless form; type references that are not object creations.
- **Primary Search Strategy:** a **complementary two-query partition** that covers the whole
  constructor family: (i) `new CancellationTokenSource\(\s*\)` (parameterless, `output_mode=count`)
  and (ii) `new CancellationTokenSource\(\s*[^)\s]` (at least one argument, `output_mode=content`).
  The two regexes are mutually exclusive and jointly exhaustive over `new CancellationTokenSource(`.
- **Primary Member Set (arm ii, 13 sites):** `UtilitiesCS/Threading/TimeOutTask.cs:53,119,200,274,358,436,506,588,670,752`;
  `QuickFiler/Controllers/QfcQueue.cs:50,101`;
  `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs:295`. Arm (i) returned 157
  occurrences across 80 files.
- **Primary Count:** 13 parameterized + 157 parameterless = **170**.
- **Cross-check Search Strategy (distinct expression):** an independent single-file exhaustive count
  using a *different, unanchored* pattern — `CancellationTokenSource\(` (no `new`, no argument
  class) — scoped to `UtilitiesCS/Threading/TimeOutTask.cs`, the file holding the largest share
  (10 of 13). This drops the `new ` anchor entirely, so it would also catch a factory-style or
  line-broken construction that the primary regex would miss.
- **Cross-check Member Set:** `UtilitiesCS/Threading/TimeOutTask.cs` — 10 occurrences, matching
  exactly the 10 line numbers enumerated in the primary member set for that file, with no eleventh
  occurrence of any other form. Independently, the brief's externally-derived textual total of
  **170** for `new CancellationTokenSource(` matches the primary partition total exactly.
- **Member-set Comparison:** the primary partition sums to 170, equal to the independently supplied
  textual total; the unanchored per-file cross-check returns 10 for `TimeOutTask.cs`, equal to that
  file's primary membership, and reveals no additional construction form. Argument-type resolution
  was verified separately: `QfcQueue.cs:48` and `:88` declare `int timeout`,
  `ConversationHelper.cs:281` declares `int timeout`, and the `TimeOutTask.cs` arguments are
  `ms`/`milliseconds` — so all 13 bind `#ctor(System.Int32)` and **zero** bind `#ctor(System.TimeSpan)`.
  **Counts agree; assertion released.**

### 7.4 Claim: 13 `WaitOne` call sites, 100% test code

- **Complete Family:** every invocation of any `System.Threading.WaitHandle.WaitOne` overload
  (5 overloads: `()`, `(Int32)`, `(TimeSpan)`, `(Int32,Boolean)`, `(TimeSpan,Boolean)`) on any
  receiver, including receivers of derived types (`AutoResetEvent`, `ManualResetEvent`, `Mutex`,
  `Semaphore`) that inherit rather than redeclare the method.
- **Exhaustive Search Scope:** all `*.cs` files in the worktree root.
- **Inclusion Rules:** any textual `WaitOne` invocation.
- **Exclusion Rules:** none applied (the raw set proved small enough to classify by hand).
- **Primary Search Strategy:** `Grep` pattern `\.WaitOne\(` — member-access-anchored,
  `output_mode=content`, so each receiver expression is visible for type classification. Note the
  pattern deliberately does not name a receiver type, so it catches all five overloads on all derived
  receivers uniformly.
- **Primary Member Set (13):** `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:397`;
  `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs:187`;
  `UtilitiesCS.Test/Threading/UiThread_Tests.cs:335,445`;
  `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs:151`;
  `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:57`;
  `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerInitializationTests.cs:362`;
  `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:243`;
  `.../OutlookFolderTreeServiceInvalidationTests.cs:420`;
  `.../OutlookFolderTreeServiceDisposalTests.cs:425`;
  `.../OutlookFolderTreeServiceConcurrencyTests.cs:150`;
  `.../OutlookFolderHierarchyReaderTests.cs:418`;
  `.../FolderTreeSnapshotBuilderYieldTests.cs:134`.
- **Primary Count:** 13 occurrences across 12 files.
- **Cross-check Search Strategy (distinct expression):** `Grep` pattern `WaitOne` — bare identifier,
  **no `.` anchor and no `(`**. Strictly broader: it would additionally match a line-broken
  `handle.\n  WaitOne()`, a method-group reference `x.WaitOne` passed as a delegate, an override
  declaration, or a `nameof(WaitOne)`. Run with `output_mode=count`, `head_limit=0`.
- **Cross-check Member Set (12 files):** `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` (1);
  `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs` (1);
  `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs` (1);
  `UtilitiesCS.Test/Threading/UiThread_Tests.cs` (**2**);
  `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` (1);
  `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerInitializationTests.cs` (1);
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotBuilderYieldTests.cs` (1);
  `.../WpfDispatcherYieldTests.cs` (1); `.../OutlookFolderTreeServiceInvalidationTests.cs` (1);
  `.../OutlookFolderTreeServiceDisposalTests.cs` (1);
  `.../OutlookFolderTreeServiceConcurrencyTests.cs` (1); `.../OutlookFolderHierarchyReaderTests.cs` (1).
- **Cross-check Count:** 13 occurrences across 12 files.
- **Member-set Comparison:** the two normalized file sets are **identical** (12 files, same members,
  `UiThread_Tests.cs` carrying 2 in both). The broader cross-check surfaced no declaration, override,
  `nameof`, or line-broken form, confirming exhaustiveness. Every path is under `QuickFiler.Test/` or
  `UtilitiesCS.Test/`, so **production usages = 0**. Receiver typing was resolved by a separate
  declaration grep `(ManualResetEvent|AutoResetEvent|SemaphoreSlim|CountdownEvent|ManualResetEventSlim)\s+_ready`,
  which returned `private readonly AutoResetEvent _ready = new AutoResetEvent(false);` for all 11
  `_ready` sites. **Counts agree; assertion released.**

### 7.5 Claim: 2 production `Console.WriteLine` diagnostics in `OlTableExtensions.TableAccess.cs`

- **Complete Family:** every `Console.WriteLine` call in
  `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`.
- **Exhaustive Search Scope:** the whole 432-line file.
- **Primary Search Strategy:** `Grep` pattern `Console\.`, glob `*.cs`, `output_mode=count` over the
  entire repository, then read the file's own hits.
- **Primary Member Set / Count:** the repo-wide count returns **3** `Console.` tokens for this file.
- **Cross-check Search Strategy (distinct expression):** direct full-file `Read` of lines 1-130 and
  370-432, enumerating each `Console.` token by hand rather than by pattern.
- **Cross-check Member Set:** line **79** `Console.WriteLine($"Task timed out on try {counter}");`
  (inside `catch (TaskCanceledException)`, `else` branch of `if (token.IsCancellationRequested)`);
  line **97** identical text (inside `catch (TimeoutException)`); line **384**
  `var target = writer ?? Console.Out;` (inside `EnumerateTable`, **not** a `WriteLine`, and out of
  scope — it is the #811 seam).
- **Cross-check Count:** 3 `Console.` tokens, of which exactly **2** are `Console.WriteLine`.
- **Member-set Comparison:** primary count 3 equals cross-check count 3, and the hand enumeration
  partitions them as 2 `WriteLine` + 1 `Out` seam. **Counts agree; assertion released** for the
  claim of exactly 2 in-scope production diagnostics.

---

## 8. Unverified items and what verification would require

| # | Item | Why unverified | What would verify it |
|---|---|---|---|
| 1 | Exact RS0030 diagnostic count at `warning` severity (the "~143" figure) | No Bash/PowerShell/msbuild in this session | Run toolchain step 2 with `dotnet_diagnostic.RS0030.severity=warning` on a scratch branch and count `RS0030` lines in the build log. Do **not** commit the severity change. |
| 2 | That the 9 proposed DocID strings resolve to real symbols | BannedApiAnalyzers reports **RS0001/RS0002** for a malformed or unresolvable DocID, and silently matches nothing for a merely wrong-but-well-formed one. Only a compile proves resolution. | Add the lines, run toolchain step 2, and confirm the expected 27 `RS0030` info diagnostics appear at the 27 sites enumerated in §2 — and that no RS0001/RS0002 appears. A DocID that produces zero diagnostics is silently wrong. |
| 3 | That removal of the 33 installs breaks no test | No vstest available | `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation` before and after; compare pass counts. Note the known local environment issue with shell-icon tests in `UtilitiesCS.Test` — exclude via `/TestCaseFilter` if it recurs. |
| 4 | CS0169/CS0414 firing on the two `TreeNode` files if the field is left behind | Compiler-behaviour prediction from documented semantics, not measured | Toolchain step 3 after a deliberately partial edit. Recommend simply deleting the field and not testing the failure mode. |
| 5 | Post-825 line numbers in `OlTableExtensions.TableAccess.cs` | Feature 825 has not landed in this worktree | Re-grep for the literal `Console.WriteLine($"Task timed out on try {counter}");` after 825 merges. The plan must not hard-code 79/97. |
| 6 | Whether `spec.md:57`'s "lines 78 and 96" reflects a pre-825 tree or a transcription error | Cannot inspect the spec author's source tree | Immaterial — §0.3 instructs locating by text. Correct the spec to "lines 79 and 97 at time of research; locate by text". |

---

## 9. Test strategy (no test code written)

Consistent with `.claude/rules/general-unit-test.md` and the C# Unit Test Policy (MSTest + Moq +
FluentAssertions).

**Item 1 — console installs.** No new tests. This is a deletion of test-infrastructure lines that
assert nothing. The regression signal is the existing suite continuing to pass at the same count. The
plan should capture a before/after pass count as evidence rather than adding a test that asserts
`Console.Out` is unmodified — such a test would itself depend on process-global state and would be
order-dependent under `ClassLevel` parallelism, violating the Independence principle.

**Item 2 — logger substitution.** One new deterministic MSTest test, in a **new file** under
`UtilitiesCS.Test/OutlookObjects/Table/` (because `OlTableExtensions_Tests.cs` is at 1846 lines), with
a corresponding `<Compile Include>` entry in `UtilitiesCS.Test.csproj`. Shape:

- **Arrange:** mock `Outlook.Explorer` / `Outlook.TableView` per the established pattern at
  `OlTableExtensions_Tests.cs:1269-1298`; inject a `Func<int, CancellationTokenSource>` via the
  existing `timeoutSourceFactory` parameter (`TableAccess.cs:37`) that forces the timeout path
  **without any wall-clock wait** — the banned-API rules prohibit `Thread.Sleep`/`Task.Delay` in
  tests, and `GetTableInViewAsync_SlowSynchronousGetTable_...` already demonstrates the
  no-wait technique.
- **Act:** invoke `GetTableInViewAsync` with `counter: 0`.
- **Assert:** `callCount.Should().Be(2)` — the retry occurred. Optionally attach an in-memory log4net
  appender and assert a `Warn`-level record was emitted; if that proves brittle, assert the retry
  behaviour only and let the logger change ride on the branch coverage.
- **Determinism:** no sleeps, no temp files, no external services. Coordinate the seam usage with
  feature 825, which owns the timeout mechanics.

**Item 3 — BannedSymbols additions.** No unit test is possible or appropriate; analyzer configuration
is verified by the toolchain, not by MSTest. Evidence should be the toolchain step 2 log showing the
expected `RS0030` info diagnostics at the enumerated sites (see §8 item 2) — and, critically, showing
that toolchain step 3 still exits 0.

**Full toolchain, in order, restarting from step 1 on any failure or auto-fix:**
`dotnet tool run csharpier format .` → `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
→ `msbuild ... /p:TreatWarningsAsErrors=true` → `vstest.console.exe ... /EnableCodeCoverage`.
Note that `BannedSymbols.txt` is a `.txt` and is not touched by CSharpier; `.editorconfig` likewise.
