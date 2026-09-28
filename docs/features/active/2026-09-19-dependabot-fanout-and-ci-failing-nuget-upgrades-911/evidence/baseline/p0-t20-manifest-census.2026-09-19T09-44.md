# P0-T20 — Manifest Census

Timestamp: 2026-09-19T23-12

Commands:

```
git ls-files -- "*packages.config"
git ls-files -- "*/app.config"
git grep -n "HintPath" -- "ToDoModel.Test/ToDoModel.Test.csproj"
git grep -c "Deedle" -- "ToDoModel.Test/packages.config"
git grep -c "FSharp.Core" -- "ToDoModel.Test/packages.config"
(Get-Content "ToDoModel.Test/packages.config").Count
```

EXIT_CODE: 0

## 1. `packages.config` count

**18.**

```
QuickFiler.Test/packages.config
QuickFiler/packages.config
SVGControl.Test/packages.config
SVGControl/packages.config
Tags.Test/packages.config
Tags/packages.config
TaskMaster.Test/packages.config
TaskMaster/packages.config
TaskTree.Test/packages.config
TaskTree/packages.config
TaskVisualization.Test/packages.config
TaskVisualization/packages.config
ToDoModel.Test/packages.config
ToDoModel/packages.config
UtilitiesCS.Test/packages.config
UtilitiesCS/packages.config
VBFunctions.Test/packages.config
VBFunctions/packages.config
```

## 2. `app.config` count

**17.**

```
QuickFiler.Test/app.config
QuickFiler/app.config
SVGControl.Test/app.config
SVGControl/app.config
Tags.Test/app.config
Tags/app.config
TaskMaster.Test/app.config
TaskMaster/app.config
TaskTree.Test/app.config
TaskTree/app.config
TaskVisualization.Test/app.config
TaskVisualization/app.config
ToDoModel.Test/app.config
ToDoModel/app.config
UtilitiesCS.Test/app.config
UtilitiesCS/app.config
VBFunctions.Test/app.config
```

`VBFunctions/` has no `app.config`, which is why 17 projects carry one against 18 carrying a
manifest. 18 plus 17 is the 35-member set P1-T7 normalises.

## 3. The #903 orphan pair — `ToDoModel.Test/ToDoModel.Test.csproj`

Verbatim `<HintPath>` lines with their line numbers:

```
ToDoModel.Test/ToDoModel.Test.csproj:93:      <HintPath>..\packages\Deedle.3.0.0\lib\netstandard2.0\Deedle.dll</HintPath>
ToDoModel.Test/ToDoModel.Test.csproj:96:      <HintPath>..\packages\FSharp.Core.11.0.100\lib\netstandard2.0\FSharp.Core.dll</HintPath>
```

**Exactly 2 orphan `<HintPath>` lines, at lines 93 and 96.**

The package folder segments these two lines name are `Deedle.3.0.0` and `FSharp.Core.11.0.100`.
P1-T11 asserts that the two version literals it writes into the manifest equal the version parts of
those segments, `3.0.0` and `11.0.100`, read from these two lines.

## 4. Matching entries in `ToDoModel.Test/packages.config`

| Package | Matches in the manifest |
|---|---|
| `Deedle` | **0** |
| `FSharp.Core` | **0** |

Both `git grep -c` invocations printed no output line at all, which is the zero-match result for a
file-scoped count.

**Manifest total line count: 172.** Measured as `(Get-Content "ToDoModel.Test/packages.config").Count`.
The file ends with a newline, so a newline-counting measurement returns the same 172; the two
methods agree and either can be used at P1-T11 without changing the comparison. This is the pre-edit
figure P1-T11 compares against: after the edit the count must be exactly **174**.

## Non-vacuity

The two zeroes in section 4 are the absence half of the #903 defect, and gate rule 2 prohibits an
absence assertion standing alone. Their guard is the positive count in section 3: the project file
carries exactly 2 `<HintPath>` lines naming these packages, recorded verbatim with line numbers, so
a search that resolved no file or matched no pattern would have produced 0 there too and would have
failed. The pairing establishes that the two packages are referenced by the build and undeclared by
the manifest, which is defect #903, rather than simply absent from the project.

The 172-line count is the second positive guard: it makes P1-T11's post-edit assertion falsifiable
against a specific integer, so a reflowed multi-line insertion — which would add more than 2 lines —
fails rather than passes.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| `**/packages.config` count | 18 | 18 | PASS |
| `*/app.config` count | 17 | 17 | PASS |
| Orphan `<HintPath>` lines recorded with line numbers | exactly 2 | 2, at lines 93 and 96, verbatim | PASS |
| Manifest matches for `Deedle` and `FSharp.Core` | exactly 0 | 0 and 0 | PASS |
| Manifest line count recorded as an integer | required | 172 | PASS |

Output Summary: the repository carries **18** `packages.config` manifests and **17** `app.config`
files, 35 files in total, which is the set P1-T7 normalises. `ToDoModel.Test/ToDoModel.Test.csproj`
carries exactly **2** orphan `<HintPath>` lines, at lines **93** and **96**, naming the package
folder segments `Deedle.3.0.0` and `FSharp.Core.11.0.100`, while
`ToDoModel.Test/packages.config` declares **0** matches for `Deedle` and **0** for `FSharp.Core` —
defect #903. That manifest is **172** lines, the pre-edit figure P1-T11 compares against for its
plus-exactly-2 assertion.
