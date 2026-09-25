# P1-T4 — `scripts/dependencies/PackageGraph.psm1` created

Timestamp: 2026-09-19T12-31

Command: `pwsh -NoProfile -Command 'Import-Module (Resolve-Path scripts/dependencies/PackageGraph.psm1).Path -Force; Get-Command -Module PackageGraph'`

EXIT_CODE: 0

## What was created

`scripts/dependencies/PackageGraph.psm1`, written with the `Write` tool and edited with the `Edit`
tool, never through a Bash heredoc or redirection, per Scope Decision 4.

Seven exported advanced functions, each carrying `[CmdletBinding()]`, named parameters and
comment-based help:

| Function | Role | Purity |
|---|---|---|
| `Get-PackageManifestPath` | discovers manifest or `app.config` paths from an injected `-DirectoryLister` delegate, filtering by leaf name and discarding candidates under `packages`, `bin`, `obj` or `node_modules` | I/O confined to the injected delegate |
| `ConvertFrom-PackagesConfigText` | parses manifest text into ordered package records carrying `Id`, `Version`, `TargetFramework`, the ordered `Attribute` map and `Index` | pure over text |
| `ConvertTo-PackagesConfigText` | renders package records as a canonical inline manifest document | pure over records |
| `ConvertFrom-ProjectFileText` | parses project-file text into dependent-element records for `Import`, `Error`, `Reference`, `HintPath` and `Analyzer`, each with a one-based line number | pure over text |
| `ConvertFrom-AppConfigText` | parses `app.config` text into binding-redirect records | pure over text |
| `ConvertTo-AppConfigText` | renders `app.config` text in canonical inline form by collapsing reflowed start tags | pure over text |
| `Invoke-ManifestNormalization` | drives discovery, parse and render across a tree through injected lister, reader and writer delegates, and returns examined and changed counts per kind | I/O confined to the injected delegates |

Two private helpers, `ConvertTo-AttributeMap` and `ConvertTo-DependentElementRecord`, are not
exported and are therefore not named in the module help.

`Invoke-ManifestNormalization` declares `SupportsShouldProcess` and guards its write through
`$PSCmdlet.ShouldProcess`, per the state-changing-action rule in `.claude/rules/powershell.md`.

## Canonical form this module renders

Confirmed against `SVGControl/packages.config`, which is already in that form: one element per
line, attributes separated by a single space in their recorded order, a space before the
self-closing slash, two-space indentation, CRLF line endings and a trailing CRLF.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| The module imports without error | `Import-Module ... -Force -ErrorAction Stop` completed; no output on the error stream | PASS |
| `Get-Command -Module PackageGraph` lists every exported function named in the module's own comment-based help | both sets have **7** members and `Compare-Object` between them returns 0 differences | PASS |
| The file is at most 500 lines | **465** | PASS |

`Get-Command -Module PackageGraph`, sorted:

```
ConvertFrom-AppConfigText
ConvertFrom-PackagesConfigText
ConvertFrom-ProjectFileText
ConvertTo-AppConfigText
ConvertTo-PackagesConfigText
Get-PackageManifestPath
Invoke-ManifestNormalization
```

The `Exported functions:` list in the module header block names exactly the same seven.

## Byte-exactness observation, per gate rule 14

The module performs its separator normalisation with `[char]92` and `[char]47` rather than with
escaped literals, because the Bash tool collapses doubled backslashes on the way to a payload. The
written file contains **1** `[char]92` occurrence and **13** raw backslash characters, all of them
regex metacharacters such as the word-boundary and whitespace classes, none of them a path
separator. That was measured after the file was written rather than assumed from the source text.

## Smoke observation (not an acceptance clause)

Exercised against the real tree to confirm the renderer behaves as the later tasks require:

- `SVGControl/packages.config` round-trips **byte-identical**, so an already-canonical file is left
  untouched by P1-T7.
- `UtilitiesCS/packages.config` and `UtilitiesCS/app.config` both change, and both renderers are
  idempotent on their own output.

Output Summary: `scripts/dependencies/PackageGraph.psm1` is created at 465 lines and imports
cleanly. It exports seven advanced functions, and the set `Get-Command -Module PackageGraph`
returns is identical to the set the module's own comment-based help names, with zero differences.
Discovery and normalisation reach the filesystem only through injected delegates, so the module is
exercisable in memory with no temporary file.
