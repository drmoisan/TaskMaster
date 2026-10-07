---
name: project-959-r8-fluentassertions-string-diff-truncation-message-gate-seam
description: FluentAssertions Be() on long strings prints only a window around the first differing index, so an expect-fail MESSAGE gate must assert the assertion-subject text plus the string tails, never a full path literal; stop-record rewrite and sweep rules for fixed-name fail-before artifacts
metadata:
  type: project
---

Issue #959, revision 1.8 (execution-time stop at P4-T9, 2026-10-03): the expect-fail gate asserted the full origin-folder literal `Sortemail959Sandbox\origin` in the failed row's `MESSAGE`, and the run stopped `FAIL-BEFORE WRONG REASON` although the test failed for exactly the planned reason.

**Why:** FluentAssertions `string.Should().Be()` renders a long string difference as `Expected <subject> to be a match with the expectation, but it differs at index N:` followed by the actual and expected strings each truncated to a window after a U+2026 ellipsis (`"…59Sandbox\origin"` / `"…59Sandbox\destination"` for a 23-character common prefix). The full literal is never printed on the success case, so the clause was unsatisfiable. Only long string comparisons are affected: collection `HaveCount` (`but found 3`) and integer `Be` (`but found 0`) messages are printed in full.

**How to apply:**
- For a string-difference expect-fail gate, require (a) the assertion's subject text as printed, for example `GetDirectoryName(helper.FilePathSaveAlt)`, which discriminates the failing assertion from its siblings (`FilePathSave)` without `Alt`, `GetFileName(`), and (b) the string tails that follow the ellipsis, as backslash-free tokens such as `origin"` and `destination"`. Never a path literal with a backslash (the atomic-plan-contract forbids it anyway).
- Derive the differing index from the test's constants (shared prefix length) and confirm the first failing assertion from the assertion order; without an `AssertionScope` only that one message prints.
- When sweeping the remaining message gates, classify each by assertion kind (string Be vs count vs numeric vs exception type); only the first kind is truncated.
- A stop record written to a FIXED-name fail-before artifact must be rewritten in full by the re-run (Write after Read, `ITERATION: 2` after the schema rows); say so in the task text and name the later readers (pass-after append, negative-controls, AC check-offs) so the superseded `STOP` heading and `NOT MET` item are not read later. The artifact-filenames "stop record stays on disk" rule applies only to timestamped, glob-located artifacts.
- The Test Inventory substring table is a consumer (negative-controls quotes "the substring observed" from it): amend the row together with the task.
- The `MESSAGE` payload line carries a multi-line InnerText, so word the gate as the `MESSAGE` entry, not the `MESSAGE` line.
- The Test Strategy / AC text was written as "observed failing on the alternate-path assertion", so the subject-text token aligns with the AC more closely than the path literal did; no spec change was needed.

Related: [[project-959-r7-ac-by-reference-spec-correction-and-delta-backtick-seams]], [[expect-fail-needs-a-synchronous-seam]].
