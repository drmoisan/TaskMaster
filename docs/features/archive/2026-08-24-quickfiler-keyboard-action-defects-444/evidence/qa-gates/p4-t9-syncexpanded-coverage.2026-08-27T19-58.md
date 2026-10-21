# [P4-T9] Coverage of the new member `SyncExpandedRegistrations`

Timestamp: 2026-08-27T19-58
Command: `@(Select-Xml -Path coverage\coverage.cobertura.final.xml -XPath "//method[@name='SyncExpandedRegistrations']")`
EXIT_CODE: 0
Output Summary: exactly one matching `<method>` node. Its `line-rate` is `1`, which is at or above
the required `0.90`.

## Matched nodes

| # | `name` | `signature` | `line-rate` | `branch-rate` |
| --- | --- | --- | --- | --- |
| 1 | `SyncExpandedRegistrations` | `(bool)` | **1** | 1 |

`MATCHES = 1`. The node lives under the `<class>` element whose `filename` is
`QuickFiler\Controllers\QfcItemController.Navigation.cs`, the file that declares the member.

A `line-rate` of `1` means every executable line of the member was reached by the test run. The
member is exercised directly by the Phase 3 interleaving regression test, and both of its
registration branches (the expanded-on and the expanded-off path) are reached, which is also why
`branch-rate` reads `1`.

## Acceptance

- At least one such node exists — met (1 node).
- Every matched node's `line-rate` is at or above `0.90` — met (the single node reads `1`).

This figure is the shared measurement for AC-QA-08 and for the deferred clause of AC-482-08; both
are checked off by `[P4-T20]`, discharging the `DEFERRED-TO-PHASE-4:` record written by `[P3-T27]`.
