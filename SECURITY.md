# Security policy

## Report privately

Send suspected vulnerabilities to [Quinntyne Brown](mailto:quinntynebrown@gmail.com)
with the subject `FaithTech event platform security report`. This is the maintainer's
[published reporting contact](https://github.com/QuinntyneBrown/quinntyne-brown-studio/blob/main/SECURITY.md).
Keep exploit details out of public issues and pull requests.

Include the affected commit, component, reproduction steps using fictional data,
required access, expected security boundary, and observed impact. Redact logs;
do not send passwords, entry codes, session cookies, digest keys, participant data,
private messages, or database dumps. Ask how to share sensitive evidence first.
Only investigate systems and accounts you have authorization to test.

## Scope and follow-up

Reports may concern the API, Angular applications, operator CLI, design system,
dependencies, or deployment tooling. Include whether the issue reproduces on
current `main`; the project is still under construction and has not passed
production acceptance. See [implementation evidence](docs/IMPLEMENTATION.md).

Allow time for investigation and coordinate disclosure with the maintainer.
Follow up through the same private channel; this document promises no response
deadline or bounty. State whether you want acknowledgment when a fix is published.

Operators should follow the [deployment runbook](docs/azure-production-deployment.md)
and [deployment acceptance criteria](docs/specs/deployment-acceptance.md).
Use [support](SUPPORT.md) for ordinary bugs and setup questions.
