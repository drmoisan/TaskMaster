# Anchor Re-Derivation Against the Current Tree (P1-T1)

Timestamp: 2026-09-01T12-30

File: `QuickFiler/Controllers/QfcHomeController.Metrics.cs`
Branch: `bug/qfc-metrics-flush-writes-empty-session-file-646`
HEAD at re-derivation: `1ea16f43` (post-P0-T6 reconciliation; `origin/main` = `8996b287`
is an ancestor)

The line numbers below were derived by searching the file as it stands now. They were not
copied from the plan text, from `research.2026-08-31T20-30.md`, or from
`research-correction.2026-08-31T20-45.md`.

## Anchor A

Command:
`grep -n -F 'var lines = strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();' QuickFiler/Controllers/QfcHomeController.Metrics.cs`
EXIT_CODE: 0

Result:

```
174:            var lines = strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
```

Occurrence count (`grep -c -F`): **1**

## Anchor B

Command:
`grep -n -F 'bool metricsWritten = await MetricsFileWriter(' QuickFiler/Controllers/QfcHomeController.Metrics.cs`
EXIT_CODE: 0

Result:

```
179:            bool metricsWritten = await MetricsFileWriter(
```

Occurrence count (`grep -c -F`): **1**

## Acceptance

| Condition | Result |
|---|---|
| Anchor A found exactly once | Yes (1 occurrence, line 174) |
| Anchor B found exactly once | Yes (1 occurrence, line 179) |
| Line numbers recorded | Yes (174 and 179) |

ACCEPTANCE: MET.

## Region Context (read directly, lines 170-192)

```
170
171            // The call is made through IQfcCollectionController.GetMoveDiagnostics, which carries
172            // no XML documentation and therefore no non-null element guarantee, so this filter
173            // defends the interface contract rather than a known producer defect.
174            var lines = strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
175
176            // CancellationToken.None, never the session Token: the dispatcher continuation that
177            // carries this write is not awaited to completion, so a session cancellation can be
178            // raised while the write is in flight and must not destroy the metrics.
179            bool metricsWritten = await MetricsFileWriter(
180                filename,
181                lines,
182                myDocuments,
183                CancellationToken.None
184            );
185            if (!metricsWritten)
186            {
187                logger.Error(
188                    $"Session metrics were not written to {LOC_TXT_FILE}. The writer exhausted its "
189                        + "retry budget or failed after opening the file."
190                );
191            }
192        }
```

## Consequences for P1-T5

The three-line `CancellationToken.None` explanatory comment occupies lines **176-178** and
immediately precedes Anchor B. P1-T5 requires the guard to be inserted immediately after
Anchor A and **before** that comment block, so the comment stays adjacent to the writer
statement it explains. The insertion point is therefore between line 174 and line 176,
replacing the single blank line at 175 with the guard block plus separating blank lines.

After the four-line guard is inserted, every line from the current 176 onward shifts by
`+4`. Predicted post-fix positions:

| Element | Pre-fix line(s) | Predicted post-fix line(s) |
|---|---|---|
| Anchor A | 174 | 174 |
| `if (lines.Length == 0)` | — | 176 |
| `{` | — | 177 |
| `return;` | — | 178 |
| `}` | — | 179 |
| `CancellationToken.None` comment | 176-178 | 180-182 |
| Anchor B | 179 | 183 |
| `if (!metricsWritten)` branch | 185-191 | 189-195 |
| Method close | 192 | 196 |

These predictions are verified against the actual file in P1-T6 and used by P2-T7 to locate
the four guard lines in the final Cobertura report.

## Cross-Check Against Independent Sources

| Source | Anchor A | Anchor B | Agrees |
|---|---|---|---|
| This re-derivation (authoritative) | 174 | 179 | — |
| Plan self-review section | 174 | 179 | Yes |
| Orchestrator post-merge cross-check at handoff | 174 | 179 | Yes |
| P0-T11 baseline Cobertura per-line hits | 174 (`hits=1`) | 179 (`hits=1`) | Yes |

Four independent derivations agree, and the coverage report additionally confirms both
anchor lines are executed by the existing test suite.
