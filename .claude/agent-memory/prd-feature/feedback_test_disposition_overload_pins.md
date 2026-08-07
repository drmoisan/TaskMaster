---
name: test-disposition-overload-pins
description: When a spec introduces a new method overload, grep all test files for Setup/Verify of the old overload before classifying any test file as "unchanged" — loose mocks fail at run time, not compile time
metadata:
  type: feedback
---

Before classifying any existing test file as "Unchanged" in a spec's Test Strategy table, grep the whole test project for `Setup`/`Verify` calls against every method signature the spec changes (especially `It.IsAny<...>` matchers on an overload the spec retires from a call site). Classify by what the file asserts on, not by its name or the issue it was written for.

**Why:** In the #424 spec (2026-08-06) I classified `QfcHomeControllerIssue218Tests.cs` as dormant based on the research's grouping, but both its tests `Setup`/`Verify` the two-argument `DequeueNextItemGroupAsync` overload that the spec moved off the pre-UI call site. Loose Moq mocks meant no compile break — the contradiction surfaced as two `Moq.MockException` failures at the Phase 5 pinned-suite gate, forcing a mid-execution spec correction (AC reword + table reclassification + correction log entry).

**How to apply:** During spec authoring, for each interface signature change, run a Grep across the test project for the method name and record every hitting file in the disposition table with an explicit disposition ("Update — overload shape only" when only matchers change). Also: when a coordinator later corrects a spec, add a dated Correction Log entry so reviewers see the change was deliberate, keep the AC count fixed, and never check items off (executor owns check-off). This pattern was validated by the coordinator's correction instructions. Related: [[promotion-scaffold-metadata-defects]].
