---
name: schema-field-contains-check-matches-prefixed-labels
description: An evidence-field gate that tests `$c.Contains("EXIT_CODE:")` passes on any artifact carrying FORMAT_EXIT_CODE:/VSTEST_EXIT_CODE: rows, so it cannot detect a missing EXIT_CODE field; anchor it per line
metadata:
  type: project
---

A plan's evidence-field gate (for example `CMD-EVIDENCE-FIELDS` in the #959 plan) that checks each artifact with `$c.Contains("EXIT_CODE:")` is satisfied by the suffix of `FORMAT_EXIT_CODE:`, `CHECK_EXIT_CODE:`, `MSBUILD_EXIT_CODE:` or `VSTEST_EXIT_CODE:`. Artifacts that record those raw payload lines therefore pass even when the schema `EXIT_CODE:` row is absent, and the gate cannot fail for them.

**Why:** Found in #959 preflight round 3 (2026-10-02). The plan's AC26 ("every artifact carries Timestamp:, Command:, EXIT_CODE:") relied on that gate, and rounds 1 and 2 accepted it, because the field list in the payload looked exact.

**Round-4 follow-on (#959, 2026-10-03).** After the planner adds per-task `EXIT_CODE:` scopes, check three more things. First, a scope that names a native call which is not the payload's final statement (e.g. `dotnet tool restore` inside one `pwsh -Command` payload) has no value source unless the payload prints `$LASTEXITCODE` under a label right after that call; the process exit code reflects only the last statement. Second, a task whose `EXIT_CODE:` is scoped to a run with an allowed non-zero branch (for example a toolchain summary scoped to the coverage collect) needs its own `ExpectedExitCode:`, because the expectation is per file. Third, the convention must put the schema rows before any copied `*_EXIT_CODE:` line, because the parsers read the first occurrence of each label. Also check that a check-off-only loop rule ("edit only spec.md") does not contradict a repair branch added to a check-off task.

**How to apply:** In preflight, read the matching predicate of any schema-field gate, not only its field list. Require a line-anchored match such as `[regex]::IsMatch($c, "(?m)^[^\w\r\n]*" + [regex]::Escape($field))`, plus an in-memory control showing that `FORMAT_EXIT_CODE: 0` does not match. Related: [[gates-can-pass-for-reasons-unrelated-to-correctness]], [[preflight-checkoff-cites-later-task-artifact]].
