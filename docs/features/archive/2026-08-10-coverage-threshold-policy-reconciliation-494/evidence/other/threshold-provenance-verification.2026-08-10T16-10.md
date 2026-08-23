# Threshold Provenance Verification (Issue #494)

Timestamp: 2026-08-10T16-10
Captured by: orchestrator (preparation mode)
Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-abe56d74550beb67c`
Branch: `bug/coverage-threshold-policy-reconciliation-494` (HEAD `edf3d34c`)

## Purpose

`spec.md` decision D1 rests on the inference that the 85/75 threshold cluster entered the
repository as un-reconciled foreign-bundle import leakage rather than as a recorded threshold
decision. The `prd-feature` agent had no shell tool and correctly recorded the supporting commit
claims as `[UNVERIFIED]`. This artifact supplies the verification. The commands and their outputs
are reproduced below.

The execution-time re-verification gate that `spec.md` D1 imposes remains in force; this artifact
does not replace it. It removes the `[UNVERIFIED]` marker from the planning-time record.

## Correction to the record

An earlier orchestrator checkpoint entry and the `prd-feature` delegation prompt both dated commit
`48e46387` as `2026-08-05`. That was an orchestrator transcription error. The verified date is
**2026-07-05**. All other claims verified as stated.

## Evidence

### E1 — `CLAUDE.md`'s 80% statement is long-standing

Command:

```
git log --format="%h|%ad|%s" --date=short -S"Repository-wide line coverage must remain" -- CLAUDE.md
```

EXIT_CODE: 0

Output:

```
25684df8|2026-03-21|(bug): fixed agents
```

Output Summary: Exactly one commit has ever changed that string in `CLAUDE.md`. The
`>= 80%` repository-wide line-coverage statement has been unchanged since 2026-03-21.

### E2 — `quality-tiers.md` has exactly one commit in its history

Command:

```
git log --format="%h|%ad|%an|%s" --date=short -- .claude/rules/quality-tiers.md
```

EXIT_CODE: 0

Output:

```
48e46387|2026-07-05|Dan Moisan|(chore): push down claude ecosystem
```

Output Summary: `.claude/rules/quality-tiers.md` was created by `48e46387` and never
subsequently amended. The 85/75 gate matrix and the `quality-tiers.yml` / `tier-classification`
claims all originate in that single commit.

### E3 — `general-unit-test.md`'s coverage lines were rewritten by the same commit

Command:

```
git log --format="%h|%ad|%an|%s" --date=short -L 22,26:.claude/rules/general-unit-test.md
```

EXIT_CODE: 0

Output (commit headers only):

```
48e46387|2026-07-05|Dan Moisan|(chore): push down claude ecosystem
a564add0|2026-06-13|Dan Moisan|refactor(coverage): exempt COM/VSTO/WinForms code from coverage metric (#197)
ca531c67|2026-05-27|Dan Moisan|fix(gitignore): track .claude env so worktrees inherit tooling
```

Output Summary: The most recent change to the coverage-threshold region of
`.claude/rules/general-unit-test.md` is `48e46387`. The prior touch, `a564add0` (#197), is the
commit that established the COM/VSTO/WinForms exemption — evidence that the exemption predates
the 85/75 cluster and was a deliberate, issue-backed change.

### E4 — `48e46387` is a bulk ecosystem sync

Command:

```
git show --stat --format="%h %ad %s" --date=short 48e46387
```

EXIT_CODE: 0

Output (summary line):

```
55 files changed, 4374 insertions(+), 641 deletions(-)
```

Output Summary: 55 files, +4374/-641. The commit subject is `(chore): push down claude
ecosystem`. Scale and subject are both consistent with a tooling-bundle synchronization rather
than a targeted policy decision.

### E5 — `48e46387` did not touch `CLAUDE.md`

Command:

```
git show --stat --format="" 48e46387 -- CLAUDE.md
```

EXIT_CODE: 0

Output: (empty)

Output Summary: `CLAUDE.md` is absent from the commit's file list. The commit that introduced the
competing 85/75 thresholds did not modify the document stating 80/90. It therefore performed no
reconciliation between them and recorded no decision as to which governs. This is the specific
fact on which D1's inference rests, and it is verified.

### E6 — Cited supporting artifacts are absent

Command:

```
ls docs/ci.research.md ; ls quality-tiers.yml
```

EXIT_CODE: 2 (both paths absent)

Output:

```
ls: cannot access 'docs/ci.research.md': No such file or directory
ls: cannot access 'quality-tiers.yml': No such file or directory
```

Output Summary: `.claude/rules/quality-tiers.md:9` cites `docs/ci.research.md` section 1 as the
tier system's source of truth and asserts a root `quality-tiers.yml`. Neither file exists.
`.github/workflows/` contains only `ci.yml` and `codex-web-setup-test.yml`, so the asserted
`tier-classification` CI stage does not exist either.

### E7 — The `quality-tiers.yml` claim appears in a second always-loaded rule file

Command:

```
grep -n "quality-tiers.yml" .claude/rules/*.md .claude/skills/**/*.md
```

EXIT_CODE: 0

Output:

```
.claude/rules/general-code-change.md:29: ... Every project must be classified in `quality-tiers.yml` at repo root.
.claude/rules/quality-tiers.md:9:  ... the file `quality-tiers.yml` at the repository root maps every project to a tier ...
.claude/rules/quality-tiers.md:20: - `quality-tiers.yml` at repo root maps every project to one tier.
```

Output Summary: Confirms the `prd-feature` finding that `.claude/rules/general-code-change.md:29`
carries the same false claim. Resolving AC6 only within `.claude/rules/quality-tiers.md` would
leave the claim live in a second always-loaded rule file. AC6 has been widened accordingly.

## Conclusion

All commit-level claims underpinning `spec.md` D1 are verified, with one date correction
(`48e46387` is 2026-07-05). The inference D1 draws from them — that the 85/75 cluster is
un-reconciled import leakage rather than a recorded threshold decision — is supported by E2, E4,
and E5 taken together.

The one qualification worth stating plainly: this evidence establishes that `48e46387` made no
*explicit written* reconciliation. It cannot establish authorial intent. `spec.md` D1's
execution-time gate, which halts and escalates if the history is found to contradict this
reading, remains the correct safeguard and must be executed as written.
