---
name: expectedexitcode-is-per-file-so-multigate-artifacts-invert
description: An evidence artifact recording several gates under one file-level ExpectedExitCode renders spurious "Normalized result: fail" rows in pr_context — the collector pairs the expectation with the FIRST command, not the one it describes
metadata:
  type: project
---

`ExpectedExitCode` is a **per-FILE** field, but an executor naturally writes one artifact per
plan task and a task often runs four or five gates. The collector then pairs the single
file-level expectation with the **first** `Command:` it parses, which is usually not the gate
the expectation was written for.

**Worked example, issue #798.** `evidence/regression-testing/p4-ac3-pass-after.md` declares
`ExpectedExitCode: 1` at line 10, correctly describing its *last* gate — a scoped vstest run at
line 107 that legitimately exits 1 because later-phase tests were still red at that point. The
collector paired that expectation with the first command, `csharpier format` at line 18, which
exits 0. The row inverted to `Normalized result: fail`. Three artifacts on that branch inverted
the same way, and every underlying gate was fine.

**Why it matters:** the inverted rows land in the generated PR context, so a reviewer reading
the artifact sees `fail` against a green change and has no way to tell it from a real failure.

**How to apply.**
- When authoring: give any gate needing a non-zero expectation **its own artifact file**. The
  evidence-conventions skill states this, and this is the concrete failure it prevents.
- When reviewing or authoring a PR body: before trusting a `fail` row in `pr_context`, open the
  artifact and check whether it carries several `Command:` lines under one `ExpectedExitCode:`.
- Do **not** rewrite committed evidence after the fact to fix the rendering. Annotate it in the
  PR body's review guide instead — rewriting a recorded run to improve a downstream display is
  worse than the display defect.

Related: [[pr-context-summary-unreliable-gh-and-classification]],
[[pr-context-top-n-churn-truncation-kills-coverage-gate]].
