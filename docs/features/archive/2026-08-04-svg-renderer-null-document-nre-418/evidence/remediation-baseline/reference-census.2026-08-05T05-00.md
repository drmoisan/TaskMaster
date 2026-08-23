# Mechanism Census — Reference and Output-Directory State Before the Fix

- Task: `[P0-T9]`
- Timestamp: 2026-08-04T23-40
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- No build was run and no file was edited by this task. Every figure below was re-measured at the point
  of writing this artifact rather than transcribed from plan prose.
- EXIT_CODE: 0 (all census commands returned 0 except the two deliberate not-found probes, noted inline)

## (a) `SVGControl.Test/SVGControl.Test.csproj` — `Svg` present, `ExCSS` absent

```
Command: grep -n -i 'Reference Include="Svg\|Reference Include="ExCSS\|excss' SVGControl.Test/SVGControl.Test.csproj
EXIT_CODE: 0
Output:
282:    <Reference Include="Svg, Version=3.4.0.0, Culture=neutral, PublicKeyToken=12a0bac221edeae2, processorArchitecture=MSIL">
```

**CONFIRMED.** Exactly one match: the `Svg` `<Reference>` at line 282. The case-insensitive pattern
included the bare token `excss`, so its zero matches establishes that the string `ExCSS` does not occur
anywhere in the project file in any casing — there is **no** `ExCSS` `<Reference>`.

## (b) `SVGControl.Test/packages.config` — `Svg` present, `ExCSS` absent

```
Command: grep -n -i 'id="Svg"\|id="ExCSS"\|excss' SVGControl.Test/packages.config
EXIT_CODE: 0
Output:
116:  <package id="Svg" version="3.4.8" targetFramework="net481" />
```

**CONFIRMED.** Exactly one match: the `Svg` entry at line 116, `version="3.4.8"`,
`targetFramework="net481"`. Zero matches for `ExCSS` in any casing.

## (c) `SVGControl.Test/bin/Debug` — `Svg.dll` present; `ExCSS.dll` and `Fizzler.dll` absent

```
Command: ls -1 SVGControl.Test/bin/Debug/ | grep -i -E 'svg|excss|fizzler'
EXIT_CODE: 0
Output:
SVGControl.Test.dll
SVGControl.Test.dll.config
SVGControl.Test.pdb
SVGControl.dll
SVGControl.dll.config
SVGControl.pdb
Svg.dll
Svg.xml
```

Explicit per-file existence test:

```
PRESENT: Svg.dll
ABSENT:  ExCSS.dll
ABSENT:  Fizzler.dll
```

**CONFIRMED.** `Svg.dll` is in the output; `ExCSS.dll` and `Fizzler.dll` are not. This is the mechanism
directly: `Svg.dll`'s manifest depends on `ExCSS`, but the dependency was never copied, so the test
host's probing of this directory cannot find it — which is exactly the `FileNotFoundException` recorded
verbatim in `order-standalone.2026-08-05T05-00.md`.

## (d) Repository-wide globs across test-project outputs

### `*.Test/bin/Debug/ExCSS.dll` → **8** files

```
Command: ls -1 *.Test/bin/Debug/ExCSS.dll
EXIT_CODE: 0
Output:
QuickFiler.Test/bin/Debug/ExCSS.dll
Tags.Test/bin/Debug/ExCSS.dll
TaskMaster.Test/bin/Debug/ExCSS.dll
TaskTree.Test/bin/Debug/ExCSS.dll
TaskVisualization.Test/bin/Debug/ExCSS.dll
ToDoModel.Test/bin/Debug/ExCSS.dll
UtilitiesCS.Test/bin/Debug/ExCSS.dll
VBFunctions.Test/bin/Debug/ExCSS.dll
count: 8
```

### `*.Test/bin/Debug/Fizzler.dll` → **0** files

```
Command: ls -1 *.Test/bin/Debug/Fizzler.dll
EXIT_CODE: 2   (deliberate not-found probe)
Output:  ls: cannot access '*.Test/bin/Debug/Fizzler.dll': No such file or directory
count: 0
```

### Two stated conclusions, both verified

**`SVGControl.Test` is the only one of the nine test projects whose output lacks `ExCSS.dll`.** Verified
by counting the test projects rather than assuming the count:

```
Command: git ls-files '*.Test/*.csproj'
EXIT_CODE: 0
Output:
QuickFiler.Test/QuickFiler.Test.csproj
SVGControl.Test/SVGControl.Test.csproj
Tags.Test/Tags.Test.csproj
TaskMaster.Test/TaskMaster.Test.csproj
TaskTree.Test/TaskTree.Test.csproj
TaskVisualization.Test/TaskVisualization.Test.csproj
ToDoModel.Test/ToDoModel.Test.csproj
UtilitiesCS.Test/UtilitiesCS.Test.csproj
VBFunctions.Test/VBFunctions.Test.csproj
count: 9
```

