# [P0-T15] Baseline Pinned-Surface Inventory

Timestamp: 2026-08-26T09-05

Task: [P0-T15]
Feature: docs/features/active/quickfiler-bug-family-446

## 1. `[TestMethod]` Count Across the Three Gate Test Partials

Command: `grep -c "\[TestMethod\]" "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs" "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs" "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs"`
EXIT_CODE: 0

| File | `[TestMethod]` count |
| --- | --- |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | 10 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | 10 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` | 3 |
| **Total** | **23** |

The observed total is **23**, which equals the value the plan expects on the tree that carries
PR #610 and is at least the `23` floor the acceptance condition states. No gate test has been
deleted.

## 2. `GetConstructor` Count

Command: `grep -c "GetConstructor" "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs"`
EXIT_CODE: 0
Output: `4`

The literal counted is `GetConstructor`. The count of **4** corresponds to the four-step
descending fallback chain at `:26-156` (8-type, 7-type, 6-type, 5-type). `[P1-T1]` replaces that
chain with a single exact lookup, and `[P4-T20]` asserts the post-change count is `1`; recording
`4` here is what makes that post-change `1` a real change rather than a vacuous match.

## 3. SHA-256 of the `DequeueAsync_BelowThresholdItemsAreDiscarded` Body Text

Extraction rule (stated here so `[P4-T14]` can recompute it identically): the body text is the
line carrying `public async Task DequeueAsync_BelowThresholdItemsAreDiscarded()` through the
first following line whose content is exactly eight spaces plus a closing brace, inclusive, with
the selected lines joined by a single LF and hashed as UTF-8 bytes. A byte comparison over this
extraction is used rather than a line-range diff because `[P1-T1]`'s helper rewrite shifts the
method's line numbers.

- Start line at baseline: `299`
- End line at baseline: `310`
- Line count: `12`
- **SHA256:** `4cd6c2650d106d987d493b1fcb42c5a0313d4a419a3fe8f3b2369fff5c661700`

The digest is 64 hexadecimal characters.

Baseline body text, reproduced for the record:

```csharp
        public async Task DequeueAsync_BelowThresholdItemsAreDiscarded()
        {
            var item = CreateMailItem("discard", "entry-discard");
            object gate = CreateGate(
                new Queue<MailItem>(new[] { item }),
                new Dictionary<MailItem, long> { [item] = 899 }
            );

            IList<MailItem> result = await DequeueAsync(gate, 1, 0, CancellationToken.None);

            result.Should().BeEmpty();
        }
```

## Output Summary

`[TestMethod]` total across the three gate test partials: **23** (10 + 10 + 3).
`GetConstructor` count in `QfcStreamingDequeueConfidenceGateTests.cs`: **4**.
SHA-256 of the `DequeueAsync_BelowThresholdItemsAreDiscarded` body text:
`4cd6c2650d106d987d493b1fcb42c5a0313d4a419a3fe8f3b2369fff5c661700` (64 hex characters).
All three acceptance conditions satisfied.
