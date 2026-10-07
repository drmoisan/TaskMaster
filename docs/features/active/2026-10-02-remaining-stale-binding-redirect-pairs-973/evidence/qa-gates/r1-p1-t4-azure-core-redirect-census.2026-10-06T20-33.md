# Remediation cycle 1, P1-T4: Azure.Core redirect and Reference census (AC17 observation i and the System.Linq.AsyncEnumerable half)

Timestamp: 2026-10-06T20-33
Command: Grep tool, path <execution-worktree-root>: (a) pattern `name="Azure\.Core"`, glob `*/app.config`, -n, -A 1; (b) pattern `oldVersion="0\.0\.0\.0-1\.63\.0\.0" newVersion="1\.63\.0\.0"`, glob `*/app.config`, count mode; (c) pattern `Include="Azure\.Core, Version=`, glob `*/*.csproj`, -n; (d) pattern `name="(Azure\.Core|System\.Linq\.AsyncEnumerable)"` over TaskMaster/app.config, -n, -A 1; (e) pattern `^BIN TaskMaster System\.Linq\.AsyncEnumerable\.dll=True` over docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/system-linq-asyncenumerable-bin-presence.md, count mode
EXIT_CODE: 0

(a) Azure.Core identity and redirect lines (16):
AZURE-REDIRECT VBFunctions.Test/app.config:66|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT Tags/app.config:78|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT SVGControl.Test/app.config:58|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT QuickFiler/app.config:82|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT UtilitiesCS.Test/app.config:78|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT QuickFiler.Test/app.config:78|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT UtilitiesCS/app.config:83|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT TaskVisualization.Test/app.config:78|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT TaskTree.Test/app.config:226|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT ToDoModel.Test/app.config:78|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT TaskVisualization/app.config:78|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT Tags.Test/app.config:226|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT TaskTree/app.config:78|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT TaskMaster.Test/app.config:70|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT ToDoModel/app.config:83|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
AZURE-REDIRECT TaskMaster/app.config:90|<bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
(Each identity line, at the line number given, reads `<assemblyIdentity name="Azure.Core" publicKeyToken="92742159e12e44c8" culture="neutral" />`; the redirect line is the next line.)

(b) Redirect-literal count per file: 1 in each of VBFunctions.Test, SVGControl.Test, TaskMaster.Test, QuickFiler.Test, Tags.Test, Tags, TaskMaster, UtilitiesCS.Test, TaskTree.Test, UtilitiesCS, TaskVisualization, TaskTree, TaskVisualization.Test, QuickFiler, ToDoModel.Test, ToDoModel; 16 occurrences across 16 files, no other file.

(c) csproj Azure.Core Reference Include lines (10), each `Version=1.63.0.0, Culture=neutral, PublicKeyToken=92742159e12e44c8`:
VBFunctions.Test/VBFunctions.Test.csproj:49
Tags.Test/Tags.Test.csproj:48
TaskVisualization.Test/TaskVisualization.Test.csproj:83
TaskTree.Test/TaskTree.Test.csproj:48
TaskMaster.Test/TaskMaster.Test.csproj:49
UtilitiesCS.Test/UtilitiesCS.Test.csproj:582
SVGControl.Test/SVGControl.Test.csproj:101
QuickFiler.Test/QuickFiler.Test.csproj:252
UtilitiesCS/UtilitiesCS.csproj:51
ToDoModel.Test/ToDoModel.Test.csproj:86
(none in TaskMaster/TaskMaster.csproj)

(d) TaskMaster/app.config:
90: <assemblyIdentity name="Azure.Core" publicKeyToken="92742159e12e44c8" culture="neutral" />
91: <bindingRedirect oldVersion="0.0.0.0-1.63.0.0" newVersion="1.63.0.0" />
218: <assemblyIdentity name="System.Linq.AsyncEnumerable" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
219: <bindingRedirect oldVersion="0.0.0.0-10.0.0.12" newVersion="10.0.0.12" />

(e) AC12 artifact `^BIN TaskMaster System\.Linq\.AsyncEnumerable\.dll=True` count 1.

Output Summary:
- 16 AZURE-REDIRECT lines, one for each of Tags, TaskTree, TaskVisualization, QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, VBFunctions.Test, Tags.Test, TaskTree.Test, QuickFiler.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test and SVGControl.Test; every redirect line reads oldVersion 0.0.0.0-1.63.0.0, newVersion 1.63.0.0.
- The redirect-literal count is 1 in each of the same 16 files and in no other.
- 10 csproj Azure.Core Reference lines, all Version=1.63.0.0; none in TaskMaster/TaskMaster.csproj.
- TaskMaster/app.config: Azure.Core 0.0.0.0-1.63.0.0 / 1.63.0.0 and System.Linq.AsyncEnumerable 0.0.0.0-10.0.0.12 / 10.0.0.12.
- AC12 artifact count 1. No CENSUS-MISMATCH.
