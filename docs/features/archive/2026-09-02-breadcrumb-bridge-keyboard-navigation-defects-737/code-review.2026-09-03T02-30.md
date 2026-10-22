# Code Review — breadcrumb-bridge-keyboard-navigation-defects (Issue #737)

- Timestamp: 2026-09-03T02-30
- Scope: full branch diff `origin/main...HEAD` (three-dot, base = `b13d5b7b`, the confirmed merge-base) — 3 code files (1 production, 2 test), plus feature-folder docs/evidence reviewed for consistency only.
- Files reviewed in depth: `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`, `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`, `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`.

## Summary

This is a small, well-scoped bug-fix branch delivering two additive JavaScript-content changes (Enter-key binding, post-render scroll-into-view) to an inline JS string constant, plus one test-quality fix (capturing two previously-discarded Arrange-phase results in an existing router test). No production C# logic changed; no new C#-side types, message shapes, or router branches. All three changed files match the spec's designed diffs line-for-line. No blocking findings.

## Finding 1 (#640) — Scroll-into-view — `BreadcrumbDocumentAssets.cs`

```
+ "      var scrollTarget = document.querySelector('.rowwrap.selected');\n"
+ "      if (scrollTarget) { scrollTarget.scrollIntoView({ block: 'nearest' }); }\n"
```

Placement: these two lines sit after the `if (msg.type === 'render') {...} else if (msg.type === 'subfolderResult') {...}` block and before the closing `});` of the `addEventListener('message', ...)` callback (lines 141-142 of the current file). This means the scroll-into-view call runs unconditionally at the end of every inbound `message` event, not gated to only the `render`/`subfolderResult` branches specifically. Functionally this is equivalent today, because those are the only two `msg.type` values the listener currently recognizes (confirmed by reading the full `BridgeJs` constant — no other `if`/`else if` arm exists in this listener). This is a minor design note, not a defect: if a third inbound message type is ever added to this listener without updating this comment, the scroll call would silently also fire for it. Non-blocking; worth a one-line comment if a maintainer wants to make the intent explicit in a follow-up, but not required by the spec or the ACs.

## Finding 2 (#641) — Enter-key binding — `BreadcrumbDocumentAssets.cs`

```
+ "    if (e.key === 'Enter') {\n"
+ "      var selected = document.querySelector('.rowwrap.selected');\n"
+ "      var id = selected ? selected.getAttribute('data-row-id') : '';\n"
+ "      post({ type: 'rowSelected', rowId: id });\n"
+ "      e.preventDefault();\n"
+ "      return;\n"
+ "    }\n"
```

- Matches spec.md's Design section verbatim (reuses `rowSelected`, no new message type/codec branch/router case — independently confirmed by reading `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` and finding the pre-existing `case BreadcrumbMessageTypes.RowSelected: SelectRow(row); break;` at line 285-287, unmodified).
- **Duplication observation (minor, non-blocking)**: the 3-line `.rowwrap.selected` -> `getAttribute('data-row-id')` -> ternary-to-`''` pattern in this new Enter branch is byte-identical to the pre-existing arrow-key branch's lookup at lines 112-114 (`var selected = document.querySelector('.rowwrap.selected'); var id = selected ? selected.getAttribute('data-row-id') : ''; post({ type: 'arrowKey', ...`). This is copy-paste duplication that a small local JS helper function could remove. It is explicitly spec-mandated, though: spec.md's Design section for Finding 2 instructs "using the identical `document.querySelector('.rowwrap.selected')` lookup the arrow-key handler already performs," i.e., the duplication is an intentional mirror of existing style rather than a new pattern introduced by this change. Given the file has no JS test-execution harness and the string-literal-concatenation authoring style makes refactoring into a shared JS function mechanically awkward within a C# string constant, this is reasonable to leave as-is; flagged for awareness only.
- **Edge-case safety (verified, not merely assumed)**: when Enter is pressed with no `.rowwrap.selected` element present, `id` resolves to `''`. On the C# side, `ProcessInboundAsync` resolves the row via `FindRow(message.RowId)` before dispatching to any `case` (line 236); an empty/unmatched `RowId` returns `null`, which is logged and the message is dropped without dispatching to `SelectRow` (lines 237-241, read directly, unmodified by this feature). This means the new Enter branch cannot reach `SelectRow` with an invalid target — the pre-existing fail-fast guard already covers this new caller. No new guard was needed and none was skipped.

## Finding 3 (#693) — Router test assertion fix — `FolderBreadcrumbBridgeRouterTests.cs`

