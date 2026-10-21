# Phase 0 — Analyzer version concordance ([P0-T5])

Timestamp: 2026-09-01T21-53

Two independently formulated searches were run. Search one is keyed on the analyzer reference path inside
`*.csproj` files. Search two is keyed on the package identifier inside `packages.config` files, which is a
different file type and a different anchor. Both patterns spell the backslash as `\x5C` and the double
quote as `\x22`; those hex escapes are interpreted by the .NET regular-expression engine and are inert to
every intervening quoting layer.

## Search one — project-file side, keyed on the analyzer reference path

Command: `pwsh -NoProfile -Command 'Get-ChildItem -Path . -Recurse -Filter *.csproj | Select-String -Pattern "packages\x5C(Meziantou\.Analyzer|Roslynator\.Analyzers)\.[0-9][0-9.]*\x5C" -AllMatches'`

EXIT_CODE: 0

Matched version strings, recorded verbatim per project. Every `Meziantou.Analyzer` match in every file
carries the path segment `packages\Meziantou.Analyzer.3.0.194\`, and every `Roslynator.Analyzers` match in
every file carries the path segment `packages\Roslynator.Analyzers.5.0.0\`. The projects matched, in the
order the search reported them:

| Project file | `Meziantou.Analyzer` version segment | `Roslynator.Analyzers` version segment |
|---|---|---|
| `QuickFiler/QuickFiler.csproj` (lines 3, 581, 588, 589-592) | `3.0.194` | `5.0.0` |
| `QuickFiler.Test/QuickFiler.Test.csproj` (lines 3, 494, 503, 504-507) | `3.0.194` | `5.0.0` |
| `Tags/Tags.csproj` (lines 3, 97, 98-101, 112) | `3.0.194` | `5.0.0` |
| `Tags.Test/Tags.Test.csproj` (lines 3, 303, 312, 313-316) | `3.0.194` | `5.0.0` |
| `TaskMaster/TaskMaster.csproj` (lines 2, 566, 571, 572-575) | `3.0.194` | `5.0.0` |
| `TaskMaster.Test/TaskMaster.Test.csproj` (lines 3, 368, 377, 378-381) | `3.0.194` | `5.0.0` |
| `TaskTree/TaskTree.csproj` (lines 3, 100, 101-104, 115) | `3.0.194` | `5.0.0` |
| `TaskTree.Test/TaskTree.Test.csproj` (lines 3, 304, 313, 314-317) | `3.0.194` | `5.0.0` |
| `TaskVisualization/TaskVisualization.csproj` (lines 3, 150, 151-154, 165) | `3.0.194` | `5.0.0` |
| `TaskVisualization.Test/TaskVisualization.Test.csproj` (lines 3, 328, 337, 338-341) | `3.0.194` | `5.0.0` |
| `ToDoModel/ToDoModel.csproj` (lines 3, 185, 189, 190-193) | `3.0.194` | `5.0.0` |
| `ToDoModel.Test/ToDoModel.Test.csproj` (lines 3, 346, 355, 356-359) | `3.0.194` | `5.0.0` |
| `UtilitiesCS/UtilitiesCS.csproj` (lines 3, 1292, 1301, 1302-1305) | `3.0.194` | `5.0.0` |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (lines 3, 951, 960, 961-964) | `3.0.194` | `5.0.0` |
| `VBFunctions/VBFunctions.csproj` (lines 3, 58, 59-62, 73) | `3.0.194` | `5.0.0` |
| `VBFunctions.Test/VBFunctions.Test.csproj` (lines 3, 283, 292, 293-296) | `3.0.194` | `5.0.0` |

Search-one version set for `Meziantou.Analyzer`: **{ 3.0.194 }**
Search-one version set for `Roslynator.Analyzers`: **{ 5.0.0 }**
Search-one project count: 16.

## Search two — package-pin side, keyed on the package identifier

Command: `pwsh -NoProfile -Command 'Get-ChildItem -Path . -Recurse -Filter packages.config | Select-String -Pattern "id=\x22(Meziantou\.Analyzer|Roslynator\.Analyzers)\x22" -Context 0,2'`

EXIT_CODE: 0

The `-Context 0,2` window shows the `version=` attribute on the line immediately following each matched
`id=` line. Verbatim readings:

| `packages.config` | `Meziantou.Analyzer` (id line : version) | `Roslynator.Analyzers` (id line : version) |
|---|---|---|
| `QuickFiler/packages.config` | 12 : `version="3.0.194"` | 34 : `version="5.0.0"` |
| `QuickFiler.Test/packages.config` | 12 : `version="3.0.194"` | 140 : `version="5.0.0"` |
| `Tags/packages.config` | 7 : `version="3.0.194"` | 19 : `version="5.0.0"` |
| `Tags.Test/packages.config` | 9 : `version="3.0.194"` | 131 : `version="5.0.0"` |
| `TaskMaster/packages.config` | 9 : `version="3.0.194"` | 31 : `version="5.0.0"` |
| `TaskMaster.Test/packages.config` | 11 : `version="3.0.194"` | 134 : `version="5.0.0"` |
| `TaskTree/packages.config` | 7 : `version="3.0.194"` | 20 : `version="5.0.0"` |
| `TaskTree.Test/packages.config` | 9 : `version="3.0.194"` | 131 : `version="5.0.0"` |
| `TaskVisualization/packages.config` | 7 : `version="3.0.194"` | 20 : `version="5.0.0"` |
| `TaskVisualization.Test/packages.config` | 9 : `version="3.0.194"` | 131 : `version="5.0.0"` |
| `ToDoModel/packages.config` | 9 : `version="3.0.194"` | 26 : `version="5.0.0"` |
| `ToDoModel.Test/packages.config` | 9 : `version="3.0.194"` | 133 : `version="5.0.0"` |
| `UtilitiesCS/packages.config` | 18 : `version="3.0.194"` | 108 : `version="5.0.0"` |
| `UtilitiesCS.Test/packages.config` | 13 : `version="3.0.194"` | 169 : `version="5.0.0"` |
| `VBFunctions/packages.config` | 5 : `version="3.0.194"` | 17 : `version="5.0.0"` |
| `VBFunctions.Test/packages.config` | 7 : `version="3.0.194"` | 128 : `version="5.0.0"` |

Every matched entry additionally carries `targetFramework="net481"` on the second context line.

Search-two version set for `Meziantou.Analyzer`: **{ 3.0.194 }**
Search-two version set for `Roslynator.Analyzers`: **{ 5.0.0 }**
Search-two project count: 16.

## Per-package equality statement

| Package identifier | Search-one version set | Search-two version set | Sets equal? |
|---|---|---|---|
| `Meziantou.Analyzer` | { 3.0.194 } | { 3.0.194 } | **YES** |
| `Roslynator.Analyzers` | { 5.0.0 } | { 5.0.0 } | **YES** |

No version is present on one side and absent from the other, for either package identifier. There is no
recorded disagreement, so there is no blocking finding to resolve before `[P0-T9]`.

Output Summary: Two independently formulated searches, over two different file types and with two
different anchors, both enumerate `Meziantou.Analyzer` at exactly one version, 3.0.194, and
`Roslynator.Analyzers` at exactly one version, 5.0.0, across the same 16 projects. The two version sets
are equal per package identifier. Analyzer version concordance holds; `[P0-T9]` is not blocked.
