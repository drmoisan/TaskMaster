# N1 Location Confirmation

- Timestamp: 2026-07-08T00-10
- Command: `Select-String -Path 'UtilitiesCS.Test\OutlookObjects\Store\StoreDisableServiceTests.cs' -Pattern 'ThrowAsync<'`
- EXIT_CODE: 0
- Output Summary: Exactly 2 matches found (not 4 as the plan's acceptance text estimated for
  total `ThrowAsync<` count — see note below):
  - Line 229: `.ThrowAsync<ArgumentException>();` inside
    `Writes_ThrowArgumentException_ForSentinelIdentity`, on the
    `service.Invoking(s => s.ReenableAsync(sentinel)).Should()...` statement. Confirmed via prior
    file read: this statement is not preceded by `await` and the method signature is
    `public void`.
  - Line 263: `.ThrowAsync<InvalidOperationException>();` inside
    `Writes_ThrowInvalidOperation_WhenModelIsNull`, on the
    `service.Invoking(s => s.ReenableAsync(StoreIdentity.Resolve(StoreName))).Should()...`
    statement. Confirmed via prior file read: this statement is not preceded by `await` and the
    method signature is `public void`.

## Clarification on Plan Acceptance Text

The plan's acceptance text describes "4 matches" as covering both `Throw<...>` (2, for the
synchronous `DisableSessionOnly`/`DisableForFutureSessions` guard calls) and `ThrowAsync<...>` (2,
for the `ReenableAsync` guard calls) collectively across both affected test methods. The
`-Pattern 'ThrowAsync<'` search specifically (as literally run) returns exactly the 2
`ThrowAsync<` occurrences, matching the plan's substantive claim: "the 2 `ReenableAsync`
`.ThrowAsync<...>()` calls ... are the only `ThrowAsync<` occurrences and are confirmed not
preceded by `await`." No discrepancy with plan intent; both target locations exist exactly as
described.
