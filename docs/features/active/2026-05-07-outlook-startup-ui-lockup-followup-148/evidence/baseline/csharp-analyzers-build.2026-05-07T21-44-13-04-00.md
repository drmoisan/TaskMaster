# Baseline C# Analyzer Build Evidence

Timestamp: 2026-05-07T21:44:13.7035277-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild
EXIT_CODE: 0
Output Summary:
- The analyzer-enabled build completed successfully with `Build succeeded.`
- Final build summary reported `26 Warning(s)` and `0 Error(s)`.
- The warnings were concentrated in `UtilitiesCS.Test` and included repeated `CS8632` nullable-annotation-context warnings in existing test files such as `OlTableExtensions_Tests.cs`, `AsyncSerialization_Tests.cs`, `DfDeedle_COM_Tests.cs`, `ProgressTracker_Tests.cs`, `ProgressTrackerAsync_Tests.cs`, `MailItemHelperCoreTests.cs`, and `ConversationHelper_ExtendedTests.cs`.
- The build also reported existing `CS0067` unused-event warnings in `StoreWrapperControllerTests.cs`, `SmartSerializable_Tests.cs`, and `SmartSerializableBase_Tests.cs`.
- Source Transcript: c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\a8da1f0c9dae597edcdc167a5b8b2c63\GitHub.copilot-chat\chat-session-resources\d9a71451-4603-4e69-8bbf-7ccf9627280e\call_m9hVj4XliRdiFZAKRYgYWnMP__vscode-1778175287991\content.txt
