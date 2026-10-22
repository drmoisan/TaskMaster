# Fusion Binding Log Measurement and Status

Recorded by `[P6-T3]`.

Timestamp: 2026-09-14T12-59

Procedure:

Enable Fusion assembly-binding logging under the registry key
`HKLM\SOFTWARE\Microsoft\Fusion`, setting the values `EnableLog=1` and `ForceLog=1`. Then
reproduce the defect and read the resulting binding log for the `netstandard` identity.

Reproduction steps:

1. Start a fresh Outlook session with the rebuilt add-in registered.
2. Open no SVG-bearing surface first. In particular open no `MyBox` dialog, no config viewer and
   no folder-not-found dialog, and start no prior QuickFiler session. Any of these can install an
   unrelated resolve handler first and mask the defect.
3. Click the QuickFiler ribbon button.

Executor Constraint:

The executor performs no `HKLM` registry change and starts no Outlook session. Writing to
`HKLM\SOFTWARE\Microsoft\Fusion` is a machine-wide privileged change, and starting Outlook is
outside the set of actions an automated executor performs in this repository. This measurement
is therefore a maintainer gate, not an executor step. No result is simulated, predicted or
inferred here.

RESULT: PENDING-MAINTAINER

Blocking: NO - the fix does not wait on this measurement

The reason the measurement is non-blocking is that ladder rung 3 loads the facade from the
runtime directory by absolute path, using `RuntimeEnvironment.GetRuntimeDirectory` joined with
`netstandard.dll` and passed to a load-from-path call. That rung bypasses assembly-cache lookup
entirely. Whatever the Fusion log would show about cache probing, it cannot change the outcome
of a load that never consults the cache, so the remedy is robust to the answer and does not
depend on obtaining it.

The measurement remains worth taking because it would explain the unexplained `2.0.0.0` frame
recorded at `evidence/other/netstandard-2-0-0-0-open-risk.2026-09-13T18-22.md`. That is a
diagnostic gain, not a precondition for the fix.
