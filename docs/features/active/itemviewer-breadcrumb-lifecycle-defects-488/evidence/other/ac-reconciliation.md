# Acceptance-Criteria Reconciliation ([P9-T15])

Timestamp: 2026-08-28T06-35

Command: checkbox counts over
`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/spec.md`, plus a
`git diff <BASE_SHA>` of that file with the checkbox prefix stripped from both sides and the results
compared.
EXIT_CODE: 0

> **Superseded at 2026-08-28T06-40.** The remediation this artifact requested was performed by the
> orchestrator; the outcome is now **54 of 54 checked — PASS**. See "Remediation performed" at the end
> of this artifact. Everything between here and that section is the executor's record as written at
> 2026-08-28T06-35 and is retained unaltered.

## Outcome as recorded by the executor: 53 of 54 checked — REMEDIATION-REQUIRED, not a pass

| Measure | Value |
| --- | --- |
| Total acceptance-criterion checkboxes | **54** |
| Checked `- [x]` | **53** |
| Remaining `- [ ]` | **1** |

The file contains exactly **54** acceptance-criterion checkboxes, matching the count `[P0-T2]` recorded
and the distribution the spec's own preamble states.

**This task's outcome is recorded as remediation-required and must not be reported as a pass.** The one
unchecked criterion is **not** the single authorized exception, so the plan's default applies: any other
unchecked criterion is remediation-required.

## No criterion text was modified

`git diff --numstat` for `spec.md` reports `53 53` — fifty-three lines changed, matching the
fifty-three check-offs exactly. Every changed line begins with a checkbox marker; a filter for changed
lines that are *not* checkbox lines returns **0**.

Stronger still, the criterion text itself was compared directly: stripping `- [ ] ` from every removed
line and `- [x] ` from every added line and diffing the two sorted sets produces **no differences**.
Only the single character inside the brackets changed on each line. No criterion was reworded,
reordered, added, or removed.

## The one remaining criterion, verbatim

At `spec.md:881-884`:

```
- [ ] The research §3.5 open item is discharged with recorded evidence: it is confirmed whether a faulted
      `QfcItemController.InitializeWebViewAsync` task is observed by its caller. If it is not observed, a
      new issue is opened against `QfcItemController.ViewerSetup.cs` (484-owned) and referenced here —
      **the guard is not weakened in response.**
```

### Why it is left unchecked

The criterion is a conjunction, and **two of its three clauses are delivered while the third is not**:

| Clause | Status |
| --- | --- |
| The open item is discharged with recorded evidence; it is confirmed whether the task is observed | **DELIVERED** — `[P5-T6]` enumerated every in-repo caller and concluded the task is **not** observed: three of four production call sites discard it (`Initialization.cs:192`, `:288`, `:324`); only `:256` awaits it |
| The guard is not weakened in response | **DELIVERED** — D5's `ObjectDisposedException` guard is unchanged and unweakened, and `QfcItemController.ViewerSetup.cs` was read but not edited, confirmed by `[P7-T3]`'s empty forbidden-file diff |
| A new issue is opened against `QfcItemController.ViewerSetup.cs` and referenced here | **NOT DELIVERED** |

Because the task **is** not observed, the issue clause is live, and it names an **issue** specifically —
unlike the three-follow-up criterion `[P7-T14]` flips, which accepts "a potential entry or GitHub
issue". A potential entry alone does not satisfy it.

### Why the issue could not be opened

The repository enforces an MCP-only promotion path.
`.claude/hooks/enforce-promotion-mcp-only.ps1` blocks `gh issue create`, `gh issue new`, and a POST to
the repository issues API, with the reason:

```
PROMOTION_MCP_ONLY_BLOCKED: Direct GitHub issue creation via `gh` bypasses the approved drm-copilot MCP
promotion path (`mcp__drm-copilot__new_potential_entry` -> `mcp__drm-copilot__potential_to_issue` ->
`mcp__drm-copilot__new_active_feature_folder`). Use those MCP tools instead.
```

**None of those three MCP tools is available in this executor's tool set** — the only `drm-copilot` MCP
tools exposed to this session are the four PoshQC ones. `gh` is installed and authenticated with `repo`
scope, so the blocker is the approved-path policy, not credentials. The approved path was unavailable
and **the forbidden path was not used**; no wording was altered to evade the hook.

