module.exports = {
  plugins: [
    {
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
    }
  ],
  rules: {
    "type-enum": [
      2,
      "always",
      [
        "feat",
        "fix",
        "refactor",
        "test",
        "docs",
        "chore",
        "ci",
        "perf",
        "security"
      ]
    ],
    "header-max-length": [2, "always", 72],
    "subject-case": [2, "always", "lower-case"],
    "subject-full-stop": [2, "never", "."],
    "type-case": [2, "always", "lower-case"],
    "subject-imperative": [2, "always"]
  }
};