```
- await ArrowAsync(router, "left");
- await ArrowAsync(router, "left");
+ var firstPress = await ArrowAsync(router, "left");
+ var secondPress = await ArrowAsync(router, "left");
  ...
+ firstPress.Should().ContainSingle();
+ BreadcrumbBridgeSerializer.Parse(firstPress[0]).Should().BeOfType<RenderMessage>();
+ secondPress.Should().ContainSingle();
+ BreadcrumbBridgeSerializer.Parse(secondPress[0]).Should().BeOfType<RenderMessage>();
```

- Correctly captures both previously-discarded results and asserts each parses to a `RenderMessage`, matching the file's own sibling pattern at `Route_ThemeChange_EchoesThemeAndReRenders` (line 408: `BreadcrumbBridgeSerializer.Parse(outputs[1]).Should().BeOfType<RenderMessage>();`), so the assertion idiom is consistent with the rest of the file rather than introducing a new style.
- `ArrowAsync`, `PopulatedRouterAsync`, and all three provider-mock factories (`ProviderMock`, `StemProviderMock`, `ParentSubfolderProviderMock`) are unmodified — confirmed by diff (only the body of `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` changed).
- The fix is verification-only: it does not alter the router's production behavior, and the isolated pass of the modified test plus the pass of the sibling `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition` test (both recorded in QA-gate evidence) independently corroborate that the two newly-asserted `RenderMessage` outcomes are consistent with the existing #440 ancestor-walk contract, not a coincidental pass.

## New Test — `BreadcrumbHtmlRendererTests.cs`

```
[TestMethod]
public void Issue737BridgeJsPostsRowSelectedOnEnterAndScrollsSelectedRowIntoView()
{
    string bridgeJs = BreadcrumbDocumentAssets.BridgeJs;
    bridgeJs.Should().Contain("e.key === 'Enter'");
    bridgeJs.Should().Contain("post({ type: 'rowSelected', rowId: id });");
    bridgeJs.Should().Contain("scrollIntoView({ block: 'nearest' })");
}
```

- Follows the file's established `Issue439ActiveAncestorChildrenAndEmbeddedBridgeUseTypedStoppedActivation` precedent exactly (string-containment against the public `BridgeJs` constant), placed immediately after it in the file.
- XML-doc `<summary>` explicitly documents the JS-execution-harness limitation (string-shape verification, not runtime-behavior verification), satisfying the spec's Test Strategy requirement to record this limitation in the test's own comment.
- The `post({ type: 'rowSelected', rowId: id });` assertion string is a substring shared between this new test's target text and the file's Enter-branch source; it is distinct from the pre-existing click handler's `post({ type: 'rowSelected', rowId: rowId });` (different variable name, `id` vs `rowId`), so this assertion cannot pass against pre-existing code accidentally — it is a meaningful, discriminating check, not a tautology.

## Design Principles (General Code Change Policy §1)

- **Simplicity first**: PASS. Both JS additions are minimal, linear insertions matching the existing file's authoring style (string-literal concatenation).
- **Reusability**: PASS with the one minor duplication note above (spec-mandated, not a new pattern).
- **Extensibility**: N/A — no public API surface changed.
- **Separation of concerns**: PASS. The JS content stays entirely within the existing `BridgeJs` constant; no C#-side wiring, message type, or router logic was touched, correctly respecting the Efc/Qfc pipeline boundary this spec explicitly documents and enforces via its Write Set.

## Naming, Comments, Documentation

- New test method name (`Issue737BridgeJsPostsRowSelectedOnEnterAndScrollsSelectedRowIntoView`) is descriptive and follows the file's existing `IssueNNN...` naming convention.
- New local variables in the router test (`firstPress`, `secondPress`) are clearly named and communicate their Arrange-phase role better than the previous discarded, unnamed `await` calls.
- No new production comments were added explaining "why" for the JS changes; given the change is a direct, spec-traceable, one-purpose insertion with an XML-doc comment on the class already describing the bridge's message contract, this is acceptable and not a gap.

## Blocking Findings

None.

## Non-Blocking Observations (for future awareness, not remediation-required)

1. The `.rowwrap.selected` lookup pattern is duplicated across the Enter and arrow-key `keydown` branches (spec-mandated mirror of existing style; a future JS-content refactor could extract a shared `getSelectedRowId()` local function).
2. The new post-render scroll-into-view call is not textually gated to the `render`/`subfolderResult` branches specifically, though it is currently behaviorally equivalent since those are the listener's only two recognized message types.

Neither observation blocks merge or requires remediation-inputs.
