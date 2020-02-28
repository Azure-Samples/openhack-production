# Working agreement

A **Working Agreement** is a living document representing the principles and expected behavior of everyone involved in the project; it is not meant to be exhaustive nor complete. The team should be accountable to these standards and revisit, review, and revise as needed. The agreement is signed off by everyone.

---

## Table Of Contents

<!-- toc -->

- [Working Together](#working-together)
- [Development Process](#development-process)
- [Coding Together](#coding-together)
- [Automated Testing](#automated-testing)
- [Deployment](#deployment)

<!-- tocstop -->

## Working Together

- Attend all planning and standup meetings whenever possible. If we will not be able to attend a meeting, we will let the Scrum Leader and/or Product Manager as soon as possible.
- Be on time for meetings and give our full attention to the meeting's duration.
- We have blameless team culture and we will not use blaming words. For example, we will avoid “You broke this” or “You messed up”, and instead ask “How did this break?”, “Why did this break?”, “What steps broke it?”, “How do we prevent this?”, “How can I help?”
- Don't use bias language, Guys can be replaced by "Y'all" or "You all"

## Development Process

The team follows the [SCRUM](<https://en.wikipedia.org/wiki/Scrum_(software_development)>) agile development process with weekly sprints.

### The Scrum Leader

- The team will have a rotating scrum leader.
- Drive standup/planning/retro meetings and hold the team accountable for attendance and participation.
- Keep the meeting moving:
  - What you did yesterday
  - What you are going to do today
  - Blockers

### Work Item Tracking

The team adopted Azure DevOps for work item management and to help track their development processes using the integrated scrum boards, burn down charts and more.

## Coding Together

- We will be responsible for the work we do individually and as a team, collectively.
- Create software at the end of each iteration
- Design reviews and architecture reviews are case-by-case and determined by the team.

### Source Control

All projects are under [Git](https://git-scm.com/) for source control using a [mono repo](https://www.atlassian.com/git/tutorials/monorepos) design. The team will follow a [Git flow](https://docs.microsoft.com/en-us/azure/architecture/framework/devops/gitflow-branch-workflow) branching strategy and anything that is merged into master is expected to be in a shippable state.

### Pull Requests (PRs)

- All changes to the code must go through a code review process by opening a pull request in Azure DevOps.
- Keep PRs as small as possible.
- When someone is tagged to review a PR, they must reply to the request within 24 hours.

## Automated Testing

Automated test suites are run as part of the continuous integration (CI) pipelines. Any test failures will fail your builds. It's expected that all new changes to the codebase include new or updated Unit Tests (UT) to validate the code changes, the CI pipelines are gated with a UT code coverage check.

## Deployment

Continuous deployment (CD) (with Azure DevOps pipelines) is already in place to release any new infrastructure or code changes all the way through to production. The production environment is gated and requires manual approval for any new releases.
