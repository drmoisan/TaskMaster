# Protected Regions Unchanged (P2-T8)

Timestamp: 2026-10-01T23-57
Command: CMD-PROTECTED-SPANS with LEFT MERGE-BASE (59cbab04f1c854baa2a03b6cbf755c1df4f961b4) and SIGNATURES-PROTECTED, then SIGNATURES-EDITED; the P2-T8 protected-files payload (git ls-tree of the fixture partials at MERGE-BASE; git diff --exit-code MERGE-BASE over the partials, RibbonController.EngineCommands.cs, EngineTogglePressedStateCache.cs, TaskMaster.csproj and both packages.config; git diff --exit-code MERGE-BASE over the two run-settings files)
EXIT_CODE: 0
Output Summary: all twelve SIGNATURES-PROTECTED spans and the _primeTasks declaration hash equal to MERGE-BASE; both SIGNATURES-EDITED spans differ (positive control); PROTECTED-PARTIAL-COUNT 5 equals ANCHOR-PARTIAL-COUNT 5; PROTECTED_FILES_DIFF_EXIT=0; RUNSETTINGS_DIFF_EXIT=0.

Deviation recorded: the two CMD-PROTECTED-SPANS runs (SIGNATURES-PROTECTED and SIGNATURES-EDITED) were issued as one invocation whose SIGNATURES list is the concatenation of both lists; each row is computed independently, so the output is the same as two runs.

## SPAN-HASH rows (hash, then line range; MERGE-BASE on the left, working file on the right)

