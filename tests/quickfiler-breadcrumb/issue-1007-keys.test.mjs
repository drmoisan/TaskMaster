// Issue 1007: QFC page keys, page size, pending descendant highlight, segment clicks and the
// zero-children toggle defence, driven against the real FolderBreadcrumb.js.
import test from "node:test";
import assert from "node:assert/strict";
import { source } from "./issue-997-source-loader.mjs";
import { createBrowser, suggestionRow, selectorView } from "./issue-997-browser-harness.mjs";

function node(identity, overrides = {}) {
  return { identity, displayName: identity, folderPath: `Projects\\Apollo\\${identity}`,
    hasChildren: false, expanded: false, selected: false, childrenLoaded: false, children: [], ...overrides };
}

function chainRow(overrides = {}) {
  return suggestionRow({ cells: [
    { kind: "segment", text: "Archive", segmentIndex: 0, truncationEligible: false },
    { kind: "arrow", text: "", segmentIndex: -1 },
    { kind: "segment", text: "Projects", segmentIndex: 1, truncationEligible: false }
  ], ...overrides });
}

function show(rows, view = {}) {
  const browser = createBrowser(source);
  browser.receive({ type: "render", rows });
  browser.receive(selectorView(view));
  return browser;
}

function key(browser, values) {
  const event = browser.dispatch(browser.list, "keydown", values);
  assert.equal(event.defaultPrevented, true, `${values.key} prevents the browser default`);
  return event;
}

// Four stacked 20px rows inside a 50px list: rows 0 and 1 are fully visible, row 2 is cut off.
function stackRows(browser, listBottom = 50) {
  browser.list.rect = { top: 0, bottom: listBottom };
  browser.list.querySelectorAll(".row").forEach((row, index) => {
    row.rect = { top: index * 20, bottom: index * 20 + 20 };
  });
}

const fourRows = () => [0, 1, 2, 3].map((rowIndex) => suggestionRow({ rowIndex, selected: rowIndex === 0 }));
const fourOptions = [0, 1, 2, 3].map((index) => ({ identity: `row-${index}`, isSelectable: true }));

test("ctrl+ArrowLeft posts the parent selector key", () => {
  const browser = show([chainRow()]);
  key(browser, { key: "ArrowLeft", ctrlKey: true });
  assert.deepEqual(browser.messages, [{ type: "selectorKey", key: "parent" }]);
});

test("ctrl+ArrowRight posts arrowKey right", () => {
  // A plain row: Ctrl+Right is Right (D-Q20) and reaches the host for every row kind.
  const browser = show([suggestionRow({ isSuggestion: false })]);
  key(browser, { key: "ArrowRight", ctrlKey: true });
  assert.deepEqual(browser.messages, [{ type: "arrowKey", direction: "right" }]);
});

test("Home and End post selector keys", () => {
  const browser = show([chainRow()]);
  key(browser, { key: "Home" });
  key(browser, { key: "End" });
  assert.deepEqual(browser.messages, [{ type: "selectorKey", key: "home" }, { type: "selectorKey", key: "end" }]);
});

test("PageUp and PageDown post the fully visible row count as pageSize", () => {
  const browser = show(fourRows(), { options: fourOptions, committedIdentity: "row-0", pendingIdentity: "row-0" });
  stackRows(browser);
  key(browser, { key: "PageDown" });
  key(browser, { key: "PageUp" });
  assert.deepEqual(browser.messages, [
    { type: "selectorKey", key: "pagedown", pageSize: 2 },
    { type: "selectorKey", key: "pageup", pageSize: 2 }
  ]);
});

test("fullyVisibleCount counts only fully visible rows with a minimum of one", () => {
  const browser = show(fourRows(), { options: fourOptions, committedIdentity: "row-0", pendingIdentity: "row-0" });
  stackRows(browser, 79);
  key(browser, { key: "PageDown" });
  stackRows(browser, 10);
  key(browser, { key: "PageDown" });
  assert.deepEqual(browser.messages.map((message) => message.pageSize), [3, 1],
    "the partly visible fourth row is not counted, and an empty viewport still pages by one");
});

test("ArrowLeft and ArrowRight always post arrowKey", () => {
  for (const rows of [[], [suggestionRow({ isSuggestion: false })]]) {
    const browser = show(rows);
    key(browser, { key: "ArrowLeft" });
    key(browser, { key: "ArrowRight" });
    assert.deepEqual(browser.messages, [
      { type: "arrowKey", direction: "left" },
      { type: "arrowKey", direction: "right" }
    ]);
  }
});

test("pending node carries sub active", () => {
  const tree = [node("Gamma"), node("Delta")];
  const browser = show([chainRow({ leafExpanded: true, subfolders: tree })], { pendingNodeIdentity: "Delta" });
  const active = browser.list.querySelectorAll(".sub.active");
  assert.equal(active.length, 1);
  assert.equal(active[0].id, "folder-option-0-node-Delta");
  assert.equal(active[0].getAttribute("aria-selected"), "true");
  assert.equal(browser.list.querySelector(".row.active"), null, "the row entry is not the pending entry");
  assert.equal(browser.list.getAttribute("aria-activedescendant"), active[0].id);
});

test("single click on a non-leaf segment in the open list posts selectorHighlight without bubbling", () => {
  const browser = show([chainRow()]);
  const [parent, leaf] = browser.list.querySelectorAll(".seg");
  const event = browser.dispatch(parent, "click");
  assert.equal(event.propagationStopped, true);
  assert.deepEqual(browser.messages, [{ type: "selectorHighlight", identity: "row-a" }]);
  browser.clearMessages();
  browser.dispatch(leaf, "click");
  assert.deepEqual(browser.messages, [{ type: "selectorActivate", identity: "row-a" }], "the leaf segment still commits");
});

test("segment double-click posts selectorSegmentDoubleClick with the row identity", () => {
  const browser = show([chainRow()]);
  const event = browser.dispatch(browser.list.querySelectorAll(".seg")[0], "dblclick");
  assert.equal(event.propagationStopped, true);
  assert.deepEqual(browser.messages, [{ type: "selectorSegmentDoubleClick", rowIdentity: "row-a", segmentIndex: 0 }]);
});

test("loaded child with zero children renders no toggle", () => {
  const tree = [
    node("Empty", { hasChildren: true, expanded: true, childrenLoaded: true }),
    node("Pending", { hasChildren: true, expanded: true, childrenLoaded: false })
  ];
  const browser = show([chainRow({ leafExpanded: true, subfolders: tree })]);
  const [empty, pending] = browser.list.querySelectorAll(".sub");
  assert.equal(empty.querySelector(".aff"), null, "a loaded empty child has no expand affordance");
  assert.notEqual(pending.querySelector(".aff"), null, "a child whose fetch is in flight keeps its toggle");
});
