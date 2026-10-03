# Human-Exception Runbook — Verify Designer Load and Outlook Add-in Start After the Binding-Redirect Sweep (Issue #973)

This runbook is the human follow-up for the `exception` response recorded against the requirement
"Re-test the #418 designer path for `PictureBoxSVG` after the sweep" on bug #973
(`docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/issue.md`, Acceptance
Criteria; `spec.md`, Test Strategy). It is contract-conformant per
`.claude/skills/human-exception-runbook/SKILL.md` (Cue, Prerequisites, Step-by-step Instructions,
Verification, Source and Citation).

It covers two manual checks:

- **Part A — WinForms designer load.** The #418 designer path for `SVGControl.PictureBoxSVG`, adapted
  from `docs/features/archive/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md`.
- **Part B — Outlook add-in start.** Build and load the TaskMaster VSTO add-in, open a viewer that
  constructs SVGControl controls, inspect the add-in log for binding failures in the corrected
  assembly families, and confirm `System.Linq.AsyncEnumerable.dll` is deployed beside `TaskMaster.dll`.

## Background (what this verifies and what it does not)

Issue #973 makes three changes:

1. It corrects 137 stale `bindingRedirect` entries (15 assembly/version pairs) across 14 `app.config`
   files so that each `newVersion` equals the version every csproj `Reference` compiles against.
   Affected families: Azure.Core, System.ClientModel, Microsoft.Identity.Client,
   Microsoft.Identity.Client.Extensions.Msal, the seven Microsoft.IdentityModel.* assemblies,
   System.IdentityModel.Tokens.Jwt, Microsoft.Bcl.Memory, Microsoft.Bcl.Numerics and
   Microsoft.Extensions.Diagnostics.Abstractions.
2. It deletes the 13 dead `Microsoft.IdentityModel.Clients.ActiveDirectory` (ADAL) redirect blocks,
   which name an assembly that nothing deploys or requests.
3. It installs the `System.Linq.AsyncEnumerable` 10.0.12 package, with an aliased csproj `Reference`,
   in UtilitiesCS, QuickFiler, ToDoModel, TaskMaster and UtilitiesCS.Test, and points the 15
   `System.Linq.AsyncEnumerable` redirects at the installed assembly version.

**Why the designer check cannot be automated.** For a .NET Framework project the WinForms designer
runs in-process inside `devenv.exe`. `SVGControl.dll` is loaded into the Visual Studio process, and
assembly binding there is governed by Visual Studio's own configuration. The designer never applies a
project `app.config`. Reproducing that host requires the `devenv.exe` AppDomain; the only automatable
substitute needs a hand-written `.config` file on disk, which the repository's unit-test policy
prohibits (`.claude/rules/general-unit-test.md`, "External Dependencies").

**Why a designer Pass is non-regression evidence only.** Two facts make Part A insensitive to this
change by construction:

- The designer does not read any project `app.config`, so none of the 137 corrected redirects, the
  ADAL deletions or the new `System.Linq.AsyncEnumerable` redirects are applied in that host.
- `PictureBoxSVG`'s binding chain is `Svg` -> `ExCSS` -> `Fizzler`. That chain is disjoint from every
  assembly family the sweep corrects.

A Pass in Part A therefore shows only that the designer path has not regressed. It is not evidence
that any corrected redirect is honoured. Record it with that meaning.

**What Part B adds and its limits.** `TaskMaster.dll.config` (built from `TaskMaster/app.config`) is
the one production configuration honoured at runtime, inside the add-in's AppDomain in
`OUTLOOK.EXE`. Part B confirms the add-in still starts with the edited configuration and that the
newly installed `System.Linq.AsyncEnumerable.dll` is deployed. Research for this issue found no
production code path that constructs a Graph, MSAL or Azure.Core client (research section 5.4), so
Part B also cannot prove that a corrected bind was taken. A clean log is non-regression evidence for
add-in start; a `FileNotFoundException` or `FileLoadException` naming a listed assembly is a defect.

## Cue

Act on this runbook at exactly one point in the workflow: **after** the atomic-executor has reported
the #973 change complete and the toolchain green (the Pester binding-redirect gate, the CSharpier
check, the analyzer Rebuild, the `TreatWarningsAsErrors` Rebuild, and the MSTest coverage route, all
passing in one consecutive pass), and **before** the feature is reported done.

This runbook satisfies the issue acceptance criterion "Re-test the #418 designer path for
`PictureBoxSVG` after the sweep." That criterion is satisfied only when this runbook has been executed
and its evidence artifact has been written to the feature folder. Do not run it before the sweep and
the package install are both present on the branch; a pre-change run produces no usable evidence.

