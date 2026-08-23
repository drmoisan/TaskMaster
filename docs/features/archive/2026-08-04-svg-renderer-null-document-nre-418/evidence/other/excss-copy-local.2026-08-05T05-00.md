# Copy-Local Mechanism Confirmation — `ExCSS.dll` Reaches `SVGControl.Test/bin/Debug`

- Task: `[P1-T4]`
- Timestamp: 2026-08-04T23-56
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- Preconditions: `[P1-T1]`, `[P1-T2]`, `[P1-T3]` applied. **Nothing was deleted** before this task; the
  build was run over the existing output tree as the task directs.

## 1. Restore

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"
```

```
EXIT_CODE: 0
```

Summary lines:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:01.27
```

`Output Summary:` restore succeeded with zero warnings and zero errors. The new `ExCSS` entry in
`SVGControl.Test/packages.config` resolved against the already-present `packages/ExCSS.4.3.2/`
directory, so no package download and no `packages/` mutation occurred.

## 2. Analyzer build

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild
```

```
EXIT_CODE: 0
```

Summary lines:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
Time Elapsed 00:00:01.37
```

`Output Summary:` build succeeded, **0 errors**, 5 warnings, 2 `csc.exe` invocations. The five warnings
are the pre-existing code-less `System.Reactive.PackagesConfigCheck.targets(31,5)` `packages.config`
advisories, emitted by exactly these five projects (measured, not assumed):

```
QuickFiler.csproj   TaskMaster.csproj   ToDoModel.csproj   UtilitiesCS.Test.csproj   UtilitiesCS.csproj
```

The basis inventory in `evidence/remediation-baseline/build-basis.2026-08-05T05-00.md` records **6**
warnings: these same five plus one `CS2002` in `UtilitiesCS.Test`. `CS2002` is `CoreCompile`-gated and
`UtilitiesCS.Test` did not recompile in this run (2 `csc.exe` invocations total, both for the
`SVGControl.Test` dependency chain). This is the expected removal the basis artifact records in advance;
its formal disposition belongs to `[P2-T5]` against the `[P2-T4]` build, not to this task.

## 3. Output-directory listing — the mechanism confirmed

```
Command: per-file existence test over SVGControl.Test/bin/Debug/
PRESENT: ExCSS.dll
PRESENT: Svg.dll
ABSENT:  Fizzler.dll
```

**`ExCSS.dll` is present.** Before the fix it was absent — see
`evidence/remediation-baseline/reference-census.2026-08-05T05-00.md` § (c), which measured
`ABSENT: ExCSS.dll` at the same path. `Svg.dll` is still present, confirming `[P1-T3]`'s
`<Private>True</Private>` addition is behavior-preserving. `Fizzler.dll` is still absent, confirming
Design Decision 3 was honoured and that no `Fizzler` reference was added.

### `ExCSS.dll` file version and identity

```
FileVersion=4.3.2.0
ProductVersion=4.3.2-release.0+Branch.release-4.3.2.Sha.0a75db4bfffe9dc26555fd8b9ca152f6f9a2760f.0a75db4bfffe9dc26555fd8b9ca152f6f9a2760f
Length=368128
AssemblyIdentity=ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a
```

Three consistency checks, all passing:

1. **`FileVersion=4.3.2.0`** matches the `Version=4.3.2.0` in the `<Reference>` `Include` identity.
2. **`AssemblyIdentity`** is `ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a`
   — the exact identity the failing tests requested, recorded verbatim in
   `evidence/remediation-baseline/order-standalone.2026-08-05T05-00.md`, and the exact identity
   `SVGControl.Test/app.config` already redirects to (`oldVersion="0.0.0.0-4.3.2.0"
   newVersion="4.3.2.0"`). **No `app.config` change is needed and none was made.**
