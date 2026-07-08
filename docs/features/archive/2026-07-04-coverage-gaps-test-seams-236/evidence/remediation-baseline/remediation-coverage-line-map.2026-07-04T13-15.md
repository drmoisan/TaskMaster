# Remediation Coverage Line Map

Timestamp: 2026-07-04T18:30:39Z
Command: PowerShell parser over final-coverage.cobertura.xml plus git diff --unified=0 HEAD for issue #236 production files
EXIT_CODE: 0
Output Summary: Parsed repository line coverage 45.12%. Generated uncovered changed/new line classifications for 10 issue #236 production files.

CoverageInput: docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\qa-gates\final-coverage.cobertura.xml
ChangedLineSource: tracked files use git diff --unified=0 HEAD; untracked files use all current file lines as new code.
Classifications: testable, seam-adjustment-required, legacy-unchanged, non-executable

## QuickFiler/Controllers/EfcHomeController.cs

ChangedOrNewLineCount: 81
CoverageEntry: FOUND
UncoveredChangedOrNewLineCount: 43

| Line | Hits | Classification | Text |
| --- | ---: | --- | --- |
| 21 |  | non-executable | `public partial class EfcHomeController : IFilerHomeController` |
| 27 |  | non-executable | `#region Constructors, Initializers, and Destructors` |
| 29 |  | non-executable | `public EfcHomeController(` |
| 31 |  | non-executable | `System.Action parentCleanup,` |
| 32 |  | non-executable | `MailItem mail = null` |
| 34 | 0 | seam-adjustment-required | `: this(globals, parentCleanup, new EfcHomeControllerDependencies(), mail) { }` |
| 91 | 0 | non-executable | `{` |
| 92 | 0 | testable | `return await CreateAsync(` |
| 93 | 0 | seam-adjustment-required | `globals,` |
| 94 | 0 | testable | `parentCleanup,` |
| 95 | 0 | testable | `new EfcHomeControllerDependencies(),` |
| 96 | 0 | testable | `mail` |
| 97 | 0 | testable | `);` |
| 98 | 0 | non-executable | `}` |
| 99 |  | non-executable | `` |
| 100 |  | non-executable | `internal static async Task<EfcHomeController> CreateAsync(` |
| 101 |  | non-executable | `IApplicationGlobals globals,` |
| 102 |  | non-executable | `System.Action parentCleanup,` |
| 103 |  | non-executable | `EfcHomeControllerDependencies dependencies,` |
| 104 |  | non-executable | `MailItem mail = null` |
| 105 |  | non-executable | `)` |
| 132 | 0 | non-executable | `{` |
| 133 | 0 | testable | `return await LoadFinderAsync(` |
| 134 | 0 | seam-adjustment-required | `globals,` |
| 135 | 0 | testable | `parentCleanup,` |
| 136 | 0 | testable | `new EfcHomeControllerDependencies(),` |
| 137 | 0 | testable | `mail` |
| 138 | 0 | testable | `);` |
| 139 | 0 | non-executable | `}` |
| 140 |  | non-executable | `` |
| 141 |  | non-executable | `internal static async Task<EfcHomeController> LoadFinderAsync(` |
| 142 |  | non-executable | `IApplicationGlobals globals,` |
| 143 |  | non-executable | `System.Action parentCleanup,` |
| 144 |  | non-executable | `EfcHomeControllerDependencies dependencies,` |
| 145 |  | non-executable | `MailItem mail = null` |
| 146 |  | non-executable | `)` |
| 247 |  | non-executable | `private static List<MailItem> LoadToList(` |
| 248 |  | non-executable | `IApplicationGlobals globals,` |
| 249 |  | non-executable | `MailItem mail,` |
| 250 |  | non-executable | `EfcHomeControllerDependencies dependencies` |
| 251 |  | non-executable | `)` |
| 284 |  | non-executable | `private EfcHomeControllerDependencies _dependencies;` |
| 285 |  | non-executable | `` |

## QuickFiler/Controllers/EfcHomeController.Metrics.cs

ChangedOrNewLineCount: 51
CoverageEntry: FOUND
UncoveredChangedOrNewLineCount: 48

