# Branch Cleanup Guide

This document lists branches that are effectively empty (contain only the initial LICENSE commit) and are candidates for deletion.

## Empty Branches (Safe to Delete)

All of the following branches point to the initial commit `c57044fd` which contains only the `LICENSE` file. They have no unique content and can be safely deleted.

| Branch | SHA | Status |
|---|---|---|
| `copilot/bigmoneyslots-merge-into-wiseeee` | `c57044fd` | ❌ Empty — delete |
| `copilot/check-bigmoney-slots-code` | `c57044fd` | ❌ Empty — delete |
| `copilot/create-bitlocker-menu-bypass` | `c57044fd` | ❌ Empty — delete |
| `copilot/create-deployable-keystroker` | `c57044fd` | ❌ Empty — delete |
| `copilot/finish-building-code-for-deployment` | `c57044fd` | ❌ Empty — delete |
| `copilot/retry-failed-requests` | `c57044fd` | ❌ Empty — delete |

## Branches with Content (Review Before Deleting)

The following branches have different SHAs from the initial commit and may contain unique content. Review before deleting:

| Branch | SHA | Status |
|---|---|---|
| `copilot/build-dark-web-co-pilot` | `1b0eb2b` | ⚠️ Review content |
| `copilot/build-own-network-storage-server` | `cbc9a60` | ⚠️ Review content |
| `copilot/check-code` | `5489c11` | ⚠️ Review content |
| `copilot/explain-repository-structure` | `24fbd4f` | ⚠️ Review content |
| `copilot/fix-unity-csharp-errors` | `bca0bd4` | ⚠️ Previous fix PR — review before deleting |

## Default Branch Issue

The `Wiseeeee` branch is the **default branch** but only contains the LICENSE file. All project code lives in the `Main` branch.

**Recommended action:** Open a PR from `Main` → `Wiseeeee` to bring all project code into the default branch, then (optionally) change the default branch to `Main` in GitHub repository settings.

## How to Delete Branches (Requires Admin Access)

Branches can be deleted via:

### GitHub Web UI
1. Go to **https://github.com/CeeMoreBooty/BigMoneySlots/branches**
2. Click the **🗑️ trash icon** next to each branch listed above

### GitHub CLI
```bash
gh auth login
gh api -X DELETE repos/CeeMoreBooty/BigMoneySlots/git/refs/heads/copilot/bigmoneyslots-merge-into-wiseeee
gh api -X DELETE repos/CeeMoreBooty/BigMoneySlots/git/refs/heads/copilot/check-bigmoney-slots-code
gh api -X DELETE repos/CeeMoreBooty/BigMoneySlots/git/refs/heads/copilot/create-bitlocker-menu-bypass
gh api -X DELETE repos/CeeMoreBooty/BigMoneySlots/git/refs/heads/copilot/create-deployable-keystroker
gh api -X DELETE repos/CeeMoreBooty/BigMoneySlots/git/refs/heads/copilot/finish-building-code-for-deployment
gh api -X DELETE repos/CeeMoreBooty/BigMoneySlots/git/refs/heads/copilot/retry-failed-requests
```

> ⚠️ **Note:** Branch deletion requires repository **Admin** or **Maintainer** permissions. This document is provided for reference only — branches have NOT been deleted automatically.
