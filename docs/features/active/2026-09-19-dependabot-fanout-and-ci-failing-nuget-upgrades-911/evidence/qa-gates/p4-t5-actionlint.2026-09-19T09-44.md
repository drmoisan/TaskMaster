# P4-T5 — actionlint, Batch B close-out

Timestamp: 2026-09-20T01-14

Commands:

```
CMD-ACTIONLINT:
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; & "<execution-worktree-root>\scripts\dev-tools\run-actionlint.ps1"'
```

```
Independent enumeration:
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; "WORKFLOW_YML_COUNT=" + (Get-ChildItem .github/workflows -Filter *.yml | Measure-Object | Select-Object -ExpandProperty Count)'
```

EXIT_CODE: 0

## Captured stdout, verbatim

```
```

**Empty.** Measured as 0 bytes:

```
ACTIONLINT_EXIT=0
STDOUT_BYTES=0
```

## The count is an independent filesystem enumeration, not actionlint output

```
WORKFLOW_YML_COUNT=8
```

This is stated in terms, as gate rule 10 requires. **actionlint prints nothing at all on a clean
run** — no file count, no per-file heading, no summary line — so **no count of any kind can be
read from its output**, and the 8 above was obtained by a separate
`Get-ChildItem .github/workflows -Filter *.yml | Measure-Object` call against the filesystem. It
is a filesystem enumeration and it is **not** actionlint output.

The figure is **exactly 8**, matching the Measured Tree Facts row that records 8 workflow YAML
files today and 9 after `dependabot-repair.yml` is created in Phase 7. Batch B created no
workflow file, so 8 is the expected value at this point in the run.

## The non-vacuity argument for an empty-output tool

An empty stdout cannot on its own distinguish a clean run from a tool that never ran. Two
independent observations close that gap:

1. **The runner throws when the binary is absent.** `scripts/dev-tools/run-actionlint.ps1`
   resolves `actionlint-bin\actionlint.exe` relative to the repository root and throws when it is
   not there, so an absent binary is a task failure rather than a silent pass. Exit 0 with no
   thrown error therefore establishes the binary was found and invoked.
2. **The population it was pointed at is non-empty**, measured independently at 8 files.

`.github/dependabot.yml` is the file Batch B changed under `.github/`, and it is not a workflow,
so actionlint does not read it; the workflow files it does read are unchanged since the Batch A
gate, which is consistent with the identical result.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| Captured stdout recorded verbatim and empty | empty | 0 bytes | PASS |
| Independent enumeration of `.github/workflows/*.yml` | exactly 8 | 8 | PASS |
| Artifact states the 8 is an independent enumeration, not actionlint output | stated | stated above in terms | PASS |

Output Summary: CMD-ACTIONLINT returned EXIT_CODE 0 with **0 bytes** of stdout, which is the
clean-run form: actionlint prints nothing at all when it finds no problem, so no count can be read
from it. The independent filesystem enumeration
`Get-ChildItem .github/workflows -Filter *.yml | Measure-Object` reports **exactly 8** workflow
YAML files, and that 8 is a filesystem enumeration rather than actionlint output. The runner
throws on an absent binary, so exit 0 establishes the tool ran against a non-empty population.