| Line | Hits | Classification | Text |
| --- | ---: | --- | --- |
| 1 |  | non-executable | `using System;` |
| 2 |  | non-executable | `using System.Collections.Generic;` |
| 3 |  | non-executable | `using System.Linq;` |
| 4 |  | non-executable | `using QuickFiler.Controllers;` |
| 5 |  | non-executable | `using QuickFiler.Helper_Classes;` |
| 6 |  | non-executable | `using UtilitiesCS;` |
| 7 |  | non-executable | `` |
| 8 |  | non-executable | `namespace QuickFiler` |
| 9 |  | non-executable | `{` |
| 10 |  | non-executable | `public partial class EfcHomeController` |
| 11 |  | non-executable | `{` |
| 12 |  | non-executable | `public void QuickFileMetrics_WRITE(` |
| 13 |  | non-executable | `string filename,` |
| 14 |  | non-executable | `string selectedFolder,` |
| 15 |  | non-executable | `List<MailItemHelper> moved` |
| 16 |  | non-executable | `)` |
| 19 | 0 | non-executable | `{` |
| 20 | 0 | testable | `var curDateText = DateTime.Now.ToString("MM/dd/yyyy");` |
| 21 | 0 | testable | `var curTimeText = DateTime.Now.ToString("hh:mm");` |
| 22 | 0 | testable | `var dataLineBeg = curDateText + "," + curTimeText + ",";` |
| 23 |  | non-executable | `` |
| 24 | 0 | testable | `var duration = _stopWatch.Elapsed.Seconds;` |
| 25 | 0 | testable | `duration /= moved.Count;` |
| 26 | 0 | testable | `var durationText = duration.ToString("##0");` |
| 27 | 0 | testable | `var durationMinutesText = (duration / 60d).ToString("##0.00");` |
| 28 |  | non-executable | `` |
| 29 | 0 | testable | `var dataLines = moved` |
| 30 | 0 | testable | `.Select(itemInfo =>` |
| 31 | 0 | testable | `dataLineBeg` |
| 32 | 0 | testable | `+ QfcCollectionController.xComma(itemInfo.Subject)` |
| 33 | 0 | testable | `+ $",SingleSorted,{durationText},{durationMinutesText},{itemInfo.ToRecipientsName}"` |
| 34 | 0 | testable | `+ $"{itemInfo.SenderName},Email,{selectedFolder},{itemInfo.SentDate.ToString("MM/dd/yyyy")},"` |
| 35 | 0 | testable | `+ $"{itemInfo.SentDate.ToString("HH:mm:ss")}"` |
| 36 | 0 | testable | `)` |
| 37 | 0 | testable | `.ToArray();` |
| 38 |  | non-executable | `` |
| 39 | 0 | seam-adjustment-required | `if (Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var folderRoot))` |
| 40 | 0 | non-executable | `{` |
| 41 | 0 | testable | `FileIO2.WriteTextFile(filename, dataLines, folderRoot);` |
| 42 | 0 | non-executable | `}` |
| 43 | 0 | non-executable | `}` |
| 45 |  | non-executable | `` |
| 46 |  | non-executable | `public void QuickFileMetrics_WRITE(string filename)` |
| 47 | 0 | non-executable | `{` |
| 48 | 0 | testable | `throw new NotImplementedException();` |
| 49 |  | non-executable | `}` |
| 50 |  | non-executable | `}` |
| 51 |  | non-executable | `}` |

## QuickFiler/Controllers/EfcHomeController.Timing.cs

ChangedOrNewLineCount: 43
CoverageEntry: FOUND
UncoveredChangedOrNewLineCount: 25

| Line | Hits | Classification | Text |
| --- | ---: | --- | --- |
| 1 |  | non-executable | `using System;` |
| 2 |  | non-executable | `using System.Threading;` |
| 3 |  | non-executable | `using UtilitiesCS;` |
| 4 |  | non-executable | `` |
| 5 |  | non-executable | `namespace QuickFiler` |
| 6 |  | non-executable | `{` |
| 7 |  | non-executable | `public partial class EfcHomeController` |
| 8 |  | non-executable | `{` |
| 9 |  | non-executable | `private static string DescribeSynchronizationContext(SynchronizationContext syncContext)` |
| 13 |  | non-executable | `` |
| 14 |  | non-executable | `private static string DescribeStartupOverlapState(IApplicationGlobals globals)` |
| 18 |  | non-executable | `` |
| 19 |  | non-executable | `private static string BuildFirstSelectionTimingContext(` |
| 20 |  | non-executable | `IApplicationGlobals globals,` |
| 21 |  | non-executable | `int selectedItemCount` |
| 22 |  | non-executable | `)` |
| 26 |  | non-executable | `` |
| 27 |  | non-executable | `private static void LogFirstSelectionTiming(` |
| 28 |  | non-executable | `string phase,` |
| 29 |  | non-executable | `IApplicationGlobals globals,` |
| 30 |  | non-executable | `int selectedItemCount,` |
| 31 |  | non-executable | `string details = null` |
| 32 |  | non-executable | `)` |
| 42 |  | non-executable | `}` |
| 43 |  | non-executable | `}` |

## QuickFiler/Controllers/EfcHomeControllerDependencies.cs

ChangedOrNewLineCount: 313
CoverageEntry: FOUND
UncoveredChangedOrNewLineCount: 254

