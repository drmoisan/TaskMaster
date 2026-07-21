---
epic: utilitiescs-nullable-remediation
integration_branch: epic/utilitiescs-nullable-remediation-integration
created_at: 2026-07-18T21-30
intent:
  epic_type: enabler
  business_outcome_hypothesis: >-
    The CI nullable gate (repaired by PR #361 to use /t:Rebuild) can be genuinely enforced
    without permanently blocking future PRs, by remediating the pre-existing CS86xx nullable
    debt under a per-file #nullable enable opt-in architecture.
  leading_indicators:
    - The nullable-gate build passes on the integration branch under per-file pragma enforcement.
    - Each child feature merges independently without cross-blocking non-opted-in files.
  nfrs:
    - No behavior changes; null-annotation and null-safety remediation only.
    - No reduction in coverage on changed lines.
features:
  - issue_num: 363
    feature_folder: utilitiescs-nullable-extensions
    depends_on: []
  - issue_num: 364
    feature_folder: utilitiescs-nullable-helperclasses
    depends_on: []
  - issue_num: 366
    feature_folder: utilitiescs-nullable-reusabletypes
    depends_on: []
  - issue_num: 367
    feature_folder: utilitiescs-nullable-newtonsofthelpers
    depends_on: []
  - issue_num: 369
    feature_folder: utilitiescs-nullable-threading
    depends_on: []
  - issue_num: 368
    feature_folder: utilitiescs-nullable-svgcontrol
    depends_on: []
  - issue_num: 365
    feature_folder: utilitiescs-nullable-outlook-folder-store
    depends_on: [utilitiescs-nullable-extensions, utilitiescs-nullable-helperclasses]
  - issue_num: 371
    feature_folder: utilitiescs-nullable-outlook-mailitem-item
    depends_on: [utilitiescs-nullable-extensions, utilitiescs-nullable-helperclasses]
  - issue_num: 370
    feature_folder: utilitiescs-nullable-email-parsing
    depends_on: [utilitiescs-nullable-extensions]
  - issue_num: 372
    feature_folder: utilitiescs-nullable-email-classifier
    depends_on: [utilitiescs-nullable-extensions]
  - issue_num: 374
    feature_folder: utilitiescs-nullable-dialogs-misc
    depends_on: [utilitiescs-nullable-extensions, utilitiescs-nullable-helperclasses]
  - issue_num: 375
    feature_folder: utilitiescs-nullable-residuals
    depends_on:
      - utilitiescs-nullable-extensions
      - utilitiescs-nullable-helperclasses
      - utilitiescs-nullable-threading
  - issue_num: 376
    feature_folder: utilitiescs-nullable-ci-capstone
    depends_on:
      - utilitiescs-nullable-extensions
      - utilitiescs-nullable-helperclasses
      - utilitiescs-nullable-reusabletypes
      - utilitiescs-nullable-newtonsofthelpers
      - utilitiescs-nullable-threading
      - utilitiescs-nullable-svgcontrol
      - utilitiescs-nullable-outlook-folder-store
      - utilitiescs-nullable-outlook-mailitem-item
      - utilitiescs-nullable-email-parsing
      - utilitiescs-nullable-email-classifier
      - utilitiescs-nullable-dialogs-misc
      - utilitiescs-nullable-residuals
---

# Epic: UtilitiesCS Nullable-Reference-Type Remediation

> **Issue numbers (all back-filled).** The `issue_num` values were provisional placeholders
> (`9001`-`9013`) at manifest-authoring time, before child promotion. As of planning completion
> every placeholder has been back-filled with the real GitHub issue number from its child's
> promotion receipt: `363`, `364`, `366`, `367`, `369`, `368`, `365`, `371`, `370`, `372`, `374`,
> `375`, and `376` (capstone). No placeholders remain. `depends_on` edges are expressed as
> `feature_folder` basenames (a form the `epic-orchestrate` schema resolves via the
> `feature_folder` index), so the dependency graph is valid independent of `issue_num`.

## Goal

Remediate the pre-existing nullable-reference-type debt (~2131 CS86xx diagnostics across ~234
files) that the CI nullable gate was silently failing to catch, so that the repaired gate can be
genuinely enforced going forward without permanently blocking future PRs. This is null-annotation
and null-safety remediation only; no behavior changes.

## Base Branch and Relationship to PR #361

This epic is premised on the nullable-gate repair delivered by **PR #361**
(`fix/ci-nullable-gate-masking`), which changes the CI step to `msbuild /t:Rebuild ...
/p:Nullable=enable /p:TreatWarningsAsErrors=true` so the gate performs a genuine recompile instead
of a silently-skipped incremental build. At epic-planning time **PR #361 is OPEN, not yet merged
to `main`**. The nullable-gate fix therefore does not exist on `origin/main`.

Consequently, the integration branch `epic/utilitiescs-nullable-remediation-integration` is based
on the PR #361 head commit `20d163ac` (which is `origin/main` plus exactly the one gate-repair
commit), **not** on `origin/main`. This ensures every child feature is prepared and later executed
against the real, repaired gate. If PR #361 merges to `main` before the epic's final
integration-to-`main` PR, that merge is clean because `20d163ac` is a direct descendant of
`origin/main`. This is a deliberate, documented deviation from the default "branch off
`origin/main`" rule, required by the epic's premise.

