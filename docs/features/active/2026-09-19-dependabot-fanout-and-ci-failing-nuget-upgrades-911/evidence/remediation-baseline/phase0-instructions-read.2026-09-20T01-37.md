# Phase 0 Instructions Read — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-25-49
- Task: [P0-T1]
- Cycle: remediation cycle 1, artifact timestamp label `2026-09-20T01-37`
- Worktree read from: execution worktree (`<execution-worktree-root>`), branch
  `bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911`
- EXIT_CODE: 0

All paths below are repository-relative. No absolute host path appears in this artifact, per
**gate rule 17**.

## Policy Order

Read in the order `policy-compliance-order` defines, then the four review artifacts of this cycle:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/quality-tiers.md`
5. `.claude/rules/tonality.md`
6. `.claude/rules/powershell.md`
7. `.claude/rules/ci-workflows.md`
8. `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/code-review.2026-09-20T01-37.md`
9. `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/feature-audit.2026-09-20T01-37.md`
10. `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/policy-audit.2026-09-20T01-37.md`
11. `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/remediation-inputs.2026-09-20T01-37.md`

## Files Read, With SHA-256

Command:

```
Get-FileHash -Algorithm SHA256 -Path <path>
```

| # | Path | SHA-256 | Lines |
|---|---|---|---|
| 1 | `CLAUDE.md` | `AFF8D68843FDCDEB6B4BBCDF8CEB7FBF90F6D3B7C2D328116F4820B1C706EB02` | 463 |
| 2 | `.claude/rules/general-code-change.md` | `91A89164532368F02B617AE9FF2B4E5247BA155C4D6F34ACEFD767B74AE46F53` | 80 |
| 3 | `.claude/rules/general-unit-test.md` | `C0B3F9B1BD2E55C29484611D64655E2F71A1DB97E05BA0680E289754713B63BF` | 105 |
| 4 | `.claude/rules/quality-tiers.md` | `4A21F084C11FD3614EC1540E7841D353C7E5B6D03FC3065D13C74FD58989A626` | 51 |
| 5 | `.claude/rules/tonality.md` | `48E35A5A941E72537A222CFD93D548218C6C6C8CBBAD3173C455F60A27415948` | 80 |
| 6 | `.claude/rules/powershell.md` | `50A8FC41474BCB896EF8F1E03F92FFC6C30F3FB8265E414EEB65369C8A74BBF6` | 97 |
| 7 | `.claude/rules/ci-workflows.md` | `A6BFDC1E9F610562E9474D150357C890AFE41438ABC864D6A2A2A065D7A160AC` | 42 |
| 8 | `docs/.../code-review.2026-09-20T01-37.md` | `FFD68F207296E7310EECA8622B3299E010EBB0C037E195BFB104EDAD25E47C17` | 96 |
| 9 | `docs/.../feature-audit.2026-09-20T01-37.md` | `B2AC896BDF7EF81C09AA3D4EA5257D8BE98A2079F79EFC0E0DB39C7963CD35BF` | 121 |
| 10 | `docs/.../policy-audit.2026-09-20T01-37.md` | `F4C8CFBE457550FE60D697468E3368883F1ABC5ADEC9316BD740FA378F99484B` | 311 |
| 11 | `docs/.../remediation-inputs.2026-09-20T01-37.md` | `E7E9A13B8B30121FB03F440643B9ADFF07EEA5B6AA88668E27CDEDDAFB4464F6` | 204 |

Rows 8 through 11 abbreviate the shared prefix
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`. Eleven files
listed, each with a non-empty 64-character hash.

## Coverage Floors as the Execution Worktree's `CLAUDE.md` States Them

Read from `CLAUDE.md` lines 303 through 312 in the execution worktree:

- C# line coverage: **>= 80%**. (`CLAUDE.md:303`)
- C# branch coverage: **>= 75%**. (`CLAUDE.md:303`)
- PowerShell line coverage: **>= 80%**. (`CLAUDE.md:304`)
- PowerShell branch coverage: **no floor is stated.** `CLAUDE.md:304` records the reason: Pester does
  not measure branch coverage.
- New modules, classes and methods: **>= 90%**. (`CLAUDE.md:312`)
- `CLAUDE.md:305` records that these figures were settled by the project maintainer on 2026-09-11
  under issue **#563**.

## The 80-Versus-85 Conflict

`.claude/rules/general-unit-test.md:23` states `Line coverage must remain >= 85% across all tiers
(T1-T4).` `.claude/rules/quality-tiers.md` carries the same 85 figure, and
`.claude/rules/powershell.md:63-64` restates it for PowerShell.

The two readings disagree. The conflict is **open issue #668** and is not resolved by this cycle.

Per **gate rule 13** of `plan.2026-09-19T09-44.md`, the authoritative figure for this cycle is the
**80** stated in the execution worktree's `CLAUDE.md`, settled by the project maintainer on
2026-09-11 under issue #563. The 85 in `.claude/rules/general-unit-test.md` is push-down-owned
upstream boilerplate.

Consequence recorded in advance, per decision **D5**: `scripts/vscode/Sync-PackageReferences.ps1`
lands at 81.89 percent after Phase 1. That clears the authoritative 80 and does not clear the
superseded 85. Both readings are recorded against it at [P1-T11].

## Output Summary

Eleven files read and hashed; every hash non-empty and 64 characters. Recorded floors: C# line 80,
C# branch 75, PowerShell line 80 with no branch floor, new modules 90. The rules-file figure of 85
is recorded together with issue #668 and the gate rule 13 precedence that makes the `CLAUDE.md`
figure authoritative for this cycle. No file was unreadable.
