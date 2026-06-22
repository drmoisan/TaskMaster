# Increment-3 OlReminders latency-vs-delay capture (2026-06-21)

Source: live Outlook startups of the add-in built from branch
`bug/outlook-startup-intelconfig-deserialize-stall-207` at commit `cfbbd636`, varying the
`RemindersProbeDelaySeconds` setting across launches.

## OlReminders latency as a function of when it is accessed

| Source | When OlReminders accessed (after startup) | OlReminders latency |
|---|---|---|
| Increment-2 Run A | ~7 s (synchronous in Hook) | 113,642 ms |
| Increment-2 Run B | ~64 s (incidental) | 20 ms |
| Increment-3 probe, delay 30 s | elapsedSinceHook 55 s | 32.35 ms |
| Increment-3 probe, delay 120 s | elapsedSinceHook 171 s | 17.79 ms |

Conclusion: **Possibility 1 confirmed.** `OlReminders` latency is a relocatable readiness wait.
Accessed early it blocks ~113 s; accessed 30 s+ later it returns in ~20–32 ms. The cost is a wait
for Outlook/Exchange store readiness, not an intrinsic build cost of the first access.

## Deferring OlReminders does NOT fix the stall — the block migrates to Ol.Inboxes

With `RemindersProbeDelaySeconds=30` (Run at 15:44–15:45), the deferred reminders access was fast
(32 ms), but `Hook` then blocked on the inbox subscription instead:

```
Hook complete | elapsedMs=53915; toDoItemsMs=3.15; remindersMs=2.54; inboxSubscribeMs=53908.91
```

`inboxSubscribeMs=53908.91` — `Globals.Ol.Inboxes` (the inbox enumeration/subscription) blocked
53.9 s. The dominant STA block simply moved from `OlReminders` to `Ol.Inboxes`. This confirms the
risk raised before implementing the probe: a single-call deferral promotes the next
readiness-dependent COM call to the blocker.

## Accessing Ol.Inboxes too early can THROW, failing the startup hookup chain

In two launches the early `Ol.Inboxes` access did not merely block — it raised a COMException that
propagated up and failed the entire `IdleAsyncQueue` startup action:

- 15:40 launch: `COMException (0xDAC40111)` at `AppOlObjects.LoadInboxes()` (AppOlObjects.cs:104)
  via `AppEvents.Hook()` (AppEvents.cs:202), ~130 ms after Hook start.
- 15:49 launch (delay 120 s): `COMException (0x8E640111)` at the same site, ~60 s after Hook start.

Both surfaced as `ERROR UtilitiesCS.Threading.IdleAsyncQueue - Failed to execute
IdleAsyncQueue.actionAsync`. When this occurs, the inbox `ItemAdd` subscription is not established —
a correctness failure (inbox auto-processing would not be hooked), not only a performance issue.

Implication: in current production (probe disabled), the long synchronous `OlReminders` block
incidentally "waits out" Outlook readiness, so by the time `Ol.Inboxes` is accessed Outlook is ready
and the subscription succeeds. The 113 s freeze is therefore acting as an accidental readiness gate.
Removing it without an explicit gate exposes `Ol.Inboxes` to the not-ready window, where it blocks or
throws.

## Separate STA-contention component (not a single TaskMaster COM call)

In the 15:45:53–15:48 launch, `Hook` was entirely fast (`elapsedMs=14`,
`inboxSubscribeMs=5.44`) because Outlook was ready by the time it ran, yet the phase table attributed
**1:54.96 to IntelConfig**. `ReadConfigurationAsync` measured ~130 ms in that run, so the ~115 s is
the `Task.Run` continuation waiting for the STA, which was saturated by assembly loading and the
Teams add-in (repeated `Microsoft.Teams.MeetingAddin` COM exceptions in that window). This component
is independent of TaskMaster's own COM calls and is not removed by gating `Hook`.

## Net conclusions for the corrective fix

1. The readiness wait is real and relocatable (P1), so a readiness-gated hookup can work.
2. The gate must cover ALL of `Hook`'s readiness-dependent COM accesses — `ToDoFolder.Items`,
   `OlReminders`, and `Ol.Inboxes` — not just `OlReminders`. Deferring one promotes the next.
3. The gate must keep the STA pumping (no synchronous block) and must treat a not-ready COMException
   from `Ol.Inboxes` as a retry condition, not a fatal error, so the inbox subscription is not lost.
4. A fixed delay is insufficient: readiness arrived ~30 s in some launches but the STA stall extended
   past 2 minutes in another. The gate must poll a cheap readiness signal, not hardcode a wait.
5. The STA-contention component (assembly load + Teams add-in) is outside TaskMaster's COM calls and
   will not be fully removed by this fix; it should be acknowledged as a residual.
