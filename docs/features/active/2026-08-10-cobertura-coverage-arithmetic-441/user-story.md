# 2026-08-10-cobertura-coverage-arithmetic-441 (User Stories)

- Work Mode: full-bug
- **Issue:** #441 (also closes #478)
- **Epic:** build-ci-coverage-gate-fidelity (wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-10T15-05
- **Status:** Narrative context only

> **Not an acceptance-criteria source.** Under work mode `full-bug`,
> `.claude/skills/acceptance-criteria-tracking/SKILL.md` resolves `spec.md` as the sole
> authoritative acceptance-criteria source. This document deliberately contains no checkbox items
> and must not be tracked for check-off. All acceptance criteria live in
> `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Acceptance Criteria.

## Who the consumer is

The consumer of the corrected coverage figure is not an end user. It is the repository's own
quality-gate machinery — the Koverage VS Code extension, the coverage-delta artifacts committed as
feature evidence, the per-file rate gates in epic #136 — and the engineers who are asked to trust
what those gates report. The stories below are framed from that perspective.

## Stories

### S1 — Reviewing a coverage delta

**As an engineer reviewing a coverage delta,** I need the reported `lines-valid` to equal the
distinct source-line count for the packages in scope, **so that** the denominator I am reasoning
about is the code I am asking to be tested, and not an artifact of the Cobertura format repeating
each line under both `<method><lines>` and the class-level `<lines>` rollup.

**Value:** Today the repository-wide denominator is roughly 1.8x the real one, and the inflation is
not uniform across assemblies, so no two figures in a delta are comparable on a common basis. A
reviewer currently cannot tell whether a movement reflects a change in tests or a change in how much
of the assembly happens to sit inside methods.

### S2 — Certifying a per-file coverage gate

**As an engineer certifying a per-file coverage gate,** I need the emitted per-file `line-rate` to
be computed from one denominator rather than blended from two, **so that** the number I record as
evidence is reproducible from the document I recorded it against.

**Value:** For `QfcHomeController.Iteration.cs` the emitted rate is 0.8625 (69/80) while the
class-level union — the only defensible figure — is 0.803571 (45/56). Neither denominator, 69/80,
matches anything a reviewer can derive by inspection. Epic #136 gates fifteen child features on
exactly this attribute.

### S3 — Trusting a branch-coverage figure

**As an engineer reading a branch-coverage figure,** I need `branches-valid` and `branches-covered`
to be deduplicated on the same basis as lines, **so that** the report is internally consistent and I
am not reading a line denominator computed one way beside a branch denominator computed another.

**Value:** The branch accumulator sits physically inside the defective line loop, so branch counts
are inflated by the same duplication. The inflation is easy to miss because the *ratio* can survive
it unchanged — for the confirmed class, 8/12 and 12/18 are both 0.666667 while the counts differ by
50%. Anyone sizing remaining branch work from the raw counts is working from a number that is half
again too large.

### S4 — Relying on a committed baseline

**As an engineer relying on a committed coverage baseline,** I need the baseline to have been
captured by arithmetic that agrees with the instrumentation tool that produced the underlying data,
**so that** a later comparison measures my change rather than a change in counting method.

**Value:** A prior evidence artifact recorded a +15.5-point line-rate "improvement" and attributed
it to instrumentation variance. It was in fact almost entirely an artifact of comparing a raw
generator figure (79957, counted correctly) against a post-processed one (110849, counted twice).
Correcting the arithmetic makes baselines comparable and removes a class of false conclusions from
the evidence record.

### S5 — Extending the coverage tooling

**As an engineer extending the coverage post-processor,** I need the line-and-branch reduction to
exist as one named pure function with an explicit deduplication rule, **so that** the next change to
this module has a single place to reason about and cannot reintroduce the divergence between the
summary path and the merge path.

**Value:** The defect reaches the merged per-file rate indirectly, through a synthetic-document
delegation that routes one function's output through the other's document-level entry point. That
coupling is why a single wrong XPath axis produced two separately-filed issues. Replacing it with a
shared helper means the two call sites can no longer disagree.

## Related documents

- Acceptance criteria and full technical specification:
  `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md`
- Defect statement and scope boundaries:
  `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/issue.md`
- Verified analysis:
  `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/research/2026-08-10T14-20-cobertura-arithmetic-research.md`
- Epic charter: `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md`
