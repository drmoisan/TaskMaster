---
name: durable-script-copy-into-feature-folder
description: When a delegation prompt supplies an implementation script located under a session-scoped scratchpad temp directory, copy it into <FEATURE>/scripts/ before referencing it in plan tasks
metadata:
  type: feedback
---

A calling agent (issue #354, stale-app-config-binding-redirects) supplied a proven-correct audit/fix script at a path under `AppData\Local\Temp\claude\...\<session-guid>\scratchpad\`. That path is tied to the current session and is not guaranteed to exist when `atomic-executor` (or a resumed/later session) actually executes the plan.

**Why:** Atomic tasks require explicit, durable file paths (per `atomic-plan-contract`). A path inside a session-specific scratchpad directory is ephemeral infrastructure, not a repo artifact, so a plan that references it verbatim risks becoming unexecutable the moment the authoring session ends.

**How to apply:** Before writing a plan task that runs a script supplied via a scratchpad/temp path, read the script's full contents and `Write` an identical copy into `<FEATURE>/scripts/<name>.py` (or another durable in-repo location appropriate to the feature). Reference the durable copy's repo-relative path in the plan task instead of the scratchpad path. This applies whether the script is a one-off audit tool, a fix script, or similar — anything the calling agent hands off from outside the repo tree.

Related: [[evidence-path-normalization]] (durable evidence-path handling follows the same "canonicalize before writing tasks" principle).
