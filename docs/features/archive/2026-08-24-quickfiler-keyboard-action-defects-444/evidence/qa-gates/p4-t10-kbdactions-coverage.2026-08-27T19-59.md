# [P4-T10] Constructor-guard branch coverage in `KbdActions.cs`

Timestamp: 2026-08-27T19-59
Command: locate the `<class>` element of `coverage\coverage.cobertura.final.xml` whose `filename` ends `KbdActions.cs`, then read the `hits` attribute of its `lines/line` children over the constructor's line span
EXIT_CODE: 0
Output Summary: exactly one matching `<class>` element. The guard's `throw` statement (source line
61) has `hits="1"` and the constructor's normal-completion statement (source line 65) has
`hits="1"`. Both are greater than 0, so the throwing path and the non-throwing path are each covered.

`CLASS_COUNT = 1` — the query matched a single `<class>`,
`QuickFiler.Controllers.KbdActions<TKey, UClass, VDelegate>`, `filename`
`QuickFiler\Controllers\KbdActions.cs`.

## The two recorded lines, with their source quoted so the mapping is auditable

| Source line | `hits` | Path represented | Source text (verbatim from `QuickFiler/Controllers/KbdActions.cs`) |
| --- | --- | --- | --- |
| 61 | **1** | the **throwing** path: a duplicate `SourceId` + stored-key pair was found in the seed | `                        throw new ArgumentException(message, nameof(list));` |
| 65 | **1** | the **non-throwing** path: the nested scan completed without finding a duplicate and the constructor returned normally | `        }` |

Line 65 is the closing brace of the `public KbdActions(IEnumerable<UClass> list)` constructor, which
opens its body at line 43. Reaching it is only possible by falling out of the bottom of the
`for`/`for`/`if` scan without executing the `throw`, so a non-zero `hits` on line 65 is exactly the
assertion that the guard was entered and completed without rejecting the seed. The compiler emits a
sequence point for that brace, which is why Cobertura carries a `<line>` element for it.

## Surrounding constructor lines, for context

```
line 42 hits=1                 // constructor signature
line 43 hits=1                 // opening brace
line 44 hits=1                 // _list = new List<UClass>(list);
line 49 hits=1  branch=True    // outer for
line 51 hits=1  branch=True    // inner for
line 53 hits=1  branch=True    // if (duplicate?)
line 58 hits=1                 // string message =
line 60 hits=1                 // logger.Error(message);
line 61 hits=1                 // throw new ArgumentException(...)   <- throwing path
line 65 hits=1                 // }                                  <- normal completion
```

Every line of the constructor carries a non-zero `hits`, and all three branch-carrying lines (49,
51, 53) are reached. The class-level figures are `line-rate="0.9897959183673469"` and
`branch-rate="1"`.

`dotnet-coverage` reports `hits` as a binary reached/not-reached indicator rather than an execution
tally, so `hits="1"` means "reached at least once", which is what this task's acceptance requires.

## Acceptance

- Both recorded `hits` values are greater than `0` — met (1 and 1), which is the throwing path and
  the non-throwing path respectively.
