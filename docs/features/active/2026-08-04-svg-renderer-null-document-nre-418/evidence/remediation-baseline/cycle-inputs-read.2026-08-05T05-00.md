# Cycle Inputs Read — Remediation Cycle 2

- Task: `[P0-T4]`
- Timestamp: 2026-08-04T23-28
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- Command: (documentary read task — no shell command; all four files read in full with the Read tool)
- EXIT_CODE: 0

## Files read, in full, in the mandated order

| # | Path | Lines read | Cited finding IDs relevant to this cycle |
|---|---|---|---|
| 1 | `remediation-inputs.2026-08-04T22-28.md` | 1-293 (entire file) | R-1 (blocking, human-only), R-7 (blocking, this cycle's item), R-8..R-12 (non-blocking), G-1/G-4/G-9 non-actionable |
| 2 | `code-review.2026-08-04T22-28.md` | 1-93 (entire file) | the single Blocking row (referred to as CR-8 by the inputs and by the plan), plus four Low and four Info rows |
| 3 | `policy-audit.2026-08-04T22-28.md` | 1-697 (entire file) | G-8 (FAIL, BLOCKING, the item this cycle delivers), G-2 (AC-11, blocking, human-only), G-1/G-3/G-9 non-blocking |
| 4 | `feature-audit.2026-08-04T22-28.md` | 1-129 (entire file) | AC-10 PARTIAL (the AC consequence of G-8), AC-11 FAIL |

All four are paths under this feature folder,
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/`.

### Note on the code-review finding label

The plan and the inputs both refer to the code review's Blocking row as **CR-8**. Measured at the
point of writing this artifact: `code-review.2026-08-04T22-28.md` does **not** print the literal token
`CR-8` anywhere. It labels the row only as `Blocking` in the § `## Findings Table` severity column
(line 51) and describes it in § `## Executive Summary` as "One new Blocking finding". The tokens
`CR-1` through `CR-7` do appear in that file, as the cycle-1 findings it verifies resolved. The label
`CR-8` originates in `remediation-inputs.2026-08-04T22-28.md` § R-7 ("code review CR-8 (Blocking)")
and in `feature-audit.2026-08-04T22-28.md` AC-10 ("code-review finding CR-8"). The referent is
unambiguous — there is exactly one Blocking row in the code review and it is the missing-`ExCSS`
finding — so this is a naming observation, not a discrepancy in substance. Recorded because the plan
directs re-measurement rather than transcription.

## Binding constraint set for this cycle

Reproduced verbatim from `remediation-inputs.2026-08-04T22-28.md` § `## Do Not Do` (lines 237-256).
This list is binding on every task in this plan.

> ## Do Not Do
>
> - Do not widen scope beyond the enumerated items. Work mode is `minor-audit`.
> - Do not attempt R-1. It is a human-only item; assigning it to an agent will produce a false capture.
> - Do not weaken, retarget, or delete any existing assertion to make a test pass. In particular, do not
>   change the `XmlException` assertions in
>   `TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException` or
>   `GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner`; those assertions are correct
>   and R-7 is what makes them hold unconditionally.
> - Do not add `[ExcludeFromCodeCoverage]` or a `coverage.config` exclusion to address G-1 or G-9.
>   `.claude/rules/general-unit-test.md` prohibits excluding production files from coverage measurement.
> - Do not modify any `app.config` binding redirect. The stale `Fizzler` and `Unsafe` redirects are
>   deliberately deferred to `docs/features/potential/`.
> - Do not attempt to fix the 195 pre-existing `UtilitiesCS` nullable diagnostics.
> - Do not modify policy documents under `.claude/rules/` or `.github/instructions/`.
> - Do not alter AC text or clear an existing `[x]`. If R-7 lands, AC-10's existing `[x]` becomes
>   accurate on its own; add an evidence note, do not restate the criterion.
> - Do not use temporary files in tests.
> - Do not report a green toolchain from a build that compiled nothing. When verifying the type-check
>   stage, force a recompile of the changed projects and state that you did.

### How each constraint is honoured by this plan

| Constraint | Honoured by |
|---|---|
| No scope widening | Scope Lock: two build-configuration files plus documentation/evidence. `[P1-T7]` guards it. |
| Do not attempt R-1 | R-1 is represented by **no task**. `[P2-T11]` leaves AC-11 `- [ ]`. |
| Do not weaken any assertion | `[P1-T5]` names weakening an assertion as a forbidden response to a failure and requires a halt instead. No `.cs` file is edited at all. |
| No `[ExcludeFromCodeCoverage]` / `coverage.config` exclusion | No task targets G-1 or G-9; `[P2-T8]` records both as not targeted. |
| Do not modify any `app.config` | Design Decision 4; the Scope Lock lists every `app.config` as out of scope; `[P1-T7]` asserts none appears in the diff. |
| Do not fix the 195 `UtilitiesCS` nullable diagnostics | Scope Lock excludes them; `[P2-T6]` compares against the transcribed basis rather than attempting a fix. |
| Do not modify `.claude/rules/` or `.github/instructions/` | Scope Lock excludes both. |
| Do not alter AC text or clear an `[x]` | `[P2-T11]` is append-only on AC-10 and changes no checkbox. |
| No temporary files in tests | No test source is modified. |
| Do not report a green toolchain from a build that compiled nothing | `[P2-T6]` states the mandated command's exit 0 is not evidence of nullable cleanliness and adds two **forced** `/t:Rebuild` project-scope runs. |

## R-7 / R-11 label reconciliation

Restated from this plan's § `## Scope of This Cycle` → `### Item label reconciliation`. The cycle-entry
inputs and the orchestrator directive use the same label for different items, so both readings are
delivered; neither can conflict with the other because both fall inside the same Scope Lock and edit
the same `<ItemGroup>`.

| Item | Inputs label | Directive label | Delivered by |
|---|---|---|---|
| Missing `ExCSS` reference on `SVGControl.Test` (the blocking finding) | R-7 | R-11 | `[P1-T1]`, `[P1-T2]` |
| `<Private>True</Private>` on the existing `Svg` reference | R-11 | (not separately labelled) | `[P1-T3]` |

Verified against the inputs at the point of writing: `remediation-inputs.2026-08-04T22-28.md` § R-7
(line 72) is titled "Add the missing `ExCSS` reference to `SVGControl.Test` (BLOCKING, new, one-line
class of change)", and § R-11 (line 192) is titled "Add `<Private>True</Private>` to the `Svg`
reference (non-blocking, bundle with R-7)". The inputs' own § `## Exit Criteria for This Cycle` (line
292) states "R-11 should be bundled with R-7 since both edit the same `ItemGroup`", which this plan
does. No other enumerated item is delivered this cycle.

## Deliberate departure from the inputs, recorded here as read

`remediation-inputs.2026-08-04T22-28.md` § R-7 (lines 105-108 and 116) directs adding a `Fizzler`
reference and `packages.config` entry "for parity with the eight sibling test projects". This plan
**deliberately omits it** per Design Decision 3, on four measured grounds recorded there and
re-measured at `[P0-T9]`. The departure is recorded rather than silently taken. `[P2-T12]` restates it
at exit.

## Output Summary

All four cycle-input artifacts were read in full, in the mandated order, before any Phase 1 task. The
binding `## Do Not Do` list is reproduced verbatim above as this cycle's constraint set, with the task
that honours each constraint named. The R-7 / R-11 label collision is reconciled by delivering both
readings. One naming observation is recorded: the token `CR-8` does not appear in the code review
itself; it is the label the inputs and the feature audit assign to that file's single Blocking row.
