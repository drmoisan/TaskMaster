# Notification refresh fault: failing regression evidence

Timestamp: 2026-08-04T21:22:28-04:00

EXIT_CODE: 1
Output Summary: Expected-red compile result: the service had no `ScheduledRefreshFaulted` member, so the controlled scheduled-refresh fault-observation regression could not compile before the repair.

Command:

```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:DefineConstants=REMEDIATION_P1_T13 /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

Result: failed as expected before the scheduled-refresh fault-observation policy is implemented.

The controlled notification refresh test requires `OutlookFolderTreeService` to publish the original refresh exception through an instance-scoped `ScheduledRefreshFaulted` notification and verifies that the reader is called exactly twice: once for the initial snapshot and once for the failing scheduled refresh. The current implementation exposes no such observed-fault policy and assigns the scheduled task without observing its failure.

Relevant compiler assertion:

```text
CS1061: 'OutlookFolderTreeService' does not contain a definition for 'ScheduledRefreshFaulted'
```

The test is deterministic and uses only the existing fake notification sink and controlled reader. It does not use Outlook, a real UI, timers, polling, or a dispatcher fallback.
