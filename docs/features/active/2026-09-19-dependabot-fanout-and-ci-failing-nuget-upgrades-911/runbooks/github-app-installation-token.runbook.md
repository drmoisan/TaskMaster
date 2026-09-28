# Runbook — Provision a GitHub App Installation Token for the Dependabot Repair Workflow

Issue: 911. Feature: `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911`.

This runbook covers a one-time setup action. Once completed, the repair workflow operates unattended
indefinitely; no recurring human step remains.

## Cue

Act on this runbook when the orchestrator records an `exception` response for the requirement
"GitHub App installation token available to the Dependabot repair workflow."

The requirement is unautomatable for the following reason. A push made with the default
`GITHUB_TOKEN` does not create a new workflow run; GitHub suppresses this to prevent recursive runs.
The ruleset on `drmoisan/TaskMaster` (ruleset id 18572843) sets
`strict_required_status_checks_policy: true` and requires five check contexts. Consequently, if the
repair workflow pushes its repair commit with `GITHUB_TOKEN`, the five required checks retain their
pre-repair (failed) results against the prior head SHA, and the pull request can never become
mergeable.

The documented fallbacks do not resolve this. GitHub's "Troubleshooting required status checks"
guidance states that checks are evaluated only when the run was triggered by `push`,
`pull_request`, `pull_request_review`, `pull_request_target`, `deployment`, or `deployment_status`.
`workflow_dispatch` is explicitly excluded, which eliminates both `workflow_dispatch` and
`workflow_run` as routes to produce satisfying check runs.

The remaining route is to push with a credential that is not `GITHUB_TOKEN`. Creating a GitHub App,
installing it on the repository, and storing its private key as a repository secret requires
authenticated interaction with the GitHub web user interface. Secrets cannot be created by a
workflow. That interaction is the exception this runbook resolves.

### Why a GitHub App installation token rather than a fine-grained personal access token

Both credentials work: the recursion suppression is keyed on the token identity, so either one
causes the push to produce a new workflow run. The trade-off is stated plainly so the reader can
choose differently.

| Property | Fine-grained personal access token | GitHub App installation token |
|---|---|---|
| Expiry | The token itself expires (maximum one year). | The per-run installation token expires in one hour and is minted fresh each run. The App private key does not expire. |
| Failure mode at expiry | Silent and delayed. The workflow stops working on a date that is not recorded anywhere, and the symptom appears only the next time an upgrade is attempted. | None on a schedule. The private key fails only if it is deliberately revoked. |
| Attribution | Acts as the human account that issued it. That account's name appears on every repair commit and pull request. | Acts as the App. Commits and pull requests carry a distinct bot identity. |
| Blast radius | Bounded by the issuing human account's permissions. | Bounded by the App's declared permissions. |
| Extra dependency | None. | Requires the `actions/create-github-app-token` action in the workflow. |

The deciding factor is the expiry failure mode. A personal access token converts the repair workflow
into a component that stops working on an unrecorded date, and the resulting breakage is silent.
Least privilege and attribution also favour the App. A reader who accepts a calendar reminder for
token rotation, and who is willing to have repair commits attributed to a human account, may
reasonably choose the personal access token instead; in that case, grant it the same two repository
permissions listed in step 6 below and skip to step 10.

## Prerequisites

- An account with **admin** permission on `drmoisan/TaskMaster`, held by the repository owner. Admin
  permission is required to create repository secrets and to install a GitHub App on the repository.
- Sign-in access to `https://github.com` in a web browser, with any configured two-factor
  authentication method available.
- The GitHub CLI (`gh`) installed and authenticated for the verification step
  (`gh auth status` reports an authenticated account with access to `drmoisan/TaskMaster`).
- Local ability to read a downloaded `.pem` file as text, in order to paste its full contents into
  the secret value field.
- A decision, already made, that the App will be owned by the repository owner's personal account
  rather than an organization. `drmoisan/TaskMaster` is a personal-account repository, so the
  personal-account navigation path applies throughout.
- The repair workflow itself is not required to exist yet. This runbook provisions the credential;
  the workflow consumes it.

## Step-by-step Instructions

### Part A — Register the GitHub App

