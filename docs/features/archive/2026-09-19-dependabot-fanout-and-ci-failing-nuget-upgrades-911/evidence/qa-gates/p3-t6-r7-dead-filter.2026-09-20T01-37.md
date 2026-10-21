# R7 — The Dead Binding-Redirect Filter Clause Is Removed

- Timestamp: 2026-09-20T08-59-39
- Task: [P3-T6]
- Finding: R7, Major, decision D2
- EXIT_CODE: 0

## The Rewritten Line, Verbatim

`.github/workflows/dependabot-repair.yml:94`:

```
          $beyondKnownWeak = @($kind | Where-Object { $_ -ne 'Analyzer' }).Count
```

It matches the required fragment `Where-Object { $_ -ne 'Analyzer' }` exactly. It replaces
`@($kind | Where-Object { $_ -ne 'Analyzer' -and $_ -ne 'BindingRedirect' }).Count`.

## The Comment, Verbatim

```
          # The binding-redirect class is not reachable from the workflow_run trigger: this step
          # invokes the repair entry point with no -CandidateUpgrade, so the applied-upgrade set
          # is always empty, the app.config reconciliation pass never runs, and the call site
          # keeps only the reconciled .Text and discards the Kind = 'BindingRedirect' record.
          # No record of that kind can reach this filter, so the clause excluding it is removed.
          # The decision is recorded in the AC14 note in spec.md and in the code review dated
          # 2026-09-20. If a later change supplies -CandidateUpgrade, the removed clause becomes
          # load-bearing again and must be restored.
```

## The Exact-One Count

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `BindingRedirect` occurrences in the file | exactly **1** | **1** | PASS |
| `Where-Object { $_ -ne 'Analyzer' }` occurrences | at least 1 | 1 | PASS |
| `not reachable from the workflow_run trigger` occurrences | at least 1 | 1 | PASS |

The single remaining occurrence is the one **inside the comment**, on the line naming the record
`Kind` the call site discards.

The exact-1 figure is the positive counterpart **gate rule 2** requires. A bare zero-count
assertion on `BindingRedirect` would also be satisfied by deleting the explanation along with the
clause, and the explanation is the point: it is what tells a later author who supplies
`-CandidateUpgrade` that the removed clause becomes load-bearing again.

A first form of the comment used the token `BindingRedirect` twice — once in prose and once
naming the record `Kind` — and measured 2. The prose mention was changed to the hyphenated
`binding-redirect`, which loses no meaning and leaves the exact token exactly where it names a
real identifier.

## The Named Assertion Is Now Green

```
PESTER Passed=1 Failed=0 Executed=1 Total=17
```

[P3-T1]'s `R7- counts beyond-known-weak repairs with the analyzer exclusion alone` failed before
this edit because the file contained neither the single-clause fragment nor the reachability
comment. It passes now.

## Gate Rule 20 — Verification Route and Residual

**Verified without a live run**, two ways:

- by the [P3-T1] `R7- counts` assertion, which was red before this edit and is green after it;
- by a **static reading of the call site**. The repair step invokes
  `Repair-PackageManifestConsistency.ps1` with no `-CandidateUpgrade`, so the parameter takes its
  `@{}` default and `$upgrade.Applied` is always empty. The `app.config` reconciliation block is
  gated on that set being non-empty, so it never executes. Separately, the call site keeps only
  `.Text` from the reconciliation result and discards the `Kind = 'BindingRedirect'` record. Two
  independent reasons, either sufficient: no record of that kind can reach the filter.

**Unverifiable until the #914 credential exists:** that no execution path under the
`workflow_run` trigger produces a `BindingRedirect` record. The static reading covers the
configured trigger **as authored**. It cannot cover a trigger or an invocation this cycle does
not write, and it observes no run: the workflow has never executed, and [P0-T13] recorded zero
Actions secrets and zero open pull requests, so it cannot be made to execute from this state.

## Decision D2, Recorded

R7 is discharged as **out of scope**, not made reachable.

The alternative the review offered — deriving the applied upgrade set from the Dependabot commit
and passing it — is new production behaviour with new untested paths, in a remediation cycle
whose purpose is to close a review. It was rejected on that ground.

What is **not** removed: the binding-redirect **write** path in the composition root. That path
is live and is exercised by the AC14 unit assertions in
`tests/scripts/dependencies/ProjectConsistency.Tests.ps1`. Only the workflow's filter clause is
removed, and only because no record can reach it.

The AC14 note in `spec.md` carries the matching statement, verified read-only at [P0-T4].

## Output Summary

The filter reads `Where-Object { $_ -ne 'Analyzer' }`. `BindingRedirect` appears exactly once in
the file, inside the eight-line comment recording the reachability decision, its two independent
static reasons, the two places the decision is written down, and the condition under which the
removed clause must be restored.
