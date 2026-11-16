# Assay's Stryker.NET

The `assay` branch is a Stryker.NET release with fixes Assay needs before they
ship upstream. Assay bundles the package it builds, so every repository it runs
in uses the same Stryker.

## Rules

- Patch only what Assay cannot work around from outside (a build setting, a
  config value, an MSBuild targets file).
- One commit per fix, with a test that fails without it.
- Send every fix upstream as a pull request. When upstream ships it, the commit
  drops out at the next rebase.

## Versions

`<next upstream patch>-assay.<n>`, for example `5.1.1-assay.1` on top of 5.1.0.
It sorts above the release it is built on, so Stryker does not say an upgrade is
available, and below the next real release, so it does once upstream moves on.
Increase `<n>` for each package built on the same release; never reuse one.

```powershell
.\assay\pack.ps1 -Version 5.1.1-assay.1
```

The package goes to `..\stryker-net-packages`, where Assay's VSIX build looks for it.

## Moving to a new upstream release

```powershell
git fetch https://github.com/stryker-mutator/stryker-net.git --tags
git rebase --onto <new release commit> <old release commit> assay
```

A commit upstream already has drops out by itself. If one conflicts because
upstream fixed the same thing another way, drop it, then run its test against
the new release: if it passes, upstream's fix covers it.

## Patches

| Commit | Fix | Upstream |
|---|---|---|
| fix(compiling): read Features entries as name=value, and its follow-up (names ignoring case, quotes, commas) | Razor components fail to compile under Stryker: the SDK's `use-roslyn-tokenizer=true` reached the Razor generator under the wrong name | [stryker-net#4010](https://github.com/stryker-mutator/stryker-net/pull/4010) |
| fix(mtp): stop waiting on a discarded test host's coverage relay | Once one test timed out during per-test coverage, every later test waited 10 s on the dead host's relay and came back Dubious ([stryker-net#3871](https://github.com/stryker-mutator/stryker-net/issues/3871)) | branch `fix/mtp-discarded-host-relay`, pull request not yet opened |
| feat(mtp): log progress while per-test coverage is captured | The coverage step logged nothing between its first and last line; on SiteManager that was most of an hour, which read as a hang. Assay reads the new lines to estimate how long a run has left | branch `feat/mtp-coverage-progress`, pull request not yet opened |
| fix(initialisation): count every discovered test when the initial run ran them all | One failing test out of hundreds stopped the run with "more than 50% failing tests": the MTP runner reports a complete run as EveryTest, whose count is 0 | branch `fix/initial-run-every-test-count`, pull request not yet opened |

Package 5.1.1-assay.4 (tag stryker-5.1.1-assay.4) has every patch above; 5.1.1-assay.3 has all but the last, and 5.1.1-assay.2 all but the last two.
