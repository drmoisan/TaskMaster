# Phase 1 source gates and write-set closure — issue #877

Timestamp: 2026-09-13T10-52
Command: `git -C <repo-root> status --porcelain --untracked-files=all -- SVGControl SVGControl.Test scripts` and `git -C <repo-root> status --porcelain --untracked-files=all -- QuickFiler.Test UtilitiesCS.Test TestSupport`
EXIT_CODE: 0
Output Summary: The out-of-scope span printed zero lines. The in-scope span printed exactly 5 lines naming exactly the five write-set paths, with `TestSupport/TestAssemblyResolver.cs` listed as an individual untracked file rather than as a collapsed directory entry. All token gates from [P1-T1] through [P1-T7] passed.

## Span 1 — out-of-scope directories must be untouched

Command: `git -C <repo-root> status --porcelain --untracked-files=all -- SVGControl SVGControl.Test scripts`

Output: zero lines.

`SVGControl`, `SVGControl.Test` and `scripts` are unmodified, which includes `scripts/vscode/TaskMaster.cli.runsettings`.

## Span 2 — the code write set is exactly five paths

Command: `git -C <repo-root> status --porcelain --untracked-files=all -- QuickFiler.Test UtilitiesCS.Test TestSupport`

Output, verbatim:

```
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler.Test/SetupAssemblyInitializer.cs
 M UtilitiesCS.Test/TestAssemblyInitializer.cs
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
?? TestSupport/TestAssemblyResolver.cs
```

Exactly 5 lines, naming exactly the five approved write-set paths. The `--untracked-files=all` flag was retained, and the new file is enumerated individually rather than as a bare `TestSupport/` directory entry, so no `git add -N` preparation was required.

## Token gate results, [P1-T1] through [P1-T7]

- [P1-T1] `TestSupport/TestAssemblyResolver.cs` exists. One line trims exactly to `internal static class TestAssemblyResolver`. One line trims exactly to `namespace TaskMaster.TestSupport`. Occurrences of `??=`: 0. Occurrences of `#nullable`: 0. Total line count: 131, under the 500-line limit.
- [P1-T2] Lines containing `does not reliably honour binding redirects`: exactly 1. Lines containing `netstandard, Version=2.1.0.0, PublicKeyToken=cc7b13ffcd2ddd51`: exactly 1. Neither token was split by wrapping.
- [P1-T3] Occurrences in the new file of `DoNotParallelize`: 0. `Workers`: 0. `Thread.Sleep`: 0. `Task.Delay`: 0.
- [P1-T4] `QuickFiler.Test/QuickFiler.Test.csproj`: lines containing `TestAssemblyResolver.cs`: exactly 2. Lines containing `..\TestSupport\TestAssemblyResolver.cs`: exactly 1. Lines still containing `TestSupport\WinFormsPumpHost.cs`: exactly 1. `git diff --stat` reports 3 insertions and 0 deletions.
- [P1-T5] `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: lines containing `TestAssemblyResolver.cs`: exactly 2. Lines containing `..\TestSupport\TestAssemblyResolver.cs`: exactly 1. Lines still containing `<Compile Include="TestAssemblyInitializer.cs" />`: exactly 1. `git diff --stat` reports 3 insertions and 0 deletions.
- [P1-T6] `QuickFiler.Test/SetupAssemblyInitializer.cs`: line 20 contains `global::TaskMaster.TestSupport.TestAssemblyResolver.Install();` (exactly 1 line). Line 23 contains `System.Windows.Forms.Application.EnableVisualStyles();` (exactly 1 line). Line 24 contains `System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);` (exactly 1 line). Line 23 precedes line 24, so the two existing calls retain their relative order.
- [P1-T7] `UtilitiesCS.Test/TestAssemblyInitializer.cs`: line 17 contains `global::TaskMaster.TestSupport.TestAssemblyResolver.Install();` (exactly 1 line). Occurrences of `ResolveByNameAndKey`: 0. `PublicKeyTokensEqual`: 0. `_resolving`: 0. Line 11 contains `[TestClass]` (exactly 1 line). Line 14 contains `[AssemblyInitialize]` (exactly 1 line). Line 15 contains `public static void Initialize(TestContext context)` (exactly 1 line).

## Sharing mechanism justification

This section is the record that acceptance criterion AC3 requires.

- **Single source file.** The resolver is shared from one file, `TestSupport/TestAssemblyResolver.cs`, at the repository root. It is linked into both test projects with `<Compile Include="..\TestSupport\TestAssemblyResolver.cs">` plus a `<Link>TestSupport\TestAssemblyResolver.cs</Link>` element.
- **Link chosen over duplication.** A link was chosen rather than duplicating the source into each project so that the two compiled copies cannot drift. The issue text permits duplication as a fallback if sharing forces awkwardness; it did not, so the sharing option was taken.
- **A hand-written item is required in each project.** Both `QuickFiler.Test.csproj` and `UtilitiesCS.Test.csproj` are legacy non-SDK projects with explicit `Compile Include` items and no globbing. Nothing is picked up automatically, so the link has to be declared by hand once per project. This is why the change touches two project files rather than one.
- **No cross-assembly ambiguity.** The shared type is declared `internal`, and neither test project grants the other `InternalsVisibleTo`. This was verified by searching both project directories for a column-start `[assembly: InternalsVisibleTo` declaration, which returned no matches in either. Each assembly therefore sees only its own compiled copy, and the `global::`-qualified call sites `global::TaskMaster.TestSupport.TestAssemblyResolver.Install();` cannot bind ambiguously.
- **Scope confined to the two test projects.** `SVGControl` is not modified. Three near-identical resolvers exist after this change, which is accepted for now; consolidating them into production code has a different blast radius and is tracked separately as issue #879.
