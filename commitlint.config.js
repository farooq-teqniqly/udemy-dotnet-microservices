export default {
  extends: ["@commitlint/config-conventional"],
  rules: {
    "type-enum": [2, "always", ["feat", "fix", "docs", "refactor", "style", "test", "chore"]],
    "type-empty": [2, "never"],
    "subject-case": [0],
    "subject-empty": [2, "never"],
    "header-max-length": [2, "always", 200],
    "subject-full-stop": [0],
  },
};
