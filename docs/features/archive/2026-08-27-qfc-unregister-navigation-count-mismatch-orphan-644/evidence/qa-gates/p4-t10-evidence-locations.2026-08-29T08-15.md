# QA gate — Evidence-location compliance ([P4-T10])

- Issue: #644
- Task: `[P4-T10]`
- Timestamp: 2026-08-29T08-15

No diff anchor substitution applies to this task: all three of its checks read the filesystem
rather than git, exactly as the task text requires. `artifacts/` is matched by `.gitignore` line
57, so a git span would report nothing for it whatever it contains.

## Check 1 — no artifact of this plan's timestamp under `artifacts/`

Command: `@(Get-ChildItem -Recurse -File -Path artifacts -ErrorAction SilentlyContinue | Where-Object { $_.FullName -match '2026-08-29T08-15' }).Count`
EXIT_CODE: 0

```
0
```

## Check 2 — no forbidden `artifacts/` evidence subfolder exists

Command: `@('artifacts\baselines','artifacts\baseline','artifacts\qa','artifacts\qa-gates','artifacts\evidence','artifacts\coverage','artifacts\regression-testing','artifacts\post-change' | Where-Object { Test-Path $_ }).Count`
EXIT_CODE: 0

```
0
```

## Check 3 — enumeration of the feature folder's evidence tree

Command: `Get-ChildItem -Recurse -File -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\evidence`
EXIT_CODE: 0

The enumeration was reconciled mechanically against the plan rather than by reading. The set of
artifact paths the plan names in Phases 0 through 4 was extracted from the plan text truncated at
the Phase 5 heading, matching paths of the form
`evidence/<kind>/<name>.2026-08-29T08-15.md` where `<kind>` is one of `baseline`,
`regression-testing`, `qa-gates`, `other`, or `issue-updates`. That extraction yields **38**
distinct artifact paths. The on-disk enumeration was normalized to the same forward-slash relative
form and the two sets compared.

Reconciliation measured before this artifact was written:

- Named in Phases 0 through 4 but absent from disk: exactly **1** —
  `evidence/qa-gates/p4-t10-evidence-locations.2026-08-29T08-15.md`, which is this task's own
  artifact and is the file being written by this task.
- Present on disk but not named in Phases 0 through 4: **0**. No stray or misplaced artifact
  exists under the evidence tree.

Reconciliation measured after this artifact was written: **38 of 38** named artifacts present,
**0** named artifacts absent, **0** unnamed artifacts present. That post-write measurement is
recorded in the "Post-write reconciliation" section below.

Full on-disk enumeration, 38 files across four kind subfolders:

