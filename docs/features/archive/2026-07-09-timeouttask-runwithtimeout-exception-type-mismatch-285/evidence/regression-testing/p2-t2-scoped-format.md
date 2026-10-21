# P2-T2 — Scoped Format After the Handler Change, and Clause-Line Re-Measurement

Timestamp: 2026-09-01T08-19

Command: `dotnet tool run csharpier format UtilitiesCS/Threading/TimeOutTask.cs UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`

EXIT_CODE: 0

## Observation beyond the exit code — the `Formatted` summary line (verbatim)

```text
Formatted 2 files in 1092ms.
```

CSharpier's write-mode exit code is identical on a clean run and on a rewriting run, so this line is
recorded as required. It reports files **processed**, and the count of 2 confirms both in-scope files
were passed and no third file was touched.

## Re-measurement of the clause line

Measured against `UtilitiesCS/Threading/TimeOutTask.cs` **after** the format pass:

| Measurement | Value |
| --- | --- |
| Simple-match count of `catch (System.Exception e) when (e is TaskCanceledException \|\| e is TimeoutException)` | **1** |
| Line number of that single occurrence | 217 |
| **Character length of that line including its leading indentation** | **97** |

Line text as it now stands (leading 12-space indent preserved):

```text
            catch (System.Exception e) when (e is TaskCanceledException || e is TimeoutException)
```

### Why this matters

A simple-match count of 1 for the **full** clause string proves CSharpier left the clause on **one
physical line**. Had the formatter wrapped the `when` filter onto its own line, the full string would
no longer occur on any single line and this count would have been 0, breaking the single-line census
assertions that P3-T8 and the AC5 check-off depend on.

The measured length of 97 confirms the plan's reasoning for naming the exception variable `e` rather
than `ex`. CSharpier's default `printWidth` is 100. At 97 columns the clause fits with three columns
to spare; the `ex` spelling would occupy exactly 100 columns, sitting on the wrap boundary. The
in-repo precedent the plan cites — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs`,
which keeps a `when` clause inline where it fits and moves it to its own line where it does not — is
the behaviour this measurement confirms was avoided.

Output Summary: The scoped format pass exited 0 and processed the two in-scope files. After
formatting, the widened catch clause occupies exactly one physical line of 97 characters at line 217,
and the full clause literal occurs exactly once in the file.

Acceptance: met. `EXIT_CODE: 0`; the simple-match count of the full clause string is 1 after
formatting, proving CSharpier left the clause on one physical line; and the recorded character length
of that line including its leading indentation is 97.
