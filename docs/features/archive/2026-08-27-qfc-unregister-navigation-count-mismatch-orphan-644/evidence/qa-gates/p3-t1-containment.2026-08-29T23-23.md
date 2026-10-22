# [P3-T1] — Change Containment Check

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P3-T1]
Working directory: `<repo-root>` (the repository root of this worktree)
EXIT_CODE: 0 (both commands)

Redaction note: no absolute host path, account name, or machine name appears in this artifact.

## Commands

1. `git diff --name-status a2c69aead286ad0ec6c7087f1bd8c46d39d0d472 -- . ':!.claude/agent-memory'`
2. `git status --porcelain -- . ':!.claude/agent-memory'` (required companion)

The two commands are complementary and each alone is wrong in one state. The anchored
name-status diff enumerates tracked changes only, so it is structurally blind to untracked
files: `policy-audit.2026-08-29T23-06.md` is untracked at cycle entry and is therefore
**correctly absent** from the name-status output even though `[P1-T3]` and `[P1-T4]` both edited
it, and its absence is not evidence that those edits did not happen. The porcelain companion is
what observes it. Conversely, porcelain goes empty once the change is committed, which is why
the anchored diff carries the tracked-file assertion.

## Name-status output

```
M	QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
M	docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/research/research.2026-08-29T07-55.md
```

## Acceptance clauses

All four clauses hold.

### Clause 1 — tracked modifications are exactly the expected two

Filtering the name-status output to lines beginning with `M` returns exactly two lines:

- `M	QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`
- `M	docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/research/research.2026-08-29T07-55.md`

Measured count of `M`-prefixed lines: `2`. Required: `2`. Result: PASS

No `A`, `D`, or `R` line appears, so no tracked file was added, deleted, or renamed.

### Clause 2 — neither out-of-scope file is dirty

A grep of the porcelain companion for `QfcCollectionController.cs` and for
`plan.2026-08-29T07-42.md` returns no match (grep exit 1). There is therefore no ` M ` entry —
and in fact no entry of any status — for the production file
`QuickFiler/Controllers/QfcCollectionController.cs` or for the approved plan of record
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/plan.2026-08-29T07-42.md`.
Result: PASS

### Clause 3 — the twice-edited untracked artifact is positively present

The porcelain companion contains this line:

```
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/policy-audit.2026-08-29T23-06.md
```

Its status field is `??` and its path matches exactly. This positively confirms that the file
edited by `[P1-T3]` and `[P1-T4]` is present in the working tree as a new untracked artifact and
will be staged by `[P3-T4]`. Result: PASS

### Clause 4 — this cycle's new evidence artifacts are present

The porcelain companion contains twelve `??` entries under
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/`.

**Listing form observed: both forms, exactly as the plan anticipates.**

- Collapsed form, one entry whose path ends in `/`:
  - `?? docs/features/active/.../evidence/remediation-baseline/`

  This subfolder is wholly untracked — it is new in this cycle and contains no artifact from any
  prior cycle — so git collapses it into a single directory entry. It holds the four Phase 0
  artifacts `p0-t1-instructions-read`, `p0-t2-cr1-baseline`, `p0-t3-pa7-baseline`, and
  `p0-t4-invariance-baseline`, all at timestamp `2026-08-29T23-23`.

- Individual form, eleven entries:
  - six under `evidence/other/`: `p1-t1-cr1-edit`, `p1-t2-pa7-research-edit`,
    `p1-t3-pa7-policy-audit-edit`, `p1-t4-pa7-verification-line-edit`, `p1-t5-pa7-sweep`,
    `p1-t6-cr1-line222-edit`
  - five under `evidence/qa-gates/`: `p2-t1-csharpier-format`, `p2-t2-csharpier-check`,
    `p2-t3-analyzer-build`, `p2-t4-nullable-build`, `p2-t5-vstest-final`

  Both of these subfolders already contain tracked artifacts from the prior cycle, so git lists
  this cycle's new files individually rather than collapsing the directory.

Result: PASS

## Other porcelain entries

The remaining `??` entries are the four feature-folder documents that were already untracked at
cycle entry and that `[P3-T4]` stages for the first time: `code-review.2026-08-29T23-06.md`,
`feature-audit.2026-08-29T23-06.md`, `policy-audit.2026-08-29T23-06.md`, and
`remediation-inputs.2026-08-29T23-23.md`, plus this cycle's plan file
`remediation-plan.2026-08-29T23-23.md`.

No build output, package directory, coverage artifact, or test-results directory appears
anywhere in the scoped porcelain. The repo-local SDK (`.dotnet-sdk/`), the restored NuGet
`packages/` tree, and the vstest results directory (`coverage/trx/...`) are all matched by
`.gitignore`, at lines 350, the packages patterns, and line 144 respectively.

## Output Summary

Change containment confirmed. The anchored name-status diff reports exactly two tracked
modifications, the digits test file and the research artifact, with no addition, deletion, or
rename. The porcelain companion shows no entry for the production file or the approved plan of
record, positively shows the twice-edited `policy-audit.2026-08-29T23-06.md` as `??`, and shows
this cycle's twelve new evidence artifacts in the mixed collapsed-and-individual listing form
the plan anticipates. All four clauses PASS.
