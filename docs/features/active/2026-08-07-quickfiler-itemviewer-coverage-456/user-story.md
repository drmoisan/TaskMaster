# `quickfiler-itemviewer-coverage` — User Story

- Issue: #456
- Parent: epic `quickfiler-per-file-coverage`, issue #136 (child F14, wave 1)
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-07T23-45
- Work Mode: full-feature

## Framing — This Is an Enabler, Not a User-Visible Change

This feature delivers **no change an end user of QuickFiler can observe**. That is a deliberate constraint,
not an omission: the parent epic's non-functional requirement is "No behavior change to end-user QuickFiler
flows; testability refactors preserve observable behavior" (`epic.md:17`), and this child's acceptance
criteria enforce it.

The beneficiaries are **the maintainer of QuickFiler and the autonomous agents that modify it**. The value
delivered is of two kinds, both measurable:

1. **Regression-escape reduction.** Ten production files totalling ~1,345 testable lines plus a 6,224-line
   generated designer are currently invisible to coverage measurement. A change to any of them today produces
   no coverage signal at all, so a regression in the item viewer's display, command, folder-search, or
   breadcrumb wiring can merge with no test failure and no metric movement. After this feature, every one of
   those files reports a real per-file line and branch figure and is gated at >= 80% / >= 75%.
2. **Removal of an unratified blanket exemption.** A single `[ExcludeFromCodeCoverage]` at
   `QuickFiler/Viewers/ItemViewer.cs:20` suppresses instrumentation for the entire partial type. Under the
   epic's ratified policy reconciliation (`epic.md:205-225`), `[ExcludeFromCodeCoverage]` on a testable seam
   is a Blocking finding, and this attribute has never been argued against the irreducible-remainder standard.
   No member of `ItemViewer.cs` touches Outlook Interop, and `ItemViewer` is a `UserControl`, not a `Form`
   (`ItemViewer.cs:21`), so neither ground of the `CLAUDE.md` § UT2 exemption applies. The exemption is
   unratified rather than legitimate, and removing it converts a blanket claim of untestability into a
   measured, defensible position.

## Story Statement

- As the **maintainer of QuickFiler**, I want every testable file in the `ItemViewer` family measured and
  gated at >= 80% line and >= 75% branch coverage, so that a regression in item display, command forwarding,
  folder search, or breadcrumb wiring fails a test instead of reaching a user.
- As an **autonomous agent modifying QuickFiler**, I want the `ItemViewer` family instrumented rather than
  blanket-exempt, so that the coverage signal I rely on to judge whether my change is safe actually responds
  to what I changed.
- As the **reviewer of a QuickFiler pull request**, I want the family's coverage exemption either removed or
  reduced to a specific irreducible remainder with a file-specific rationale in F1's ledger, so that I can
  tell the difference between "this code is genuinely untestable" and "nobody has argued about it yet".
- As the **owner of a sibling epic child** (F1, F7, F10, F12, F13, F15), I want F14's dependencies on my
  files stated as explicit frozen contracts and advisory notes, so that my own coverage work does not silently
  break F14's tests or F14's measured figures.

## Problem / Why

The `ItemViewer` form family under `QuickFiler/Viewers/` is entirely invisible to coverage measurement.
`ItemViewer` is a partial type spread across six hand-written source files plus a 6,224-line generated
designer file, and the single `[ExcludeFromCodeCoverage]` attribute at `ItemViewer.cs:20` suppresses
instrumentation for the whole type. This is confirmed rather than assumed: the committed Cobertura report
contains no `<class>` element for any `QuickFiler\Viewers\ItemViewer*.cs` file, while same-folder siblings
such as `Viewers\ItemViewerExpanded.cs` (XML `:5364`) and `Viewers\BreadcrumbUiDispatcher.cs` (XML `:8874`)
are present, proving the folder was instrumented.

The one member of the family that is measured, `QuickFiler/Viewers/ItemViewerExpanded.cs`, sits at **37.74%
line and 8.33% branch** when recomputed from the report's own `<line>` children — below both the 80% line
gate that issue #136 sets and the 75% branch gate that `.claude/rules/general-unit-test.md` sets. That
coverage is also incidental: it is produced entirely by an F7-owned test constructing a live `QfcFormViewer`,
so it would collapse if F7 replaces that construction with a seam.

The family is not, however, untested in practice. Ten existing plain `[TestMethod]`s already construct a live
headless `ItemViewer` and drive parts of it as a fixture for some other subject. So the family is
simultaneously **executed and unmeasured** — the worst of both positions, because the work already done is
invisible and the gaps are unknown.

## Personas & Scenarios

