# Development Environment Setup for Lockb0x Protocol

This guide will help you set up your local environment for contributing to the Lockb0x Protocol specification, reference implementation, and documentation.

## Prerequisites

- [Git](https://git-scm.com/)
- [Node.js](https://nodejs.org/) (v18 or newer recommended)
- [Visual Studio Code](https://code.visualstudio.com/)

## 1. Clone the Repository

```bash
git clone https://github.com/lockb0x-llc/lockb0x-protocol.git
cd lockb0x-protocol
```

## 2. Install Dependencies

```bash
npm install
```

## 3. Recommended VS Code Extensions

- Markdown All in One
- Prettier - Code formatter
- JSON Schema Validator
- GitHub Copilot
- Copilot Agent (if available)

## 4. Linting and Validation

Run these commands before submitting a pull request:

```bash
npm run lint:md   # Lint all markdown files
npm run lint:json # Validate all JSON files
```

## 5. Continuous Integration

All pull requests are checked automatically for linting and validation via GitHub Actions.

## 6. Contribution Guidelines

See [`CONTRIBUTING.md`](CONTRIBUTING.md) for details on how to contribute.

---

If you have any setup issues, please open an issue in the repository for help.