## Prerequisites

- **Visual Studio with the Office/SharePoint development workload, the Windows Forms designer and the
  .NET Framework 4.8.1 targeting pack.** The projects target `net481`, so the classic in-process
  designer is used. The TaskMaster project is a VSTO Outlook add-in (`TaskMaster/TaskMaster.csproj`,
  `<OfficeApplication>Outlook</OfficeApplication>`), and starting it under the debugger requires the
  Office development tools.
- **Microsoft Outlook (desktop) installed** on the same machine, with a mail profile that opens
  normally.
- **The working tree on branch `bug/remaining-stale-binding-redirect-pairs-973`**, with the #973 change
  present. Confirm with `git branch --show-current` and record `git rev-parse HEAD`.
- **Outlook and Visual Studio closed before the build.** A running Outlook holds the add-in's build
  output open. Close Outlook normally through its own window; do not terminate the process. Close
  Visual Studio so the designer releases any previously loaded `SVGControl.dll`.
- **Packages restored and the solution built in `Debug|Any CPU`.** The change adds a new NuGet package,
  so restore first, then rebuild. From the repository root, in PowerShell:

  ```
  msbuild TaskMaster.sln /t:Restore /p:Configuration=Debug "/p:Platform=Any CPU" /p:RestorePackagesConfig=true /m
  msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
  ```

  The restore command is the one `scripts/vscode/Invoke-Restore.ps1` runs. Both commands must end with
  `0 Error(s)`. Use `/t:Rebuild`, not `/t:Build`, so every project's output and `.dll.config` is
  regenerated from the current sources.
- **Build outputs present.** Confirm these files exist after the build:
  - `SVGControl\bin\Debug\SVGControl.dll` and `SVGControl\bin\Debug\ExCSS.dll` (Part A).
  - `TaskMaster\bin\Debug\TaskMaster.dll`, `TaskMaster\bin\Debug\TaskMaster.dll.config` and
    `TaskMaster\bin\Debug\System.Linq.AsyncEnumerable.dll` (Part B).
- **Write access to** `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/`
  to record the evidence artifact.
- No registry changes, Fusion logging or network access are required for the observation itself
  (restore requires access to api.nuget.org).

## Step-by-step Instructions

### Part A — WinForms designer load (`PictureBoxSVG`)

1. Confirm the branch and that the restore and Rebuild in Prerequisites completed after the last
   change on the branch.
2. Start Visual Studio and open `TaskMaster.sln`. Visual Studio must have been closed during the build
   (Prerequisites) so that the designer loads the freshly built `SVGControl.dll`. An observation taken
   in a Visual Studio session that was open during the build is not valid evidence.
3. Open the **Output** window before the designer loads: on the menu bar choose **View** > **Output**,
   or press **Ctrl**+**Alt**+**O**.
4. In **Solution Explorer**, expand **UtilitiesCS** > **Dialogs** and select `MyBoxViewer.cs`.
5. Open it **in the designer**, not the code editor: double-click `MyBoxViewer.cs`, or right-click it
   and select **View Designer**, or select it and press **Shift**+**F7**. If the code editor opens,
   press **Shift**+**F7**. Opening `MyBoxViewer.Designer.cs` in the code editor does not exercise the
   design-time construction of `PictureBoxSVG`.
6. Wait for the designer to finish loading. Exactly one of the following occurs:
   - The form surface renders, including the `PictureBoxSVG` control. Go to step 8.
   - The designer error page appears in place of the form surface, naming an exception. Go to step 7.
7. If the error page appeared: expand the error entry, reveal the call stack, select the exception
   type, message and call stack, and copy them (**Ctrl**+**C**). Also copy the **Output** window pane
   contents (**Ctrl**+**A**, then **Ctrl**+**C** inside the pane). Do not select **Ignore and
   Continue** before copying; it reloads the designer and discards the detail.
8. Inspect the **Output** window and the **Error List** (**Ctrl**+**\\**, **E**) for any
   `NullReferenceException`, `FileNotFoundException` or `FileLoadException`, and for any logged SVG
   parse failure from `SvgRenderer`. Copy matching lines verbatim.
9. Optional corroboration: repeat steps 5 through 8 for
   `UtilitiesCS/Threading/ProgressMultiStepViewer.cs`, which also hosts `PictureBoxSVG`.
10. Close Visual Studio before starting Part B if you will start the add-in from a different Visual
    Studio instance; otherwise keep the solution open for step 12.

### Part B — Outlook add-in start check

