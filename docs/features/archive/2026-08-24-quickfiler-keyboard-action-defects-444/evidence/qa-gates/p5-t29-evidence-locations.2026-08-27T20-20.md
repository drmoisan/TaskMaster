# [P5-T29] Evidence-location audit

Timestamp: 2026-08-27T20-20
Command: `find docs/features/active/quickfiler-keyboard-action-defects-444/evidence -type f`, `find docs/features/active/quickfiler-keyboard-action-defects-444 -name '*.ps1'`, and `git status --porcelain -- QuickFiler QuickFiler.Test docs/features/active/quickfiler-keyboard-action-defects-444`
EXIT_CODE: 0
Output Summary: 85 files under the feature's evidence tree, every one resolving under a canonical
`<kind>` folder from the set `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`.
Zero artifacts resolve under `artifacts/`. Zero files with a `.ps1` extension exist anywhere under
the feature tree.

## Artifacts by resolved `<kind>` folder

| `<kind>` | Canonical? | File count |
| --- | --- | --- |
| `baseline` | yes | 18 |
| `regression-testing` | yes | 15 |
| `qa-gates` | yes | 44 |
| `issue-updates` | yes | 3 |
| `other` | yes | 5 |
| **total** | — | **85** |

Those five are exactly the five canonical `<kind>` values named by
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. A directory listing at
`evidence/` depth 1 returns those five names and nothing else, so no non-canonical `<kind>` folder
exists.

### `baseline` (18)

`phase0-instructions-read`, `p0-t6-environment`, `p0-t7-dotnet-sdk`, `p0-t8-nuget-restore`,
`p0-t9-meziantou`, `p0-t10-roslynator`, `p0-t11-tool-restore`, `p0-t12-upstream-468-verification`,
`p0-t13-controller-anchors`, `p0-t14-digits-read-baseline`, `p0-t15-csproj-anchors`,
`p0-t16-csharpier-check`, `p0-t17-analyzer-baseline`, `p0-t18-nullable-baseline`, `p0-t19-build`,
`p0-t20-coverage-baseline`, `p0-t21-file-metrics`, `p0-t22-nav-tests-baseline`.

### `regression-testing` (15)

`fail-before-exception`, `p1-t2-build`, `p1-t5-build`, `p1-t10-build`, `p1-t11-kbdactions-suite`,
`p1-t14-build`, `p1-t15-keysdown-pin`, `p1-t16-keysdown-binding`, `p2-t6-build`, `p2-t10-nav-tests`,
`p3-t2-build`, `p3-t7-build`, `p3-t12-build`, `p3-t13-482-suite`, `p5-t7-no-live-form`.

### `qa-gates` (44)

`fail-before-444`, `fail-before-472`, `fail-before-482`, `p1-t3-444-red`, `p1-t6-444-green`,
`p1-t19-format`, `p1-t20-size`, `p2-t3-472-red`, `p2-t7-472-green`, `p2-t8-digits-zero-read`,
`p2-t9-format-selection`, `p2-t11-frozen-test-file`, `p2-t12-interface-untouched`, `p2-t13-format`,
`p2-t14-size`, `p3-t3-482-red`, `p3-t8-482-green`, `p3-t14-single-owner`,
`p3-t15-signature-retention`, `p3-t16-format`, `p3-t17-size`, `p3-t27-ac482-08`, `p4-t1-format`,
`p4-t2-format-check`, `p4-t3-size-audit`, `p4-t4-analyzers`, `p4-t5-typecheck`,
`p4-t6-final-tests`, `p4-t7-trx-hygiene`, `p4-t8-coverage-final`, `p4-t9-syncexpanded-coverage`,
`p4-t10-kbdactions-coverage`, `p4-t11-coverage-delta`, `p4-t12-clean-pass`, `p5-t1-branch-diff`,
`p5-t2-forbidden-paths`, `p5-t4-remove-contract`, `p5-t6-declined-seam`, `p5-t8-plan-anchors`,
`p5-t10-completion-report`, `p5-t28-ac-reconciliation`, this audit artifact, plus the `p4-t6`
results subdirectory holding `p4-t6-final.trx` and two binary `.coverage` attachments.

### `issue-updates` (3)

`p5-t25-ac472-10-deferred`, `p5-t26-ac482-11-deferred`, `p5-t27-ac482-12-deferred`.

### `other` (5)

`p1-t30-logger-review`, `p3-t28-unread-mechanism`, `p5-t3-upstream-contract`, `p5-t5-public-api`,
`p5-t9-pr-body-inputs`.

## Zero artifacts under `artifacts/`

