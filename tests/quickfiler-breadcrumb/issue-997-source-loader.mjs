import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import { fileURLToPath } from "node:url";

// Source selection and filesystem access happen once in the outer input adapter.
const filename = process.env.ISSUE_997_SCRIPT_FILE
  ? resolve(process.env.ISSUE_997_SCRIPT_FILE)
  : fileURLToPath(new URL("../../QuickFiler/Resources/FolderBreadcrumb.js", import.meta.url));
const variant = process.env.ISSUE_997_SCRIPT_VARIANT || "final";
if (variant !== "baseline" && variant !== "final") {
  throw new Error("Issue 997 source variant must be baseline or final.");
}
export const source = Object.freeze({ filename, variant, text: readFileSync(filename, "utf8") });