### What was delivered instead, and what remains

The follow-up is fully prepared and needs only promotion. `[P5-T6]` created
`docs/features/potential/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md`, filed against
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, carrying the standard potential-entry front
matter and section headings the promotion tooling maps into the GitHub bug issue template, and naming
the mechanism, the four call sites with their observation status, and the trigger.

**Remediation required:** the caller, who holds the MCP promotion tools, runs
`mcp__drm-copilot__potential_to_issue` against that entry, then records the resulting issue number and
URL in `evidence/qa-gates/d5-faulted-task-observation.md` and checks this criterion off. Two plan tasks
are correspondingly left unchecked and reported: `[P5-T6]`, whose acceptance requires the issue number
and URL, and `[P5-T11]`, which flips this criterion.

### The authorized exception was NOT used

The single exception this task authorizes is the pre-existing-unformatted-file branch of `[P9-T6]`: if
the `[P0-T9]` baseline unformatted set were non-empty and `[P8-T2]` reported exactly that same set, the
formatting criterion would be left unchecked and the outcome would still be a pass.

That branch is not in play. `[P0-T9]` recorded an **empty** baseline unformatted set and `[P8-T2]`
reported an **empty** set over 1554 files, so the formatting criterion was checked off normally in
`[P9-T6]`. The one unchecked criterion is a different one, and the exception does not extend to it.

Output Summary: `spec.md` contains exactly **54** acceptance-criterion checkboxes, **53 checked** and
**1 remaining**, with **no criterion text modified** — the diff is `53 53`, all on checkbox lines, and
the stripped text sets are identical. The outcome is **remediation-required**, not a pass. The one
unchecked criterion is the research §3.5 item: its discharge and its guard-not-weakened clauses are
delivered, but the GitHub issue it requires could not be opened because the MCP-only promotion tools are
unavailable to this executor and the `gh` path is hook-forbidden. The authorized
pre-existing-unformatted-file exception was not used and does not apply.

---

## Remediation performed — outcome revised to 54 of 54, PASS

Timestamp: 2026-08-28T06-40

Command: `mcp__drm-copilot__potential_to_issue` with `promotion_type=bug`, `work_mode=full-bug`, and an
absolute `potential_path` to the executor's prepared entry.
EXIT_CODE: 0

The orchestrator holds the MCP promotion tool set the executor lacked, so it ran exactly the approved
path the executor identified. `gh issue create` was not used and no wording was altered to evade
`enforce-promotion-mcp-only.ps1`.

| Measure | Executor | After remediation |
| --- | --- | --- |
| Total acceptance-criterion checkboxes | 54 | **54** |
| Checked `- [x]` | 53 | **54** |
| Remaining `- [ ]` | 1 | **0** |
| Plan tasks checked | 143 of 145 | **145 of 145** |
| Outcome | remediation-required | **PASS** |

The third clause of the research §3.5 criterion is now delivered: GitHub issue
**[#670](https://github.com/drmoisan/TaskMaster/issues/670)** — "Bug:
qfc-initializewebviewasync-fault-is-unobserved" — is OPEN against
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, verified with
`gh issue view 670 --json number,title,state,url`. It is referenced by number and URL in the D5 section
of `spec.md` and in `evidence/qa-gates/d5-faulted-task-observation.md`. The promoted record is at
`docs/features/potential/promoted/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md`.

The other two clauses were already delivered and were not revisited: the D5 guard remains unweakened and
`QfcItemController.ViewerSetup.cs` remains unedited.

The check-off changed only the single character inside the brackets on `spec.md`'s research §3.5
criterion. No criterion text was modified, added, removed, or reordered, and the total remains exactly
54. Separately, a non-criterion paragraph was added to the spec's D5 design section recording the
discharge and the #670 reference, because the criterion requires the issue to be "referenced here"; that
paragraph is prose, not a checkbox, and does not alter the criterion count.

Output Summary (revised): `spec.md` contains exactly **54** acceptance-criterion checkboxes, **54
checked** and **0 remaining**, with **no criterion text modified**. The plan is **145 of 145** tasks
complete. The single blocking gap — the GitHub issue for the research §3.5 follow-up — was closed by
promoting the executor's prepared entry through the approved MCP path to issue **#670**. The outcome of
`[P9-T15]` is **PASS**.
