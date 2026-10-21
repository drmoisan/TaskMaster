# P5-T17 AC17 check-off - NOT MET (left unchecked)

Timestamp: 2026-10-06T18-36
Command: Grep tool, path TaskMaster/app.config, pattern `name="(Azure\.Core|System\.Linq\.AsyncEnumerable)"`, -n, -A 1; Grep `Include="Azure\.Core, Version=` over TaskMaster/TaskMaster.csproj and over glob */*.csproj; pwsh -NoProfile -Command 'Test-Path' probes of `<execution-worktree-root>\TaskMaster\bin\Debug` (and UtilitiesCS, QuickFiler, ToDoModel) for Azure.Core.dll, Microsoft.Kiota.Authentication.Azure.dll, UtilitiesCS.dll, Microsoft.Graph.dll, Microsoft.Graph.Core.dll, Azure.Identity.dll, Microsoft.Kiota.Abstractions.dll, System.Linq.AsyncEnumerable.dll; Read of the AC12 artifact
EXIT_CODE: 1
Output Summary: AC17: NOT MET. The plan's three named observations hold: Azure.Core at 0.0.0.0-1.63.0.0 / 1.63.0.0 and System.Linq.AsyncEnumerable at 0.0.0.0-10.0.0.12 / 10.0.0.12 in TaskMaster/app.config, and the System.Linq.AsyncEnumerable DLL in TaskMaster's bin\Debug. The criterion's conclusion, however, is that each traced request redirects to an assembly present in the add-in's output directory. For Azure.Core this is contradicted by observation: neither Azure.Core.dll nor the requesting Microsoft.Kiota.Authentication.Azure.dll is in TaskMaster\bin\Debug. The criterion is left unchecked and the gap is reported.

## Observations (2026-10-06, after the P4-T6 Rebuild)

TaskMaster/app.config:
- line 90 `<assemblyIdentity name="Azure.Core" publicKeyToken="92742159e12e44c8" culture="neutral" />`
- line 91 `<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />`
- line 218 `<assemblyIdentity name="System.Linq.AsyncEnumerable" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />`
- line 219 `<bindingRedirect oldVersion="0.0.0.0-10.0.0.12" newVersion="10.0.0.12" />` (10.0.0.12 is the AC8 value)

evidence/qa-gates/system-linq-asyncenumerable-bin-presence.md (EXIT_CODE 0): `BIN TaskMaster System.Linq.AsyncEnumerable.dll=True System.Linq.Async.dll=True`.

Azure.Core References: TaskMaster/TaskMaster.csproj carries no Azure.Core Reference (Grep: no match). `Include="Azure.Core, Version=1.63.0.0` occurs in UtilitiesCS/UtilitiesCS.csproj:51 and in nine test csproj (TaskVisualization.Test, VBFunctions.Test, SVGControl.Test, TaskMaster.Test, TaskTree.Test, UtilitiesCS.Test, Tags.Test, QuickFiler.Test, ToDoModel.Test).

Output-directory probes:

    BIN TaskMaster Azure.Core.dll=False Microsoft.Kiota.Authentication.Azure.dll=False
    BIN UtilitiesCS Azure.Core.dll=True Microsoft.Kiota.Authentication.Azure.dll=True
    BIN QuickFiler Azure.Core.dll=False Microsoft.Kiota.Authentication.Azure.dll=False
    BIN ToDoModel Azure.Core.dll=False Microsoft.Kiota.Authentication.Azure.dll=False

    TaskMaster\bin\Debug: UtilitiesCS.dll=True Microsoft.Graph.dll=False Microsoft.Graph.Core.dll=False Azure.Identity.dll=False Microsoft.Kiota.Abstractions.dll=False System.Linq.AsyncEnumerable.dll=True TaskMaster.dll=True

## Traced requests (spec Proposed Fix)

- System.Linq.AsyncEnumerable 10.0.0.6, requested by System.Linq.Async 7.0.1: falls inside `0.0.0.0-10.0.0.12` and binds to System.Linq.AsyncEnumerable 10.0.0.12. That assembly is present in TaskMaster\bin\Debug. This half of the criterion holds.
- Azure.Core 1.50.0.0, requested by Microsoft.Kiota.Authentication.Azure 2.1.2: falls inside `0.0.0.0-1.63.0.0` and redirects to Azure.Core 1.63.0.0, the version every Azure.Core Reference in the repository declares. Neither Azure.Core.dll nor Microsoft.Kiota.Authentication.Azure.dll exists in TaskMaster\bin\Debug, which is the add-in's local output directory; both exist only in UtilitiesCS\bin\Debug. The claim that this request redirects to an assembly existing in the add-in's output directory is therefore not observed. This half of the criterion is not met as written.

GAP: the spec's AC17 premise assumes the Microsoft.Graph and Azure stack is deployed to the add-in output directory. In the local Debug build it is not deployed there: the whole Graph, Kiota and Azure family is absent from TaskMaster\bin\Debug. This condition predates this item, which changes no Reference copy behaviour for those assemblies. It needs a planner or orchestrator ruling. One option is to reword AC17 to the System.Linq.AsyncEnumerable half plus the Azure.Core redirect value. The other is to investigate why UtilitiesCS's Graph and Azure dependencies are not copied to TaskMaster's output.
AC17: NOT MET (Azure.Core target assembly not present in TaskMaster\bin\Debug)
SPEC-LINE: `- [ ] AC17 (invariant trace delivered).` (left unchecked)
