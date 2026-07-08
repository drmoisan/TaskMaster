# Runtime Capture PLACEHOLDER — AC10 Junk Folder Navigation (issue #211)

Timestamp: 2026-06-24T17-30

## MAINTAINER-GATED (runtime, not CI-automatable)

This capture is PENDING maintainer execution. It requires a live Outlook process and the
maintainer's configured stores, so it cannot be produced in CI or by the executor. The automated
portion of AC10 (path-bound navigation logic, red->green regression evidence, full toolchain pass)
is complete; this runtime confirmation is the remaining maintainer-gated item.

## Expected pass condition

After a non-debugger cold start on the branch head containing the JunkFolderPathNavigator fix:

- `[spam-init] ValidatePathsSet.JunkCertain` elapsed: well under the 5000 ms threshold
  (target single-digit-to-low-double-digit ms, comparable to the ~4.4 ms direct
  `DefaultStore.GetDefaultFolder` reference), versus the proven pre-fix ~50,172 ms.
- `[spam-init] ValidatePathsSet.JunkPotential` elapsed: well under the 5000 ms threshold.
- No full default-store tree enumeration during junk-folder resolution.

## How to perform and record

See: evidence/other/ac10-coldstart-junk-navigation-recapture-instructions-2026-06-24T17-30.md

When captured, replace this placeholder (or add a dated sibling) with the actual `[spam-init]` ms
values and the `[phase-net] phase=Engines` line, and set EXIT_CODE / pass status accordingly.

Status: PENDING MAINTAINER CAPTURE.
