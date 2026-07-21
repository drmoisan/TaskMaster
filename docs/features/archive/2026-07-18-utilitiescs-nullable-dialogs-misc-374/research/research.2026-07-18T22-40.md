# Research: utilitiescs-nullable-dialogs-misc (Issue #374) — Scope Determination

- **Issue:** #374
- **Epic:** `utilitiescs-nullable-remediation` (manifest `docs/features/epics/utilitiescs-nullable-remediation/epic.md`), Wave 1, placeholder `feature_folder: utilitiescs-nullable-dialogs-misc` (epic frontmatter placeholder `issue_num: 9011`, back-filled to #374)
- **Base:** integration branch commit `ca3195aa` (checked out in this worktree)
- **Timestamp:** 2026-07-18T22-40
- **Researcher:** task-researcher (research-only; no source files modified outside this artifact)

## 1. Current State Analysis

### 1.1 What was read

- `docs/features/epics/utilitiescs-nullable-remediation/epic.md` (full manifest, Wave decomposition, ownership map).
- `docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/spec.md` (gold-standard shape: batch grouping,
  partial-class batching rules, upstream dependency mapping, net481 constraints, do-not-split rule).
- `docs/features/active/2026-07-18-utilitiescs-nullable-extensions-363/spec.md` and `plan.2026-07-18T21-20.md` (Wave-0,
  issue #363: batch grouping for `Extensions/`, confirms `WinFormsExtensions.cs` is Batch D).
- `docs/features/active/2026-07-18-utilitiescs-nullable-helperclasses-364/spec.md` and `plan.2026-07-18T21-21.md`
  (Wave-0, issue #364: 8-batch grouping for `HelperClasses/`, `Interfaces/IHelperClasses/` explicitly
  out of scope/oblivious, Designer-file handling precedent for `DvgForm.Designer.cs`).
- The existing (placeholder/unfilled) `docs/features/active/2026-07-18-utilitiescs-nullable-dialogs-misc-374/spec.md`,
  `issue.md`, `user-story.md`, `plan.2026-07-18T22-30.md` — all templates with no content yet; this
  research is the evidentiary input for filling them.
- Every `.cs` file under `UtilitiesCS/Dialogs/` (16 files, read in full).
- Targeted samples across every candidate residual subdirectory (see §4) to verify genuine CS86xx
  risk, existing `#nullable enable` state, and upstream dependencies, rather than assuming from
  directory names alone.
- `UtilitiesCS.Test/Dialogs/` test inventory (13 files) as the baseline regression harness.

### 1.2 `UtilitiesCS/Dialogs/` inventory (16 `.cs` files, verified by `Glob`)

No file under `UtilitiesCS/Dialogs/` currently carries `#nullable enable` (verified by `Grep` for
`#nullable` across the directory: zero matches). All 16 files are candidates; 4 are Designer-generated
and excluded, leaving 12 remediation targets.

| # | File | Kind | `#nullable enable` today | Classification |
|---|---|---|---|---|
| 1 | `DelegateButtonTemplate.Designer.cs` | Designer | No | Designer-excluded (generated; `IContainer components = null;` stays oblivious) |
| 2 | `DelegateButtonTemplate.cs` | Form code-behind (partial) | No | Remediation target (trivial: single ctor, no fields) |
| 3 | `FolderNotFoundViewer.Designer.cs` | Designer | No | Designer-excluded |
| 4 | `FolderNotFoundViewer.cs` | Form code-behind (partial) | No | Remediation target (`FolderAction` auto-property is CS8618-prone) |
| 5 | `InputBoxViewer.Designer.cs` | Designer | No | Designer-excluded |
| 6 | `InputBoxViewer.cs` | Form code-behind (partial) | No | Remediation target |
| 7 | `MyBoxViewer.Designer.cs` | Designer | No | Designer-excluded (references `SVGControl.PictureBoxSVG`, an out-of-cluster type; no bearing on annotation work) |
| 8 | `MyBoxViewer.cs` | Form code-behind (partial) | No | Remediation target (`_map` field uninitialized in default ctor) |
| 9 | `ActionButton.cs` | Plain class | No | Remediation target (`_name`, `_button`, `_action` CS8618-prone) |
| 10 | `DelegateButton.cs` | Plain class | No | Remediation target (same pattern as `ActionButton`) |
| 11 | `FunctionButton.cs` | Plain generic class | No | Remediation target (`_name`, `_button`, `_function` CS8618-prone; `Value` property) |
| 12 | `InputBox.cs` | Static class | No | Remediation target (`ShowDialog` returns `null` on cancel — CS8603-prone) |
| 13 | `MyBox.cs` | Static class | No | Remediation target (multiple `ShowDialog` overloads, generic `T` returns via `FunctionButtonGroup<T>.Result`) |
| 14 | `MyBoxModeless.cs` | Static class | No | Remediation target (`showAction: null` passed to non-optional `Action<MyBoxViewer>` param — CS8625-prone) |
| 15 | `NotImplementedDialog.cs` | Static class | No | Remediation target |
| 16 | `YesNoToAll.cs` | Static class | No | Remediation target |

Line counts for all 12 remediation targets are well under the repo's 500-line limit (largest is
`MyBox.cs` at 416 lines; no file-size flag is needed for this cluster, unlike the Extensions/
HelperClasses/EmailParsingSorting clusters).

### 1.3 Partial-class / Designer-pair handling

Four remediation targets are the hand-written half of a Designer partial-class pair:
`DelegateButtonTemplate.cs`, `FolderNotFoundViewer.cs`, `InputBoxViewer.cs`, `MyBoxViewer.cs`. In every
case the `.Designer.cs` sibling declares only `private System.ComponentModel.IContainer components = null;`
plus generated `InitializeComponent()` layout code — the same pattern documented and left oblivious for
`DvgForm.Designer.cs` in the helperclasses spec (issue #364, Constraints & Risks item 3). Because
`#nullable enable` is lexical/per-file, leaving the Designer half non-opted-in produces no CS86xx from
that half and does not cross-block the opted-in hand-written half. **No combined-batch requirement
applies** to these four pairs (unlike `SubjectMapSco`/`EmailDataMiner` in the email-parsing cluster or
`Theme`/`Theme.Rendering` in the helperclasses cluster, which are two hand-written files of one partial
type that must be annotated together) — here only one side of each pair is ever opted in, so there is no
cross-file nullable-contract-consistency risk to reconcile.

## 2. Candidate Approaches (batch grouping)

Two groupings were evaluated: (a) group strictly by file-name similarity (button-family files together,
viewer files together) with no dependency analysis, and (b) group by leaf-first internal dependency
order, verified by reading every file's actual field/constructor/type usage. Approach (a) is faster to
produce but risks ordering a consumer before its producer (e.g., batching `MyBox.cs` before
`MyBoxViewer.cs` even though `MyBox.ShowDialog` constructs `new MyBoxViewer()`). Approach (b) mirrors
the email-parsing precedent's rigor and was selected. Rejected-alternative summary: (a) is not used
because it does not verify a real dependency graph and does not match the precedent's leaf-first
convention.

### Selected batch grouping (leaf-first, dependency-ordered)

- **Batch A — Leaves (no intra-cluster dependency)**: `DelegateButtonTemplate.cs`,
  `FolderNotFoundViewer.cs`, `MyBoxViewer.cs`, `InputBoxViewer.cs`.
  - `DelegateButtonTemplate.cs` is a trivial `Form` partial with a single constructor and no fields;
    likely verify-only or a one-line annotation change.
  - `FolderNotFoundViewer.cs`: `public string FolderAction { get; set; }` is an uninitialized
    non-nullable auto-property — genuine CS8618 candidate.
  - `MyBoxViewer.cs`: `private readonly Dictionary<string, Delegate> _map;` is set only in the
    2-argument constructor, not the parameterless one — genuine CS8618 candidate (annotate `_map` as
    nullable or restructure the guard, per the "no behavior change" and "prefer annotation over new
    guards" constraints from the Wave-0 precedents).
  - `InputBoxViewer.cs`: trivial code-behind; verify-only or minor annotation.
- **Batch B — Button wrapper types** (consume Batch A's `DelegateButtonTemplate.Button1` and the
  Wave-0 `WinFormsExtensions.Clone<T>` contract — see §3): `ActionButton.cs`, `DelegateButton.cs`,
  `FunctionButton.cs`.
  - All three share the identical shape: `private string _name; private Button _button; private
    Button _template = new DelegateButtonTemplate().Button1; private <Delegate-typed field>;` — none
    initialized by the parameterless constructor, all genuine CS8618 candidates on `_name`, `_button`,
    and the delegate-typed field (`_action` / `_delegate` / `_function`). `FunctionButton<T>.Value`
    (public property, `internal set`) is also uninitialized-until-first-click and a candidate for `T?`
    or a documented `default!` decision consistent with the "prefer annotation over new guards" rule.
  - Recommend remediating all three together in one batch (not required for compile correctness since
    each is an independent standalone class, but they are near-duplicate implementations of the same
    button-wrapper pattern, so annotating them in the same review pass keeps the three CS8618
    decisions — nullable field vs. non-null-with-guard — consistent across the trio).
- **Batch C — Direct viewer consumers** (consume only Batch A viewers, no Batch B dependency):
  `InputBox.cs`, `NotImplementedDialog.cs`.
  - `InputBox.cs.ShowDialog` returns `string` but returns `null` on cancel (line ~94) — genuine CS8603
    candidate; the documented XML comment already says "or null if cancelled", so the annotation
    (`string?`) reflects existing, intentional behavior (no behavior change).
  - `NotImplementedDialog.cs` constructs `MyBoxViewer` via the 3-argument constructor and returns
    `bool`; low annotation risk, mostly verify-only plus the `DisplayInvoker` seam's delegate typing.
- **Batch D — `MyBox` core** (consumes Batch A's `MyBoxViewer` and Batch B's `ActionButton`/
  `DelegateButton`/`FunctionButton<T>`): `MyBox.cs`.
  - Largest file in the cluster (416 lines). Six `ShowDialog` overloads; the generic
    `ShowDialog<T>(..., FunctionButtonGroup<T> group)` overload returns `group.Result`, and
    `FunctionButtonGroup<T>.Result` (`public T Result { get; set; }`) is a deliberate unconstrained-
    generic contract decision (mirrors the `Initializer.GetOrLoad<T>` / `ObjectCopier.Clone<T>`
    decisions already made in the helperclasses spec) — annotate as `T?` rather than adding a new
    runtime guard, consistent with the AC4 "avoid new uncovered executable lines" preference.
- **Batch E — `MyBox` dependents** (consume Batch D's `MyBox` plus Batch B's `ActionButton`/
  `DelegateButton`): `MyBoxModeless.cs`, `YesNoToAll.cs`.
  - `MyBoxModeless.cs` (issue #264 / epic #260 precedent, already has an `[ExcludeFromCodeCoverage]`
    host-bound entry point) calls `MyBox.ReplaceButtons` (internal) and constructs `ActionButton`
    instances; its internal 5-argument overload declares `Action<MyBoxViewer> showAction` (no `?`) but
    is invoked with `showAction: null` from the public 4-argument overload — genuine CS8625 candidate;
    annotate the parameter `Action<MyBoxViewer>? showAction`, which reflects the file's own documented
    "defaulting to `viewer => viewer.Show()` when null" behavior (no behavior change, annotation only).
  - `YesNoToAll.cs` calls `MyBox.ShowDialog(message, "Dialog", BoxIcon.Question, delegateButtons)` and
    constructs `DelegateButton` instances; low annotation risk beyond the `AsyncLocal<YesNoToAllResponse>`
    field (value type, not nullable-reference-prone) and the `Properties.Resources.*` image arguments
    (generated resource properties, out of cluster scope).

Full task-by-task sequencing within each batch belongs to the atomic plan, not this research artifact
(per the email-parsing and Wave-0 precedents).

## 3. Upstream Dependency Mapping (contracts consumed from #363 / #364)

### 3.1 Confirmed: Extensions (#363) — `WinFormsExtensions.Clone<T>()`

`ActionButton.cs`, `DelegateButton.cs`, `FunctionButton.cs`, and `MyBox.cs` (via
`ButtonTemplate` setter `_template = value.Clone();`) all call `.Clone()` on a `Button` /
`Button`-typed value. This resolves to
`public static T Clone<T>(this T controlToClone, bool deep = false) where T : Control` in
`UtilitiesCS/Extensions/WinFormsExtensions.cs` (verified by reading the method body: it constructs a new
instance via `GetInstance<T>` and copies properties; it does not return a nullable `T`). Per the
extensions spec, `WinFormsExtensions.cs` is **Batch D** of the `utilitiescs-nullable-extensions` (#363)
plan (`EnumExtensions.cs`, `TraceExtensions.cs`, `WinFormsExtensions.cs`).

**Ordering constraint**: this cluster's atomic plan must not begin until #363's Batch D
(`WinFormsExtensions.cs` in particular) has merged, so that the `Clone<T>` signature Dialogs compiles
against is already annotated. Because `Clone<T>` is constrained `where T : Control` and returns a
non-nullable `T` in the current (pre-annotation) code, and nothing in the method body suggests a
null-returning path, the annotated signature is expected to remain `T` (non-nullable) — Dialogs callers
require no special null-handling for this call.

No other `Extensions/` static class (`StringExtensions`, `IEnumerableExtensions`, `ArrayExtensions`,
`JsonExtensions`, etc.) is referenced anywhere under `UtilitiesCS/Dialogs/` (verified by `Grep` across
the directory for every class name enumerated in the extensions spec's batch list; only `Clone` calls,
all resolving to `WinFormsExtensions`, were found).

### 3.2 HelperClasses (#364) — no confirmed direct consumption within `Dialogs/` itself

`Grep` across every `.cs` file in `UtilitiesCS/Dialogs/` for every `HelperClasses/` type name enumerated
in the helperclasses spec (`FilePathHelper`, `Initializer`, `PrettyPrint`, `ThemeHelpers`/`Theme`,
`SystemThemeDetector`, `TraceUtility`, `ReflectionHelper`, `DebugTextLogger`, `VerboseLogger`,
`ControlResizer`, `ScreenHelper`, `MouseDownFilter`, `TableLayoutHelper`, `ObjectCopier`, `DeepCompare`,
etc.) found **zero matches**. The epic manifest's Wave-1 table lists `depends_on: [extensions,
helperclasses]` for `dialogs-misc`, but for the `Dialogs/`-only scope this research verifies no file
in `Dialogs/` actually imports or calls a `HelperClasses/` member. This is flagged (not silently
corrected) for the atomic-plan author: the declared dependency edge is not falsified by this finding
(both Wave-0 children are already prepared, so the edge is harmless), but the *reason* for the edge as
applied to `Dialogs/` proper is unconfirmed by source evidence. If the eventual scope is expanded to
include residual subdirectories (see §4), the `helperclasses` dependency edge becomes concretely
relevant only for files outside `Dialogs/` that reference `HelperClasses/` types — none were found in
the smallest defensible expanded set either (see §4.3).

## 4. Ownership Gaps to Flag for Epic-Planner / Maintainer

The epic manifest's Wave-1 table describes this child's scope as `Dialogs/` + "remaining small subdirs
(catch-all)" at "est. files ~16". This research finds that **`Dialogs/` alone already contains 16 total
`.cs` files** (12 remediation targets + 4 Designer-excluded). This is strong evidence that the epic's
"~16" estimate already fully describes `Dialogs/` by itself, and that the "+ remaining small subdirs"
wording was aspirational/approximate rather than backed by an actual file-count tally across the
listed residual directories. Folding any of the larger residual trees in verbatim would silently
multiply the child's scope well past its stated estimate. Each residual area was sampled (not assumed)
to determine genuine CS86xx risk and dependency exposure:

| Residual area | File count | Genuine CS86xx risk (sampled evidence) | Recommendation |
|---|---|---|---|
| `Interfaces/**` | ~62 | **Near-zero.** Sampled `IForm.cs` and `IPrefix.cs`: pure interface member declarations (properties, events, method signatures) with no bodies, no field storage, no constructors. Interface auto-property syntax does not back a field, so CS8618 cannot fire; adding `#nullable enable` to a declaration-only interface produces at most a handful of return/param nullability *decisions*, not compiler errors. This matches the already-established precedent that `Interfaces/IHelperClasses/` is explicitly out of scope/oblivious for #364. | Recommend the epic formally exclude all of `Interfaces/**` from every child (extend the existing `IHelperClasses/` precedent repo-wide) rather than assign it piecemeal to whichever child happens to touch the concrete type implementing it. |
| `Properties/` | 3 | **Near-zero.** `AssemblyInfo.cs` is assembly-level attributes only; `Resources.Designer.cs` / `Settings.Designer.cs` are fully generated. | Recommend leaving all three oblivious; `AssemblyInfo.cs` is a safe verify-only add if the maintainer wants every file opted in, but not required. |
| `WindowsAPI/ExtraDeclarations.cs` | 1 | **Zero.** Verified by `Grep`: every `public`/`extern` declaration in the file is commented out; the file compiles to an effectively empty namespace. | Lowest-risk optional inclusion (verify-only); safe to add to this child if the maintainer wants a non-empty "misc" component. |
| `Examples/MSDemoConv.cs` | 1 | **Genuine.** COM/Outlook demo code: `mailItem.Parent as Outlook.Folder` then unguarded `folder.Store` dereference, and `mailItem.GetConversation()` is checked for null but earlier casts are not — real CS8602-class candidates. | Flag for maintainer: this is demo/sample code (namespace `UtilitiesCS.Examples`), not production surface. Recommend deciding whether to remediate, exclude via `[ExcludeFromCodeCoverage]`-style annotation-only guard, or delete, rather than defaulting it into this child. |
| `To Depricate/*` | 2 | **Genuine but small.** `FileIO2.cs` and `StringManipulation.cs` are real (if minor) production helpers explicitly named for future deprecation. | Flag for maintainer: remediating code already marked for deprecation may be wasted effort; recommend a scope decision (remediate vs. exclude vs. schedule deletion) rather than silent inclusion. |
| `OneDriveHelpers/*` | 2 | **Genuine, with an undeclared cross-cluster dependency.** `OneDriveDownloader.cs` has an explicit `using UtilitiesCS.Extensions;` (confirmed dependency on #363) **and** calls `.RunWithTimeout(...)` / `.TryCopyToAsyncWithTimeout(...)`, which resolve to `UtilitiesCS/Threading/TimeOutTask.cs` — a **different** Wave-0 cluster (`utilitiescs-nullable-threading`) not listed in `dialogs-misc`'s `depends_on`. | Flag prominently: folding this in would silently add an undeclared dependency edge on the Threading Wave-0 child. Recommend excluding from this child unless the epic manifest's `depends_on` for `dialogs-misc` is updated to include `utilitiescs-nullable-threading`. |
| `OutlookObjects/` root + 8 leaf dirs (`AppointmentItem`, `Calendar`, `Category`, `Com`, `Explorer`, `Fields`, `Filter DASL`, `Recipient`) | 13 | **Genuine.** Sampled `IOutlookReadinessGate.cs` / `OutlookReadinessGate.cs` (issue #207 precedent, COM-bound, already documented as COM/VSTO coverage-exempt but not nullable-exempt): `IsReady(Outlook.Store store)` is documented "a null store returns false" — a genuine `Outlook.Store?` annotation candidate. These are natural cousins of the already-planned `outlook-folder-store` (#9007, Folder+Store only) and `outlook-mailitem-item` (#9008, MailItem/Item/Conversation/Attachment/Table only) Wave-1 children, neither of which claims this root/leaf-dir set. | Flag as a genuine epic-decomposition gap: recommend the epic-planner assign this 13-file set to one of the two existing Outlook Wave-1 children (both already depend on extensions+helperclasses, so no new dependency edge) or spin off a dedicated child — do NOT fold into `dialogs-misc`, which has no thematic or dependency relationship to Outlook COM readiness/calendar/category/recipient code. |
| `EmailIntelligence/` root (4) + `Evaluation/` (2) + `OlFolderTools/` (18) + `People/` (2) | 26 | **Genuine.** Sampled `FilterEntry.cs` (root): a real class whose second constructor omits initializing `_description`, a genuine CS8618-class candidate. Sampled `SmithWaterman.cs` (`OlFolderTools/OlFolderHelper/`): real string-processing production logic. `OlFolderTools/` additionally contains 4 WinForms Designer/code-behind pairs (`FilterOlFoldersViewer`, `FolderInfoViewer`, `OSBrowser`, `OSFolder` in `FilterOlFolders/`; `FolderRemapViewer`, `FolderSelector` in `FolderRemap/`) requiring the same Designer-exclusion handling as `Dialogs/`. | Flag as the largest genuine gap: 26 files is more than 1.5x the entire `Dialogs/` cluster and would need its own leaf-first batch plan (it already has 4 Designer pairs, mirroring `Dialogs/`'s structure). These are natural cousins of `email-parsing` (#370) or `email-classifier`, neither of which claims this set per the epic's ownership map. Recommend a dedicated Wave-1/Wave-1.5 child rather than folding into `dialogs-misc`. |

### 4.1 Count reconciliation against the epic's ~16 estimate

- **Definitive scope (this research's recommendation): `Dialogs/` only — 16 total `.cs` files, 12
  remediation targets.** This matches the epic's "~16 files" estimate essentially exactly and requires
  no scope expansion.
- If every residual area in the table above were folded in verbatim, the true total would be
  approximately 16 (Dialogs) + 62 (Interfaces, near-zero risk) + 3 (Properties) + 1 (WindowsAPI) + 1
  (Examples) + 2 (To Depricate) + 2 (OneDriveHelpers) + 13 (OutlookObjects residual) + 26
  (EmailIntelligence residual) = **126 files**, roughly 8x the epic's estimate. This confirms the
  instruction's premise: the evidence-based target set materially exceeds ~16, and the correct response
  is to flag the gap rather than silently absorb it.

### 4.2 Recommended target set (submitted to atomic planning)

`UtilitiesCS/Dialogs/` only — the 12 remediation targets in §1.2/§2, batched A–E. This is the
smallest, dependency-clean, evidence-backed set that satisfies the epic's stated estimate without
introducing any undeclared cross-cluster dependency edge (the only confirmed upstream contract is
`WinFormsExtensions.Clone<T>` from #363 Batch D, already an epic-declared dependency).

### 4.3 Smallest defensible expanded scope (only if the maintainer wants a non-empty "misc" component)

If the maintainer prefers this child to close at least one item of the "+ remaining small subdirs"
wording rather than leaving it entirely to a future child, the smallest defensible addition — chosen
because it is zero-risk (verify-only, no genuine annotation work, no new dependency edge) — is:

- `UtilitiesCS/WindowsAPI/ExtraDeclarations.cs` (entirely commented out; verify-only).
- `UtilitiesCS/Properties/AssemblyInfo.cs` (assembly attributes only; verify-only).

This raises the total to 14 files (12 Dialogs remediation targets + 2 verify-only) while deliberately
excluding `Examples/`, `To Depricate/`, `OneDriveHelpers/`, the `OutlookObjects/` residual, and the
`EmailIntelligence/` residual for the reasons in the table above (undeclared dependency edges, genuine
non-trivial remediation work more thematically owned by the Outlook/Email Wave-1 children, or
deprecation-candidate status requiring a maintainer decision before spending remediation effort).

## 5. net481 / C# 12 Constraints (verified against the `Dialogs/` cluster specifically)

- Target framework net481, C# 12 (`LangVersion` 12.0) — same as every other child in this epic.
- Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`,
  `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) from
  `System.Diagnostics.CodeAnalysis` are **not available or polyfilled** on this target and must not be
  used or added — identical constraint to #363/#364/#370. `MyBoxModeless.cs` already has a
  `using System.Diagnostics.CodeAnalysis;` for its existing `[ExcludeFromCodeCoverage]` attribute (that
  attribute IS available on net481); this must not be read as evidence that post-condition attributes
  are available too.
- No `record` / `record struct` / `init` accessors anywhere in the cluster (`BoxIcon` and
  `YesNoToAllResponse` are plain `enum`s; no `struct` declarations exist in `Dialogs/`). No CS0518 risk
  in this cluster.
- No file in the cluster exceeds the 500-line limit (largest is `MyBox.cs` at 416 lines); no
  do-not-split flag is needed, unlike the Extensions/HelperClasses/EmailParsingSorting clusters.
- COM/Outlook interop types are **not referenced anywhere in `Dialogs/`** (verified: no
  `Microsoft.Office.Interop.Outlook` using directive or type reference in any of the 16 files). This is
  a pure WinForms cluster; compile-time `msbuild /t:Rebuild` verification requires no live Outlook
  process, and there is no COM-boundary annotation-decision complexity comparable to the
  `PhysicalFileInfoAdapter` or `DispatchUtility` cases in #364. (Contrast: if `Examples/MSDemoConv.cs`
  or the `OutlookObjects/` residual were folded in, COM interop annotation decisions — e.g. `as Outlook.Folder`
  nullability — would become necessary; this is one more reason those areas are recommended for
  separate scope handling rather than blanket inclusion.)
- Existing test-seam patterns already present in the cluster (issue #253/#264/#260 precedents) use
  `AsyncLocal<T>` for per-flow dialog-invoker/response storage (`InputBox.DialogInvoker`,
  `MyBox.DialogInvoker`, `YesNoToAll.Response`) — these are already-annotated-in-spirit seams (the
  `AsyncLocal<Func<...>>` field itself is not nullable-prone; the `?? RealDialogInvoker` fallback
  pattern already guards the nullable `_dialogInvoker.Value`). Preserve these seams exactly; do not
  restructure them during annotation-only remediation.
- Duplicate-named test files exist in `UtilitiesCS.Test/Dialogs/`, mirroring the email-parsing
  precedent's flagged constraint: `DialogTest.cs` vs. `DialogTests.cs`, `InputBox_Test.cs`,
  `YesNoToAll_Test.cs` vs. `YesNoToAll_Tests.cs`. As with the email-parsing cluster, this is not
  necessarily a build problem (MSTest requires unique fully-qualified class names, not unique file
  names), but the atomic plan must capture a clean baseline test run before editing so any pre-existing
  ambiguity is not attributed to this feature's changes.

## 6. Verification Approach

- Per-file pragma gate (do **not** pass `/p:Nullable=enable` globally, per every Wave-0/Wave-1
  precedent in this epic):
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
  /p:TreatWarningsAsErrors=true`
- Baseline capture before any edit: run the pragma gate to confirm zero CS86xx from currently-enabled
  files (expected: zero, since no `Dialogs/` file is opted in today), then run the full
  `UtilitiesCS.Test` suite (or at minimum the `UtilitiesCS.Test/Dialogs/` 13-file subset) with coverage
  via the repo's `scripts/vscode/Invoke-MSTestWithCoverage.ps1` wrapper, and record numeric baseline
  pass/fail counts and line/branch coverage before Batch A begins, per the evidence-and-timestamp-
  conventions skill (`<FEATURE>/evidence/baseline/...`, `yyyy-MM-ddTHH-mm` timestamps).
- Per-batch: pragma-gate rebuild + `UtilitiesCS.Test/Dialogs/` regression run, mirroring the Wave-0
  precedents' per-batch verification structure.
- Final QC: full toolchain in CLAUDE.md order (csharpier -> analyzer/codestyle build -> pragma-only
  nullable build -> vstest with coverage), plus the same AC2 (`no <Nullable> element added`) and
  no-post-condition-attribute verification greps used by #363/#364.
- Ordering precondition: this cluster's atomic plan must not begin until #363's Batch D
  (`WinFormsExtensions.cs`) has merged (see §3.1). No confirmed precondition on #364 exists for the
  `Dialogs/`-only scope (see §3.2); if the maintainer expands scope per §4.3 or beyond, re-evaluate
  this precondition against the newly included files' actual `HelperClasses/`/`Threading/` usage.

## 7. Automation Feasibility

Not applicable. This is a source-only, compile-time nullable-annotation change (`.cs` file edits plus
an `msbuild`/`vstest.console.exe` verification loop) with no third-party UI, browser, or interactive
human-in-the-loop surface of any kind — the same posture as every other child in this epic. The
autonomous-execution research gate is satisfied by this statement; no human-interaction automation
feasibility assessment is required.
