# utilitiescs-nullable-residuals — Spec

- **Issue:** #375
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T23-13
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-feature

## Overview

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` (a genuine recompile
rather than a silently-skipped incremental build), cannot be enforced against new code until the
pre-existing nullable-reference-type debt (CS86xx diagnostics) across `UtilitiesCS/` is
remediated under a per-file `#nullable enable` opt-in architecture. This feature is the Wave-1
child that remediates the residual, previously-unowned CS86xx-risk trees that no other Wave-0 or
Wave-1 child claims: the 44 files enumerated by the epic's Residual-Scope Decision
(`Examples/` 1 + `To Depricate/` 2 + `OneDriveHelpers/` 2 + `OutlookObjects/` residual 13 +
`EmailIntelligence/` residual 26). These residuals were surfaced by the `dialogs-misc` (#374)
spec's "Ownership Gaps Flagged for Epic-Planner / Maintainer" section and reconciled against the
epic's definition-of-done inventory before being assigned to this child.

This is null-annotation and null-safety remediation ONLY. It introduces no behavior change, no
refactor, no API redesign, and no feature work. The child's declared upstream contracts are the
three Wave-0 clusters it consumes: `utilitiescs-nullable-extensions` (#363),
`utilitiescs-nullable-helperclasses` (#364), and `utilitiescs-nullable-threading` (#369) —
`depends_on: [extensions, helperclasses, threading]`, all Wave 0, so this child is Wave 1.

## Behavior

Remediate the residual pre-existing nullable-reference-type debt across the 44 in-scope files
using a per-file `#nullable enable` opt-in. The following are maintainer-mandated hard
constraints, not options; no alternative architecture is to be proposed or adopted:

- Add a `#nullable enable` pragma to each in-scope hand-written file and bring that file to zero
  CS86xx diagnostics under the pragma with `/p:TreatWarningsAsErrors=true`.
- Do NOT enable nullable at the project or solution level. `UtilitiesCS.csproj` has no
  `<Nullable>` element and must keep none. No project-level or solution-level `<Nullable>`
  element may be introduced by this feature.
- Annotation and null-safety ONLY: nullable annotations (`?`), null-flow corrections, and
  null-forgiving operators (`!`) only where justified. Preserve existing null guards exactly.
  Prefer annotation plus justified `!` over introducing any new runtime guard; add a new runtime
  guard only if zero CS86xx is otherwise strictly unreachable.
- No behavior changes, no refactors, no API redesign, no feature work. Keep public signatures
  behavior-compatible; annotate to reflect the actual runtime null behavior so the annotations
  serve as accurate downstream contracts.

Files that are not opted-in remain in an oblivious nullable context and are not cross-blocking.
This per-file lexical mechanism is what lets each epic child (and each batch within this child)
merge independently without requiring the entire epic (~2131 diagnostics across ~234 files) to be
fixed first.

The nullable debt in these residual files is dominated by each file's OWN members, because every
cross-cluster type they touch other than the three declared upstreams is nullable-oblivious at
execution time (either not yet opted in, or in a different assembly). Under the pragma,
dereferencing an oblivious-typed member produces no CS86xx. Consequently the real debt profile is:
CS8618 (uninitialized non-null fields / auto-properties / events), CS8603/CS8625 (own
`return null` / `= null` on unconstrained-`T` or reference returns), CS8600 (`x as T` result
locals), and self-induced CS8602 (appearing only after a field is annotated `T?`, resolved with a
justified `!` rather than a new guard).

## Inputs / Outputs

- Inputs (CLI flags, files, env vars): none at runtime. The change inputs are the 44 residual
  `.cs` files enumerated in the epic's Residual-Scope Decision. The 6 `*.Designer.cs` files under
  `OlFolderTools` are OUT of scope for opt-in; they receive no pragma and stay oblivious.
- Outputs (artifacts, logs, telemetry): none added. No logging or telemetry is introduced. The
  source outputs are a `#nullable enable` pragma plus annotation/null-safety edits on each
  in-scope hand-written file that emits CS86xx; no new files, no removed files, no project-file
  edits.
- Config keys and defaults: none introduced. `UtilitiesCS.csproj` remains without a `<Nullable>`
  element.
- Versioning or backward-compatibility constraints: public member signatures remain
  behavior-compatible. Nullability annotations added to public members are additive contract
  metadata, not a source- or binary-breaking behavior change. Examples: `ComType.GetTypeName`
  returns `string?` (already `return null`); `Calendar.FindCalendar` returns `Folder?`;
  `OneDriveDownloader.TryGetUrlStreamAsync` / `TryGetFileStreamWriter` return `Task<Stream?>`
  (callers already null-check); `IOutlookReadinessGate.IsReady(Store store)` becomes
  `IsReady(Store? store)`, co-annotated with `OutlookReadinessGate`, matching the documented "a
  null store returns false" contract.

## API / CLI Surface

There is no CLI surface and no new API. This is a library-internal change. The relevant "API
surface" is the set of nullability annotations applied to the public and internal members of the
in-scope files.

- Example invocations with expected outputs (concise): not applicable; no command or CLI flag is
  added. No `/p:Nullable=enable` global flag is introduced into any verification command (see
  Constraints & Risks item 1).
- Contracts and validation rules:
  - Public method/property signatures remain behavior-compatible; only nullability annotations
    change. Uninitialized non-null fields/auto-props/events (CS8618) are annotated `T?` when the
    value is genuinely nullable, or set to `= null!` when an initialization-order or call-site
    invariant guarantees non-null at every use (behavior-preserving). Own `return null` reference
    returns become the corresponding `T?`.
  - Annotations must be consistent with the already-annotated upstream contracts consumed from
    the three declared Wave-0 clusters. In particular:
    - **Threading `TimeOutTask.RunWithTimeout<...>` returns non-nullable `Task<TResult>`** and
      must stay so (pinned by #369). Consequence for `OneDriveDownloader`:
      `var response = await ClientGetAsync.RunWithTimeout(...)` and the `factory.RunWithTimeout(...)`
      result are non-null — annotate no null handling around `response.IsSuccessStatusCode` or the
      returned stream beyond the file's own existing guards.
    - **Extensions `StreamExtensions.TryCopyToAsyncWithTimeout` returns `Task<bool>`** (value
      type, non-null) — no nullability concern; the existing `?.Dispose()` in `DownloadFileAsync`
      is unaffected. This is the Extensions edge (not the Threading edge) on `OneDriveDownloader`.
    - **Extensions `IsNullOrEmpty(this string?)`** is the annotated nullable-receiver contract;
      safe to call on nullable strings. On net481 it does NOT act as a `[NotNullWhen(false)]`
      refinement, so a value proven non-null by such a guard remains maybe-null to flow analysis;
      resolve with a justified `!` at the guaranteed-non-null site (for example
      `FolderPredictorEvaluator` `trueLeaf!` / `example!`), never a new guard.
    - **HelperClasses** members consumed (`SegmentStopWatch`, `TimedBatchAction`, `FileSystem`
      types) — treat their #364-annotated signatures as authoritative; the residual call sites
      store/pass them without relying on a nullable-return refinement.
  - COM interop types (`Application`/`MailItem`/`Store`/`MAPIFolder`/`Folder`/`Recipient`/
    `AddressEntry`/`PropertyAccessor` etc.) are nullable-oblivious on net481; dereferencing them
    emits no CS8602. The COM-heavy files therefore need no `!` on COM member chains and no new
    runtime guards; existing guards (for example `store?.GetDefaultFolder`,
    `_app.Session?.DefaultStore?…`) already handle the null paths and MUST be preserved as-is.
  - Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`,
    `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) are NOT
    available or polyfilled on net481 and must not be used or added. Zero CS86xx is reachable with
    `?`, `= null!`, and justified `!` only. `[ExcludeFromCodeCoverage]` (present in several
    in-scope files) IS available on net481 and must not be read as evidence that the post-condition
    attributes are available.
  - No `record`/`record struct`/`init` is introduced (`init`/positional `record`/`record struct`
    fail CS0518 on net481 — no `IsExternalInit` polyfill). `IntelligenceConfig.ResourceTimingRow`
    is already a plain constructor-initialized `readonly struct` chosen specifically to avoid
    CS0518 and is left as-is.

## Data & State

- Data transformations and invariants: none changed. This is annotation-only; no runtime data
  flow, transform, or invariant is altered. Existing guard patterns (`store?.`, `?? ""`,
  `Links ??=`, `?.Dispose()`, `is null` checks) remain unchanged.
- Caching or persistence details: none changed.
- Migration or backfill requirements (if any): none. No project-level `<Nullable>` element is
  introduced into `UtilitiesCS.csproj`; the project has no `<Nullable>` element today and must
  keep none. Enforcement is per-file pragma only.

## Constraints & Risks

The following mechanics flags are carried in substance from the research findings and govern
execution:

1. **Pragma-only verification command (do NOT use `/p:Nullable=enable`).** Local and CI
   verification of the opted-in files must use the pragma-only build
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true`, relying on each file's own `#nullable enable` pragma. It must
   NOT add `/p:Nullable=enable`, which would enable nullable project-wide and surface the whole
   epic's ~2131 CS86xx diagnostics across ~234 files as false failures unrelated to issue #375.
   This is a deliberate, documented deviation from the stock CLAUDE.md / `.claude/rules/csharp.md`
   type-check command for this child only; it must NOT be resolved by editing `.claude/rules/*`.
2. **net481 BCL-oblivious profile shapes the fixes.** The debt is dominated by CS8618, CS8625,
   CS8603/CS8600, and self-induced CS8602. Post-condition attributes are unavailable (item in API
   / CLI Surface); `string.IsNullOrEmpty`/`IsNullOrWhiteSpace` do not refine null-state, so a
   value proven non-null by such a guard requires a justified `!` at the guaranteed-non-null site
   rather than a post-condition attribute or a new guard.
3. **Designer-file pairs stay non-opted-in; no combined-batch requirement.** The six
   `*.Designer.cs` files under `OlFolderTools` are the generated halves of WinForms partial-class
   pairs. Because `#nullable enable` is lexical/per-file and only the hand-written half is ever
   opted in, the oblivious Designer half emits no CS86xx and does not cross-block the opted-in
   hand-written half. Designer-declared controls referenced from the hand-written partial are
   treated as oblivious and need no `?`. No two-hand-written-file partial type exists among the 44,
   so no combined-batch requirement applies.
4. **COM/Outlook interop is compile-time-verifiable only.** COM types are oblivious on net481 and
   require no `!` or new guards; verification is the compile-time `msbuild /t:Rebuild` gate — no
   live Outlook process is required for the nullable gate.
5. **Prefer annotation plus justified `!` over new runtime guard statements.** New
   `if (x is null) throw` statements are executable lines that would require new test coverage and
   could constitute a behavior change. Existing guards stay as-is.
6. **Three pre-existing >500-line files inside the 44 are FLAGGED, not fixed** (see Maintainer
   Decisions and Flags item 6). Splitting a file is a refactor and out of scope.

## Implementation Strategy

- Implementation scope (what changes, not sequencing): add a `#nullable enable` pragma to each
  in-scope hand-written file and bring each to zero CS86xx under the pragma-only build. No new
  types, methods, commands, or files are added; only nullability annotations on existing members
  change. No project or solution file changes.
- New classes/functions/commands to add or update: none.
- Batch grouping (from research; leaf-first, directory-cohesive, annotation-scope only; Designer
  files never opted in; the three 500-line-breach files placed last within their batches with the
  breach FLAG recorded):
  - **Batch 0 — verify-only / expect zero CS86xx:** `IntelligenceFilters.cs`,
    `EvaluationResult.cs`, `MAPIFields.cs`, `FolderConverter.cs` (EmailIntel),
    `StringManipulation.cs`, `IFilterOlFoldersViewer.cs`, `IFolderRemapViewer.cs`,
    `SmithWaterman.cs` (verify), `OSFolder.cs`.
  - **Batch 1 — small static COM helpers (return-nullability):** `Calendar.cs`,
    `CreateCategory.cs`, `ComType.cs`, `ExplorerActions.cs`, `MailResolution.cs` (root).
  - **Batch 2 — Outlook readiness pair (co-annotate interface + impl):** `IOutlookReadinessGate.cs`
    + `OutlookReadinessGate.cs` (`Store? store` on both).
  - **Batch 3 — Recipient cluster (co-annotate):** `RecipientInfo.cs` + `RecipientStatic.cs`
    (773 lines — 500-line FLAG). Keep `RecipientInfo` field-nullability consistent with #371's
    ItemInfo/EmailDetails pattern.
  - **Batch 4 — OneDrive (Extensions + Threading edges):** `AngleSharpParsedEmailBody.cs`,
    `OneDriveDownloader.cs`.
  - **Batch 5 — EmailIntelligence data types:** `FilterEntry.cs`, `IntelligenceConfig.cs`,
    `FolderPredictorEvaluator.cs`, `PeopleScoDictionaryNew.cs`.
  - **Batch 6 — OlFolderTools FilterOlFolders (hand-partials; Designer halves oblivious):**
    `FolderInfoViewer.cs`, `OSBrowser.cs`, `FilterOlFoldersViewer.cs`,
    `FilterOlFoldersController.cs`.
  - **Batch 7 — OlFolderTools FolderRemap (hand-partials; Designer halves oblivious):**
    `FolderSelector.cs`, `FolderRemapViewer.cs`, `FolderRemapTree.cs`, `FolderRemapController.cs`.
  - **Batch 8 — large COM helpers (500-line FLAG batch):** `UserDefinedFields.cs` (722),
    `MeetingItemHelper.cs` (847).
  - **To Depricate (own batch, pending Maintainer Decision item 3):** `FileIO2.cs`,
    `StringManipulation.cs`.
  - **Pending Maintainer Decisions (not in any execution batch by default):**
    `Examples/MSDemoConv.cs` (item 2), `People/PeopleScoDictionaryNewBackup.cs` (item 1).
  - Full task-by-task sequencing within each batch belongs to the atomic plan, not this spec.
- Dependency changes (new/removed packages) and rationale: none.
- Logging/telemetry additions and locations: none.
- Rollout plan (feature flags, staged deploys, fallback path): not applicable. Each remediated
  batch is independently mergeable because non-opted-in files remain null-oblivious and are not
  cross-blocking under the per-file pragma architecture.

## Maintainer Decisions and Flags

The research surfaced the following items that are scope/ownership decisions, not technical
blockers. They are recorded here explicitly and are NOT silently resolved by this feature.

1. **Effective compiled opt-in set is 37 files, not 44 — `PeopleScoDictionaryNewBackup.cs` is a
   DEAD, uncompiled duplicate.** Both `PeopleScoDictionaryNewBackup.cs` and the live
   `PeopleScoDictionaryNew.cs` declare a non-partial `class PeopleScoDictionaryNew` in namespace
   `ToDoModel.Data_Model.People`; two non-partial classes of one fully-qualified name cannot
   co-compile (CS0101). The old-style `UtilitiesCS.csproj` lists only the live file in its
   `<Compile Include>` set; `PeopleScoDictionaryNewBackup.cs` is not in the compile set, so a
   pragma on it is a no-op that cannot emit CS86xx. **MAINTAINER DECISION required: exclude from
   the child's opt-in set or delete the dead file.** The effective compiled hand-written opt-in
   set is 37, not 38, once this file is excluded.
2. **`Examples/MSDemoConv.cs` (Examples) — annotate-only vs exclude vs delete.** Production-compiled
   sample code (namespace `UtilitiesCS.Examples`) with an unguarded `mailItem.Parent as
   Outlook.Folder` then `folder.Store` dereference and repeated `... as Outlook.Folder` then
   `.Name`. Annotation-only remediation is feasible (`Outlook.Folder?` locals plus justified `!`
   at the demo's own derefs). **Default: remediate annotation-only.** The alternatives (exclude via
   `[ExcludeFromCodeCoverage]`/pragma omission, or delete) are surfaced for a maintainer decision.
3. **`To Depricate/FileIO2.cs` and `To Depricate/StringManipulation.cs` — deprecation-marked.**
   Real production helpers explicitly named for future deprecation. Annotation-only is feasible
   (`FileIO2` needs `string[]?` returns plus `!`; `StringManipulation` is already clean). This
   child's scope is annotate-only; the maintainer may prefer deletion or scheduled deletion instead
   of spending annotation effort on deprecation-marked code. **Flag; do not delete within this
   child.**
4. **`OutlookObjects/MailResolution.cs` — class `MailResolution_ToRemove`.** The `_ToRemove` suffix
   signals a deletion candidate. Annotation-only remediation is trivial (`MailItem?` return plus
   `MailItem? OlMail = null`). **Default: remediate in place under this child; flag the type as a
   deletion candidate for the maintainer; do not delete it here.**
5. **Undeclared dependency edge on `ReusableTypeClasses` (#366).** Six in-scope files
   (`DASLFilterParser.cs`, `IntelligenceConfig.cs`, `FolderRemapTree.cs`,
   `FilterOlFoldersController.cs`, `FolderRemapController.cs`, `PeopleScoDictionaryNew.cs`) consume
   #366 types (`TreeNode<T>`, `SmartSerializableLoader`, `ScoDictionaryNew<,>`). #366 is Wave 0
   (prepared) but is NOT in this child's `depends_on: [extensions, helperclasses, threading]`.
   Harmless for ordering (Wave 0 precedes Wave 1), but **flagged for the epic-planner** to add the
   edge or confirm the consumed members are annotated null-neutral — the same treatment
   `dialogs-misc` gave its flagged, undeclared Threading edge. Latent, sibling-oblivious edges also
   exist on #365/#371/#372/#374 and external `ToDoModel`/`Tags`.
6. **Three pre-existing >500-line files inside the 44 that annotation will NOT fix.**
   `OutlookObjects/AppointmentItem/MeetingItemHelper.cs` (847),
   `OutlookObjects/Recipient/RecipientStatic.cs` (773), and
   `OutlookObjects/Fields/UserDefinedFields.cs` (722) already exceed 500 lines before any edit.
   Splitting a file is a refactor and out of scope. **FLAG as pre-existing; do NOT split** — the
   same precedent Wave-0 threading (#369) applied to `TimeOutTask.cs` (975 lines). The "no file
   exceeds 500 lines AS A RESULT OF edits" constraint is satisfied: adding a pragma keeps these
   three at 848/774/723 (already over, not newly breached), annotation-in-place edits (`?`,
   `= null!`, `!`) plus csharpier reflow do not push them further in a way that changes their
   already-breached status, and no other in-scope file is newly pushed past 500 (next largest
   hand-written file is `SmithWaterman.cs` at 376).

## Acceptance Criteria

- [ ] AC1: Every compiled in-scope hand-written file carries a `#nullable enable` pragma and
  compiles with zero nullable (CS86xx) diagnostics under the pragma-only build
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
  /p:TreatWarningsAsErrors=true`.
- [ ] AC2: No project-level or solution-level `<Nullable>` element is introduced;
  `UtilitiesCS.csproj` retains none. Verification uses the pragma-only command with no global
  `/p:Nullable=enable`.
- [ ] AC3: The 6 `*.Designer.cs` files under `OlFolderTools` are left oblivious (no pragma) and
  are not cross-blocked; hand-written partial halves annotate only their own declared fields, never
  Designer-declared controls.
- [ ] AC4: No behavior change — no new types, no post-condition attributes, no `record`/`record
  struct`/`init`, existing guards preserved, and no new runtime guard beyond what reaching zero
  CS86xx strictly requires.
- [ ] AC5: Annotations are consistent with the upstream extensions/helperclasses/threading
  annotated signatures — in particular `TimeOutTask.RunWithTimeout` returns non-null
  `Task<TResult>`, `StreamExtensions.TryCopyToAsyncWithTimeout` returns `Task<bool>`, and
  `IsNullOrEmpty(this string?)` is treated as non-refining on net481.
- [ ] AC6: A clean baseline `vstest.console.exe` test run for `UtilitiesCS.Test` (pass/fail counts
  and coverage) is captured before edits, per the evidence-and-timestamp-conventions skill; after
  remediation there are no test regressions and no coverage regression on changed lines attributable
  to this child.
- [ ] AC7: The six Maintainer Decisions and Flags above are recorded in this spec and not silently
  resolved.
- [ ] AC8: No in-scope file exceeds 500 lines as a result of edits; the three pre-existing
  >500-line files (`MeetingItemHelper.cs`, `RecipientStatic.cs`, `UserDefinedFields.cs`) are
  flagged, not split, and are not worsened past their pre-existing breach in a status-changing way.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format → lint → type-check → test)

## Seeded Test Conditions (from potential)

- [ ] Existing MSTest suites for the touched areas (Recipient, `OutlookReadinessGate` #207,
  `IntelligenceConfig` #207, `FolderPredictorEvaluator`, and the OlFolderTools controllers, which
  already have injectable-viewer seams) continue to pass with no behavior change.
- [ ] Changed-line coverage does not regress relative to baseline (prefer annotation and justified
  `!` over new runtime guards to avoid introducing uncovered executable lines).
- [ ] The pragma-driven nullable gate produces zero CS86xx diagnostics for the compiled opted-in
  files without passing `/p:Nullable=enable` globally.
- [ ] A baseline `vstest.console.exe` run (pass/fail counts and coverage percentage) for
  `UtilitiesCS.Test` is captured before any edit, per the evidence-and-timestamp-conventions skill,
  so any regression during remediation is attributable to an annotation change.
- [ ] After each batch, the same test assembly is rerun and pass/fail counts and per-file
  changed-line coverage are diffed against the baseline — no new failures, no coverage regression
  on the lines touched by that batch.
- [ ] COM/VSTO-bound members covered by the repo's documented COM/VSTO exemption gain no new
  executable guard lines that would create newly-uncovered branches.

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

Do NOT pass `/p:Nullable=enable` globally for this feature's verification. The global flag forces
nullable project-wide and surfaces the full pre-existing debt across the solution, drowning this
child's signal. This is the same rules-versus-convention conflict the Wave-0 specs and the epic
manifest flag for the maintainer and defer to the Wave-2 CI capstone child; resolving it is out of
scope here.
