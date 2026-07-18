# `efcviewer-breadcrumb-webview2` — User Story

- Issue: #349
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-16T23-45
- Work Mode: full-feature
- Epic: `folder-tree-breadcrumb-redesign` (child 9102, wave 1, `depends_on: [9101]`)

## Story Statement

- As a user filing Outlook mail through QuickFiler's EfcViewer suggestion form, I want each
  suggested folder to render as a single-line breadcrumb anchored at the predicted leaf, so that
  I can read the full filing location of every suggestion at a glance instead of decoding an
  indented multi-row tree.
- As a user whose intended target folder is a subfolder of a suggestion, I want to expand a
  breadcrumb segment and see every real immediate Outlook subfolder of that folder, so that I can
  file into the correct destination even when it never appeared among the top-ranked suggestions.
- As a user choosing between suggestions, I want the prediction percentage on every row to be
  fully visible at all window sizes and DPI settings, so that I can weigh the model's confidence
  without scrolling horizontally or resizing the form.

## Problem / Why

The EfcViewer matching-folders control currently renders folder suggestions as a conventional
indented multi-row tree using `BrightIdeasSoftware.TreeListView` (`QuickFiler/Viewers/EfcViewer.cs`,
`QuickFiler/Viewers/EfcViewer3.cs`, plus their Designer files). The intended design is a single-line
breadcrumb per suggestion anchored at the selected leaf. The current hierarchy is synthesized by
`FolderSuggestionTree.BuildFromRows` via prefix-matching over the top-ranked suggestion rows, so
expanding a folder does not reveal its real Outlook subfolders. The prediction percentage is also
reported as obscured at runtime even though static column/rect math shows no overlap.

