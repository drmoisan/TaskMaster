# D4 — `using`-Directive Removal Is HYGIENE, Not a Gate Fix (Issue #449, [P4-T3])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command: see the per-claim `Command:` lines below.
EXIT_CODE: 0

## Classification

The removal of the ten orphaned `using` directives from
`QuickFiler/Controllers/QfcExplorerController.cs` is **HYGIENE**, not a gate fix. An orphaned `using`
directive fails **neither** gate in this repository. Nothing in the analyzer build or the nullable
build would have reported these directives, and the file was green on `main` while carrying all
sixteen of them.

This classification matters because it fixes the burden of proof. Since no gate flags an orphaned
directive, no gate confirms that a removal was safe either. The safety argument therefore rests
entirely on the self-verifying property in [P4-T4], described below.

## Reason 1 — `IDE0005`'s analyzer is not wired into these projects

Command: `grep -n '<Analyzer Include' QuickFiler/QuickFiler.csproj`
EXIT_CODE: 0
Output:
```
582: ..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll
583: ..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll
584: ..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll
585: ..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll
586: ..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll
587: ..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll
588: ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll
589: ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll
591: ..\packages\SonarAnalyzer.CSharp.10.32.0.713\analyzers\SonarAnalyzer.CSharp.dll
```

`QuickFiler/QuickFiler.csproj` wires exactly the five packages the repository's analyzer stack
specifies — Meziantou.Analyzer, Roslynator.Analyzers, AsyncFixer,
Microsoft.CodeAnalysis.BannedApiAnalyzers, and SonarAnalyzer.CSharp. `IDE0005`
("Using directive is unnecessary") is produced by the .NET SDK's built-in code-style analyzers, which
are **not** among them. These are legacy non-SDK `packages.config` projects; the SDK code-style
analyzer set is not present, so `IDE0005` cannot be emitted no matter what `/p:` flags are passed.

## Reason 2 — no `IDE0005` severity is configured, and there is no `.globalconfig`

Command: `grep -rn 'IDE0005' .editorconfig`
EXIT_CODE: 1
Output: (no match)

Command: `ls -1 .globalconfig`
EXIT_CODE: 2
Output: `ls: cannot access '.globalconfig': No such file or directory`

The repo-root `.editorconfig` configures no `IDE0005` severity, and there is no `.globalconfig`
anywhere. Even if the analyzer were present, its severity would default to `hidden`/`suggestion` and
`/p:EnforceCodeStyleInBuild=true` would not promote it to a build-breaking diagnostic.

## Reason 3 — `CS8019` is a hidden diagnostic that `TreatWarningsAsErrors` does not promote

The compiler's own equivalent, `CS8019` ("Unnecessary using directive"), is emitted at **hidden**
severity. Hidden diagnostics are not surfaced as warnings, and `/p:TreatWarningsAsErrors=true`
promotes warnings, not hidden diagnostics. The nullable build therefore cannot fail on an orphaned
directive either. Both baseline builds confirm this empirically: the merge-base file carried nine
directives that this plan removes as orphaned, and both baseline builds reported **0 errors** with
only the 5 pre-existing `System.Reactive` `packages.config` warnings
(`../baseline/step3-analyzer-build.2026-08-22T09-16.md`,
`../baseline/step4-nullable-build.2026-08-22T09-16.md`).

## Direct empirical confirmation — three directives were ALREADY unused on green `main`

Three of the removed directives (merge-base lines 7, 13, and 15) were unused **even before** the
[P4-T1] dead-region deletion, while the file was green on `main`. Verified against the merge-base
copy of the file (`git show c551eabab0aa0a6b1a284252811a2e1de819634e:QuickFiler/Controllers/QfcExplorerController.cs`):

