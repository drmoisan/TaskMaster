# MANUAL-ONLY Criteria Remain Unchecked — Issue #503 (P7-T2)

Timestamp: 2026-08-08T15-01

Source file audited: `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\spec.md`

Command:
```
grep -nE "^\- \[.\] \*\*AC(19|20|21)" spec.md
```

## The three criterion lines, quoted verbatim from `spec.md`

Line 456:

```
- [ ] **AC19 (R5b, A3) — MANUAL-ONLY.** In a live Outlook Explorer, immediately after add-in reload and before initialization completes, each of the eight engine-backed ribbon commands is clicked and produces no `NullReferenceException` and no `KeyNotFoundException` in the log, and shows the "still loading" indication. Requires a running Outlook process and a live mail profile; **must not be checked off on the strength of unit tests.** Outcome recorded under `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/manual-verification/`.
```

Line 457:

```
- [ ] **AC20 (R5b, A2, A3) — MANUAL-ONLY.** In the same live session, after `InitAsync()` completes, each of the eight commands behaves exactly as it did before this change. Requires live Outlook; outcome recorded under `.../evidence/manual-verification/`.
```

Line 458:

```
- [ ] **AC21 (R5c, A4) — MANUAL-ONLY.** In a live Outlook session, Office visually greys the eight buttons during initialization and re-enables them after the post-`InitAsync()` invalidation fires, without an add-in restart. This also confirms the `getEnabled` callback is actually bound, which VSTO does not report on a signature mismatch. Office's callback-caching behavior is internal to the host and is not locally observable; **must not be checked off on the strength of unit tests.** Outcome recorded under `.../evidence/manual-verification/`.
```

## Assertion

| Criterion | Line | Prefix | Still `- [ ]`? |
|---|---|---|---|
| AC19 | 456 | `- [ ] **AC19` | **Yes** |
| AC20 | 457 | `- [ ] **AC20` | **Yes** |
| AC21 | 458 | `- [ ] **AC21` | **Yes** |

Binary outcome: **PASS** — all three remain `- [ ]`.

Checking any of them off from unit-test or source-inspection evidence would be a policy violation, and no such check-off was made. Their maintainer checklist is at `<FEATURE>\evidence\manual-verification\ac19-ac21-checklist.2026-08-08T15-00.md`, which carries `Status: PENDING MAINTAINER EXECUTION`.
