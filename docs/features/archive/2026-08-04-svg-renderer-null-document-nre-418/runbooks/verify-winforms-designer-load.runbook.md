# Human-Exception Runbook — Verify WinForms Designer Load of `MyBoxViewer` (Issue #418)

This runbook is the human follow-up for the `exception` response recorded against the requirement
"confirm the Visual Studio WinForms designer loads `MyBoxViewer` without a `NullReferenceException`"
on bug #418. It is contract-conformant per `.claude/skills/human-exception-runbook/SKILL.md` (Cue,
Prerequisites, Step-by-step Instructions, Verification, Source and Citation).

The requirement is unautomatable. Reproducing the designer's assembly-resolution environment
requires the `devenv.exe` AppDomain, the Visual Studio configuration file, and the designer's
shadow-copy type-resolution service, none of which can be created from a test process. The only
automatable substitute — constructing a surrogate `AppDomain` with a hand-authored
`AppDomainSetup.ConfigurationFile` — requires writing a synthetic `.config` file to disk, which the
repository's UT4 policy prohibits with zero approved exceptions
(`.claude/rules/general-unit-test.md`, "External Dependencies").

## Background (what is being verified and why)

`SVGControl.SvgRenderer`'s byte-array constructors previously threw a `NullReferenceException`.
`SvgRenderer.GetSvgDocument` caught every exception raised by `Svg`'s `SvgDocument.Open` and returned
`null`; the constructor then dereferenced that `null`.

The underlying cause is an assembly-binding mismatch. The deployed `Svg 3.4.7` assembly carries a
reference to `ExCSS, Version=4.2.3.0`, but only `ExCSS 4.3.1.0` is deployed. For a .NET Framework
project, the WinForms designer runs in-process inside `devenv.exe`, so `SVGControl.dll` is loaded
into the Visual Studio process, whose configuration file carries no ExCSS binding redirect. The bind
therefore fails in the designer. Production is unaffected: the product ships as a VSTO add-in inside
`OUTLOOK.EXE`, and the per-add-in AppDomain applies `TaskMaster.dll.config`, which redirects ExCSS
correctly.

Two parts of the fix are relevant to this runbook:

- The byte-array `SvgRenderer` constructors no longer throw. When the document cannot be produced,
  the constructor logs the real exception at error level through the existing `log4net` logger and
  degrades to a blank image rather than dereferencing `null` (AC-3).
- The `AssemblyResolve` fallback in `SVGControl/SvgRenderer.cs` gains directory probing so it can
  find `ExCSS.dll` next to `SVGControl.dll` rather than only on the host's probing path (AC-8).

Because the first part is in place, this single manual observation produces evidence for two
acceptance criteria at once: it demonstrates AC-3's degradation path, and — if the bind still fails —
it captures the observed exception type and message that AC-7 requests, which was previously
unavailable because the exception was discarded.

## Cue

Act on this runbook at exactly one point in the workflow: **after** the atomic-executor has reported
the #418 fix complete and the C# toolchain green (CSharpier, the analyzer build, the
nullable/`TreatWarningsAsErrors` build, and `vstest.console.exe` all passing in one consecutive pass,
per AC-6), and **before** the feature is reported done.