Nine tracked test projects. The eight `ExCSS.dll` outputs enumerated above are exactly the eight
projects in that list other than `SVGControl.Test`. **`SVGControl.Test` is the sole exception.**

**No test project's output contains `Fizzler.dll`.** The glob returns zero across all ten `*.Test`
directories, so adding a `Fizzler` reference would make `SVGControl.Test` the **only** test project
carrying it — divergence from the siblings, not parity with them. This is ground 2 of Design
Decision 3, re-measured and confirmed.

### Why the glob returns 8 and not 9 — this must not be read as an off-by-one

Ten directories match `*.Test`:

```
Command: ls -1d *.Test
QuickFiler.Test/    SVGControl.Test/    Tags.Test/    TaskMaster.Test/    TaskTree.Test/
TaskVisualization.Test/    ToDoModel.Test/    UtilitiesCS.Test/    UtilitiesSwordfish.Test/    VBFunctions.Test/
count: 10
```

`UtilitiesSwordfish.Test` is **not a test project at all** — it is stale, wholly untracked build output.
Four independent grounds, each re-measured here:

1. **Zero tracked files.**
   ```
   Command: git ls-files UtilitiesSwordfish.Test | wc -l
   Output:  0
   ```
2. **No `*Swordfish*` project file exists in the repository outside `packages/` and `.claude/`.**
   ```
   Command: git ls-files | grep -i swordfish | grep -cE '\.(csproj|vbproj|sln)$'
   Output:  0
   Command: find . -maxdepth 3 -iname '*swordfish*.csproj' -not -path './packages/*' -not -path './.claude/*'
   Output:  (no matches — tested untracked files too, not only tracked ones)
   ```
   The teardown commit the plan names was verified to exist rather than transcribed:
   ```
   Command: git log --oneline --all --grep='tear down vendored UtilitiesSwordfish'
   Output:  bafeae70 Merge PR #318: F5 tear down vendored UtilitiesSwordfish structural surface (#308)
            0ec111b2 refactor(swordfish): tear down vendored UtilitiesSwordfish structural surface (#308)
   ```
   Commit `0ec111b2` carries the exact title the plan cites.
3. **Its `bin/Debug` holds no `*.Test.dll`, so no runner can discover it.**
   ```
   Command: ls -1 UtilitiesSwordfish.Test/bin/Debug/
   Output:  Newtonsoft.Json.dll   Swordfish.NET.General.dll   Swordfish.NET.General.pdb
            Swordfish.NET.Test.exe   Swordfish.NET.Test.exe.config   Swordfish.NET.Test.pdb
   Command: ls -1 UtilitiesSwordfish.Test/bin/Debug/*.Test.dll
   EXIT_CODE: 2   (deliberate not-found probe)
   Output:  No such file or directory
   ```
   It contains `Swordfish.NET.Test.exe` and no `*.Test.dll`. Neither `vstest.console.exe` nor
   `Invoke-MSTestWithCoverage.ps1` (which filters on `*.Test.dll`) can discover it.
4. **It is absent from the solution.**
   ```
   Command: grep -c -i swordfish TaskMaster.sln
   Output:  0
   ```

**Arithmetic, stated so the count cannot be misread:** 8 sibling outputs carrying `ExCSS.dll` +
`SVGControl.Test` (lacking it) = **9**, the nine assemblies the coverage run discovers. The tenth
directory is not a project. `[P2-T7]` expects an assembly count of 9 on that basis.

## (e) The `ExCSS` package asset on disk

```
Command: ls -la packages/ExCSS.4.3.2/lib/net48/ExCSS.dll
EXIT_CODE: 0
Output:  -rwxr-xr-x 1 DanMoisan 197121 368128 Jul 23 19:21 packages/ExCSS.4.3.2/lib/net48/ExCSS.dll
```

**The `HintPath` target exists**, 368128 bytes.

```
Command: ls -1 packages/ExCSS.4.3.2/lib/
Output:  net10.0/  net48/  net6.0/  net7.0/  net8.0/  netcoreapp3.1/  netstandard2.0/  netstandard2.1/
Command: [ -d "packages/ExCSS.4.3.2/lib/net481" ] && echo PRESENT || echo ABSENT
Output:  ABSENT
```

**`ExCSS.4.3.2` has no `lib\net481` folder.** Its .NET Framework asset is `lib\net48`. `net48` is
therefore the correct and only available choice for this `v4.8.1` project, and it matches the
production precedent in `SVGControl/SVGControl.csproj`. This also forecloses the one silent-rewrite
risk `[P1-T4]` checks for: `Sync-PackageReferences.ps1` rewrites a `HintPath` only when it fails to
resolve, and this one resolves.

```
Command: ls -1d packages/ExCSS.*
Output:  packages/ExCSS.4.3.2/
```

Exactly one `ExCSS` package directory on disk, confirming `4.3.2` as the version for `[P1-T2]`.

## Identity source for `[P1-T1]`, quoted verbatim

