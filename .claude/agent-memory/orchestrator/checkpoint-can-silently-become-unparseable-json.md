---
name: checkpoint-can-silently-become-unparseable-json
description: orchestrator-state.json built by successive text edits can be invalid JSON while reading fine to a human; re-run the validator AFTER every hand edit
metadata:
  type: feedback
---

`artifacts/orchestration/orchestrator-state.json` is maintained by successive Edit-tool
insertions across many turns and agents. That process can produce a file that is not parseable
JSON while still looking completely normal when read.

**Why:** On issue #796 I ran the routing validator before my first delegation and it failed —
not on a routing defect, but with `Expected ',' or '}' ... at line 308`. A stray `]` appended
during the *previous* run's round-4 record had terminated that object one key early, orphaning
the key after it. It had survived undetected because nothing re-parsed the file between that
append and my resume, and the `model_routing_preflight: pass` recorded in between predated the
damage. Every gate that reads this file — PR-creation readiness, the completion gate, the
PreToolUse hooks — uses a real parser, so the file had been silently unusable by all of them.

**How to apply:** Run `mcp__drm-copilot__validate_orchestration_artifacts` with
`artifact_type=orchestrator-state` **after** every hand edit to the checkpoint, not only before
a delegation. It is one cheap call and it is the only thing that distinguishes "reads correctly"
from "parses correctly." When it reports a position, read the surrounding 20 lines rather than
the reported line alone — the stray bracket sat several lines above the reported column.

When repairing, change only the structural character and alter no recorded value, then say so in
the checkpoint. A repair that also edits content is indistinguishable from tampering in an audit
trail. Related: [[orchestrator-state-flat-keys-and-enum]] and
[[orchestrator-state-validator-divergence]].
