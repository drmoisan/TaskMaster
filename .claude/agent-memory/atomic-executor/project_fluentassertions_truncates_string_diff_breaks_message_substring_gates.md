---
name: fluentassertions-truncates-string-diff-breaks-message-substring-gates
description: FluentAssertions string Be() failures print only a window around the first differing index ("…59Sandbox\origin"), so an expect-fail MESSAGE substring gate on a full path literal can never match
metadata:
  type: project
---

FluentAssertions `string.Should().Be(expected)` failures on long strings print "differs at index N" plus a truncated window prefixed by U+2026 (e.g. `"…59Sandbox\origin"`), not the full actual value. An `[expect-fail]` gate requiring the MESSAGE to contain a full literal such as `Sortemail959Sandbox\origin` is unsatisfiable even when the failure is exactly the expected defect (#959 P4-T9, 2026-10-03, stopped as FAIL-BEFORE WRONG REASON).

**Why:** the planner derived the substring from the test's input constants, not from an observed failure message; the TRX ErrorInfo/Message holds the same truncated text as the console.

**How to apply:** in preflight, for any expect-fail MESSAGE substring over a FluentAssertions string comparison, check that the literal fits inside the window around the first differing index (or is the short distinguishing tail). Flag full-path literals as a defect. At execution, confirm absence with a read-only Grep of the TRX before declaring the stop. Related: [[project_trx_message_multiline_and_markdown_indent_width]].
