---
name: committed-cobertura-baselines
description: Measured per-file/per-line C# coverage is already committed under docs/features/active|archive/*/evidence/{baseline,qa-gates}/*.cobertura.xml — read it instead of inferring coverage from test files
metadata:
  type: reference
---

Recent feature folders commit full Cobertura reports as evidence. The most recent ones as of 2026-08-07 are
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
and `docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/qa-gates/remediation-cycle4-coverage-final.cobertura.xml`.

**How to use for coverage research:** `Grep` for `filename="Project\\Path\\File.cs"` to find the `<class>`
element, then `Read` the `<methods>` and `<lines>` blocks. Before trusting it, verify the line numbers in the
hit map still align with the current source (spot-check 3-4 members). If `git log` shows only docs commits
since the report, it is current. This converts "derive coverage by reading tests" into measured evidence.

**Two gotchas:**
- The `<class line-rate=...>` attribute does NOT reconcile with a hand count of the `<lines>` union. The
  `<methods>` list omits async state-machine methods (`CreateAsync`, `*Async`) while `<lines>` includes their
  source lines. Report both figures and say which denominator each uses; do not silently pick one.
- Async methods therefore have no per-method entry — find their coverage only in the class-level `<lines>` map.

Related: [[qfc-item-controller-227-r2-denial]], [[feedback-exemption-audit-check-proven-techniques]].