1. Sign in to `https://github.com` as the repository owner.
2. Click the profile picture in the upper-right corner, then select **Settings**.
3. In the left sidebar, select **Developer settings**.
4. In the left sidebar, select **GitHub Apps**.
5. Click **New GitHub App**.
6. Complete the registration form:
   - **GitHub App name** — enter a unique name, for example `taskmaster-dependabot-repair`. The name
     must be unique across GitHub and is limited to 34 characters. The resulting App slug determines
     the bot identity that appears on repair commits.
   - **Homepage URL** — enter `https://github.com/drmoisan/TaskMaster`. A value is required; it is
     not functionally significant for this use.
   - **Webhook** — under the "Webhook" heading, clear the **Active** checkbox. The App does not
     receive events; it is used only to mint tokens. Leaving webhooks active would require a
     reachable webhook URL.
7. Scroll to **Permissions** > **Repository permissions** and set exactly these two, leaving every
   other permission at **No access**:
   - **Contents** — set to **Read and write**. Required to push the repair commit to the Dependabot
     branch.
   - **Pull requests** — set to **Read and write**. Required to update the pull request.

   Do not grant any additional permission. The installation token inherits all permissions granted
   to the installation, so each extra permission directly widens the blast radius of the credential.
   In particular, do not grant **Administration**, **Actions**, **Secrets**, or **Workflows**.
8. Under **Where can this GitHub App be installed?**, select **Only on this account**.
9. Click **Create GitHub App**.

### Part B — Record the App ID and generate a private key

10. On the App's settings page that appears after creation, locate the **App ID** in the "About"
    section near the top of the page and record it. The adjacent **Client ID** is also shown; record
    it as well, because the `actions/create-github-app-token` action now documents `client-id` as
    the recommended input and continues to accept the legacy `app-id` input. Neither value is a
    secret in the cryptographic sense, but this runbook stores the App ID as a repository secret to
    keep the workflow configuration uniform.
11. On the same page, scroll to the **Private keys** section and click **Generate a private key**.
    A `.pem` file downloads automatically. GitHub issues the key in PKCS#1 `RSAPrivateKey` PEM
    format. This is the only opportunity to obtain this key material; GitHub does not display it
    again.
12. Treat the downloaded file as a long-lived credential. Do not commit it to any repository,
    including a private one, and delete the local copy once it has been stored as a repository
    secret in Part D.

### Part C — Install the App on the repository

13. In the left sidebar of the App's settings page, click **Install App**.
14. Next to the repository owner's account, click **Install**.
15. On the installation permission prompt, select **Only select repositories**.
16. In the **Select repositories** dropdown, select `TaskMaster`. Select no other repository.
17. Review the summary of the two requested permissions and click **Install**.

### Part D — Store the credentials as repository secrets

18. Navigate to `https://github.com/drmoisan/TaskMaster`.
19. Click **Settings** on the repository navigation bar.
20. In the sidebar's "Security" section, select **Secrets and variables**, then **Actions**.
21. Select the **Secrets** tab, then click **New repository secret**.
22. Create the App ID secret:
    - **Name** — `DEPENDABOT_REPAIR_APP_ID`
    - **Secret** — the App ID value recorded in step 10
    - Click **Add secret**.
23. Click **New repository secret** again and create the private key secret:
    - **Name** — `DEPENDABOT_REPAIR_APP_PRIVATE_KEY`
    - **Secret** — the entire contents of the downloaded `.pem` file, including the
      `-----BEGIN RSA PRIVATE KEY-----` and `-----END RSA PRIVATE KEY-----` lines and all line
      breaks between them. Open the file in a plain-text editor and copy all of it; a partial paste
      produces a key-parse failure at run time.
    - Click **Add secret**.
24. Delete the downloaded `.pem` file from the local machine and from the browser's downloads
    folder.

### Part E — Consume the token in the repair workflow

25. In the repair workflow, mint the installation token as the first step of the job and use its
    output for every subsequent authenticated operation. The maintained, GitHub-published action for
    this exchange is `actions/create-github-app-token`, currently at major version `v3`:

    ```yaml
    - name: Mint installation token
      id: app-token
      uses: actions/create-github-app-token@v3
      with:
        app-id: ${{ secrets.DEPENDABOT_REPAIR_APP_ID }}
        private-key: ${{ secrets.DEPENDABOT_REPAIR_APP_PRIVATE_KEY }}

    - name: Check out the Dependabot branch
      uses: actions/checkout@v4
      with:
        token: ${{ steps.app-token.outputs.token }}
        ref: ${{ github.event.pull_request.head.ref }}
    ```

    The action exchanges the private key for an installation access token scoped to the
    installation, exposes it as `steps.app-token.outputs.token`, masks it in logs, and revokes it
    when the job completes. The token expires after one hour regardless.