This runbook satisfies **AC-11 — Designer load verified by the documented human step** in
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`. AC-11 is satisfied
only when this runbook has been executed and its evidence artifact written to the feature folder.
Do not run this runbook before the fix is implemented; a pre-fix run produces no usable evidence,
because the pre-fix code discards the exception this runbook is intended to capture.

## Prerequisites

- **Visual Studio with the WinForms designer for .NET Framework 4.8.1.** The projects in this
  repository target `net481`, so the classic in-process designer is used and design-time control code
  executes inside `devenv.exe`. The out-of-process `DesignToolsServer.exe` designer is not used for
  these projects. Verify that the "Windows Forms Designer" and .NET Framework 4.8.1 targeting pack
  components are installed.
- **The repository built in `Debug|Any CPU` on branch `bug/svg-renderer-null-document-nre-418`**, with
  the #418 fix present. Confirm the checked-out branch before building. Build with the repository's
  standard command:

  ```
  msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
  ```

  Confirm `SVGControl/bin/Debug/SVGControl.dll` and `SVGControl/bin/Debug/ExCSS.dll` both exist after
  the build. `ExCSS.dll` sitting next to `SVGControl.dll` is what the AC-8 directory-probing fallback
  depends on.
- **Designer assembly caching — close and reopen before observing.** The designer loads control
  assemblies into the running Visual Studio process and does not release them when the project is
  rebuilt. If Visual Studio was open during the build, the designer may still be holding the previous
  `SVGControl.dll`. Before performing the steps below, either close and reopen the solution, or
  restart Visual Studio entirely. Restarting Visual Studio is the more reliable of the two and is
  recommended when a prior designer session already reported the error. An observation made without
  this step is not valid evidence, because it may reflect the pre-fix assembly.
- **Write access to the feature folder**
  `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/`, to record the evidence
  artifact.
- No administrator rights, registry changes, or network access are required. Do not enable Fusion
  assembly-binding logging for this runbook; it is not needed once the fix stops discarding the
  exception.

## Step-by-step Instructions

1. Confirm the working tree is on branch `bug/svg-renderer-null-document-nre-418` and that the
   solution has been built in `Debug|Any CPU` after the fix was applied (see Prerequisites).
2. If Visual Studio is already running with `TaskMaster.sln` open, close it. Start Visual Studio and
   open `TaskMaster.sln`. This guarantees the designer loads the freshly built `SVGControl.dll`.
3. Open the **Output** window so it is visible before the designer loads: on the menu bar choose
   **View** > **Output**, or press **Ctrl**+**Alt**+**O**. In the **Show output from** list, keep the
   default pane selected; the designer writes load diagnostics here.
4. In **Solution Explorer**, expand the **UtilitiesCS** project, then the **Dialogs** folder, and
   locate `MyBoxViewer.cs`.
5. Open the file **in the designer, not in the code editor**. Either double-click `MyBoxViewer.cs`
   (which opens the designer view for a form), or right-click `MyBoxViewer.cs` and select **View
   Designer**, or select the file and press **Shift**+**F7** (`View.ViewDesigner`). If the code
   editor opens instead, press **Shift**+**F7** to switch to the designer view. Opening
   `MyBoxViewer.Designer.cs` in the code editor is not the same thing and does not exercise the
   design-time control-construction path.
6. Wait for the designer to finish loading. One of two outcomes occurs:
   - **The form surface renders.** The design surface shows the form with its controls, including the
     `PictureBoxSVG` control. Proceed to step 8.
   - **The designer error page appears.** Instead of the form surface, the designer window shows a
     text panel reporting that an error occurred while loading the document, together with the
     exception type and message. Proceed to step 7.
7. If the designer error page appeared, capture the full text:
   1. On the error page, expand the exception entry. The panel provides links to show the details and
      the call stack for each error instance (for example an instances count link and a link to show
      or hide the call stack). Select the link that reveals the call stack so the full stack trace is
      visible.
   2. Select the revealed text (the exception type, message, and call stack) and copy it with
      **Ctrl**+**C**. The error page text is selectable and copyable.
   3. Also copy the contents of the **Output** window pane from step 3, using **Ctrl**+**A** then
      **Ctrl**+**C** inside the pane.
   4. Do not select **Ignore and Continue** before capturing the text; that action reloads the
      designer and discards the error detail.
8. Whether or not the error page appeared, inspect the **Output** window for a logged SVG parse
   failure written by `SvgRenderer`. The fix logs the underlying exception at error level through
   `log4net`. Copy the matching lines verbatim, including the exception type and message. If the
   repository's `log4net` configuration also writes to a file appender in this host, copy the
   corresponding log file entries as well and note the log file path.
9. Optionally, repeat steps 5 through 8 for `UtilitiesCS/Dialogs/FolderNotFoundViewer.cs` and
   `QuickFiler/Viewers/ItemViewer.cs`. Both host the same `PictureBoxSVG` control on the same
   construction path. This is corroborating evidence only; `MyBoxViewer.cs` is the criterion named by
   AC-11.
10. Optionally, and only if the designer error page reported a failure to load `ExCSS`, open
    `%LOCALAPPDATA%\Microsoft\VisualStudio\<version>\ProjectAssemblies\` in File Explorer and record
    whether `ExCSS.dll` is present in the same subdirectory as the shadow-copied `SVGControl.dll`.
    This resolves an open question about whether the AC-8 directory-probing fallback can succeed in
    the designer host. Record the answer as a plain observation; do not modify anything in that
    directory.
11. Write the evidence artifact as described under Verification.

## Verification

Classify the observation into exactly one of the three outcomes below, then record it.

### Pass

The form surface renders in the designer, no designer error page appears, and no
`NullReferenceException` is reported in the designer, the **Output** window, or the **Error List**.
The `PictureBoxSVG` control is visible on the design surface with its image.

This outcome indicates both remedies worked: the constructor no longer throws, and the ExCSS bind
succeeded in the designer host.

### Partial pass (acceptable; must still be recorded)

The form surface renders and no `NullReferenceException` appears, **but** the **Output** window or the
`log4net` output shows a logged SVG parse failure from `SvgRenderer`.

This outcome is acceptable and satisfies AC-11. It means the AC-3 degradation path worked as
designed — the parse failure produced a blank image and a logged, diagnosable error instead of a
`NullReferenceException` — even though the ExCSS bind still failed in the designer host.

**The operator must capture the logged exception type and message verbatim.** That capture is what
satisfies AC-7's request for the observed exception identity. Before the fix, this exception was
discarded and could not be observed anywhere. The research artifact predicts
`System.IO.FileNotFoundException` naming
`ExCSS, Version=4.2.3.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a`, but records that
prediction as inferred rather than observed. Record what is actually observed, whatever it is, and do
not adjust it to match the prediction.

### Fail

Either of the following:

- A `NullReferenceException` is reported anywhere — the designer error page, the **Output** window, or
  the **Error List**. AC-3 requires that a `NullReferenceException` is never the observed failure
  mode, so this outcome means the fix is incomplete.
- The designer error page still blocks the form: the design surface does not render at all,
  regardless of which exception is named.

On a Fail outcome, capture the full exception text and call stack per step 7, record the evidence
artifact, and return the result to the orchestrator for remediation. Do not mark AC-11 satisfied.

### Evidence capture (mandatory location)

Write the evidence artifact to:

```
docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md
```

Replace `<yyyy-MM-ddTHH-mm>` with the ISO-8601 timestamp of the observation, using the repository
convention (for example `2026-08-04T16-45`). `evidence/regression-testing/` is the correct `<kind>`
subdirectory because this observation is the manual regression check for the reported defect.

This path is mandatory. Evidence must never be written to any `artifacts/`-rooted path. The PreToolUse
hook `.claude/hooks/enforce-evidence-locations.ps1` blocks writes to non-canonical evidence locations
(including `artifacts/evidence/`, `artifacts/regression-testing/`, and `artifacts/qa-gates/`) and
returns `EVIDENCE_LOCATION_BLOCKED`. The canonical scheme is `<FEATURE>/evidence/<kind>/` per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.

The artifact must contain, at minimum:

- `Timestamp: <ISO-8601 timestamp, matching the filename>`
- `Command: opened UtilitiesCS/Dialogs/MyBoxViewer.cs in the Visual Studio WinForms designer` (name
  any additional forms opened under step 9)
- `EXIT_CODE: 0` for Pass or Partial pass; `EXIT_CODE: 1` for Fail
- `Outcome: Pass | Partial pass | Fail`
- `Branch: bug/svg-renderer-null-document-nre-418` and the commit SHA that was built
- The Visual Studio product name and version, and the build configuration used (`Debug|Any CPU`)
- Whether Visual Studio was restarted or the solution reopened after the build (Prerequisites)
- The verbatim designer error page text and call stack, if any
- The verbatim logged exception type and message from `SvgRenderer`, if any — required for a Partial
  pass, since this is the AC-7 capture
- The verbatim **Output** window lines relating to the designer load
- The step 9 results for the additional forms, if performed
- The step 10 `ProjectAssemblies` observation, if performed
- A screenshot of the design surface or the error page is optional; if included, place the image file
  in the same `evidence/regression-testing/` directory and reference it by relative path

## Source and Citation

**Sourcing-order note.** The skill's sourcing rule is MCP-first, then web-second. No callable MCP
documentation-retrieval tool is wired in this repository at this time; a repo-wide search for an
`mcp__*` documentation tool found none. This limitation is recorded in the two-axis-model-selection
spec's Out of Scope section and is a repository-wide condition, not specific to this runbook. The
MCP-first clause therefore could not be satisfied for the third-party UI steps below, and `WebFetch`
against current vendor documentation was used as the sole available web-second mechanism. This runbook
does not attempt to resolve that limitation.

Third-party UI sources (web-second; MCP unavailable per the note above):

- Designer host identity for .NET Framework projects (Step 2, Prerequisites) — Microsoft Learn,
  "Designers changes from .NET Framework - Windows Forms": "With a .NET Framework project, both the
  Visual Studio environment and the Windows Forms app being designed, run within the same process:
  **devenv.exe**." Source URL:
  https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls-design/designer-differences-framework
  — updated_at: 2026-04-14. Captured: 2026-08-04.
- In-process designer applicability to .NET Framework and platform-target caveats (Prerequisites) —
  Microsoft Learn, "Debug Custom Controls at Design Time - Windows Forms," which states the article is
  "primarily intended for the classic In-Process Designer for Windows Forms with .NET Framework," and
  documents opening a form in the **Windows Forms Designer**. Source URL:
  https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/walkthrough-debugging-custom-windows-forms-controls-at-design-time
  — updated_at: 2025-08-27. Captured: 2026-08-04.
- Opening the **Output** window (Step 3) — Microsoft Learn, "Output Window - Visual Studio (Windows)":
  "To open the **Output** window, on the menu bar, choose **View** > **Output**, or press
  **Ctrl**+**Alt**+**O**," and the **Show output from** pane selector. Source URL:
  https://learn.microsoft.com/en-us/visualstudio/ide/reference/output-window — updated_at: 2026-07-07.
  Captured: 2026-08-04.
- **View Designer** command and keyboard shortcut (Step 5) — Microsoft Learn, "Keyboard shortcuts -
  Visual Studio (Windows)": `View.ViewDesigner` is bound to **Shift+F7**; `View.ViewCode` to **F7**;
  `View.Output` to **Ctrl+Alt+O**; `View.ErrorList` to **Ctrl+\, E** or **Ctrl+\, Ctrl+E**. Source URL:
  https://learn.microsoft.com/en-us/visualstudio/ide/default-keyboard-shortcuts-in-visual-studio —
  updated_at: 2026-07-07. Captured: 2026-08-04.
- Design-time control instantiation and rebuild/reload behavior (Prerequisites, designer caching) —
  Microsoft Learn, "Troubleshooting Control and Component Authoring - Windows Forms," which documents
  that design-time control code is executed by the design environment and that a control must be
  reloaded for a rebuilt assembly to be picked up. Source URL:
  https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/troubleshooting-control-and-component-authoring
  — updated_at: 2025-08-27. Captured: 2026-08-04.

Repository sources (non-UI; primary for the mechanism, the acceptance criteria, and the evidence
contract):

- Research artifact establishing the mechanism, the host matrix, the `AssemblyResolve` failure mode,
  and requirements H-1 and H-2 with their favourable sequencing:
  `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/research/2026-08-04T15-05-svg-renderer-null-document-research.md`
  (sections 2, 3, 4, and 9). Captured/read: 2026-08-04.
- Acceptance criteria AC-3, AC-6, AC-7, AC-8, and AC-11:
  `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`. Captured/read:
  2026-08-04.
- Defect surface, the `AssemblyResolve` fallback, and the `log4net` logger used for the AC-3
  degradation message: `SVGControl/SvgRenderer.cs`. Captured/read: 2026-08-04.
- Construction path from the control to the byte-array `SvgRenderer` constructor, and the hardcoded
  default SVG payload: `SVGControl/SvgImageSelector.cs`. Captured/read: 2026-08-04.
- Designer-generated construction of `PictureBoxSVG` on the form under test:
  `UtilitiesCS/Dialogs/MyBoxViewer.Designer.cs`. Captured/read: 2026-08-04.
- Canonical evidence location and the `yyyy-MM-ddTHH-mm` timestamp convention:
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Captured/read: 2026-08-04.
- Enforcement of the evidence location, including the forbidden `artifacts/` sub-paths and the
  `EVIDENCE_LOCATION_BLOCKED` decision reason: `.claude/hooks/enforce-evidence-locations.ps1`.
  Captured/read: 2026-08-04.
- Prohibition on temporary files in tests, which rules out the surrogate-`AppDomain` automation
  alternative: `.claude/rules/general-unit-test.md`, "External Dependencies". Captured/read:
  2026-08-04.
