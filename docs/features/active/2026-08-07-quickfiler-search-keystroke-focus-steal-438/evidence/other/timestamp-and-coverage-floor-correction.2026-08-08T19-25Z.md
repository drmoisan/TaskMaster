# Orchestrator Correction Note — Evidence Timestamps and the Cycle-1 Coverage Floor

- **Issue:** #438
- **Author:** orchestrator (main session)
- **Written at:** 2026-08-08T19:25:43Z (measured via `Get-Date`, not estimated)

This note records two corrections that affect how the evidence in this feature folder should be read. Both are disclosed rather than silently repaired, because the underlying artifacts are an audit trail.

---

## 1. Evidence timestamp labels are unreliable; file content is not

The remediation executor disclosed that the ISO-8601 timestamps embedded in its evidence filenames and headers were **estimated rather than read from the system clock**. Labels in the range `20:45`–`22:35` were written while the actual local time was approximately `15:23`–`15:25` (`19:23`–`19:25` UTC).

**The same defect applies to this orchestrator's checkpoint.** Timestamps written into `artifacts/orchestration/orchestrator-state.json` during this run (`complexity_assessments[].assessed_at`, `delegation_receipts[].started_at`/`completed_at`, `last_updated`, and similar fields) were also estimated. Several are internally inconsistent and some are in the future relative to the real clock at the time of writing.

What this does and does not affect:

- **Unaffected:** all technical content — commands executed, exit codes, coverage figures, test counts, diffs, and findings. Every such value in this folder was produced by a real command and is independently reproducible. The orchestrator re-verified the load-bearing ones directly (see section 2).
- **Unaffected:** the *relative ordering* of artifacts. Sequence numbers, pass labels (`pass1`, `pass2`, `final`), and phase/task IDs correctly express what happened in what order.
- **Affected:** any inference about *absolute wall-clock time* or *elapsed duration* between artifacts. Do not use these labels to reason about how long a stage took, or to correlate against external logs such as CI runs.

Files were **not** renamed. Renaming would break the cross-references already written into the plan, the evidence bodies, and the checkpoint, and would substitute one inaccuracy for another. The labels are retained as opaque ordering identifiers, and this note is the authoritative statement of their meaning.

Corrective practice going forward: read the clock (`Get-Date`) before stamping an artifact. Do not derive a timestamp from context, from a prior artifact's label, or from an estimate of elapsed time.

---

## 2. The Cycle-1 repo-wide line-coverage constant is not reproducible

Plan task `[P2-T7]` required repo-wide coverage to be no lower than **line `0.858665` / branch `0.792502`**, the figures recorded by the Cycle-1 full-suite run. The remediation's final run measured **line `0.858620` / branch `0.792860`**. Branch clears the floor; line falls short by roughly `0.00005`.

That shortfall is **not** caused by the remediation. The decisive control measurement is the remediation's own Phase 0 baseline, captured on the **unmodified tree before any test was added**:

| Measurement | Repo line-rate | Repo branch-rate | Target file branch-rate |
|---|---|---|---|
| Cycle-1 final (the floor constant) | 0.858665 | 0.792502 | 0.5 (2/4) |
| Remediation Phase 0 baseline (unmodified tree) | **0.858512** | 0.792359 | 0.5 (2/4) |
| Remediation final | **0.858620** | 0.792860 | **1.0 (4/4)** |

The unmodified tree already measured `0.858512`, below the `0.858665` floor. The floor therefore encodes run-to-run measurement variance, not a property of the source. Measured against a same-session baseline — the only valid comparison — the remediation **increased** both figures: line `0.858512 → 0.858620` and branch `0.792359 → 0.792860`.

All figures in the table above were independently re-verified by the orchestrator by parsing the Cobertura artifacts directly, not by reading the executor's summaries.

### Disposition

The orchestrator's position, submitted to feature-review for adjudication rather than self-approved:

1. The applicable policy floors are met. `.claude/rules/general-unit-test.md` requires line `>= 85%` and branch `>= 75%`; the final run gives **85.862%** and **79.286%**. `CLAUDE.md` requires `>= 80%` line.
2. The no-regression requirement is satisfied against a same-session baseline, which is the comparison that isolates the change under test.
3. `[P2-T7]` is left **unchecked** in the plan. Its literal acceptance text was not met, and checking it off would misrepresent the record.

### Underlying defect, now tracked

The variance was isolated to a small, rotating set of legacy classes unrelated to this work — `EfcHomeController.cs`, `PropertyStore.cs`, `SubjectMapSco.Orchestration.cs`, and `SegmentStopWatch.cs`, the last being a wall-clock timing helper. The target file was never among them. Five full-suite coverage runs clustered tightly below the historical constant.

Nondeterministic coverage measurement is a real problem: it makes any fixed cross-session coverage constant an unreliable gate and will keep producing false failures. This is the same root cause family as the load-dependent test flakiness tracked in issue **#511**, and has been recorded there.

**Recommended gate change (not made under #438):** compare coverage against a baseline captured in the same session and run, rather than against a constant carried across sessions.
