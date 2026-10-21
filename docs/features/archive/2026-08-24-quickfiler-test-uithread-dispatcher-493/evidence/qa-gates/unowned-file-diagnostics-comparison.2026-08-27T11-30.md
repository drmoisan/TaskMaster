# Unowned-File Diagnostic Comparison (P4-T2)

Timestamp: 2026-08-27T11-30
Task: [P4-T2]
Command: For each of `TestResults/plan-logs/p3-t3/msbuild-analyzers.log` and `TestResults/plan-logs/p3-t4/msbuild-nullable.log`, extract every line containing the simple string `QfcItemController.FocusAndThemeTests.cs` and every line containing the simple string `UiThread.cs`, apply the § Conventions redaction filter, and compare against the sets recorded by `P0-T10`.
EXIT_CODE: 0
Output Summary: **Match counts are equal on every one of the four token-and-log combinations** (2
each, matching the baseline's 2 each). **The diagnostic-bearing subset is empty on both sides of
every comparison**, so AC-6's diagnostic clause holds **absolutely**, not merely as non-regression.
**Byte-exact set equality holds for the `UiThread.cs` token in both logs.** It does **not** hold for
the `QfcItemController.FocusAndThemeTests.cs` token in either log, and the symmetric difference is
exactly two tokens — the two source-file arguments this feature added to the `QuickFiler.Test`
compilation — with no other difference of any kind.

## Cited baseline artifact

Resolved per § Conventions from the stem `unowned-file-diagnostics-baseline`:
`<FEATURE>/evidence/baseline/unowned-file-diagnostics-baseline.2026-08-27T10-14.md`

## Result 1 — match counts

| Log | Token | Baseline count | Final count | Equal |
| --- | --- | --- | --- | --- |
| analyzer step | `QfcItemController.FocusAndThemeTests.cs` | 2 | 2 | **yes** |
| analyzer step | `UiThread.cs` | 2 | 2 | **yes** |
| type-check step | `QfcItemController.FocusAndThemeTests.cs` | 2 | 2 | **yes** |
| type-check step | `UiThread.cs` | 2 | 2 | **yes** |

Baseline `AnalyzerStepMatchCount: 4`, final analyzer-step total 4.
Baseline `NullableStepMatchCount: 4`, final type-check-step total 4.

## Result 2 — diagnostic-bearing subset equality

| Log | Token | Baseline diagnostic count | Final diagnostic count | Equal |
| --- | --- | --- | --- | --- |
| analyzer step | `QfcItemController.FocusAndThemeTests.cs` | 0 | 0 | **yes** |
| analyzer step | `UiThread.cs` | 0 | 0 | **yes** |
| type-check step | `QfcItemController.FocusAndThemeTests.cs` | 0 | 0 | **yes** |
| type-check step | `UiThread.cs` | 0 | 0 | **yes** |

Because the baseline diagnostic count was zero, set equality against it **is** the absolute condition
AC-6's final sentence states ("No analyzer diagnostic is raised at either call site under toolchain
steps 2 and 3"). This is the stronger of the two cases the plan's § Notes rule 2 anticipates:
no diagnostic naming either unowned file exists after the change, and none existed before, so the
criterion is discharged absolutely rather than as non-regression.

## Result 3 — byte-exact set equality, and the symmetric difference

### `UiThread.cs` — equality holds

| Log | Baseline SHA-256 of each redacted line | Final SHA-256 | Equal |
| --- | --- | --- | --- |
| analyzer step | `897a69626ed94b1f9a4f48dcecaa35ebece77e508b404cd013d2223d8f598cd4` | `897a69626ed94b1f9a4f48dcecaa35ebece77e508b404cd013d2223d8f598cd4` | yes |
| analyzer step | `5177d946258328a9fb3ae8d2b1a236e99e86066a135753f7fae90209fc350b5f` | `5177d946258328a9fb3ae8d2b1a236e99e86066a135753f7fae90209fc350b5f` | yes |
| type-check step | `166e4ace653d4a19d6638723485c631ec863dfcc4a46a65e9b2ea6a6a712cc8b` | `166e4ace653d4a19d6638723485c631ec863dfcc4a46a65e9b2ea6a6a712cc8b` | yes |
| type-check step | `b1173f7203e898f9d51b53d8f3390f7a093fc1c28c4754333b504a676380bd52` | `b1173f7203e898f9d51b53d8f3390f7a093fc1c28c4754333b504a676380bd52` | yes |

Symmetric difference: **empty**. These lines belong to the `UtilitiesCS` compilation, which this
feature does not change, so this is a real and satisfied gate on AC-7.

### `QfcItemController.FocusAndThemeTests.cs` — equality does not hold

| Log | Baseline SHA-256 | Final SHA-256 | Baseline length | Final length |
| --- | --- | --- | --- | --- |
| analyzer step, `csc.exe` line | `5e9bcfaf9a2dbe939b5de86d59b2e818c61abf98e6ffab2a120735e041794923` | `5564f2be040abe735cff23793b5645a8eb2f7096f4367fef91ca32a13279fd67` | 33240 | 33363 |
| analyzer step, `BuildResponseFile` line | `feae55559f707ab32c10b006a641986383febb91bc633e4bb5d172a4171df901` | `9cbd854d436dd6025c4b9660ffdb231c9f766acd6adef307bca30adf832ec143` | 33163 | 33286 |
| type-check step, `csc.exe` line | `b2501bc1592c1717c3206bd9743b047fdeed8d5245c26e8ae68ca1c54cd45a58` | `9d2f31b0b516d71d9c96dcd3ac702348331ef80d017dd759448a50ae96dc00da` | 33254 | 33377 |
| type-check step, `BuildResponseFile` line | `8cdc01b82cc4d95d6bddf312703114e23428878030db13972a3a19deb8e9a217` | `557016fc44c2eaf58d3a3557dcc75aec73f4a2b6e9112d7b4f21ea500dc666d4` | 33177 | 33300 |

Each of the four lines grew by exactly **123 characters**.

**Symmetric difference, computed at token granularity** by splitting the baseline and final
`csc.exe` command lines on whitespace and comparing the resulting token multisets:

```
TOKEN_DIFF_COUNT=2
=> Controllers\QfcItemController.UiThreadDispatcherFixture.cs
=> Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs
```

`=>` marks a token present in the final line and absent from the baseline line. There is no token
present in the baseline and absent from the final line, and there is no third added token. The two
added tokens are the two source files `P1-T1` and `P1-T3` created and `P1-T2` wired into
`QuickFiler.Test.csproj`; their two path strings total 123 characters including the separating space,
which accounts for the length delta exactly.

## Assessment stated plainly

The `P0-T10` baseline artifact recorded this outcome as an expected hazard before the change was
made, so it is a disclosed result rather than a surprise. The reason byte-exact set equality cannot
hold for this token is structural, not behavioural: **every** line matching
`QfcItemController.FocusAndThemeTests.cs` in an MSBuild log at default verbosity is a compiler
invocation line — the `csc.exe` command line and its `BuildResponseFile` echo — and such a line
enumerates the compiling project's entire source-file set. Adding two files to `QuickFiler.Test`
therefore necessarily lengthens the line that happens to contain the token, without touching the file
the token names.

Three separate facts establish that no diagnostic regression occurred:

1. The diagnostic-bearing subset is empty on both sides of all four comparisons (Result 2).
2. Both Phase 3 logs contain **zero** lines matching `error CS` or `warning CS` anywhere, recorded in
   `<FEATURE>/evidence/qa-gates/msbuild-analyzers.2026-08-27T11-13.md` and
   `<FEATURE>/evidence/qa-gates/msbuild-nullable.2026-08-27T11-16.md`; both report 5 warnings and
   0 errors, the same counts as the Phase 0 baselines.
3. `P4-T1` proves `QfcItemController.FocusAndThemeTests.cs` is byte-identical to its Phase 0 state,
   so no diagnostic could have been introduced *into* it.

The condition that failed is the byte-exact comparison of compiler-invocation text; the condition
AC-6 states is about diagnostics, and that condition holds absolutely. `P5-T6` records the baseline
diagnostic counts this comparison ran against so a reviewer can see which case held.

Full redacted extract files (git-ignored, not committed):
`TestResults/plan-logs/p4-t2/analyzer-step.*.extract.txt` and
`TestResults/plan-logs/p4-t2/nullable-step.*.extract.txt`, against the `p0-t10` counterparts.