| Line | Hits | Classification | Text |
| --- | ---: | --- | --- |
| 1 |  | non-executable | `using System;` |
| 2 |  | non-executable | `using System.Collections.Generic;` |
| 3 |  | non-executable | `using System.Linq;` |
| 4 |  | non-executable | `using System.Threading;` |
| 5 |  | non-executable | `using System.Threading.Tasks;` |
| 6 |  | non-executable | `using Microsoft.Office.Interop.Outlook;` |
| 7 |  | non-executable | `using QuickFiler.Controllers;` |
| 8 |  | non-executable | `using QuickFiler.Interfaces;` |
| 9 |  | non-executable | `using UtilitiesCS;` |
| 10 |  | non-executable | `` |
| 11 |  | non-executable | `namespace QuickFiler` |
| 12 |  | non-executable | `{` |
| 13 |  | non-executable | `internal sealed class EfcHomeControllerDependencies` |
| 14 |  | non-executable | `{` |
| 74 |  | non-executable | `` |
| 75 |  | non-executable | `internal Func<` |
| 76 |  | non-executable | `IApplicationGlobals,` |
| 77 |  | non-executable | `MailItem,` |
| 78 |  | non-executable | `CancellationTokenSource,` |
| 79 |  | non-executable | `CancellationToken,` |
| 80 |  | non-executable | `EfcDataModel` |
| 81 |  | non-executable | `> DataModelFactory { get; }` |
| 82 |  | non-executable | `` |
| 83 |  | non-executable | `internal Func<` |
| 84 |  | non-executable | `IApplicationGlobals,` |
| 85 |  | non-executable | `List<MailItem>,` |
| 86 |  | non-executable | `CancellationTokenSource,` |
| 87 |  | non-executable | `CancellationToken,` |
| 88 |  | non-executable | `bool,` |
| 89 |  | non-executable | `Task<EfcDataModel>` |
| 90 |  | non-executable | `> AsyncDataModelFactory { get; }` |
| 91 |  | non-executable | `` |
| 92 |  | non-executable | `internal Func<EfcViewer> ViewerFactory { get; }` |
| 93 |  | non-executable | `` |
| 94 |  | non-executable | `internal Func<` |
| 95 |  | non-executable | `EfcViewer,` |
| 96 |  | non-executable | `EfcHomeController,` |
| 97 |  | non-executable | `IQfcKeyboardHandler` |
| 98 |  | non-executable | `> KeyboardHandlerFactory { get; }` |
| 99 |  | non-executable | `` |
| 100 |  | non-executable | `internal Func<` |
| 101 |  | non-executable | `QfEnums.InitTypeEnum,` |
| 102 |  | non-executable | `IApplicationGlobals,` |
| 103 |  | non-executable | `EfcHomeController,` |
| 104 |  | non-executable | `IQfcExplorerController` |
| 105 |  | non-executable | `> ExplorerControllerFactory { get; }` |
| 106 |  | non-executable | `` |
| 107 |  | non-executable | `internal Func<` |
| 108 |  | non-executable | `IApplicationGlobals,` |
| 109 |  | non-executable | `EfcDataModel,` |
| 110 |  | non-executable | `EfcViewer,` |
| 111 |  | non-executable | `EfcHomeController,` |
| 112 |  | non-executable | `System.Action,` |
| 113 |  | non-executable | `QfEnums.InitTypeEnum,` |
| 114 |  | non-executable | `CancellationToken,` |
| 115 |  | non-executable | `EfcFormController` |
| 116 |  | non-executable | `> FormControllerWithDataFactory { get; }` |
| 117 |  | non-executable | `` |
| 118 |  | non-executable | `internal Func<` |
| 119 |  | non-executable | `IApplicationGlobals,` |
| 120 |  | non-executable | `EfcViewer,` |
| 121 |  | non-executable | `EfcHomeController,` |
| 122 |  | non-executable | `System.Action,` |
| 123 |  | non-executable | `QfEnums.InitTypeEnum,` |
| 124 |  | non-executable | `CancellationToken,` |
| 125 |  | non-executable | `EfcFormController` |
| 126 |  | non-executable | `> FormControllerWithoutDataFactory { get; }` |
| 127 |  | non-executable | `` |
| 128 |  | non-executable | `internal Func<` |
| 129 |  | non-executable | `EfcFormController,` |
| 130 |  | non-executable | `EfcDataModel,` |
| 131 |  | non-executable | `EfcFormController` |
| 132 |  | non-executable | `> InitializeDataFields { get; }` |
| 133 |  | non-executable | `` |
| 134 |  | non-executable | `internal Func<IApplicationGlobals, MailItem, List<MailItem>> SelectionLoader { get; }` |
| 135 |  | non-executable | `` |
| 136 |  | non-executable | `private static EfcDataModel CreateDataModel(` |
| 137 |  | non-executable | `IApplicationGlobals globals,` |
| 138 |  | non-executable | `MailItem mail,` |
| 139 |  | non-executable | `CancellationTokenSource tokenSource,` |
| 140 |  | non-executable | `CancellationToken token` |
| 141 |  | non-executable | `)` |
| 142 | 0 | non-executable | `{` |
| 143 | 0 | seam-adjustment-required | `if (globals is null)` |
| 144 | 0 | non-executable | `{` |
| 145 | 0 | seam-adjustment-required | `throw new ArgumentNullException(nameof(globals));` |
| 146 |  | non-executable | `}` |
| 147 | 0 | testable | `if (tokenSource is null)` |
| 148 | 0 | non-executable | `{` |
| 149 | 0 | testable | `throw new ArgumentNullException(nameof(tokenSource));` |
| 150 |  | non-executable | `}` |
| 151 |  | non-executable | `` |
| 152 | 0 | seam-adjustment-required | `return new EfcDataModel(globals, mail, tokenSource, token);` |
| 153 | 0 | non-executable | `}` |
| 154 |  | non-executable | `` |
| 155 |  | non-executable | `private static IQfcKeyboardHandler CreateKeyboardHandler(` |
| 156 |  | non-executable | `EfcViewer viewer,` |
| 157 |  | non-executable | `EfcHomeController homeController` |
| 158 |  | non-executable | `)` |
| 159 | 0 | non-executable | `{` |
| 160 | 0 | testable | `if (viewer is null)` |
| 161 | 0 | non-executable | `{` |
| 162 | 0 | testable | `throw new ArgumentNullException(nameof(viewer));` |
| 163 |  | non-executable | `}` |
| 164 | 0 | testable | `if (homeController is null)` |
| 165 | 0 | non-executable | `{` |
| 166 | 0 | testable | `throw new ArgumentNullException(nameof(homeController));` |
| 167 |  | non-executable | `}` |
| 168 |  | non-executable | `` |
| 169 | 0 | seam-adjustment-required | `return new KeyboardHandler(viewer, homeController);` |
| 170 | 0 | non-executable | `}` |
| 171 |  | non-executable | `` |
| 172 |  | non-executable | `private static IQfcExplorerController CreateExplorerController(` |
| 173 |  | non-executable | `QfEnums.InitTypeEnum initType,` |
| 174 |  | non-executable | `IApplicationGlobals globals,` |
| 175 |  | non-executable | `EfcHomeController homeController` |
| 176 |  | non-executable | `)` |
| 177 | 0 | non-executable | `{` |
| 178 | 0 | seam-adjustment-required | `if (globals is null)` |
| 179 | 0 | non-executable | `{` |
| 180 | 0 | seam-adjustment-required | `throw new ArgumentNullException(nameof(globals));` |
| 181 |  | non-executable | `}` |
| 182 | 0 | testable | `if (homeController is null)` |
| 183 | 0 | non-executable | `{` |
| 184 | 0 | testable | `throw new ArgumentNullException(nameof(homeController));` |
| 185 |  | non-executable | `}` |
| 186 |  | non-executable | `` |
| 187 | 0 | seam-adjustment-required | `return new QfcExplorerController(initType, globals, homeController);` |
| 188 | 0 | non-executable | `}` |
| 189 |  | non-executable | `` |
| 190 |  | non-executable | `private static EfcFormController CreateInitializedFormControllerWithData(` |
| 191 |  | non-executable | `IApplicationGlobals globals,` |
| 192 |  | non-executable | `EfcDataModel dataModel,` |
| 193 |  | non-executable | `EfcViewer viewer,` |
| 194 |  | non-executable | `EfcHomeController homeController,` |
| 195 |  | non-executable | `System.Action cleanup,` |
| 196 |  | non-executable | `QfEnums.InitTypeEnum initType,` |
| 197 |  | non-executable | `CancellationToken token` |
| 198 |  | non-executable | `)` |
| 199 | 0 | non-executable | `{` |
| 200 | 0 | seam-adjustment-required | `if (globals is null)` |
| 201 | 0 | non-executable | `{` |
| 202 | 0 | seam-adjustment-required | `throw new ArgumentNullException(nameof(globals));` |
| 203 |  | non-executable | `}` |
| 204 | 0 | testable | `if (dataModel is null)` |
| 205 | 0 | non-executable | `{` |
| 206 | 0 | testable | `throw new ArgumentNullException(nameof(dataModel));` |
| 207 |  | non-executable | `}` |
| 208 | 0 | testable | `if (viewer is null)` |
| 209 | 0 | non-executable | `{` |
| 210 | 0 | testable | `throw new ArgumentNullException(nameof(viewer));` |
| 211 |  | non-executable | `}` |
| 212 | 0 | testable | `if (homeController is null)` |
| 213 | 0 | non-executable | `{` |
| 214 | 0 | testable | `throw new ArgumentNullException(nameof(homeController));` |
| 215 |  | non-executable | `}` |
| 216 | 0 | testable | `if (cleanup is null)` |
| 217 | 0 | non-executable | `{` |
| 218 | 0 | testable | `throw new ArgumentNullException(nameof(cleanup));` |
| 219 |  | non-executable | `}` |
| 220 |  | non-executable | `` |
| 221 | 0 | seam-adjustment-required | `return new EfcFormController(` |
| 222 | 0 | seam-adjustment-required | `globals,` |
| 223 | 0 | testable | `dataModel,` |
| 224 | 0 | testable | `viewer,` |
| 225 | 0 | testable | `homeController,` |
| 226 | 0 | testable | `cleanup,` |
| 227 | 0 | testable | `initType,` |
| 228 | 0 | testable | `token` |
| 229 | 0 | testable | `).Initialize();` |
| 230 | 0 | non-executable | `}` |
| 231 |  | non-executable | `` |
| 232 |  | non-executable | `private static EfcFormController CreateInitializedFormControllerWithoutData(` |
| 233 |  | non-executable | `IApplicationGlobals globals,` |
| 234 |  | non-executable | `EfcViewer viewer,` |
| 235 |  | non-executable | `EfcHomeController homeController,` |
| 236 |  | non-executable | `System.Action cleanup,` |
| 237 |  | non-executable | `QfEnums.InitTypeEnum initType,` |
| 238 |  | non-executable | `CancellationToken token` |
| 239 |  | non-executable | `)` |
| 240 | 0 | non-executable | `{` |
| 241 | 0 | seam-adjustment-required | `if (globals is null)` |
| 242 | 0 | non-executable | `{` |
| 243 | 0 | seam-adjustment-required | `throw new ArgumentNullException(nameof(globals));` |
| 244 |  | non-executable | `}` |
| 245 | 0 | testable | `if (viewer is null)` |
| 246 | 0 | non-executable | `{` |
| 247 | 0 | testable | `throw new ArgumentNullException(nameof(viewer));` |
| 248 |  | non-executable | `}` |
| 249 | 0 | testable | `if (homeController is null)` |
| 250 | 0 | non-executable | `{` |
| 251 | 0 | testable | `throw new ArgumentNullException(nameof(homeController));` |
| 252 |  | non-executable | `}` |
| 253 | 0 | testable | `if (cleanup is null)` |
| 254 | 0 | non-executable | `{` |
| 255 | 0 | testable | `throw new ArgumentNullException(nameof(cleanup));` |
| 256 |  | non-executable | `}` |
| 257 |  | non-executable | `` |
| 258 | 0 | seam-adjustment-required | `return new EfcFormController(` |
| 259 | 0 | seam-adjustment-required | `globals,` |
| 260 | 0 | testable | `viewer,` |
| 261 | 0 | testable | `homeController,` |
| 262 | 0 | testable | `cleanup,` |
| 263 | 0 | testable | `initType,` |
| 264 | 0 | testable | `token` |
| 265 | 0 | testable | `).InitializeWithoutData();` |
| 266 | 0 | non-executable | `}` |
| 267 |  | non-executable | `` |
| 268 |  | non-executable | `private static EfcFormController InitializeFormControllerDataFields(` |
| 269 |  | non-executable | `EfcFormController controller,` |
| 270 |  | non-executable | `EfcDataModel dataModel` |
| 271 |  | non-executable | `)` |
| 272 | 0 | non-executable | `{` |
| 273 | 0 | testable | `if (controller is null)` |
| 274 | 0 | non-executable | `{` |
| 275 | 0 | testable | `throw new ArgumentNullException(nameof(controller));` |
| 276 |  | non-executable | `}` |
| 277 | 0 | testable | `if (dataModel is null)` |
| 278 | 0 | non-executable | `{` |
| 279 | 0 | testable | `throw new ArgumentNullException(nameof(dataModel));` |
| 280 |  | non-executable | `}` |
| 281 |  | non-executable | `` |
| 282 | 0 | testable | `return controller.InitializeDataFields(dataModel);` |
| 283 | 0 | non-executable | `}` |
| 284 |  | non-executable | `` |
| 285 |  | non-executable | `private static List<MailItem> LoadSelection(IApplicationGlobals globals, MailItem mail)` |
| 286 | 0 | non-executable | `{` |
| 287 | 0 | seam-adjustment-required | `if (globals is null)` |
| 288 | 0 | non-executable | `{` |
| 289 | 0 | seam-adjustment-required | `throw new ArgumentNullException(nameof(globals));` |
| 290 |  | non-executable | `}` |
| 291 |  | non-executable | `` |
| 292 | 0 | testable | `List<MailItem> mailItems = [];` |
| 293 |  | non-executable | `` |
| 294 | 0 | testable | `if (mail is not null)` |
| 295 | 0 | non-executable | `{` |
| 296 | 0 | testable | `mailItems.Add(mail);` |
| 297 | 0 | testable | `return mailItems;` |
| 298 |  | non-executable | `}` |
| 299 |  | non-executable | `` |
| 300 | 0 | seam-adjustment-required | `var selection = globals.Ol.App.ActiveExplorer().Selection;` |
| 301 | 0 | testable | `if (selection.Count > 0)` |
| 302 | 0 | non-executable | `{` |
| 303 | 0 | testable | `mailItems = selection` |
| 304 | 0 | testable | `.Cast<object>()` |
| 305 | 0 | testable | `.Where(x => x is MailItem)` |
| 306 | 0 | testable | `.Cast<MailItem>()` |
| 307 | 0 | testable | `.ToList();` |
| 308 | 0 | non-executable | `}` |
| 309 |  | non-executable | `` |
| 310 | 0 | testable | `return mailItems;` |
| 311 | 0 | non-executable | `}` |
| 312 |  | non-executable | `}` |
| 313 |  | non-executable | `}` |

