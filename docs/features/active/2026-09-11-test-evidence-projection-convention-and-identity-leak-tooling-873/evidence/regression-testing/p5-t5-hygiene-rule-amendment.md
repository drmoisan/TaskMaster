# P5-T5 — Hygiene Rule Text Amendment

Timestamp: 2026-09-13T06-18
Task: [P5-T5]

Command: pwsh -NoProfile -Command '<read .claude/agent-memory/_shared_no_absolute_host_paths.md and count the fenced-code-block delimiter and the two rule sentences>'
EXIT_CODE: 0

```
FENCED_CODE_BLOCK_DELIMITER_COUNT: 0
PLAN_SCAN_RULE_PRESENT: 1
PARSE_CHECK_RULE_PRESENT: 1
LINES: 122
```

## The two rules

A new section, `## Two obligations on the hygiene task itself`, carries both rules as their own numbered
statements:

1. A per-plan hygiene task must include the plan file itself in its residual scan, because a scan that
   excludes the plan by path cannot detect a host path reintroduced into the plan, and the plan is among
   the files most likely to carry one since it quotes commands and paths verbatim.
2. A residual-match count of zero is necessary but not sufficient and must be paired with a parse check
   on every XML-family file the sweep rewrites, because the count measures only that the identifiers
   were removed and never that the rewritten file still parses.

Neither rule was stated as an obligation in the file's prior text. The first appeared only as a
parenthetical incident note in a "Related structural gap" paragraph, not as a rule. The second appeared
only as a sub-rule scoped to angle-bracket redaction of XML, so a hygiene task that performed no
angle-bracket substitution would not have read it as applying. The new section says so explicitly: the
sections above record these as incidents, and they are restated as obligations on the task being
authored, because a task author reading an incident note does not necessarily recognise it as a
requirement on the task in front of them.

## No executable sweep added

The file contains zero fenced-code-block delimiters after the amendment, so no script and no command
block was added. The section states this in prose as well: no executable sweep is added by this
delivery, no such sweep exists in this repository, and building one belongs to the repository-wide sweep
item rather than to any single bug-fix child.

Output Summary: Both rules are present, each as its own numbered statement in a new section, and the
file adds no script and no command block.
