# Post-change call-site census (issue #968, tasks P7-T1 and P7-T2)

Timestamp: 2026-10-03T03-24
Command: pwsh -NoProfile -Command '<CMD-CENSUS payload>' (the Command Reference macro executed verbatim with PREFIX expanded and WORKTREE substituted)
Canonical command: CMD-CENSUS (primary strategy: content grep `EnsureUiThreadDispatcher\(\)|EnsureDispatcher\(\)`; cross-check strategy: grep of the bare identifiers `EnsureUiThreadDispatcher|EnsureDispatcher`; positive control `BeginTransactionAsync\(`; over every *.cs outside packages, .claude, obj and bin)
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- CS_FILES: 1709
- PRIMARY_LINES: 16 (PRIMARY-FILE fixture 1, test support 2, fixture tests 3, pin-count tests 10; no focus-and-theme entry)
- CROSS_LINES: 32 (CROSS-FILE fixture 5, test support 3, InitializationTests.Part2 1, fixture tests 8, pin-count tests 15; no focus-and-theme entry)
- CONTROL_LINES: 28
- MEMBER-SET-COMPARISON: AGREE

## PRIMARY lines and classification

| Line | Text | Class |
|---|---|---|
| TestSupport.cs:240 | `internal static IDisposable EnsureUiThreadDispatcher() =>` | DECLARATION |
| TestSupport.cs:241 | `UiThreadDispatcherFixture.EnsureDispatcher();` | FORWARDER |
| UiThreadDispatcherFixture.cs:143 | `internal static IDisposable EnsureDispatcher()` | DECLARATION |
| UiThreadDispatcherFixtureTests.cs:60 | `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (R1) |
| UiThreadDispatcherFixtureTests.cs:119 | `IDisposable ensureScope = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (R2) |
| UiThreadDispatcherFixtureTests.cs:166 | `IDisposable ensureScope = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (R3) |
| UiThreadDispatcherPinCountTests.cs:45 | `IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (test 1) |
| UiThreadDispatcherPinCountTests.cs:46 | `IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (test 1) |
| UiThreadDispatcherPinCountTests.cs:93 | `IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (test 2) |
| UiThreadDispatcherPinCountTests.cs:94 | `IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (test 2) |
| UiThreadDispatcherPinCountTests.cs:146 | `IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (test 3) |
| UiThreadDispatcherPinCountTests.cs:147 | `IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (test 3) |
| UiThreadDispatcherPinCountTests.cs:194 | `IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (test 4) |
| UiThreadDispatcherPinCountTests.cs:195 | `IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (test 4) |
| UiThreadDispatcherPinCountTests.cs:201 | `IDisposable freshPin = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (test 4) |
| UiThreadDispatcherPinCountTests.cs:230 | `IDisposable foreignPin = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` | INVOCATION (test 4) |

Thirteen test-side INVOCATION lines (three in the fixture tests, ten in the pin-count tests), one FORWARDER and two DECLARATION lines.

## CROSS lines not in the PRIMARY set (sixteen, all non-executable)

| Line | Text | Class |
|---|---|---|
| UiThreadDispatcherFixture.cs:26 | `/// <see cref="EnsureDispatcher"/> deliberately never acquires <c>TransactionGate</c>. Callers of` | DOC |
| UiThreadDispatcherFixture.cs:27 | `/// the <c>QfcItemControllerTestSupport.EnsureUiThreadDispatcher</c> wrapper live in test files` | DOC |
| UiThreadDispatcherFixture.cs:217 | `/// <c>QfcItemControllerTestSupport.EnsureUiThreadDispatcher</c>.` | DOC |
| UiThreadDispatcherFixture.cs:266 | `/// The scope returned by <see cref="EnsureDispatcher"/>: one counted pin. Disposal is` | DOC |
| TestSupport.cs:218 | `/// <see cref="UiThreadDispatcherFixture.EnsureDispatcher"/> (issue #968): the first pin on a` | DOC |
| InitializationTests.Part2.cs:124 | `// from QfcItemControllerTestSupport.EnsureUiThreadDispatcher. Neither case can carry` | COMMENT |
| UiThreadDispatcherFixtureTests.cs:44 | `public async Task EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt()` | TEST-NAME |
| UiThreadDispatcherFixtureTests.cs:70 | `because: "EnsureDispatcher installs only when the field is null, so a live "` | DOC (a `because` string literal; names the method, invokes nothing) |
| UiThreadDispatcherFixtureTests.cs:107 | `public async Task EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose()` | TEST-NAME |
| UiThreadDispatcherFixtureTests.cs:128 | `because: "EnsureDispatcher seeds the parked dispatcher when the field is null"` | DOC (a `because` string literal; names the method, invokes nothing) |
| UiThreadDispatcherFixtureTests.cs:157 | `public async Task EnsureDispatcher_ScopeDisposedTwice_IsIdempotent()` | TEST-NAME |
| UiThreadDispatcherPinCountTests.cs:36 | `public async Task EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease()` | TEST-NAME |
| UiThreadDispatcherPinCountTests.cs:84 | `public async Task EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome()` | TEST-NAME |
| UiThreadDispatcherPinCountTests.cs:127 | `/// EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt. While a` | DOC |
| UiThreadDispatcherPinCountTests.cs:134 | `public async Task EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher()` | TEST-NAME |
| UiThreadDispatcherPinCountTests.cs:184 | `public async Task EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores()` | TEST-NAME |

