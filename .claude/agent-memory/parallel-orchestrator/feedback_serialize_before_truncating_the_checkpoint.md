---
name: serialize-before-truncating-the-checkpoint
description: json.dump to an open(path,'w') handle truncates the checkpoint BEFORE encoding, so one unserializable value destroys it; build_add_entry returns a raw datetime in `at`, which is exactly that value — serialize to a string first, and recover from the child-worktree snapshot
metadata:
  type: feedback
---

Never write the parallel checkpoint with `json.dump(d, open(path, 'w'))`. Serialize to a
string first and only then touch the file:

```python
text = json.dumps(d, indent=2) + '\n'      # can raise; file untouched
tmp = path + '.tmp'
open(tmp, 'w', encoding='utf-8').write(text)
json.load(open(tmp, encoding='utf-8'))     # parse-back guard
os.replace(tmp, path)                       # atomic
```

**Why:** `open(path, 'w')` truncates on open, and `json.dump` streams — it writes every
chunk it has already encoded before it reaches the value it cannot encode. A single bad
value therefore leaves a valid JSON PREFIX and no tail, and the original is gone. The
`with` block's close even flushes the partial write for you.

**The bad value is not hypothetical — the mutation engine hands it to you.**
`build_add_entry(...)` in `scripts/dev_tools/_parallel_mutation_entries.py` takes a
`clock: Callable[[], datetime]` seam and puts the raw `datetime` straight into the entry's
`at` field. `dataclasses.asdict` preserves it as a `datetime` object. Invariant 16 requires
`at` to be a non-empty STRING in the canonical `yyyy-MM-ddTHH-mm` shape, so the value is
both unserializable and schema-invalid. Format it the moment the entry comes back:
`mut['at'] = entry.at.strftime('%Y-%m-%dT%H-%M')`. Observed 2026-09-07 on `/parallel-add 809`,
where it truncated the `bugs-2026-09-06` checkpoint at exactly `"at": ` — 35223 bytes of
valid prefix, everything from `mutations` onward destroyed.

**What survives such a truncation is the good news, and it is worth knowing before you
panic.** Python dicts serialize in insertion order and the checkpoint's schema-required keys
all sit ahead of the narrative keys, so the prefix carried `objective` through
`conflict_edges` — including every mutation the failed write had already applied to
`items`, `cohorts`, `conflict_edges`, `current_cohort` and `completed_steps`. Recover it by
cutting the text at the first key you lost and re-parsing:

```python
prefix = raw[:raw.index('  "mutations": [')]
d = json.loads(prefix.rstrip().rstrip(',') + '}')
```

**A CHILD WORKTREE HOLDS A SNAPSHOT OF THE SAME RUN.** This is the recovery source and it is
not obvious. Each item's execution worktree carries its own
`artifacts/orchestration/parallel-orchestrator-state.json` frozen at the moment that item
launched, because the parent's checkpoint is copied into the isolated tree. On the 809
incident the item-798 worktree held `bugs-2026-09-06` at `last_updated 2026-09-07T00-40`,
which restored `main_tip_at_seeding`, `fable_policy`, `worktree_reuse_decision`,
`eligibility_at_kickoff`, `skill_receipts`, `mcp_call_receipts` and the item-798 receipts
verbatim. Find them with
`find <repo-root>/.claude/worktrees -name parallel-orchestrator-state.json` and match on
`parallel_slug`; the EARLIEST-launched item holds the earliest snapshot, so prefer the
latest-launched item whose snapshot still predates the loss. The checkpoint is gitignored
(`.gitignore:57` matches `artifacts/`), so git offers nothing.

Everything the snapshot cannot supply — narrative keys written after it froze — is genuinely
gone. Rebuild what durable state supports (`delegation_receipts` reconstruct cleanly from
each `items[]` record's `worktree_created_at`, `worktree_path` and `resolved_model`), mark
every rebuilt entry `reconstructed: true` with its provenance, and record the exact list of
unrecoverable key names in one incident key. Do NOT invent plausible values for the rest:
a fabricated `resolved_at` is worse than a null one, and the key list alone lets a later
reader recover the outcomes from `parallel-status.md`, the merged pull requests, and git log.

**How to apply:** Treat the string-first write as the only accepted form for this file, and
format the engine's `at` at the boundary where the entry is built. Take the pre-operation
baseline validation anyway ([[verify-delivery-before-preparing-an-admission]]) — it is what
tells you the corruption is yours. And prefer writing the script with the Write tool to a
`.txt` file in the scratchpad and running `python <file>.txt` over a Bash heredoc: heredocs
die unpredictably on this repository ([[parallel-run-execution-playbook]]) and `.txt` avoids
the extension gate that blocks `.ps1` and `.json`.
