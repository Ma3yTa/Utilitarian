# install and configure VS Code

# see https://code.visualstudio.com/docs/editor/extension-gallery#_command-line-extension-management

# https://github.com/tonsky/FiraCode/wiki/VS-Code-Instructions

choco install -y visualstudiocode
$env:Path += ";C:\Program Files\Microsoft VS Code\bin\"

# Utility
code --install-extension PKief.material-icon-theme
code --install-extension metaseed.metago
code --install-extension vstirbu.vscode-mermaid-preview
code --install-extension pnp.polacode
code --install-extension Shan.code-settings-sync
code --install-extension WakaTime.vscode-wakatime
code --install-extension Compulim.vscode-ipaddress
code --install-extension CoenraadS.bracket-pair-colorizer

# Formats
code --install-extension mikestead.dotenv
code --install-extension bungcip.better-toml
code --install-extension ionutvmi.reg
code --install-extension sidneys1.gitconfig

# Git
code --install-extension felipecaputo.git-project-manager
code --install-extension eamodio.gitlens

# fsharp
code --install-extension Ionide.ionide-fsharp

# Debuggers
code --install-extension msjsdiag.debugger-for-chrome

# Docker
code --install-extension PeterJausovec.vscode-docker

# Snippets
code --install-extension rebornix.project-snippets
code --install-extension nikitaKunevich.snippet-creator
code --install-extension bang.antd-snippets
code --install-extension demijollamaxime.bulma
