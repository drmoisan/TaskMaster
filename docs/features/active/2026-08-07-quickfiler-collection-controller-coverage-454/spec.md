# quickfiler-collection-controller-coverage — Spec

- **Issue:** #454
- **Parent:** epic #136 `quickfiler-per-file-coverage`, child F11, wave 1
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`
- **Work Mode:** `full-feature` (this file and `user-story.md` are both authoritative AC sources)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T23-10
- **Status:** Draft
- **Version:** 1.0

Throughout this document `<FEATURE>` denotes
`docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/`. All other paths are
repo-relative. This work executes in a different worktree than the one in which this spec was
authored; absolute paths are deliberately absent.

Primary evidence for every material claim below is one of the three research artifacts in
`<FEATURE>/research/`:

- `research/qfc-collection-controller.md` — split design, seam inventory, defect inventory, per-partial
  test strategy.
- `research/iqfc-collection-controller.md` — interface classification and the full consumer inventory.
- `research/coverage-harness-contract.md` — the measurement contract.

---

## 1. Overview

`QuickFiler/Controllers/QfcCollectionController.cs` is the single largest production file in the
repository at 2,349 lines. It carries a real `[ExcludeFromCodeCoverage]` attribute at
`QuickFiler/Controllers/QfcCollectionController.cs:21`, immediately above
`public class QfcCollectionController : IQfcCollectionController` at `:22`. The attribute removes the
type from instrumentation entirely, so the file is **unmeasured, not covered**: it emits no Cobertura
`<class>` element at all. Its contract file,
`QuickFiler/Interfaces/IQfcCollectionController.cs` (118 lines), completes the pair.

Three repository positions bear on this file today:

1. `.claude/rules/general-code-change.md` § File Size Limit sets a 500-line ceiling for production
   files. At 2,349 lines the file breaches it by a factor of nearly five.
2. `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy states that no production file may
   be excluded from coverage measurement, and that the correct response to untestable lines is to
   refactor — extracting logic into host-neutral testable modules and leaving only the thinnest
   possible wiring in the host-bound entry point.
3. Issue #136 AC1 requires every compiled QuickFiler file to reach >= 80% line coverage or sit on a
   ratified exemption ledger with a file-specific rationale.