3. **`Length=368128`** is byte-for-byte the size of the `HintPath` source,
   `packages/ExCSS.4.3.2/lib/net48/ExCSS.dll` (368128 bytes, measured at `[P0-T9]` § (e)), and the
   copied file retains the source modification timestamp `2026-07-23 19:21:12`, which is how MSBuild
   copy-local behaves. The output file is therefore the package asset itself, not a rebuilt or
   substituted assembly.

## 4. Post-build re-read of the `HintPath` — the silent-rewrite check

This re-read is required because `Invoke-VSBuild.ps1` invokes `Sync-PackageReferences.ps1`, which
rewrites a `HintPath` when the current one fails to resolve. A silent rewrite is the one mechanism that
could break this plan undetected and would invalidate `[P1-T7]`'s five-added-lines count, so it is
checked rather than assumed.

Post-build content of the block, read from disk **after** the build completed:

```
130:    <Reference Include="ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL">
131-      <HintPath>..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll</HintPath>
132-      <Private>True</Private>
```

**Post-build `HintPath` text, verbatim:**

```
..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll
```

**CONFIRMED UNCHANGED.** This is byte-identical to the value `[P1-T1]` wrote. The `net48` segment was
**not** retargeted to a nonexistent `net481` path. No halt condition fires.

Direct corroboration from the build log, first line of output:

```
Sync-PackageReferences: All HintPaths are up to date
```

The synchronizer ran and reported no rewrite was necessary, which is the expected outcome given that
`packages/ExCSS.4.3.2/lib/net48/ExCSS.dll` exists (`[P0-T9]` § (e)).

### Diff line counts still hold after the build

```
Command: git diff --numstat -- SVGControl.Test/SVGControl.Test.csproj
Output:  5	0	SVGControl.Test/SVGControl.Test.csproj

Command: git diff --numstat -- SVGControl.Test/packages.config
Output:  1	0	SVGControl.Test/packages.config
```

**`git diff -- SVGControl.Test/SVGControl.Test.csproj` still shows exactly five added lines** and zero
removed or modified. `packages.config` still shows exactly one added line. The build wrote to neither
file. `[P1-T7]`'s count is therefore intact.

## 5. `MSB3243` / `MSB3245` / `MSB3277` disposition input for `[P2-T5]`

```
Command: grep -nE 'MSB3243|MSB3245|MSB3277' <build log>
count: 0
```

**Zero occurrences of `MSB3243`, `MSB3245`, or `MSB3277` anywhere in the build log**, for
`SVGControl.Test` or for any other project. No reference-resolution diagnostic was emitted by the
`ExCSS` addition, so there is no verbatim line to record and nothing for `[P2-T5]` to escalate as an
accepted-with-evidence finding from this task. This is the expected result: the added identity matches
the deployed assembly exactly on name, version, and public key token, so `ResolveAssemblyReference` had
no version-mismatch or missing-file condition to report.

## Halt conditions — neither fired

| Halt condition from `[P1-T4]` | Outcome |
|---|---|
| `HintPath` was rewritten → halt and report the rewritten value | Did **not** occur. `HintPath` is unchanged and `Sync-PackageReferences` reported "All HintPaths are up to date". |
| `ExCSS.dll` absent from the output after a successful build → halt and report | Did **not** occur. `ExCSS.dll` is present with `FileVersion=4.3.2.0`. |

## Output Summary

Both commands returned `EXIT_CODE: 0` — restore (0 warnings, 0 errors) and the analyzer build (0 errors,
5 warnings). The directory listing shows **`ExCSS.dll` present** with `FileVersion=4.3.2.0`, assembly
identity `ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a`, and length 368128
matching the package asset byte-for-byte; **`Svg.dll` still present**; **`Fizzler.dll` still absent**.
The post-build `HintPath` re-read returns `..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll` unchanged, with
`Sync-PackageReferences: All HintPaths are up to date` in the log confirming no rewrite, and the csproj
diff still shows exactly five added lines. **Zero** `MSB3243`/`MSB3245`/`MSB3277` lines were emitted for
`SVGControl.Test` or any project. The copy-local mechanism is confirmed and neither halt condition fired.
