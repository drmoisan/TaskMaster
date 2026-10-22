# Phase 0 — Research Open Item U1 Answer and Format-Invocation Decision (P0-T10)

Timestamp: 2026-08-27T23-22
Command: (reads the `BaselineUnformattedSet:` block recorded by P0-T9; no new command is required by
this task. The supporting single-file evidence quoted below was captured under P0-T9.)
EXIT_CODE: 0

## Branch selected: A

**Branch A.** Phase 11 runs the policy form `dotnet tool run csharpier format .`.

Exactly one branch is named. Branch B is **not** selected.

## U1 answer

Research open item U1 asks whether CSharpier skips `*.Designer.cs` by filename. **It does.**

- `dotnet tool run csharpier check .` on the untouched worktree reported `Checked 1543 files` and
  listed **no** unformatted file, so the `BaselineUnformattedSet:` block recorded by P0-T9 is
  **empty**. P0-T10's branch rule selects Branch A when and only when that block is empty, so Branch A
  is selected.
- The empty set alone would leave U1 ambiguous between "the Designer files are processed and already
  formatted" and "the Designer files are not processed". P0-T9 resolved that ambiguity with two
  read-only single-file checks: both `QuickFiler/Viewers/ItemViewer.Designer.cs` and
  `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` reported `Checked 0 files`, not `Checked 1 files`.
  CSharpier declined to process either file.
- `.csharpierignore` is 14 lines and its eight exclusion patterns are `**/evidence/**`,
  `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`, `*.targets`. It does
  **not** exclude `*.Designer.cs`, and no `.csharpierrc` exists in the repository, so the print width is
  the 100-column default. `ItemViewer.Designer.cs:256` measures 111 columns and
  `ItemViewerExpanded.Designer.cs:274` measures 110 columns; each would have been re-wrapped had its
  file been processed. The skip is therefore CSharpier 1.2.6's built-in generated-file detection acting
  on the filename, not `.csharpierignore` and not a coincidence of existing formatting.

## Consequence for the risk this gate exists to control

The risk recorded in `spec.md` § Risks & Mitigations is that a one-line `.Designer.cs` edit triggers a
whole-file CSharpier reformat of a 6224-line generated file, making the diff unreviewable and
unattributable. Under the measured U1 answer that risk does not materialize: CSharpier will not
process either `.Designer.cs` file at all, whether invoked repo-wide as `format .` or otherwise. The
`git diff --stat` acceptance conditions on those two files (spec AC15) can therefore be satisfied by a
single deleted line each.

## Designer-edit status at this point

**No `*.Designer.cs` edit has occurred up to this point.** Verified:
`git diff --name-only 69e8317152c0a9ee6ee6e65db0ef81f6906189b1 -- 'QuickFiler/**/*.Designer.cs'`
returns no path. The complete diff against the branch base at this moment is two files, both under
`docs/`: this feature's `plan.2026-08-25T01-04.md` and its
`evidence/baseline/phase0-instructions-read.2026-08-27T23-16.md`. No production, test, or project file
has been edited in Phase 0.

Output Summary: Branch **A** is selected — Phase 11 runs the repo-wide policy form
`dotnet tool run csharpier format .` — because the P0-T9 `BaselineUnformattedSet:` block is empty.
Research open item U1 is answered **yes**: CSharpier 1.2.6 skips `*.Designer.cs` by filename through
its generated-file detection, proved by a `Checked 0 files` result on each file individually against a
111-column and a 110-column line that a 100-column print width would otherwise re-wrap. No
`*.Designer.cs` edit has occurred up to this point. This task is the hard gate on P2-T3, P4-T2 and
P4-T4, and it is now recorded.
