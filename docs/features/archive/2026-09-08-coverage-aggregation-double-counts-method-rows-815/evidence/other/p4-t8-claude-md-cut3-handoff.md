# P4-T8 — CLAUDE.md CUT3 Step 4 Wording Mismatch: Handoff, Not Fix

Timestamp: 2026-09-09T11-21
Task: [P4-T8]
Command: MCP promotion route attempted; tool not present in this executor's tool surface
EXIT_CODE: 0

## POSTING BLOCKED

**Reason.** The MCP promotion route for creating a new potential entry or bug issue is not present in
this executor's tool surface. The only MCP tools available to this session are
`mcp__drm-copilot__run_poshqc_format`, `mcp__drm-copilot__run_poshqc_analyze`,
`mcp__drm-copilot__run_poshqc_test` and `mcp__drm-copilot__run_poshqc_analyze_autofix`. No promotion,
issue-creation or feature-lifecycle tool is exposed, so the call could not be made with an explicit
`promotion_type` and `work_mode` as the task specifies. Per the task's own instruction this is not a
halt condition: the intended promotion text is recorded in full below so the orchestrator can raise
it directly.

No issue number and no issue URL can be recorded, because no issue was created.

## The mismatch, stated precisely

`CLAUDE.md` section **CUT3 (C# Toolchain Command Selection), step 4** names:

```
vstest.console.exe <test-assembly-paths> /EnableCodeCoverage
```

The coverage route actually in use in this repository is **`dotnet-coverage collect` wrapping
`vstest.console.exe`**, with the inner vstest invocation deliberately **not** given
`/EnableCodeCoverage`.

**Corroborating in-code comment**, at `scripts/vscode/Invoke-MSTestWithCoverage.ps1` lines 19-26, in
the `.DESCRIPTION` block of `Resolve-RunSettingsPath`:

> The CLI runsettings (TaskMaster.cli.runsettings) lives alongside this script in scripts/vscode and
> is resolved deterministically from the script directory. It carries the MSTest parallelization only
> and no coverage data collector, so the inner vstest invocation never activates the Code Coverage
> collector; instrumentation comes solely from the outer dotnet-coverage --settings coverage.config
> path.

The omission is therefore deliberate and load-bearing: enabling the built-in collector alongside the
outer `dotnet-coverage` instrumentation conflicts. `scripts/vscode/TaskMaster.cli.runsettings`
carries no data collector, which is the second half of the same arrangement.

## Consequence, and why it is worth raising

The mismatch is what made issue 809's AC6 read as PARTIAL on wording alone: the reviewer found the
delivery had not run the command `CLAUDE.md` names, recomputed AC6's measurable clauses from raw
Cobertura, and found they pass. A policy document that names a command the repository deliberately
does not run produces false PARTIAL verdicts on every future item that cites it.

## Intended promotion text, in full

> **Title:** CLAUDE.md CUT3 step 4 names a coverage command the repository deliberately does not run
>
> **Type:** bug. **Work mode:** minor-audit.
>
> **Summary.** `CLAUDE.md` section CUT3 step 4 specifies
> `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` as the C# test toolchain command.
> The route actually in use is `dotnet-coverage collect` wrapping `vstest.console.exe`, with
> `/EnableCodeCoverage` deliberately omitted from the inner invocation because the outer
> `dotnet-coverage` instrumentation conflicts with the built-in Code Coverage collector. The comment
> at `scripts/vscode/Invoke-MSTestWithCoverage.ps1` lines 19-26 records that reasoning in code, and
> `scripts/vscode/TaskMaster.cli.runsettings` carries no data collector.
>
> **Impact.** A policy document naming a command the repository does not run produces false PARTIAL
> verdicts against acceptance criteria that cite it. This occurred on issue 809's AC6, where the
> reviewer recomputed the measurable clauses from raw Cobertura and they passed; the PARTIAL was on
> wording alone.
>
> **Proposed correction.** Reword CUT3 step 4 to name the `dotnet-coverage collect` route, and cite
> `scripts/vscode/Invoke-MSTestWithCoverage.ps1` as the implementation of record.
>
> **Ownership caveat, and the reason this cannot be a drive-by edit.** `CLAUDE.md` is not modified by
> issue #815's branch. `spec.md` Non-Goal 1 places this feature under a hard constraint not to modify
> it, and the epic manifest records the same constraint for a sibling feature. Whether the correction
> belongs in this repository or upstream in `drm-copilot` needs deciding first: everything under
> `.claude/` other than `agent-memory/` is pushed down from that governance repository with no
> templating, so a change made here would be overwritten. `CLAUDE.md` itself sits at the repository
> root rather than under `.claude/`, so its ownership should be confirmed before the edit is made.
>
> **Origin.** Raised by the delivery of issue #815 under AC14, which requires the mismatch to be
> handed off rather than fixed in that change. Bundling it would have put a `CLAUDE.md` edit inside a
> change whose only code surface is three PowerShell files.

## Consequence for AC14, recorded at 2026-09-09T11-45

AC14 requires the mismatch to be "recorded in this feature's evidence **with a pointer to a separate
promotion or issue raised for it**, and CLAUDE.md does not appear in this branch's diff."

Two of the three clauses are satisfied: the mismatch is recorded here in full, and P4-T7 verified
individually that `CLAUDE.md` appears in neither the anchored diff nor the porcelain status. **The
middle clause is not satisfied**: no promotion or issue was raised, so no pointer to one exists, for
the tool-surface reason stated above.

AC14 is therefore left as `- [ ]` in `spec.md` and reported as PARTIAL. Plan task P6-T14 accepts a
`POSTING BLOCKED` artifact as sufficient for its own check-off, but the criterion's own text asks for
a pointer that does not exist, and marking it delivered would assert a fact this run cannot verify.
The residual action is for the orchestrator, which has the promotion route available: raise the issue
using the text below, then check AC14 off citing the resulting issue number and URL.

## RESOLUTION — promotion raised by the orchestrator at 2026-09-09T15-42

The `POSTING BLOCKED` state recorded above was a tool-surface limitation of the executor session, not
of the run. The orchestrator holds the promotion route and has now exercised it, so the middle clause
of AC14 is satisfied and the criterion is checked off.

- `PostedAs: body`
- **Issue number:** 828
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/828
- **Title:** Bug: claude-md-cut3-names-uninvoked-coverage-command
- **State at creation:** OPEN
- **Work mode:** minor-audit. **Promotion type:** bug.
- **Potential entry:** `docs/features/potential/2026-09-09-claude-md-cut3-names-uninvoked-coverage-command.md`
- **Promoted record:** `docs/features/potential/promoted/2026-09-09-claude-md-cut3-names-uninvoked-coverage-command.md`

Fidelity check, per the promotion tooling's section-mapping behaviour: the issue body is 3383 bytes
and contains zero occurrences of the literal `not provided in potential file`, so every canonical
bug-template section carried through with its content intact. The ownership caveat — that `CLAUDE.md`
ownership between this repository and `drm-copilot` must be settled before any edit, because
everything under `.claude/` other than `agent-memory/` is pushed down with no templating — is carried
in the issue body itself under `## Suspected Cause / Notes`, so the reader who picks up issue 828
does not need this feature folder.

Scope note: creating the promotion wrote two files under `docs/features/potential/`. That prefix is
one of the five permitted prefixes of plan decision D6, whose Deviation 2 anticipates exactly this
case and admits the prefix because AC14 requires a promotion and this repository's promotion
lifecycle is file-based. The AC13 scope gate is therefore unaffected, and it was re-verified against
the committed tree after this promotion landed.

`EXIT_CODE: 0` for the promotion route. The `Command:` field at the head of this artifact records the
executor's blocked attempt and is left unaltered as the audit trail of that attempt.

## CLAUDE.md is not modified by this branch

This feature makes no change to `CLAUDE.md`. The companion assertion — that `CLAUDE.md` is absent
from this branch's diff — is carried by P4-T7, which asserted it individually against both the
anchored name-only diff and the porcelain status and found zero occurrences. See
`evidence/qa-gates/p4-t7-scope-boundary.md`.

Output Summary: The mismatch is stated precisely, with the corroborating in-code comment at
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` lines 19-26 quoted. The MCP promotion route is not
available in this executor's tool surface, so the artifact is marked `POSTING BLOCKED` with that
reason and the intended promotion text is recorded in full for the orchestrator to raise. No issue
number or URL exists. `CLAUDE.md` is not modified by this branch.
