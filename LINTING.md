# Linting

For each PR, there's a build to validate that all markdown files are following conventions defined in `.markdownlint.json`

---

## Table Of Contents

<!-- toc -->

- [Pre-req](#pre-req)
- [Linting manually](#linting-manually)
- [Vscode Extension](#vscode-extension)
- [Fixing lint errors](#fixing-lint-errors)
  <!-- tocstop -->

## Pre-req

Install NPM dependencies to set up pre-commit hook for linting

```bash
# make sure you're at the root of the urlist app
npm install
```

## Linting manually/locally

If you want to run these lint check locally before pushing

```bash
# make sure you're at the root of the urlist app
npm run lint
```

## Vscode Extension

It is recommended that you install the [markdownlint](https://github.com/DavidAnson/vscode-markdownlint/blob/master/README.md#rules)
extension.

This extension will highlight rule violation as you edit the files.

## Fixing lint errors

### Extension

If you have the vscode extenstion installed, you could configure your vscode preferences to fix linting issues on save

```json
"editor.codeActionsOnSave": {
    "source.fixAll.markdownlint": true
}
```

### NPM package

Alternatively, you could run the following command

```bash
npm run lint:fix
```
