# FORMAT-APPLY verification ([P5-T1], AC1 / AC7)

Timestamp: 2026-08-10T23-48
Command: `./.dotnet-sdk/dotnet.exe tool run csharpier format .`
EXIT_CODE: 0

This is **FORMAT-APPLY**, the corrected apply form this feature documents at `CLAUDE.md` § C#1
item 1, § CUT3 step 1, § "C# Toolchain (run in this exact order)" step 1,
`.claude/rules/csharp.md` § Toolchain item 1, and
`.claude/skills/csharp-qa-gate/SKILL.md` § Toolchain Execution Sequence step 1.

## Console output

```
Formatted 1518 files in 6233ms.
```

## Post-run working-tree check

```
$ git status --porcelain -- '*.cs'
(empty)
```

**No `*.cs` file was modified.** The tree was format-clean at [P0-T8] (`Checked 1517 files`,
`EXIT_CODE: 0`) and this feature edits no `*.cs` file, so an empty `*.cs` status is the expected
result. No unexpected reformat occurred and no revert was required.

Note the file-count difference between this `format` run (1518) and the [P0-T8] `check` run (1517).
The extra file was identified as the repository-root **`coverage.xml`**, an untracked Pester
code-coverage byproduct written by the [P0-T16] direct Pester run
(`$c.CodeCoverage.OutputPath` defaults to `coverage.xml`). It is not a repository source file, it did
not exist at [P0-T8], and it is neither gitignored nor listed in `.csharpierignore`, so CSharpier
discovered it. It was removed as tool-byproduct cleanup before [P5-T2], which then reported the
baseline 1517. Full attribution is recorded in
`FEATURE/evidence/qa-gates/csharpier-check.2026-08-10T23-50.md`. The tracked-source population is
unchanged.

## Contrast with the documented (defective) form

| Form | Command | EXIT_CODE | Effect |
|---|---|---|---|
| Documented at the merge base | `dotnet tool run csharpier .` | **1** ([P0-T7]) | rejected; formats nothing |
| **Adopted (FORMAT-APPLY)** | `dotnet tool run csharpier format .` | **0** | formats 1518 files |

## Output Summary

FORMAT-APPLY runs successfully against the pinned CSharpier 1.2.6 with `EXIT_CODE: 0`, formatting
1518 files in 6.2 s. `git status --porcelain -- '*.cs'` is empty afterwards, confirming the tree
required no reformatting and that this feature introduced no `*.cs` change. This satisfies AC1's
execute-and-record requirement for the apply form.