From `SVGControl/SVGControl.csproj`, lines 54-60 as read (the `ExCSS` block begins at line 55 and its
`HintPath` is line 56, matching the plan's citation):

```xml
  <ItemGroup>
    <Reference Include="ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL">
      <HintPath>..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll</HintPath>
    </Reference>
    <Reference Include="Fizzler, Version=1.3.1.0, Culture=neutral, PublicKeyToken=4ebff4844e382110, processorArchitecture=MSIL">
      <HintPath>..\packages\Fizzler.1.3.1\lib\netstandard2.0\Fizzler.dll</HintPath>
    </Reference>
```

The `Include` string `[P1-T1]` must reproduce byte-for-byte is therefore:

```
ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL
```

Corroborating package version, `SVGControl/packages.config:3`:

```xml
  <package id="ExCSS" version="4.3.2" targetFramework="net481" />
```

## Supplementary measurement supporting Design Decision 3 (no `Fizzler`)

Grounds 1 and 4 of Design Decision 3, re-measured here rather than transcribed:

```
Command: git ls-files '*.csproj' | xargs grep -n 'Reference Include="Fizzler'
Output:
SVGControl/SVGControl.csproj:58:    <Reference Include="Fizzler, Version=1.3.1.0, ... PublicKeyToken=4ebff4844e382110, ...>
UtilitiesCS/UtilitiesCS.csproj:63:  <Reference Include="Fizzler, Version=1.3.1.0, ... PublicKeyToken=4ebff4844e382110, ...>

Command: git ls-files '*packages.config' | xargs grep -n 'id="Fizzler"'
Output:
SVGControl/packages.config:4:  <package id="Fizzler" version="1.3.1" targetFramework="net481" />
UtilitiesCS/packages.config:11: <package id="Fizzler" version="1.3.1" targetFramework="net481" />
```

**Ground 1 confirmed:** `Fizzler` is referenced by exactly two projects, `SVGControl` and
`UtilitiesCS`, both **production**. No test project references it.

**Ground 4 confirmed, and it contradicts the cycle inputs:** the on-disk `Fizzler` identity is
`Version=1.3.1.0`. The snippet in `remediation-inputs.2026-08-04T22-28.md` § R-7 (line 105) states
`Version=1.3.0.0`. The measured value is `1.3.1.0`, so the inputs' snippet is factually wrong on this
point. `SVGControl.Test/app.config` redirects `Fizzler` to `1.3.0.0`, so placing a `1.3.1.0` assembly
into that output directory would activate a redirect that is inert today only because no `Fizzler.dll`
is present. That latent defect is owned by
`docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md` and is deliberately
not touched. **No `Fizzler` reference is added by this cycle and no `app.config` is edited.**

For completeness, the `ExCSS` reference census across the repository:

```
Command: git ls-files '*.csproj' | xargs grep -n 'Reference Include="ExCSS'
Output:
QuickFiler/QuickFiler.csproj:47:   <Reference Include="ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a">
SVGControl/SVGControl.csproj:55:   <Reference Include="ExCSS, Version=4.3.2.0, ... processorArchitecture=MSIL">
UtilitiesCS/UtilitiesCS.csproj:60: <Reference Include="ExCSS, Version=4.3.2.0, ... processorArchitecture=MSIL">
```

Three production projects reference `ExCSS` explicitly; no test project does. The eight sibling test
outputs carry `ExCSS.dll` transitively from their `ProjectReference` to `UtilitiesCS` or `QuickFiler`,
whose own copy-local supplies it — which is why the absence shows up only in `SVGControl.Test`, whose
production dependency is `SVGControl` alone.

## Output Summary

All five census parts confirmed. (a) `SVGControl.Test.csproj` has a `Svg` `<Reference>` at line 282 and
zero occurrences of `ExCSS` in any casing. (b) `packages.config` has a `Svg` entry at line 116 and zero
`ExCSS`. (c) `SVGControl.Test/bin/Debug` contains `Svg.dll` but neither `ExCSS.dll` nor `Fizzler.dll`.
(d) `*.Test/bin/Debug/ExCSS.dll` returns **8** and `*.Test/bin/Debug/Fizzler.dll` returns **0**;
`SVGControl.Test` is the only one of the nine tracked test projects lacking `ExCSS.dll`, no test
project's output contains `Fizzler.dll`, and the 8-not-9 count is explained by `UtilitiesSwordfish.Test`
being stale untracked build output on four verified grounds (0 tracked files, no project file on disk,
`Swordfish.NET.Test.exe` with no `*.Test.dll`, and zero `Swordfish` matches in `TaskMaster.sln`).
(e) `packages/ExCSS.4.3.2/lib/net48/ExCSS.dll` exists and `lib\net481` does not, making `net48` correct.
The verbatim `ExCSS` identity from `SVGControl/SVGControl.csproj:55` is recorded as the source
`[P1-T1]` must copy. One cycle-input claim was re-measured and found false: the on-disk `Fizzler`
version is `1.3.1.0`, not the `1.3.0.0` the inputs' snippet states.