## QuickFiler/Helper Classes/EfcViewerQueue.cs

ChangedOrNewLineCount: 38
CoverageEntry: FOUND
UncoveredChangedOrNewLineCount: 18

| Line | Hits | Classification | Text |
| --- | ---: | --- | --- |
| 1 |  | non-executable | `using System.Threading;` |
| 2 |  | non-executable | `using System.Windows.Threading;` |
| 26 |  | non-executable | `` |
| 27 |  | non-executable | `/// <summary>` |
| 28 |  | non-executable | `/// Replaces the production queue core for deterministic unit tests.` |
| 29 |  | non-executable | `/// </summary>` |
| 30 |  | non-executable | `internal static void SetCoreForTesting(ViewerQueueCore<EfcViewer> core)` |
| 34 |  | non-executable | `` |
| 35 |  | non-executable | `/// <summary>` |
| 36 |  | non-executable | `/// Restores the production queue core after deterministic unit tests.` |
| 37 |  | non-executable | `/// </summary>` |
| 38 |  | non-executable | `internal static void ResetCoreForTesting()` |
| 43 |  | non-executable | `` |
| 44 |  | non-executable | `private static ViewerQueueCore<EfcViewer> CreateProductionCore()` |
| 47 | 0 | seam-adjustment-required | `() => new EfcViewer(),` |
| 48 | 0 | testable | `action => action(),` |
| 49 | 0 | seam-adjustment-required | `(action, priority) => _ = UiThread.Dispatcher.InvokeAsync(action, priority),` |
| 50 | 0 | testable | `(action, priority) => action()` |

