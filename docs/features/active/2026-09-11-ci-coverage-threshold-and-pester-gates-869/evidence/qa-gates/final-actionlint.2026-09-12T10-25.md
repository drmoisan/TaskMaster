# Final workflow lint (P10-T5)

Timestamp: 2026-09-14T21-12

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
<repo-root>\.github\workflows\ci.yml
<repo-root>\.github\workflows\_pester.yml
```

Entry count: **8**, exactly the required number.

The single entry the list gained relative to the seven recorded in the P0-T14 baseline artifact is:

```
.github/workflows/_pester.yml
```

Every other entry is present in both listings and unchanged. The Glob listing is item-rooted, so a listing that did not hold exactly eight entries, or that did not contain `.github/workflows/_pester.yml`, would be the observation that a wrong tree was addressed or that P7-T3 did not land. Neither is the case.

## Lint run

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -Command '<worktree prologue>; & "<repo-root>/scripts/dev-tools/run-actionlint.ps1"'`
EXIT_CODE: 0

The prologue is what points actionlint's lint target at this worktree: line 11 of that 14-line script invokes the binary with no path argument, so actionlint resolves its target from the working directory.

Output Summary: the lint run printed no diagnostic lines. A clean actionlint run prints nothing, so the absence of output is the pass signal and carries no file count or file name. The file count of eight and the identity of the single added entry, `.github/workflows/_pester.yml`, are therefore read from the recorded Glob listing above rather than from the lint output.

This run was made after the P8-T3 correction to the gate step body of `.github/workflows/_pester.yml`, so the linted content is the delivered content.