## Scope

- All CS86xx nullable-diagnostic remediation across `UtilitiesCS/` (~40 subdirectories), grouped
  into cohesive, independently mergeable clusters.
- `SVGControl/` (a separate `net481` WinForms control project) is included because the current
  solution-level nullable gate covers it too; it is remediated and opted in on the same per-file
  basis.
- Finalization of the CI nullable-gate enforcement mechanism to match the per-file opt-in
  architecture (the capstone child).

## Non-Goals

- No behavior changes, refactors, or feature work. Null-annotation and null-safety only.
- No global force-enable of nullable at the project or solution level as the enforcement
  mechanism (see Shared Design). A project-level `<Nullable>enable</Nullable>` flip is considered
  only as an optional, separately-gated capstone step once every file is opted in and clean.
- No editing of `.claude/rules/*` (policy prohibits it); the rules-vs-convention conflict is
  flagged for the maintainer rather than resolved by editing policy.

## Shared Design

### Per-file `#nullable enable` opt-in (confirmed architecture)

`UtilitiesCS.csproj` has no `<Nullable>` element (nullable OFF at project level). The organic
repository convention is per-file opt-in: a subset of `.cs` files already carry `#nullable enable`
pragmas. The confirmed architecture continues that convention:

- Each file that is remediated receives a `#nullable enable` pragma and is brought to zero CS86xx
  diagnostics under that pragma.
- Files that are not yet remediated remain non-opted-in and are not cross-blocking.
- This makes each child feature independently mergeable and CI-passable. A global force-enable
  would make no child mergeable until all ~234 files were fixed simultaneously.

### CI gate enforcement change (capstone)

Because enforcement moves to per-file pragmas, the gate must stop overriding `-p:Nullable=enable`
globally at the solution level. The capstone child revises the PR #361 gate step to rely on each
file's own pragma under `/t:Rebuild /p:TreatWarningsAsErrors=true`, so opted-in files are
enforced while non-opted-in files are not cross-blocking. The capstone verifies genuine
enforcement (a deliberately-introduced null defect in an opted-in file fails the gate) and flags
the rules-vs-convention conflict for the maintainer.

### Rules-vs-convention conflict (flagged, not resolved)

`.claude/rules/csharp.md` documents the toolchain as forcing `/p:Nullable=enable` globally, which
conflicts with the codebase's per-file opt-in convention. Policy prohibits editing
`.claude/rules/*`. This conflict is FLAGGED for the maintainer (consistent with the
coverage-threshold conflict-handling precedent), not silently resolved.

## Decomposition and Waves