```
SPAN-HASH [internal EngineToggleStateCoordinator(] left=DE-A5-1B-58-50-6E-A0-3E-F5-54-72-81-82-3A-3D-8B-4D-96-24-9A-BE-09-8A-68-2E-26-0B-D3-94-BF-F2-BE 104-118 work=DE-A5-1B-58-50-6E-A0-3E-F5-54-72-81-82-3A-3D-8B-4D-96-24-9A-BE-09-8A-68-2E-26-0B-D3-94-BF-F2-BE 113-127 equal=True
SPAN-HASH [internal bool GetPressed(string engineName)] left=7F-79-C0-99-7C-FB-C8-CB-97-FB-2A-44-41-92-0D-6B-1D-2C-98-AD-25-3E-1B-E4-38-80-89-C6-96-A6-ED-E9 137-151 work=7F-79-C0-99-7C-FB-C8-CB-97-FB-2A-44-41-92-0D-6B-1D-2C-98-AD-25-3E-1B-E4-38-80-89-C6-96-A6-ED-E9 146-160 equal=True
SPAN-HASH [internal async Task HandleToggleClickAsync(string engineName)] left=BB-60-88-6D-20-62-D6-19-32-6A-88-98-18-3A-43-54-A3-CB-2E-BE-45-6A-72-E2-19-95-F4-C7-3C-54-EB-BA 173-196 work=BB-60-88-6D-20-62-D6-19-32-6A-88-98-18-3A-43-54-A3-CB-2E-BE-45-6A-72-E2-19-95-F4-C7-3C-54-EB-BA 182-205 equal=True
SPAN-HASH [internal async Task ExecuteToggleAsync(string engineName)] left=70-C5-42-79-7D-F3-2F-4B-15-D9-06-AC-3C-D9-2A-26-FE-62-27-AA-4D-75-89-A8-83-D3-38-94-6D-7C-EA-38 219-246 work=70-C5-42-79-7D-F3-2F-4B-15-D9-06-AC-3C-D9-2A-26-FE-62-27-AA-4D-75-89-A8-83-D3-38-94-6D-7C-EA-38 228-255 equal=True
SPAN-HASH [internal Task GetPrimeTask(string engineName)] left=7C-84-C4-4F-C7-5B-9B-A3-AB-4B-CE-25-1E-46-B5-F2-2D-E8-09-9C-F3-EA-21-19-46-4D-F3-15-E9-F9-03-26 261-269 work=7C-84-C4-4F-C7-5B-9B-A3-AB-4B-CE-25-1E-46-B5-F2-2D-E8-09-9C-F3-EA-21-19-46-4D-F3-15-E9-F9-03-26 270-278 equal=True
SPAN-HASH [private void StartPrimeIfNeeded(string engineName, string controlId)] left=B2-16-BC-A4-29-25-CE-20-8E-22-40-6E-5C-9E-07-09-4A-BC-68-AF-1F-2A-0D-F4-FE-8B-48-4D-DA-FD-68-DA 275-300 work=B2-16-BC-A4-29-25-CE-20-8E-22-40-6E-5C-9E-07-09-4A-BC-68-AF-1F-2A-0D-F4-FE-8B-48-4D-DA-FD-68-DA 284-309 equal=True
SPAN-HASH [private void StartObservedPrime(] left=0C-0D-0F-CF-C7-C0-A5-10-17-31-EA-FB-4F-64-A2-1B-6E-DD-30-78-1B-F1-00-2B-F0-C3-1A-CC-57-19-B3-69 316-340 work=0C-0D-0F-CF-C7-C0-A5-10-17-31-EA-FB-4F-64-A2-1B-6E-DD-30-78-1B-F1-00-2B-F0-C3-1A-CC-57-19-B3-69 325-349 equal=True
SPAN-HASH [private async Task ApplyPrimeAsync(] left=78-81-C7-71-0D-CB-F5-D4-79-11-E0-B1-66-BC-34-DD-BB-D2-FE-B8-2F-8B-52-C6-DB-88-D1-6A-8E-D7-A9-AE 347-362 work=78-81-C7-71-0D-CB-F5-D4-79-11-E0-B1-66-BC-34-DD-BB-D2-FE-B8-2F-8B-52-C6-DB-88-D1-6A-8E-D7-A9-AE 356-371 equal=True
SPAN-HASH [private static string RenderEngineName(string engineName)] left=65-C7-A8-8B-A9-96-65-00-A5-EE-2A-49-CE-76-9B-8D-A4-68-2B-9E-A7-D8-AA-5B-0C-E1-B4-0C-60-2C-29-4E 421-424 work=65-C7-A8-8B-A9-96-65-00-A5-EE-2A-49-CE-76-9B-8D-A4-68-2B-9E-A7-D8-AA-5B-0C-E1-B4-0C-60-2C-29-4E 440-443 equal=True
SPAN-HASH [private static string BuildUnavailableMessage(string engineName)] left=4F-34-77-2D-A9-15-E0-26-F2-15-95-27-9E-77-44-CF-D5-6B-D0-25-C4-1A-84-E3-07-B9-C7-32-2E-05-CC-04 429-437 work=4F-34-77-2D-A9-15-E0-26-F2-15-95-27-9E-77-44-CF-D5-6B-D0-25-C4-1A-84-E3-07-B9-C7-32-2E-05-CC-04 448-456 equal=True
SPAN-HASH [private static string BuildToggleFailedMessage(string engineName)] left=AC-4C-C1-83-2F-B0-77-A5-6D-68-3B-ED-F6-EC-78-F1-49-03-27-5E-D9-AB-68-BA-21-07-7D-78-2C-6D-09-BE 442-449 work=AC-4C-C1-83-2F-B0-77-A5-6D-68-3B-ED-F6-EC-78-F1-49-03-27-5E-D9-AB-68-BA-21-07-7D-78-2C-6D-09-BE 461-468 equal=True
SPAN-HASH [private static string BuildUnmappedKeyMessage(string engineName)] left=81-FF-41-81-59-B7-B7-21-E5-9B-3E-1A-46-66-58-AF-0E-FB-13-E9-33-39-9B-60-C1-8A-32-27-BE-7B-E7-33 467-474 work=81-FF-41-81-59-B7-B7-21-E5-9B-3E-1A-46-66-58-AF-0E-FB-13-E9-33-39-9B-60-C1-8A-32-27-BE-7B-E7-33 487-494 equal=True
SPAN-HASH [private void CompletePrime(Task completed, string engineName)] left=4B-6C-50-98-CC-C8-3F-EC-D4-B7-23-D5-D7-8D-95-FA-19-80-DA-E6-A5-E4-93-98-4A-46-44-0E-42-04-E9-E4 391-416 work=1D-B0-2D-43-90-51-44-CD-BE-4D-BD-CD-93-30-3D-88-12-88-86-8A-31-10-D4-59-06-15-4E-0E-02-F9-58-0F 405-435 equal=False
SPAN-HASH [private static string BuildPrimeFailedMessage(string engineName)] left=02-8E-7F-53-F5-E4-F2-1D-A8-C2-49-63-8E-C7-91-76-2B-93-0E-98-E3-A7-55-9B-13-FD-8D-DB-87-56-A6-50 454-462 work=53-4D-AF-46-82-B7-E3-95-02-D7-36-BC-05-53-27-71-7E-E0-34-B4-A9-46-42-1D-13-D3-C6-ED-A4-DC-CC-38 473-482 equal=False
SPAN-HASH [PRIMETASKS-DECLARATION] left=C8-E3-8D-08-43-39-59-CE-12-F3-E9-74-1F-3A-FF-7B-F6-36-9F-11-33-3E-35-69-DF-40-99-A7-83-80-E8-44 78-81 work=C8-E3-8D-08-43-39-59-CE-12-F3-E9-74-1F-3A-FF-7B-F6-36-9F-11-33-3E-35-69-DF-40-99-A7-83-80-E8-44 78-81 equal=True
```

## Protected files

```
PROTECTED-PARTIALS: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs
PROTECTED_FILES_DIFF_EXIT=0
RUNSETTINGS_DIFF_EXIT=0
PROTECTED-PARTIAL-COUNT: 5
```

## Acceptance

- every SIGNATURES-PROTECTED row and the PRIMETASKS-DECLARATION row print equal=True, neither side ABSENT or UNTERMINATED
- both SIGNATURES-EDITED rows print equal=False
- PROTECTED-PARTIAL-COUNT (5) equals ANCHOR-PARTIAL-COUNT (5) and the listed partials are every fixture partial at MERGE-BASE
- PROTECTED_FILES_DIFF_EXIT=0 (the AC-J identity observation, part of AC-M); RUNSETTINGS_DIFF_EXIT=0

