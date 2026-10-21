# P6-T46 Acceptance Inventory and Final Hygiene Sweep

Timestamp: 2026-10-06T15-23
Command: (1) CMD-SPEC-CHECK (STAGE final); (2) CMD-EVIDENCE-FIELDS (INHERITED the INHERITED-CLAUSE-A list of P0-T3 minus phase0-instructions-read.md and p0-t2-mode-preconditions.2026-10-03T08-24.md, 25 paths); (3) CMD-SWEEP (the last command of the plan); each run as pwsh -NoProfile -Command with Set-Location to the item worktree
EXIT_CODE: 0 (scoped to the CMD-SWEEP payload, its process exit code)
Output Summary: branch (a) of P6-T7: AC-CHECKED 25, AC6-UNCHECKED 1, AC27-UNCHECKED 1, AC-ANY-UNCHECKED 2 (AC6 and AC27 deferred to the PR step, no other AC unchecked); every evidence artifact of this run carries the three line-leading fields under the three canonical subfolders and all four controls discriminate; every host-identifier and raw-document sweep count is 0 over 89 files. Plan outcome: complete, with AC6 and AC27 deferred to the PR step.

- AC7-READ: HOLDS (P6-T25 Read of UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs lines 28 to 46: the three-line comment beginning `// A property, not an array field` sits directly above `private static YesNoToAllPromptSession[] AllPromptSessions =>`, the array holds exactly AttachmentsOverwritePrompt, PicturesOverwritePrompt, AttachmentsAltNamePrompt and RemoveReadOnlyPrompt, and `Cleanup_Files` is a `foreach` over `AllPromptSessions` calling `Reset()`)

## Acceptance criteria inventory

| AC | State | Check-off task | Evidence artifact(s) |
| --- | --- | --- | --- |
| AC1 | checked | P6-T19 | qa-gates/p6-t10-post-format-census.2026-10-03T12-52.md |
| AC2 | checked | P6-T20 | regression-testing/fail-before-save-case.md; regression-testing/pass-after-regression-tests.md; p6-t10 census |
| AC3 | checked | P6-T21 | p6-t10 census; qa-gates/coverage-comparison.md |
| AC4 | checked | P6-T22 | regression-testing/fail-before-try-save-retry.md; p6-t10 census |
| AC5 | checked | P6-T23 | qa-gates/p6-t16-identity-and-sweep.2026-10-06T15-11.md; pass-after-regression-tests.md; p6-t10 census |
| AC6 | unchecked (DEFERRED TO PR STEP) | P6-T24 (deferral) | qa-gates/p6-t24-ac6-deferred.2026-10-06T15-15.md; regression-testing/fail-before-cleanup-files-phase-one.md; qa-gates/pr-description-inputs.2026-10-06T15-12.md |
| AC7 | checked | P6-T25 | p6-t10 census; AC7-READ above; pass-after-regression-tests.md |
| AC8 | checked | P6-T26 | p6-t10 census; FEATURE/spec.md Grep (Func.Attachment 0 lines, TrySaveAttachmentDelegate 7 lines); coverage-comparison.md; qa-gates/p6-t12-scope-boundary.2026-10-06T13-23.md; p6-t3 and p6-t4 rebuilds; pass-after-regression-tests.md |
| AC9 | checked | P6-T27 | pass-after-regression-tests.md; regression-testing/compile-red-attachment-saving-seams.md; p6-t10 census |
| AC10 | checked | P6-T28 | p6-t10 census; qa-gates/negative-controls.md |
| AC11 | checked | P6-T29 | p6-t10 census; regression-testing/fail-before-redirect-save-folder.md; p6-t12 (ITERATION 2) |
| AC12 | checked | P6-T30 | p6-t12 (ITERATION 2); p6-t10 census; regression-testing/fail-before-exception.2026-10-03T12-33.md entry 3 |
| AC13 | checked | P6-T31 | p6-t10 census (EFCC-MEMBERS); pass-after-regression-tests.md |
| AC14 | checked | P6-T32 | p6-t10 census; pass-after-regression-tests.md; p6-t12 (ITERATION 2) |
| AC15 | checked | P6-T33 | regression-testing/fail-before-write-csv.md; p6-t10 census |
| AC16 | checked | P6-T34 | qa-gates/p5-t11-cr1-spec956.2026-10-03T12-32.md; p6-t12 (ITERATION 2) |
| AC17 | checked | P6-T35 | p6-t10 census (CMD-USINGS, CMD-GREP-FACTS); qa-gates/p6-t3-msbuild-analyzers.2026-10-03T12-36.md; qa-gates/p6-t4-msbuild-nullable.2026-10-03T12-37.md |
| AC18 | checked | P6-T36 | p6-t10 census; regression-testing/fail-before-efc-filer-cleanup.md; p6-t12 (ITERATION 2); pass-after-regression-tests.md |
| AC19 | checked | P6-T37 | p6-t10 census; qa-gates/p5-t10-todomodel-deletion.2026-10-03T12-30.md; dossier entry 5; p6-t3 and p6-t4 |
| AC20 | checked | P6-T38 | the six fail-before artifacts; the plan's `[expect-fail]` tags on P1-T6, P1-T7, P2-T6, P3-T3, P4-T9, P5-T7; dossier (eight entries) |
| AC21 | checked | P6-T39 | p6-t10 census; qa-gates/coverage-post-change.md (branch (a)); pass-after-regression-tests.md |
| AC22 | checked | P6-T40 | p6-t12 (ITERATION 2); p6-t10 CMD-CSPROJ |
| AC23 | checked | P6-T41 | p6-t10 CMD-LINES (MAX-LINES 488, AC23-CLOSEST); qa-gates/p6-t14-doc-comment-format-and-census.2026-10-06T13-22.md (TST1 488 after the doc edit) |
| AC24 | checked | P6-T42 | qa-gates/toolchain-final-pass.md |
| AC25 | checked | P6-T43 | baseline/coverage-baseline.md; qa-gates/coverage-post-change.md; qa-gates/coverage-comparison.md |
| AC26 | checked | P6-T44 | p6-t12 (ITERATION 2); p6-t16; the P6-T44 CMD-EVIDENCE-FIELDS run below; the six fail-before artifacts; compile-red-attachment-saving-seams.md |
| AC27 | unchecked (DEFERRED TO PR STEP) | P6-T45 (deferral) | qa-gates/p6-t45-ac27-deferred.2026-10-06T15-22.md; qa-gates/pr-description-inputs.2026-10-06T15-12.md |