26. Ensure the `actions/checkout` step and the `git push` step both use
    `steps.app-token.outputs.token` rather than `secrets.GITHUB_TOKEN` or the default checkout
    credential. If the checkout step retains the default credential, the persisted push credential
    remains `GITHUB_TOKEN` and the suppression behaviour described in the Cue section reappears even
    though the token was minted correctly.

27. Pin the action by major version tag as shown, consistent with the pinning convention used by the
    repository's existing workflows.

## Verification

Verification must positively assert that the five required check contexts exist on the new head SHA
produced by an App-token push. Confirming only that the workflow reported no error is insufficient:
a run that pushed with the wrong credential produces zero check runs on the new SHA and also reports
no error.

Perform the following after the repair workflow has run once against a Dependabot pull request.

1. Capture the pull request's current head SHA after the repair push. Substitute the pull request
   number for `<PR_NUMBER>`:

   ```
   gh api repos/drmoisan/TaskMaster/pulls/<PR_NUMBER> --jq '.head.sha'
   ```

   Record the returned 40-character SHA as `<HEAD_SHA>`. Confirm it differs from the SHA the pull
   request had before the repair run. If it is unchanged, no push occurred and the remaining checks
   are not meaningful.

2. Assert that the push was made by the App identity rather than by `GITHUB_TOKEN`:

   ```
   gh api repos/drmoisan/TaskMaster/commits/<HEAD_SHA> --jq '.author.login'
   ```

   Expected output is the App's bot login, which is the App slug with a `[bot]` suffix, for example:

   ```
   taskmaster-dependabot-repair[bot]
   ```

   Output of `github-actions[bot]` indicates the push used `GITHUB_TOKEN`; return to step 26 of the
   instructions.

3. Assert that exactly the five required check contexts are present on the new head SHA:

   ```
   gh api repos/drmoisan/TaskMaster/commits/<HEAD_SHA>/check-runs --jq '[.check_runs[].name] | sort'
   ```

   Expected output, in this exact set (sorted):

   ```
   [
     "actionlint / actionlint",
     "build-analyzers / Build with analyzers and code style enforcement",
     "build-nullable / Build with nullable warnings treated as errors",
     "format-check / Verify formatting",
     "mstest-coverage / Run MSTest suite with coverage"
   ]
   ```

   An empty array (`[]`) is the specific failure signature of a `GITHUB_TOKEN` push: the push
   succeeded, no run was created, and the ruleset continues to evaluate the stale results on the
   prior SHA. Any output other than the five names above is a failure of this verification.

4. Assert the count independently, so that a partial set is not mistaken for success:

   ```
   gh api repos/drmoisan/TaskMaster/commits/<HEAD_SHA>/check-runs --jq '.total_count'
   ```

   Expected output:

   ```
   5
   ```

5. Assert that the runs are not parked awaiting manual approval, which is the state that the App
   token is intended to avoid:

   ```
   gh api repos/drmoisan/TaskMaster/commits/<HEAD_SHA>/check-runs --jq '[.check_runs[] | select(.status == "queued" or .status == "in_progress" or .status == "completed")] | length'
   ```

   Expected output is `5`. A value of `action_required` appearing in any run's `conclusion`, or a
   status of `waiting`, indicates the run requires manual approval and that the push was not
   attributed to the App.

6. Once the runs complete, confirm the pull request reports as mergeable against the ruleset:

   ```
   gh pr checks <PR_NUMBER> --repo drmoisan/TaskMaster
   ```

   Expected output lists all five contexts with a `pass` result.

Verification is complete only when steps 1 through 6 all produce the expected output in a single
pass against the same `<HEAD_SHA>`.

## Security Note

- The installation token minted by this App has **write** access to repository contents and pull
  requests on `drmoisan/TaskMaster`. Any workflow step that runs after the minting step, and any
  action invoked by it, executes in a job where that token is reachable. Keep the repair job minimal
  and avoid invoking third-party actions in the same job after the token is minted.
