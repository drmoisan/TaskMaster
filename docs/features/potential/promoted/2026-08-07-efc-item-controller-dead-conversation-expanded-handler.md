# efc-item-controller-dead-conversation-expanded-handler (Issue #461)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-item-controller-dead-conversation-expanded-handler/ (Issue #461)
- Work Mode: full-bug

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #461
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/461
- Last Updated: 2026-08-08
## Summary

`EfcItemController.ConversationResolverPropertyChanged` filters on a property name that
`ConversationResolver` never raises, so the handler body is dead. Background-loaded conversation rows
never reach the topic thread through this path.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in
- UI path: `QuickFiler/Controllers/EfcItemController.cs` conversation-loading notification path
- Data source or fixture: a mail item whose conversation is resolved asynchronously

## Steps to Reproduce

1. Open the Email Filer on a mail item that participates in a conversation.
2. Allow the `ConversationResolver` to complete its background load, which raises
   `PropertyChanged` notifications.
3. Observe that the `EfcItemController` handler body never executes.

## Expected Behavior

When the conversation resolver signals that its conversation information has changed, the item
controller reacts and the background-loaded conversation rows reach the topic thread.

## Actual Behavior

`EfcItemController.cs:746` guards on:

```csharp
e.PropertyName == nameof(_dataModel.ConversationResolver.ConversationInfo.Expanded)
```

`nameof(...)` resolves at compile time to the literal `"Expanded"`. `ConversationResolver` only ever
raises:

- `"ConversationInfo"` — `ConversationResolver.Loading.cs:26`
- `"ConversationItems"` — `ConversationResolver.Loading.cs:167`
- `"Df"` — `ConversationResolver.Loading.cs:205`, `:227`
- `"UpdateUI"` — `ConversationResolver.cs:277`

It never raises `"Expanded"`. The subscription at `:667` fires, but the body at `:749-753` never
executes.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Code-read evidence recorded above (verified 2026-08-07 against the working tree).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

A documented notification path is silently inert. The failure mode is missing behavior rather than an
exception, which is why it has not been noticed.

## Suspected Cause / Notes

`nameof` applied to a nested property path (`...ConversationInfo.Expanded`) yields only the final
segment, so the expression compiles cleanly and reads plausibly while selecting a name the publisher
never emits. This is the classic `nameof`-on-a-path trap: the compiler cannot warn, because
`"Expanded"` is a genuine member name — just not one this publisher raises.

Determining the correct replacement requires deciding which of the four published names carries the
intent; `"ConversationInfo"` is the most likely, but that should be confirmed against the intended
behavior rather than assumed.

Discovered during preparation of issue #452 (epic #136) per-file coverage research. Out of scope there
under that feature's no-behavior-change constraint.

## Proposed Fix / Validation Ideas

- [ ] Confirm which published property name expresses the intended trigger
- [ ] Replace the guard with that name and consider a defensive test asserting publisher/subscriber agreement
- [ ] Unit coverage: handler fires for the intended name; ignores unrelated names
- [ ] Manual verification: conversation rows appear in the topic thread after a background load

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
