---
name: bash-hook-keyword-false-positive
description: Bash PreToolUse hooks match keywords in the whole command text, so a heredoc that merely mentions promotion/lifecycle terms is denied even when the command only writes JSON
metadata:
  type: project
---

Bash enforcement hooks in this repo match **keywords across the entire command string**, including heredoc
body text. A `python - <<EOF` block whose *data* mentioned the promotion lifecycle tools was denied with
`PROMOTION_MCP_ONLY_BLOCKED: Direct Bash promotion-script execution is not allowed`, even though the command
ran no script and only serialized JSON into the epic checkpoint.

**Why:** This is the same regex-over-prompt-text design as the pre-implementation gate
([[preimplementation-gate-blocks-epic-execution]]), where pass/fail is decided by a token match rather than by
what the call actually does. Expect false positives whenever checkpoint prose quotes tool names.

**How to apply:** Write the payload to a scratchpad file with the Write tool, then run
`python <path>` so the command line carries no trigger tokens. This is not evading the gate — the gate exists
to stop Bash-invoked promotion scripts, and writing a JSON audit record is not that; the fix removes an
accidental token match, not a control. Do **not** reword the *record itself* to dodge a keyword: the audit
trail has to keep naming what happened. Disclose the false positive when reporting.
