# Phase 4 — fail-before evidence for the #463 incognito argument

Timestamp: 2026-08-28T00-27
Task: [P4-T3] [expect-fail]
Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU`, then `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~IncognitoArgument_IsAsciiDoubleHyphenIncognitoWithTrailingSpace" "/Logger:trx;LogFileName=463-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p4-t3`, both under `pwsh -NoProfile`
EXIT_CODE: 1
ExpectedExitCode: 1

A failing run is the intended outcome of this task. Build exit code was 0, so the red result is a genuine
assertion failure.

## Counters

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0"
inProgress="0" pending="0"
```

Total executed: **1**, which is greater than zero, satisfying the non-vacuity rule.

## The result

| Test | Outcome | Duration |
|---|---|---|
| `IncognitoArgument_IsAsciiDoubleHyphenIncognitoWithTrailingSpace` | **Failed** | 123 ms |

## The observed non-ASCII code point

Failure message, verbatim (the two arrow glyphs are FluentAssertions' index markers):

```
Expected actual to be a match with the expectation because Chromium command-line switches are
introduced by two ASCII hyphen-minus characters, but it differs at index 0:
   ↓ (actual)
  "–incognito "
  "--incognito "
   ↑ (expected)
```

The observed character is identified by **byte inspection of the TRX itself**, not by transcription. The
actual-value fragment inside the `<Message>` element renders as these bytes:

```
22 E2 80 93 69 6E 63 6F 67 6E 69 74 6F 20 22
"  <-- E2 80 93 -->  i  n  c  o  g  n  i  t  o  SP  "
```

`E2 80 93` is the UTF-8 encoding of **U+2013 EN DASH**. The value therefore begins with one U+2013
character where two U+002D HYPHEN-MINUS characters are required. This matches the byte dump taken from
the source line in `[P4-T1]`, which showed the same `E2 80 93` sequence immediately after the opening
quotation mark.

Chromium introduces command-line switches with two ASCII hyphens and passes
`CoreWebView2EnvironmentOptions.AdditionalBrowserArguments` through verbatim, so the unrecognised token
is discarded silently and the preview WebView2 retains browsing data.

## Defect-preserving introduction

Per decision D8, the constant `IncognitoArgument` was introduced by `[P4-T1]` in a **defect-preserving**
form: it was initialised to the file's existing literal, EN DASH and all, and no character was corrected.
That is what makes this red run meaningful — the assertion could not have been written at all before the
member existed, and introducing the member without preserving the defect would have produced a test that
was green from the outset and gated nothing.

## TRX artifact

`docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p4-t3/463-fail.trx`

Sanitised in place: worktree paths replaced with `<repo-root>`, account and machine names replaced with
`<user>` and `<host>`; a case-insensitive search for either returns zero matches. The sanitisation
touched no part of the failure message. The `/InIsolation` `Deploy_*` scratch tree was removed.

Output Summary: 1 executed, 1 failed, vstest exit code 1 as expected. The failure message's actual-value
bytes are `22 E2 80 93 ...`, identifying the observed leading character as U+2013 EN DASH where two
U+002D characters are required.
