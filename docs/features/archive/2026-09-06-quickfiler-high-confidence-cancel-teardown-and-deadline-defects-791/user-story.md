# User Story — QuickFiler High Confidence deadline and Cancel teardown (Issue #791)

Why this file exists in full-bug mode: both defects are operator-facing (an empty High Confidence dialog with no explanation, and an Outlook keyboard left unusable after Cancel), so the operator's perspective is warranted; spec.md remains the sole authoritative acceptance-criteria source under work mode full-bug.

> This document is narrative context only. It contains no acceptance criteria and no checkboxes. Do not use it as an AC source or a check-off target; the criteria live in spec.md in this folder.

## Story 1 — High Confidence returns suggestions instead of an empty dialog

As an Outlook user filing mail with QuickFiler in High Confidence mode, I want the scan to keep looking when the first seconds of a view produce no confident match, so that I get the suggestions that exist further down the view instead of an empty dialog.

- **Given** an Explorer view whose leading items all score below the confidence cutoff while later items score above it,
- **When** I launch QuickFiler from the High Confidence ribbon button,
- **Then** scanning continues past the first-batch checkpoint with progress still reported, the dialog opens with the first confident suggestions once they are found, and it opens empty only when the candidate queue is genuinely exhausted or a documented scan bound is reached — with the cutoff, the scanned and accepted counts, and the stop decision written to the log so the outcome is explainable.

## Story 2 — Cancel leaves Outlook usable

As an Outlook user who presses Cancel in QuickFiler, I want the add-in to shut down completely and in order, so that my keyboard keeps working in Outlook and no leftover background work interferes with the next launch.

- **Given** a QuickFiler session in which I have filed suggestions and used Undo,
- **When** I press Cancel,
- **Then** the background queue loader is stopped and awaited before any state it uses is released, the keyboard handlers are unregistered before the item rows are removed, the keyboard-active flag is reset, WebView2 focus is parked and any open breadcrumb dropdown is closed, the ribbon is released even if a teardown step fails so both ribbon buttons still work, typing in Outlook works immediately afterwards, and every teardown stage — including any failure — appears in the log.

## Out of scope for these stories

The breadcrumb WebView2 initialization failure observed in the same session is tracked separately as issue #792.