Clusters follow `UtilitiesCS/` subdirectory cohesion. Shared, cross-module clusters
(Extensions, HelperClasses, ReusableTypes, NewtonsoftHelpers) are remediated first so that
downstream Outlook/Email/Dialogs clusters consume already-annotated contracts. Dependency edges
reflect real annotation-contract consumption only.

Wave assignment uses longest-path layering (`wave = 0` when `depends_on` is empty, else
`1 + max(wave(dep))`):

### Wave 0 (no dependencies)

| feature_folder | cluster | est. files | complexity |
| --- | --- | --- | --- |
| utilitiescs-nullable-extensions | `Extensions/` | ~12 | C3 |
| utilitiescs-nullable-helperclasses | `HelperClasses/` (incl. FileSystem, ThemeHelpers, root) | ~15 | C3 |
| utilitiescs-nullable-reusabletypes | `ReusableTypeClasses/` (incl. TimedActions, NewSmartSerializable) | ~12 | C3 |
| utilitiescs-nullable-newtonsofthelpers | `NewtonsoftHelpers/` | ~14 | C3 |
| utilitiescs-nullable-threading | `Threading/` | ~15 | C3 |
| utilitiescs-nullable-svgcontrol | `SVGControl/` (separate net481 WinForms project) | ~14 | C2 |

### Wave 1 (consume wave-0 shared annotations)

| feature_folder | cluster | depends_on | est. files | complexity |
| --- | --- | --- | --- | --- |
| utilitiescs-nullable-outlook-folder-store | `OutlookObjects/Folder` + `OutlookObjects/Store` | extensions, helperclasses | ~29 | C2 |
| utilitiescs-nullable-outlook-mailitem-item | `OutlookObjects/MailItem` + Item + Conversation + Attachment + Table | extensions, helperclasses | ~25 | C2 |
| utilitiescs-nullable-email-parsing | `EmailIntelligence/EmailParsingSorting` + SubjectMap + Ctf | extensions | ~18 | C2 |
| utilitiescs-nullable-email-classifier | `EmailIntelligence/Bayesian` + ClassifierGroups + Flags | extensions | ~18 | C3 |
| utilitiescs-nullable-dialogs-misc | `Dialogs/` (12 targets + 2 verify-only misc: `WindowsAPI/ExtraDeclarations.cs`, `Properties/AssemblyInfo.cs`) | extensions, helperclasses | 14 | C2 |
| utilitiescs-nullable-residuals | Residual unowned CS86xx-risk trees: `Examples/` + `To Depricate/` + `OneDriveHelpers/` + `OutlookObjects/` root+8 leaf dirs + `EmailIntelligence/` root+Evaluation+OlFolderTools+People | extensions, helperclasses, threading | 44 | C3 |

### Wave 2 (capstone)

| feature_folder | cluster | depends_on | est. files | complexity |
| --- | --- | --- | --- | --- |
| utilitiescs-nullable-ci-capstone | CI nullable-gate finalization + rules-conflict flag + optional project-level Nullable | all twelve remediation children | ~3 | C2 |

### Capstone scope addendum (2026-07-19)

The `~3 est. files` figure above was set before any of the twelve Wave-0/Wave-1 children had
merged, when the CI nullable-gate finalization was believed to be a workflow-YAML-only change.
Once the capstone's child orchestrator branched from the fully-fanned-in integration tip
(`bfcdb394`) and re-ran the pragma-driven gate for the first time (`ci.yml` never triggers on the
integration branch, so this measurement had never previously been taken), it found the gate does
not pass cleanly:

- **SVGControl/SvgImageSelector.cs CS0649** (`_relativeImagePath`, `_absoluteImagePath` never
  assigned; `ImagePath` setter body is dead/commented out per #368's already-documented
  judgment call) blocks the `/t:Rebuild` dependency chain outright.
- **UtilitiesCS nullable fan-in debt**: 296 CS86xx-range diagnostics plus 28 CS0618 and 2 CS0168
  across 62 already-`#nullable enable`-opted-in files under `UtilitiesCS/EmailIntelligence/**`
  and `UtilitiesCS/OutlookObjects/Folder/**` — none of the twelve per-cluster-locked children
  could remediate this under their own scope locks, since it is cross-child annotation
  propagation, not any single child's own files.

