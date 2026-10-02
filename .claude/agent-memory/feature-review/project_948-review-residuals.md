---
name: 948-review-residuals
description: '#948 (repeat prime-failure report suppression in EngineToggleStateCoordinator) full-bug review 2026-10-02T04-30 PASS 16/16 AC, 0 blocking, 2 non-blocking; lock-free check-then-record proved safe by marker serialisation; record-after-sink inside the #947 try; Cobertura epoch 63 min behind labels; no-Bash review mechanics'
metadata:
  type: project
---

Full-bug review (parallel cohort bugs-2026-09-28, nested worktree `agent-a41eaaa29afcc2884`, head `601f06174`, merge base
`59cbab04f` = origin/main after the orchestrator's reconciliation merge `f96aab71e`): PASS, 16/16 AC, 0 blocking, 2 non-blocking
(CR-1 stale Race-partial remark, CR-5 evidence labels not clock-derived), 4 observations, 7 follow-ups. Caller forbade Bash
entirely (a `git -C` hang had stalled an earlier review) and supplied the production diff; everything was verified with
Read/Grep/Glob against the worktree plus the committed footprint/anchor evidence. Advertised the 3-`..` hook traversal path
from the session cwd as at [[956-review-residuals]]; plain and absolute paths in prose; no mirror written.

**Reusable verification points:**
- Lock-free `ContainsKey` -> sink -> indexer-set in `CompletePrime` is race-free per key because the next prime for that key
  cannot register until the `TryRemove` that is the LAST statement has run; the argument lives in spec.md but not in the code
  comment (recorded as CR-3). Check `REMOVE-IS-LAST` style evidence plus a Read of statement order rather than trusting prose.
- Record-after-sink placement inside a pre-existing guarded try: a throwing sink skips the record, so the report stays owed and
  a persistently throwing sink is re-invoked on every re-prime (no log volume, intended). The #947 "later read starts new
  prime" tests still hold because their second prime SUCCEEDS; `ContainSingle` is not weakened.
- Glob does not list git-ignored files (`artifacts/csharp/*` returned nothing) but Grep/Read on the exact path work; use Grep
  `filename="[^"]*<File>\.cs"` to find the Cobertura `<class>` line, then Read ~300 lines from it for per-line `hits`.
- Cobertura root `timestamp=` epoch (1790911441 = 2026-10-02T03:24:01Z) sat ~63 min BEHIND the artifact label (00-27 local,
  UTC-4): labels monotone and mutually consistent but synthetic; recorded non-blocking with a "derive from clock" follow-up.
- Collector writes binary `hits` (max 1); a plan clause "guard hits > record hits" is unsatisfiable. The executor's v0.6 fix
  (guard line `condition-coverage` 2/2 + record hits >= 1) is the right proof; confirm the negative control (a 1/2 line exists).
- Spec "no digit in any AC line" convention + `## Acceptance Criteria` with sixteen `- [x] AC-X.` lines; the executor's
  `evidence/other/ac-status-summary.md` carries one `AC-X: MET` line each, which made per-criterion verification quick.

**Follow-ups owed to the orchestrator:** F-1 fix Race partial remark (lines 196-201); F-2 split coordinator (496/500) and primary
fixture (470/500) before the next change; F-3 add the serialisation "why" comment; F-4 clock-derived evidence `Timestamp:`
labels; F-5 guard `_notifyUnavailable` (#947 F-1 still open at line 186); F-6 canonical C# coverage artifact path (recurring);
F-7 `quality-tiers.yml` absent (pre-existing, promoted by #956).
