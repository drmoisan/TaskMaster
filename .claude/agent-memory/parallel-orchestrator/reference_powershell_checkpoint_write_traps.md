---
name: powershell-checkpoint-write-traps
description: Three mechanics that silently corrupt a checkpoint read-modify-write on this surface — an [ordered] hashtable with integer-like keys resolves POSITIONALLY, a single-element pipeline result collapses to a string so [0] yields a CHARACTER, and a bash heredoc breaks on long PowerShell bodies
metadata:
  type: reference
---

Applying a mutation to `artifacts/orchestration/parallel-orchestrator-state.json` means a
PowerShell read-modify-write through a scratchpad `.ps1`. Two mechanics bite there and
neither announces itself.

**1. `[ordered]@{ 285 = 4 }` indexed by an integer resolves POSITIONALLY, not by key.**
`$h[285]` on an `OrderedDictionary` is an INDEX lookup, so it returns `$null` for any key
value larger than the entry count. Observed 2026-09-01 applying `/parallel-add 633`: a
`$assignments = [ordered]@{ 285 = 4; 633 = 5; 646 = 6; 656 = 7 }` recolor map was iterated
with `foreach ($key in $assignments.Keys) { ... index = $assignments[$key] ... }` and wrote
four cohort rows with `index: null`. The MCP validator caught it —
`cohorts[12] index must be a non-negative integer; found: None` — but nothing before it did,
and `item_keys` was correct on the same rows because that value came from `[int]$key`
directly. Use a plain `@{}` hashtable, or `[string]` keys, or index with `.Item($key)`. A
regular `@{}` hashtable does NOT have this behavior; only `[ordered]` does.

**1b. A single-element pipeline result collapses to a SCALAR, so `[0]` returns a CHARACTER.**
`@($a | Where-Object {...}) | Sort-Object` yields a plain `[string]` when exactly one item
matches, because `Sort-Object` re-emits a lone object unwrapped even though the `@()` was
applied upstream. Indexing that with `[0]` then returns the string's FIRST CHARACTER.
`.Count` still reports `1`, so a count guard does not catch it. Observed 2026-09-01 applying
`/parallel-add 670`: the `285~670` edge has exactly one exact path intersection,
`.claude/agent-memory/orchestrator/MEMORY.md`, and the recorded `detail` came out as
`. ~ .`. Every other edge that write produced had 3 or more intersections and was correct,
which is why it presented as one odd row rather than as a systematic failure.

The failure is doubly deceptive: a bare `.` looks like a REAL derivation artifact — a
repository-root path token that would match everything and would plausibly explain a complete
conflict graph — so the natural reaction is to go investigate the radius rather than the
script. Confirm first that no item actually carries the token
(`$i['blast_radius']['paths'] -contains '.'`); on this run none did. Wrap the whole expression:
`$inter = @(@($b | Where-Object {...}) | Sort-Object)`, or use `Select-Object -First 1`.
The schema validator cannot catch this: invariant 15 constrains only `a`, `b` and `reason`, so
a garbage `detail` passes validation cleanly.

**1c. The collapse point is usually the function `return`, and the obvious fix over-corrects.**
The `@()` wrapping in 1b protects the expression but NOT the handoff: `return $inter` from a
PowerShell function UNROLLS the array to the pipeline, so a 1-element array becomes a scalar
string at the call site no matter how carefully the expression was wrapped. Observed 2026-09-01
building the `/parallel-add 287` conflict harness: a `Get-ExactOverlap` helper whose body already
read `@(@($a | Where-Object {...}) | Sort-Object)` still reported `first=.` and `first=s` on the
three pairs with exactly one intersection, reproducing the 670 symptom exactly.

Fix it at the RETURN with the comma operator — `return ,$inter` — and then do NOT also wrap the
call site. Applying both produces an array-of-array: `.Count` reads `1` for every pair regardless
of the real overlap, and `[0]` yields the whole inner array, which string-concatenates into a
space-joined run of paths. That reads as a plausible multi-path detail, so it is harder to spot
than the character case it replaced. One fix or the other, never both.

Validate the harness before trusting it by replaying every recorded edge and printing only
mismatches: a correct harness reproduces all of them silently, and the per-pair overlap counts
then vary (0 for a `module_overlap`-only edge, 3 for a heavily shared pair) instead of being
uniformly 1, which is the tell that distinguishes a working script from either failure mode.

**2. A bash heredoc breaks on long PowerShell bodies.** `cat > f.ps1 <<'EOF'`
is a quoted heredoc and should suppress expansion, but a PowerShell escape such as
``"line1`nline2"`` still aborts the whole call with
``unexpected EOF while looking for matching ` ' ` ``, pointing at a line number inside the
body. Two backticks pair into what the shell treats as a command substitution and swallow
the intervening quotes. Write PowerShell strings without backtick escapes — join with
`' ;; '` instead of a newline escape.

**Backtick avoidance is NOT sufficient, and the chunked-append mitigation is the real one.**
Observed 2026-09-01 on `/parallel-add 670`: a roughly 95-line body containing NO backtick at
all still died with the identical `unexpected EOF while looking for matching ' ` error
reported at a line past the end of the body, while a shorter script in the SAME session that
DID contain a backtick escape ran fine. So body length, not the backtick alone, is the
better predictor. Do not spend turns hunting for an unbalanced quote: go straight to building
the script as four to six `cat >>` appends of roughly 20 lines each, echoing `wc -l` after
each so a failure localizes immediately. That worked first time where the single heredoc had
failed, and it costs one extra tool call rather than a debugging cycle.

Note the Write tool is not an alternative: `.ps1` is on the pre-implementation gate's blocked
extension list and is denied everywhere, including the temp scratchpad, so the heredoc is the
only route. See [[preimplementation-gate-scope]].

**Always follow the write with `mcp__drm-copilot__validate_orchestration_artifacts`**, and
take a BASELINE validation before the operation starts so a pre-existing failure from a
concurrent add is not misattributed. Both traps above produce a file that parses as valid
JSON, so only the schema validator distinguishes them from a correct write. See
[[parallel-run-execution-playbook]] and [[blast-radius-powershell-calling-convention]].
