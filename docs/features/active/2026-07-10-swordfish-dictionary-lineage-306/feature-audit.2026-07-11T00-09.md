# Feature Audit — swordfish-dictionary-lineage (F1, Issue #306)

- Timestamp: 2026-07-11T00-09
- Work Mode: full-feature (AC sources: spec.md and user-story.md)
- Branch: feature/swordfish-dictionary-lineage-306 (HEAD a0073fc1)
- Base: origin/epic/swordfish-removal-integration
- Reviewer: feature-review agent (review-only)

## Scope and Baseline

Audit scope is the full branch diff against `origin/epic/swordfish-removal-integration`
(27 source/test files, plus evidence and feature docs). Baseline and post-change
coverage evidence are the executor-produced artifacts under
`evidence/baseline/baseline-tests-coverage.md` and
`evidence/qa-gates/final-tests-coverage.md`, with the delta in
`evidence/qa-gates/coverage-delta.md`. No caller narrowing of scope was applied or
attempted (see policy-audit `## Rejected Scope Narrowing`).

## Acceptance Criteria Inventory

spec.md `## Acceptance Criteria`: 9 items (AC-S1..AC-S9; AC-S9 is the optional legacy deletion).
user-story.md `## Acceptance Criteria`: 7 items (AC-U1..AC-U7; AC-U7 is the optional legacy deletion).
Both files also carry seeded/definition-of-done checklists that map onto the same delivery.

## Acceptance Criteria Evaluation

### spec.md

| ID | Criterion (abbrev) | Verdict | Evidence |
|---|---|---|---|
| AC-S1 | All production consumers re-pointed to `ScoDictionaryNew` (AppToDoObjects, SubjectMapEncoder, FolderScorer) | PASS | AppToDoObjects.cs / SubjectMapEncoder.cs / FolderScorer.cs diffs; P8-T2 census confirms no live production `ScoDictionary<` reference remains. |
| AC-S2 | `IToDoObjects` contract change compiles across modules incl. ripple consumers and `ISubjectMapEncoder.Encoder` | PASS | IToDoObjects.cs / ISubjectMapEncoder.cs / IEmailDetailsWrapper.cs / EmailDetails.cs / EmailDetailsWrapper.cs diffs; final-analyzers.md EXIT 0 (full solution builds). |
| AC-S3 | Per-persisted-dictionary on-disk round-trip test (DictRemap, FilteredFolderScraping, FolderRemap, Encoder) via read seam, entry fidelity, no `$type`/`$id`/`CoDictionary`/`RemainingObject` | PASS | ScoDictionaryNew_OnDiskCompatibility_Tests.cs (4 per-type tests + all-types default-path test); in-memory `DeserializeObject`/`SerializeToString`, no temp files. |
| AC-S4 | In-memory-only fields (Decoder, FolderScorer scores) pure type swap, no on-disk test | PASS | SubjectMapEncoder `_decoder` and FolderScorer `_folderNameScores` swapped; no filename/serialize introduced. |
| AC-S5 | Globals-converter path not used by any persisted dictionary | PASS | policy-audit section 4; grep shows no `GetSettingsJson`/`ScoDictionaryConverter`/`PreserveReferencesHandling`; DefaultWritePath test asserts wrapper-token absence. |
| AC-S6 | SubjectMapEncoder construction/persistence reconciliation (Static.Deserialize; no-arg Deserialize/ToDictionary rewritten; missing-file and duplicate-key paths preserved) | PASS | SubjectMapEncoder.cs:22,40-44,96-99,127-135; duplicate-key rebuild catch preserved. |
| AC-S7 | Affected tests migrated (incl. EmailDetailsTests, EmailDetailsWrapperTests) | PASS | 14 test files migrated to the new lineage; final-tests-coverage.md 4524/4524 pass. |
| AC-S8 | `PeopleScoDictionary.cs` confirmed inert, no F1 change | PASS | evidence/other/peoplescodictionary-inert-confirmation.md; file not in diff. |
| AC-S9 | Optional legacy `SCODictionary.cs` deletion with SmartSerializable substitution | NOT PURSUED (optional; correctly left unchecked) | Plan P8-T3 explicitly skipped deletion per spec Non-Goals; criterion is not applicable and remains `[ ]`. |
| AC-S(final) | Full C# toolchain green in single final pass; coverage holds for changed/new code | PASS | evidence/qa-gates final-csharpier/analyzers/nullable/tests-coverage (all EXIT 0); coverage-delta.md (no regression; new-code 100%). |