| Merge-base line | Directive | Consumer searched for | Hits | Verdict |
| --- | --- | --- | --- | --- |
| 7 | `using System.Text;` | `StringBuilder`, `Encoding` | **0** | already unused |
| 13 | `using ToDoModel;` | any occurrence of `ToDoModel` | **1**, the directive itself only | already unused |
| 15 | `using UtilitiesCS.OutlookExtensions;` | see note below | **0** | already unused |

Note on line 15: the only extension-method call that could plausibly have needed it is
`strOutput.IsInitialized()` at merge-base line 187 (inside the dead region). `IsInitialized` is
declared in `UtilitiesCS/Extensions/ArrayExtensions.cs:154-210`, and that file's namespace is
`UtilitiesCS` (`grep -n '^namespace' UtilitiesCS/Extensions/ArrayExtensions.cs` -> `9:namespace UtilitiesCS`),
**not** `UtilitiesCS.OutlookExtensions`. The retained `using UtilitiesCS;` (merge-base line 14)
supplies it. `using UtilitiesCS.OutlookExtensions;` therefore had no consumer at all.

This is the strongest available evidence for the classification: three directives sat orphaned in a
file on a green `main` branch, and no gate in this repository ever reported them.

For contrast, the six directives whose only consumers were inside the deleted dead region were
genuinely in use before [P4-T1] — `Regex` (2 hits), `List<>`/`Dictionary<>`/`IEnumerable<>` (1),
LINQ operators (6), `Path.`/`File.`/`Directory.`/`StreamWriter` (3), and `Debug.`/`Stopwatch` (1) all
appear in the merge-base file, all within lines 183-321. Their removal is therefore consequent on the
dead-region deletion rather than pre-existing rot.

## The self-verifying property described in [P4-T4]

Because no gate flags an orphaned directive, the safety of removing one is established by the
CONVERSE: a directive that was in fact still required makes the build **fail** with `CS0246`
("type or namespace could not be found") or `CS1061` ("no such member — are you missing a using
directive?"). The analyzer build in [P4-T4] is therefore the self-verifying gate for [P4-T2]. If it
had failed with either code, the remedy prescribed by the plan is to restore that specific directive
and record the restoration in this artifact.

## Restorations recorded

**None.** No directive was restored.

- [P4-T4] (analyzer build after removing nine directives): EXIT_CODE 0, 0 errors. No `CS0246`, no
  `CS1061`. See `../regression-testing/phase4-analyzer-build.2026-08-22T09-16.md`.
- [P4-T5] (nullable build): EXIT_CODE 0, 0 errors. See
  `../regression-testing/phase4-nullable-build.2026-08-22T09-16.md`.
- [P5-T5] (analyzer build after removing the tenth directive and adding the fully-qualified
  `System.Func<...>` seam): EXIT_CODE 0, 0 errors. In particular **`using System;` was NOT restored** —
  the fully-qualified `System.Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>`
  declaration compiles without it, so the seam did not resurrect the directive D4 removed, and AC-8
  is not contradicted. See `../regression-testing/phase5-analyzer-build.2026-08-22T09-16.md`.

All ten directives in the D4 disposition table were removed and none was required.

## Output Summary

The `using` removals are classified as **HYGIENE, not a gate fix**: an orphaned directive fails
neither the analyzer build nor the nullable build in this repository, because `IDE0005`'s analyzer is
not among the five wired into `QuickFiler.csproj`, no `IDE0005` severity is configured in
`.editorconfig`, there is no `.globalconfig`, and `CS8019` is a hidden diagnostic that
`/p:TreatWarningsAsErrors=true` does not promote. Three of the removed directives (`using System.Text;`,
`using ToDoModel;`, `using UtilitiesCS.OutlookExtensions;`) were verified **already unused on green
`main`** before the dead-region deletion, which is direct empirical confirmation. Safety rests on the
self-verifying converse property: a still-required directive breaks the build with `CS0246` or
`CS1061`. Every build gate passed with 0 errors, and **no directive was restored** — including
`using System;`, which the fully-qualified seam declaration deliberately avoids resurrecting.