## QuickFiler/Helper Classes/ItemViewerQueue.cs

ChangedOrNewLineCount: 43
CoverageEntry: FOUND
UncoveredChangedOrNewLineCount: 16

| Line | Hits | Classification | Text |
| --- | ---: | --- | --- |
| 46 |  | non-executable | `` |
| 47 |  | non-executable | `/// <summary>` |
| 48 |  | non-executable | `/// Replaces the production queue core for deterministic unit tests.` |
| 49 |  | non-executable | `/// </summary>` |
| 50 |  | non-executable | `internal static void SetCoreForTesting(ViewerQueueCore<ItemViewer> core)` |
| 54 |  | non-executable | `` |
| 55 |  | non-executable | `/// <summary>` |
| 56 |  | non-executable | `/// Restores the production queue core after deterministic unit tests.` |
| 57 |  | non-executable | `/// </summary>` |
| 58 |  | non-executable | `internal static void ResetCoreForTesting()` |
| 63 |  | non-executable | `` |
| 64 |  | non-executable | `private static ViewerQueueCore<ItemViewer> CreateProductionCore()` |
| 67 | 0 | seam-adjustment-required | `() => new ItemViewer(),` |
| 68 | 0 | testable | `action => action(),` |
| 69 | 0 | seam-adjustment-required | `(action, priority) => _ = UiThread.Dispatcher.InvokeAsync(action, priority),` |
| 70 | 0 | seam-adjustment-required | `(action, priority) => UiThread.Dispatcher.Invoke(action, priority)` |

