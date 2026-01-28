// Local commitlint rule to enforce an imperative mood summary.
// Heuristic: block common past-tense/gerund starts (e.g., "added", "fixing").
module.exports = {
  rules: {
    "subject-imperative": (parsed) => {
      const subject = (parsed.subject || "").trim();
      if (!subject) {
        return [false, "summary is required"];
      }

      const firstWord = subject.split(/\s+/)[0].toLowerCase();
      const blocked = new Set([
        "added",
        "adds",
        "adding",
        "fixed",
        "fixes",
        "fixing",
        "removed",
        "removes",
        "removing",
        "updated",
        "updates",
        "updating",
        "refactored",
        "refactors",
        "refactoring",
        "improved",
        "improves",
        "improving",
        "changed",
        "changes",
        "changing",
        "created",
        "creates",
        "creating",
        "renamed",
        "renames",
        "renaming",
        "migrated",
        "migrates",
        "migrating"
      ]);

      if (blocked.has(firstWord)) {
        return [false, "summary should use imperative mood (e.g., \"add\" not \"added\")"];
      }

      return [true];
    }
  }
};