This was decided directly at the child-orchestrator delegation for issue #376 (this capstone's
own child orchestrator, 2026-07-19 session), not as a pre-existing line item recorded elsewhere
in this manifest: the capstone's scope is expanded from `~3 est. files` to include this build-debt
remediation (annotation-only / narrow-pragma-only, no behavior change) as a mandatory prerequisite
for the CI gate finalization to be genuinely testable at all. The corresponding `spec.md`
"Scope reconciliation (2026-07-19)" section in
`docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/spec.md` records the full rationale and
measurement detail. The `complexity` column above (`C2`) is retained as-is for this addendum;
this capstone's own `orchestrator-state.json` checkpoint records the child orchestrator's
complexity-band assessment for the expanded atomic planning/execution phases.

## Residual-Scope Decision (2026-07-18)

The `dialogs-misc` child (issue #374) was the designated Wave-1 catch-all, but narrowed its
adopted scope to `UtilitiesCS/Dialogs/` (12 remediation targets + 2 verify-only misc files) and
flagged the unowned residual trees in its `spec.md` "Ownership Gaps Flagged for
Epic-Planner / Maintainer". Those residuals were cross-checked against this epic's
definition-of-done inventory (~2131 CS86xx diagnostics across ~234 files / ~40 subdirectories of
`UtilitiesCS/`, the CI error-log distribution). Ownership was reconciled by exhaustive `.cs`
file count against the already-prepared children:

- `OutlookObjects/` (126 `.cs`): Folder(63)+Store(20)=83 owned by `outlook-folder-store` (#365);
  MailItem(12)+Item(9)+Conversation(2)+Attachment(2)+Table(5)=30 owned by `outlook-mailitem-item`
  (#371); **residual = 13** (root 3 + AppointmentItem, Calendar, Category, Com, Explorer, Fields,
  Filter DASL, Recipient).
- `EmailIntelligence/` (100 `.cs`): EmailParsingSorting(14)+SubjectMap(7)+Ctf(4)=25 owned by
  `email-parsing` (#370); Bayesian(27)+ClassifierGroups(16)+Flags(6)=49 owned by `email-classifier`
  (#372); **residual = 26** (root 4 + Evaluation 2 + OlFolderTools 18 + People 2).
- Other unowned trees: `Examples/` (1), `To Depricate/` (2), `OneDriveHelpers/` (2).

### Decision: one additional remediation child

**In-scope residuals exist**, so per the epic's definition-of-done a new remediation child is
added rather than an exclusion note. `utilitiescs-nullable-residuals` (Wave 1,
`issue_num: 375`) owns the **44** residual files with genuine CS86xx risk
(Examples 1 + To Depricate 2 + OneDriveHelpers 2 + OutlookObjects residual 13 +
EmailIntelligence residual 26). Sampled evidence in the `dialogs-misc` spec plus structural
confirmation (uninitialized non-nullable fields, unguarded COM dereferences) show these are
genuine CS8618/CS8602-class candidates in the epic's error-log inventory. The child's
`depends_on: [extensions, helperclasses, threading]` edges are source-confirmed by grep across
the 44 files: 7 files consume `UtilitiesCS.Extensions`, 4 consume `UtilitiesCS.HelperClasses`,
and 1 (`OneDriveHelpers/OneDriveDownloader.cs`) consumes the Threading cluster
(`TimeOutTask.RunWithTimeout` / `TryCopyToAsyncWithTimeout`) — an edge the `dialogs-misc` spec
flagged as undeclared, now declared here. All three dependency clusters are Wave 0, so the child
is Wave 1. Complexity band C3: the residual set spans Outlook COM adapters and EmailIntelligence
modules and folds in the `cross_module_contract_change` floor signal via its multi-cluster
consumption.

### Epic-wide exclusions (recorded, not assigned to any child)

- `UtilitiesCS/Interfaces/**` (~62 `.cs`): near-zero CS86xx risk — pure interface member
  declarations with no bodies, fields, or constructors, so CS8618 cannot fire. Formally excluded
  from every child, extending the established `Interfaces/IHelperClasses/` precedent (already out
  of scope for `helperclasses` #364) repo-wide.
- `UtilitiesCS/Properties/Resources.Designer.cs` and `Settings.Designer.cs` (2 `.cs`): fully
  generated Designer files, left null-oblivious (no pragma). `AssemblyInfo.cs` is already a
  verify-only member of `dialogs-misc` scope.

These exclusions carry no CS86xx debt under the per-file pragma enforcement design; leaving them
non-opted-in does not cross-block and does not diminish the epic's definition of done.

### residuals (#375) execution-time findings (recorded for epic-orchestrator)

The prepared `utilitiescs-nullable-residuals` child (issue #375, plan `plan.2026-07-18T23-13.md`,
13 phases, C3/opus, `require_model_routing` ok, `PREFLIGHT: ALL CLEAR`) refined the residual set
during preparation. epic-orchestrator must carry these findings into atomic execution:

- **Effective opt-in set is 37 files, not the 44 estimate.** `PeopleScoDictionaryNewBackup.cs` is
  a dead, uncompiled duplicate flagged for maintainer exclude/delete (not opted in), and 6
  `OlFolderTools` Designer-generated files are left null-oblivious (no pragma), consistent with the
  epic-wide Designer-file exclusion. The 44-file DoD inventory count is unchanged; the effective
  remediation set is narrowed to 37 by these two maintainer-flagged exclusions.
- **Undeclared-but-harmless dependency edge on `reusabletypes` (#366, Wave 0).** The residuals set
  consumes a `reusabletypes` contract not declared in the manifest `depends_on`. It is harmless:
  `reusabletypes` is Wave 0 and is prepared/fanned in, so the annotated contract is available
  before residuals executes in Wave 1. Flagged, not added as a manifest edge (adding it would not
  change the wave layering; residuals is already Wave 1).
- **Three pre-existing >500-line files flagged, not split.** The residual set contains three files
  exceeding the 500-line general-code-change limit. They are pre-existing and are annotated in
  place without splitting (splitting would be a refactor, out of scope for null-annotation-only
  remediation). Flagged for the maintainer.
- **Maintainer decisions surfaced in `spec.md`:** `MSDemoConv.cs`, the `To Depricate/*` tree, and
  `MailResolution_ToRemove` are surfaced as maintainer decisions (candidate deletions / deprecated
  code) in the residuals `spec.md` rather than silently remediated.

### dialogs-misc (#374) flags retained

- The manifest's `depends_on` edge from `dialogs-misc` to `helperclasses` (#364) is
  grep-unconfirmed by source (zero `HelperClasses/` type references under `Dialogs/`). The edge is
  retained (both Wave-0 upstreams are prepared, so it is harmless) and flagged, not dropped.
- `dialogs-misc`'s atomic plan carries a Phase-0 execution-start gate: its execution must not
  begin until `extensions` (#363) Batch D (`Extensions/WinFormsExtensions.cs`, the `Clone<T>()`
  contract consumed by the button wrappers and `MyBox`) has merged. This gate is enforced at
  atomic-execution time by `epic-orchestrator`, not during planning.

## Complexity Rationale

- **C3 (opus-tier)** clusters alter contracts consumed across module boundaries
  (`cross_module_contract_change` floor signal): Extensions, HelperClasses, ReusableTypes,
  NewtonsoftHelpers. Threading is floored C3 by `concurrency_or_ordering`. Email-classifier is
  floored C3 by `classifier_or_model_logic` (it touches T1 classifier modules), though the change
  is annotation-only with no scoring change.
- **C2 (sonnet-tier)** clusters are file-local null guards/annotations in adapter/UI code
  (Outlook adapters, Email parsing, Dialogs, SVGControl) and the small CI capstone.

Each band is a reviewed starting assessment; each child orchestrator re-runs its own
model-selection step during preparation.