## QuickFiler/Helper Classes/QfcThemeHelper.cs

ChangedOrNewLineCount: 228
CoverageEntry: FOUND
UncoveredChangedOrNewLineCount: 69

| Line | Hits | Classification | Text |
| --- | ---: | --- | --- |
| 1 |  | non-executable | `using System;` |
| 5 |  | non-executable | `using Microsoft.Web.WebView2.Core;` |
| 43 | 0 | testable | `if (controller is null)` |
| 44 | 0 | non-executable | `{` |
| 45 | 0 | testable | `throw new ArgumentNullException(nameof(controller));` |
| 46 |  | non-executable | `}` |
| 47 | 0 | testable | `if (viewer is null)` |
| 48 | 0 | non-executable | `{` |
| 49 | 0 | testable | `throw new ArgumentNullException(nameof(viewer));` |
| 50 |  | non-executable | `}` |
| 51 |  | non-executable | `` |
| 52 | 0 | testable | `var controlSet = new QfcThemeControlSet(` |
| 53 | 0 | testable | `viewer.LblItemNumber,` |
| 54 | 0 | testable | `viewer.LblSender,` |
| 55 | 0 | testable | `viewer.LblSubject,` |
| 56 | 0 | testable | `controller.TableLayoutPanels,` |
| 57 | 0 | testable | `controller.Buttons,` |
| 58 | 0 | testable | `viewer.MenuItems,` |
| 59 | 0 | testable | `viewer.MoveOptionsStrip,` |
| 60 | 0 | testable | `controller.ListTipsDetails,` |
| 61 | 0 | testable | `controller.ListTipsExpanded,` |
| 62 | 0 | testable | `viewer.TxtboxSearch,` |
| 63 | 0 | testable | `viewer.TxtboxBody,` |
| 64 | 0 | testable | `viewer.CboFolders,` |
| 65 | 0 | testable | `viewer.TopicThread,` |
| 66 | 0 | testable | `viewer.L0v2h2_WebView2,` |
| 67 | 0 | testable | `viewer,` |
| 68 | 0 | testable | `() => !controller.Mail.UnRead,` |
| 69 | 0 | testable | `htmlConverter,` |
| 70 | 0 | seam-adjustment-required | `uiDispatcher` |
| 71 | 0 | testable | `);` |
| 72 |  | non-executable | `` |
| 73 | 0 | testable | `return SetupThemes(controlSet);` |
| 74 | 0 | non-executable | `}` |
| 75 |  | non-executable | `` |
| 76 |  | non-executable | `internal static Dictionary<string, Theme> SetupThemes(QfcThemeControlSet controlSet)` |
| 79 | 0 | non-executable | `{` |
| 80 | 0 | testable | `throw new ArgumentNullException(nameof(controlSet));` |
| 81 |  | non-executable | `}` |
| 82 |  | non-executable | `` |
| 269 |  | non-executable | `` |
| 270 |  | non-executable | `private static Theme CreateTheme(` |
| 271 |  | non-executable | `QfcThemeControlSet controlSet,` |
| 272 |  | non-executable | `string name,` |
| 273 |  | non-executable | `CoreWebView2PreferredColorScheme web2ViewScheme,` |
| 274 |  | non-executable | `Enums.ToggleState htmlDark,` |
| 275 |  | non-executable | `Color navBackgColor,` |
| 276 |  | non-executable | `Color navForeColor,` |
| 277 |  | non-executable | `Color tlpBackColor,` |
| 278 |  | non-executable | `Color tipsForeColor,` |
| 279 |  | non-executable | `Color tipsBackColor,` |
| 280 |  | non-executable | `Color mailReadForeColor,` |
| 281 |  | non-executable | `Color mailReadBackColor,` |
| 282 |  | non-executable | `Color mailUnreadForeColor,` |
| 283 |  | non-executable | `Color mailUnreadBackColor,` |
| 284 |  | non-executable | `Color tipsDetailsBackColor,` |
| 285 |  | non-executable | `Color tipsDetailsForeColor,` |
| 286 |  | non-executable | `Color buttonBackColor,` |
| 287 |  | non-executable | `Color buttonMouseOverColor,` |
| 288 |  | non-executable | `Color buttonClickedColor,` |
| 289 |  | non-executable | `Color txtboxSearchBackColor,` |
| 290 |  | non-executable | `Color txtboxSearchForeColor,` |
| 291 |  | non-executable | `Color txtboxBodyBackColor,` |
| 292 |  | non-executable | `Color txtboxBodyForeColor,` |
| 293 |  | non-executable | `Color cboFoldersBackColor,` |
| 294 |  | non-executable | `Color cboFoldersForeColor,` |
| 295 |  | non-executable | `Color defaultBackColor,` |
| 296 |  | non-executable | `Color defaultForeColor` |
| 297 |  | non-executable | `)` |

