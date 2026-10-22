# Zero-caller proof for the four legacy QfcDatamodel members (issue #968, task P4-T1)

Timestamp: 2026-10-03T03-03
Command: pwsh -NoProfile -Command '<CMD-LEGACY-CALLERS payload>' (the Command Reference macro executed verbatim with PREFIX expanded and WORKTREE substituted), run against the pre-change tree before any Phase 4 edit
Canonical command: CMD-LEGACY-CALLERS (primary content grep `Worker_RunWorkerCompleted|LoadRemainingEmailsToQueue\b|LoadRemainingEmailsToQueueAsync|Linked List Locking` over every *.cs outside packages, .claude, obj and bin; `\blog\b` over QuickFiler/Controllers/QfcDatamodel*.cs; the string-literal and reflection cross-check; the extension-unfiltered sweep; the IQfcDatamodel interface grep; the InternalsVisibleTo grep)
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- CS_FILES: 1707 (includes the new pin-count test file)
- QFCDATAMODEL_LINES: 495 (QFCDATAMODEL-LINES-BEFORE: 495, the AC28 before figure)
- PRIMARY_LINES: 25
- LOG_LINES: 3
- CROSS_LINES: 2
- SWEEP_FILES: 98 (of which five are .cs files)
- INTERFACE_LINES: 0
- INVOCATIONS: 0

## SWEEP-CS lines (exactly the five .cs files of fact 15)

- SWEEP-CS \QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs = 1
- SWEEP-CS \QuickFiler.Test\Controllers\QfcHomeControllerRunAsyncTests.cs = 2
- SWEEP-CS \QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs = 1
- SWEEP-CS \QuickFiler\Controllers\QfcDatamodel.cs = 15
- SWEEP-CS \QuickFiler\Controllers\QfcHomeController.cs = 4

## IVT lines (the four grants of fact 15; all four members are private, so no grant exposes them)

