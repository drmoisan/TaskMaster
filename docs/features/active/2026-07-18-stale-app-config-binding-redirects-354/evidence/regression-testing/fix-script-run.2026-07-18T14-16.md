# Fix Script Run — fix_binding_redirects.py (Issue #354, AC1)

Timestamp: 2026-07-18T14:16:24Z

Command: `python3 docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/scripts/fix_binding_redirects.py` (run from repo root on branch `bug/stale-app-config-binding-redirects-354`)

EXIT_CODE: 0

Output Summary:
- Script corrected stale `<bindingRedirect>` `newVersion` entries in 8 first-party projects' `app.config` files: `QuickFiler.Test` (5), `Tags.Test` (5), `TaskMaster.Test` (5), `TaskTree.Test` (5), `TaskVisualization.Test` (5), `ToDoModel.Test` (5), `UtilitiesCS` (9), `UtilitiesCS.Test` (13), `VBFunctions.Test` (5).
- Corrected packages included: `Microsoft.ApplicationInsights` (-> 3.1.2.115), `Microsoft.Identity.Client` (-> 4.86.1.0), `Microsoft.Identity.Client.Extensions.Msal` (-> 4.86.1.0), `Microsoft.IdentityModel.Abstractions` (-> 8.19.2.0), `Microsoft.IdentityModel.Protocols` (-> 8.19.2.0), `Microsoft.IdentityModel.Protocols.OpenIdConnect` (-> 8.19.2.0), `Microsoft.IdentityModel.Tokens` (-> 8.19.2.0), `Microsoft.IdentityModel.Validators` (-> 8.19.2.0), `Microsoft.IdentityModel.JsonWebTokens` (-> 8.19.2.0), `Microsoft.IdentityModel.Logging` (-> 8.19.2.0), `Microsoft.Web.WebView2.Core` (-> 1.0.4078.44), `System.ClientModel` (-> 1.14.0.0), `System.IdentityModel.Tokens.Jwt` (-> 8.19.2.0).
- Final script line: **`TOTAL: 57`** (matches the 57-stale-redirect count documented in `issue.md`'s Suspected Cause / Notes section).
- Full per-project correction lines: see script stdout, reproduced verbatim below.

```
QuickFiler.Test: app.config Microsoft.ApplicationInsights bindingRedirect -> 3.1.2.115
QuickFiler.Test: app.config Microsoft.Identity.Client bindingRedirect -> 4.86.1.0
QuickFiler.Test: app.config Microsoft.Identity.Client.Extensions.Msal bindingRedirect -> 4.86.1.0
QuickFiler.Test: app.config Microsoft.IdentityModel.Abstractions bindingRedirect -> 8.19.2.0
QuickFiler.Test: app.config System.ClientModel bindingRedirect -> 1.14.0.0
Tags.Test: app.config Microsoft.ApplicationInsights bindingRedirect -> 3.1.2.115
Tags.Test: app.config Microsoft.Identity.Client bindingRedirect -> 4.86.1.0
Tags.Test: app.config Microsoft.Identity.Client.Extensions.Msal bindingRedirect -> 4.86.1.0
Tags.Test: app.config Microsoft.IdentityModel.Abstractions bindingRedirect -> 8.19.2.0
Tags.Test: app.config System.ClientModel bindingRedirect -> 1.14.0.0
TaskMaster.Test: app.config Microsoft.ApplicationInsights bindingRedirect -> 3.1.2.115
TaskMaster.Test: app.config Microsoft.Identity.Client bindingRedirect -> 4.86.1.0
TaskMaster.Test: app.config Microsoft.Identity.Client.Extensions.Msal bindingRedirect -> 4.86.1.0
TaskMaster.Test: app.config Microsoft.IdentityModel.Abstractions bindingRedirect -> 8.19.2.0
TaskMaster.Test: app.config System.ClientModel bindingRedirect -> 1.14.0.0
TaskTree.Test: app.config Microsoft.ApplicationInsights bindingRedirect -> 3.1.2.115
TaskTree.Test: app.config Microsoft.Identity.Client bindingRedirect -> 4.86.1.0
TaskTree.Test: app.config Microsoft.Identity.Client.Extensions.Msal bindingRedirect -> 4.86.1.0
TaskTree.Test: app.config Microsoft.IdentityModel.Abstractions bindingRedirect -> 8.19.2.0
TaskTree.Test: app.config System.ClientModel bindingRedirect -> 1.14.0.0
TaskVisualization.Test: app.config Microsoft.ApplicationInsights bindingRedirect -> 3.1.2.115
TaskVisualization.Test: app.config Microsoft.Identity.Client bindingRedirect -> 4.86.1.0
TaskVisualization.Test: app.config Microsoft.Identity.Client.Extensions.Msal bindingRedirect -> 4.86.1.0
TaskVisualization.Test: app.config Microsoft.IdentityModel.Abstractions bindingRedirect -> 8.19.2.0
TaskVisualization.Test: app.config System.ClientModel bindingRedirect -> 1.14.0.0
ToDoModel.Test: app.config Microsoft.ApplicationInsights bindingRedirect -> 3.1.2.115
ToDoModel.Test: app.config Microsoft.Identity.Client bindingRedirect -> 4.86.1.0
ToDoModel.Test: app.config Microsoft.Identity.Client.Extensions.Msal bindingRedirect -> 4.86.1.0
ToDoModel.Test: app.config Microsoft.IdentityModel.Abstractions bindingRedirect -> 8.19.2.0
ToDoModel.Test: app.config System.ClientModel bindingRedirect -> 1.14.0.0
UtilitiesCS: app.config Microsoft.Identity.Client bindingRedirect -> 4.86.1.0
UtilitiesCS: app.config Microsoft.Identity.Client.Extensions.Msal bindingRedirect -> 4.86.1.0
UtilitiesCS: app.config Microsoft.IdentityModel.Abstractions bindingRedirect -> 8.19.2.0
UtilitiesCS: app.config Microsoft.IdentityModel.Protocols bindingRedirect -> 8.19.2.0
UtilitiesCS: app.config Microsoft.IdentityModel.Protocols.OpenIdConnect bindingRedirect -> 8.19.2.0
UtilitiesCS: app.config Microsoft.IdentityModel.Tokens bindingRedirect -> 8.19.2.0
UtilitiesCS: app.config Microsoft.IdentityModel.Validators bindingRedirect -> 8.19.2.0
UtilitiesCS: app.config System.ClientModel bindingRedirect -> 1.14.0.0
UtilitiesCS: app.config System.IdentityModel.Tokens.Jwt bindingRedirect -> 8.19.2.0
UtilitiesCS.Test: app.config Microsoft.ApplicationInsights bindingRedirect -> 3.1.2.115
UtilitiesCS.Test: app.config Microsoft.Identity.Client bindingRedirect -> 4.86.1.0
UtilitiesCS.Test: app.config Microsoft.Identity.Client.Extensions.Msal bindingRedirect -> 4.86.1.0
UtilitiesCS.Test: app.config Microsoft.IdentityModel.Abstractions bindingRedirect -> 8.19.2.0
UtilitiesCS.Test: app.config Microsoft.IdentityModel.JsonWebTokens bindingRedirect -> 8.19.2.0
UtilitiesCS.Test: app.config Microsoft.IdentityModel.Logging bindingRedirect -> 8.19.2.0
UtilitiesCS.Test: app.config Microsoft.IdentityModel.Protocols bindingRedirect -> 8.19.2.0
UtilitiesCS.Test: app.config Microsoft.IdentityModel.Protocols.OpenIdConnect bindingRedirect -> 8.19.2.0
UtilitiesCS.Test: app.config Microsoft.IdentityModel.Tokens bindingRedirect -> 8.19.2.0
UtilitiesCS.Test: app.config Microsoft.IdentityModel.Validators bindingRedirect -> 8.19.2.0
UtilitiesCS.Test: app.config Microsoft.Web.WebView2.Core bindingRedirect -> 1.0.4078.44
UtilitiesCS.Test: app.config System.ClientModel bindingRedirect -> 1.14.0.0
UtilitiesCS.Test: app.config System.IdentityModel.Tokens.Jwt bindingRedirect -> 8.19.2.0
VBFunctions.Test: app.config Microsoft.ApplicationInsights bindingRedirect -> 3.1.2.115
VBFunctions.Test: app.config Microsoft.Identity.Client bindingRedirect -> 4.86.1.0
VBFunctions.Test: app.config Microsoft.Identity.Client.Extensions.Msal bindingRedirect -> 4.86.1.0
VBFunctions.Test: app.config Microsoft.IdentityModel.Abstractions bindingRedirect -> 8.19.2.0
VBFunctions.Test: app.config System.ClientModel bindingRedirect -> 1.14.0.0
TOTAL: 57
```