11. Confirm Outlook is not running (Prerequisites).
12. In Visual Studio, in **Solution Explorer**, right-click the **TaskMaster** project and select
    **Set as Startup Project**. Start debugging with **F5** (**Debug** > **Start Debugging**). Visual
    Studio starts a new Outlook process and loads the VSTO add-in from `TaskMaster\bin\Debug`.
13. Wait for Outlook to finish starting. On the ribbon, select the **Taskmaster** tab and confirm the
    **Task Master** group (with **Quick Filer**, **Sort Email**, **Flag Task**) is present. If the
    **Taskmaster** tab is absent, the add-in did not load; record this as a Part B failure and go to
    step 17.
14. With a mail folder containing at least one message selected, select **Taskmaster** > **Quick
    Filer**. QuickFiler opens and constructs its item viewers (`QuickFiler/Viewers/ItemViewer`), which
    host SVGControl `ButtonSVG` controls rendered through the same `SvgImageSelector` -> `SvgRenderer`
    path as `PictureBoxSVG`. Wait until the viewer is displayed, then close QuickFiler normally.
15. Close Outlook normally through its own window (**File** > **Exit**, or the window close button).
    Closing Outlook ends the debugging session; do not stop the debugger first, because stopping the
    debugger terminates the Outlook process abruptly.
16. Open today's add-in log, `TaskMaster\bin\Debug\logs\debug_<yyyy-MM-dd>.log` (the file name
    follows the `log4net` pattern `'debug_'yyyy-MM-dd'.log'` in `TaskMaster/log4net.config`). Search it
    for the strings `FileNotFoundException` and `FileLoadException`, and for each of the following
    assembly names:
    - `Azure.Core`
    - `System.ClientModel`
    - `Microsoft.Identity.Client` (also matches `Microsoft.Identity.Client.Extensions.Msal`)
    - `Microsoft.IdentityModel.` (all seven IdentityModel assemblies)
    - `System.IdentityModel.Tokens.Jwt`
    - `Microsoft.Bcl.Memory`
    - `Microsoft.Bcl.Numerics`
    - `Microsoft.Extensions.Diagnostics.Abstractions`
    - `System.Linq.AsyncEnumerable`

    A PowerShell form of the same search, run from the repository root:

    ```
    Select-String -Path "TaskMaster\bin\Debug\logs\debug_$(Get-Date -Format 'yyyy-MM-dd').log" -Pattern 'FileNotFoundException|FileLoadException|Azure\.Core|System\.ClientModel|Microsoft\.Identity\.Client|Microsoft\.IdentityModel\.|System\.IdentityModel\.Tokens\.Jwt|Microsoft\.Bcl\.Memory|Microsoft\.Bcl\.Numerics|Microsoft\.Extensions\.Diagnostics\.Abstractions|System\.Linq\.AsyncEnumerable'
    ```

    Copy every matching line, with its timestamp, verbatim. Restrict attention to entries written
    during this session (timestamps after step 12).
17. Confirm the deployed assembly: verify that `TaskMaster\bin\Debug\System.Linq.AsyncEnumerable.dll`
    exists beside `TaskMaster\bin\Debug\TaskMaster.dll`:

    ```
    Test-Path TaskMaster\bin\Debug\System.Linq.AsyncEnumerable.dll
    ```

    The expected output is `True`. Record the result.
18. Write the evidence artifact as described under Verification.

## Verification

Record a Pass or Fail for each part independently.

### Part A — designer load

- **Pass.** The `MyBoxViewer` form surface renders, no designer error page appears, and no
  `NullReferenceException`, `FileNotFoundException` or `FileLoadException` is reported in the designer,
  the **Output** window or the **Error List**.
