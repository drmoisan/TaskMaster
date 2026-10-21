# Post-change workflow lint (P7-T7)

Timestamp: 2026-09-14T20-22

## Item-rooted workflow listing, recorded before the lint run

Tool: Glob
Pattern used: `<repo-root>/.github/workflows/*.yml`

Returned list, verbatim, with the item worktree root shown as `<repo-root>`:

```
<repo-root>\.github\workflows\_actionlint.yml
<repo-root>\.github\workflows\_build-analyzers.yml
<repo-root>\.github\workflows\_build-nullable.yml
<repo-root>\.github\workflows\_format-check.yml
<repo-root>\.github\workflows\codex-web-setup-test.yml
<repo-root>\.github\workflows\_mstest-coverage.yml
<repo-root>\.github\workflows\_pester.yml
<repo-root>\.github\workflows\ci.yml
```

The list contains all three changed `.yml` paths this phase wrote:

- `.github/workflows/_mstest-coverage.yml`
- `.github/workflows/_pester.yml`
- `.github/workflows/ci.yml`

This item-rooted listing is the named source of the changed-path claim. Entry count is 8, one more than the 7 recorded in the P0-T14 baseline; the added entry is `.github/workflows/_pester.yml`.

## Lint run

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -Command '<worktree prologue>; & "<repo-root>/scripts/dev-tools/run-actionlint.ps1"'`
EXIT_CODE: 0

The prologue is what points actionlint's lint target at this worktree: line 11 of that 14-line script invokes the binary with no path argument, so actionlint resolves its target from the working directory. Without the prologue this gate would have linted the coordinator session worktree and would have observed none of the workflow files this phase wrote.

Output Summary: the lint run printed no diagnostic lines. A clean actionlint run prints nothing, so the absence of output is the pass signal and carries no file count or path name. The file count and the changed-path names are therefore read from the recorded Glob listing above rather than from the lint output.

The fourth changed workflow path is `.github/workflows/README.md`. It is a markdown file, is neither returned by the `*.yml` Glob pattern nor linted by actionlint, and is verified by the content assertions in P7-T6 instead.

## Falsifiability observations for the two removal assertions

### `were moved, not edited` in `.github/workflows/README.md`

- Before count: **1**. The literal occurred at line 49 of that file before P7-T6 ran, verified by reading the file in P0-T1.
- After count: **0**.

This before-and-after pair is what makes the P7-T6 removal falsifiable rather than a claim about a literal that might never have been present.

### `if-no-files-found: warn` in `.github/workflows/_mstest-coverage.yml`

- Before count: **1**. The literal occurred at line 112 of that file before P7-T2 ran, verified by reading the file immediately before the edit.
- After count: **0**.

This before-and-after pair is what makes the P7-T2 replacement falsifiable. The replacement literal `if-no-files-found: error` now occurs exactly once in that callee.
