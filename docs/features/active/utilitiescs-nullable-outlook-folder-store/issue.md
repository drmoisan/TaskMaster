# utilitiescs-nullable-outlook-folder-store (Issue #365)

- Date captured: 2026-07-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/utilitiescs-nullable-outlook-folder-store/ (Issue #365)

- Issue: #365
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/365
- Last Updated: 2026-07-19
- Work Mode: full-feature

## Problem / Why

What need or gap does this idea address?

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` so it performs a
genuine recompile instead of a silently-skipped incremental build, cannot be enforced
against new code until the pre-existing nullable-reference-type debt (CS86xx diagnostics)
is remediated under the repository's per-file `#nullable enable` opt-in architecture. This
issue is the Wave-1 child of the `utilitiescs-nullable-remediation` epic covering
`UtilitiesCS/OutlookObjects/Folder/` (including `MsgToMime/`) and
`UtilitiesCS/OutlookObjects/Store/`. Per
`docs/features/active/utilitiescs-nullable-outlook-folder-store/research/2026-07-18T22-30-outlook-folder-store-nullable-research.md`,
the cluster contains 83 `.cs` files: 18 already opted in (verify-only), 2 Designer-generated
files recommended to remain non-opted-in, and 63 opt-in remediation targets — a refined
count that supersedes the epic manifest's stale `~29` estimate. This is null-annotation and
null-safety remediation only; no behavior changes, refactors, API redesign, or new features.

## Proposed Behavior

What should the feature do at a high level?

Each of the 63 opt-in-target files receives a per-file `#nullable enable` pragma and is
brought to zero CS86xx diagnostics under that pragma with `TreatWarningsAsErrors`, applying
`?` annotations, null-flow corrections, and justified `!` operators only, with existing null
guards left as-is. The 18 already-enabled files are verified, not edited. The 2
Designer-generated files (`DisabledStoresViewer.Designer.cs`, `StoreWrapperViewer.Designer.cs`)
remain non-opted-in per repository convention. Partial-class groups
(`FolderPredictor.cs` + `FolderPredictor.IFolderSearchHandler.cs`; `StoresWrapper.cs` +
`StoresWrapper.Filtering.cs`) are remediated together in the same commit so shared members
carry a single, consistent nullable shape. COM/VSTO/WinForms coverage-exempt classes are
annotated for null-safety without new tests or new runtime guard code paths. No project-level
`<Nullable>` element is added to `UtilitiesCS.csproj`, and no `/p:Nullable=enable` global flag
is used in verification — enforcement is per-file pragma only, matching the pattern already
established by the Wave-0 `#363`/`#364` children this feature depends on.

## Acceptance Criteria

- [ ] AC1: Every `.cs` file under `UtilitiesCS/OutlookObjects/Folder/` and
  `UtilitiesCS/OutlookObjects/Store/` that emits CS86xx carries `#nullable enable` and
  compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [ ] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`; no
  `/p:Nullable=enable` global flag is used in verification.
- [ ] AC3: No behavior change; the existing `UtilitiesCS.Test` suite covering this cluster
  still passes.
- [ ] AC4: No coverage regression on changed lines; COM-bound coverage-exempt files are
  annotated without new tests, per the CLAUDE.md coverage exemption.
- [ ] AC5: Public signatures of the remediated Folder and Store types remain
  behavior-compatible; nullability annotations reflect actual null behavior so they are safe
  contracts for downstream epic consumers.
- [ ] AC6: No `System.Diagnostics.CodeAnalysis` nullable post-condition attribute is added,
  and no `record`, `record struct`, or `init` accessor is introduced anywhere in this
  cluster.
- [ ] AC7: Each partial-class group (`FolderPredictor.cs` +
  `FolderPredictor.IFolderSearchHandler.cs`; `StoresWrapper.cs` +
  `StoresWrapper.Filtering.cs`) is remediated in the same commit/batch with a consistent
  nullable shape for shared members.

## Constraints & Risks

List notable constraints (performance, compatibility, scope) or risks.

## Test Conditions to Consider

- [ ] Unit coverage areas
- [ ] Integration scenarios
- [ ] CLI/API examples

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/utilitiescs-nullable-outlook-folder-store/` folder from the template