### Persona — QuickFiler maintainer

- **Who:** the developer responsible for QuickFiler's behavior in Outlook, working in a legacy non-SDK
  VSTO/WinForms/.NET Framework 4.8.1 codebase.
- **What they care about:** that a change to the item viewer does not silently break filing, conversation
  display, folder search, or the breadcrumb selector for users.
- **Constraints:** cannot run unit tests against a live Outlook process; cannot show forms or popups in unit
  tests; cannot break the `IItemViewer` contract, which is consumed by `QfcItemController.*` mocks (F10) and
  by `EfcItemController.cs:247` (F9).
- **Frustration today:** the coverage report is silent on ten files. A pull request touching
  `ItemViewer.FolderSearch.cs` produces the same coverage number as one touching nothing.

### Persona — autonomous agent modifying QuickFiler

- **Who:** an agent executing an atomic plan under this repository's policy stack.
- **What they care about:** a trustworthy signal that a change is covered, because coverage is one of the few
  automated checks that can catch a behavioral regression it did not anticipate.
- **Constraint:** the agent cannot distinguish "0% because untested" from "absent because exempt" without
  reading the attribute. `epic.md:187` states the rule plainly — *an absent file is not a covered file*.

### Scenario 1 — a regression in the folder-search forwarders reaches a user (today)

An agent edits `ItemViewer.FolderSearch.cs` to satisfy an unrelated request and changes a forwarder's null
handling. Nothing fails: no test executes any line of that file, and the file emits no coverage row, so the
per-file gate cannot notice. The full toolchain passes. The regression ships. A user types in the folder
search box and the breadcrumb no longer populates.

### Scenario 2 — the same regression after this feature

The same edit lands. The per-file harness reports `ItemViewer.FolderSearch.cs`'s line and branch rate; the
forwarding tests that pin the documented "on a bare viewer the members are inert" contract fail; the change
is stopped before merge. If the change was intentional, the red test is a legible signal that a documented
contract moved, not a mystery.

### Scenario 3 — a reviewer asks why the family is exempt (today)

The reviewer greps for `[ExcludeFromCodeCoverage]`, finds one attribute at `ItemViewer.cs:20`, and has no
way to tell whether it is justified. Four sibling files carry comments *asserting* the exemption
(`ItemViewer.Commands.cs:10`, `ItemViewer.DisplayState.cs:9-10`, `ItemViewer.FolderSearch.cs:17`,
`ItemViewer.WebViewThread.cs:8-12`) while carrying no attribute themselves — comments that caused the epic's
original 33-file over-count (`epic.md:121-130`). The reviewer either accepts the exemption on faith or spends
an afternoon re-deriving it.

### Scenario 4 — the same question after this feature

The attribute is gone, the four stale comments are corrected, F1's ledger carries a row per file with its
bucket and measured figure, and any residual uncovered line is recorded as a named, argued remainder — for
example the two `FocusSearch` lines that marshal through `Control.Invoke` and require a real window handle,
or the three lines in each designer's `Dispose` guard that are unreachable because `components` is never
assigned. The reviewer reads the answer instead of reconstructing it.

### Scenario 5 — a sibling child breaks F14 without knowing (the risk this feature must manage)