## QuickFiler/Helper Classes/QfcThemeControlSet.cs

ChangedOrNewLineCount: 101
CoverageEntry: FOUND
UncoveredChangedOrNewLineCount: 55

| Line | Hits | Classification | Text |
| --- | ---: | --- | --- |
| 1 |  | non-executable | `using System;` |
| 2 |  | non-executable | `using System.Collections.Generic;` |
| 3 |  | non-executable | `using System.ComponentModel;` |
| 4 |  | non-executable | `using System.Windows.Forms;` |
| 5 |  | non-executable | `using BrightIdeasSoftware;` |
| 6 |  | non-executable | `using Microsoft.Web.WebView2.WinForms;` |
| 7 |  | non-executable | `using UtilitiesCS;` |
| 8 |  | non-executable | `using UtilitiesCS.Threading;` |
| 9 |  | non-executable | `` |
| 10 |  | non-executable | `namespace QuickFiler` |
| 11 |  | non-executable | `{` |
| 12 |  | non-executable | `internal sealed class QfcThemeControlSet` |
| 13 |  | non-executable | `{` |
| 54 |  | non-executable | `` |
| 55 |  | non-executable | `internal Label LblItemNumber { get; }` |
| 56 |  | non-executable | `` |
| 57 |  | non-executable | `internal Label LblSender { get; }` |
| 58 |  | non-executable | `` |
| 59 |  | non-executable | `internal Label LblSubject { get; }` |
| 60 |  | non-executable | `` |
| 61 |  | non-executable | `internal IList<TableLayoutPanel> TableLayoutPanels { get; }` |
| 62 |  | non-executable | `` |
| 63 |  | non-executable | `internal IList<Button> Buttons { get; }` |
| 64 |  | non-executable | `` |
| 65 |  | non-executable | `internal IList<Component> MenuItems { get; }` |
| 66 |  | non-executable | `` |
| 67 |  | non-executable | `internal MenuStrip MenuStrip { get; }` |
| 68 |  | non-executable | `` |
| 69 |  | non-executable | `internal IList<IQfcTipsDetails> TipsDetailsLabels { get; }` |
| 70 |  | non-executable | `` |
| 71 |  | non-executable | `internal IList<IQfcTipsDetails> TipsExpanded { get; }` |
| 72 |  | non-executable | `` |
| 73 |  | non-executable | `internal TextBox TextboxSearch { get; }` |
| 74 |  | non-executable | `` |
| 75 |  | non-executable | `internal TextBox TextboxBody { get; }` |
| 76 |  | non-executable | `` |
| 77 |  | non-executable | `internal ComboBox ComboFolders { get; }` |
| 78 |  | non-executable | `` |
| 79 |  | non-executable | `internal FastObjectListView TopicThread { get; }` |
| 80 |  | non-executable | `` |
| 81 |  | non-executable | `internal WebView2 WebView2 { get; }` |
| 82 |  | non-executable | `` |
| 83 |  | non-executable | `internal Control Viewer { get; }` |
| 84 |  | non-executable | `` |
| 85 |  | non-executable | `internal Func<bool> MailRead { get; }` |
| 86 |  | non-executable | `` |
| 87 |  | non-executable | `internal Action<Enums.ToggleState> HtmlConverter { get; }` |
| 88 |  | non-executable | `` |
| 89 |  | non-executable | `internal IUiDispatcher UiDispatcher { get; }` |
| 90 |  | non-executable | `` |
| 91 |  | non-executable | `private static IList<T> RequireCollection<T>(IList<T> value, string parameterName)` |
| 96 |  | non-executable | `}` |
| 97 |  | non-executable | `` |
| 100 |  | non-executable | `}` |
| 101 |  | non-executable | `}` |

