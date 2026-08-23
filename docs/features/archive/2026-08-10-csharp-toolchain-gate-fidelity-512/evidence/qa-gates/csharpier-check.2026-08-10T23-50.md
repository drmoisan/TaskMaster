# FORMAT-VERIFY verification ([P5-T2], AC1 / AC7)

Timestamp: 2026-08-10T23-50
Command: `./.dotnet-sdk/dotnet.exe tool run csharpier check .`
EXIT_CODE: 0

This is **FORMAT-VERIFY**, the corrected read-only CI-parity form this feature documents.

## Console output

```
Checked 1517 files in 5463ms.
```

**`Checked 1517 files` matches the [P0-T8] baseline count of 1517 exactly.**

## Recorded reconciliation of an intermediate 1518 reading

A first `check` run immediately after [P5-T1] reported `Checked 1518 files`. The one extra file was
identified and attributed before this run:

- The file is the repository-root **`coverage.xml`**, an untracked Pester code-coverage byproduct
  written by the [P0-T16] direct Pester run (`$c.CodeCoverage.OutputPath` defaults to
  `coverage.xml`). It did not exist at [P0-T8] (`git status --porcelain` at [P0-T3] shows it absent)
  and is **not** a repository source file.
- It is untracked and, unlike `artifacts/**` (gitignored, and therefore skipped by CSharpier's
  gitignore handling — the three `artifacts/pester/*.xml` byproducts are not counted), the root
  `coverage.xml` is neither gitignored nor listed in `.csharpierignore`, so CSharpier discovered it.
- It was removed (`rm -f coverage.xml`) as tool-byproduct cleanup, restoring the working tree to its
  pre-run state, and this run then reported the baseline 1517.

Identification command:

```
find . -newermt "2026-08-10 22:44" \( -name '*.cs' -o -name '*.xml' -o -name '*.config' -o -name '*.csx' \) \
  -not -path './packages/*' -not -path './.dotnet-sdk/*' -not -path '*/bin/*' -not -path '*/obj/*' -not -path './.git/*'
-> ./artifacts/pester/pester-junit.xml
   ./artifacts/pester/powershell-coverage.koverage.xml
   ./artifacts/pester/powershell-coverage.xml
   ./coverage.xml
```

The tracked-source population is therefore **unchanged**; the transient +1 was a generated file, not
a source change. [P6-T3] regenerates `coverage.xml`, so it is removed again before [P6-T6].

## Contrast with the documented (defective) form

| Form | Command | EXIT_CODE | Result |
|---|---|---|---|
| Documented at the merge base | `dotnet tool run csharpier .` | **1** ([P0-T7]) | `Required command was not provided.` |
| **Adopted (FORMAT-VERIFY)** | `dotnet tool run csharpier check .` | **0** | `Checked 1517 files in 5463ms.` |

## Output Summary

FORMAT-VERIFY returns `EXIT_CODE: 0` with `Checked 1517 files`, matching the [P0-T8] baseline count
exactly. Together with [P5-T1]'s `EXIT_CODE: 0` for FORMAT-APPLY, this satisfies AC1's
execute-and-record requirement for both documented format forms against the manifest-pinned
CSharpier 1.2.6.
