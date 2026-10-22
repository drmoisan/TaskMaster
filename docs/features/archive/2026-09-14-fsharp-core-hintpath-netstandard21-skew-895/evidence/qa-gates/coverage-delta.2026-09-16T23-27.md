# Phase 4 — Coverage Delta and No-Regression Record (Issue #895)

Timestamp: 2026-09-17T01-28
Task: [P4-T11]
WORKTREE-LEAF: agent-a8bc4dc5978785885

The six coverage figures below are copied out of
`evidence/baseline/test-coverage-baseline.2026-09-16T23-27.md` and
`evidence/qa-gates/test-final.2026-09-16T23-27.md`. No new coverage command was run for this task.

EXIT_CODE: 0
ExpectedExitCode: 0

## Figures

```
BASELINE_LINE_RATE=0.858708
BASELINE_LINES_VALID=65616
BASELINE_DOCUMENT_STATE=POSTPROCESSED
POST_CHANGE_LINE_RATE=0.858678
POST_CHANGE_LINES_VALID=65616
POST_CHANGE_DOCUMENT_STATE=POSTPROCESSED
```

## BRANCH: COMPARABLE

Both document states are `POSTPROCESSED`, so the two rates were computed by the same
post-processing path. The two `LINES_VALID` figures are identical at 65616, a difference of 0, which
is within 1 percent of the baseline figure (656.16). The comparison is therefore over the same
instrumented denominator and the rate clause applies.

Rate clause: `POST_CHANGE_LINE_RATE` must be at least `BASELINE_LINE_RATE` minus 0.005, that is at
least 0.853708. The observed value is 0.858678, which satisfies it. The measured movement is
-0.00003, equivalent to two lines out of 65616 (56345 covered at baseline against 56343 after), and
is well inside the tolerance.

No coverage regression is recorded.

## CHANGED-PRODUCTION-LINES: 0

Derived from two complementary spans, both run in this task, and both of which printed nothing.

First, the anchored name-listing diff:

```
git diff --name-only origin/main -- "*.cs" ":(exclude)*.Test/*"
```

Output: empty. This enumerates tracked changes only and is therefore blind to a production `.cs`
file the run created but never staged, which is why the second span is also run.

Second, its companion:

```
git status --porcelain --untracked-files=all -- "*.cs" ":(exclude)*.Test/*"
```

Output: empty. This is the only one of the two that can report such an untracked production `.cs`
file. It is still live because this task executes before the `[P4-T13]` commit; the anchored diff
covers the committed Phase 1 state that the porcelain span no longer reports. Neither span alone can
fail in every state this record claims to exclude, which is why both are run.

No production `.cs` file changed. The three `.cs` files this plan touches are all under test project
directories, and the coverage runner appends an `.*\.Test\.dll$` module exclusion at run time, so
all three are outside the instrumented denominator. The three edited project files are `.csproj` and
contain no executable lines.

The changed-line no-regression obligation is therefore discharged by the absence of any changed
production line, not by a measured per-line comparison.

## NEW-MODULE-COVERAGE: N/A (test code, outside the instrumented denominator)

## Acceptance

- All six figures are present as numbers or the two permitted state literals: yes.
- Exactly one `BRANCH:` line: yes, `BRANCH: COMPARABLE`.
- The rate clause holds under the `COMPARABLE` branch: yes, 0.858678 is at least 0.853708.
- `CHANGED-PRODUCTION-LINES: 0` is present, and both the name-only span and the
  `git status --porcelain --untracked-files=all` companion span printed nothing: yes.
