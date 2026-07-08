# Increment 2 Seam Verification — QuickFiler

Timestamp: 2026-06-14T08-22

Command: source inspection of QuickFiler target files (Read/Grep)

EXIT_CODE: 0

InternalsVisibleTo("QuickFiler.Test") confirmed in QuickFiler/Properties/AssemblyInfo.cs line 5.

## Confirmed seams (file/line; no [ExcludeFromCodeCoverage]); fully testable without Outlook

- KaChar (line 11) and KaCharAsync (line 58) — QuickFiler/Controllers/KaChar.cs
  - Pure value objects: SourceId/Key/Delegate/Update properties; `KeyEquals(char)`; `DelegateType` (KaChar). KaChar.Delegate is `Action<char>`; KaCharAsync.Delegate is `Func<char, Task>`. Two constructors each: parameterless and (sourceId, key, delegate). NO null-guard in the constructor: a null delegate is simply stored (negative test asserts stored null / no throw, NOT an exception). No Outlook, no exemption.

- KaKey (line 11) and KaKeyAsync (line 58) — QuickFiler/Controllers/KaKey.cs
  - Same shape with Key of type System.Windows.Forms.Keys. KaKey.Delegate is `Action<Keys>`; KaKeyAsync.Delegate is `Func<Keys, Task>`. Keys is an enum (no WinForms message loop required to use the enum value). No null-guard; no Outlook; no exemption.

- KaStringAsync (line 10) — QuickFiler/Controllers/KaStringAsync.cs
  - Pure value object. Constructor normalizes Key via ToLower(); Key setter also ToLower(). `KeyEquals(string other)` has branching with Update/ToggleControl side effects and an `Activated` flag; all delegates are plain Action/Func that can be stubbed synchronously. No Outlook; no exemption.

- KbdActions<TKey, UClass, VDelegate> (line 15) — QuickFiler/Controllers/KbdActions.cs
  - Generic registry over Swordfish ConcurrentObservableCollection. Public surface: indexer get/set (line 37), ContainsKey (50), FilterKeys (52), Find (54: 0 -> default, 1 -> first, >1 -> InvalidOperationException), FindIndex (72: 0 -> -1, 1 -> index, >1 -> InvalidOperationException), Add(sourceId,key,delegate) (91: duplicate -> ArgumentException), Add(instance) (107: duplicate -> ArgumentException), Remove (124: present -> true, absent -> false), GetEnumerator (138), Keys (142). Requires a concrete UClass with new() constraint; KaChar/KaKey satisfy IKbdAction<char,Action<char>> / IKbdAction<Keys,Action<Keys>>. Pure collection management; no Outlook; no exemption.

## Restricted / partially-untestable seams (pure surface only)

- FilerQueue (line 14) and FilerQueueItem (line 68) — QuickFiler/Controllers/FilerQueue.cs
  - PURE/TESTABLE: FilerQueueItem constructor (line 70) — positive (filer+helpers stored), negative (ThrowIfNull on null filer / null helpers; null element in helpers -> ArgumentNullException). FilerQueue.Consumer default is Task.CompletedTask (line 42); initial Queue is empty.
  - NOT PURE: FilerQueue.Enqueue (line 22/31) immediately starts ConsumeAsync() -> item.Filer.SortAsync(...) on a background Task.Run, touching EmailFiler (Outlook-bound). The enqueue/consume path is NOT a pure queue-management path and is NOT exercised (would require a live/Outlook EmailFiler and would spin an uncontrolled background task — violates determinism/no-Outlook constraints). RESTRICTION recorded: P2-T5 covers only FilerQueueItem construction/validation and the FilerQueue default state.

- QfcQueue (line 20) — QuickFiler/Controllers/QfcQueue.cs
  - NOT PURE / NOT TESTABLE without the WinForms+Outlook graph: the primary constructor requires (CancellationToken, QfcHomeController, IApplicationGlobals). All queue operations use TableLayoutPanel, MailItem, EmailMoveMonitor, UiThread.Dispatcher, and Task.Delay. There is no pure queue-management path that can be constructed/exercised without the Outlook/WinForms controller graph. Constructing QfcHomeController is itself Outlook/WinForms-bound. RESTRICTION recorded: P2-T6 has no pure, Outlook-free path to exercise; this is a Flag-and-Stop-style gap reported in evidence/other/ at P2-T6 (no production seam added).

## Output Summary
KaChar/KaCharAsync, KaKey/KaKeyAsync, KaStringAsync, and KbdActions<> exist with the file/line
references above, are pure (no Outlook), and carry no [ExcludeFromCodeCoverage]; they are fully
testable from QuickFiler.Test. FilerQueue/FilerQueueItem have a pure testable subset (item
construction + validation) only; the enqueue/consume path is Outlook-bound and excluded. QfcQueue
has no pure Outlook-free path and cannot be exercised without the WinForms/Outlook controller graph;
this is recorded as a Flag-and-Stop gap. Constructor null-guard nuance for the Ka* types: null
delegates are stored, not rejected.
