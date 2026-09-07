# Preflight Round 3 — Clearance (issue 662)

- Timestamp: 2026-09-01T07-45
- Directive: `DIRECTIVE: PREFLIGHT VALIDATION ONLY`
- Reviewer: atomic-executor (validation-only pass; nothing was executed, edited, or written)
- Plan of record: `docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/plan.2026-08-31T20-11.md`
- Tree reviewed: HEAD `db59adfe`, base `2b85134b42872e405602e6064e02dc9cda6c319b`
- Signal: `PREFLIGHT: ALL CLEAR`
- Convergence: `CONVERGENCE: NO FURTHER ROUNDS EXPECTED`
- Coverage: all 52 tasks and every prose region, with every citation and count re-derived.

## Verdict

No acceptance condition was found that cannot fail, no gate that is unsatisfiable, no task that
is unexecutable, and no path by which committed evidence leaks a host identifier.

## Round-2 defects: all ten closed

D1 (artifact hygiene rule and the three sweep appends), D2 (results-directory deletion plus the
`LastWriteTime` staleness check in the four vstest tasks, and the Cobertura equivalent in P0-T13
and P2-T9), D3 (class-node enumeration ordered by `name`, with the `BLOCKED, not a pass` rules),
D4 (the `return`-keyword-through-terminating-semicolon span rule; the phrase "first line of that
statement" now occurs zero times in the plan), D5 (the quoted-heredoc `pwsh` form), D6 (null
guards in all ten spans), D7 (`-uall` on both P2-T23 status spans), D8 (the two-trace clause),
D9 (the three-pass loop bound and `loop-termination.md`), D10 (the agent-memory ordering clause).

## The four planner deviations: all accepted

1. The D1 plan-file carve-out is a correct mechanism, and the `git check-ignore -q` filter on the
   rewrite loop is warranted because `/EnableCodeCoverage` writes a binary `.coverage` into each
   results directory that a text rewrite would corrupt.
2. The D3 de-duplication is correct; the replaced sentence occurs exactly once.
3. The D4 sibling harmonization is complete across P0-T13, P2-T9 and P2-T10. Verified against the
   source: `IsValidCreationSelection`'s return statement spans `EfcSelectionGuard.cs:74-76` with
   the renamed call site on `:75`, so the span form is required exactly as D4 argued.
4. The D10 ordering clause is correct and executable; the scoped status runs last, so the
   acceptance can hold.

## Independently re-derived and correct

Counts (all `*.cs`-scoped): `= "===";` gives 1; `= "====";` gives 2; the declaration regex gives 3
with a 9-line superset cross-check carrying 6 non-members; both `("===")` and `("====")` give no
output at exit 1 in the test file, so P1-T8's post-change figure of 2/2 holds.

The base anchor resolves, is an ancestor of HEAD, and equals `origin/main`. The AC5b diff currently
prints nothing at exit 0 and the P2-T23 scope diff is currently empty, so both gates are
falsifiable rather than pre-satisfied. `bin/` and `obj/` are ignored, so the `git add` spans cannot
sweep build output. Every artifact path and every results directory resolves under the canonical
feature `evidence/<kind>/` tree. The runsettings split holds: the repository-root file on exactly
the four coverage spans, the CLI variant on the two that pass no `/EnableCodeCoverage`, and
`/InIsolation` on all six.

The Directional Constraint is unchanged and correct: widening the guard makes
`IsValidFilingSelection("===")` and `IsSelectableFolder("===")` both true, so `:462` still passes
while `:463` fails.

## Non-blocking observations recorded for the file

- **N1.** The D1 plan-file carve-out is now dead text and its stated rationale is false, because the
  orchestrator removed the absolute worktree root from the plan after the carve-out was written. A
  recursive case-insensitive search of the whole feature folder for the account name, the machine
  name, `C:\Users` and `repos/TaskMaster` returns no match in any file. Excluding a file that
  cannot match changes no outcome, so this is inert and not worth a further round. If the plan is
  touched for another reason, delete the scope-decision sentence, the `Where-Object` clause and the
  `$plan` assignment together.
- **N2.** The sweep's rewrite loop filters gitignored files; its verification loop does not, so the
  verification reads the binary `.coverage` the rewrite skips. Tested against a real `.coverage`
  from this repository's own coverage path: the account name occurs zero times as ASCII and 23
  times in UTF-16LE, so a UTF-8 read yields NUL-interleaved text and the ordinal search does not
  match. A zero residual count is therefore attainable and the gate is not unsatisfiable. The
  asymmetry is latent fragility only; applying the same filter to the verification pipeline would
  remove it.
- **N3.** A vstest run with an explicit results directory creates a deployment directory whose NAME
  carries both the account and the machine name. The sweep rewrites and checks file content only,
  so neither clears a directory name. In the observed run those directories are empty and git
  cannot commit an empty directory, so nothing leaks today. Recorded because it is the one
  identifier channel the hygiene rule does not cover.
- **N4.** P0-T13 field group (4) records class-node counts and `line-rate` but not the `name`
  attribute, while P2-T10's class-half gate compares `name` sets across the two captures. Still
  executable, because the baseline Cobertura copy is retained and P2-T10 can read names from both
  documents directly.
- **N5.** P2-T1's before-and-after tree observation is scoped to the three project directories
  while `csharpier format .` is repository-wide. A rewrite outside those directories is invisible
  to P2-T1 but is caught pre-emptively by P0-T6 and finally by P2-T23's repository-wide scope
  check. Detection is late, not absent.
- **N6.** The two items the planner declined to change are not defects. P1-T10's "two items" is
  accurate. D7's `-uall` scoping to P2-T23 is correct, because P0-T18 and P1-T11 use directory
  pathspecs against which a collapsed untracked-directory entry still matches, and both run
  `git add` over the same pathspec immediately before the status.
- **N7.** AC4 says no occurrence-count assertion is made against comment prose, while P1-T5 and
  P2-T14 assert one absence and one count against that comment. The plan's gates are strictly
  stronger than the criterion and both are satisfiable, so there is no conflict requiring revision.

## Round history

| Round | Signal | Blocking | Non-blocking | Convergence |
|---|---|---|---|---|
| 1 | REVISIONS REQUIRED | 9 | 6 | NO FURTHER ROUNDS EXPECTED |
| 2 (abandoned) | none — lost to a session rate limit before reporting | n/a | n/a | n/a |
| 2 (re-run) | REVISIONS REQUIRED | 4 | 6 | FURTHER ROUNDS LIKELY |
| 3 | ALL CLEAR | 0 | 7 observations | NO FURTHER ROUNDS EXPECTED |

Three completed rounds were required against a two-round target. The round-2 re-run attributed the
overrun to the planner substituting its own wording for supplied delta items, which leaves the
substituted text unreviewed until the following round; round 3 supplied verbatim replacement text
for every item and the planner reported each item's disposition explicitly, which is what closed
the loop.
