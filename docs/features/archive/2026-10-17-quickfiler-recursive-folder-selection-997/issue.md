# QuickFiler descendant selection and recursive expansion (Issue #997)

- Date captured: 2026-10-17
- Author: Dan Moisan
- Status: Implemented and locally verified; independent review and final PR-head CI pending. Active folder: docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/

- Issue: #997
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/997
- Last Updated: 2026-10-18
- Work Mode: full-bug

## Problem / Why

QuickFiler expands top-level folders, but revealed subfolders cannot be selected as filing destinations. Descendants with children do not show their own expansion controls, preventing navigation to deeper destinations.

## Delivered Behavior

Every displayed folder is selectable using the same interaction as a top-level destination, including intermediate folders with children. Plus/minus controls only expand or collapse their folder. Expansion reveals immediate children recursively at every depth; each folder with children has a control and leaves have none.

## Original Intake Criteria (spec.md is authoritative for full-bug)

- [x] Every displayed descendant folder, including intermediate folders, is selectable as a filing destination using the top-level selection behavior.
- [x] Clicking plus/minus expands or collapses only and does not change the selected destination.
- [x] Every folder with children has an expansion control at every depth; leaves have none.

## Constraints & Risks

Preserve archive-relative destination validation, existing confirmation behavior, duplicate folder identity handling, and asynchronous request ordering. Use existing dependencies and deterministic in-memory tests.

## Test Conditions to Consider

- [x] Regression reproductions for descendant selection and missing nested controls.
- [x] Three or more folder levels, intermediate and leaf selection, and expansion/collapse without selection.
- [x] Existing bridge, state-model, and embedded browser UI contracts remain valid.

## Next Step

- [x] Promote to a bug issue and create a full-bug active folder.

## Delivery and Test Results

Implemented QFC recursive host-owned descendants, validated archive-relative filing, durable collapse/Escape selection, independent recursive controls and stale-request rejection. Typed and legacy indexed activation both validate known targets before mutation. No packages or new settings were added; EFC production remains unchanged.

The four original regressions and a fifth legacy root-boundary regression have fail-before/pass-after evidence. Final ordered CSharpier, analyzer Rebuild, warnings-as-errors Rebuild and full MSTest coverage pass succeeded: 7,504 passed, zero failures; zero build warnings/errors. Production line coverage rose from 86.03% to 86.10%; new measured files 98.70%, changed lines 99.09%, and all 45 measured new methods at least 90%. Hygiene, registration, script syntax/composition and <=500-line gates passed. spec.md has seven of eight authoritative ACs checked; AC8 is pending final-head CI.

## Rollout and Next Gates

Select a folder name to file to that folder; plus/minus only reveals/hides children. Use the existing TaskMaster release process after orchestrator-owned independent review, canonical PR context, final PR-head required CI and completion validation. No migration is required. Local proof uses deterministic in-memory host/viewer tests and compiled asset/syntax contracts; no live Outlook UI session is claimed.