The exemption on this file is **unratified**. It was applied by the blanket 28-class sweep in commit
`a564add0` (issue #197), not by a file-specific ratification. Per the epic's policy reconciliation
(`docs/features/epics/quickfiler-per-file-coverage/epic.md:205-225`), the qualifier "without an
injectable seam" in the `CLAUDE.md` § UT2 COM/VSTO/WinForms exemption is a live obligation, not a
standing permission: if a seam can be introduced, the exemption does not apply.
`[ExcludeFromCodeCoverage]` on a testable seam is a Blocking finding. The attribute stays unratified
until F1's ledger says otherwise.

The starting position is not zero. Two compiled test classes already exercise the type —
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` and
`QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`, both listed at
`QuickFiler.Test/QuickFiler.Test.csproj:112-113`. They reach 24 of roughly 95 members, several along a
single branch only. The estimated line coverage once the exemption is removed is **12%-20%, most
likely near 15%** (`research/qfc-collection-controller.md` §C5). That estimate is a planning input to
be replaced by a measurement, not an acceptance figure.

---

## 2. Scope

### 2.1 In scope — production files owned by this child

| File | Lines today | Disposition |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2,349 | Split into a root file plus 13 partials; `[ExcludeFromCodeCoverage]` removed; seams extracted; covered |
| `QuickFiler/Interfaces/IQfcCollectionController.cs` | 118 | **Zero edits.** Classification only — `interface-only / not-measured` |

### 2.2 In scope — production files this child creates

13 new partial files plus 4 non-partial seam files. See §5 and §6.

### 2.3 In scope — shared files this child appends to

- `QuickFiler/QuickFiler.csproj` — one contiguous additive block of `<Compile Include>` entries.
- `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` — one contiguous additive block
  of ledger rows, delivered in the same change as the csproj entries.

### 2.4 In scope — test files

All new tests live in **new** files under `QuickFiler.Test/Controllers/`, mirroring the partial names.
See §9.

### 2.5 Out of scope

- Every file assigned to another epic child. Named sibling-owned files this child must not edit:
  `QuickFiler/Controllers/QfcItemGroup.cs` and `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`
  (F2); `QuickFiler/Controllers/QfcFormController.Actions.cs`,
  `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
  `QuickFiler/Controllers/IQfcFormController.cs` (F6);
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs` (F7);
  `QuickFiler/Controllers/EfcHomeController.Metrics.cs` (F8).
- `UtilitiesCS/Properties/AssemblyInfo.cs` — widening the internals grant is not this epic's mandate
  (`epic.md:619-631`).
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — repository-wide files outside this
  assignment; issue #441 owns the fix. `scripts/temp-extract-coverage.ps1` must not be reused or
  extended.
- `coverage.config` and `TaskMaster.runsettings` — no edit is needed or permitted (§7.5).

---

## 3. Non-Goals

- **No behavior change to observable QuickFiler flows.** This is a testability refactor plus test
  authorship.
- **No defect fixes.** Fourteen latent defects were found during research and promoted to issues. They
  are characterized by test, not corrected. See §10.
- **No contract additions.** `Task CleanupAsync()` and an `IItemViewer`-typed return for
  `LoadItemViewer_03` are the two members a seam design might be tempted to promote onto the
  interface. Both are cross-child contract changes (F6 and F14 respectively) and are deferred
  (`research/iqfc-collection-controller.md` §C.3).
- **No deletion of unreachable code.** Twelve members have no caller anywhere in the repository
  (~227 lines). Deleting `public` members is a public-API change; they are isolated into one partial
  and covered by direct-call tests this cycle. Removal is issue #468.
- **No change to repository-wide coverage thresholds.**
- **No mechanical 500-line chop.** The split follows logical responsibility seams. A line-count-driven
  split is explicitly rejected.

---

## 4. Upstream Dependency — the F1 Contract

F1 is `quickfiler-coverage-ledger`, issue #432, wave 0. Its ledger will be at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.

**That ledger does not exist on this branch today, and its absence is expected and correct.** F1 is
being prepared concurrently and executes before this child. The F1 contract is therefore specified
here as an **execution-time read behind a Phase 0 halt gate**, not as a preparation-time blocker and
not as an open risk.

### 4.1 The minimal assumption set

Encode exactly these four and nothing else (`research/coverage-harness-contract.md` §E.3). Anything
beyond this couples the plan to a format F1 has not yet fixed.

| # | Assumption |
| --- | --- |
| A1 | A ledger exists at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` and is Markdown in which each production file's repo-relative path appears as a literal substring on its classifying row |
| A2 | Each row states one of exactly three bucket tokens: `testable`, `ratified-exempt`, `interface-only` (match `interface-only` as a prefix so the `/ not-measured` suffix is optional) |
| A3 | The ledger is append-only and additive; fan-in conflicts are resolved by keeping both sides (`epic.md:579-582`) |
| A4 | New files created by this child default to `testable` at >= 90% line unless a rationale meeting one of the three exemption grounds is recorded (`epic.md:583-585`) |

**Explicitly not assumed** — the ledger's column names, count, ordering, or path separator; and the
name, path, parameters, or output format of F1's harness, or that F1's harness implements the
recompute recipe in §7.

### 4.2 Phase 0 gate — two halts, two record-and-proceed

| Gate | Check | Outcome |
| --- | --- | --- |
| **G0.1** | `coverage-ledger.md` exists | **HALT** if absent: `F1_LEDGER_MISSING: coverage-ledger.md not present; F1 (#432) has not landed on the integration branch.` Do not author a substitute ledger. Do not remove `[ExcludeFromCodeCoverage]`. |
| **G0.2** | The row containing the literal `QfcCollectionController.cs` (the `Controllers/QfcCollectionController.cs` row, not a `.<Concern>.cs` partial row) is bucketed `testable` | **HALT** if the row is absent (`F1_LEDGER_ROW_MISSING`), or bucketed `ratified-exempt` or `interface-only` (`F1_LEDGER_CONFLICT`). Exemption removal is this child's entire premise; a conflicting classification must be reconciled with the epic owner first. |
| **G0.3** | The row containing `IQfcCollectionController.cs` is bucketed `interface-only` | **RECORD, DO NOT HALT.** If absent or otherwise classified, proceed and record `F1_LEDGER_RECONCILE: IQfcCollectionController.cs classified <X>; expected interface-only / not-measured.` Append or correct the row per `epic.md:579-582`, citing `research/iqfc-collection-controller.md` §A.3-A.5. |
| **G0.4** | An F1 per-file harness exists that emits line **and** branch rates from a Cobertura path | **RECORD, DO NOT HALT. FALLBACK MANDATORY.** If found and it computes from class-level `<lines>` children, use it and record a one-line confirmation. If found but it reads the `line-rate` attribute, use §7's recipe as authoritative, record F1's figure alongside, and record `F1_HARNESS_DISAGREES`. If not found, proceed with §7's recipe and record `F1_HARNESS_ABSENT_FALLBACK_APPLIED`. |

The split is deliberate: **classification is a contract this child cannot manufacture** (halt);
**measurement is fully reproducible from the Cobertura XML with the recipe in §7** (never halt).

### 4.3 Ledger rows this child appends

Seventeen rows — 13 partials and 4 seam files — appended as one contiguous block in the same change
that adds the `<Compile Include>` entries, so both shared files present a single hunk at fan-in.

The row for `QuickFiler/Interfaces/IQfcCollectionController.cs` is **F1's** responsibility, not an F11
append: that file is a pre-existing member of the compile set at `QuickFiler/QuickFiler.csproj:360`.
This child verifies or reconciles it (G0.3) and authors it only if F1's ledger lacks it.

---

## 5. Sequencing (mandatory)

Each stage depends on the previous. The order is not stylistic.

**Stage 0 — gate and baseline.** Run the §4.2 Phase 0 gate. Capture the merge-base baseline with
`[ExcludeFromCodeCoverage]` still present at `QuickFiler/Controllers/QfcCollectionController.cs:21`. In
that report the file is **absent entirely**, not present at 0%. That absence is the baseline.

**Stage 1 — split.** Split `QfcCollectionController.cs` into the root file plus 13 partials along the
responsibility boundaries in §6. Remove `[ExcludeFromCodeCoverage]` (`:21`) and
`using System.Diagnostics.CodeAnalysis;` (`:4`). Add the 17 `<Compile Include>` entries and the 17
ledger rows. No seam work, no test authorship, no behavior change. Toolchain green. Then take the
**second measurement**, immediately after attribute removal and before any new test: that is the true
starting figure against which this child's gain is stated.

A transient drop in the QuickFiler package line rate is expected at this moment, because all 17 files
enter the denominator at once before any new test lands. State the expected drop in the evidence so a
reviewer does not read it as a regression.

**Stage 2 — seam extraction.** Introduce seams S1-S10 (§8) and the four seam files. Every seam's
production default body is bit-identical in effect to the code it replaces. All seams are `private`
fields or `internal` interfaces; the only public-surface change is optional trailing constructor
parameters.

**Stage 3 — coverage.** Author per-partial tests (§9), then measure per file with the §7 recipe and
write the evidence.

A mechanical 500-line chop is explicitly rejected. The `#region` markers in the current file were used
as a hypothesis and overridden in six places where a region mixed unrelated responsibilities
(`research/qfc-collection-controller.md` §A2).

---

## 6. Partial-Split Design

### 6.1 Files

Projections use a 20-line per-file overhead constant. "Thin" marks a member reduced to delegation over
a §8 seam. Source: `research/qfc-collection-controller.md` §A3.

| # | File (all under `QuickFiler/Controllers/`) | Projected | Responsibility |
| --- | --- | --- | --- |
| 1 | `QfcCollectionController.cs` (retained root) | ~202 | Class declaration, `log4net` logger, constructor, all instance fields, seam fields and production defaults, `Cleanup`/`CleanupAsync` |
| 2 | `QfcCollectionController.State.cs` | ~180 | Observable controller state; the layout-suspension gate (`TlpLayout`, `SafeSetTlpLayout`, `Digits`, `ReadyForMove`) |
| 3 | `QfcCollectionController.LoadSync.cs` | ~113 | Synchronous load pipeline and the cached-page swap entry point |
| 4 | `QfcCollectionController.LoadAsync.cs` | ~293 | The two production async load paths (standard and the issue-#171 carrier list) plus the secondary load |
| 5 | `QfcCollectionController.GroupFactory.cs` | ~134 | Create one `QfcItemGroup` (viewer + controller) and place it in the layout |
| 6 | `QfcCollectionController.Removal.cs` | ~348 | Group removal, outgoing-page caching, and the page swap |
| 7 | `QfcCollectionController.KeyboardWiring.cs` | ~129 | Register/unregister the "Collection" keyboard action set |
| 8 | `QfcCollectionController.Selection.cs` | ~184 | Which item is active; index/selection arithmetic |
| 9 | `QfcCollectionController.NavigationToggle.cs` | ~140 | Bulk navigation toggling and row-height expansion style |
| 10 | `QfcCollectionController.Conversation.cs` | ~235 | Collapsing and expanding a conversation into/out of item groups |
| 11 | `QfcCollectionController.Layout.cs` | ~122 | Row-space arithmetic, renumbering, selection-index fix-up on removal |
| 12 | `QfcCollectionController.Theme.cs` | ~82 | Dark/light propagation and the `IOlObjects.PropertyChanged` subscription lifecycle |
| 13 | `QfcCollectionController.Move.cs` | ~163 | Batched move execution and CSV move diagnostics |
| 14 | `QfcCollectionController.LegacyLoadPaths.cs` | ~249 | The twelve superseded members with no production caller (issue #468) |

The largest projected partial is #6 `Removal.cs` at ~348 lines. **A further split is pre-authorized**
so the planner does not have to re-derive it: if seam work pushes that file past ~430 lines, move
`RemoveSpecificControlGroup(int)`, `RemoveSpecificControlGroupAsync`, and the static counter (current
lines 1099-1248) into `QuickFiler/Controllers/QfcCollectionController.RemoveGroup.cs`, taking the
csproj/ledger additions from 17 to 18.

The `LegacyLoadPaths.cs` file header must state that no member has a production caller and cite issue
#468.

### 6.2 `using` hygiene

Three directives are removed from the root file and must not be propagated into any new partial:

- `using System.Diagnostics.CodeAnalysis;` (`:4`) — no longer needed once the attribute is gone.
- `using System.Net.NetworkInformation;` (`:6`) — unused; no `Ping`/`NetworkInterface`/`IPGlobal*`/
  `PhysicalAddress`/`NetworkChange` token appears anywhere in the file.
- `using System.Windows;` (`:10`) — load-bearing only in that it makes `System.Drawing.Size`/`Point`
  ambiguous, which is why every such construction in the file is already fully qualified. Keep those
  call sites fully qualified as-is to avoid churn. `PresentationFramework` is not referenced, so
  `MessageBox` at `:186` resolves unambiguously to `System.Windows.Forms.MessageBox`.

### 6.3 `QuickFiler.csproj` impact

`QuickFiler/QuickFiler.csproj:311` already carries
`<Compile Include="Controllers\QfcCollectionController.cs" />`. Seventeen new entries are required (13
partials + 4 seam files), inserted **contiguously immediately after line 311**, before
`<Compile Include="Controllers\EmailSorter.cs" />` at `:312`. That anchor is an F11-owned line and
minimizes overlap with the concurrent hunks F2 (near `:340-341`), F3, F7 (near `:325-327`), and F9
(near `:297-301`) will each produce.

Binding rules from `epic.md:604-612`:

- Only this child's `<Compile Include>` entries. No property changes, no reference changes, no
  reordering of unrelated entries.
- **Preserve CRLF.** The file is entirely CRLF-terminated (593 `\r$` matches over 593 lines). Use the
  Edit tool or `perl -0777` with explicit `\r\n`. A git-bash `sed -i` will strip CRLF and produce a
  whole-file diff that is guaranteed to conflict.
- An additive fan-in conflict with siblings is anticipated and is resolved by keeping both sides. It is
  not a decomposition defect.

**No csproj edit is required for the interface file.** Its entry at `QuickFiler/QuickFiler.csproj:360`
stays exactly as-is.

---

## 7. Measurement Contract (requirement, not implementation detail)

This section is the highest-risk area of the child, and it is resolved. Every clause below is a
requirement.

### 7.1 Per-file attribution survives the partial split

Cobertura emits **one `<class>` element per `(type, source file)` pair**. Verified empirically against
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`:
the ten `QfcItemController` partials produce ten `<class>` elements carrying the same
`name="QuickFiler.Controllers.QfcItemController"` and ten distinct `filename` values. No partial file
collapses into another. Attribution is in fact finer than per-method: a single constructor whose lines
span two partials appears under both files.

### 7.2 The `line-rate` attribute must never be read

**The `<class>` `line-rate` and `branch-rate` attributes are not trustworthy.** Two separate defects
produce the inflation:

- **Issue #441** — `Get-CoberturaCoverageSummary`
  (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:98`) selects `.//lines/line` at `:122`, which
  matches both the `<methods><method><lines>` subtree and the class-level `<lines>` block, so every
  line is counted twice. Confirmed exactly: the report header reports `lines-valid="110849"` and a raw
  count of `<line number=` occurrences returns 110849.
- **Issue #478** — `Merge-CoberturaClassesByFilename` (`:167`) correctly unions the class-level
  `<lines>` across the group with max hits (`:217-268`) but never merges the other group members'
  `<methods>` (`:202-206`). The recomputed rate is then taken over the blended node set, counting the
  primary type's lines twice and the companion class's once.

Worked verification on `QuickFiler\Controllers\QfcHomeController.Iteration.cs`: the class-level
`<lines>` set is 45/56 = **80.36%**; the emitted attribute is `line-rate="0.8625"` (69/80) — overstated
by 5.9 points. An independent derivation in #424's own evidence agrees.

This matters here specifically because `QfcCollectionController` is async-heavy (~30 `async` members),
so nearly every new partial will have one or more state-machine companion classes and will therefore be
in the merged, wrong-attribute case.

### 7.3 The recompute recipe (mandatory)

1. Load the post-processed Cobertura XML.
2. Select **all** `<class>` elements whose `@filename` equals the target repo-relative path
   (case-insensitive, `\`-separated). Select all, because the merge is scoped per `<package>`.
3. Union their **`./lines/line` children only** — the class-level block. **Exclude
   `./methods//lines/line` entirely.** Key the union on `@number`, taking `MAX(@hits)`.
4. `line rate = |{ line : hits > 0 }| / |lines|`. If `|lines| == 0`, report **N/A**, never 0%.
5. For branch: over the same unioned set take lines with `@branch="True"`, parse `@condition-coverage`
   with `\((\d+)/(\d+)\)`, and sum. `branch rate = sum(covered)/sum(total)`. Where two class elements
   report the same line number with different condition counts, keep the larger total. If
   `sum(total) == 0`, report **N/A**, never 0%.

Step 3's class-level-only rule is the single load-bearing correction: it fixes both the double count
and the merge blend simultaneously, because the merge already wrote the correct union into the
class-level block.

Three states must be handled distinctly: **filename absent from the report**; **present with zero
lines**; **present with lines**. An interface-only file emits no `<class>` element whatsoever.

### 7.4 Branch coverage is enforceable

Branch data is emitted completely: every `<line>` carries a `branch` attribute, branching lines carry
`condition-coverage` and a `<conditions>` child list, and multi-condition lines are represented. The
75% branch gate is measurable per partial by step 5 above. Several proposed partials — the
pure-arithmetic `Layout.cs` in particular — may legitimately have very few branching lines; a file with
no branching lines yields `0/0` and reports **N/A**, never 0%.

### 7.5 Repository-wide measurement

- **CI produces no Cobertura at all.** `.github/workflows/ci.yml:147` runs `/EnableCodeCoverage`, which
  emits the binary `.coverage` format; nothing converts it or asserts a threshold, and only
  `TestResults/**/*.trx` and `TestResults/**/*.coverage` are uploaded. The repository-wide figure for
  this child must therefore be produced **locally**.
- **A stale-worktree pre-flight assertion is required before every measurement run.** Neither CI
  (`.github/workflows/ci.yml:134-140`) nor the local harness
  (`scripts/vscode/Invoke-MSTestWithCoverage.ps1:296-302`) filters `.claude/worktrees`. Running with
  `-SearchRoot .` from the canonical repo root sweeps in every `*.Test.dll` under
  `.claude/worktrees/<agent>/**/bin/Debug/`, producing both bogus assembly-initialization failures and
  a silently wrong denominator. The assertion must replicate the harness's own `\bin\Debug\` /
  `not \obj\` / `not \ref\` filter set and add a `\.claude\` exclusion, and must throw when any match
  is found.
- **Before and after artifacts must be produced by the identical command and both post-processed.**
  Comparing a raw `dotnet-coverage` report to a Koverage-processed report is a category error; it is
  the specific trap in #424's evidence that produced an apparent +38.6% denominator growth.
- **Report two repository-wide figures each time**: the harness-native `/coverage/@line-rate` and
  `/coverage/@branch-rate` (the comparator for the "retain or improve" gate, valid only because the
  same defective method is applied to both sides), and the recomputed figure from §7.3 (the honest
  figure, and the one to cite in prose). Never mix one with the other across the before/after boundary.
- **Do not use the epic's 70.19% figure as this child's comparator.** It is a raw, unprocessed report
  (`epic.md:479-480`, sourced from a baseline XML with no `<sources>` element). This child generates its
  own before-figure with its own command.

### 7.6 No coverage-config change is needed or permitted

`coverage.config:12-22` excludes seven third-party module patterns, none of which matches
`QuickFiler.dll`. There is no `Attributes`, `Functions`, `Sources`, `Companies`, or `PublicKeyTokens`
exclusion section anywhere in it. `QuickFiler/Properties/AssemblyInfo.cs` carries no assembly-level
`[ExcludeFromCodeCoverage]`. `TaskMaster.runsettings` is not on the harness path at all — the harness
passes `scripts/vscode/TaskMaster.cli.runsettings`.

**`[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcCollectionController.cs:21` is the sole
mechanism keeping the type out of instrumentation. Removing it is necessary and sufficient.** This
child creates no fan-in conflict on `coverage.config`.

### 7.7 Evidence locations

All evidence is written under `<FEATURE>/evidence/<kind>/` per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Timestamps use `yyyy-MM-ddTHH-mm`.

| Artifact | Path |
| --- | --- |
| Merge-base baseline (attribute present; file absent from the report) | `<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml` |
| Post-removal, pre-test measurement (the true starting figure) | `<FEATURE>/evidence/baseline/coverage-post-exemption-removal.<timestamp>.cobertura.xml` |
| Final measurement | `<FEATURE>/evidence/qa-gates/coverage-final.cobertura.xml` |
| Per-file line and branch table | `<FEATURE>/evidence/qa-gates/per-file-coverage.<timestamp>.md` |
| Toolchain pass record | `<FEATURE>/evidence/qa-gates/toolchain.<timestamp>.md` |
| Phase 0 gate outcomes | `<FEATURE>/evidence/other/phase0-f1-gate.<timestamp>.md` |
| Pre-flight stale-worktree assertion output | `<FEATURE>/evidence/other/preflight-stale-worktrees.<timestamp>.md` |

Writing evidence to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or any other
non-canonical path is a policy violation.

---

## 8. Seam Design

Seams are applied in the mandated order: **interface seam > injectable delegate > adapter**.

### 8.1 Seam inventory

| # | Seam | Tier | Replaces | Visibility |
| --- | --- | --- | --- | --- |
| S1 | `IQfcTlpSurface _tlpSurface` and `IQfcItemViewerSurface _viewerSurface` | interface | all ~45 control-read/write sites | `internal` interfaces, `private` fields |
| S2 | `UtilitiesCS.Threading.IUiDispatcher _uiDispatcher` | interface (**already exists**) | all 7 static `UiThread.Dispatcher` sites | `private` field |
| S3 | `Func<CancellationToken, ItemViewer> _itemViewerFactory` | delegate | `ItemViewerQueue.Dequeue` (2 sites) | `private` |
| S4 | `Func<QfcItemGroup, int, int, TlpCellStates, string, IQfcItemController> _itemControllerFactory` | delegate | `new QfcItemController(...)` (6 sites) | `private` |
| S5 | `Func<MailItem, Task<MailItemHelper>> _helperFactory` | delegate | `MailItemHelper.FromMailItemAsync` | `private` |
| S6 | `Action<string, string> _showError` | delegate | `MessageBox.Show` at `:186-191` | `private` |
| S7 | `Func<MailItem, bool, Task> _popOutAsync` | delegate | `new EfcHomeController(...)` (2 sites) | `private` |
| S8 | `Func<Task> _skipGroupAsync` | delegate | `((QfcFormController)_parent).SkipGroupAsync()` at `:1232` | `private` |
| S9 | `IEmailMoveMonitor` via optional ctor parameter | interface (**already exists**) | field initializer at `:78` | field already `private` |
| S10 | `Func<string, Task> _removeGroupByEntryId` | delegate (**already present** at `:1067`) | `RemoveSpecificControlGroup(string)` | `private` |

**S2 is mandatory, not optional.** `UiThread.Dispatcher` is a static `Dispatcher` with a `private set`
initialized to `null!` (`UtilitiesCS/Threading/UiThread.cs:135-140`) and assigned only inside `Init()`
(`:61`), so in a unit test every `UiThread.Dispatcher.InvokeAsync(...)` call throws a
`NullReferenceException`. The interface is `public` (`UtilitiesCS/Threading/IUiDispatcher.cs:15`), its
production adapter `WpfUiDispatcher` forwards to `UiThread.Dispatcher` from a parameterless constructor
(`UtilitiesCS/Threading/WpfUiDispatcher.cs:17,24-25`), and the sibling `QfcItemController` already
takes it as an optional constructor parameter
(`QuickFiler/Controllers/QfcItemController.Initialization.cs:38`). No new file, no new dependency, no
contract widening.

**`await _formViewer.UiSyncContext` is already seamed.** `IQfcFormViewer.UiSyncContext`
(`QuickFiler/Interfaces/IQfcFormViewer.cs:17`) is an interface member. A Moq default of `null` will
throw, because `SynchronizationContextAwaiter`'s constructor throws `ArgumentNullException` on a null
context (`UtilitiesCS/Threading/UiThread.cs:93-96`). Tests must set up `UiSyncContext` to return a real
`SynchronizationContext` whose `Post` executes inline. No production change is required.

**`log4net` is not a barrier.** `QfcCollectionControllerDarkModeTests.cs:50` already constructs the
type, forcing the static initializer at `:24-26` to run, and the suite passes. No seam is proposed for
it.

**S9 constraint.** The field must keep the name `_moveMonitor`;
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:359` injects it by reflection and would
otherwise break.

### 8.2 The four new seam files

| File (under `QuickFiler/Controllers/`) | Kind | Projected | Ledger bucket |
| --- | --- | --- | --- |
| `IQfcTlpSurface.cs` | `internal interface` | ~55 | `interface-only / not-measured` |
| `QfcTlpSurface.cs` | thin adapter over `TableLayoutPanel`/`Panel` | ~115 | `testable`, >= 90% via STA tests |
| `IQfcItemViewerSurface.cs` | `internal interface` | ~40 | `interface-only / not-measured` |
| `QfcItemViewerSurface.cs` | thin adapter over `ItemViewer` members | ~70 | `testable`; the **only** ratified-exemption candidate in this child (§8.4) |

Two interfaces rather than one, because the split matters for the coverage gate. `QfcTlpSurface`
operates on plain `TableLayoutPanel`/`Panel`/`RowStyle`, for which STA-thread coverage precedent
already exists (`UtilitiesCS.Test/HelperClasses/TableLayoutHelper_Tests.cs:11-23`,
`UtilitiesCS.Test/HelperClasses/WindowsForms/ScreenAndTableLayoutTests.cs:41-54`). `QfcItemViewerSurface`
operates on `ItemViewer` members backed by 6,224 lines of generated designer code carrying a WebView2
surface.

`TableLayoutHelper.InsertSpecificRow`/`RemoveSpecificRow`
(`UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs:13,55`) both begin with
`panel.InvokeRequired` (`:21`, `:62`) and therefore dereference a null panel. All six call sites in the
controller move behind S1.

### 8.3 Public-surface invariance — a constraint to hold, not an observation

**The proposed seam set adds nothing to `QuickFiler/Interfaces/IQfcCollectionController.cs`, and that
file must end this feature with a zero diff.**

- All ten seams are `private` fields on the class or `internal` interfaces in `QuickFiler.Controllers`.
- The only public-surface change is **optional trailing constructor parameters**. All three production
  construction sites — `QuickFiler/Controllers/QfcFormController.Actions.cs:49`, `:83`, `:139` — pass
  seven named arguments plus one positional and therefore compile unchanged. Those files are F6-owned
  and are not edited.
- `QuickFiler/Controllers/QfcItemGroup.cs:12` carries a vestigial
  `using static QuickFiler.Controllers.QfcCollectionController;`. A `partial` split does not affect it,
  because `using static` binds to the type, not the file. No F2 edit is required.
- `xComma` stays `public static string xComma(string)` on the `QfcCollectionController` type, in any
  partial file. `QuickFiler/Controllers/EfcHomeController.Metrics.cs:79` (F8) calls it, and that file
  must remain compilable and unedited.
- Members 33 (`EmailsToMove`) and 43 (`GetMoveDiagnostics`, including its `ref AppointmentItem`
  parameter) are frozen: no signature change, no rename, no removal. F7's existing Moq setup expression
  at `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:110,121` depends on the `ref`
  parameter.

**F7's "no contract additions needed" conclusion is preserved and must remain true at the end of this
feature.**

### 8.4 The single permitted exemption request

If a full `new ItemViewer()` proves unsafe or slow in the runner, `QfcItemViewerSurface.cs` is the
**one** file for which this child may request a ratified exemption. Conditions: it must not exceed
~70 lines; the request must carry a file-specific rationale meeting one of the three exemption grounds
in `epic.md:205-225`; and it must be recorded in F1's ledger with the **exact uncovered member list**.
A blanket file exemption is not acceptable, and a blanket re-exemption of
`QfcCollectionController.cs` is not acceptable under any circumstances.

Note the risk that motivates this: an `ItemViewer` obtained via
`FormatterServices.GetUninitializedObject` has no designer-assigned child controls, so
`LblItemNumber`, `LblSubject`, and `ConversationMenuItem` are `null` and any write throws.

---

## 9. Test Design

### 9.1 Conventions

MSTest (`[TestClass]`/`[TestMethod]`), Moq for mocks and stubs, FluentAssertions for assertions,
Arrange-Act-Assert, one descriptive name or docstring per scenario. Deterministic and isolated: no
temporary files, no external services, no live shown forms, no popups, no `Thread.Sleep`/`Task.Delay`,
no `DateTime.Now`/`DateTime.UtcNow`, no unseeded `Random`.

Test files mirror the partial names under `QuickFiler.Test/Controllers/`.

### 9.2 No new test may be added to the existing test file

`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is at **exactly 500 lines — compliant,
zero headroom**. All new tests go in new files. Splitting that file along the same partial boundaries
is optional and low-risk but is not required by policy today.

A policy audit of both existing test files found **no violation**: no banned APIs, no temporary files,
no live forms or popups, MSTest + Moq + FluentAssertions throughout, Arrange-Act-Assert throughout.

### 9.3 The partial split is source-compatible with both existing test files

Neither existing test file needs editing. Both address the type by name
(`typeof(QfcCollectionController)`), never by file; all reflection lookups use
`BindingFlags.NonPublic | BindingFlags.Instance` against the type;
`FormatterServices.GetUninitializedObject` operates on the type; and the seam work adds only optional
constructor parameters, so the 8-argument construction at
`QfcCollectionControllerDarkModeTests.cs:50-59` still binds.

### 9.4 Internals are testable; note one fragile dependency

`QuickFiler/Properties/AssemblyInfo.cs:5` grants `[assembly: InternalsVisibleTo("QuickFiler.Test")]`, so
`internal` seams on `QfcCollectionController` are directly reachable from tests. No grant needs adding
and no fallback to public seams is required.

`[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` — the grant that lets Moq proxy `internal`
types, including the two new `internal` seam interfaces — is **not** in `Properties/AssemblyInfo.cs`. It
is declared at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11`, an **F2-owned** file.

**Record this fragility; do not propose editing that file.** If F2's refactor moves, splits, or removes
that file without preserving the assembly-level attribute, every child relying on mocking a `QuickFiler`
internal loses that capability at fan-in, surfacing as a Castle `ProxyGenerationException` at runtime
rather than a compile error. The redundant copy at `QuickFiler/Legacy/IAcceleratorCallbacks.cs:5` is not
compiled and provides no cover.

`UtilitiesCS` grants internals only to `DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and
`ToDoModel.Test` (`UtilitiesCS/Properties/AssemblyInfo.cs:18-20`) — **not** to `QuickFiler.Test`. Any
`UtilitiesCS` internal remains unreachable; build a local seam rather than editing that file.

### 9.5 STA is a last resort and is minimized to two files

`QuickFiler.Test` already has working manual STA-thread helpers at
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:267-278` and `:302-317` (with
`ShutdownDispatcher` at `:323-326`). **No new NuGet package is required** — there is no
`MSTest.STAExtensions`/`[STATestClass]` in the project and none needs to be added.

Only the two production adapter files require STA: `QfcTlpSurface.StaTests.cs` and
`QfcItemViewerSurface.StaTests.cs`. Both live in dedicated `*.StaTests.cs` files, every control is
created in memory and never shown, and each STA-bound test documents why no seam can isolate the logic.
Everything else is plain `[TestClass]` with Moq.

`LoadGroup_03bAsync` needs a non-null `SynchronizationContext.Current` because of
`TaskScheduler.FromCurrentSynchronizationContext()`. Use
`SynchronizationContext.SetSynchronizationContext(new SynchronizationContext())`, the pattern already at
`QfcItemController.TestSupport.cs:87-93`. **No STA is required for it.**

### 9.6 Determinism under `ClassLevel` parallelism

`scripts/vscode/TaskMaster.cli.runsettings:3-8` (the file the harness actually applies) and
`TaskMaster.runsettings:3-8` both set `<Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope>`.
`Workers 0` means "use the processor count", so test-class-level parallelism is active on every harness
run.

**Confirmed hazard —** `private static int removespecificcontrolgroupcounter`
(`QuickFiler/Controllers/QfcCollectionController.cs:1157`). `Interlocked` makes the arithmetic atomic
(`:1161` increment, `:1247` decrement) but does not make the `> 1` read at `:1237` deterministic. Two
parallel test classes each observe the other's increment, producing both a flaky assertion and a
non-reproducible hit map for lines 1237-1242 — which can straddle the 80% gate between two runs of the
same commit. The decrement is also not in a `finally`, so any throw between `:1161` and `:1247` leaks a
permanent `+1` into process-global state.

All three mitigations are required:

1. Confine every test driving `RemoveSpecificControlGroup(int)` or `RemoveSpecificControlGroupAsync` to
   a **single** test class carrying `[DoNotParallelize]`. Precedent in this exact project:
   `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:11` and
   `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:22`.
2. Reset the counter to `0` by reflection in `[TestInitialize]`, defending against the missing `finally`
   and against a leak from a prior class.
3. Any test still using `ItemViewerQueue.SetCoreForTesting` carries `[DoNotParallelize]` plus a
   `[TestCleanup]` calling `ResetProductionCoreDefaultsForTesting()` and `ResetCoreForTesting()`, per
   `ViewerQueueStaticWrapperTests.cs:11-22`. Seam S3 exists so most tests avoid this entirely.

**No hazard —** `public static string xComma(string)` (`:2330`) is a pure static function with no field
access. Its hit map is deterministic and `QfcCollectionController.Move.cs` needs no
`[DoNotParallelize]`.

### 9.7 Branch-coverage attention list

The 75% branch gate is independent of the 80% line gate, so these members need explicit both-ways
cases: `ReadyForMove`, `TlpLayout`, `Digits`, `GenerateStringKbdAction` (three-way with an unhandled
default leaving `key == ""`), `UpdateSelectionNumberForRemoval` (four paths),
`DarkMode_CheckedChanged` (five paths), `RemoveSpecificControlGroup(int)` (six paths),
`RemoveSpecificControlGroupAsync` (eight paths), `GetMoveDiagnostics` (four paths, one unreachable),
`ToggleGroupConv(string)` (four paths), `LoadItemToTlp` (three paths).

### 9.8 Prohibited test shapes

No test may reflect over `typeof(IQfcCollectionController)` for the purpose of manufacturing coverage.
The interface file is `interface-only / not-measured`: it is reported **N/A**, never 0%, never counts
as a failure, and must **not** receive `[ExcludeFromCodeCoverage]`. Mocking the interface in service of
covering a consumer is a different thing and remains correct and expected.

---

## 10. Latent Defects — Characterize, Do Not Fix

Under the epic's no-behavior-change NFR, every defect found during research is characterized by a test
asserting **current** behavior and referenced by issue number. None is fixed in this child.

### 10.1 Promoted from this research (out of scope here)

| Issue | Subject |
| --- | --- |
| #468 | Twelve unreachable members, ~227 lines, isolated into `LegacyLoadPaths.cs` |
| #469 | Move-diagnostics defects |
| #470 | Conversation index defects |
| #471 | `EliminateSpaceForItems` sign error |
| #472 | Navigation `Digits` desync |
| #473 | Background-task and catch defects |
| #474 | Concrete downcast and modal property getter |
| #478 | Harness merge defect (the blended `line-rate`, §7.2) |

Per-defect mapping back to the research inventory (`research/qfc-collection-controller.md` §E1-E19)
should be confirmed against the issues at execution time. The binding rule does not depend on that
mapping: characterize, do not fix.

Two of these have an in-scope seam half whose production default keeps the current behavior
bit-identical, with only the design half deferred: the `((QfcFormController)_parent)` downcast at
`:1232` (seam S8) and the modal `MessageBox.Show` inside the `ReadyForMove` getter at `:186-191`
(seam S6).

### 10.2 Pre-existing issues that received new findings

**#444 — duplicate `KaKey` registration (`:1265-1272`). The defect is DORMANT.**
`WireUpKeyboardHandler` has **no caller** anywhere in the repository. Production wires keys through
`WireUpAsyncKeyboardHandler` (`:1275-1280`) -> `RegisterAsyncKeyActions` (`:1282-1291`), which registers
`Keys.Up`/`Keys.Down` exactly once each. Severity should be downgraded accordingly and the fix folded
into #468's dead-code removal.

Required characterization test, three assertions against current behavior:
(a) the `KbdActions(IEnumerable<UClass>)` constructor (`QuickFiler/Controllers/KbdActions.cs:26-29`)
does **not** throw, because it performs no duplicate check;
(b) `FilterKeys(Keys.Down)` returns **two** entries without throwing;
(c) `Find(Keys.Down)` throws `InvalidOperationException`.

**#286 — the static counter is process-global across instances.**
`removespecificcontrolgroupcounter` (`:1157`) is shared by every controller instance, so two independent
controllers running concurrently trip the `> 1` check at `:1237` and log a race-condition error with no
actual race. Required characterization test: assert the current process-global behavior across two
controller instances. That test lives inside the single `[DoNotParallelize]` class described in §9.6 and
resets the counter by reflection in `[TestInitialize]`.

---

## 11. Constraints

1. **`QuickFiler/Interfaces/IQfcCollectionController.cs` ends this feature with a zero diff.** (§8.3)
2. **`xComma` remains `public static` on the `QfcCollectionController` type.** (§8.3)
3. **No sibling-owned file is edited.** (§2.5)
4. **`QuickFiler.csproj` edits are additive `<Compile Include>` entries only, CRLF preserved.** (§6.3)
5. **The `_moveMonitor` field name is preserved.** (§8.1)
6. **No new test is added to `QfcCollectionControllerTests.cs`.** (§9.2)
7. **No production file in scope exceeds 500 lines; no test file exceeds 500 lines.**
8. **Evidence is written only under `<FEATURE>/evidence/<kind>/`.** (§7.7)
9. **The `line-rate`/`branch-rate` attributes are never read.** (§7.2)
10. **`scripts/vscode/Invoke-MSTestWithCoverage*.ps1` are not modified;
    `scripts/temp-extract-coverage.ps1` is not reused or extended.** (§2.5)

---

## 12. Risks

| # | Risk | Mitigation |
| --- | --- | --- |
| R1 | Seventeen `<Compile Include>` entries is the largest csproj delta of any wave-1 child; fan-in conflict is near-certain | Single contiguous hunk anchored after `:311`, CRLF preserved; additive on both sides so the resolution is to keep both. Handled by the child's own remediation loop, not a decomposition defect |
| R2 | The `DynamicProxyGenAssembly2` grant lives in an F2-owned file (`QfcHighConfidencePreFilter.cs:11`) | Recorded as a cross-child coupling. Prefer seams that do not require proxying a `QuickFiler` internal where a plain interface or delegate will do. Do not edit the F2 file |
| R3 | `Removal.cs` at ~348 projected lines may exceed 500 after seam work | Further split into `QfcCollectionController.RemoveGroup.cs` is pre-authorized (§6.1) |
| R4 | `QfcItemViewerSurface` may be uncoverable on an uninitialized `ItemViewer` | The single permitted exemption request, bounded at ~70 lines with an exact uncovered member list (§8.4) |
| R5 | Transient package-rate drop when the exemption is removed and 17 files enter the denominator at once | Order the plan so the package-level regression is closed before the final QA gate; state the expected transient drop in the evidence |
| R6 | Non-reproducible coverage on lines 1237-1242 under `ClassLevel` parallelism, straddling the 80% gate between runs | The three mandatory mitigations in §9.6 |
| R7 | The epic's "new files default to >= 90%" rule applies to partials carrying pre-existing extracted code, not new logic | Raise the tension with F1 explicitly rather than assume the 80% figure applies. This child does not unilaterally lower the bar |
| R8 | Measuring from the canonical repo root sweeps in stale `.claude/worktrees` assemblies | Mandatory pre-flight assertion before every measurement run (§7.5) |

The F1 dependency is **not** listed as a risk. Its absence today is expected; it is handled
deterministically by the Phase 0 gate in §4.2.

---

## 13. Cross-Child Notes

- **F7 (#433)** — unaffected. No `IQfcCollectionController` change. F7's three call sites
  (`EmailsToMove` twice, `GetMoveDiagnostics` twice, and a pass-through of `Groups`) resolve entirely
  against members already declared. F7's "no contract additions needed" conclusion is preserved.
- **F6** — `QfcFormController.Actions.cs:49,83,139` compile unchanged against optional trailing
  constructor parameters. No F6 file is edited. The two same-named `IQfcFormController` interfaces
  (`QuickFiler/Controllers/IQfcFormController.cs` and `QuickFiler/Interfaces/IQfcFormController.cs`) are
  an F6-owned design issue and are not touched here.
- **F8** — `xComma` must remain `public static` for `EfcHomeController.Metrics.cs:79`.
- **F2** — `QfcItemGroup.cs` is F2-owned, so `QfcItemGroup.ItemViewer` cannot be retyped from the
  concrete `ItemViewer` to `IItemViewer`. That would be the single highest-leverage testability change
  available; because it is out of file scope, a viewer-surface adapter (S1) is used instead. The
  `using static` at `QfcItemGroup.cs:12` is unaffected by the split. See also R2.
- **F1** — 17 ledger rows appended by this child; the interface-file row is F1's and is reconciled, not
  appended (§4.3).
- **In-flight work on `main`** — issues #400 and #424 do not overlap this child's file set. The
  integration branch is rebased on `main` before each wave by `epic-orchestrator`.

---

## 14. Documented Deviations

Each item corrects a statement in the delegation brief, in `issue.md`, or in inherited context. The
research artifact is the primary evidence in every case.

**D-1 — `QuickFiler` DOES grant `InternalsVisibleTo("QuickFiler.Test")`.**
`issue.md` § Constraints states "No `InternalsVisibleTo` grant from `UtilitiesCS` to `QuickFiler.Test`".
That is accurate **as to `UtilitiesCS`** and remains binding. The inherited implication that `internal`
seams therefore need a fallback is wrong: `QuickFiler/Properties/AssemblyInfo.cs:5` grants the internals
of `QuickFiler` itself to `QuickFiler.Test`, so `internal` seams on `QfcCollectionController` are
directly testable. All ten seams are `private` or `internal` and none is reachable through `UtilitiesCS`.

**D-2 — `QuickFiler.Test` already has working STA infrastructure.**
Inherited context (F4/#434 research) recorded "QuickFiler.Test has zero STA infra". False. Three manual
STA-thread helpers exist (§9.5). The epic's STA last-resort clause can be satisfied with **no NuGet
package** and no `packages.config` edit, materially lowering the risk of the STA path.

**D-3 — `[ExcludeFromCodeCoverage]` does not mean untested.**
`issue.md` states "Starting coverage is unknown and likely near zero". Research replaces this: two
compiled test classes already exercise the type and reach 24 of ~95 members. The estimated floor once
the exemption is removed is 12%-20%, most likely near 15%. The number that matters for planning is that
this child must add roughly 65-80 points of line coverage across 13 partials.

**D-4 — "verified with F1's harness" cannot be an unconditional requirement.**
The epic brief's AC1 wording assumes F1's harness exists and is correct. F1's harness may be absent, and
if present may read the untrustworthy `line-rate` attribute. The recompute recipe in §7.3 is therefore
the authoritative method and is a **mandatory fallback**; F1's harness is used only where it demonstrably
implements that recipe. This is gate G0.4, a record-and-proceed, never a halt.

**D-5 — the split is 13 partials plus 4 seam files, not "at least five files".**
`issue.md` § Problem states "a partial split into at least five files". Research produced a 13-partial
design at the same density as the existing `QfcItemController` family (10 partials for 3,073 lines),
plus 4 non-partial seam files.

**D-6 — the largest projected partial is ~348 lines, not 293.**
The delegation brief states "largest projected partial 293 lines". That figure is
`QfcCollectionController.LoadAsync.cs`. The largest is `QfcCollectionController.Removal.cs` at ~348
projected lines (`research/qfc-collection-controller.md` §A3 file 6), which is why the further split into
`RemoveGroup.cs` is pre-authorized in §6.1.

**D-7 — the epic's measured baseline table is quantitatively wrong, not merely indicative.**
`epic.md:155-178` was built from the Cobertura `line-rate` attributes. Its `Lines` column is roughly
double the real coverable-line count and its `Line %` column is **overstated** for every file with a
compiler-generated companion class. Files listed just above 80% in that table may in fact be below it.
No F11 planning input changes, because this child's files are absent from the table.

**D-8 — the 70.19% repository baseline is a raw, unprocessed figure.**
`epic.md:479-480` imported it from a baseline XML with no `<sources>` element, so it never passed
through `ConvertTo-KoverageCoberturaXml`. It is not comparable to any post-processed figure and must not
be used as this child's comparator (§7.5).

**D-9 — "Mid-Wave File Creation" rule 3 does not apply to the interface file.**
Rule 3 (`epic.md:580-582`) binds a child that *adds* a production file.
`QuickFiler/Interfaces/IQfcCollectionController.cs` is a pre-existing member of the compile set
(`QuickFiler/QuickFiler.csproj:360`), so its ledger row is F1's responsibility. This child verifies or
reconciles it. Rule 3 does apply to the 13 new partials and 4 seam files.

**D-10 — `issue.md` metadata is stale.**
`issue.md:5` records `Status: Promoted -> docs/features/active/quickfiler-collection-controller-coverage/`,
omitting the `2026-08-07-` date prefix and the `-454` issue suffix of the actual active folder;
`issue.md:11` records `Last Updated: 2026-08-08`, one day ahead of the preparation date. This spec uses
the real folder path throughout. Correcting `issue.md` is outside this task's write scope and is flagged
for the caller.

---

## 15. Acceptance Criteria

Each criterion names the artifact or command that verifies it. This section is the authoritative AC
block for `spec.md`; the Definition of Done in §16 is a numbered non-checkbox list so it cannot inflate
an AC tally.

- [ ] **AC1 — Split completed on responsibility seams.** `QuickFiler/Controllers/QfcCollectionController.cs` is split into the retained root plus the 13 partials named in §6.1, and **no production file created or modified by this feature exceeds 500 lines**. Verify: a line-count listing of `QuickFiler/Controllers/QfcCollectionController*.cs` and the four seam files recorded in `<FEATURE>/evidence/qa-gates/file-sizes.<timestamp>.md`, with every entry below 500.
- [ ] **AC2 — csproj edit is a single additive hunk with CRLF preserved.** Seventeen `<Compile Include>` entries (18 if the pre-authorized `RemoveGroup.cs` split is taken) are inserted contiguously immediately after `QuickFiler/QuickFiler.csproj:311`, with no property change, no reference change, and no reordering of unrelated entries. Verify: `git diff -- QuickFiler/QuickFiler.csproj` shows one contiguous addition-only hunk, and a `\r$` count over the file equals its line count.
- [ ] **AC3 — Exemption removed, no blanket re-exemption.** `[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcCollectionController.cs:21` and `using System.Diagnostics.CodeAnalysis;` at `:4` are removed, and no file in scope carries the attribute afterwards, with the single possible exception of `QuickFiler/Controllers/QfcItemViewerSurface.cs` under §8.4 — file-specific rationale, exact uncovered member list, and an F1 ledger entry. A blanket re-exemption of `QfcCollectionController.cs` is not acceptable. Verify: a repo-wide grep for `ExcludeFromCodeCoverage` restricted to this child's files, recorded in `<FEATURE>/evidence/qa-gates/`.
- [ ] **AC4 — Interface file untouched.** `QuickFiler/Interfaces/IQfcCollectionController.cs` has a zero diff for the whole feature. Verify: `git diff --exit-code <merge-base>..HEAD -- QuickFiler/Interfaces/IQfcCollectionController.cs` returns 0 with no output.
- [ ] **AC5 — Cross-child contracts preserved.** `xComma` remains `public static string xComma(string)` on the `QfcCollectionController` type; `EmailsToMove` and `GetMoveDiagnostics` (including the `ref AppointmentItem` parameter) are unchanged; the `_moveMonitor` field name is unchanged; and no sibling-owned file listed in §2.5 has any diff. Verify: `git diff --stat <merge-base>..HEAD` shows no sibling-owned file, and the analyzer/type-check build succeeds with `QuickFiler/Controllers/EfcHomeController.Metrics.cs` and `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` unedited.
- [ ] **AC6 — Per-file coverage gate.** `QuickFiler/Controllers/QfcCollectionController.cs` and **every** partial it is split into reaches **>= 80% line and >= 75% branch** coverage. Rates are computed by the §7.3 recipe (union of class-level `./lines/line` only, keyed on `@number` with `MAX(@hits)`; `./methods//lines/line` excluded), never from the `line-rate`/`branch-rate` attributes. Verify: `<FEATURE>/evidence/qa-gates/per-file-coverage.<timestamp>.md` lists one row per file with line and branch figures, and states explicitly that the attributes were not used and why, citing issues #441 and #478.
- [ ] **AC7 — New-file coverage gate and the N/A rule.** Every production file newly created by this feature reaches **>= 90% line coverage**. A file with zero coverable lines reports **N/A** for line, and a file with zero branch conditions reports **N/A** for branch — never 0%, and never counted as a failure. Verify: the same per-file table, with the two `interface-only` seam interface files shown as N/A.
- [ ] **AC8 — Ledger rows appended and reconciled.** Seventeen ledger rows (one per new production file) are appended to `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` as one contiguous block in the same change as the csproj entries, and the `IQfcCollectionController.cs` row is verified or reconciled to `interface-only / not-measured`. Verify: `git diff -- docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` shows one contiguous addition-only block containing every new file path.
- [ ] **AC9 — Phase 0 gate executed and recorded.** All four §4.2 gates are evaluated before any production edit: G0.1 and G0.2 halt on failure; G0.3 and G0.4 record and proceed, with the §7.3 recipe applied as the mandatory fallback for G0.4. Verify: `<FEATURE>/evidence/other/phase0-f1-gate.<timestamp>.md` records each gate, its outcome, and any of the literal codes `F1_LEDGER_RECONCILE`, `F1_HARNESS_DISAGREES`, or `F1_HARNESS_ABSENT_FALLBACK_APPLIED` that applied.
- [ ] **AC10 — Two baselines plus a like-for-like measurement protocol.** The merge-base baseline is captured with the attribute still present (the file is absent from that report, and that absence is the baseline); a second measurement is captured immediately after attribute removal and before any new test. Before and after artifacts are produced by the identical command with identical post-processing. The stale-`.claude/worktrees` pre-flight assertion is run before every measurement run. Verify: `<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml`, `<FEATURE>/evidence/baseline/coverage-post-exemption-removal.<timestamp>.cobertura.xml`, `<FEATURE>/evidence/qa-gates/coverage-final.cobertura.xml` (all three carrying a `<sources>` element proving post-processing), plus `<FEATURE>/evidence/other/preflight-stale-worktrees.<timestamp>.md`.
- [ ] **AC11 — Repository-wide coverage retained or improved.** Repository-wide line and branch coverage is retained or improved against **this child's own before-figure**, comparing harness-native to harness-native. Both the harness-native and the recomputed figures are reported for each run. The transient package-rate drop at exemption removal is stated in the evidence so it is not read as a regression. The epic's 70.19% figure is not used as the comparator. Verify: a before/after table in `<FEATURE>/evidence/qa-gates/coverage-delta.<timestamp>.md` showing both figure kinds for both runs and the identical command used for each.
- [ ] **AC12 — Test-policy compliance.** All new tests use MSTest, Moq, and FluentAssertions in Arrange-Act-Assert form; are deterministic and isolated; create no temporary files; contact no external services; construct no live shown forms; raise no popups; and contain no `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, or unseeded `Random`. STA-bound tests are confined to `*.StaTests.cs` files, each documenting why no seam is feasible. Verify: a banned-API grep over the new test files plus the STA file listing, recorded in `<FEATURE>/evidence/qa-gates/test-policy-audit.<timestamp>.md`.
- [ ] **AC13 — Existing test file untouched and size limits held.** No new test is added to `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (exactly 500 lines, zero headroom), and no test file created by this feature exceeds 500 lines. Verify: a line-count listing of all new and modified test files in `<FEATURE>/evidence/qa-gates/file-sizes.<timestamp>.md`.
- [ ] **AC14 — Determinism under `ClassLevel` parallelism.** Every test driving `RemoveSpecificControlGroup(int)` or `RemoveSpecificControlGroupAsync` lives in a **single** `[DoNotParallelize]` test class that resets `removespecificcontrolgroupcounter` to 0 by reflection in `[TestInitialize]`; every test using `ItemViewerQueue.SetCoreForTesting` carries `[DoNotParallelize]` plus the `[TestCleanup]` reset pair. Verify: two consecutive full test runs on the same commit produce identical per-file line and branch figures for the file containing those members, both runs recorded in `<FEATURE>/evidence/qa-gates/`.
- [ ] **AC15 — Defects characterized, not fixed.** A characterization test asserts #444's current behavior (constructor does not throw; `FilterKeys(Keys.Down)` returns two entries; `Find(Keys.Down)` throws `InvalidOperationException`) and a second asserts #286's current process-global counter behavior across two controller instances. Issues #468, #469, #470, #471, #472, #473, #474, and #478 are referenced by number and none is fixed. Verify: the named tests exist and pass, and a grep of the diff shows no change to the code paths those issues describe.
- [ ] **AC16 — No behavior change to observable QuickFiler flows.** The production diff is confined to file layout, `using` removal, optional trailing constructor parameters, and `private`/`internal` seam fields whose production defaults are bit-identical in effect to the code they replace. Both existing test files pass **unedited**. Verify: `git diff <merge-base>..HEAD -- QuickFiler/` reviewed against this list, and a full MSTest run showing `QfcCollectionControllerTests` and `QfcCollectionControllerDarkModeTests` passing with no source change to either file.
- [ ] **AC17 — Full C# toolchain green in final form.** `csharpier .`, then the analyzer build, then the nullable/`TreatWarningsAsErrors` build, then `vstest.console.exe` with coverage — all four pass in that order in a single final pass with no step auto-fixing files. Verify: `<FEATURE>/evidence/qa-gates/toolchain.<timestamp>.md` recording each command, its `EXIT_CODE`, and an output summary.

---

## 16. Definition of Done

A numbered, non-checkbox list. The `## Acceptance Criteria` block in §15 is the authoritative AC source
for this file.

1. All 17 acceptance criteria in §15 are checked off with the named artifact present.
2. The Phase 0 gate outcome is recorded and no halt condition is outstanding.
3. Both shared files (`QuickFiler/QuickFiler.csproj` and the epic coverage ledger) carry a single
   contiguous additive block each.
4. All evidence sits under `<FEATURE>/evidence/<kind>/` with ISO-8601 `yyyy-MM-ddTHH-mm` timestamps.
5. The full C# toolchain passes in order in a single final pass.
6. `user-story.md` acceptance criteria are checked off independently.
7. The working tree is clean and every evidence artifact is committed.

---

## 17. Seeded Test Conditions

Carried from `issue.md` § Test Conditions to Consider. These are inputs to the plan, not acceptance
criteria.

1. Per-partial unit coverage for every responsibility group produced by the split (13 partials).
2. Seam-level tests exercising injected delegates and adapters without live COM or forms (S1-S10).
3. Characterization of the #444 duplicate-`KaKey` registration without changing behavior, including the
   new dormancy finding.
4. STA last-resort tests, if unavoidable, isolated in dedicated `*.StaTests.cs` files — expected to be
   exactly two files.
5. Branch-coverage-sensitive scenarios per the §9.7 attention list, since the 75% branch gate is
   independent of the 80% line gate.
6. Direct-call tests for the twelve unreachable members isolated in `LegacyLoadPaths.cs`, each docstring
   stating that the member has no production caller and citing issue #468.
