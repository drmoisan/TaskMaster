# Baseline — `Directory.Build.props` Pre-Change Absence (Task [P0-T2])

Timestamp: 2026-09-02T22-11
Command: Test-Path Directory.Build.props
EXIT_CODE: 0
Output Summary: False

The literal boolean printed by `Test-Path Directory.Build.props`, executed with the
repository root of the item worktree as the working directory, is `False`. No
`Directory.Build.props` exists at the repository root before Phase 1 runs.

This establishes that the Phase 0 baseline rebuilds captured in [P0-T3] and [P0-T4]
are executed against a tree in which the file does not yet exist, so the
System.Reactive.PackagesConfigCheck warning counts they record are genuine
pre-suppression values.
