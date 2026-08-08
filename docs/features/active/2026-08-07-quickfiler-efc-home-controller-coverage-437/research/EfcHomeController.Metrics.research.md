# Research — `QuickFiler/Controllers/EfcHomeController.Metrics.cs`

- **Feature:** `2026-08-07-quickfiler-efc-home-controller-coverage-437` (epic child F8, issue #437, parent epic #136)
- **Production file:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aea998f94efaa2eb4\QuickFiler\Controllers\EfcHomeController.Metrics.cs`
- **Size:** 87 lines (limit 500 — compliant, see § 9)
- **`[ExcludeFromCodeCoverage]`:** absent. The file is already inside the coverage denominator.
- **Research date:** 2026-08-07
- **Method:** static reading of production and test sources plus an existing Cobertura artifact already committed in this repository. No build, no test run.

---

## 1. Headline finding

**This file is at 97.59% line coverage and has exactly one uncovered line.** A Cobertura report
committed in this repository reports `line-rate="0.975904"` and `branch-rate="0.916667"` for this
exact file. The sole miss is **line 23**, the delegation inside the public three-argument
`QuickFileMetrics_WRITE` overload:

```csharp
QuickFileMetrics_WRITE(filename, selectedFolder, moved, _stopWatch.Elapsed.Seconds);
```

The whole genuine gap for this file is **one test**. Everything else — the null guard, the empty
guard, the `NotImplementedException` overload, both `TryGetValue` arms, the empty-`dataLines`
short-circuit, and the full line-formatting path — is already covered. A plan that proposes a broad
new metrics test suite is duplicating existing work.

---

## 2. Verified coverage evidence and its provenance

Source artifact (read-only, produced by a sibling in-flight feature, not by F8):

```
docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml
```

- Line 805 of that artifact:
  `<class line-rate="0.975904" branch-rate="0.916667" complexity="13" name="QuickFiler.EfcHomeController" filename="QuickFiler\Controllers\EfcHomeController.Metrics.cs">`
- Denominator reconciliation (verified by hand): per-method `<lines>` entries
  (6 + 2 + 15 + 16 = 39) plus the class-level `<lines>` block (44) = 83 counted entries; exactly two
  carry `hits="0"`, and both are line 23; `81 / 83 = 0.975904`. The arithmetic reconciles exactly.
- Line-number alignment against the current file was verified member by member (see § 3), confirming
  the artifact describes this file as it stands.

**Provenance caveat.** The artifact was captured on feature branch `...-424`, not on the current
worktree HEAD (`74be1964`). Treat it as a strong prior, not as F8's acceptance evidence.

**Authority for acceptance:** F8 must re-derive the per-file number with the coverage harness
delivered by upstream child **F1 (`quickfiler-coverage-ledger`)** and commit it under
`<FEATURE>/evidence/qa-gates/`. F1's harness and its ratified exemption ledger at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` **do not exist on disk yet**;
they were not read. This file is expected to be classified `testable`, not `ratified-exempt`: it has
no COM dependency, no WinForms surface, and its only I/O is already behind an injected writer.

---

## 3. Member-by-member inventory

Existing tests: `QuickFiler.Test\Controllers\EfcHomeControllerMetricsTests.cs` (`MetricsTests`) and
`QuickFiler.Test\Controllers\EfcHomeControllerTests.cs` (`HomeTests`).

| Lines | Member / branch | Status | Evidence |
| --- | --- | --- | --- |
| 12–24 | `public void QuickFileMetrics_WRITE(string, string, List<MailItemHelper>)` — entry, line 17 | COVERED | `HomeTests.QuickFileMetrics_WRITE_WithEmptyList_SkipsBodyAndDoesNotThrow` |
| 18 | guard `moved is null` (condition 0) | COVERED both ways | `HomeTests.QuickFileMetrics_WRITE_WithNullList_SkipsBodyAndDoesNotThrow` and `..._WithEmptyList_...`; Cobertura condition 0 `coverage="100%"` |
| 18 | guard `moved.Count == 0` (condition 1) | **HALF-COVERED** — only the `== 0` (early-return) outcome is exercised | Cobertura line 18 `condition-coverage="75% (3/4)"`, condition 1 `coverage="50%"` |
| 19–20 | early `return;` | COVERED | both `HomeTests` guard tests |
| 23 | delegation `QuickFileMetrics_WRITE(filename, selectedFolder, moved, _stopWatch.Elapsed.Seconds)` | **UNCOVERED** | Cobertura line 23 `hits="0"` (both in the method block and the class block) |
| 24 | closing brace | COVERED | via the early-return path |
| 26–29 | `public void QuickFileMetrics_WRITE(string)` → `throw new NotImplementedException()` | COVERED | `MetricsTests.QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract` |
| 31–43 | `internal void QuickFileMetrics_WRITE(string, string, List<MailItemHelper>, int)` — builds `dataLines` via `_dependencies.MetricsNowFactory()` | COVERED | `MetricsTests.QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter` |
| 44–46 | `if (dataLines.Length == 0) return;` — both arms | COVERED | `MetricsTests.QuickFileMetricsWrite_WithNoMovedItems_DoesNotInvokeWriter` (null and empty) drives the `== 0` arm; the `MyDocuments` test drives the other; Cobertura line 44 `100% (2/2)` |
| 49 | `Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var folderRoot)` — both arms | COVERED | `MetricsTests.QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter` (true) and `..._WithoutMyDocumentsFolder_DoesNotInvokeWriter` (false); Cobertura line 49 `100% (2/2)` |
| 50–53 | `_dependencies.MetricsLineWriter(filename, dataLines, folderRoot)` | COVERED | `MetricsTests.QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter` asserts filename, folder root, and line content |
| 55–65 | `internal static string[] BuildQuickFileMetricLines(...)` — null/empty guard, both conditions | COVERED | `MetricsTests.BuildQuickFileMetricLines_WithNullOrEmptyMovedItems_ReturnsNoLines`; Cobertura line 62 `condition-coverage="100% (4/4)"` |
| 67–69 | date/time prefix formatting from the injected `currentDateTime` | COVERED | `MetricsTests.BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` |
| 71–74 | duration arithmetic (`duration /= moved.Count`, `##0`, `(duration / 60d)` as `##0.00`) | COVERED (single-item case only) | same test (`120 / 1 = 120` → `"120"`, `"2.00"`) |
| 76–85 | projection over `moved` producing the CSV line | COVERED (single-item case only) | same test |

**Uncovered line set (exactly): `{23}`. Half-covered branch set: line 18, condition 1.**
The two are the same gap seen from two angles: nothing has yet called the three-argument public
overload with a **non-empty** `moved` list.

---

## 4. Accumulation, ordering, initialization and boundary semantics

### 4.1 There is no accumulator — the file is stateless per call

`spec.md` seeds "metrics accumulation ordering and state transitions". Verified by reading every
line: **this file holds no counters, no running totals, and no mutable state of its own.** There is
no `Reset()`, no initialization step, and no state machine. Every call is a pure function of its
arguments plus two injected dependencies. The only mutable state read is `_stopWatch` (owned by
`EfcHomeController.cs`) and `Globals` (likewise).

Consequences for planning:
- There is **no reset/initialization semantic to test** beyond `_stopWatch`'s own lifecycle, which
  is owned by `EfcHomeController.cs` (allocated at line 76 in the mail-bearing constructor and at
  line 225 in `InitAsync`; never `Start()`ed anywhere in the family, and never `Reset()`).
- **`_stopWatch` is never started.** Grep of the family shows `new Stopwatch()` at
  `EfcHomeController.cs` lines 76 and 225 and no `Start()` / `StartNew()` call against `_stopWatch`.
  Its `Elapsed` is therefore always `TimeSpan.Zero` in production as well as in tests. This is a
  behavioural observation, not a licence to change it (see § 8, defect 1).

### 4.2 Ordering invariants that do exist

**O1 — output order mirrors input order.** `BuildQuickFileMetricLines` projects with
`moved.Select(...).ToArray()`, which preserves enumeration order. The i-th output line corresponds
to `moved[i]`. Untested explicitly (only single-item lists have been used), but line-covered.

**O2 — the timestamp prefix is identical for every line in a call.** `curDateText` / `curTimeText`
are computed once (lines 67–68) from a single `currentDateTime`, then reused for all items. A
multi-item test would pin this; a per-item `DateTime.Now` read would be a regression that no current
test detects.

**O3 — duration is computed once and divided by the batch size, before formatting.** Lines 71–74
compute `duration = elapsedSeconds / moved.Count` (integer division), then format both the seconds
and the minutes text from that already-truncated integer. Every line in a call carries the same
duration text.

**O4 — write happens only after a successful `dataLines` build and a successful special-folder
lookup.** Order is: build lines (line 38) → bail if empty (line 44) → look up `MyDocuments`
(line 49) → write (line 51). No partial write is possible; the writer is invoked once with the whole
array or not at all. Both bail-out points are covered.

### 4.3 Divide-by-zero and overflow boundaries

- **Divide-by-zero is structurally impossible.** `duration /= moved.Count` at line 72 is guarded
  twice: the three-argument overload returns early at line 18 when `moved` is null or empty, and
  `BuildQuickFileMetricLines` itself returns `Array.Empty<string>()` at line 63 for the same
  condition before reaching line 72. `HomeTests.QuickFileMetrics_WRITE_WithEmptyList_SkipsBodyAndDoesNotThrow`
  exists precisely as a regression guard for a historic inverted guard (`moved.Count == 0` entering
  the body) that produced `DivideByZeroException`; its comment documents this. **Both guards are
  covered — do not re-test this.**
- **Integer truncation, not overflow, is the real boundary.** `elapsedSeconds` is an `int`;
  `duration /= moved.Count` truncates toward zero, so any batch larger than `elapsedSeconds` yields
  `0` and `"0.00"`. `durationMinutesText` then divides the *already truncated* integer by `60d`,
  compounding the loss. No overflow path exists (`int / int` cannot overflow for positive operands;
  a negative `elapsedSeconds` is not producible from `Stopwatch.Elapsed`).
- **`.Seconds` versus `.TotalSeconds` (line 23).** `_stopWatch.Elapsed.Seconds` is the 0–59
  component, not the total. A 90-second elapsed interval would be reported as 30. Combined with 4.1
  (the stopwatch is never started) this is currently latent. See § 8, defect 1.

### 4.4 Interaction with `EfcHomeController.Timing.cs` and the time source

Verified by reading `EfcHomeController.Timing.cs` (43 lines) in full: it contains only four private
static logging helpers — `DescribeSynchronizationContext`, `DescribeStartupOverlapState`,
`BuildFirstSelectionTimingContext`, `LogFirstSelectionTiming`. **It exposes no clock, no time
source, and no member consumed by `EfcHomeController.Metrics.cs`.** There is no coupling between the
two files. The `Stopwatch` instances used by `Timing.cs`'s callers are local variables created in
`EfcHomeController.HandleSelectionChangedAsync` (`Stopwatch.StartNew()`, line 176) — distinct from
the field `_stopWatch` that Metrics reads.

The metrics time source is therefore, exhaustively:

| Time source | Injection status | Where |
| --- | --- | --- |
| Wall-clock date/time for the line prefix | **Already injected** — `_dependencies.MetricsNowFactory()` (`Func<DateTime>`, default `() => DateTime.Now`) | consumed at `Metrics.cs` line 39; declared at `EfcHomeControllerDependencies.cs` lines 62, 77, 125 |
| Elapsed duration | **Not injected** — read directly from the `_stopWatch` field at `Metrics.cs` line 23 | field declared at `EfcHomeController.cs` line 383 |

`MetricsTests.CreateController` already supplies `metricsNowFactory: () => new DateTime(2026, 7, 4, 13, 5, 0)`,
so the date/time half of the clock requirement is satisfied today and needs no new work.

**No `Thread.Sleep`, `Task.Delay`, or wall-clock wait is required to cover line 23** — see § 5.

---

## 5. The one required test

### T1 — the three-argument overload forwards the stopwatch elapsed seconds to the internal overload
*Closes: line 23 (the only uncovered line) and the line-18 `moved.Count != 0` branch outcome.*

- **Arrange.** Reuse `MetricsTests.CreateController(specialFolders, writer)` verbatim: it builds a
  `FakeApplicationGlobals` carrying `{"MyDocuments": "C:/Users/Test/Documents"}`, an
  `EfcHomeControllerDependencies` with a fixed `metricsNowFactory` and a recording
  `metricsLineWriter`, and constructs a real `EfcHomeController` through its internal constructor.
  Then supply a non-null `_stopWatch`, because that helper passes `mail: null`, so
  `EfcHomeController.cs` line 76 never runs and `_stopWatch` stays `null` (a non-empty `moved` list
  would otherwise throw `NullReferenceException` at line 23).

  **Preferred arrangement — reflection field set (simplest, matches existing practice):**
  set the private field `_stopWatch` to `new Stopwatch()` using the same
  `BindingFlags.NonPublic | BindingFlags.Instance` helper already present in
  `EfcHomeControllerExecuteMovesTests.SetPrivateField` and `EfcHomeControllerTests.SetField`. Lift
  that helper into a shared internal test-support class rather than copying it a third time.

  **Alternative arrangement — construct through the mail-bearing path:** pass a
  `Mock<MailItem>(MockBehavior.Loose).Object` and a `dataModelFactory` returning a data model whose
  `Mail` is non-null, so the constructor allocates `_stopWatch = new Stopwatch()` at line 76. This
  requires also injecting `viewerFactory`, `keyboardHandlerFactory`, `explorerControllerFactory` and
  `formControllerWithDataFactory` (otherwise the production `ViewerFactory` constructs a live
  `EfcViewer` form — prohibited). The full probe already exists as
  `EfcHomeControllerLifecycleTests.LifecycleProbe.CreateControllerWithMail`. Use this only if the
  reflection route is rejected in review; it is roughly forty lines of arrangement for the same
  assertion.

- **Act.** `controller.QuickFileMetrics_WRITE("metrics.csv", "Archive", moved);` with
  `moved` a single-element `List<MailItemHelper>` populated exactly as the existing tests do
  (`Subject`, `ToRecipientsName`, `SenderName`, `SentDate` set on a plain `new MailItemHelper { ... }`).

- **Assert.** The recording writer received exactly one call; `Filename == "metrics.csv"`;
  `FolderRoot == "C:/Users/Test/Documents"`; and the single emitted line contains the duration
  fields produced by an elapsed value of zero (`",0,0.00,"`), which proves the
  `_stopWatch.Elapsed.Seconds` argument actually flowed through line 23 rather than the test having
  silently taken the early-return path.

- **Determinism.** `new Stopwatch()` that is never started returns `TimeSpan.Zero` from `Elapsed`
  unconditionally, so `Elapsed.Seconds == 0` on every run and on every machine. **No timer, no
  sleep, no delay, no wall-clock read.** The date/time half of the output is pinned by the already-
  injected `metricsNowFactory`. The test is fully deterministic.

### T2 (optional hardening — adds no covered lines)
*Pins ordering invariants O1–O3, which are line-covered but only ever exercised with a single item.*

- **Arrange.** Three `MailItemHelper` instances with distinct subjects, in a known order; call
  `EfcHomeController.BuildQuickFileMetricLines(fixedNow, elapsedSeconds: 120, "Archive/Target", moved)`
  directly — it is `internal static`, so no controller instance is needed.
- **Assert.** Three lines returned in input order (O1); all three carry the identical
  `"07/04/2026,01:05,"` prefix (O2); all three carry `",40,0.67,"` from `120 / 3 = 40` and
  `(40 / 60d).ToString("##0.00") == "0.67"` (O3).

  > **Correction (preflight 2026-08-07).** An earlier revision of this artifact asserted
  > `",40,0.66,"`. That was arithmetically wrong: the production format string at
  > `EfcHomeController.Metrics.cs` L74 is `(duration / 60d).ToString("##0.00")`, and the custom
  > numeric format rounds `0.666…` away from zero, so .NET emits `"0.67"`. The inputs
  > `120 / 3 = 40` are also exact, so this case does **not** characterize the integer-truncation
  > defect of § 4.3; a separate case with a non-divisible `elapsedSeconds` would be needed for
  > that.
- Classify as optional: it does not move the per-file number and is not required for the 80% gate.
  Include it only if the plan has budget for regression hardening.

**Nothing else is needed for this file.**

---

## 6. Seam inventory

### Already injectable (no work required)

| Seam | Type | Declared | Default |
| --- | --- | --- | --- |
| `MetricsNowFactory` | `Func<DateTime>` | `EfcHomeControllerDependencies.cs` lines 62, 77, 125 | `() => DateTime.Now` |
| `MetricsLineWriter` | `Action<string, string[], string>` | `EfcHomeControllerDependencies.cs` lines 63, 78, 127 | `FileIO2.WriteTextFile` |
| `Globals.FS.SpecialFolders` | `IFileSystemFolderPaths` (interface) | `UtilitiesCS` | fakeable; `MetricsTests.FakeFileSystemFolderPaths` already does so |
| `EfcHomeControllerDependencies` itself | constructor injection | `EfcHomeController.cs` lines 54–95 | `CreateDefaultDependencies()` |

The `MetricsLineWriter` seam is what keeps this file free of filesystem I/O in tests, satisfying the
"no temporary files" rule without any exception.

### New additive seam required: **none**

Line 23 is reachable with existing seams plus a `Stopwatch` supplied to the existing private field.

**If — and only if — review rejects both arrangements in T1**, the minimal additive option is an
instance property on `EfcHomeController` (not on the shared dependency contract):

```csharp
internal Func<int> MetricsElapsedSecondsFactory { get; set; }   // null by default
// line 23 becomes:
QuickFileMetrics_WRITE(filename, selectedFolder, moved,
    MetricsElapsedSecondsFactory?.Invoke() ?? _stopWatch.Elapsed.Seconds);
```

This is behaviour-preserving when unset and matches the existing `MoveToFolderAsyncAction` pattern
in `EfcHomeController.ExecuteMoves.cs`. It is **not recommended** — it adds production surface to
avoid three lines of test arrangement, and § 8 defect 1 argues the elapsed-seconds expression should
be revisited on its own issue rather than frozen behind a new seam.

---

## 7. Cross-child contract note (F9)

`EfcHomeControllerDependencies.cs` and `EfcHomeControllerDependencyFactories.cs` are the injection
contract for the whole EFC controller family, including `EfcFormController` and `EfcItemController`,
which belong to **sibling child F9 (`quickfiler-efc-form-item-controller-coverage`)**. F8 must not
edit F9's files.

**Determination for this file: no dependency-contract change is required.** `MetricsNowFactory` and
`MetricsLineWriter` already exist as optional constructor parameters with `null` defaults that fall
back to production behaviour — the exact additive pattern any future addition must follow. F8 adds
nothing to that contract on behalf of this file, so F9 needs no edit and there is no cross-child
contract note to escalate.

Also out of scope for F8 by explicit constraint: `coverage.config` and any shared build property
file must not be modified.

---

## 8. Latent defects observed — record, do not fix in F8

The epic NFR forbids behaviour change. Promote each to its own GitHub issue via the MCP promotion
lifecycle rather than leaving it as prose that disappears at merge.

1. **`_stopWatch.Elapsed.Seconds` is the 0–59 component, not the total elapsed** (line 23), and
   `_stopWatch` is never `Start()`ed anywhere in the `EfcHomeController` family, so the emitted
   duration is always `0` in production. The metric is effectively inert. Fixing it means both
   starting the stopwatch and switching to `(int)Elapsed.TotalSeconds` — a real behaviour change
   needing its own issue.
2. **Compounded integer truncation** in the duration arithmetic (lines 71–74): `elapsedSeconds` is
   integer-divided by `moved.Count`, and the minutes figure is then derived from the already
   truncated value rather than from the original seconds.
3. **Missing field separator in the emitted CSV** (lines 80–81): the interpolated segments are
   `...,{itemInfo.ToRecipientsName}` immediately followed by `{itemInfo.SenderName},Email,...` with
   no comma between them, so recipient and sender are concatenated into one column. The existing
   assertion in `MetricsTests.BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`
   expects `"RecipientSender"`, pinning the defect as current behaviour.
4. **Inconsistent `xComma` sanitization**: `QfcCollectionController.xComma(...)` is applied to
   `Subject` only (line 79), while `ToRecipientsName`, `SenderName` and `selectedFolder` are
   interpolated raw — an embedded comma in any of those corrupts the CSV row. `QfcCollectionController`
   itself sanitizes all four (see `QfcCollectionController.cs` lines 2311–2316).
5. **`public void QuickFileMetrics_WRITE(string filename)` throws `NotImplementedException`**
   (lines 26–29) on a public surface. It exists to satisfy an interface obligation; the existing test
   pins the contract deliberately. Removing it is an API change outside F8's scope.

---

## 9. File-size compliance

- Current: **87 lines** against the 500-line ceiling in `.claude/rules/general-code-change.md`.
  Headroom: 413 lines. **No partial split is needed, and none should be proposed.**
- The recommended work adds **zero production lines** (no new seam), so the file remains 87 lines.
- Test-side: `EfcHomeControllerMetricsTests.cs` is currently 244 lines. Adding T1 (and optionally T2)
  keeps it comfortably under 500. Extracting the duplicated `SetPrivateField` / `SetField` reflection
  helper and the `FakeApplicationGlobals` / `FakeFileSystemFolderPaths` fakes — which are currently
  triplicated across `EfcHomeControllerMetricsTests`, `EfcHomeControllerLifecycleTests` and
  `EfcHomeControllerTests` — into a shared internal test-support class is recommended and would
  reduce total test lines.

---

## 10. Do not duplicate — scenarios already covered

Do **not** author tests for any of the following:

| Already covered scenario | Existing test |
| --- | --- |
| `BuildQuickFileMetricLines` returns no lines for a `null` moved list | `EfcHomeControllerMetricsTests.BuildQuickFileMetricLines_WithNullOrEmptyMovedItems_ReturnsNoLines` |
| `BuildQuickFileMetricLines` returns no lines for an empty moved list | same test |
| `BuildQuickFileMetricLines` formats the full CSV line (date, time, subject, `SingleSorted`, duration, duration-minutes, recipients, sender, folder, sent date, sent time) | `EfcHomeControllerMetricsTests.BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` |
| Four-argument `QuickFileMetrics_WRITE` invokes the injected writer with the `MyDocuments` root | `EfcHomeControllerMetricsTests.QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter` |
| Four-argument `QuickFileMetrics_WRITE` writes nothing when `MyDocuments` is absent | `EfcHomeControllerMetricsTests.QuickFileMetricsWrite_WithoutMyDocumentsFolder_DoesNotInvokeWriter` |
| Four-argument `QuickFileMetrics_WRITE` writes nothing for a null or empty moved list | `EfcHomeControllerMetricsTests.QuickFileMetricsWrite_WithNoMovedItems_DoesNotInvokeWriter` |
| `QuickFileMetrics_WRITE(string)` preserves its `NotImplementedException` contract | `EfcHomeControllerMetricsTests.QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract` |
| Three-argument `QuickFileMetrics_WRITE` skips the body (no divide-by-zero, no null `_stopWatch` deref) for an empty list | `EfcHomeControllerTests.QuickFileMetrics_WRITE_WithEmptyList_SkipsBodyAndDoesNotThrow` |
| Three-argument `QuickFileMetrics_WRITE` skips the body for a null list | `EfcHomeControllerTests.QuickFileMetrics_WRITE_WithNullList_SkipsBodyAndDoesNotThrow` |
| A fixed, injected `DateTime` drives the line prefix (no wall-clock read in tests) | `EfcHomeControllerMetricsTests.CreateController` supplies `metricsNowFactory` |

---

## 11. Recommended approach and rejected alternatives

**Recommended:** add T1 only (with T2 as optional hardening), change no production code, extract the
duplicated test helpers into shared test-support, re-measure with F1's harness, and commit the
per-file number under `<FEATURE>/evidence/qa-gates/`. Expected result: 100% line coverage for this
file at zero production risk.

**Rejected alternative A — add an injectable `Func<int>` elapsed-seconds seam to reach line 23.**
Rejected: it adds production surface purely to simplify test arrangement, and the elapsed-seconds
expression is itself a defect (§ 8, defect 1) that should be corrected on its own issue rather than
frozen behind a new seam. Retained in § 6 as the documented fallback if review rejects both T1
arrangements.

**Rejected alternative B — move `BuildQuickFileMetricLines` into a new host-neutral formatter class
to raise testability.** Rejected: the method is already `internal static` and pure, it is already
fully line-covered, and extracting it would be a structural change with no coverage benefit,
increasing merge-conflict risk against sibling children for nothing.

**Rejected alternative C — declare the file `ratified-exempt` in F1's ledger.** Rejected on the
evidence: 97.59% line coverage, no COM types, no WinForms types, no designer code. It fails the
irreducible-remainder test decisively.
