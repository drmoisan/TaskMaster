# P1-T8 — Runbook instructs storing the Client ID

Timestamp: 2026-09-30T10-13
Command: Edit docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md (Part B heading, step 10, step 22, YAML sample line 154); the P1-T8 pwsh measurement command verbatim; git diff 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- <runbook path>
EXIT_CODE: 0
Output Summary:
- RB_APPID=0
- RB_CLIENTID=1
- RB_SECRET=2 (step 22 and the YAML sample; at least 2)
- RB_PARTB=0 (a search for "Record the App ID" returns 0 lines; P0-T19 measured 1)
- PARTD_CLIENTID=2 (at least 1)
- The citations at lines 301 and 318 were not edited.

Diff hunks against <BASE-SHA>, verbatim:
```
@@ -96,13 +96,13 @@ permissions listed in step 6 below and skip to step 10.
-### Part B — Record the App ID and generate a private key
+### Part B — Record the Client ID and generate a private key
-10. On the App's settings page that appears after creation, locate the **App ID** in the "About"
-    section near the top of the page and record it. The adjacent **Client ID** is also shown; record
-    it as well, because the `actions/create-github-app-token` action now documents `client-id` as
-    the recommended input and continues to accept the legacy `app-id` input. Neither value is a
-    secret in the cryptographic sense, but this runbook stores the App ID as a repository secret to
+10. On the App's settings page that appears after creation, locate the **Client ID** in the "About"
+    section near the top of the page and record it. The numeric **App ID** shown beside it is not
+    needed: the repair workflow passes the Client ID to the `actions/create-github-app-token` action
+    as its `client-id` input, which the action documents as the recommended input. The Client ID is
+    not a secret in the cryptographic sense, but this runbook stores it as a repository secret to
@@ -126,9 +126,9 @@ permissions listed in step 6 below and skip to step 10.
-22. Create the App ID secret:
+22. Create the Client ID secret:
-    - **Secret** — the App ID value recorded in step 10
+    - **Secret** — the Client ID value recorded in step 10
@@ -151,7 +151,7 @@ permissions listed in step 6 below and skip to step 10.
-        app-id: ${{ secrets.DEPENDABOT_REPAIR_APP_ID }}
+        client-id: ${{ secrets.DEPENDABOT_REPAIR_APP_ID }}
```
