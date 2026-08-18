# Sistema de Estoque

Documentação técnica e guia de ambiente para desenvolvedores.

---

## 🛠️ Tecnologias e Gerenciamento de Versões

Neste projeto, utilizamos o **[mise](https://mise.jdx.dev/)** para gerenciar a versão do **.NET SDK**. 

* **Versão do .NET:** **.NET 10 (LTS)** — Versão mais recente com Suporte de Longo Prazo (Long-Term Support).
* **Arquivo de configuração:** A versão exata do .NET está fixada no arquivo `.mise.toml` na raiz do repositório.

---

## 🐧 Guia de Instalação no Ubuntu

Siga os passos abaixo para configurar o ambiente de desenvolvimento no Ubuntu.

### 1. Instalar o `mise`

Você pode instalar o `mise` no Ubuntu através do script oficial ou via repositório APT:

#### Opção A: Instalação Rápida (Script Oficial)
```bash
curl https://mise.run | sh
```

#### Opção B: Instalação via Repositório APT
```bash
# Adicionar a chave GPG e o repositório
sudo apt update && sudo apt install -y curl gpg
gpg --keyserver hkps://keyserver.ubuntu.com --recv-keys 0x0AB051D4590300B4 2>/dev/null || true
curl -fsSL https://mise.jdx.dev/gpg-key.pub | gpg --dearmor | sudo tee /etc/apt/keyrings/mise-archive-keyring.gpg > /dev/null
echo "deb [signed-by=/etc/apt/keyrings/mise-archive-keyring.gpg arch=amd64] https://mise.jdx.dev/deb stable main" | sudo tee /etc/apt/sources.list.d/mise.list

# Instalar o mise
sudo apt update && sudo apt install -y mise
```

---

### 2. Configurar a Shell

Após instalar o `mise`, adicione a integração à sua shell (Bash ou Zsh) para que as ferramentas sejam ativadas automaticamente ao entrar no repositório:

#### Para **Bash**:
```bash
echo 'eval "$(~/.local/bin/mise activate bash)"' >> ~/.bashrc
source ~/.bashrc
```
*(Se você instalou via APT, utilize `eval "$(mise activate bash)"`)*

#### Para **Zsh**:
```bash
echo 'eval "$(~/.local/bin/mise activate zsh)"' >> ~/.zshrc
source ~/.zshrc
```

---

### 3. Instalar o .NET 10 com o `mise`

Com o `mise` configurado, navegue até a pasta do projeto e execute o comando de instalação:

```bash
# Clona/acessa a pasta do projeto
cd estoque

# Instala automaticamente as ferramentas listadas no .mise.toml (.NET 10 LTS)
mise install
```

Caso queira fixar ou alterar manualmente a versão do .NET no projeto via `mise`:
```bash
mise use dotnet@10
```

---

## 🔍 Verificação do Ambiente

Para confirmar que o .NET 10 (LTS) foi instalado e está ativo:

```bash
dotnet --version
```
*Saída esperada:* Uma versão iniciando com `10.x.x` (ex: `10.0.302`).

Se a integração com a shell ainda não estiver ativa na sessão atual, você pode executar utilizando o `mise`:
```bash
mise exec -- dotnet --version
```

---

## 📋 Pré-requisitos Gerais
- **Gerenciador de Versões:** `mise`
- **SDK:** .NET 10 (LTS)
- **IDE Recomendada:** VS Code (com extensão *C# Dev Kit*), Visual Studio 2022 ou JetBrains Rider.