`TreeListView` (as currently used) does not naturally support single-line breadcrumb rendering with
per-segment double-click collapse, and it is a VSTO/WinForms-hosting-specific investment that would
not carry forward to the planned VSTO migration. The redesign targets WebView2 (HTML/CSS/JS), which
is largely reusable across a post-VSTO UI stack and reuses a dependency already proven in this
codebase (QuickFiler's WebView2 message-body pane, including the `cid:` fix from feature 326).

Scope note (research finding 1): only `EfcViewer` is live — the sole runtime instantiation is
`new EfcViewer()` at `QuickFiler/Helper Classes/EfcViewerQueue.cs:83`, and `EfcFormController` is
typed to the concrete `EfcViewer`. `EfcViewer3` is dead code and receives at most a mechanical
Designer-only control swap or removal, with no behavioral wiring.

## Personas & Scenarios

- **Persona: Dan, a high-volume Outlook user triaging and filing mail with QuickFiler.**
  - Who: a knowledge worker who processes a large daily inbox and files most messages into a deep
    Outlook folder hierarchy using the EfcViewer suggestion form rather than drag-and-drop.
  - What they care about: filing each message into the correct deep folder in a few keystrokes;
    trusting the suggestion list enough not to open the full folder tree.
  - Constraints: works keyboard-first (arrow keys, `'F'` jump, Enter to file); runs Outlook on
    displays with differing DPI, where the current control hides the percentage column; the
    correct target is often a subfolder that never appears among the top-ranked suggestions.
  - Goals and frustrations: wants to see the whole path and the confidence percentage for every
    suggestion at once; is frustrated that expanding a suggestion today only reveals other
    already-listed suggestions instead of the folder's real subfolders, and that the percentage
    is cut off at runtime.
  - Context and motivations: the suggestion list is the primary filing surface; every extra
    click, scroll, or misfiled message costs triage time and later search time.

- **Scenario: filing a message into a real subfolder of a suggestion (primary flow).**
  - Who is acting: Dan, in the EfcViewer form opened for the next message in the filing queue.
  - Trigger: a project status email arrives; the model's top suggestion is
    `Clients -> Contoso -> Projects`, but Dan files status emails one level deeper, in a
    subfolder that is not among the ranked suggestions.
  - Steps:
    1. The suggestion list renders. Each row is a single-line breadcrumb, for example
       `Clients -> Contoso -> Projects`, anchored at the predicted leaf, with the prediction
       percentage (for example `62%`) fully visible at the right edge of the row.
    2. Dan reads the top rows and compares their percentages without scrolling or resizing.
    3. The `Projects` leaf on the top row shows a plus affordance, because that folder has real
       subfolders; a different row whose leaf has no subfolders shows no affordance.
    4. Dan activates the plus on the leaf (double-click, or right-arrow with the row selected).
       The affordance switches to minus and the control lists every real immediate Outlook
       subfolder of `Projects` — including `Status Reports`, which never appeared among the
       ranked suggestions — fetched live through the shared 9101 hierarchy provider.
    5. Dan selects `Status Reports` and confirms; the message is filed there. The percentage on
       the original suggestion row was the model's score, unchanged by any of this navigation.
  - Obstacles / decisions: if the live subfolder query is slow or fails, the row must not
    corrupt its state — the leaf stays collapsed and the error is logged; banner rows
    (`"===="`) are visible but not selectable or expandable.
  - Expected outcome: the message lands in the correct deep subfolder in a few interactions,
    without opening the full Outlook folder tree.

- **Scenario: collapsing a long breadcrumb to its ancestor, then restoring it.**
  - Who is acting: Dan, reviewing a suggestion whose breadcrumb is long, for example
    `Archive -> 2025 -> Clients -> Contoso -> Projects -> Closed`.
  - Trigger: Dan only cares about which client subtree the suggestion points into and wants to
    de-clutter the row.
  - Steps:
    1. Dan double-clicks the non-leaf segment `Contoso`.
    2. The row collapses after `Contoso`: the downstream arrows, the `Projects` and `Closed`
       segments, and the original leaf are hidden. A plus appears to the left of the
       now-terminal `Contoso` segment. The percentage remains fully visible on the row.
    3. Dan clicks that plus; the full breadcrumb
       `Archive -> 2025 -> Clients -> Contoso -> Projects -> Closed` is restored, anchored again
       at the original leaf.
  - Expected outcome: per-segment collapse is a reversible view operation; it never changes the
    suggestion, its score, or the selected folder used for filing.

- **Scenario: keyboard-first navigation.**
  - Who is acting: Dan, hands on the keyboard after typing a search term.
  - Steps:
    1. From the search box, Down-arrow moves focus into the breadcrumb list and selects the
       first suggestion row.
    2. Right-arrow on a selected row expands (leaf subfolder listing); Left-arrow collapses.
       These key events originate in the hosted document and reach the controller over the
       JS<->.NET bridge.
    3. Up-arrow on the top row returns focus to the `SearchText` box (a `focusSearch` bridge
       message), matching current behavior.
    4. The `'F'` keyboard action jumps focus to the breadcrumb control from elsewhere on the
       form.
  - Expected outcome: the keyboard flow of the current control is preserved end-to-end over the
    new bridge.

## Acceptance Criteria

These criteria are identical to, and jointly authoritative with, the Definition of Done
acceptance criteria in `spec.md`.

- [ ] Every suggestion row in the live `EfcViewer` renders as a single-line breadcrumb
  `Folder -> SubFolder -> Leaf`, anchored at the selected/predicted leaf, in a WebView2-hosted
  HTML/CSS/JS control replacing the `TreeListView`.
- [x] The expand affordance (plus when collapsed, minus when expanded) appears only on the leaf
  segment, and only when the leaf's `HasSubfolders` is true; leaves without subfolders show no
  affordance.
- [x] Double-clicking a non-leaf segment collapses the row after that segment — downstream
  arrows, segments, and the leaf are hidden — and shows a plus to the left of the now-terminal
  segment; activating that plus re-expands the full breadcrumb.
- [ ] Expanding a segment lists every real immediate Outlook subfolder of that folder via the
  9101 `IFolderHierarchyProvider` seam (ancestor-chain plus on-demand immediate-subfolders
  calls); no hierarchy is derived by prefix-matching over suggestion rows.
- [ ] The prediction percentage is always fully visible: a runtime reproduction of the current
  obscuring defect (screenshot plus geometry diagnostic log) is captured first and stored under
  `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/repro/`, and the
  CSS-based fix (percent as the trailing fixed, non-shrinking flex item) is applied afterward.
- [ ] A JS<->.NET bridge (`window.chrome.webview.postMessage` -> `WebMessageReceived`;
  `PostWebMessageAsJson`/`NavigateToString` outbound) carries double-click and left/right-arrow
  keyboard interaction and routes the live subfolder query across the WebView2 boundary.
- [x] `EfcViewer3` is handled as a mechanical Designer-only control swap or removal with no
  behavioral wiring; the behavioral conversion targets `EfcViewer` + `EfcFormController` only
  (EfcViewer3 is dead code: sole runtime instantiation is `new EfcViewer()` at
  `EfcViewerQueue.cs:83`).
- [x] No third-party WinForms tree/list control and no WPF/`ElementHost` are introduced; the
  control technology is WebView2 (HTML/CSS/JS).
- [x] The scoring/ranking algorithm is unchanged; the feature-324 percentage plumbing
  (`FolderRow.Score` -> `PercentageFormatter.FormatPercent`) is reused as-is.
- [ ] Behavior parity is preserved: Up-at-top focuses `SearchText` (via a `focusSearch` bridge
  message); the `"Trash to Delete"` pseudo-row remains selectable; `"===="` banner rows remain
  non-interactive and rejected by `IsValidSelection`; the `'F'` action focuses the breadcrumb
  control; dark-mode re-theming works.
- [x] The pure breadcrumb row model and collapse/expand state machine, the bridge message
  contracts (JSON round-trip plus malformed-input negatives), the HTML renderer, and the bridge
  router are unit-tested with MSTest + Moq + FluentAssertions (router against
  `Mock<IFolderHierarchyProvider>` and `Mock<IBreadcrumbWebHost>`), meeting repository coverage
  floors with >= 90% on new modules; host/Outlook wiring stays behind coverage-exempt seams with
  in-code justification, and no new testable logic is added to `EfcFormController`.
- [x] The full C# toolchain passes in a single pass: csharpier, msbuild with analyzers, msbuild
  with nullable/TreatWarningsAsErrors, and vstest with coverage; no banned APIs
  (`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`) in
  touched/new code or tests.

## Non-Goals

- No change to the scoring or ranking algorithm or to model output; the percentage shown is the
  score already computed for internal ranking (feature-324 plumbing reused as-is).
- No behavioral conversion of `EfcViewer3` and no unification of the two EfcViewer
  implementations into a shared base control; `EfcViewer3` is dead code and receives at most a
  mechanical Designer-only swap or removal.
- No changes to the QuickFiler folder dropdown (`CboFolders`) — that is sibling feature 9103.
- No implementation of the live folder-hierarchy provider itself — that is dependency 9101; this
  feature only consumes its contract.
- No third-party WinForms tree/list control and no WPF/`ElementHost`.
- No packaged or on-disk web assets, no `SetVirtualHostNameToFolderMapping`, and no
  `AddHostObjectToScript` host objects; the document is C#-generated and delivered via
  `NavigateToString`.
- No changes to the EfcViewer mail-body pane (`EfcItemController` WebView2 init is untouched).
