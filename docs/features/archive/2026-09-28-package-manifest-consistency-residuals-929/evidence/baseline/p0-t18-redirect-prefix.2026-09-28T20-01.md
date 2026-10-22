# P0-T18 — Pre-fix binding redirect state

Timestamp: 2026-09-30T09-50
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; (quote SVGControl/app.config lines 15 and 19 and SVGControl/SVGControl.csproj lines 58 and 82); "FIZZLER_ASM=" + [System.Reflection.AssemblyName]::GetAssemblyName((Resolve-Path "packages/Fizzler.1.3.1/lib/netstandard2.0/Fizzler.dll").Path).Version; "UNSAFE_ASM=" + [System.Reflection.AssemblyName]::GetAssemblyName((Resolve-Path "packages/System.Runtime.CompilerServices.Unsafe.6.1.2/lib/net462/System.Runtime.CompilerServices.Unsafe.dll").Path).Version; (CMD-REDIRECT-OBSERVE); (tree-wide Fizzler newVersion listing over */app.config)' — the plan's three commands run in one pwsh invocation
EXIT_CODE: 0
Output Summary:
- SVGControl/app.config line 15: `        <bindingRedirect oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0" />`
- SVGControl/app.config line 19: `        <bindingRedirect oldVersion="0.0.0.0-6.0.2.0" newVersion="6.0.2.0" />`
- SVGControl/SVGControl.csproj line 58: `    <Reference Include="Fizzler, Version=1.3.1.0, Culture=neutral, PublicKeyToken=4ebff4844e382110, processorArchitecture=MSIL">`
- SVGControl/SVGControl.csproj line 82: `    <Reference Include="System.Runtime.CompilerServices.Unsafe, Version=6.0.3.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL">`
- FIZZLER_ASM=1.3.1.0
- UNSAFE_ASM=6.0.3.0
- FIZZLER_REPAIRS=1 UNSAFE_REPAIRS=1 EXAMINED=4 (the pre-fix drift as Invoke-BindingRedirectReconciliation sees it)

Tree-wide Fizzler redirect listing (13 configs: 12 at 1.3.0.0, UtilitiesCS at 1.3.1.0):
- SVGControl Fizzler 1.3.0.0 (in-scope AC2 target)
- UtilitiesCS Fizzler 1.3.1.0 (already names 1.3.1.0)
- OUT-OF-SCOPE-RESIDUAL: QuickFiler Fizzler 1.3.0.0
- OUT-OF-SCOPE-RESIDUAL: QuickFiler.Test Fizzler 1.3.0.0
- OUT-OF-SCOPE-RESIDUAL: SVGControl.Test Fizzler 1.3.0.0
- OUT-OF-SCOPE-RESIDUAL: Tags Fizzler 1.3.0.0
- OUT-OF-SCOPE-RESIDUAL: TaskMaster Fizzler 1.3.0.0
- OUT-OF-SCOPE-RESIDUAL: TaskTree Fizzler 1.3.0.0
- OUT-OF-SCOPE-RESIDUAL: TaskVisualization Fizzler 1.3.0.0
- OUT-OF-SCOPE-RESIDUAL: TaskVisualization.Test Fizzler 1.3.0.0
- OUT-OF-SCOPE-RESIDUAL: ToDoModel Fizzler 1.3.0.0
- OUT-OF-SCOPE-RESIDUAL: ToDoModel.Test Fizzler 1.3.0.0
- OUT-OF-SCOPE-RESIDUAL: UtilitiesCS.Test Fizzler 1.3.0.0

The 11 out-of-scope residuals are already recorded in docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md (decision D5). The tree-wide count matches the plan's expectation of 13.