### user-story.md

| ID | Criterion (abbrev) | Verdict | Evidence |
|---|---|---|---|
| AC-U1 | All production consumers re-pointed | PASS | maps to AC-S1. |
| AC-U2 | `IToDoObjects` members use new lineage; implementers/callers incl. ripple + Encoder return compile | PASS | maps to AC-S2. |
| AC-U3 | On-disk JSON compatibility preserved for the four persisted dictionaries with per-dictionary round-trip test | PASS | maps to AC-S3. |
| AC-U4 | Two in-memory-only fields pure type swap, no on-disk test | PASS | maps to AC-S4. |
| AC-U5 | Globals-converter path constraint respected | PASS | maps to AC-S5. |
| AC-U6 | Affected tests migrated; toolchain green; coverage holds for changed/new code | PASS | maps to AC-S7 + final toolchain. |
| AC-U7 | Optional legacy deletion | NOT PURSUED (optional; correctly left unchecked) | maps to AC-S9. |

## Acceptance Criteria Check-off

All non-optional acceptance criteria in both spec.md and user-story.md were already
checked `[x]` by the executor and are confirmed PASS by this audit; no additional
check-off edits were required. The optional deletion criteria (spec AC-S9,
user-story AC-U7) are correctly left `[ ]` because deletion was not elected (spec
Non-Goals mark it optional). No phantom criteria were added.

## Focus-Point Confirmation (caller-requested)

1. On-disk JSON compatibility: CONFIRMED. The four persisted dictionaries use only
   `Static.Deserialize` / plain `Serialize()`; no globals converter path. The added
   round-trip tests assert flat payloads with no `$type`/`$id`/`CoDictionary`/`RemainingObject`.
2. Out-of-scope-lock edits: EVALUATED, all non-blocking. `.Remove` -> `.TryRemove`
   (two controllers) is compile-forced by the new concurrent API and behavior-preserving,
   with existing test coverage. The `FolderScorer` ordinal tie-break restores
   determinism lost with the ConcurrentDictionary-backed type and is asserted by two
   equal-score array-load tests. The `SortEmail` async->sync serialize (on the
   scope-lock, task P3-T5) is mechanically necessary and spec-consistent (Low note).
3. Test policy: CONFIRMED. MSTest + Moq + FluentAssertions, no temp files, no
   Thread.Sleep/Task.Delay, deterministic.
4. Coverage: CONFIRMED. No regression on changed lines; new production lines 100%
   covered; per-file scope-lock coverage holds or improves. Repo-wide raw figure is
   a pre-existing, unchanged, exemption-governed condition (non-blocking).

## Summary

The feature satisfies all binding acceptance criteria in both spec.md and
user-story.md. The two optional legacy-deletion criteria are legitimately not
pursued. The three caller-flagged incidental edits are correct, necessary, and
adequately tested. Toolchain is green (4524/4524 tests) and coverage regresses
nothing while adding new-code coverage. No remediation is required, so no
remediation-inputs artifact is produced.

Artifact verdicts:
- policy-audit: PASS (one non-blocking coverage disposition)
- code-review: PASS (two Low advisories, non-blocking)
- feature-audit: PASS

BLOCKING_COUNT: 0

### Acceptance Criteria Status
- Source: spec.md — Total AC items: 9; Checked off (delivered): 8; Remaining (unchecked): 1 (AC-S9 optional legacy deletion, not elected)
- Source: user-story.md — Total AC items: 7; Checked off (delivered): 6; Remaining (unchecked): 1 (AC-U7 optional legacy deletion, not elected)
- Items remaining: optional `SCODictionary.cs` deletion (both files) — intentionally deferred per spec Non-Goals; not a blocking gap.
