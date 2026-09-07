# P4-T11 — Final commit of the remediation (cycle 1)

Timestamp: 2026-08-28T04-19
Task: [P4-T11]
Command: git add -- QuickFiler/ QuickFiler.Test/ docs/features/active/itemviewer-surface-defects-489/ && git commit
EXIT_CODE: 0

## The commit series, and why there is more than one

The plan directs a single commit. The execution briefing that carries this plan directs committing the
plan first and then after each phase, so that a 2.4 MB MSBuild log, four TRX files and every gate
artifact are not held uncommitted across two full solution rebuilds, four test runs and two
repository-wide coverage runs. Those two instructions cannot both be satisfied literally. The
commit-as-you-go instruction was followed, for the same reason the feature's own Phase 11 followed it
(`p11-t14-final-commit.2026-08-28T02-35.md`: "Two earlier commits rather than one were made
deliberately, per the instruction to commit after each gate completes"), and the whole series is
recorded here so the audit trail is complete rather than implied.

The remediation base is `7ad2bd17`, the branch head at planning time. Every commit below is this
cycle's work.

| # | SHA | Contents |
|---|---|---|
| 1 | `899000d379f1f54b71aaf88cb3b5173a42509d37` | the approved remediation plan, which arrived untracked |
| 2 | `d77ac2126ec62a37e18a9e20ef220571dc2e4ec2` | P0-T1 read artifact and check-off — this is REM_BASE |
| 3 | `aedfde52e47bf2fd43202f7a37b8801c54b179cc` | Phase 0 baseline: repo state, defect measurement, csharpier baseline, adopted baseline |
| 4 | `71e363eaf1f61b1d470e74390c9f7501f2725b92` | Phase 1: the RED test in `EventWiringTests.Part2.cs`, the compile build, the RED run and its TRX |
| 5 | `6d6a0ee1fbd29575e3ff26fa5394e8817d842f71` | Phase 2: the production detachment line in `EventWiring.cs`, the rebuild, the GREEN run and its TRX |
| 6 | `77596b58c65e2231594a323554219ea7ae990718` | Phase 3: the handoff-record addendum, the three `spec.md` amendments, and both gate artifacts |
| 7 | this task's commit | Phase 4: all eleven QC artifacts, the analyzer and nullable `.msbuild.txt` logs, `rem1-p4-t6.trx`, and the P4-T1..T11 plan check-offs |

**This task's commit SHA:** `2a7521da4d8887c458a238ce195b58eaec20b71e`

That value was written into this artifact after the commit was created, by a follow-up commit — a
commit cannot record its own hash, so the alternative would have been to leave the field blank. The
follow-up commit is the one P4-T12 records, which the plan explicitly permits ("amending into or
appending after the P4-T11 commit is acceptable and is recorded in the artifact").

## The required file list

The plan's acceptance names four paths that must appear in the commit's file list. All four appear in
this cycle's commit series; because commit-as-you-go was followed, they are distributed across it
rather than concentrated in the terminal commit. `git diff --stat 7ad2bd17..HEAD` — the cumulative file
list for the whole remediation — is the authoritative view:

```
 QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs   |    24 +
 QuickFiler/Controllers/QfcItemController.EventWiring.cs                   |     1 +
 docs/.../evidence/other/wireintentevents-16-to-17-handoff...md            |    82 +
 docs/.../evidence/qa-gates/rem1-p3-t1-handoff-addendum...md               |    72 +
 docs/.../evidence/qa-gates/rem1-p3-t2-spec-amendment...md                 |   128 +
 docs/.../evidence/regression-testing/rem1-p1-t2-build...msbuild.txt       | 11982 +
 docs/.../evidence/regression-testing/rem1-p1-t2-build...md                |    94 +
 docs/.../evidence/regression-testing/rem1-p1-t3-red-rc1...md              |   121 +
 docs/.../evidence/regression-testing/rem1-p1-t3.trx                       |   106 +
 docs/.../evidence/regression-testing/rem1-p2-t2-build...md                |    80 +
 docs/.../evidence/regression-testing/rem1-p2-t2-build...msbuild.txt       | 11948 +
 docs/.../evidence/regression-testing/rem1-p2-t3-green-rc1...md            |   101 +
 docs/.../evidence/regression-testing/rem1-p2-t3.trx                       |    63 +
 docs/.../evidence/remediation-baseline/rem1-phase0-adopted-baseline...md  |   131 +
 docs/.../evidence/remediation-baseline/rem1-phase0-csharpier-check...md   |    48 +
 docs/.../evidence/remediation-baseline/rem1-phase0-defect-measurement...md|    93 +
 docs/.../evidence/remediation-baseline/rem1-phase0-instructions-read...md |    68 +
 docs/.../evidence/remediation-baseline/rem1-phase0-repo-state...md        |    96 +
 docs/.../remediation-plan.2026-08-28T03-13.md                             |   143 +
 docs/.../spec.md                                                          |    47 +-
```

That listing is taken before this task's commit, which adds the thirteen Phase 4 artifacts and the
P4-T1..T11 check-offs on top of it.

| Required path | Present | Commit that carried it |
|---|---|---|
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | **Yes**, +1 line | `6d6a0ee1` |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` | **Yes**, +24 lines | `71e363ea` |
| `docs/features/active/itemviewer-surface-defects-489/spec.md` | **Yes**, +45 / -2 | `77596b58` |
| `docs/features/active/itemviewer-surface-defects-489/evidence/other/wireintentevents-16-to-17-handoff.2026-08-28T01-55.md` | **Yes**, +82 lines | `77596b58` |

## No path outside the permitted set

Every path in `git diff --name-only 7ad2bd17..HEAD` is either one of the two scope-locked source files
or lives under `docs/features/active/itemviewer-surface-defects-489/`. There is no third source path,
no `.csproj`, no `.props`, no `.targets`, no `.config`, and nothing under any other feature folder.
P4-T9 verified this independently against the 25-path scope lock and against the REM_BASE-scoped
`*.csproj` diff.

`.claude/agent-memory/` is tracked in this repository rather than gitignored, and is deliberately
outside every pathspec used by this remediation — every `git add` in this cycle named either
`QuickFiler/`, `QuickFiler.Test/` or the feature folder explicitly, never `-A` and never a bare `.`.

`coverage/coverage.cobertura.xml` is not committed and is not an evidence artifact: `coverage/*` is
gitignored at `.gitignore:144`. The two MSBuild file logs carry the `.msbuild.txt` extension precisely
so that `.gitignore:84` (`*.log`) does not match them.

## Sanitisation state of the committed binaries and logs

Every committed TRX and `.msbuild.txt` log was sanitised before it was staged, case-insensitively, with
the worktree root replaced by `<repo-root>`, the main checkout root by `<main-checkout-root>`, the
machine name by `<host>` and the account name by `<user>`; placeholders in TRX files are written in XML
entity form and each sanitised TRX was re-parsed with a strict XML reader whose `UnitTestResult` count
matched the run's reported total. A search across all committed `rem1-*` artifacts for the account
name, the machine name and the 8.3 account form returns **zero** matches.

## Acceptance

| P4-T11 condition | Result |
|---|---|
| The commit exists on `bug/itemviewer-surface-defects-489` | **Yes** — `2a7521da4d8887c458a238ce195b58eaec20b71e`, plus the six earlier cycle commits listed above |
| Its file list contains the four required paths | **Yes** — all four present in the cycle's cumulative file list, each attributed to its commit above |
| It contains no path outside the scope lock plus `FEATURE/` documentation and evidence | **Yes** — two scope-locked source files and the feature folder only |

Output Summary: Every change this remediation produced is committed on
`bug/itemviewer-surface-defects-489`. This task's commit is
**`2a7521da4d8887c458a238ce195b58eaec20b71e`**, the seventh and last content commit of a series that
begins at `899000d3`; the series shape follows the briefing's commit-after-each-phase instruction,
which cannot be reconciled literally with the plan's single-commit wording, and the whole series is
recorded above rather than implied. All four required paths are present in the cumulative file list —
`QfcItemController.EventWiring.cs` (+1), `QfcItemController.EventWiringTests.Part2.cs` (+24),
`spec.md` (+45/-2) and the handoff record (+82) — and no path outside the two scope-locked source files
and the feature folder appears anywhere in `7ad2bd17..HEAD`. No `.csproj` is touched,
`coverage/coverage.cobertura.xml` is gitignored and uncommitted, the two build logs use `.msbuild.txt`
so `.gitignore:84` does not exclude them, and every committed TRX and log is sanitised with zero
residual host tokens. `EXIT_CODE: 0`.
