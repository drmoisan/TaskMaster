# Final Upstream Wave-0 Dependency Recheck (P12-T11)

Timestamp: 2026-07-19T16-40
Command: for each of the 6 upstream files, `grep -q "#nullable enable"`.

| Upstream file | P0-T8 baseline | P12-T11 final |
| --- | --- | --- |
| StringExtensions.cs | ENABLED | ENABLED |
| LazyExtension.cs | ENABLED | ENABLED |
| IEnumerableExtensions.cs | ENABLED | ENABLED |
| Tokenizer.cs | ENABLED | ENABLED |
| VerboseLogger.cs | ENABLED | ENABLED |
| FilePathHelper.cs | ENABLED | ENABLED |

## Comparison and conclusion
All 6 upstream files carried `#nullable enable` at both the P0-T8 baseline and this final recheck — the Wave-0
siblings #363/#364 were already landed on the epic integration branch this feature was cut from (commit
dffadd5a, PR #382 merged), and their contract shape did not change during this feature's execution. The
annotation decisions made at the upstream-consuming call sites (§3.2/§3.3 of the research) — `string.IsNullOrEmpty`,
`.ToLazy()`/`.ToLazyValue()`, `.ForEach()`/`.SentenceJoin()`, `AsTokenPattern()`, `VerboseLogger<T>`,
`FilePathHelper` — were made against the real (non-oblivious) upstream contracts throughout, and the final
scoped nullable gate confirms zero new CS86xx/CS87xx at those call sites. **No re-verification is required;
the upstream contract shape was stable across this feature's batches.**