## AC26 check-off evidence fields (P6-T44)

Printed lines of the CMD-EVIDENCE-FIELDS run issued by P6-T44 (2026-10-06, before the AC26 Edit; INHERITED substituted exactly as in P6-T16):

```
EVIDENCE-FILES-CHECKED: 82
EVIDENCE-MISSING-FIELDS: 0
FIELD-CHECK-CONTROL: False
FIELD-CHECK-POSITIVE: True
NONCANONICAL-SUBFOLDER-FILES: 0
SUBFOLDER-CHECK-FLAGS-OTHER: True
SUBFOLDER-CHECK-FLAGS-QA-GATES: False
EVIDENCE-INHERITED-SKIPPED: 1
```

## CMD-SPEC-CHECK (STAGE final)

```
WORKMODE-LINES: 1
AC-HEADING-LINES: 1
AC-UNCHECKED: 2
AC-CHECKED: 25
AC6-UNCHECKED: 1
AC27-UNCHECKED: 1
AC-ANY-UNCHECKED: 2
USERSTORY-EXISTS: False
AC15-SEAM-STEP: 3
AC6-RESET-LITERAL: 2
AC25-NINETY: 2
S956-OLD-99: 0
S956-NEW-99: 1
S956-149: 1
S956-NEW-149: 1
S956-OLD-155: 0
S956-NEW-155: 1
S956-AC-CHECKED: 17
S956-AC-UNCHECKED: 0
S956-LINES: 315
```

## CMD-EVIDENCE-FIELDS (this task's run, the third)

```
EVIDENCE-FILES-CHECKED: 83
EVIDENCE-MISSING-FIELDS: 0
FIELD-CHECK-CONTROL: False
FIELD-CHECK-POSITIVE: True
NONCANONICAL-SUBFOLDER-FILES: 0
SUBFOLDER-CHECK-FLAGS-OTHER: True
SUBFOLDER-CHECK-FLAGS-QA-GATES: False
EVIDENCE-INHERITED-SKIPPED: 1
```

No MISSING-FIELDS: and no NONCANONICAL: rows were printed.

## CMD-SWEEP

```
FILES: 89
ACCOUNT-TOKEN-FILES: 0
PROFILE-LEAF-FILES: 0
MACHINE-TOKEN-FILES: 0
WORKTREE-ROOT-FILES: 0
USERS-PATH-FILES: 0
RAW-DOCUMENT-FILES: 0
```

## Acceptance (P6-T46, all five required)

1. EVIDENCE-MISSING-FIELDS: 0, NONCANONICAL-SUBFOLDER-FILES: 0, FIELD-CHECK-CONTROL: False, FIELD-CHECK-POSITIVE: True, SUBFOLDER-CHECK-FLAGS-OTHER: True and SUBFOLDER-CHECK-FLAGS-QA-GATES: False in this task's run: met.
2. Branch (a) of P6-T7: AC-CHECKED: 25, AC6-UNCHECKED: 1, AC27-UNCHECKED: 1, AC-ANY-UNCHECKED: 2: met.
3. ACCOUNT-TOKEN-FILES, PROFILE-LEAF-FILES, MACHINE-TOKEN-FILES, WORKTREE-ROOT-FILES, USERS-PATH-FILES and RAW-DOCUMENT-FILES all 0: met.
4. FILES: 89 (at least fifty): met.
5. Plan outcome reported as complete with AC6 and AC27 deferred to the PR step (branch (a)), not as PASS over an unchecked AC: met.
