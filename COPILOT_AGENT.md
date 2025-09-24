# Copilot Coding Agent Best Practices for Lockb0x Protocol

This repository follows best practices for using GitHub Copilot Coding Agent. Please review and follow these guidelines to ensure a productive and secure experience.

## 1. Repository Setup

- All development tooling configuration files (e.g., `.msp.json`, `.vscode/`, `.github/`) are included for collaboration.
- Sensitive files (e.g., `.env`, secrets, keys, certificates) are excluded via `.gitignore`.
- Documentation and setup files are always included for transparency.

## 2. Coding and Contribution

- Use pull requests for all code and documentation changes.
- Open issues for discussion, questions, or bug reports.
- Follow the [CONTRIBUTING.md](CONTRIBUTING.md) and [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) guidelines.

## 3. Copilot Agent Usage

- Use Copilot Agent for:
  - Automated code suggestions and refactoring
  - Linting and validation (see `SETUP.md` for scripts)
  - Reviewing and improving documentation
  - Generating tests and verifying reference implementations
- Do **not** use Copilot Agent to generate or store secrets, credentials, or sensitive data.
- Always review Copilot Agent suggestions for accuracy, security, and compliance before merging.

## 4. Security and Compliance

- Ensure all dependencies are up-to-date and free of known vulnerabilities.
- Validate all JSON and markdown files before submitting changes.
- Do not commit sensitive information or credentials.

## 5. Collaboration

- Encourage open discussion and feedback via issues and pull requests.
- Use clear commit messages and PR descriptions.
- Reference relevant documentation and setup files in your contributions.

---

For more tips, see the official [Copilot Coding Agent Best Practices](https://gh.io/copilot-coding-agent-tips).
