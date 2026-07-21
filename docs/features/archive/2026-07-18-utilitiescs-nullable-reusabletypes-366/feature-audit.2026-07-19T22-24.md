# Feature Audit — utilitiescs-nullable-reusabletypes (Issue #366)

- Timestamp: 2026-07-19T22-24
- Reviewer: feature-review agent
- Branch head: 685a7a24
- Review base: 0b000511 (origin/epic/utilitiescs-nullable-remediation-integration)

## Scope and Baseline

Baseline for acceptance verification is the epic integration branch at 0b000511. The audited change
set is `git diff 0b000511..HEAD`: 51 `UtilitiesCS/ReusableTypeClasses/` files receive per-file
`#nullable enable` remediation, three truly-generic dictionary bases receive the ratified additive
`where TKey : notnull` constraint, and four `#367`-owned NewtonsoftHelpers consumers receive that
constraint under the epic-authorized Option A'' four-file waiver. The change is annotation-only.

Work mode is `full-feature`. Per the delegating authorization, issue.md `## Acceptance Criteria`
(AC1–AC6) is the authoritative acceptance list for this review; each is evaluated below.

## Acceptance Criteria Inventory

| ID | Criterion (abbreviated) |
|---|---|
| AC1 | Every `.cs` under `ReusableTypeClasses/` that emits CS86xx carries `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with `TreatWarningsAsErrors`. |
| AC2 | No project-level `<Nullable>` element in `UtilitiesCS.csproj`. |
| AC3 | No behavior change; existing tests still pass. |
| AC4 | No coverage regression on changed lines. |
| AC5 | Public signatures of remediated reusable types remain behavior-compatible; annotations reflect actual null behavior (safe cross-module contracts). |
| AC6 | Non-opted-in files elsewhere are not cross-blocked by this change. |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence |
|---|---|---|
| AC1 | PASS | Isolated-cluster nullable gate: 0 CS86xx / 0 CS8714 attributable to any #366 cluster file (`ReusableTypeClasses/**` + the four waiver files) under `msbuild UtilitiesCS.csproj /t:Rebuild /p:TreatWarningsAsErrors=true` (per-file pragma; no `/p:Nullable=enable`). 51 files carry `#nullable enable` at HEAD (0 at base). evidence/qa-gates/final-nullable-pragma-gate.md, batch-8-nullable-gate.md; independently reconfirmed via diff. |
| AC2 | PASS | `grep -c "<Nullable>" UtilitiesCS/UtilitiesCS.csproj` = 0 (independently reconfirmed). evidence/qa-gates/final-ac2-csproj-check.md. |
| AC3 | PASS | Full suite 5702/5702 pass, 0 fail, 0 skip; pass count unchanged from baseline. Diff scan shows no new executable statement/branch logic; flagged control-flow lines are annotation re-emissions of pre-existing conditionals. evidence/qa-gates/final-tests-coverage.md, final-signature-compat.md. |
| AC4 | PASS | Change is annotation-only (non-executable directives + annotations on pre-existing executable lines); no changed line introduces a new uncovered path. Whole-run line-rate +0.09pp (83.79%→83.88%), branch-rate stable; representative remediated files retain strong per-file coverage. evidence/qa-gates/final-coverage-delta.md, final-coverage.cobertura.xml (independently parsed). |
| AC5 | PASS | Public-signature changes limited to additive nullability annotations, the ratified additive `where TKey : notnull` constraint, and justified `!`. No parameter add/remove/reorder; no return-type semantics change; no `record`/`init`/`record struct` conversion. Annotations reflect actual null behavior for downstream consumers. evidence/qa-gates/final-signature-compat.md, final-scope-guards.md. |
| AC6 | PASS | Enforcement is per-file pragma only; csproj has no `<Nullable>`. Every solution-wide nullable/vendored error originates in a sibling-owned or vendored file, none in a #366 file. Three WinForms files (ConfigViewer.Designer.cs, ConfigViewer.cs, ConfigGroupBox.cs) remain null-oblivious (exemption b) and are not cross-blocked. evidence/qa-gates/final-nullable-pragma-gate.md, final-constraint-and-exemption-check.md. |

## Cross-Child Waiver and Solution-Wide Deviation (verified, not failures)

- The four cross-child `where TKey : notnull` propagations to WrapperScoDictionary.cs,
  ScoDictionaryConverter.cs, WrapperScDictionary.cs, and ScDictionaryConverter.cs are epic-authorized
  (Option A'', ratified 2026-07-19T22:14:30Z), definitively enumerated, and CLOSED at four. Diff
  confirms exactly these four NewtonsoftHelpers files changed, each adding one constraint line.
  Not scope violations; not blocking.
- The solution-wide pragma-gate `EXIT 1` is caused by ~148 pre-existing cross-child CS86xx in
  sibling-owned files plus 2 vendored CS0649; zero #366-owned errors. Expected cross-child-fan-in
  deviation per epic P9-T3; not a #366 failure.

## Summary

All six acceptance criteria (AC1–AC6) evaluate PASS. blocking_count = 0. The feature delivers its
stated objective — per-file `#nullable enable` remediation of the `ReusableTypeClasses/` cluster with
zero nullable diagnostics under the pragma, no project-level nullable, no behavior change, no
changed-line coverage regression, behavior-compatible public signatures, and no cross-blocking of
non-opted-in files. No remediation inputs are required.

### Acceptance Criteria Check-off

All AC1–AC6 items in issue.md are already checked `[x]` and are confirmed by this audit. No source
checkbox changes were required.

### Acceptance Criteria Status
- Source: docs/features/active/2026-07-18-utilitiescs-nullable-reusabletypes-366/issue.md (`## Acceptance Criteria`)
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: none
