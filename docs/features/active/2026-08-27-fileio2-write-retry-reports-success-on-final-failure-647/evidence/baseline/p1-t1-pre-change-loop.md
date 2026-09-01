# P1-T1 — Pre-Change Control Flow of the Retry Loop

Timestamp: 2026-08-31T19-14
Command: read `UtilitiesCS/To Depricate/FileIO2.cs` lines 63 through 88 and count the single-line token `string filename,` across the whole file
EXIT_CODE: 0

## Verbatim quotation, `UtilitiesCS/To Depricate/FileIO2.cs` lines 63 through 88

```
63            while (!success)
64            {
65                try
66                {
67                    token.ThrowIfCancellationRequested();
68                    using (var sw = new StreamWriter(filepath, true, System.Text.Encoding.UTF8))
69                    {
70                        success = true;
71                        foreach (var output in strOutput)
72                            await sw.WriteLineAsync(output);
73                    }
74                }
75                catch (IOException)
76                {
77                    Interlocked.Increment(ref attempts);
78                    if (attempts < 100)
79                    {
80                        await Task.Delay(100);
81                    }
82                    else
83                    {
84                        logger.Error($"Failed to write to {filepath} after {attempts} attempts.");
85                        success = true;
86                    }
87                }
88            }
```

That is 26 lines, inclusive of both endpoints. Line 70 is `success = true;` and line 80 is `await Task.Delay(100);`, both as the task's acceptance requires.

## The four recorded observations

1. **The success flag is assigned inside the writer's `using` block before any write executes.** Line 68 opens the `using` on the `StreamWriter` constructor; line 69 opens its block; line 70 assigns `success = true`; only then does the `foreach` at line 71 begin issuing `await sw.WriteLineAsync(output)` at line 72. The flag is therefore set on the strength of the constructor having returned, not on the strength of any byte having been written.

2. **The catch clause is written without an exception variable.** Line 75 reads `catch (IOException)`. No identifier is bound, so the causing exception is unreachable from the handler body and is discarded. The log call at line 84 consequently uses the single-argument `ILog.Error(object)` overload and the cause never reaches the log. This contradicts the issue body's statement that the final-failure path "logs the exception"; the spec records the same correction.

3. **The delay is called with a single argument.** Line 80 reads `await Task.Delay(100);`. The method's `token` parameter is not passed, so the retry window is uncancellable even though line 67 already calls `token.ThrowIfCancellationRequested()` at the top of each attempt.

4. **The exhaustion branch logs without passing an exception and then sets the success flag.** Lines 83 through 86 are the `else` of `attempts < 100`: line 84 logs `$"Failed to write to {filepath} after {attempts} attempts."` with no exception argument, and line 85 assigns `success = true`. Setting the flag is what terminates the `while (!success)` loop at line 63, so the method returns normally after a write that never happened. This is the conflation the spec's Root Cause Analysis names: the flag means "stop retrying" at line 85 and is read as "the write succeeded" by the caller.

Consequence for the mid-write path, which follows from observations 1 and 4 together: an `IOException` raised at line 72 or by the implicit `sw.Dispose()` at line 73 reaches the catch at line 75 with `success` already `true`. The catch increments `attempts` to 1, takes the `attempts < 100` branch, awaits exactly one 100 millisecond delay at line 80, and falls out. The `while (!success)` test at line 63 is then false, so the loop exits with no retry and no log entry at all. That is the behavior the P3-T2 expect-fail run observes as a delay-invocation count of 1 against an expected 0.

## Parameter-list token count

BASELINE_FILENAME_PARAM_COUNT: 7

The single-line token `string filename,` occurs 7 times in the pre-change file, on lines 18, 36, 51, 110, 136, 210 and 221, in seven distinct method declarations:

```
18:        public static void DELETE_TextFile(string filename, string stagingPath)
36:        public static void WriteTextFile(string filename, string[] strOutput, string folderpath)
51:            string filename,
110:            string filename,
136:            string filename,
210:            string filename,
221:            string filename,
```

The five occupying a line of their own belong to `WriteTextFileAsync` (declared line 50), `CSV_ReadTxtF` (line 109), `CsvRead` (line 135), `CsvReadTo2D` (line 209) and `CsvReadToJagged` (line 220). The two further occurrences at lines 18 and 36 sit inside the single-line declarations of `DELETE_TextFile` and `WriteTextFile`.

DRIFT: the plan's P1-T1 text records the authoring-time observation as 5, on lines 51, 110, 136, 210 and 221. The measured whole-file count is 7. The authoring-time figure counted only the occurrences that stand alone on their own line and omitted the two embedded in a single-line declaration; the task asks for the whole-file occurrence count of the token, which is 7. The value recorded in this field is the measured one, taken by counting `[regex]::Matches` of the escaped literal against every line of the file rather than by copying the plan's figure.

P7-T1 asserts the post-change count equals this recorded value plus 1, the increment being the one parameter list the seam overload adds. Against the measured baseline of 7 the required post-change count is 8. The parenthetical in P7-T1 naming 6 is conditioned on the recorded value being 5 and does not apply.

Output Summary: The 26 quoted lines, four control-flow observations and the parameter-count field are all recorded. The two positional assertions hold: quoted line 70 is `success = true;` and quoted line 80 is `await Task.Delay(100);`.
