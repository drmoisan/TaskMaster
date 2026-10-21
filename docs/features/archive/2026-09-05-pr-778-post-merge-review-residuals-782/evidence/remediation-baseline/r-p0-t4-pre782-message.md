# [P0-T4] The pre-782 and current `WpfDispatcherYield` message literals

Timestamp: 2026-09-06T01-29

Command:

```powershell
git show pre-782-base:UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs
Get-Content -LiteralPath 'UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs'
```

EXIT_CODE: 0

Output Summary: the pre-782 message literal contains both `UiThread.Init()` and
`before yielding folder tree work`; the current throw passes
`UiThread.DispatcherNotInitializedMessage` and contains no literal at all. That pairing is the
evidence that a wildcard pattern on `UiThread.Init()` cannot distinguish the two messages.

## The pre-782 revision

`git show pre-782-base:UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` returns a 77-line
file. Its single `throw` spans lines 64-66 of that revision, and the message literal is on line 65:

```text
[64]                 throw new InvalidOperationException(
[65]                     "The UI dispatcher has not been captured. Call UiThread.Init() before yielding folder tree work."
[66]                 );
```

The literal, quoted verbatim and on one line:

```text
"The UI dispatcher has not been captured. Call UiThread.Init() before yielding folder tree work."
```

It contains the substring `UiThread.Init()` and it contains the substring
`before yielding folder tree work`.

## The current worktree

`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` is 76 lines. Its single `throw` is on
line 65:

```text
[65]                 throw new InvalidOperationException(UiThread.DispatcherNotInitializedMessage);
```

That line contains `UiThread.DispatcherNotInitializedMessage` and contains no message literal.

## Why this pair is the R3 evidence

The pre-782 message and the message the shared constant now supplies both contain the substring
`UiThread.Init()`. A FluentAssertions pattern of `"*UiThread.Init()*"` therefore matches both of
them, so that assertion cannot distinguish the delivered message from the pre-782 message and cannot
detect the removal or the restoration of the `before yielding folder tree work` tail. That is the
precise sense in which the claim R3 reports — that the tail's removal is pinned by the C20
`WithMessage` assertion — is false as it was written.

The remediation replaces the wildcard with a reference to the shared constant, which is compared
against the entire message, so a tail appended at this throw site no longer matches. [P1-T5] through
[P1-T9] observe that difference directly rather than resting on this derivation.
