# utilitiescs-nullable-outlook-mailitem-item (Issue #371)

- Date captured: 2026-07-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/utilitiescs-nullable-outlook-mailitem-item/ (Issue #371)
- Parent: Epic `utilitiescs-nullable-remediation` (Wave 1)
- Depends on: utilitiescs-nullable-extensions (#363), utilitiescs-nullable-helperclasses (#364)

- Issue: #371
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/371
- Last Updated: 2026-07-19
- Work Mode: full-feature

## Problem / Why

The CI nullable gate (repaired by PR #361 to use `msbuild /t:Rebuild`) now performs a genuine
recompile and surfaces pre-existing CS86xx nullable-reference-type diagnostics that were
previously masked. The Outlook item-adapter cluster under `UtilitiesCS/OutlookObjects/` —
`MailItem/`, `Item/`, `Conversation/`, `Attachment/`, and `Table/` (~30 `.cs` files) — carries
such pre-existing nullable debt. This is the Wave-1 child that remediates that cluster only,
consuming the already-annotated cross-module contracts produced by the Wave-0 Extensions (#363)
and HelperClasses (#364) children.

## Proposed Behavior

Remediate the pre-existing nullable debt in the Outlook item-adapter cluster using the epic's
per-file `#nullable enable` opt-in architecture. Each remediated `.cs` file receives a
`#nullable enable` pragma and is brought to zero CS86xx diagnostics under
`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
/p:TreatWarningsAsErrors=true` (pragma-only; do NOT pass `/p:Nullable=enable` globally).
Non-remediated files remain non-opted-in and must not be cross-blocked. This is null-annotation
and null-safety work only: no behavior changes, no refactors, no API redesign.

## Acceptance Criteria (early draft)

- [ ] AC1: Every `.cs` file under `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}`
  that emits CS86xx carries `#nullable enable` and compiles with zero nullable diagnostics under
  the per-file pragma with `/p:TreatWarningsAsErrors=true`.
- [ ] AC2: No project-level or solution-level `<Nullable>` element is introduced; `UtilitiesCS.csproj`
  retains none.
- [ ] AC3: No behavior change; existing MSTest tests for UtilitiesCS still pass.
- [ ] AC4: No coverage regression on changed lines.
- [ ] AC5: Public signatures of remediated members remain behavior-compatible; nullability
  annotations reflect actual null behavior and correctly consume the upstream #363/#364 contracts.
- [ ] AC6: Outlook Interop event-handler classes that directly depend on
  `Microsoft.Office.Interop.Outlook` types without an injectable seam are annotated for null-safety
  but respect the repo COM/VSTO coverage exemption (no new tests forced around COM-bound code).

## Constraints & Risks

- Pragma-only verification (do NOT use `/p:Nullable=enable`); the global flag surfaces the whole
  epic's debt and drowns this child's signal. Rules-vs-convention conflict is deferred to the
  Wave-2 CI capstone child; do not edit `.claude/rules/*`.
- COM-bound Outlook interop adapters are coverage-exempt per repo policy; annotate for null-safety
  without forcing new tests around COM-bound code.
- Nullable post-condition attributes from `System.Diagnostics.CodeAnalysis` are not polyfilled on
  net481 and must not be used or added.
- Partial-class file groups (e.g. `MailItemHelper.*`, `OlTableExtensions.*`,
  `ConversationHelper.*`) must be opted-in together to avoid inconsistent field-null-state analysis.
- Pre-existing >500-line files (if any) are annotation-only; do not split (flag, do not fix).

## Test Conditions to Consider

- [ ] Existing `UtilitiesCS.Test/OutlookObjects/` suite continues to pass with no behavior change.
- [ ] Changed-line coverage does not regress relative to baseline.
- [ ] The pragma-driven nullable gate produces zero CS86xx for the remediated files without
  `/p:Nullable=enable`.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/` folder from the template
