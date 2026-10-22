# Coverage delta against the `[P0-T14]` baseline

Timestamp: 2026-08-26T14-05
Task: [P7-T7]

Sources: `[P0-T14]` baseline Cobertura
(`docs/features/active/qfc-item-controller-defects-484/evidence/baseline/coverage-baseline.cobertura.xml`)
and `[P7-T6]` post-change Cobertura
(`docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/coverage-final.cobertura.xml`).
Per decision D10 the repository-wide figure is the root `<coverage>` element's `line-rate` and the
per-member figures come from the `<method>` elements' `line-rate`.

EXIT_CODE: 0

## 1. Repository-wide line coverage

| | Root `line-rate` | Percent | `lines-covered` / `lines-valid` |
|---|---|---|---|
| `[P0-T14]` baseline | 0.84775 | 84.775 % | 53766 / 63422 |
| `[P7-T6]` post-change | **0.848323** | **84.8323 %** | 53905 / 63543 |
| Delta | +0.000573 | +0.0573 pp | |

**Post-change repository-wide line coverage is 84.8323 percent, which is at least 80 percent.** It is
also higher than the baseline, so the repository-wide figure is not reduced.

Branch rate moved from 0.786876 to 0.788057 over the same interval.

## 2. Per-member line rate for the five named new production members

Every one of these is at least 90 percent:

| Member | `line-rate` | Percent | >= 90 % |
|---|---|---|---|
| `TryResolveCidResource` | 1 | 100 % | yes |
| `NotifyMoveFailure` | 1 | 100 % | yes |
| `UnwireEvents` | 1 | 100 % | yes |
| `UnwireControlTreeEvents` | 1 | 100 % | yes |
| `UnwireIntentEvents` | 1 | 100 % | yes |

The compiler-generated `ForAllControls` walk delegate inside `UnwireControlTreeEvents`
(`<UnwireControlTreeEvents>b__172_0`) also measures `line-rate="1"`.

## 3. Production lines added by this feature that fall below 90 percent

Six executable lines added by this feature are uncovered. They are exactly the three carve-outs named
by `[P7-T7]`, and nothing else.

### (a) Inside the pre-existing `[ExcludeFromCodeCoverage]` `InitializeWebViewAsync`

The two capture-field assignments and the lambda adapter added at
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:85`, `:92-106`, and `:107` sit inside
`InitializeWebViewAsync`, which carries the pre-existing `[ExcludeFromCodeCoverage]` attribute. The
method's own lines are consequently not measured at all; the compiler-generated lambda body is measured
separately as `<InitializeWebViewAsync>b__124_0` with `line-rate="0"`, contributing uncovered lines
`:94`, `:95`, and `:101`. Reaching them requires a live WebView2 runtime, which the repository
unit-test policy prohibits.

### (b) `DetachWebResourceRequestedHandler`

| Member | `line-rate` | Percent |
|---|---|---|
| `DetachWebResourceRequestedHandler` | **0.625** | **62.5 %** |

Recorded verbatim. This figure is expected to fall below 90 percent and does, for one reason alone: its
guarded `-=` statement is unreachable without a live WebView2 runtime, because `_coreWebView2` and
`_webResourceRequestedHandler` are assigned only inside `InitializeWebViewAsync` (research section 2.4).
The measured value is **non-zero**: the uncovered lines are exactly `:489`, `:490`, and `:491` (the
guarded block and its braces), while the method entry, the null guard at `:488`, and the two
field-nulling statements at `:492` and `:493` are covered — 5 of 8 lines. That is the partial-coverage
fact verified by the `[P5-T12]` fail-before exception dossier at
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/fail-before-exception.webresourcerequested-detach.md`.

### (c) The default `MoveFailureNotifier` delegate `text => MessageBox.Show(text)`

Recorded verbatim: the measured value is **`line-rate="1"` (100 percent)** for the enclosing `.ctor`
entries, and the source line `QuickFiler/Controllers/QfcItemController.MailActions.cs:31` records
`hits="1"`.

This differs from the plan's stated expectation of zero, and the reason is a measurement artefact rather
than a behavioural one: the lambda is a property initializer written on a single source line, so
constructing the delegate registers a hit on the same line that holds the `MessageBox.Show(text)` body.
The body itself is never invoked by any test — every `MoveMailAsync` failure-path test replaces the
notifier through the seam, because invoking it would open a modal dialog, which the headless unit-test
policy of constraint C3 forbids. No separate zero-rate `<method>` element exists for it, so it
contributes no uncovered line.

This statement is the relocation of the pre-existing uncovered `MessageBox.Show` call at
`QuickFiler/Controllers/QfcItemController.MailActions.cs:119-121` as it stood at `BASE_SHA`, where the
baseline Cobertura records `hits="0"` on each of lines 119, 120, and 121. Relocating an
already-uncovered call therefore **reduces no changed line's coverage**; it in fact converts three
uncovered baseline lines into one covered line.

## 4. Coverage for the changed lines is not reduced

### Changed-line coverage (added executable lines only)

| File | Added lines | Executable added | Covered | Percent | Uncovered lines |
|---|---|---|---|---|---|
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 91 | 70 | 70 | 100.0 % | none |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 82 | 36 | 30 | 83.3 % | 94, 95, 101, 489, 490, 491 |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 35 | 18 | 18 | 100.0 % | none |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 13 | 8 | 8 | 100.0 % | none |
| **Total** | 221 | **132** | **126** | **95.5 %** | the six carve-out lines above |

The six uncovered added lines are exactly carve-outs (a) and (b); there is no other uncovered added
production line.

### Per-file line rate, baseline versus post-change

| File | Baseline | Post-change | Direction |
|---|---|---|---|
| `QfcItemController.EventWiring.cs` | 247/303 = 81.52 % | 317/373 = **84.99 %** | up |
| `QfcItemController.ViewerSetup.cs` | 154/181 = 85.08 % | 189/209 = **90.43 %** | up |
| `QfcItemController.MailActions.cs` | 96/125 = 76.80 % | 119/141 = **84.40 %** | up |
| `QfcItemController.FocusAndTheme.cs` | 188/237 = 79.32 % | 198/244 = **81.15 %** | up |
| Every other `QfcItemController.*.cs` partial | unchanged | unchanged | flat |

No file this feature touched lost coverage, and no line that existed at `BASE_SHA` and was covered there
is uncovered now.

Output Summary: Repository-wide line coverage rose from 84.775 % to **84.8323 %**, comfortably above the
80 % floor. All five named new production members measure 100 % line coverage. Changed-line coverage is
126 of 132 added executable lines (95.5 %); the six uncovered lines are exactly the WebView2-runtime
carve-outs (a) and (b). The `MoveFailureNotifier` default delegate measures 100 % rather than the
predicted zero, for the measurement reason recorded above, and its relocation reduces no changed line's
coverage. Coverage for the changed lines is not reduced relative to the baseline.
