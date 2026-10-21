# P1-T4 — Post-Split Citation Stability for Members At or Above Line 409

Timestamp: 2026-08-26T08-56

Command: `grep -nE "ProcessInboundAsync|OnHostMessageReceived|HandleArrowKeyAsync|ExpandLeafAsync|BindRowsAsync|SelectFirstRow|_selectedRowId = null;|SegmentDoubleClick" QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, with each member's closing line confirmed by `sed -n '<start>,<end>p' QuickFiler/Controllers/BreadcrumbBridgeRouter.cs | cat -n`

EXIT_CODE: 0

## Output Summary

**Zero divergence.** All seven cited locations hold at their expected line numbers after the `P1-T2`
split. Every line number cited by Phases 2 through 6 for a member at or above `:409` is therefore
still correct, and no correction propagates into the remainder of the plan.

`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` now measures **410** lines.

| # | Member / anchor | Expected | Observed | Match |
|---:|---|---|---|:--:|
| 1 | `ProcessInboundAsync` opens | `:229` | opens at **229** | YES |
| 2 | `SegmentDoubleClick` arm of `ProcessInboundAsync` | `:241-247` | **241-247** | YES |
| 3 | `OnHostMessageReceived` | `:266-277` | **266-277** | YES |
| 4 | `HandleArrowKeyAsync` | `:304-339` | **304-339** | YES |
| 5 | `ExpandLeafAsync` | `:364-408` | **364-408** | YES |
| 6 | `BindRowsAsync` ends with `_selectedRowId = null;` | `:136` | statement at **136** | YES |
| 7 | `SelectFirstRow` opens | `:192` | opens at **192** | YES |

### How each end-of-range was determined

Line numbers were read from `grep -n` output and from `cat -n`-numbered windows rather than counted by
eye, so no figure below is an estimate.

- **Row 1.** `grep -n` reports `229:        public async Task ProcessInboundAsync(string json)`.
- **Row 2.** `grep -n` reports `241:                case BreadcrumbMessageTypes.SegmentDoubleClick:`.
  The numbered window from 241 shows the arm's terminating `break;` as its 7th line — line 247 — with
  `case BreadcrumbMessageTypes.SegmentActivate:` beginning at 248. The arm is 241-247. Its null-forgiving
  dereference `row.CollapseAfter(message.SegmentIndex!.Value)` sits at 242, exactly where `P2-T4` will
  replace it.
- **Row 3.** `grep -n` reports `266:        private async void OnHostMessageReceived(object? sender,
  string json)`. The numbered window from 266 shows the closing `}` as its 12th line — line 277. The
  method is 266-277 and contains exactly one catch clause, `catch (BreadcrumbMessageException)`, at
  line 272, which `P2-T7` re-checks.
- **Row 4.** `grep -n` reports `304:        private async Task HandleArrowKeyAsync(BreadcrumbRow row,
  string key)`. The numbered window from 336 shows the `default:` arm's `log.Error` at 336, its `break;`
  at 337, the switch's closing `}` at 338 and the method's closing `}` at 339, with `HandleUpArrow`
  beginning at 341. The method is 304-339.
- **Row 5.** `grep -n` reports `364:        private async Task ExpandLeafAsync(BreadcrumbRow row)`. The
  numbered window from 405 shows the catch block's closing `}` at 407 and the method's closing `}` at
  408, immediately followed by the class-closing `}` at 409 and the namespace-closing `}` at 410. The
  method is 364-408, and it is now the LAST member of the primary file — which is the direct
  consequence of `P1-T2` relocating the contiguous tail that previously began at 410.
- **Row 6.** The numbered window from 134 shows `AttachSegmentKeys(presentedRows, chains);` at 135,
  `_selectedRowId = null;` at **136**, `DeliverDocument();` at 137 and the method's closing `}` at 138.
  The cited statement is at 136 as expected. `P3-T3` will add the `SelectedFolderPath` reset alongside
  it.
- **Row 7.** `grep -n` reports `192:        public void SelectFirstRow()`.

### Why the numbering is stable

`P1-T2` made exactly two kinds of change to this file: it inserted the keyword `partial` into the type
declaration on line 19 — a within-line edit that shifts nothing — and it deleted the contiguous block
that ran from the blank line 409 through line 594. Deleting a suffix cannot renumber any preceding
line, so every member at or above 408 keeps its pre-split coordinates. The observations above confirm
that reasoning empirically rather than assuming it.

### Consequence

No corrected line value needs to be carried forward. Tasks `P2-T4`, `P2-T7`, `P3-T3`, `P6-T3`, `P6-T5`,
`P6-T7` and `P6-T19` may use their stated pre-split citations for this file unchanged. The twelve
relocated members remain cited by NAME and by their new file
`QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`, as the plan's "Citation Basis" section
requires.
