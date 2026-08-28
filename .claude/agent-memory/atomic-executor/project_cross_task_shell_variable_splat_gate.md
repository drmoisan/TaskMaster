---
name: cross-task-shell-variable-splat-gate
description: A plan task that consumes a shell variable produced by an earlier task is vacuous unless the enumeration body is re-executed in the SAME -Command payload and a count-identity gate plus a >= 1 floor is asserted.
metadata:
  type: project
---

When a plan enumerates test assemblies in task A (`$assemblies = @(...)`) and splats them in task B
(`& $vstest @assemblies ...`), no shell state persists between tool invocations, so a bare
`@assemblies` in task B expands to nothing. vstest then runs zero assemblies and still reports a
zero failure count, so the gate passes while measuring nothing.

**Why:** #491 preflight round 2 blocked on exactly this. The fix that cleared round 3 was two-part:
(1) task B must re-execute task A's enumeration BODY (not the whole `pwsh -NoProfile -Command '...'`
wrapper — nesting pwsh puts the variable in a child session) inside the same payload; (2) acceptance
must assert the assembly count on the command line equals the count recorded in task A's artifact
AND is at least 1. The `>= 1` conjunct is what makes it falsifiable; the equality alone normally
always holds because nothing between the two tasks adds a test project.

**How to apply:** Whenever a plan passes a shell variable across task boundaries, demand both the
same-payload clause and the `>= 1` floor. Also check the composed payload's quoting: a vswhere
re-resolution embeds single quotes (`& 'C:\Program Files (x86)\...'`) and needs them doubled inside
a single-quoted `-Command` payload. See [[project_pwsh_command_quoting_from_bash]] and
[[project_preflight_selfderived_gate_thresholds_are_blind]].