Per file: fixture 4, test support 1, Part2 1, fixture tests 5, pin-count tests 5 (sixteen). The CROSS set contains every PRIMARY line (sixteen PRIMARY lines plus sixteen CROSS-only lines make the thirty-two CROSS lines) and no CROSS-only line is an invocation (no call written across two lines). MEMBER-SET-COMPARISON: AGREE.

## P7-T2 Nesting classification (CMD-PIN-NESTING)

Timestamp: 2026-10-03T03-25
Commands: eight separate payloads, pwsh -NoProfile -Command '<CMD-PIN-NESTING payload>' with (FILE, START, END) = R1SPAN, R2SPAN, R3SPAN, R4SPAN (FT) and T1SPAN, T2SPAN, T3SPAN, T4SPAN (PC), each the Command Reference macro executed verbatim with PREFIX expanded and WORKTREE substituted; every payload exited 0 and printed `WORKTREE-LEAF: agent-a291a7fbabf9d0229`.

R1SPAN (SPAN: 44-106):
- NEST 51 [BeginTransactionAsync()] .BeginTransactionAsync()
- NEST 56 [.Install(] transaction.Install(liveA);
- NEST 60 [EnsureUiThreadDispatcher()] QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 62 [Dispose()] ensureScope.Dispose();
- NEST 81 [Dispose()] transaction.Dispose();
- NEST 89 [finally] finally
- NEST 91 [Dispose()] transaction.Dispose();
- NEST 94 [finally] finally
- NEST 96 [ShutdownDispatcher(] QfcItemControllerTestSupport.ShutdownDispatcher(liveA);
- Reading: BeginTransactionAsync 51, Install 56, pin acquired 60, pin first Dispose 62, transaction first Dispose 81. NESTED: YES; INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE.

R2SPAN (SPAN: 107-156):
- NEST 107 [Dispose()] public async Task EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose() (the method signature: its name ends in `OnDispose()`, so the token matches; it is not a Dispose call)
- NEST 111 [BeginTransactionAsync()] .BeginTransactionAsync()
- NEST 116 [.Install(] transaction.Install(null);
- NEST 119 [EnsureUiThreadDispatcher()] IDisposable ensureScope = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 121 [Dispose()] ensureScope.Dispose();
- NEST 137 [Dispose()] transaction.Dispose();
- NEST 145 [finally] finally
- NEST 147 [Dispose()] transaction.Dispose();
- Reading: BeginTransactionAsync 111, Install 116, pin acquired 119, pin first Dispose 121, transaction first Dispose 137. NESTED: YES; INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE.

R3SPAN (SPAN: 157-211):
- NEST 161 [BeginTransactionAsync()] .BeginTransactionAsync()
- NEST 165 [.Install(] transaction.Install(null);
- NEST 166 [EnsureUiThreadDispatcher()] IDisposable ensureScope = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 169 [Dispose()] ensureScope.Dispose();
- NEST 171 [Dispose()] Action secondDispose = () => ensureScope.Dispose();
- NEST 186 [finally] finally
- NEST 188 [Dispose()] transaction.Dispose();
- Reading: BeginTransactionAsync 161, Install 165, pin acquired 166, pin first Dispose 169, transaction first Dispose 188. NESTED: YES; INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE.

R4SPAN (SPAN: 212-286):
- NEST 219 [BeginTransactionAsync()] .BeginTransactionAsync()
- NEST 224 [.Install(] transactionA.Install(liveA);
- NEST 235 [BeginTransactionAsync()] .BeginTransactionAsync()
- NEST 241 [finally] finally
- NEST 243 [Dispose()] transactionB.Dispose();
- NEST 249 [Dispose()] transactionA.Dispose();
- NEST 269 [finally] finally
- NEST 271 [Dispose()] transactionA.Dispose();
- NEST 274 [finally] finally
- NEST 276 [ShutdownDispatcher(] QfcItemControllerTestSupport.ShutdownDispatcher(liveA);
- Reading: no EnsureUiThreadDispatcher() line; two transactionA.Dispose(); lines (249 and 271), the second inside the finally at 269. No pin in R4.

T1SPAN (SPAN: 36-83):
- NEST 40 [BeginTransactionAsync()] .BeginTransactionAsync()
- NEST 44 [.Install(] transaction.Install(null);
- NEST 45 [EnsureUiThreadDispatcher()] IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 46 [EnsureUiThreadDispatcher()] IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 50 [Dispose()] pinA.Dispose();
- NEST 52 [Dispose()] pinB.Dispose();
- NEST 71 [finally] finally
- NEST 73 [Dispose()] transaction.Dispose();
- Reading: BeginTransactionAsync 40, Install 44, pinA 45 then its Dispose 50, pinB 46 then its Dispose 52, transaction first Dispose 73. NESTED: YES; INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE.

T2SPAN (SPAN: 84-133):
- NEST 88 [BeginTransactionAsync()] .BeginTransactionAsync()
- NEST 92 [.Install(] transaction.Install(null);
- NEST 93 [EnsureUiThreadDispatcher()] IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 94 [EnsureUiThreadDispatcher()] IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 98 [Dispose()] pinB.Dispose();
- NEST 100 [Dispose()] pinA.Dispose();
- NEST 119 [finally] finally
- NEST 121 [Dispose()] transaction.Dispose();
- NEST 130 [finally] /// fixture did not seed the field. The live dispatcher is shut down in a finally block. (a doc-comment line of the next test, matched by the `finally` token; not code)
- Reading: BeginTransactionAsync 88, Install 92, pinA 93 then its Dispose 100, pinB 94 then its Dispose 98, transaction first Dispose 121. NESTED: YES; INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE.

T3SPAN (SPAN: 134-183):
- NEST 141 [BeginTransactionAsync()] .BeginTransactionAsync()
- NEST 145 [.Install(] transaction.Install(live);
- NEST 146 [EnsureUiThreadDispatcher()] IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 147 [EnsureUiThreadDispatcher()] IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 150 [Dispose()] pinA.Dispose();
- NEST 151 [Dispose()] pinB.Dispose();
- NEST 163 [finally] finally
- NEST 165 [Dispose()] transaction.Dispose();
- NEST 168 [finally] finally
- NEST 170 [ShutdownDispatcher(] QfcItemControllerTestSupport.ShutdownDispatcher(live);
- Reading: BeginTransactionAsync 141, Install 145, pinA 146 then its Dispose 150, pinB 147 then its Dispose 151, transaction first Dispose 165. NESTED: YES; INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE.

T4SPAN (SPAN: 184-248):
- NEST 189 [BeginTransactionAsync()] .BeginTransactionAsync()
- NEST 193 [.Install(] transaction.Install(null);
- NEST 194 [EnsureUiThreadDispatcher()] IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 195 [EnsureUiThreadDispatcher()] IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 197 [Dispose()] pinA.Dispose();
- NEST 198 [Dispose()] pinB.Dispose();
- NEST 201 [EnsureUiThreadDispatcher()] IDisposable freshPin = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 203 [Dispose()] freshPin.Dispose();
- NEST 218 [finally] finally
- NEST 220 [Dispose()] transaction.Dispose();
- NEST 225 [BeginTransactionAsync()] .BeginTransactionAsync()
- NEST 229 [.Install(] foreignTransaction.Install(parked);
- NEST 230 [EnsureUiThreadDispatcher()] IDisposable foreignPin = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 231 [Dispose()] foreignPin.Dispose();
- NEST 242 [finally] finally
- NEST 244 [Dispose()] foreignTransaction.Dispose();
- Reading: transaction: BeginTransactionAsync 189, Install 193, pinA 194 then Dispose 197, pinB 195 then Dispose 198, freshPin 201 then Dispose 203, transaction first Dispose 220. foreignTransaction: BeginTransactionAsync 225 (after the transaction.Dispose(); line 220), Install 229, foreignPin 230 then Dispose 231, foreignTransaction first Dispose 244. NESTED: YES for both transactions; INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE.

Per method: R1 NESTED: YES, INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE; R2 NESTED: YES, NONE; R3 NESTED: YES, NONE; T1 NESTED: YES, NONE; T2 NESTED: YES, NONE; T3 NESTED: YES, NONE; T4 NESTED: YES, NONE. R4 carries no pin.

INVOCATIONS-CLASSIFIED: 13 of 13 nested (three in the fixture tests: lines 60, 119, 166; ten in the pin-count tests: lines 45, 46, 93, 94, 146, 147, 194, 195, 201, 230). No NESTING VIOLATION.