## QuickFiler/Helper Classes/ViewerQueueCore.cs

ChangedOrNewLineCount: 161
CoverageEntry: FOUND
UncoveredChangedOrNewLineCount: 70

| Line | Hits | Classification | Text |
| --- | ---: | --- | --- |
| 1 |  | non-executable | `using System;` |
| 2 |  | non-executable | `using System.Collections.Generic;` |
| 3 |  | non-executable | `using System.Threading;` |
| 4 |  | non-executable | `using System.Windows.Threading;` |
| 5 |  | non-executable | `` |
| 6 |  | non-executable | `namespace QuickFiler` |
| 7 |  | non-executable | `{` |
| 8 |  | non-executable | `internal sealed class ViewerQueueCore<TViewer>` |
| 9 |  | non-executable | `where TViewer : class` |
| 10 |  | non-executable | `{` |
| 11 |  | non-executable | `private readonly Func<TViewer> _viewerFactory;` |
| 12 |  | non-executable | `private readonly Action<Action> _synchronousScheduler;` |
| 13 |  | non-executable | `private readonly Action<Action, DispatcherPriority> _priorityScheduler;` |
| 14 |  | non-executable | `private readonly Action<Action, DispatcherPriority> _blockingPriorityScheduler;` |
| 15 |  | non-executable | `private readonly Action<TViewer> _disposeViewer;` |
| 17 |  | non-executable | `` |
| 36 |  | non-executable | `` |
| 38 |  | non-executable | `` |
| 39 |  | non-executable | `internal int BuildQueue(int count)` |
| 42 |  | non-executable | `` |
| 43 |  | non-executable | `// Synchronous builds are used by callers that must have queued viewers available immediately.` |
| 48 |  | non-executable | `` |
| 51 |  | non-executable | `` |
| 52 |  | non-executable | `internal void BuildQueue(int count, DispatcherPriority priority)` |
| 55 |  | non-executable | `` |
| 56 |  | non-executable | `// Priority builds preserve production dispatcher behavior while tests can supply a deterministic scheduler.` |
| 62 |  | non-executable | `` |
| 63 |  | non-executable | `internal TViewer Dequeue(` |
| 64 |  | non-executable | `CancellationToken cancellationToken,` |
| 65 |  | non-executable | `DispatcherPriority emptyQueuePriority,` |
| 66 |  | non-executable | `int cachedReplacementCount,` |
| 67 |  | non-executable | `int emptyReplacementCount,` |
| 68 |  | non-executable | `DispatcherPriority replacementPriority` |
| 69 |  | non-executable | `)` |
| 74 |  | non-executable | `` |
| 80 |  | non-executable | `}` |
| 81 |  | non-executable | `` |
| 86 |  | non-executable | `` |
| 87 |  | non-executable | `internal IReadOnlyList<TViewer> DequeueChunk(` |
| 88 |  | non-executable | `int count,` |
| 89 |  | non-executable | `DispatcherPriority missingViewerPriority,` |
| 90 |  | non-executable | `DispatcherPriority replacementPriority` |
| 91 |  | non-executable | `)` |
| 94 |  | non-executable | `` |
| 103 |  | non-executable | `` |
| 105 |  | non-executable | `` |
| 107 |  | non-executable | `// Chunk dequeue returns the requested number after filling any shortfall synchronously.` |
| 112 |  | non-executable | `` |
| 115 |  | non-executable | `` |
| 116 |  | non-executable | `internal void Reset()` |
| 118 |  | non-executable | `// Reset owns cleanup for static-wrapper tests so queued viewer instances do not leak between tests.` |
| 125 |  | non-executable | `` |
| 126 |  | non-executable | `private TViewer CreateWithPriority(` |
| 127 |  | non-executable | `DispatcherPriority priority,` |
| 128 |  | non-executable | `CancellationToken cancellationToken` |
| 129 |  | non-executable | `)` |
| 140 |  | non-executable | `` |
| 143 |  | non-executable | `` |
| 144 |  | non-executable | `private void EnqueueWith(Action<Action> scheduler)` |
| 148 |  | non-executable | `` |
| 149 |  | non-executable | `private static void ValidateCount(int count)` |
| 152 | 0 | non-executable | `{` |
| 153 | 0 | testable | `throw new ArgumentOutOfRangeException(` |
| 154 | 0 | testable | `nameof(count),` |
| 155 | 0 | testable | `count,` |
| 156 | 0 | testable | `"Queue counts cannot be negative."` |
| 157 | 0 | testable | `);` |
| 158 |  | non-executable | `}` |
| 160 |  | non-executable | `}` |
| 161 |  | non-executable | `}` |

## QuickFiler/Helper Classes/TlpCellSnapShot.cs

ChangedOrNewLineCount: 10
CoverageEntry: FOUND
UncoveredChangedOrNewLineCount: 4

| Line | Hits | Classification | Text |
| --- | ---: | --- | --- |
| 23 |  | non-executable | `}` |
| 24 |  | non-executable | `` |
| 35 |  | non-executable | `}` |
| 36 |  | non-executable | `` |

## Totals

ChangedOrNewLines: 1069
UncoveredChangedOrNewLines: 602
ParserResult: PASS
