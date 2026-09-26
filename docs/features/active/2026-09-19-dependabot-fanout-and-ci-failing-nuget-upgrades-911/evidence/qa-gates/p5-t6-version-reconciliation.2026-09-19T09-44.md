# P5-T6 — Version reconciliation implemented in ProjectConsistency.psm1

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = "<execution-worktree-root>\scripts\dependencies\ProjectConsistency.psm1"; Import-Module $p -Force -ErrorAction Stop; ...; Invoke-VersionReconciliation -ProjectText <AC11 fixture> -PackageId "Contoso.Widgets" -ManifestVersion "2.0.0"'
```

EXIT_CODE: 0

## Output Summary

```
IMPORT=ok
Invoke-BindingRedirectReconciliation, Invoke-VersionReconciliation
LINES=362
WORD_SED=0
EXTERNAL=0
EXAMINED=4
REPAIRS=4
LINE> <Import Project="..\packages\Contoso.Widgets.2.0.0\build\Contoso.Widgets.props" Condition="Exists('..\packages\Contoso.Widgets.2.0.0\build\Contoso.Widgets.props')" />
LINE> <Reference Include="Contoso.Widgets, Version=2.0.0, Culture=neutral, processorArchitecture=MSIL">
LINE> <HintPath>..\packages\Contoso.Widgets.2.0.0\lib\net472\Contoso.Widgets.dll</HintPath>
LINE> <Error Condition="!Exists('..\packages\Contoso.Widgets.2.0.0\build\Contoso.Widgets.props')" Text="Missing ..\packages\Contoso.Widgets.2.0.0\build\Contoso.Widgets.props" />
```

The fixture entered the run with four different versions — `<Import>` 1.0.1,
`<Reference>` 1.0.2, `<HintPath>` 1.0.3 and `<Error>` 1.0.4 — and all four now name the
manifest version 2.0.0.

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| Module imports without error | yes | `IMPORT=ok`, run with `-ErrorAction Stop` |
| Exports the reconciliation function | `Invoke-VersionReconciliation` present | present |
| Contains no invocation of `sed` | 0 | `WORD_SED=0` |
| Contains no other external text-substitution executable | 0 | `EXTERNAL=0`, searching `Start-Process`, `awk`, `perl`, `python`, `cmd.exe`, `bash`, `Invoke-Expression` |
| At most 500 lines | <= 500 | 362 |

`WORD_SED` uses the word-anchored pattern `\bsed\b`. An unanchored search for the three
letters is not usable as this gate: it matches `used`, `passed`, `parsed` and `based` in
ordinary prose and returned 4 on a module that invokes nothing.

## Design notes

- Parsing is delegated to `ConvertFrom-ProjectFileText` in `PackageGraph.psm1` rather than
  re-implemented. `scripts/dependencies/PackageGraph.psm1` is imported and is **not**
  edited: it is not registered in Batch C and a write to it would be the fourth production
  file of the batch.
- Every rewrite is a byte-exact replacement performed over the file's own text in
  PowerShell, per gate rule 15. The text is split with `[regex]::Split($text, '(\r?\n)')`,
  which captures the terminators, so every line ending is reassembled exactly as the file
  carried it and a substitution on one line cannot normalise the rest of the file.
- Path separators in the substitution patterns are built from `[char]92` through
  `[regex]::Escape` rather than written as literal doubled backslashes. A doubled backslash
  can be collapsed in transit between an author and the file, which would leave a character
  class that matches nothing while the surrounding code still looks correct. This is the
  same failure mode gate rule 15 records for `sed` through the Bash tool, reaching the
  source file instead of the command line.
- The version segment pattern requires a leading digit. That is what stops a package
  identifier matching a longer sibling: with the identifier
  `Microsoft.Extensions.Configuration`, the folder
  `Microsoft.Extensions.Configuration.Binder.10.0.12` does not match, because the character
  after the identifier and its dot is a letter.
- `<Analyzer Include>` is deliberately **not** reconciled by this function. That item is
  repaired by `AnalyzerItemRepair.psm1` under the preserve rule, which confirms against the
  restored package listing that the existing folder segment still exists before the version
  moves. Rewriting an analyzer item's version from here would bypass that confirmation and
  could emit a path the package does not ship.
- `<Reference>` carries an assembly version rather than a package version, and the two are
  not required to track each other — measured in this repository, `Castle.Core` 5.2.1
  resolves assembly version 5.0.0.0 and `FSharp.Core` 11.0.100 resolves 11.0.0.0. The
  caller therefore supplies the resolved value through `-AssemblyVersion`; the manifest
  version is the fallback only. The rewrite applies only when the Include's simple assembly
  name equals the package identifier, so a package whose assemblies are named differently
  is left alone rather than guessed at.