## PRECOMMIT-FORMAT-RECHECK: (P2-T9, after the scoped CSharpier format)

Timestamp: 2026-10-02T00-01. Re-run on the formatted production file (hash fields abbreviated to line ranges; equality is computed on the hashes):

```
SPAN-HASH [internal EngineToggleStateCoordinator(] left=104-118 work=113-127 equal=True
SPAN-HASH [internal bool GetPressed(string engineName)] left=137-151 work=146-160 equal=True
SPAN-HASH [internal async Task HandleToggleClickAsync(string engineName)] left=173-196 work=182-205 equal=True
SPAN-HASH [internal async Task ExecuteToggleAsync(string engineName)] left=219-246 work=228-255 equal=True
SPAN-HASH [internal Task GetPrimeTask(string engineName)] left=261-269 work=270-278 equal=True
SPAN-HASH [private void StartPrimeIfNeeded(string engineName, string controlId)] left=275-300 work=284-309 equal=True
SPAN-HASH [private void StartObservedPrime(] left=316-340 work=325-349 equal=True
SPAN-HASH [private async Task ApplyPrimeAsync(] left=347-362 work=356-371 equal=True
SPAN-HASH [private static string RenderEngineName(string engineName)] left=421-424 work=440-443 equal=True
SPAN-HASH [private static string BuildUnavailableMessage(string engineName)] left=429-437 work=448-456 equal=True
SPAN-HASH [private static string BuildToggleFailedMessage(string engineName)] left=442-449 work=461-468 equal=True
SPAN-HASH [private static string BuildUnmappedKeyMessage(string engineName)] left=467-474 work=487-494 equal=True
SPAN-HASH [private void CompletePrime(Task completed, string engineName)] left=391-416 work=405-435 equal=False
SPAN-HASH [private static string BuildPrimeFailedMessage(string engineName)] left=454-462 work=473-482 equal=False
SPAN-HASH [PRIMETASKS-DECLARATION] left=78-81 work=78-81 equal=True
PROTECTED-PARTIALS: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs
PROTECTED_FILES_DIFF_EXIT=0
RUNSETTINGS_DIFF_EXIT=0
PROTECTED-PARTIAL-COUNT: 5
```

Every P2-T8 clause holds; no repair was needed.

## POST-FORMAT: (P3-T2, after the repository-wide format of P3-T1)

Timestamp: 2026-10-02T00-08. CMD-PROTECTED-SPANS (both signature lists) and the protected-files payload re-run on the post-format tree (hash fields abbreviated to line ranges; equality is computed on the hashes):

```
SPAN-HASH [internal EngineToggleStateCoordinator(] left=104-118 work=113-127 equal=True
SPAN-HASH [internal bool GetPressed(string engineName)] left=137-151 work=146-160 equal=True
SPAN-HASH [internal async Task HandleToggleClickAsync(string engineName)] left=173-196 work=182-205 equal=True
SPAN-HASH [internal async Task ExecuteToggleAsync(string engineName)] left=219-246 work=228-255 equal=True
SPAN-HASH [internal Task GetPrimeTask(string engineName)] left=261-269 work=270-278 equal=True
SPAN-HASH [private void StartPrimeIfNeeded(string engineName, string controlId)] left=275-300 work=284-309 equal=True
SPAN-HASH [private void StartObservedPrime(] left=316-340 work=325-349 equal=True
SPAN-HASH [private async Task ApplyPrimeAsync(] left=347-362 work=356-371 equal=True
SPAN-HASH [private static string RenderEngineName(string engineName)] left=421-424 work=440-443 equal=True
SPAN-HASH [private static string BuildUnavailableMessage(string engineName)] left=429-437 work=448-456 equal=True
SPAN-HASH [private static string BuildToggleFailedMessage(string engineName)] left=442-449 work=461-468 equal=True
SPAN-HASH [private static string BuildUnmappedKeyMessage(string engineName)] left=467-474 work=487-494 equal=True
SPAN-HASH [private void CompletePrime(Task completed, string engineName)] left=391-416 work=405-435 equal=False
SPAN-HASH [private static string BuildPrimeFailedMessage(string engineName)] left=454-462 work=473-482 equal=False
SPAN-HASH [PRIMETASKS-DECLARATION] left=78-81 work=78-81 equal=True
PROTECTED-PARTIALS: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs
PROTECTED_FILES_DIFF_EXIT=0
RUNSETTINGS_DIFF_EXIT=0
PROTECTED-PARTIAL-COUNT: 5
```

Every P2-T8 clause holds on the post-format tree: all twelve protected spans and the declaration equal; both edited spans differ; PROTECTED-PARTIAL-COUNT 5 equals ANCHOR-PARTIAL-COUNT; PROTECTED_FILES_DIFF_EXIT=0; RUNSETTINGS_DIFF_EXIT=0.
