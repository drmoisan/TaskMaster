# Baseline — Analyzer Package Back-Fill

Timestamp: 2026-08-22T09-20

Command:

```
# 1. Skew confirmation (run from the worktree root)
sed -n '470,482p' QuickFiler.Test/QuickFiler.Test.csproj
ls -d packages/Meziantou.Analyzer.* packages/Roslynator.Analyzers.*
grep -rl "Meziantou.Analyzer.3.0.156" --include=*.csproj . | wc -l

# 2. Back-fill
pwsh -NoProfile -Command 'nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages'
pwsh -NoProfile -Command 'nuget install Roslynator.Analyzers -Version 4.16.0 -OutputDirectory packages'

# 3. Post-state confirmation (run from the worktree root)
ls -l packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.dll
ls -l packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator.CSharp.Analyzers.dll
ls packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/
git status --porcelain
```

EXIT_CODE: 0

Both `nuget install` invocations reported `NUGET_EXIT=0`.

Output Summary:

## Skew confirmed empirically before the back-fill

The condition the plan predicts was measured, not assumed:

- **16 of 16** first-party `.csproj` files carry an unconditional `<Analyzer Include>` naming
  `Meziantou.Analyzer.3.0.156` (`grep -rl ... --include=*.csproj | wc -l` → `16`).
- The P0-T9 `nuget restore` installed only the newer pins:
  `ls -d packages/Meziantou.Analyzer.* packages/Roslynator.Analyzers.*` →
  `packages/Meziantou.Analyzer.3.0.174/` and `packages/Roslynator.Analyzers.4.16.1/`.
- The representative `<Analyzer Include>` block at `QuickFiler.Test/QuickFiler.Test.csproj` reads
  (line numbers re-derived in this worktree; the block spans lines 472 through 481, with the five
  skewed entries at lines 474 through 478):

  ```
    <ItemGroup>
      <!-- Issue #181: analyzer-only references (first-party scope). Severities are set to suggestion in .editorconfig so none break the nullable TreatWarningsAsErrors build. -->
      <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
      <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll" />
      <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll" />
      <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll" />
      <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll" />
  ```

  The five skewed paths name `3.0.156` (one entry) and `4.16.0` (four entries). Before the back-fill
  none of the five resolved, so every msbuild task in this plan would have reported `error CS0006`
  and a non-zero exit before producing any diagnostic.

## Resulting folder names

```
packages/Meziantou.Analyzer.3.0.156/
packages/Roslynator.Analyzers.4.16.0/
```

These sit **beside** the restore-installed `packages/Meziantou.Analyzer.3.0.174/` and
`packages/Roslynator.Analyzers.4.16.1/` folders. The accumulation is exactly the state the plan's
Binding Constraints note describes on CI and in the main checkout.

## Post-state — acceptance conditions

1. **Both commands record `EXIT_CODE: 0`.** Confirmed; each printed
   `Successfully installed '<id> <version>' to ...packages` and `NUGET_EXIT=0`.
2. **Both named DLL files exist:**

   ```
   -rwxr-xr-x 1 <user> 197121 2749952 Aug 14 20:39 packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.dll
   -rwxr-xr-x 1 <user> 197121  382464 Aug  8 12:24 packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator.CSharp.Analyzers.dll
   ```

   The remaining three Roslynator `4.16.0` DLLs the project files name are also present, so all five
   skewed `<Analyzer Include>` paths now resolve:

   ```
   Roslynator.CSharp.Analyzers.CodeFixes.dll
   Roslynator.CSharp.Analyzers.dll
   Roslynator_Analyzers_Roslynator.Common.dll
   Roslynator_Analyzers_Roslynator.Core.dll
   Roslynator_Analyzers_Roslynator.CSharp.dll
   Roslynator_Analyzers_Roslynator.CSharp.Workspaces.dll
   Roslynator_Analyzers_Roslynator.Workspaces.Common.dll
   Roslynator_Analyzers_Roslynator.Workspaces.Core.dll
   ```

3. **`git status --porcelain` reports zero entries whose path begins with the packages directory
   name.** Full output, unchanged from before the back-fill:

   ```
    M docs/features/active/winformspumphost-suite-determinism-511/plan.2026-08-21T18-10.md
   ?? docs/features/active/winformspumphost-suite-determinism-511/evidence/
   ```

   `.gitignore:191` (`**/[Pp]ackages/*`) ignores the tree.

## Scope preservation

This back-fill installs into the untracked `packages` tree and edits **no tracked file**. No
`.csproj` and no `packages.config` was modified. Binding Constraint 1 (no
`QuickFiler.Test/QuickFiler.Test.csproj` edit) and the P6-T11 scope lock (zero paths ending `.csproj`
in the diff) are both preserved. The underlying repository-wide skew — realigning the `<Analyzer
Include>` paths with `packages.config` across all 16 projects — remains out of scope for this child
and is the subject of the follow-up issue that P6-T20 files.
