# 2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state (User Story)

- **Issue:** #792
- **Kind:** bug
- **Work Mode:** full-bug
- **Last Updated:** 2026-09-12

> **Acceptance-criteria source.** This is a full-bug item. The sole authoritative acceptance-criteria source is spec.md. This document contains no checkboxes and no acceptance criteria of its own; nothing here is tracked or checked off. It describes the user-facing outcome and the manual verification runbook for AC-U5.

## Why this document exists on a bug

The feature-promotion lifecycle says a full-bug folder normally carries spec.md only. This document is required here for two stated reasons.

First, the defect is reported as two distinct user-visible symptoms on two different entry points, so the user-facing outcome needs its own record rather than being inferred from a technical root-cause narrative.

Second, AC-U5 is a human-executed live-Outlook verification. Its actor, its preconditions and its observable pass or fail outcome belong in a user story, not in a technical spec.

## Actor

An Outlook user filing mail with the TaskMaster add-in, working in a live Outlook session with the add-in loaded. The same person performs the manual verification; no separate QA role is assumed.

## Scenario 1: pop-out from a QuickFiler row

**Given** the user has opened QuickFiler from the ribbon and is looking at a list of mail rows,

**When** the user pops a row out into the single-item Efc view,

**Then** the folder area of the Efc view shows suggested folder rows for that mail item, and typing a search string in the folder search box narrows the rows as the user types.

Before the fix, the folder area is blank: no suggestions, no banners, and typing has no effect for the remainder of the session.

## Scenario 2: ribbon Sort Email

**Given** the user has an Outlook session with the add-in loaded, and has already used QuickFiler at least once in that session,

**When** the user selects a mail item and invokes Sort Email from the ribbon,

**Then** the Efc viewer opens with suggested folder rows listed under the "Matched Folders:" label, and typing a search string narrows the rows.

Before the fix, the "Matched Folders:" label is present but the list beneath it is empty. The label is a static WinForms control, which is why it survives while the list does not.

## Observable outcome the user must see

On both entry points, after the fix:

- At least one folder suggestion row is visible in the folder area within the normal time it takes the Efc view to finish opening.
- Typing into the folder search box changes the set of visible rows.
- No error notice appears during a successful open.

If initialization nevertheless fails after the retries are exhausted, the user must see a visible error banner in the folder area and a fault notice, rather than a silently blank list. A blank list with no explanation is the defect and is not an acceptable outcome in any branch.

## AC-U5 manual verification runbook

This verification is performed by a person against a live Outlook session. It is not automated and is not an automated gate.

### Preconditions

- A live Outlook session with the rebuilt add-in loaded from the debug output.
- Outlook is closed, not killed, before the rebuild. Killing the process leaves the build output locked and the rebuild fails or produces a stale assembly. Close Outlook through its own exit path, run the rebuild, then reopen Outlook.
- At least one mail folder tree with enough history for the folder predictor to produce suggestions. A newly configured profile with no filing history may legitimately produce no suggestions and is not a valid test subject.
- The add-in debug log for the current date is accessible for the log check in step 6. It is written to the logs folder under the add-in's debug output directory.

### Steps

1. Close Outlook through its normal exit. Do not end the process.
2. Run the full toolchain and rebuild the solution.
3. Reopen Outlook and confirm the add-in loaded.
4. Open QuickFiler from the ribbon. Confirm at least one item body renders, which establishes that an item-body WebView is running in the process. This ordering matters: it is the condition under which the defect reproduces deterministically before the fix.
5. Pop a row out into the Efc view. Observe the folder area.
6. In the popped-out Efc view, type a partial folder name into the folder search box. Observe the rows.
7. Close the Efc view. Select a mail item in the Outlook list and invoke Sort Email from the ribbon. Observe the folder area under the "Matched Folders:" label.
8. In that Efc viewer, type a partial folder name into the folder search box. Observe the rows.
9. Open the add-in debug log for the current date and search it for the text "Breadcrumb CoreWebView2 initialization failed".

### Observation that decides pass or fail

The verification passes only if all four of the following hold:

1. In step 5 the folder area shows one or more suggestion rows, not a blank list.
2. In step 6 the visible rows change in response to the typed text.
3. In step 7 the area under the "Matched Folders:" label shows one or more suggestion rows, not a blank list.
4. In step 9 the log contains no occurrence of "Breadcrumb CoreWebView2 initialization failed" with HRESULT 0x8007139F for the current session.

The verification fails if any one of those four does not hold. A blank folder area on either entry point, or any occurrence of that HRESULT in the session's log, is a fail.

Record the result, the Outlook session start time, and the log lines inspected in the Markdown evidence artifact under the feature folder's evidence directory. Per the evidence convention for this change, raw tool output is not committed; the observation is recorded as text.
