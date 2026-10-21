# Pre-change census of the five code files (P0-T12)

Timestamp: 2026-10-02T00-51
Command: one pwsh -NoProfile -Command payload: PREFIX (Set-Location -LiteralPath "WORKTREE"; SetCurrentDirectory; WORKTREE-LEAF echo); then CMD-LINECOUNT and CMD-HASH over CODE5; CMD-TIMESPAN over THREE; CMD-TOKEN-COUNT once per code file (bodies verbatim, wrapped in a local function `Tok FILE TOKENS`); CMD-SPAN-TOKEN-COUNT for INITQ, R4SPAN, R4PRE, R4HEAD and R4TAIL (body verbatim, wrapped in a local function `Span LABEL FILE START END TOKENS`; a missing anchor sets a flag and the payload exits 4 at the end instead of mid-payload). Token lists exactly as plan P0-T12.
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4

## CMD-LINECOUNT
LINES QuickFiler\Controllers\QfcDatamodel.cs = 483
LINES QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs = 255
LINES QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs = 235
LINES QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs = 212
LINES QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 458

## CMD-HASH
BASE-HASH: QuickFiler\Controllers\QfcDatamodel.cs = 0C8F7E7DDEB0F1E52843DF18244A8E792CD6127B419737839F748976EEB12B94
BASE-HASH: QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs = 390B28D669815DE30C1D0724938FA2E58C889F18AF8DFF62090DD5F2FACA2FDF
BASE-HASH: QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs = 44A1952093125CB42891B01DC7FAD1A4C2B379E88D34082DFE040DD6BC74FAE6
BASE-HASH: QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs = 4F350522F071BBF1B9890D570DC143027C979CA2CFC051054DE681E8A55EBB07
BASE-HASH: QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 40EE455C7D2ACF6ADA5D01B8880B37A7BCBAF320CEC9F83A8B497015EAE56F52

## CMD-TIMESPAN
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:56 UNCLASSIFIED :: SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5)).Should().BeTrue(because);
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:103 UNCLASSIFIED :: .Task.Wait(TimeSpan.FromSeconds(5))
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:113 CLASSIFIED :: fake.Advance(TimeSpan.FromMilliseconds(200));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:115 CLASSIFIED :: fake.Advance(TimeSpan.FromMilliseconds(200));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:130 CLASSIFIED :: fake.Advance(TimeSpan.FromMilliseconds(200));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:173 UNCLASSIFIED :: .Task.Wait(TimeSpan.FromSeconds(5))
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs:67 UNCLASSIFIED :: SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5)).Should().BeTrue(because);
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs:118 CLASSIFIED :: Task pending = model.QuiesceLoaderAsync(TimeSpan.FromSeconds(5));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs:149 CLASSIFIED :: Task pending = model.QuiesceLoaderAsync(TimeSpan.FromSeconds(5));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs:154 CLASSIFIED :: fake.Advance(TimeSpan.FromSeconds(6));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs:220 UNCLASSIFIED :: .Task.Wait(TimeSpan.FromSeconds(5))
TIMESPAN QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs:161 UNCLASSIFIED :: .Task.Wait(TimeSpan.FromSeconds(5))
TIMESPAN-UNCLASSIFIED: 6

## CMD-TOKEN-COUNT (THREE; tokens SpinWait, .Wait(, WaitForState, new BackgroundWorker(), new SynchronousBackgroundWorker(), WorkerStarter = StartSynchronously;, IsBusy, starts a real)
Liveness: 1, 2, 5, 2, 0, 0, 6, 0
Teardown: 1, 1, 2, 1, 0, 0, 0, 0
Zero-batch: 0, 1, 0, 3, 0, 0, 2, 1

## CMD-TOKEN-COUNT QfcDatamodel.cs
TOKEN [WorkerStarter] = 0
TOKEN [RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;] = 2
TRIMMED-EQUAL [worker.RunWorkerAsync();] = 2

## CMD-TOKEN-COUNT R4 file
TOKEN [flake-watch] = 1
TOKEN [Append an observation] = 1
TOKEN [Issue #950:] = 0
TOKEN [[Timeout(GateTimeoutMs)]] = 8
TOKEN [private const int GateTimeoutMs = 60000;] = 1

## CMD-SPAN-TOKEN-COUNT
INITQ: SPAN: 259-304; RunWorkerAsync 2; WorkerStarter(worker); 0
R4SPAN: SPAN: 206-272; IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher() 0; EnsureUiThreadDispatcher() 0; using ( 1; .BeSameAs( 1; .NotBeSameAs( 1
R4PRE: SPAN: 206-212; EnsureUiThreadDispatcher() 0; using ( 0
R4HEAD: SPAN: 206-214; EnsureUiThreadDispatcher() 0; using ( 0
R4TAIL: SPAN: 256-261; } 2

Verdict: every value equals the plan's expected pre-change value (facts 1 to 5). No CENSUS MISMATCH. The non-zero wall-clock counts are the positive control for the P6-T8 zero gates.
