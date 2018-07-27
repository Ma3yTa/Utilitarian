# Contributing to Utilitarian

## Conversational Development

There are several issue templates in place to communicate intent efficiently.

### Github

* [start a conversation](https://github.com/kfrie/Utilitarian/issues/new?template=Conversation.md)
* [propose an enhancement](https://github.com/kfrie/Utilitarian/issues/new?template=Enhancement.md)
* [propose a new feature / MVP](https://github.com/kfrie/Utilitarian/issues/new?template=Feature.md)
* [propose a research topic](https://github.com/kfrie/Utilitarian/issues/new?template=Research.md)
* [report a bug](https://github.com/kfrie/Utilitarian/issues/new?template=Bug.md)

## Repository

I plan to use the [CHANGELOG](CHANGELOG.md) at repository level for longer-term documentation of development experience and for bookkeeping of changes or features that have an effect on all projects maintained within the repository.

Every application or library within the repository is supposed to have its own release cycle, with a `CHANGELOG.md` located within its subfolder.

The repository is maintained according to [GitLab Flow](https://docs.gitlab.com/ce/workflow/gitlab_flow.html). For now, the project follows the [mono repo](https://medium.com/@maoberlehner/monorepos-in-the-wild-33c6eb246cb9) pattern, since centralization of knowledge is a key factor. Once this leads to problems, there is still the [meta repo](https://medium.com/@patrickleet/mono-repo-or-multi-repo-why-choose-one-when-you-can-have-both-e9c77bd0c668) pattern to try out. The modular and versatile nature of the [FAKE build system](https://fake.build/) and the powerful [Paket package manager](https://fsprojects.github.io/Paket/index.html) with its [independent resolution groups](https://fsprojects.github.io/Paket/groups.html) will probably make it scale well enough though.

## Maintainers

| Name        | Github           | Gitlab  | Twitter | Slack
| ------------- |:-------------:| :-----:| :-----:| :-----:|
| Kai Friedrich | [kfrie](https://github.com/kfrie) | [kfrie](https://gitlab.com/kfrie) | [@taleOfFsharp](https://twitter.com/taleOfFsharp) | [kfrie](https://fsharp.slack.com/team/U8XU0S362)
