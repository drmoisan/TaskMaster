# utilitiescs-nullable-dialogs-misc — Spec

- **Issue:** #374
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18
- **Status:** Draft
- **Version:** 0.1
- **Work Mode:** full-feature

## Overview

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` (a genuine recompile
rather than a silently-skipped incremental build), cannot be enforced against new code until
the pre-existing nullable-reference-type debt (CS86xx diagnostics) is remediated under a
per-file `#nullable enable` opt-in architecture. This feature is the Wave-1 child that
remediates the `UtilitiesCS/Dialogs/` cluster (12 of 16 `.cs` files; 4 Designer-generated
files excluded) plus the smallest defensible "misc" component named by the epic's
`dialogs-misc` label: `UtilitiesCS/WindowsAPI/ExtraDeclarations.cs` and
`UtilitiesCS/Properties/AssemblyInfo.cs`, both verify-only additions. Total remediation-target
count confirmed by research: 14 of 20 `.cs` files across the two areas receive the
`#nullable enable` pragma (12 substantive + 2 verify-only); 4 Designer-generated files stay
oblivious.

This cluster consumes exactly one cross-module contract from the Wave-0
`utilitiescs-nullable-extensions` child (issue #363): `WinFormsExtensions.Clone<T>()`. This
cluster's atomic plan must not begin until #363's Batch D (`WinFormsExtensions.cs`) has
merged — see "Upstream Dependency Mapping" below.

## Behavior

Each of the 14 opted-in files receives a per-file `#nullable enable` pragma. The 12 `Dialogs/`
substantive files are brought to zero CS86xx diagnostics under that pragma with
`TreatWarningsAsErrors`, applying nullable annotations (`?`), null-flow corrections, and
null-forgiving operators (`!`) only where justified. The 2 "misc" files
(`ExtraDeclarations.cs`, `AssemblyInfo.cs`) are confirmed zero-CS86xx by research and are
verify-only: the pragma is added and a clean rebuild is confirmed, with no annotation edits
expected. Existing null guards already present in the files remain as-is.

This is null-annotation and null-safety remediation only. There are NO behavior changes to
dialog display, button-wrapper, or MyBox logic, no refactors, no API redesign, and no feature
work. Public method signatures remain behavior-compatible: an existing caller that compiles
today continues to compile and behaves identically. Non-remediated files elsewhere in the
repository remain non-opted-in and must not be cross-blocked by this change.

The following are maintainer-mandated hard constraints, not options; no alternative
architecture is to be proposed or adopted:

- Add a `#nullable enable` pragma to each of the 14 in-scope files and bring each substantive
  file to zero CS86xx diagnostics under the pragma.
- Do NOT enable nullable at the project or solution level. `UtilitiesCS.csproj` has no
  `<Nullable>` element and must keep none. No project-level or solution-level `<Nullable>`
  element may be introduced by this feature.
- Annotation and null-safety ONLY: nullable annotations (`?`), null guards, null-forgiving
  operators (`!`) only where justified, and null-flow corrections. No behavior changes, no
  refactors, no API redesign, no feature work.
- Keep public signatures behavior-compatible; annotate to reflect the actual runtime null
  behavior so the annotations serve as accurate downstream contracts.

Files that are not opted-in remain in an oblivious nullable context and are not cross-blocking.
The four Designer-generated files (`DelegateButtonTemplate.Designer.cs`,
`FolderNotFoundViewer.Designer.cs`, `InputBoxViewer.Designer.cs`, `MyBoxViewer.Designer.cs`)
are never opted in and receive no pragma; because `#nullable enable` is lexical/per-file, this
produces no CS86xx from the Designer half and does not cross-block the opted-in hand-written
half of each pair.

## Inputs / Outputs

- Inputs (CLI flags, files, env vars): none. This is a library-internal source change with no
  runtime inputs.
- Outputs (artifacts, logs, telemetry): none added. No logging or telemetry is introduced.
- Config keys and defaults: none.
- Versioning or backward-compatibility constraints: public method signatures of the remediated
  types remain behavior-compatible. The observable change is limited to nullability
  annotations, which are additive contract metadata rather than a source- or binary-breaking
  behavior change. Examples: `InputBox.ShowDialog` becomes `string?` (already documented "or
  null if cancelled"); `MyBox.ShowDialog<T>`'s `FunctionButtonGroup<T>.Result` becomes
  unconstrained `T?`; `MyBoxModeless`'s internal `showAction` parameter becomes
  `Action<MyBoxViewer>? showAction` (reflecting its own documented null-defaulting behavior).

## API / CLI Surface

There is no CLI surface and no new API. This is a library-internal change. The relevant
"API surface" is the set of nullability annotations applied to the public and internal members
of the 14 opted-in files.

- Example invocations with expected outputs (concise): not applicable; no command or CLI flag
  is added. No `/p:Nullable=enable` global flag is introduced into any verification command
  (see Toolchain Note).
- Contracts and validation rules:
  - Public method/property signatures remain behavior-compatible; only nullability annotations
    change. `FolderNotFoundViewer.FolderAction` (uninitialized auto-property, CS8618 candidate),
    `MyBoxViewer._map` (uninitialized-in-default-ctor field, CS8618 candidate), and the three
    button wrappers' `_name`/`_button`/delegate-typed fields (CS8618 candidates) are annotated
    consistently across the trio (`ActionButton`, `DelegateButton`, `FunctionButton`) so the
    nullable-field-vs-non-null-with-guard decision does not diverge between near-duplicate
    implementations of the same pattern.
  - `FunctionButton<T>.Value` (public property, `internal set`, uninitialized until first click)
    is annotated `T?` or documented `default!`, consistent with the "prefer annotation over new
    guards" rule.
  - `InputBox.ShowDialog` returning `null` on cancel becomes `string?`, reflecting the existing
    documented "or null if cancelled" behavior — no behavior change.
  - `MyBox.ShowDialog<T>(..., FunctionButtonGroup<T> group)`'s `group.Result` returns via
    `FunctionButtonGroup<T>.Result` (`public T Result { get; set; }`), a deliberate
    unconstrained-generic contract decision annotated `T?` rather than adding a new runtime
    guard (mirrors the `Initializer.GetOrLoad<T>` / `ObjectCopier.Clone<T>` decisions already
    made in the helperclasses spec).
  - `MyBoxModeless`'s internal 5-argument overload's `Action<MyBoxViewer> showAction` parameter
    (invoked with `showAction: null` from the public 4-argument overload) is annotated
    `Action<MyBoxViewer>? showAction`, reflecting the file's own documented "defaulting to
    `viewer => viewer.Show()` when null" behavior.
  - Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`,
    `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) are not
    available or polyfilled on this target and must not be used or added (see Constraints &
    Risks). `[ExcludeFromCodeCoverage]` (already present in `MyBoxModeless.cs`) IS available on
    net481 and is not to be read as evidence that post-condition attributes are available too.

## Data & State

- Data transformations and invariants: none changed. This is annotation-only; no runtime data
  flow, transform, or invariant is altered. The existing `AsyncLocal<T>` dialog-invoker/response
  seams (`InputBox.DialogInvoker`, `MyBox.DialogInvoker`, `YesNoToAll.Response`) and their
  `?? RealDialogInvoker` fallback patterns remain unchanged.
- Caching or persistence details: none.
- Migration or backfill requirements (if any): none. No project-level `<Nullable>` element is
  introduced into `UtilitiesCS.csproj`; the project has no `<Nullable>` element today and must
  keep none. Enforcement is per-file pragma only.

## Constraints & Risks

The following mechanics flags are carried verbatim in substance from the research findings and
govern execution:

1. **Pragma-only verification command (do NOT use `/p:Nullable=enable`).** Local and CI
   verification of the opted-in files must use the pragma-only build
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true`, relying on each file's own `#nullable enable` pragma. It must
   NOT add `/p:Nullable=enable`, which would enable nullable project-wide and surface the whole
   epic's pre-existing CS86xx diagnostics across unrelated files as false failures unrelated to
   issue #374. This is a deliberate, documented deviation from the stock CLAUDE.md /
   `.claude/rules/csharp.md` type-check command for this child only; it must NOT be resolved by
   editing `.claude/rules/*`.
2. **Designer-file pairs stay non-opted-in; no combined-batch requirement.** Four remediation
   targets are the hand-written half of a Designer partial-class pair:
   `DelegateButtonTemplate.cs`, `FolderNotFoundViewer.cs`, `InputBoxViewer.cs`,
   `MyBoxViewer.cs`. In every case the `.Designer.cs` sibling declares only
   `private System.ComponentModel.IContainer components = null;` plus generated
   `InitializeComponent()` layout code — the same pattern documented and left oblivious for
   `DvgForm.Designer.cs` in the helperclasses spec (issue #364). Because only one side of each
   pair is ever opted in (unlike `SubjectMapSco`/`EmailDataMiner` in the email-parsing cluster
   or `Theme`/`Theme.Rendering` in the helperclasses cluster, which are two hand-written files
   of one partial type that must be annotated together), there is no cross-file
   nullable-contract-consistency risk to reconcile and no combined-batch requirement applies.
3. **Ordering precondition: #363 Batch D must merge first.** The only confirmed cross-module
   contract consumed by `Dialogs/` is `WinFormsExtensions.Clone<T>()` (Wave-0 issue #363, Batch
   D), called by `ActionButton`, `DelegateButton`, `FunctionButton`, and `MyBox`
   (`ButtonTemplate` setter `_template = value.Clone();`). This cluster's atomic plan must not
   begin until #363's Batch D (`WinFormsExtensions.cs`) has merged, so that the `Clone<T>`
   signature Dialogs compiles against is already annotated. Because `Clone<T>` is constrained
   `where T : Control` and returns a non-nullable `T` in the current (pre-annotation) code, and
   nothing in the method body suggests a null-returning path, the annotated signature is
   expected to remain `T` (non-nullable) — Dialogs callers require no special null-handling for
   this call.
4. **`helperclasses` (#364) dependency edge unconfirmed by source for this scope — flagged, not
   dropped.** The epic manifest's Wave-1 table lists `depends_on: [extensions, helperclasses]`
   for `dialogs-misc`. Grep across every `.cs` file in `UtilitiesCS/Dialogs/` for every
   `HelperClasses/` type name found zero matches. This finding does not falsify the declared
   dependency edge (both Wave-0 children are already prepared, so the edge is harmless), but the
   reason for the edge as applied to `Dialogs/` proper is unconfirmed by source evidence for
   this feature's scope. This is flagged for the maintainer/epic-planner, not silently dropped.
5. **Nullable post-condition attributes are NOT available on net481.** Nullable post-condition
   attributes from `System.Diagnostics.CodeAnalysis` (`[NotNullWhen]`, `[MaybeNullWhen]`,
   `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
   `[MemberNotNull]`) are NOT available or polyfilled on this target and must not be used or
   added. `MyBoxModeless.cs` already has a `using System.Diagnostics.CodeAnalysis;` for its
   existing `[ExcludeFromCodeCoverage]` attribute (available on net481); this must not be read
   as evidence that post-condition attributes are available too.
6. **No `record`/`record struct`/`init` risk in this cluster.** `BoxIcon` and
   `YesNoToAllResponse` are plain `enum`s; no `struct` declarations exist in `Dialogs/`. No
   CS0518 risk applies to this cluster.
7. **No file exceeds the 500-line limit.** All 12 `Dialogs/` remediation targets are well under
   the repo's 500-line limit (largest is `MyBox.cs` at 416 lines); no do-not-split flag is
   needed for this cluster, unlike the Extensions/HelperClasses/EmailParsingSorting clusters.
8. **Preserve `AsyncLocal<T>` dialog-invoker/response seams exactly.** `InputBox.DialogInvoker`,
   `MyBox.DialogInvoker`, and `YesNoToAll.Response` (issue #253/#264/#260 precedents) are
   already-annotated-in-spirit seams (the `AsyncLocal<Func<...>>` field itself is not
   nullable-prone; the `?? RealDialogInvoker` fallback pattern already guards the nullable
   `_dialogInvoker.Value`). Do not restructure these seams during annotation-only remediation.
9. **Prefer annotation plus justified `!` over new runtime guard statements.** New
   `if (x is null) throw` statements are executable lines that would require new test coverage
   (AC4 pressure) and could constitute a behavior change (AC3). Existing guards stay as-is.
10. **Duplicate-named test files — capture a clean baseline.** `UtilitiesCS.Test/Dialogs/`
    contains duplicate-named test file pairs (`DialogTest.cs` vs. `DialogTests.cs`,
    `InputBox_Test.cs`, `YesNoToAll_Test.cs` vs. `YesNoToAll_Tests.cs`). This is not necessarily
    a build problem (MSTest requires unique fully-qualified class names, not unique file names)
    but the atomic plan must capture a clean baseline test run before editing so any
    pre-existing ambiguity is not attributed to this feature's changes.
11. **No COM/Outlook interop in scope.** COM/Outlook interop types are not referenced anywhere
    in `UtilitiesCS/Dialogs/`; this is a pure WinForms cluster requiring only compile-time
    `msbuild /t:Rebuild` verification — no live Outlook process is needed.
12. **"Misc" files are verify-only.** `UtilitiesCS/WindowsAPI/ExtraDeclarations.cs` (entirely
    commented out) and `UtilitiesCS/Properties/AssemblyInfo.cs` (assembly attributes only) are
    each confirmed zero-CS86xx by research. Adding `#nullable enable` to each is expected to
    require no annotation edits; if either file unexpectedly emits a CS86xx diagnostic once the
    pragma is added, that finding must be resolved as annotation-only per the rest of this
    feature's constraints, not silently deferred.

## Implementation Strategy

- Implementation scope (what changes, not sequencing): add a `#nullable enable` pragma to each
  of the 14 in-scope files (12 `Dialogs/` substantive + 2 verify-only "misc") and bring each
  substantive file to zero CS86xx under the pragma. No new types, methods, commands, or files
  are added; only nullability annotations on existing members change. No project or solution
  file changes.
- New classes/functions/commands to add or update: none.
- Batch grouping (from research; leaf-first, dependency-ordered, annotation-scope only):
  - **Batch A — Leaves (no intra-cluster dependency)**: `DelegateButtonTemplate.cs`,
    `FolderNotFoundViewer.cs`, `MyBoxViewer.cs`, `InputBoxViewer.cs`.
    - `DelegateButtonTemplate.cs` is a trivial `Form` partial with a single constructor and no
      fields; likely verify-only or a one-line annotation change.
    - `FolderNotFoundViewer.cs`: `public string FolderAction { get; set; }` is an uninitialized
      non-nullable auto-property — genuine CS8618 candidate.
    - `MyBoxViewer.cs`: `private readonly Dictionary<string, Delegate> _map;` is set only in
      the 2-argument constructor, not the parameterless one — genuine CS8618 candidate.
    - `InputBoxViewer.cs`: trivial code-behind; verify-only or minor annotation.
  - **Batch B — Button wrapper types** (consume Batch A's `DelegateButtonTemplate.Button1` and
    the Wave-0 `WinFormsExtensions.Clone<T>` contract): `ActionButton.cs`, `DelegateButton.cs`,
    `FunctionButton.cs`. Remediate all three together in one review pass — not required for
    compile correctness, but they share the identical CS8618-prone shape
    (`_name`/`_button`/delegate-typed field), so annotating them together keeps the
    nullable-field-vs-non-null-with-guard decision consistent across the trio.
  - **Batch C — Direct viewer consumers** (consume only Batch A viewers, no Batch B
    dependency): `InputBox.cs`, `NotImplementedDialog.cs`.
  - **Batch D — `MyBox` core** (consumes Batch A's `MyBoxViewer` and Batch B's
    `ActionButton`/`DelegateButton`/`FunctionButton<T>`): `MyBox.cs`.
  - **Batch E — `MyBox` dependents** (consume Batch D's `MyBox` plus Batch B's
    `ActionButton`/`DelegateButton`): `MyBoxModeless.cs`, `YesNoToAll.cs`.
  - **Misc batch (verify-only, no ordering constraint on A-E)**:
    `UtilitiesCS/WindowsAPI/ExtraDeclarations.cs`, `UtilitiesCS/Properties/AssemblyInfo.cs`.
  - Full task-by-task sequencing within each batch belongs to the atomic plan, not this spec.
- **Partial-class batching rules:** No combined-batch requirement applies to the four
  Designer/hand-written pairs in this cluster (`DelegateButtonTemplate.cs`,
  `FolderNotFoundViewer.cs`, `InputBoxViewer.cs`, `MyBoxViewer.cs`). Because `#nullable enable`
  is lexical/per-file and only the hand-written half of each pair is ever opted in, there is no
  cross-file nullable-contract-consistency risk to reconcile — unlike the two-hand-written-file
  partial types in the email-parsing (`SubjectMapSco`/`EmailDataMiner`) and helperclasses
  (`Theme`/`Theme.Rendering`) clusters, which do require combined-batch remediation. This
  cluster has no analogous two-hand-written-file partial type.
- Upstream dependency mapping (issue #363 contracts consumed by this cluster):
  - `WinFormsExtensions.Clone<T>()` (Wave-0 Batch D, `UtilitiesCS/Extensions/WinFormsExtensions.cs`):
    consumed by `ActionButton.cs`, `DelegateButton.cs`, `FunctionButton.cs`, and `MyBox.cs`
    (`ButtonTemplate` setter `_template = value.Clone();`). No other `Extensions/` static class
    is referenced anywhere under `UtilitiesCS/Dialogs/` (grep-verified against every class name
    enumerated in the extensions spec's batch list).
  - **Ordering constraint:** this cluster's atomic plan must not begin until #363's Batch D
    (`WinFormsExtensions.cs`) has merged (see Constraints & Risks item 3).
  - `HelperClasses/` (#364): no confirmed direct consumption within `Dialogs/` itself
    (grep-verified against every `HelperClasses/` type name enumerated in the helperclasses
    spec — zero matches). See Constraints & Risks item 4 for the flagged, unconfirmed
    epic-declared dependency edge.
- Dependency changes (new/removed packages) and rationale: none.
- Logging/telemetry additions and locations: none.
- Rollout plan (feature flags, staged deploys, fallback path): not applicable. Each remediated
  batch is independently mergeable because non-opted-in files remain null-oblivious and are not
  cross-blocking under the per-file pragma architecture.

## Ownership Gaps Flagged for Epic-Planner / Maintainer

The epic manifest's Wave-1 table describes this child's scope as `Dialogs/` + "remaining small
subdirs (catch-all)" at "est. files ~16". Research finds that `Dialogs/` alone already contains
16 total `.cs` files (12 remediation targets + 4 Designer-excluded). This is strong evidence
that the epic's "~16" estimate already fully describes `Dialogs/` by itself, and that the
"+ remaining small subdirs" wording was aspirational rather than backed by an actual file-count
tally. Folding any of the larger residual trees in verbatim would silently multiply this
child's scope well past its stated estimate — from ~16 to approximately 126 files (roughly 8x),
per the reconciliation below.

| Residual area | File count | Genuine CS86xx risk (sampled evidence) | Recommendation |
|---|---|---|---|
| `Interfaces/**` | ~62 | Near-zero. Sampled `IForm.cs` / `IPrefix.cs`: pure interface member declarations with no bodies, no field storage, no constructors — CS8618 cannot fire. Matches the established `Interfaces/IHelperClasses/` precedent (out of scope for #364). | Recommend the epic formally exclude all of `Interfaces/**` from every child, extending the `IHelperClasses/` precedent repo-wide, rather than assign it piecemeal. |
| `Properties/` (`Resources.Designer.cs`, `Settings.Designer.cs`; `AssemblyInfo.cs` included in this feature's scope) | 3 (2 remain out of scope) | Near-zero. `Resources.Designer.cs` / `Settings.Designer.cs` are fully generated. | Recommend leaving the two Designer-generated files oblivious; `AssemblyInfo.cs` is already included in this feature as the smallest defensible "misc" addition. |
| `WindowsAPI/ExtraDeclarations.cs` (included in this feature's scope) | 1 | Zero. Every declaration in the file is commented out. | Included in this feature as a verify-only "misc" component. |
| `Examples/MSDemoConv.cs` | 1 | Genuine. COM/Outlook demo code: `mailItem.Parent as Outlook.Folder` then unguarded `folder.Store` dereference; `mailItem.GetConversation()` null-checked but earlier casts are not — real CS8602-class candidates. | Flag for maintainer: demo/sample code (namespace `UtilitiesCS.Examples`), not production surface. Recommend a maintainer decision (remediate, exclude via annotation-only guard, or delete) rather than defaulting it into this child. |
| `To Depricate/*` | 2 | Genuine but small. `FileIO2.cs` and `StringManipulation.cs` are real production helpers explicitly named for future deprecation. | Flag for maintainer: remediating code already marked for deprecation may be wasted effort; recommend a scope decision (remediate vs. exclude vs. schedule deletion). |
| `OneDriveHelpers/*` | 2 | Genuine, with an undeclared cross-cluster dependency. `OneDriveDownloader.cs` confirms a dependency on #363 (`using UtilitiesCS.Extensions;`) **and** calls `.RunWithTimeout(...)` / `.TryCopyToAsyncWithTimeout(...)`, resolving to `UtilitiesCS/Threading/TimeOutTask.cs` — a different Wave-0 cluster (`utilitiescs-nullable-threading`) not listed in `dialogs-misc`'s `depends_on`. | Flag prominently: folding this in would silently add an undeclared dependency edge on the Threading Wave-0 child. Recommend excluding unless the epic manifest's `depends_on` for `dialogs-misc` is updated to include `utilitiescs-nullable-threading`. |
| `OutlookObjects/` root + 8 leaf dirs | 13 | Genuine. Sampled `IOutlookReadinessGate.cs` / `OutlookReadinessGate.cs` (issue #207 precedent): `IsReady(Outlook.Store store)` documented "a null store returns false" — a genuine `Outlook.Store?` annotation candidate. These are natural cousins of the already-planned `outlook-folder-store` (#9007) and `outlook-mailitem-item` (#9008) Wave-1 children, neither of which claims this root/leaf-dir set. | Flag as a genuine epic-decomposition gap: recommend the epic-planner assign this 13-file set to one of the two existing Outlook Wave-1 children or spin off a dedicated child — do NOT fold into `dialogs-misc`, which has no thematic or dependency relationship to Outlook COM readiness/calendar/category/recipient code. |
| `EmailIntelligence/` root + `Evaluation/` + `OlFolderTools/` + `People/` | 26 | Genuine. Sampled `FilterEntry.cs`: a real class whose second constructor omits initializing `_description`, a genuine CS8618-class candidate. `OlFolderTools/` additionally contains 4 WinForms Designer/code-behind pairs requiring the same Designer-exclusion handling as `Dialogs/`. | Flag as the largest genuine gap: 26 files is more than 1.5x the entire `Dialogs/` cluster and would need its own leaf-first batch plan. These are natural cousins of `email-parsing` (#370) or `email-classifier`, neither of which claims this set. Recommend a dedicated Wave-1/Wave-1.5 child rather than folding into `dialogs-misc`. |

Count reconciliation: if every residual area above were folded in verbatim, the true total
would be approximately 16 (Dialogs) + 62 (Interfaces) + 3 (Properties) + 1 (WindowsAPI) + 1
(Examples) + 2 (To Depricate) + 2 (OneDriveHelpers) + 13 (OutlookObjects residual) + 26
(EmailIntelligence residual) = 126 files, roughly 8x the epic's estimate. This confirms that
the evidence-based target set materially exceeds ~16, and the correct response is to flag the
gap rather than silently absorb it. This feature's scope — `Dialogs/` (12 remediation targets)
plus the 2-file verify-only "misc" addition (14 files total receiving the pragma) — is the
smallest, dependency-clean, evidence-backed set that satisfies the epic's stated estimate
without introducing any undeclared cross-cluster dependency edge.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format → lint → type-check → test)

Acceptance criteria (from `issue.md`, mapped here for traceability):

- [x] AC1: Every one of the 14 in-scope files (12 `Dialogs/` remediation targets +
  `ExtraDeclarations.cs` + `AssemblyInfo.cs`) carries `#nullable enable` and compiles with zero
  nullable (CS86xx) diagnostics under the per-file pragma with `TreatWarningsAsErrors`.
- [x] AC2: No project-level or solution-level `<Nullable>` element is introduced;
  `UtilitiesCS.csproj` retains none.
- [x] AC3: No behavior change to dialog display, button-wrapper, or MyBox logic; existing
  `UtilitiesCS.Test/Dialogs/` tests still pass.
- [x] AC4: No coverage regression on changed lines.
- [x] AC5: Public signatures of the remediated types remain behavior-compatible; nullability
  annotations reflect actual null behavior and are consistent with the consumed
  `WinFormsExtensions.Clone<T>()` contract from `utilitiescs-nullable-extensions` (#363).
- [x] AC6: Non-remediated files (the 4 Designer-generated files and every other file outside
  this cluster) remain non-opted-in and are not cross-blocked; the change is independently
  mergeable under the per-file pragma architecture.

## Seeded Test Conditions (from potential)

- [ ] Existing `UtilitiesCS.Test/Dialogs/` suite (13 files) continues to pass with no behavior
  change.
- [ ] Changed-line coverage does not regress relative to baseline (prefer annotation and
  justified `!` over new runtime guards to avoid introducing uncovered executable lines).
- [ ] The pragma-driven nullable gate produces zero CS86xx diagnostics for the 14 opted-in
  files without passing `/p:Nullable=enable` globally.
- [ ] A baseline `vstest.console.exe` run (pass/fail counts and coverage percentage) for
  `UtilitiesCS.Test` (or at minimum `UtilitiesCS.Test/Dialogs/`) is captured before any edit,
  per the evidence-and-timestamp-conventions skill, so any regression during remediation is
  attributable to an annotation change and not a pre-existing duplicate-test-name ambiguity
  (see Constraints & Risks item 10).
- [ ] After each batch (A-E plus the misc batch), the same test assembly is rerun and pass/fail
  counts and per-file changed-line coverage are diffed against the baseline — no new failures,
  no coverage regression on the lines touched by that batch.

## Toolchain Note

Run the repo C# toolchain in CLAUDE.md order:

1. `csharpier .` (adding a pragma line and `?` annotations reformats; run before each build).
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
   /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzers / code style).
3. Nullable verification via the per-file pragma gate:
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true`. Under `TreatWarningsAsErrors`, any CS86xx in a pragma-enabled
   file becomes an error while non-opted files stay silent.
4. `vstest.console.exe <UtilitiesCS.Test assembly> /EnableCodeCoverage`.

Do NOT pass `/p:Nullable=enable` globally for this feature's verification. The global flag
forces nullable project-wide and surfaces the full pre-existing debt across the solution,
drowning this child's signal. This is the same rules-versus-convention conflict the Wave-0
spec and the epic manifest flag for the maintainer and defer to the Wave-2 CI capstone child;
resolving it is out of scope here.
