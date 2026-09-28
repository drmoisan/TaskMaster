---
name: mandatory-string-array-param-rejects-blank-line
description: A mandatory [string[]] parameter rejects an array containing a blank element, so a test helper fed a workflow file's lines fails with a binding error instead of its assertion — a fail-before that proves nothing
metadata:
  type: project
---

`[Parameter(Mandatory = $true)][string[]]$Line` rejects an array that contains an **empty-string
element**, with `Cannot bind argument to parameter 'Line' because it is an empty string.` A YAML
workflow file is full of blank lines, so a helper that parses one this way never runs.

**Why:** in an `[expect-fail]` task this is worse than a plain bug. The test **is** red, the
task's acceptance (`Failed=1`, `EXIT_CODE: 1`) **is** satisfied, and the fail-before evidence
looks complete — but the red is a binding error, not the assertion. The test would have stayed
red after the fix, and the pass-after half of the pair could never close. A red for the wrong
reason is worthless as fail-before evidence, and the acceptance conditions as written do not
distinguish the two.

**How to apply:** add `[AllowEmptyString()]` when a mandatory `[string[]]` receives file lines:

```powershell
[Parameter(Mandatory = $true)][AllowEmptyString()][string[]]$Line,
```

Then **read the failure message** of every `[expect-fail]` run and confirm it names the
assertion, not the parameter binder. Quote it verbatim into the evidence artifact — that is what
makes the defect visible to the next reader.

A sibling helper in the same file can carry the same declaration and work, because the file it
parses happens to have no blank line in the region it scans. Do not infer safety from a passing
sibling.

Confirmed 2026-09-20 on issue #911 remediation cycle 1, task P3-T1.
