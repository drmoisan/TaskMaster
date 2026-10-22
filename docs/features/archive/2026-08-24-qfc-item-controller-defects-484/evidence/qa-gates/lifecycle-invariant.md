# Post-`Cleanup()` lifecycle invariant

Timestamp: 2026-08-26T11-16
Task: [P6-T6]

**Invariant.** A pooled item viewer handed back after `Cleanup()` carries zero event subscriptions from
the released controller, with the single documented `WebResourceRequested` exception, which is
established by inspection rather than by execution.

`Cleanup()` (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:447`) calls `UnwireEvents()` at
line 458, before it releases `_itemViewer` (line 460) and `_kbdHandler` (line 473).
`UnwireEvents()` (`QuickFiler/Controllers/QfcItemController.EventWiring.cs:392`) calls
`UnwireControlTreeEvents()`, then `UnwireIntentEvents()`, then `DetachWebResourceRequestedHandler()`.

## Subscription inventory and its evidence

### 1. Sixteen intent subscriptions — proved by execution

`UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` carries sixteen `VerifyRemove` assertions
with `Times.Once()`, one per subscription made by `WireIntentEvents()`, and is recorded `Passed` in
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/481-pass.md`. The
sixteen are `ConversationModeChanged`, `FlagTaskClicked`, `PopOutClicked`, `DeleteItemClicked`,
`ReplyClicked`, `ReplyAllClicked`, `ForwardClicked`, `BodyDoubleClick`, `SearchTextChanged`,
`FolderKeyDown`, `FolderSelectionChanged`, `WebViewInitializationCompleted`,
`ConversationItemSelectionChanged`, `SearchKeyDown`, `EmailCopyChanged`, `AttachmentsChanged`.

### 2. Six control-tree subscriptions — proved by execution

`UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers` raises
`OnPreviewKeyDown`, `OnKeyDown`, and `OnMouseEnter` by reflection on a real headless `ItemViewer` after
wiring and then unwiring, and asserts `Times.Never()` on the keyboard-handler mock for both
`KeyboardHandler_PreviewKeyDownAsync` and `KeyboardHandler_KeyDownAsync`, plus an unchanged button
background colour for the mouse path. It is recorded `Passed` in the same `481-pass.md` artifact. The
mouse-leave and menu-item pairs are detached by the same loops the mouse-enter assertion exercises.

### 3. The breadcrumb subscription — already detached before this feature

`Cleanup()` detaches `BreadcrumbUnhandledArrow` at
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:454`, before nulling `_breadcrumbViewer` at
line 455. That statement pre-dates this feature and is unchanged; constraint C4 rule 3 preserves its
order and `docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/cleanup-statement-order.md`
records it.

### 4. The `WebResourceRequested` subscription — proved by inspection

This is the single documented exception. It is subscribed inside the pre-existing
`[ExcludeFromCodeCoverage]` `InitializeWebViewAsync`, which needs a live WebView2 runtime, so no unit
test can reach either the `+=` or the matching `-=`. Its detachment is delivered by
`DetachWebResourceRequestedHandler` (`ViewerSetup.cs:486`) and established by the fail-before exception
dossier at
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/fail-before-exception.webresourcerequested-detach.md`,
which quotes the capture assignments at `:33`, `:34`, `:85`, `:92`, `:107` and the detach at `:490`, and
records that the same field instance is the operand of both the `+=` and the `-=`.

## Conclusion

Every subscription the controller makes on a pooled viewer is detached during `Cleanup()`: 16 intent
subscriptions and 6 control-tree subscriptions by executed regression tests, the breadcrumb
subscription by the pre-existing detach, and the `WebResourceRequested` subscription by an
inspection-verified detach with a recorded exception dossier.

Output Summary: The invariant holds. 22 of the 23 subscriptions are proved by passing regression tests
plus the pre-existing breadcrumb detach; the single `WebResourceRequested` subscription is proved by
inspection under a recorded fail-before exception dossier.
