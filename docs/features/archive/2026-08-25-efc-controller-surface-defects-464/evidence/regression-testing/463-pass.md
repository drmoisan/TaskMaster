# Phase 4 — pass-after evidence for the #463 incognito argument

Timestamp: 2026-08-28T00-29
Task: [P4-T7]
Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU`, then `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~IncognitoArgument_IsAsciiDoubleHyphenIncognitoWithTrailingSpace" "/Logger:trx;LogFileName=463-pass.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p4-t7`, both under `pwsh -NoProfile`
EXIT_CODE: 0

Build exit code: 0.

## Counters

```
total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0"
inProgress="0" pending="0"
```

Total executed: **1**. Failed: **0**.

## The result

| Test | Outcome | Duration |
|---|---|---|
| `IncognitoArgument_IsAsciiDoubleHyphenIncognitoWithTrailingSpace` | **Passed** | 71 ms |

The same test, against the same three assertions, was **red** in `[P4-T3]` with the actual value's first
character identified from the TRX bytes as U+2013 EN DASH. The only intervening change is `[P4-T4]`,
which replaced that one character with two U+002D HYPHEN-MINUS characters in the constant's initialiser.
The red-to-green transition is therefore attributable to that correction and to nothing else.

## What the green run establishes

All three assertions now hold against `EfcItemController.IncognitoArgument`:

1. the value equals `--incognito ` — two ASCII hyphen-minus characters, the token `incognito`, and one
   trailing space;
2. every character satisfies `c <= 0x7F`;
3. the first two characters are both `-` (U+002D).

The test performs no file input or output and reads no `.cs` file from disk: it asserts against the value
the program actually uses, not against source bytes.

This covers the `EfcItemController` site only. The `QfcItemController.ViewerSetup.cs` site has no
executable instrument — its enclosing member is coverage-exempt and needs the real WebView2 runtime — and
is verified instead by the one-line-diff assertion in
`evidence/qa-gates/463-viewersetup-one-line-diff.md` and the byte review in
`evidence/other/463-viewersetup-review.md`. The third site, inside the dead `InitializeWebView()`, was
removed with its container in `[P1-T8]`.

## TRX artifact

`docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p4-t7/463-pass.trx`

Sanitised in place: worktree paths replaced with `<repo-root>`, account and machine names replaced with
`<user>` and `<host>`; a case-insensitive search for either returns zero matches. The `/InIsolation`
`Deploy_*` scratch tree was removed.

Output Summary: 1 of 1 executed and passed, 0 failed, vstest exit code 0. The assertion that was red in
[P4-T3] against a U+2013 leading character is now green against `--incognito `.
