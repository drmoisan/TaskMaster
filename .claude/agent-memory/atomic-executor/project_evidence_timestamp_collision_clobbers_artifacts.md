---
name: evidence-timestamp-collision-clobbers-artifacts
description: A remediation cycle's <TS> can collide with committed implementation-cycle evidence filenames and silently overwrite them; check git ls-files before writing
metadata:
  type: project
---

Evidence filenames are `<kind>/<name>.<TS>.md`. A **remediation cycle run on the same day** as the implementation cycle can resolve `<TS>` to a value that collides exactly with an already-committed artifact, and the Write tool will **silently overwrite** it — the loss is invisible until `git status --porcelain` shows the path as ` M` (tracked, modified) rather than `??` (untracked).

**Why:** In issue #503 remediation cycle 1, `<TS>` resolved to `2026-08-08T14-52`, which is exactly the timestamp of the committed implementation-cycle artifact `evidence/qa-gates/tests-with-coverage.2026-08-08T14-52.md` (its P6-T6 record). The remediation P3-T6 write destroyed it. It was only caught at the P3-T11 scope-lock audit, which classifies porcelain entries and noticed a ` M` under `evidence/` where every other cycle artifact was `??`.

**How to apply:**
- Before writing the first evidence artifact of a cycle, run `git ls-files '<FEATURE>/evidence'` and compare the planned filenames against the committed set. Same-day cycles are the high-risk case.
- On collision: restore the original with `git checkout -- <path>` (verify the content came back), then write the new record to a disambiguated name such as `<name>.remediation.<TS>.md`, and record the disambiguation in the artifact body — the plan's stated filename is not worth destroying prior evidence for.
- Treat any ` M` entry under `evidence/` in a scope-lock audit as a clobber until proven otherwise; cycle artifacts should be `??`.

See also [[project_preflight_mergebase_diff_gates_need_commit_cadence]].
