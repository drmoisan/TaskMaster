# Toolchain Gate for the Branch Diff (P4-T3) — discharges AC-15

- **Issue:** #635
- **Plan task:** [P4-T3]

Timestamp: 2026-08-29T06-41

## Output Summary

The branch diff contains Markdown only. No path outside this item's feature folder carries a C# source,
project, resource, configuration, or PowerShell extension, so branch two of this task applies: the C#
gates and the PowerShell gates have no in-scope file and are recorded as not applicable with that
reason. This task runs no command and records no command output.

TOOLCHAIN_BRANCH: 2

## The branch condition

The value branched on is the `LANGUAGE_COMPOSITION:` line recorded by [P4-T2], reproduced verbatim:

```
LANGUAGE_COMPOSITION: Markdown only. All 28 union paths carry the `.md` extension; no path carries any other extension; no source, project, resource, configuration, or PowerShell extension is present.
```

The [P4-T2] artifact is at
`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/p4-t2-no-modification-proof.2026-08-29T04-55.md`.

Branch one applies when the [P4-T2] union lists any path outside
`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/` whose extension is
`.cs`, `.csproj`, `.props`, `.targets`, `.resx`, `.config`, `.settings` or `.xaml`, or, for the
PowerShell gates, a `.ps1` path outside that folder. The [P4-T2] union lists 28 paths. Twenty-two are
inside the feature folder. The remaining six are `.claude/agent-memory/atomic-planner/MEMORY.md`,
`.claude/agent-memory/atomic-planner/project_635_reflective_caller_audit_plan_seams.md`,
`.claude/agent-memory/orchestrator/MEMORY.md`,
`.claude/agent-memory/orchestrator/pwsh-double-quoted-command-refused-in-worktree.md`,
`.claude/agent-memory/task-researcher/MEMORY.md` and
`.claude/agent-memory/task-researcher/project_reflective_caller_closure_635.md`. All six carry the `.md`
extension, which is in neither the C# extension set nor the PowerShell extension set.

The [P4-T2] union therefore lists no path that would force branch one, and branch two is taken. The
branch recorded here matches the [P4-T2] union.

## Branch two record

CSHARP_GATE: NOT APPLICABLE

Reason: no in-scope file. The C# gates — the CSharpier format command, the analyzer rebuild, the
nullable rebuild, and the test run — take C# source and build-input files as their input, and the branch
diff contains none outside this item's feature folder. This is not a skip of a gate that had input; it
is the recorded absence of input. Evidence:
`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/p4-t2-no-modification-proof.2026-08-29T04-55.md`,
whose two assertions establish that no union path carries a `.cs`, `.csproj`, `.props`, `.targets`,
`.resx`, `.config`, `.settings` or `.xaml` extension.

POWERSHELL_GATE: NOT APPLICABLE

Reason: no in-scope file. The PowerShell gates take `.ps1` files as their input, and the branch diff
contains none. Evidence:
`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/p4-t2-no-modification-proof.2026-08-29T04-55.md`,
whose extension grouping records 28 `.md` paths and zero paths of any other extension, so no `.ps1`
path is present either inside or outside the feature folder.

## Why this gate can fail

This gate is not a bare skip and could have failed. A source-extension path outside the feature folder
in the [P4-T2] union would have forced branch one, and a branch-one toolchain failure would have failed
this task. The branch taken is determined mechanically by the recorded `LANGUAGE_COMPOSITION:` value and
by the enumerated union paths, both of which are fixed before this task runs and neither of which this
task can alter.

The condition is decidable from the [P4-T2] artifact alone: a third party reading its two assertion
tables reaches the same branch.

## Coverage

No coverage command is run and no coverage artifact is emitted by either branch, because no executable
line changes in this item. The specification records the same conclusion: coverage is unchanged because
no production or test code is touched, and the item cannot reduce coverage for any changed line, because
no executable line changes.