- **Partial pass (acceptable; must be recorded).** The form surface renders with no
  `NullReferenceException`, but the **Output** window shows a logged SVG parse failure from
  `SvgRenderer` (the #418 degradation path). Record the logged exception type and message verbatim.
  This is the pre-existing #418 behaviour in the designer host and is not attributable to #973,
  because the designer applies no project `app.config`.
- **Fail.** A `NullReferenceException` is reported anywhere, or the designer error page blocks the form
  surface. Capture the full text per step 7 and return the result to the orchestrator.

State in the evidence that a Part A Pass is non-regression evidence only: the designer runs inside
`devenv.exe`, never applies a project `app.config`, and `PictureBoxSVG`'s Svg/ExCSS/Fizzler chain is
disjoint from the corrected assembly families.

### Part B — add-in start

- **Pass.** All of the following hold: the **Taskmaster** ribbon tab appears; QuickFiler opens and
  displays its item viewer; the log search in step 16 returns no `FileNotFoundException` or
  `FileLoadException` naming any listed assembly for entries written during this session; and step 17
  returns `True`.
- **Fail.** Any of the following: the **Taskmaster** tab is absent; Outlook reports the add-in as
  disabled or inactive; QuickFiler fails to open; the log contains a `FileNotFoundException` or
  `FileLoadException` naming a listed assembly; or `System.Linq.AsyncEnumerable.dll` is absent from
  `TaskMaster\bin\Debug`. Capture the log lines verbatim and return the result to the orchestrator.

Log lines that name a listed assembly without a load exception (for example an informational load
message) are not failures; copy them into the evidence and classify them as informational.

### Evidence capture (mandatory location)

Write the evidence artifact to:

```
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md
```

Read `<yyyy-MM-ddTHH-mm>` from the system clock at the time of the observation, for example:

```
Get-Date -Format 'yyyy-MM-ddTHH-mm'
```

Do not estimate or reuse a timestamp. Evidence must not be written to any `artifacts/`-rooted path;
the PreToolUse hook `.claude/hooks/enforce-evidence-locations.ps1` blocks non-canonical evidence
locations (`EVIDENCE_LOCATION_BLOCKED`).

The artifact must contain, at minimum:

- `Timestamp: <ISO-8601 timestamp read from the clock, matching the filename>`
- `Branch: bug/remaining-stale-binding-redirect-pairs-973`
- `HEAD: <output of git rev-parse HEAD>`
- `Visual Studio: <product name and version, as shown for the installed instance in the Visual Studio Installer>`
- `Build: Debug|Any CPU; restore and /t:Rebuild completed with 0 Error(s)`
- `Visual Studio restarted after the build: yes | no` (a `no` invalidates Part A)
- `Part A — MyBoxViewer designer load: Pass | Partial pass | Fail`, plus the optional
  `ProgressMultiStepViewer` result if step 9 was performed
- The Part A non-regression statement from Verification above
- `Part B — Taskmaster ribbon tab present: Pass | Fail`
- `Part B — QuickFiler item viewer opened: Pass | Fail`
- `Part B — log search (no FileNotFoundException/FileLoadException for listed assemblies): Pass | Fail`,
  with the log file path
- `Part B — System.Linq.AsyncEnumerable.dll beside TaskMaster.dll: Pass | Fail`
- `EXIT_CODE: 0` if both parts are Pass or Partial pass; `EXIT_CODE: 1` otherwise
- Verbatim excerpts: designer error page text and call stack (if any), relevant **Output** window
  lines, and every log line matched in step 16 (or the statement `no matches`)

Optional screenshots may be placed in the same `evidence/regression-testing/` directory and referenced
by relative path.

## Source and Citation

**Sourcing-order note.** The skill's sourcing rule is MCP-first, then web-second. No callable MCP
documentation-retrieval tool is wired in this repository at this time; a repository-wide search for an
`mcp__*` documentation tool found none. This limitation is recorded in the two-axis-model-selection
spec's Out of Scope section and is a repository-wide condition. The MCP-first clause therefore could
not be satisfied for the third-party UI steps below, and `WebFetch` against current Microsoft Learn
documentation was used as the sole available web-second mechanism. Every source below was fetched on
2026-10-02.

Third-party UI and CLI sources (web-second; MCP unavailable per the note above):

- Designer host for .NET Framework projects (Background, Part A, step 2) — Microsoft Learn, "Designers
  changes from .NET Framework - Windows Forms": "With a .NET Framework project, both the Visual Studio
  environment and the Windows Forms app being designed, run within the same process: devenv.exe."
  Source URL:
  https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls-design/designer-differences-framework
  — updated_at: 2026-04-14. Captured: 2026-10-02.
- Opening the **Output** window (step 3) — Microsoft Learn, "Output Window - Visual Studio (Windows)":
  "To open the Output window, on the menu bar, choose View > Output, or press Ctrl+Alt+O." Source URL:
  https://learn.microsoft.com/en-us/visualstudio/ide/reference/output-window — updated_at: 2026-08-13.
  Captured: 2026-10-02.
- Keyboard shortcuts (steps 3, 5, 8, 12) — Microsoft Learn, "Keyboard shortcuts - Visual Studio
  (Windows)": `View.ViewDesigner` **Shift+F7**; `View.Output` **Ctrl+Alt+O**; `View.ErrorList`
  **Ctrl+\\, E** or **Ctrl+\\, Ctrl+E**; `Debug.Start` **F5**. Source URL:
  https://learn.microsoft.com/en-us/visualstudio/ide/default-keyboard-shortcuts-in-visual-studio —
  updated_at: 2026-08-13. Captured: 2026-10-02.
- Starting a VSTO add-in under the debugger and stopping behaviour (Prerequisites, steps 12 and 15) —
  Microsoft Learn, "Debug Office projects - Visual Studio (Windows)": "When you start debugging a VSTO
  Add-in project, a new process for the targeted Office application is started and the VSTO Add-in is
  loaded"; "When you stop the debugger, the debugger terminates the application process abruptly";
  "close any open instances of the Office application before you build and debug it"; and the
  hard-disabled / soft-disabled add-in conditions used in the Part B Fail criteria. Source URL:
  https://learn.microsoft.com/en-us/visualstudio/vsto/debugging-office-projects — updated_at:
  2026-04-24. Captured: 2026-10-02.
- Binding redirects are read from the application configuration file, and plug-in hosts may not honour
  `.dll.config` files (Background) — Microsoft Learn, "Redirecting Assembly Versions - .NET Framework":
  "Plugins might honor .dll.config files, however, they also might not. The only fool-proof mechanism
  for redirects is by providing bindingRedirects when the AppDomain is created." Source URL:
  https://learn.microsoft.com/en-us/dotnet/framework/configure-apps/redirect-assembly-versions —
  updated_at: 2026-03-03. Captured: 2026-10-02.
- Restore command for packages.config projects (Prerequisites) — Microsoft Learn, "NuGet pack and
  restore as MSBuild targets": "With MSBuild 16.5+, packages.config are also supported for
  msbuild -t:restore" with `msbuild -t:restore -p:RestorePackagesConfig=true`, and "The restore target
  should not be run in combination with the build target." Source URL:
  https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets — updated_at: 2026-07-20.
  Captured: 2026-10-02.
- Build command switches `-target`, `-property`, `-maxCpuCount` (Prerequisites) — Microsoft Learn,
  "MSBuild Command-Line Reference". Source URL:
  https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference — updated_at:
  2026-08-13. Captured: 2026-10-02.
- Locating the installed Visual Studio instance and its version in the Visual Studio Installer
  (evidence field `Visual Studio:`) — Microsoft Learn, "Update Visual Studio installation to recent
  release": "In the Windows Start menu, search for 'installer', and then select Visual Studio Installer
  from the results", then "look for the installation of Visual Studio". Source URL:
  https://learn.microsoft.com/en-us/visualstudio/install/update-visual-studio — updated_at:
  2026-04-24. Captured: 2026-10-02.

Repository sources (primary for the mechanism, scope, paths and evidence contract; read 2026-10-02):

- Base runbook adapted for Part A:
  `docs/features/archive/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md`.
- Automation feasibility, designer insensitivity, Outlook-host smoke and the latent-bind analysis:
  `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/research/2026-10-02T22-50-stale-binding-redirect-pairs-research.md`
  (sections 4, 5.4, 6 and 8).
- `System.Linq.AsyncEnumerable` install, restore route, expected output locations and the add-in log
  check: `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/research/2026-10-02T23-40-system-linq-asyncenumerable-install-research.md`
  (sections 8, 11 and Automation Feasibility).
- Acceptance criterion text: `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/issue.md`
  and `spec.md` (Test Strategy).
- Ribbon tab, group and **Quick Filer** button labels (steps 13–14): `TaskMaster/Ribbon/RibbonExplorer.xml`
  (tab `TabTaskMaster` label "Taskmaster"; group "Task Master"; button "Quick Filer").
- VSTO project type, Outlook target and `bin\Debug\` output path (Prerequisites, step 12):
  `TaskMaster/TaskMaster.csproj`.
- Add-in log location and file-name pattern (step 16): `TaskMaster/log4net.config`
  (`<file value="logs\\" />`, `<datePattern value="'debug_'yyyy-MM-dd'.log'" />`).
- Forms hosting `PictureBoxSVG` (steps 4 and 9): `UtilitiesCS/Dialogs/MyBoxViewer.Designer.cs`,
  `UtilitiesCS/Threading/ProgressMultiStepViewer.Designer.cs`; shared render path for `ButtonSVG`
  (step 14): `SVGControl/ButtonSVG.cs`, `SVGControl/SvgImageSelector.cs`; QuickFiler viewer:
  `QuickFiler/Viewers/ItemViewer.Designer.cs`.
- Restore command used by the repository: `scripts/vscode/Invoke-Restore.ps1`.
- Canonical evidence location and timestamp convention:
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`; enforcement:
  `.claude/hooks/enforce-evidence-locations.ps1`.
- Prohibition on temporary files in tests, which rules out a surrogate-AppDomain automation:
  `.claude/rules/general-unit-test.md`, "External Dependencies".
