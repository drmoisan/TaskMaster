Timestamp: 2026-08-04T20:52:00-04:00
Command: N/A — historical source-backed requirements-to-evidence mapping; no command was run for this mapping artifact.
EXIT_CODE: N/A — no command was run.
Output Summary: This historical mapping kept AC1-AC6 and CR-001-CR-006 PASS and deferred AC7, AC8, and CR-007; it is superseded by the cycle-3/pass-3 controlling mapping.
P5-T1 evidence: remediation-targeted-regressions-pass.2026-08-04T20-52.md
Functional reconciliation: AC1 through AC6 and CR-001 through CR-006 remain PASS under the named 69-test targeted execution. The added controller assertion `CreateAsync_SnapshotFault_WhenViewerRequiresInvoke_ClosesOnViewerContext` proves snapshot-failure cleanup closes through the viewer context while preserving the original snapshot exception. AC3 no-fallback evidence remains the complete changed-production C# diff inspection and WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict. AC7/CR-007 remain pending the restarted Phase 6 coverage run and comparable-scope analysis; AC8 remains pending Phase 7 documentation reconciliation.