F15 "fixes" `ToolStripMenuItemCb`'s shadowing `Checked` setter by adding `base.Checked = value;` at
`ToolStripMenuItemCb.cs:37` (issue #486). F14's menu test cases, which depend on that setter raising
`CheckedChanged` unconditionally, go red. Or F13 removes
`BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` as unused test-only surface, and F14's entire
off-context test path disappears. The spec's `## Cross-Child Notes` section exists to make both of these
visible as frozen contracts before they happen.

## Acceptance Criteria

These mirror the authoritative list in `spec.md` and are tracked independently in this file per
`.claude/skills/acceptance-criteria-tracking/SKILL.md`.

- [ ] AC1 — Every file in scope classified `testable` reaches **>= 80% line and >= 75% branch** coverage,
      measured with F1's per-file harness on this feature's branch, with the numeric per-file result committed
      under `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/evidence/qa-gates/`. Figures
      come from F1's recomputed per-file numbers, never from a `<class>` `line-rate` attribute (#441). A
      zero-`<condition>` file is reported N/A for branch, not 0%.
- [ ] AC2 — `[ExcludeFromCodeCoverage]` is removed from `QuickFiler/Viewers/ItemViewer.cs:20` and the seven
      files it was suppressing are genuinely covered per AC1, **unless** F1's ledger ratifies a specific
      irreducible remainder with a file-specific rationale. The removal lands in the same commit as at least
      one F14-owned test that constructs a real `ItemViewer`. Re-exempting the whole partial type, or
      re-exempting individual members, is prohibited.
- [ ] AC3 — `QuickFiler/Viewers/IItemViewer.cs` is classified `interface-only / not-measured`, receives no
      `[ExcludeFromCodeCoverage]` attribute and no other edit, is reported **N/A** rather than 0%, and has
      **zero** tests written for it. No shape-assertion test is added.
- [ ] AC4 — `QuickFiler/Viewers/ItemViewer.Designer.cs` and
      `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` are each classified per the ledger's generated-code
      rules; per deviation D3 that classification is **`testable`**, with the recorded structural caps.
      Neither file is edited, and neither receives an exemption.
- [ ] AC5 — No production file in scope exceeds **500 lines** after refactor, and the generated
      `*.Designer.cs` files are recorded as exempt from that rule as generated code.
- [ ] AC6 — `QuickFiler/Viewers/ControlColumnTrimmer.cs` is created, is referenced by a
      `<Compile Include="Viewers\ControlColumnTrimmer.cs" />` entry in `QuickFiler/QuickFiler.csproj` with
      CRLF preserved and minimal adjacent hunks, has a ledger row appended in that same change, and reaches
      **>= 90% line coverage**. `IItemViewer.cs:131`'s signature is unchanged by the extraction.
- [ ] AC7 — All new and modified tests use **MSTest**, **Moq**, and **FluentAssertions**, follow
      Arrange-Act-Assert, and are independent, isolated, and deterministic: no temporary files, no external
      services, no live `Form`, no popup, no `Thread.Sleep`/`Task.Delay`/wall-clock wait. No `*.StaTests.cs`
      file is created in `QuickFiler.Test`. Every test constructing an `ItemViewer` installs a
      `SynchronizationContext` first and restores the previous one.
- [ ] AC8 — The full C# toolchain passes in order in its final form — `csharpier .`, the analyzer build, the
      nullable/`TreatWarningsAsErrors` build, and `vstest.console.exe ... /EnableCodeCoverage` — with exact
      commands and results recorded. Repository-wide line coverage is measured before and after and is
      **retained or improved** against the measured baseline.
- [ ] AC9 — No observable behavior change to QuickFiler flows. No public `IItemViewer` member is added,
      removed, retyped, or renamed; no Designer-backed property is retyped; no event wiring is added or
      removed.
- [ ] AC10 — The three stale `[ExcludeFromCodeCoverage]` comments at `ItemViewer.Commands.cs:10`,
      `ItemViewer.DisplayState.cs:9-10`, and `ItemViewer.FolderSearch.cs:17`, and the header at
      `ItemViewer.WebViewThread.cs:8-12`, are corrected in the same change that removes the attribute. The
      CS0579 note at `ItemViewer.DisplayState.cs:10` is retained.
- [ ] AC11 — Issue **#438** is **not** fixed by this feature, and every test case asserting the current
      `SetFolderDroppedDown(true)` → `FocusBreadcrumb()` behavior carries an in-code comment citing #438 and
      stating that it pins current behavior.
- [ ] AC12 — Any latent defect surfaced during execution that is out of scope under the no-behavior-change
      NFR is promoted to a GitHub issue through the MCP promotion lifecycle. The already-promoted set — #486,
      #487, #488, #489, #490, #491 — is referenced, not re-promoted.

## Non-Goals

- Any user-visible change. This feature is an enabler; if a user notices anything, that is a defect against
  AC9.
- Fixing any of the promoted latent defects: #438, #440, #441, #457, #486, #487, #488, #489, #490, #491.
- Editing any F10, F12, F13, or F15 production file, or `QuickFiler/Viewers/IItemViewer.cs`, or either
  `*.Designer.cs` file.
- Editing `coverage.config`, `TaskMaster.runsettings`,
  `scripts/vscode/Invoke-MSTestWithCoverage*.ps1`, or `UtilitiesCS/Properties/AssemblyInfo.cs`.
- Introducing `[STATestClass]`/`[STATestMethod]` or any `*.StaTests.cs` file into `QuickFiler.Test`.
- Introducing a clock abstraction; no file in scope reads a clock.
- Changing repository-wide coverage thresholds, or meeting the absolute repository-wide floors that predate
  this epic. The gate here is retain-or-improve against the measured baseline (`epic.md:490`).
- Migrating QuickFiler away from VSTO/WinForms. Where a seam choice is open, the host-neutral extraction
  chosen here (`ControlColumnTrimmer`) is reusable by a future WebView2/Office.js port (`epic.md:198-200`).
