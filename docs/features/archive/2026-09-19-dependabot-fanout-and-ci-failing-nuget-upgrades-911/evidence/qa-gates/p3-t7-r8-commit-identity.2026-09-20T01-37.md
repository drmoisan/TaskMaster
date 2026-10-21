# R8 — The Repair Commit Identity Is Derived at Run Time

- Timestamp: 2026-09-20T09-00-20
- Task: [P3-T7]
- Finding: R8, Major, decision D3
- EXIT_CODE: 0

## The Full Rewritten Commit-Step Block, Verbatim

`.github/workflows/dependabot-repair.yml:110-139`:

```yaml
      - name: Commit and push the repair onto the Dependabot branch
        if: steps.repair.outputs.written-count != '0'
        shell: pwsh
        env:
          GH_TOKEN: ${{ steps.app-token.outputs.token }}
        run: |
          # The commit identity is derived at run time, not written as a literal. GitHub
          # resolves commits/<sha>.author.login by matching the commit author email to an
          # account, and a GitHub App bot's noreply address is
          # <bot-user-id>+<slug>[bot]@users.noreply.github.com where the numeric part is the
          # BOT USER'S id, not the app id. Neither value is knowable when this file is
          # authored, and a hand-written address matches no account, resolves author.login to
          # null, and makes AC18 unsatisfiable. Both reads are guarded so a wrong assumption
          # fails this step with a named error instead of producing a silent bad identity.
          $slug = '${{ steps.app-token.outputs.app-slug }}'
          if ([string]::IsNullOrWhiteSpace($slug)) {
            throw 'dependabot-repair: the token step published no app-slug output, so the commit identity cannot be derived. See issue 914.'
          }
          $botLogin = $slug + '[bot]'
          $botUserId = gh api "/users/$([uri]::EscapeDataString($botLogin))" --jq .id
          if ([string]::IsNullOrWhiteSpace($botUserId)) {
            throw "dependabot-repair: the users API returned no id for $botLogin, so the commit identity cannot be derived. See issue 914."
          }
          git config user.name $botLogin
          git config user.email "$botUserId+$botLogin@users.noreply.github.com"
          git add --update -- '*.csproj' '*/packages.config' '*/app.config'
          git commit -m 'chore(deps): repair manifest and project-file consistency'
          git push origin "HEAD:$env:HEAD_BRANCH"
```

The step already carried `GH_TOKEN` in its `env:` block, so the `gh api` call authenticates with
the same installation token the push uses.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| Occurrences of the literal `dependabot-repair[bot]@users.noreply.github.com` | exactly **0** | **0** | PASS |
| Explicit empty-value guards, one per derived value | exactly **2** | **2** | PASS |
| `steps.app-token.outputs.app-slug` present | yes | 1 | PASS |
| `users/` present | yes | 1 | PASS |

### The Two Guards, Quoted

**Guard 1 — the slug:**

```powershell
          if ([string]::IsNullOrWhiteSpace($slug)) {
            throw 'dependabot-repair: the token step published no app-slug output, so the commit identity cannot be derived. See issue 914.'
          }
```

**Guard 2 — the bot user id:**

```powershell
          if ([string]::IsNullOrWhiteSpace($botUserId)) {
            throw "dependabot-repair: the users API returned no id for $botLogin, so the commit identity cannot be derived. See issue 914."
          }
```

Each names the value that was missing, names the consequence, and cites #914. Neither falls back
to a literal.

## Why the Address Is Derived and Not Written

The review's own recommendation named the wrong quantity and the plan corrected it before
execution. A GitHub App bot's noreply address is

```
<bot-user-id>+<slug>[bot]@users.noreply.github.com
```

where the numeric part is the **bot user's** id, not the **app** id. The two are different
numbers and neither is knowable when this file is authored. That is why the step resolves the id
through `gh api "/users/<slug>%5Bbot%5D" --jq .id` at run time rather than carrying any literal.

`[uri]::EscapeDataString` is what encodes the square brackets of `<slug>[bot]` into a valid path
segment.

## Assumption of Record

Decision **D3** records one assumption: that `actions/create-github-app-token@v3` publishes an
`app-slug` output.

It is **not verifiable in this worktree and not verifiable without a live run**. [P0-T13]
recorded zero repository Actions secrets, so the token step cannot execute at all from this
state.

Guard 1 is what converts a wrong assumption from a silent bad commit identity into a loud step
failure: if the output does not exist, the expression expands to the empty string, the guard
throws with a named error, and the job fails visibly rather than committing under an address that
resolves to null. Issue **#914** is where the assumption is settled.

## Gate Rule 20 — Verification Route and Residual

**Verified without a live run:**

- by [P3-T1]'s `R8- derives` assertion, which failed before this edit because the file contained
  no reference to `app-slug`, and which is green after it. That test also asserts the hand-written
  literal is **absent**, composing it from three fragments so the test file is not itself a match
  for a repository search;
- by [P3-T9]'s actionlint pass, which validates the step's YAML and expression syntax statically.

**Unverifiable until the #914 credential exists:**

- that `actions/create-github-app-token@v3` publishes `app-slug` at all — decision D3's
  assumption of record;
- that the resolved bot user id produces a commit whose `author.login` ends `[bot]` and is not
  `github-actions[bot]`, which is AC18's stated acceptance;
- that `gh api` resolves `/users/<slug>[bot]` for this particular App's bot account.

Nothing in this cycle calls the GitHub API for any of the three.

## Output Summary

The commit step reads the app slug from the token step, resolves the bot user id through the
users API, and composes the address from both. Zero occurrences of the hand-written literal, two
explicit empty-value guards each throwing a named error citing #914, and the `app-slug` and
`users/` references both present.