No artifact this plan produced resolves under `artifacts/`. Every write target named by every task
of this plan is a path under
`docs/features/active/quickfiler-keyboard-action-defects-444/evidence/<kind>/`. No
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` record applies: no caller or task supplied a non-canonical
path that had to be substituted (decision D-P2).

In particular, `artifacts/csharp/coverage.xml` was **not** created. A repository hook reads that
exact path as JaCoCo against a hard-coded 85 percent floor; this plan records its coverage figures
inside the feature evidence artifacts instead, per `[P4-T8]` through `[P4-T11]`.

## Zero `.ps1` files under the feature tree

`find docs/features/active/quickfiler-keyboard-action-defects-444 -name '*.ps1'` returns **0** files.
Every helper script used during execution was written to the session scratch directory outside the
repository, never under the feature tree. This matters because the feature-review coverage check
matches by file extension and is path-blind: a single retained `.ps1` anywhere under the feature tree
would force a spurious coverage failure.

## Host-identifying values

The two binary `.coverage` attachments under `evidence/qa-gates/p4-t6/` retain host-derived file
names, as `vstest.console.exe` wrote them. They are matched by `.gitignore:140` (`*.coverage`) and are
therefore **never committed**, so no host-identifying value enters a tracked file. The one tracked
file in that directory, `p4-t6-final.trx`, was normalized by `[P4-T7]`: its `Select-String
-SimpleMatch` counts for `:\Users\`, the machine name, and the account name are each `0`.

## Uncommitted paths at this moment

`git status --porcelain -- QuickFiler QuickFiler.Test docs/features/active/quickfiler-keyboard-action-defects-444`
reports 14 entries at the moment this artifact is written:

| State | Path |
| --- | --- |
| ` M` | `docs/features/active/quickfiler-keyboard-action-defects-444/plan.2026-08-24T20-33.md` |
| ` M` | `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md` |
| `??` | `evidence/issue-updates/` (three deferral artifacts) |
| `??` | `evidence/other/p5-t3-upstream-contract.2026-08-27T20-07.md` |
| `??` | `evidence/other/p5-t5-public-api.2026-08-27T20-09.md` |
| `??` | `evidence/other/p5-t9-pr-body-inputs.2026-08-27T20-13.md` |
| `??` | `evidence/qa-gates/p5-t1-branch-diff.2026-08-27T20-06.md` |
| `??` | `evidence/qa-gates/p5-t2-forbidden-paths.2026-08-27T20-06.md` |
| `??` | `evidence/qa-gates/p5-t4-remove-contract.2026-08-27T20-08.md` |
| `??` | `evidence/qa-gates/p5-t6-declined-seam.2026-08-27T20-10.md` |
| `??` | `evidence/qa-gates/p5-t8-plan-anchors.2026-08-27T20-12.md` |
| `??` | `evidence/qa-gates/p5-t10-completion-report.2026-08-27T20-14.md` |
| `??` | `evidence/qa-gates/p5-t28-ac-reconciliation.2026-08-27T20-19.md` |
| `??` | `evidence/regression-testing/p5-t7-no-live-form.2026-08-27T20-11.md` |

plus this audit artifact itself and this task's `spec.md` edit, both already covered by the two rows
above.

**Recorded divergence from this task's wording.** The task text anticipates that "the only paths
still uncommitted at this moment are this task's own spec edit and this audit artifact". That is not
the case, and the reason is structural rather than a defect: Phase 5 contains **no intermediate
commit** between `[P5-T1]` and `[P5-T30]`, so every Phase 5 artifact from `[P5-T1]` onward is still
uncommitted when this audit runs. The substance of the requirement is unaffected — every uncommitted
path listed above is committed by `[P5-T30]`, and `[P5-T31]` and `[P5-T32]` then verify the tree
clean under the same pathspec. The statement is recorded truthfully rather than restated to match the
task's presupposition.

No `.cs`, `.csproj`, or other source path is uncommitted: the two `QuickFiler` and `QuickFiler.Test`
pathspec components contribute zero entries, confirming the final toolchain pass rewrote no source
file.

## AC-QA-13 check-off

AC-QA-13 reads: "All evidence artifacts produced by this feature are written under
`docs/features/active/quickfiler-keyboard-action-defects-444/evidence/<kind>/` per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, and the working tree is clean at
completion."

| Clause | Status | Evidence |
| --- | --- | --- |
| all evidence artifacts under `evidence/<kind>/` | **satisfied** | 85 of 85 files under the five canonical kinds; zero under `artifacts/` |
| the working tree is clean at completion | **discharged by `[P5-T30]` through `[P5-T32]`** | `[P5-T30]` commits the 14 uncommitted paths; `[P5-T31]` records the resulting `git status --porcelain` under the feature pathspec; `[P5-T32]` commits that record and requires the same command to produce no output afterwards |

The criterion is checked off as `[P5-T29]` directs. Its clean-tree clause speaks to the state "at
completion", which is after `[P5-T32]`, not at the moment of this audit.

## Acceptance

- AC-QA-13 is `- [x]` — met.
- Every listed artifact resolves under
  `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/` with a `<kind>` in the set
  `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other` — met; 85 of 85.
- Zero artifacts resolve under `artifacts/` — met.
- Zero files with a `.ps1` extension exist anywhere under the feature's evidence tree — met; the
  search was run over the whole feature folder, which is a superset of the evidence tree, and
  returned 0.
