# TaskMaster

An Outlook add-in and set of supporting libraries to **triage, tag, and file email quickly**; **visualize tasks**; and **apply ML-assisted classifiers** (spam, triage, folder/category predictions) — all integrated into the Outlook Ribbon.

> **Target platform:** Windows • Outlook desktop (Microsoft 365/2019+) • .NET Framework **4.8.1** • Visual Studio (recommended: 2022) with **VSTO** development tools.

---

## Contents

* [Features](#features)
* [Solution layout](#solution-layout)
* [Getting started](#getting-started)
* [Build & debug (VSTO add-in)](#build--debug-vsto-add-in)
* [Running the tests](#running-the-tests)
* [Configuration & storage](#configuration--storage)
* [Common issues](#common-issues)
* [Contributing & branches](#contributing--branches)
* [License](#license)

---

## Features

* **Outlook Ribbon integration** via the `TaskMaster` VSTO add-in

  * Launch **Quick Filer** for keyboard-driven email filing and queueing
  * **SpamBayes**: trainable Bayesian spam classifier (enable/disable, save/load state)
  * **Triage**: A/B/C trainable classifier; precision controls; filter viewer
  * **Tags**: apply/view people, project, topic tags via a dedicated viewer
  * **Task Visualization**: tree/graph views to explore projects, people, topics

* **ML & analytics helpers**

  * Engines and utilities in `UtilitiesCS.EmailIntelligence` (SpamBayes, Triage, folder/category predictors)
  * Uses **Microsoft.ML**, **Deedle**, and related data/ML dependencies
  * Optional WebView2 hosting and Graph client utilities included in `UtilitiesCS`

* **Outlook & filesystem tooling**

  * Rich **Outlook interop** helpers (Explorer/Inspector hooks, folder utilities, PST helpers)
  * File/save helpers for model state and diagnostics (JSON snapshots, staging areas)

---

## Solution layout

Top-level Visual Studio solution: **`TaskMaster.sln`**

> Most projects target **.NET Framework 4.8.1** and build as **Class Library**; the VSTO add-in (`TaskMaster`) is a COM add-in loaded by Outlook.

| Project                            | Type               | Purpose (one-liner)                                                                                                                                      |
| ---------------------------------- | ------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **TaskMaster**                     | VSTO Add-in        | Outlook add-in entrypoint (`ThisAddIn`, Ribbon wiring) orchestrating modules.                                                                            |
| **TaskMaster.Test**                | MSTest             | Unit tests for add-in-level orchestration and contracts.                                                                                                 |
| **QuickFiler**                     | Library (WinForms) | Keyboard-centric filing UI (Explorer integration, queues, viewers, controllers).                                                                         |
| **QuickFiler.Test**                | MSTest             | Unit tests for QuickFiler controllers and helpers.                                                                                                       |
| **TaskVisualization**              | Library (WinForms) | Task/project/people/topic viewers, auto-assign context, Outlook links.                                                                                   |
| **TaskVisualization.Test**         | MSTest             | Unit tests for visualization components.                                                                                                                 |
| **TaskTree**                       | Library            | Core tree models and operations used by viewers and classifiers.                                                                                         |
| **Tags**                           | Library (WinForms) | Tag controller & viewer for applying people/project/topic labels.                                                                                        |
| **Tags.Test**                      | MSTest             | Unit tests for tag logic.                                                                                                                                |
| **ToDoModel**                      | Library            | To-do, people/project data model; email capture, PST helpers, sorting utilities.                                                                         |
| **ToDoModel.Test**                 | MSTest             | Unit tests for to-do model & utilities.                                                                                                                  |
| **UtilitiesCS**                    | Library            | Shared utilities: Outlook extensions, threading, serialization, **EmailIntelligence** (SpamBayes/Triage/multiclass engines), log4net, ML, Graph helpers. |
| **UtilitiesCS.Test**               | MSTest             | Unit tests for UtilitiesCS (mail helpers, file system helpers, etc.).                                                                                    |
| **UtilitiesSwordfish.NET.General** | Library            | General helper library (collections, misc utilities).                                                                                                    |
| **UtilitiesSwordfish.Test**        | MSTest             | Tests for Swordfish helpers.                                                                                                                             |
| **SVGControl**                     | Library (WinForms) | SVG rendering control used across UI projects.                                                                                                           |
| **SVGControl.Test**                | MSTest             | Tests for SVG control.                                                                                                                                   |
| **VBFunctions**                    | Library            | Small VB-style helper functions (ported to C#).                                                                                                          |
| **VBFunctions.Test**               | MSTest             | Tests for VB helper wrappers.                                                                                                                            |

> The repo also contains `UtilitiesSwordfish/Swordfish.NET.sln` for the standalone sub-solution.

---

## Getting started

### Prerequisites

* **Windows 10/11**
* **Microsoft Outlook** (Microsoft 365 or 2019+) desktop installed
* **Visual Studio 2022** (recommended) with:

  * **Office/SharePoint development** workload (installs VSTO tools)
  * .NET desktop development
* **.NET Framework Developer Pack 4.8.1**
* Internet access to restore NuGet packages

### Setup

1. **Clone** the repository and open `TaskMaster.sln` in Visual Studio.
2. **Restore NuGet packages** (VS usually auto-restores on open).
3. **Build** the solution (Debug, Any CPU).
   Projects use packages such as `Microsoft.ML`, `log4net`, `Newtonsoft.Json`, `ObjectListView.Official`, `Svg`, `Microsoft.Graph`, `WebView2`.

---

## Build & debug (VSTO add-in)

Because `TaskMaster` is an Outlook add-in (OutputType=Library, VSTO), you debug by launching **Outlook** under the debugger:

1. Right-click **TaskMaster** → **Properties** → **Debug**.
2. Choose **Start external program** and browse to your Outlook executable, e.g.:
   `C:\Program Files\Microsoft Office\root\Office16\OUTLOOK.EXE`
3. Press **F5**. Outlook starts, loads the add-in, and shows the **TaskMaster** Ribbon.
4. Set breakpoints in `TaskMaster`, `QuickFiler`, `TaskVisualization`, etc.

> If Outlook is already running, close it before F5. For 64-bit Office, ensure the interop assemblies match your Office bitness.

---

## Running the tests

The solution uses **MSTest v3** with **Microsoft.Testing.Platform**.

* In Visual Studio, open **Test Explorer** → **Run All**.
* Or run at the command line via `vstest.console.exe` (installed with VS) if needed.

Test projects include:
`*.Test` projects for TaskMaster, QuickFiler, TaskVisualization, TaskTree, ToDoModel, UtilitiesCS, UtilitiesSwordfish, SVGControl, VBFunctions.

---

## Configuration & storage

* Logging: **log4net** (`log4net.config`), enabled in `TaskMaster` (see assembly attribute).
* Classifier state (e.g., **SpamBayes**, **Triage**):

  * Controllable from the Ribbon (Enable/Disable, **Save**, **Save Local**, **Get Save Location**).
  * Saved via file helpers in `UtilitiesCS` (JSON serialization + file path helper).
* UI scaling: TreeListView and forms adjust pixel constants at startup for DPI scaling.
* Packages bring optional capabilities (e.g., **Microsoft.Graph**, **WebView2**) used where available.

> **Privacy note:** The add-in processes Outlook items locally. Saved classifier snapshots and diagnostics are written to your local filesystem; review the save location from the Ribbon before sharing files.

---

## Common issues

* **Add-in not loading**

  * Verify Outlook started under the debugger from the `TaskMaster` project.
  * Check **File → Options → Add-ins** (COM Add-ins) in Outlook for load failures.
  * Ensure VSTO runtime and Office PIA are installed (VS Office workload).

* **Interop / bitness mismatch**

  * Match Outlook bitness (x64 vs x86) with installed interop assemblies and any native dependencies.

* **NuGet restore problems**

  * Clear `~\AppData\Local\NuGet\Cache` and re-restore; confirm internet access to feeds.

* **Permissions / file saves**

  * If saving classifier state fails, review the save path (Ribbon → “Get Save Location”) and file system permissions.

---

## Contributing & branches

* Current working branch (from the project brief): **`feature/PstCompare`** at commit `273feb632f56`.
* Standard GitHub workflow: feature branches → PR → review → merge.

Coding guidelines (high-level):

* Prefer small, testable controllers and viewers.
* Keep Outlook interop isolated behind helper classes and adapters.
* Put shared logic in `UtilitiesCS` (and `UtilitiesSwordfish.NET.General`) to reduce duplication.
* Add MSTest coverage for regressions, especially in email parsing, model serialization, and keyboard handlers.

---

## License

TaskMaster — Outlook add-in and supporting libraries for email triage, tagging, and task visualization.

SPDX-License-Identifier: GPL-3.0-or-later
Copyright (C) 2025 Dan Moisan

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by the
Free Software Foundation, either version 3 of the License, or (at your
option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with
this program in a file named “COPYING”.  If not, see <https://www.gnu.org/licenses/>.

---

### Short glossary

* **Quick Filer** — Keyboard-driven UI to rapidly queue and file emails into folders.
* **SpamBayes** — Trainable Bayesian spam classifier with Ribbon controls (enable/save/load).
* **Triage** — A/B/C classifier for prioritization; precision tuning and a filter viewer.
* **Tags** — People/project/topic tagging UI and controllers.
* **Task Visualization** — Views to explore tasks across projects/people/topics.
* **UtilitiesCS** — Shared ML, Outlook, threading, and file helpers (includes EmailIntelligence).
* **SVGControl** — Custom control for rendering SVG assets in WinForms.

---

If you want this README saved into the repo (or tailored for end-user vs developer audiences), tell me the audience and I’ll produce that variant.