- IVT \QuickFiler\Controllers\QfcHighConfidencePreFilter.cs:11 :: [assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
- IVT \QuickFiler\Controllers\QfcHomeController.cs:15 :: [assembly: InternalsVisibleTo("QuickFiler.Test")]
- IVT \QuickFiler\Legacy\IAcceleratorCallbacks.cs:5 :: [assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
- IVT \QuickFiler\Properties\AssemblyInfo.cs:5 :: [assembly: InternalsVisibleTo("QuickFiler.Test")]

## Classification of every PRIMARY line

| Line | Text | Category |
|---|---|---|
| QfcDatamodel.cs:40 | `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` | METHOD-GROUP-ONE-ARG-OVERLOAD |
| QfcDatamodel.cs:52 | `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` | METHOD-GROUP-ONE-ARG-OVERLOAD |
| QfcDatamodel.cs:130 | `/// constructors, below) to the single-argument <see cref="LoadRemainingEmailsToQueueAsync(CancellationToken)"/>` | CREF-ONE-ARG-OVERLOAD |
| QfcDatamodel.cs:194 | `//worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);` | COMMENTED-OUT |
| QfcDatamodel.cs:209 | `//e.Result = await LoadRemainingEmailsToQueueAsync(bw, _token);` | COMMENTED-OUT |
| QfcDatamodel.cs:210 | `//e.Result = LoadRemainingEmailsToQueue(bw, _token);` | COMMENTED-OUT |
| QfcDatamodel.cs:246 | `private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)` | DECLARATION |
| QfcDatamodel.cs:335 | `private async Task<bool> LoadRemainingEmailsToQueueAsync(CancellationToken cancel)` | DECLARATION (the surviving one-argument overload; not removed) |
| QfcDatamodel.cs:363 | `//logger.Debug($"{nameof(LoadRemainingEmailsToQueue)} Task cancelled");` | COMMENTED-OUT |
| QfcDatamodel.cs:369 | `$"{nameof(LoadRemainingEmailsToQueue)} Error. \n {e.Message}\n{e.StackTrace}"` | NAMEOF-RETARGETED |
| QfcDatamodel.cs:378 | `private bool LoadRemainingEmailsToQueue(BackgroundWorker bw, CancellationToken token)` | DECLARATION |
| QfcDatamodel.cs:404 | `//logger.Debug($"{nameof(LoadRemainingEmailsToQueue)} Task cancelled");` | SELF-REFERENCE (inside the removed synchronous method's own body) |
| QfcDatamodel.cs:410 | `$"{nameof(LoadRemainingEmailsToQueue)} Error. \n {e.Message}\n{e.StackTrace}"` | SELF-REFERENCE (inside the removed synchronous method's own body) |
| QfcDatamodel.cs:418 | `private async Task<bool> LoadRemainingEmailsToQueueAsync(` | DECLARATION (the two-argument overload) |
| QfcDatamodel.cs:462 | `//logger.Debug($"{nameof(LoadRemainingEmailsToQueueAsync)} Task cancelled");` | SELF-REFERENCE (inside the removed two-argument overload's own body) |
| QfcDatamodel.cs:469 | `#region Linked List Locking` | REGION-DIRECTIVE |
| QfcDatamodel.cs:472 | `#endregion Linked List Locking` | REGION-DIRECTIVE |
| QfcHomeController.cs:92 | `_formViewer.Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;` | OTHER-TYPE-SAME-NAME |
| QfcHomeController.cs:132 | `_formViewer.Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;` | OTHER-TYPE-SAME-NAME |
| QfcHomeController.cs:344 | `private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)` | OTHER-TYPE-SAME-NAME |
| QfcHomeController.cs:379 | `worker.RunWorkerCompleted -= Worker_RunWorkerCompleted;` | OTHER-TYPE-SAME-NAME |
| QfcDatamodelLivenessTests.cs:104 | `/// <c>LoadRemainingEmailsToQueueAsync</c> is still producing. The dequeue gate's` | DOC-PROSE |
| QfcHomeControllerRunAsyncTests.cs:325 | `public async System.Threading.Tasks.Task Worker_RunWorkerCompleted_HandlesCompletionCorrectly()` | OTHER-TYPE-SAME-NAME (a test of QfcHomeController) |
| QfcHomeControllerRunAsyncTests.cs:376 | `"Worker_RunWorkerCompleted",` | OTHER-TYPE-SAME-NAME (the reflective GetMethod is invoked on `_controller`, a QfcHomeController) |
| QfcInitEmailQueueZeroBatchTests.cs:28 | `/// <c>LoadRemainingEmailsToQueueAsync</c>, which pops a live` | DOC-PROSE |

## Classification of every LOG line

| Line | Text | Category |
|---|---|---|
| QfcDatamodel.cs:109 | `private static readonly log4net.ILog log = log4net.LogManager.GetLogger(` | DECLARATION |
| QfcDatamodel.QueueProcessing.cs:71 | `/// raising the log level, which is what the 37-minute silent gap in the field report needed.` | DOC-PROSE |
| QfcDatamodel.QueueProcessing.cs:90 | `/// at delegate-construction time, which is exactly the crash the field log records after a` | DOC-PROSE |

## Classification of every CROSS line

| Line | Text | Category |
|---|---|---|
| QfcDatamodel.cs:130 | `/// constructors, below) to the single-argument <see cref="LoadRemainingEmailsToQueueAsync(CancellationToken)"/>` | CREF-ONE-ARG-OVERLOAD |
| QfcHomeControllerRunAsyncTests.cs:376 | `"Worker_RunWorkerCompleted",` | OTHER-TYPE-SAME-NAME |

No line is classified `INVOCATION`. INVOCATIONS: 0. Every test-file hit is DOC-PROSE (QfcDatamodelLivenessTests.cs:104, QfcInitEmailQueueZeroBatchTests.cs:28) or OTHER-TYPE-SAME-NAME (QfcHomeControllerRunAsyncTests.cs:325 and 376), so no test references any of the four members.

## Numeric Derivation Evidence

- Complete Family: log, Worker_RunWorkerCompleted, LoadRemainingEmailsToQueue, LoadRemainingEmailsToQueueAsync-BackgroundWorker-overload
- Exhaustive Search Scope: every *.cs file in the item worktree outside packages, .claude, obj and bin (CS_FILES: 1707), plus an extension-unfiltered sweep of every file outside packages, .claude, obj, bin, .git, coverage and .dotnet-sdk for the method names (SWEEP_FILES: 98), plus QuickFiler/Interfaces/IQfcDatamodel.cs, both QfcDatamodel partial siblings, and the InternalsVisibleTo grants under QuickFiler/
- Inclusion Rules: a member is counted as caller-free when every occurrence of its name outside its own declaration and body is a comment, a commented-out statement, a doc-comment reference, a reference to a different type's member of the same name, or a nameof symbol reference that is not an invocation and that the same edit retargets
- Exclusion Rules: members that implement an IQfcDatamodel interface member, members with any live invocation, event subscription, reflection-by-string lookup against QfcDatamodel, override, or designer wiring are excluded from the caller-free set; the one-argument LoadRemainingEmailsToQueueAsync(CancellationToken) overload is excluded because the constructors assign it to RemainingEmailLoader (lines 40 and 52, method-group conversions that bind the one-argument overload only)
- Primary Search Strategy or Query Expression: content grep `Worker_RunWorkerCompleted|LoadRemainingEmailsToQueue\b|LoadRemainingEmailsToQueueAsync|Linked List Locking` (case-sensitive) over every *.cs in scope, plus `\blog\b` over QuickFiler/Controllers/QfcDatamodel*.cs, each hit classified above
- Primary Member Set: log, Worker_RunWorkerCompleted, LoadRemainingEmailsToQueue, LoadRemainingEmailsToQueueAsync-BackgroundWorker-overload
- Primary Count: 4
- Cross-check Search Strategy or Query Expression: the string-literal and reflection sweep `"Worker_RunWorkerCompleted"|"LoadRemainingEmailsToQueue|"log"|nameof\(log\)|GetField\("log` over every *.cs in scope (2 lines, classified above), the extension-unfiltered sweep `Worker_RunWorkerCompleted|LoadRemainingEmailsToQueue` (five .cs files, each matching fact 15), the IQfcDatamodel grep (0 lines) and the InternalsVisibleTo grep (four grants; all four members are private)
- Cross-check Member Set: log, Worker_RunWorkerCompleted, LoadRemainingEmailsToQueue, LoadRemainingEmailsToQueueAsync-BackgroundWorker-overload
- Cross-check Count: 4
- Member-set Comparison: identical (the same four names in the same sense; both counts are 4)

This is unreachable dead code with no behaviour to regress, so no failing test precedes its removal; the compile proof is the two rebuilds (P8-T3 and P8-T4), with the P4-T10 build as the first compile check.
