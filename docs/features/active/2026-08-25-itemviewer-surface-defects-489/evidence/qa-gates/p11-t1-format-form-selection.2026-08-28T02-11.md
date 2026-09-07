# P11-T1 — Format-invocation selection for the Phase 11 loop

Timestamp: 2026-08-28T02-11
Command: (reads the branch recorded by P0-T10 in
`evidence/baseline/phase0-u1-designer-format-gate.2026-08-27T23-22.md`; no new command is required by
this task)
EXIT_CODE: 0

## Branch read from P0-T10: A

`evidence/baseline/phase0-u1-designer-format-gate.2026-08-27T23-22.md` records, under the heading
"Branch selected: A", the sentence "Phase 11 runs the policy form `dotnet tool run csharpier format .`"
and states explicitly that "Branch B is **not** selected". Exactly one branch is named there, and it
is `A`.

## Form selected for P11-T2 — exactly one

```
dotnet tool run csharpier format .
```

This is the policy command from `CLAUDE.md` § C# Toolchain step 1, invoked through `dotnet tool run`
so the manifest-pinned CSharpier 1.2.6 is used, and run repo-wide from the worktree root.

No second form is named. The Branch B alternative — `dotnet tool run csharpier format` followed by an
explicit file list — is **not** selected and is not used by P11-T2.

## Why Branch A is the selected branch, restated from the recorded evidence

P0-T10's branch rule selects Branch A when and only when the `BaselineUnformattedSet:` block recorded
by P0-T9 is empty. `evidence/baseline/phase0-csharpier-check.2026-08-27T23-21.md` records that block
as empty, with the run reporting `Checked 1543 files in 4729ms.` and listing no unformatted file.

The hazard Branch B exists to avoid does not arise under an empty baseline set. A repo-wide mutating
`format .` rewrites only files CSharpier considers unformatted; with the baseline set empty there is
no 488-owned or 501-owned file for it to rewrite, and no already-unformatted generated file for it to
reflow. The two `*.Designer.cs` files this feature edits are additionally not processed at all:
P0-T9 recorded `Checked 0 files` for each on a single-file check, which is CSharpier 1.2.6's
generated-file detection acting on the filename. The deletions-only diff that AC16 requires is
therefore not at risk from the repo-wide form.

## Scope guard that P11-T2 applies regardless of branch

P11-T2 runs `git status --porcelain` over the full nineteen-directory C# project set immediately
after the format pass and requires that it list no path outside the P10-T2 scope list. That guard is
what detects a rewrite outside this feature's 25 permitted paths; it is unaffected by which form is
selected here.

Output Summary: The branch recorded by P0-T10 is **A**, so exactly one format form is selected for
P11-T2: the repo-wide policy command `dotnet tool run csharpier format .`. Branch B and its explicit
file-list form are not selected. The selection is a direct read of the P0-T10 artifact, which names
Branch A on the strength of P0-T9's empty `BaselineUnformattedSet:` over 1543 checked files; no new
command was run by this task.