```
evidence/baseline/p0-t2-dotnet-sdk.2026-08-29T08-15.md
evidence/baseline/p0-t3-tool-restore.2026-08-29T08-15.md
evidence/baseline/p0-t4-nuget-restore.2026-08-29T08-15.md
evidence/baseline/p0-t5-analyzer-backfill.2026-08-29T08-15.md
evidence/baseline/p0-t6-dotnet-coverage.2026-08-29T08-15.md
evidence/baseline/p0-t7-counts.2026-08-29T08-15.md
evidence/baseline/p0-t8-csharpier-check.2026-08-29T08-15.md
evidence/baseline/p0-t9-analyzer-build.2026-08-29T08-15.md
evidence/baseline/p0-t10-nullable-build.2026-08-29T08-15.md
evidence/baseline/p0-t11-vstest-baseline.2026-08-29T08-15.md
evidence/baseline/p0-t12-coverage-baseline.2026-08-29T08-15.md
evidence/baseline/phase0-instructions-read.2026-08-29T08-15.md
evidence/regression-testing/p1-t1-new-test-file.2026-08-29T08-15.md
evidence/regression-testing/p1-t2-csproj-registration.2026-08-29T08-15.md
evidence/regression-testing/p1-t3-prefix-build.2026-08-29T08-15.md
evidence/regression-testing/p1-t4-expect-fail.2026-08-29T08-15.md
evidence/regression-testing/p2-t5-ledger-green.2026-08-29T08-15.md
evidence/regression-testing/p3-t7-reconciled-green.2026-08-29T08-15.md
evidence/other/p2-t1-ledger-field.2026-08-29T08-15.md
evidence/other/p2-t2-record-after-add.2026-08-29T08-15.md
evidence/other/p3-t1-reported-repro.2026-08-29T08-15.md
evidence/other/p3-t2-swaps-page.2026-08-29T08-15.md
evidence/other/p3-t3-swap-guarded.2026-08-29T08-15.md
evidence/other/p3-t4-digits-flip.2026-08-29T08-15.md
evidence/other/p3-t5-comment-sync.2026-08-29T08-15.md
evidence/qa-gates/p2-t3-registereddigits-removed.2026-08-29T08-15.md
evidence/qa-gates/p2-t4-nullable-build.2026-08-29T08-15.md
evidence/qa-gates/p3-t6-frozen-file-interim.2026-08-29T08-15.md
evidence/qa-gates/p4-t1-csharpier-format.2026-08-29T08-15.md
evidence/qa-gates/p4-t2-csharpier-check.2026-08-29T08-15.md
evidence/qa-gates/p4-t3-analyzer-build.2026-08-29T08-15.md
evidence/qa-gates/p4-t4-nullable-build.2026-08-29T08-15.md
evidence/qa-gates/p4-t5-vstest-final.2026-08-29T08-15.md
evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md
evidence/qa-gates/p4-t7-file-size-audit.2026-08-29T08-15.md
evidence/qa-gates/p4-t8-footprint.2026-08-29T08-15.md
evidence/qa-gates/p4-t9-comment-only-diff.2026-08-29T08-15.md
evidence/qa-gates/p4-t10-evidence-locations.2026-08-29T08-15.md
```

Every one of the 38 lies in a subfolder named for its artifact kind and carries the
`2026-08-29T08-15` timestamp in its filename, as the canonical scheme in
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md` requires.

`evidence/issue-updates/issue-644.2026-08-29T08-15.md` is not in this enumeration because it is
written by `[P5-T19]`, which is a Phase 5 task and therefore outside the "named in Phases 0
through 4" set this task reconciles.

## Post-write reconciliation

Re-running the extraction and the comparison after this file was written:

- Artifact paths named in Phases 0 through 4: 38.
- Named artifacts present on disk: 38.
- Named artifacts absent from disk: 0.
- Artifacts on disk not named in Phases 0 through 4: 0.

## Acceptance evaluation

1. **First count is 0** — PASS. No file under `artifacts/` carries this plan's timestamp.
2. **Second count is 0** — PASS. None of the eight forbidden `artifacts/` evidence subfolders
   exists.
3. **Every artifact path named in Phases 0 through 4 exists under the feature folder's `evidence/`
   tree, in a subfolder named for its kind, with the `2026-08-29T08-15` timestamp in its
   filename** — PASS. 38 named, 38 present, 0 absent, 0 unnamed extras.

Output Summary: Evidence-location compliance verified by filesystem read, which is the only
observation that works here because `.gitignore` line 57 matches `artifacts/` and makes every git
span silent about its contents. No file under `artifacts/` carries the `2026-08-29T08-15`
timestamp (count 0), and none of the eight forbidden evidence subfolders `artifacts\baselines`,
`artifacts\baseline`, `artifacts\qa`, `artifacts\qa-gates`, `artifacts\evidence`,
`artifacts\coverage`, `artifacts\regression-testing`, or `artifacts\post-change` exists (count 0).
This plan therefore wrote no artifact under `artifacts/`. The feature folder's `evidence/` tree
holds 38 files across `baseline/` (12), `regression-testing/` (6), `other/` (7), and `qa-gates/`
(13). Reconciled mechanically against the 38 artifact paths the plan names in Phases 0 through 4:
38 present, 0 absent, and 0 artifacts on disk that the plan does not name. All three acceptance
clauses pass.
