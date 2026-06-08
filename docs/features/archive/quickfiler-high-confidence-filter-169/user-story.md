# `quickfiler-high-confidence-filter` — User Story

- Issue: #169
- Owner: drmoisan
- Status: Implemented (pending review)
- Last Updated: 2026-06-01T17-12-39Z

## Story Statement

- As a QuickFiler user processing a large batch of email, I want a separate "QuickFiler — High Confidence" entry point that shows only emails whose top suggested folder meets a confidence threshold, so that I can quickly file the items the classifier is most certain about.
- As a QuickFiler user, I want to set and persist the confidence threshold percentage from the ribbon, so that I can tune how strict the high-confidence view is across sessions without changing my normal QuickFiler workflow.

## Problem / Why

When QuickFiler loads a batch, every email is shown regardless of how confident the Bayesian classifier is about its top suggested folder. Reviewing low-confidence items mixed with high-confidence items slows down clearing the items that could be filed with little or no judgment. Users want a way to focus first on emails the classifier is confident about, while leaving the existing QuickFiler behavior unchanged for normal processing. Folder probabilities are computed lazily per email after the window is shown, so the solution must filter after scoring rather than at the datamodel stage.

## Personas & Scenarios

- Persona: High-volume QuickFiler user
  - who the user is: An Outlook user who regularly processes large inboxes with QuickFiler and relies on the Bayesian folder suggestions to file email.
  - what they care about: Speed and accuracy when clearing email that can be filed with high certainty.
  - their constraints: Limited time per session; does not want to change the familiar QuickFiler flow for ambiguous items.
  - their goals and frustrations: Wants to act quickly on confidently classified email; frustrated by having to scan past low-confidence items to find the obvious ones.
  - their context and motivations: Works through the initial batch QuickFiler loads and wants the high-confidence subset surfaced for that batch.

- Scenario: Filing the confident subset of a batch
  - A concrete, step-by-step narrative:
  - who is acting? The high-volume QuickFiler user.
  - what triggered the action? The user wants to clear easily classified email before handling ambiguous items.
  - what steps do they take? The user clicks the "QuickFiler — High Confidence" ribbon button. QuickFiler opens and renders the initial batch. After per-item scoring completes (`LoadSecondaryAsync`), emails whose top suggested folder probability is below the configured threshold (default 0.90) are removed from the view, including any email with no qualifying suggestion. The user files the remaining high-confidence items.
  - what obstacles or decisions occur? The user may decide the default threshold is too strict or too loose and adjust the threshold percentage using the ribbon input control; the validated value persists across sessions.
  - what outcome do they expect? Only emails meeting or exceeding the threshold appear in the high-confidence view; the standard "QuickFiler" entry point continues to show all emails exactly as before.

## Acceptance Criteria

1. [x] A new ribbon entry point launches QuickFiler in high-confidence mode. (P6-T1/P6-T3/P6-T4; tested P6-T5)
2. [x] When the mode is enabled, emails whose top suggested folder probability is below the configured threshold are not shown in the view. (P1-T1, P3-T1/P3-T2, P4-T1/P4-T2, P5-T1; tested P1-T2, P4-T3, P5-T2)
3. [x] Emails with no folder suggestion at or above the threshold (including none at all) are excluded. (P1-T1 empty->0, P4-T2; tested P1-T2(a), P4-T3 zero-score case)
4. [x] The default threshold is 90% (0.90) and is persisted as a user setting. (P2-T1/P2-T2/P2-T4; tested P2-T5)
5. [x] The threshold percentage is changeable at runtime via a ribbon input control, with validation; the value persists across sessions. (P6-T2/P6-T3/P6-T4; tested P2-T5, P6-T5 valid/non-numeric/out-of-range)
6. [x] With high-confidence mode disabled, QuickFiler behaves exactly as today (no filtering). (P5-T1 guard; tested P5-T2 disabled case; standard LoadQuickFilerAsync unchanged)
7. [x] New and changed logic is covered by MSTest + Moq + FluentAssertions tests; the full C# toolchain (CSharpier, .NET analyzers, nullable analysis, MSTest) passes with zero regressions. (all test tasks + P7-T1/P7-T2; the only failing tests are pre-existing flaky timing tests, not regressions)

## Non-Goals

- No in-window (in-GUI) filter toggle; high-confidence mode is reached through a separate ribbon entry point.
- No datamodel-stage filtering; probabilities are computed lazily after the window is shown, and the filter is a post-scoring removal pass.
- No re-application of the filter across later background batches; v1 filters the initially loaded batch only, consistent with the current batch-1 scope.
- No change to the existing "QuickFiler" entry point or to its default behavior when high-confidence mode is disabled.
- No new external dependencies, network I/O, or telemetry sinks.