- The App private key is a **long-lived credential that does not expire**. An actor who obtains it
  can mint installation tokens continuously and act as the App against the repository until the key
  is manually revoked. GitHub's guidance is not to hard-code the private key in any application,
  including in a private repository.
- Storage as a GitHub Actions repository secret is the mechanism this runbook uses because the
  consumer is a workflow in the same repository. GitHub's documentation notes that an attacker with
  access to the execution environment can read a private key held in that environment, and
  recommends a key vault for applications that have that option. The mitigation applied here is
  least privilege: only two repository permissions are granted, and the installation is scoped to a
  single repository.
- To revoke, delete the private key from the App's **Private keys** section, or uninstall the App
  from the account. Either action takes effect for subsequent token minting. Revocation is manual;
  no automatic expiry applies.
- Record the App name and its two secret names in the repository's operational documentation so that
  a future maintainer can locate and revoke the credential without discovering it by inspection.

## Source and Citation

Sourcing note: the repository has no callable MCP documentation-retrieval tool at this time, so the
skill's MCP-first clause could not be satisfied for the third-party UI steps in this runbook. Every
UI and CLI step below is sourced web-second from the vendor's current published documentation,
retrieved with `WebFetch` on the capture date shown. This limitation is recorded in the
two-axis-model-selection specification's Out of Scope section and is not resolved here.

- App registration UI navigation (steps 1–9). GitHub Docs — "Registering a GitHub App."
  https://docs.github.com/en/apps/creating-github-apps/registering-a-github-app/registering-a-github-app
  — captured 2026-09-19.
- Private key generation and App ID location (steps 10–12), and the private-key security guidance in
  the Security Note. GitHub Docs — "Managing private keys for GitHub Apps."
  https://docs.github.com/en/apps/creating-github-apps/authenticating-with-a-github-app/managing-private-keys-for-github-apps
  — captured 2026-09-19.
- Installation UI navigation and the "Only select repositories" option (steps 13–17). GitHub Docs —
  "Installing your own GitHub App."
  https://docs.github.com/en/apps/using-github-apps/installing-your-own-github-app
  — captured 2026-09-19.
- Repository secret creation UI navigation (steps 18–24). GitHub Docs — "Using secrets in GitHub
  Actions."
  https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-secrets
  — captured 2026-09-19.
- Token exchange in a workflow, the five-step App-authentication procedure, and the recommendation
  of `actions/create-github-app-token@v3` (steps 25–27). GitHub Docs — "Making authenticated API
  requests with a GitHub App in a GitHub Actions workflow."
  https://docs.github.com/en/apps/creating-github-apps/authenticating-with-a-github-app/making-authenticated-api-requests-with-a-github-app-in-a-github-actions-workflow
  — captured 2026-09-19.
- Action inputs (`client-id` recommended, legacy `app-id` accepted), outputs (`token`,
  `installation-id`, `app-slug`), automatic token revocation, log masking, and one-hour expiry
  (steps 10 and 25). GitHub — `actions/create-github-app-token`, major version v3.
  https://github.com/actions/create-github-app-token — captured 2026-09-19.
- `GITHUB_TOKEN` recursion suppression, its exceptions, and the statement that a GitHub App
  installation access token or a personal access token may be used to trigger events that require a
  token (Cue section, and the trade-off table). GitHub Docs — "Trigger a workflow."
  https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/trigger-a-workflow
  — captured 2026-09-19.
- `gh api` GET request syntax and `--jq` filtering used in the Verification commands. GitHub CLI
  manual — `gh api`. https://cli.github.com/manual/gh_api — captured 2026-09-19.
- Check-runs endpoint `GET /repos/{owner}/{repo}/commits/{ref}/check-runs` and the `total_count`,
  `check_runs[].name`, `status`, and `conclusion` response fields used in the Verification commands.
  GitHub Docs — REST API, "Check runs." https://docs.github.com/en/rest/checks/runs — captured
  2026-09-19.
- The five required check context names, the ruleset id and its
  `strict_required_status_checks_policy: true` setting, and the "Troubleshooting required status
  checks" trigger-event allow-list. Local repository research artifact:
  `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/research/2026-09-19T11-30-dependabot-nuget-upgrade-automation-research.md`
  (sections 2.1, 3.1, and 3.3) — dated 2026-09-19.
