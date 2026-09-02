# Padrão de registro e entrega de evidências

- **Cartão:** `[S1-DOC-011] Padronizar evidências - registro de entregas e testes`
- **Sprint:** Sprint 1
- **Prazo formal:** 09/09/2026 às 23:59 (`America/Sao_Paulo`)
- **Versão:** 1.0
- **Data:** 01/09/2026
- **Situação:** pronto para revisão por outro integrante

## 1. Objetivo

Definir um padrão simples e único para registrar evidências de Pull Requests, testes, telas, documentos e demonstrações do SQUAD Estoque.

O cartão do Trello funciona como índice da entrega. A fonte de verdade permanece no Pull Request, no commit, no resultado do CI ou no arquivo versionado correspondente.

Uma evidência válida deve permitir que outra pessoa identifique:

- qual cartão foi atendido;
- quem realizou a entrega ou validação;
- quando a evidência foi produzida;
- qual versão do sistema ou documento foi avaliada;
- qual cenário foi executado;
- qual era o resultado esperado;
- qual foi o resultado observado;
- onde a evidência pode ser conferida.

## 2. Resumo da forma de entrega

1. Produzir a evidência vinculada ao cartão e ao commit correto.
2. Conferir legibilidade, contexto, resultado e ausência de dados sensíveis.
3. Publicar a fonte principal no PR, no repositório ou em artefato protegido.
4. Registrar no cartão um resumo e os links permanentes.
5. Solicitar revisão de outro integrante.
6. Após a aprovação e as verificações aplicáveis, manter no cartão os links finais.

Não duplicar arquivos grandes quando um link permanente, acessível à equipe e com retenção adequada for suficiente.

## 3. Evidência mínima por tipo

| Tipo | Conteúdo mínimo | Fonte preferencial |
| --- | --- | --- |
| Pull Request | Link do PR, cartão relacionado, objetivo, escopo, revisão e situação do CI | GitHub / PR |
| Teste automatizado | Commit testado, comando ou job, casos executados e resultado | Job do CI ou log protegido |
| Teste manual | Perfil, tela ou rota, entrada, esperado, observado, ambiente e executor | PR ou relatório versionado |
| Tela ou UX | Imagem real, cenário, viewport, perfil, commit e resultado da revisão | PNG versionado ou anexo referenciado |
| Documento | Arquivo revisado, versão, objetivo, responsável e commit do conteúdo | Repositório / PR |
| Demonstração | Data, cartão, item demonstrado, resultado e links técnicos de suporte | Registro no cartão |

### Não usar como evidência única

- frases como "funcionou" ou "testado" sem contexto e resultado observável;
- protótipo para comprovar execução da aplicação;
- banco local, arquivo temporário ou captura sem vínculo com o commit;
- reunião, movimentação do Trello ou quantidade de cartões como substituto da entrega técnica;
- imagem simulada, vazia ou renomeada apenas para preencher uma estrutura.

## 4. Campos obrigatórios

| Campo | Como preencher |
| --- | --- |
| ID da evidência | `EV-{cartão}-{sequência}`; exemplo: `EV-S1-DOC-011-001` |
| Data e hora | `DD/MM/AAAA HH:MM` e fuso `America/Sao_Paulo` |
| Cartão | Identificador e título do cartão relacionado |
| Responsável | Pessoa que executou e registrou a validação |
| Tipo | PR, teste automatizado, teste manual, tela, documento ou demonstração |
| Versão | Hash curto do commit; para documento, incluir também a versão do arquivo |
| Ambiente | CI, Ubuntu ou Windows; navegador, sistema operacional e viewport quando relevantes |
| Cenário | Entrada, pré-condições e caso de teste ou requisito, quando existente |
| Resultado | Esperado, observado e situação: aprovado, falhou, bloqueado ou não aplicável |
| Local | Link permanente para PR, job, commit, arquivo versionado ou artefato protegido |
| Revisão | Nome do revisor, data e observações ou pendência de revisão |

## 5. Convenção de nomes

Usar nomes curtos, sem espaços e sem dados pessoais:

```text
s{n}-categoria-id_aaaa-mm-dd_tipo-descricao.ext
```

Exemplo:

```text
s1-doc-011_2026-09-01_padrao-evidencias.md
```

Para os protótipos já definidos, preservar os nomes oficiais:

- `lojista-desktop.png`;
- `lojista-tablet.png`;
- `vendedor-mobile.png`.

Evitar sufixos vagos como `final`, `novo` ou `corrigido2`. A versão deve ser identificada pelo Git.

## 6. Fluxo de entrega

### 6.1 Confirmar o escopo

Ler o cartão, os critérios de aceite, a Sprint e as dependências. Definir quais evidências serão necessárias antes de executar a atividade.

### 6.2 Trabalhar na versão correta

Usar a branch do cartão e registrar o commit que será validado. Não misturar entregas independentes.

### 6.3 Executar a validação

Compilar, executar testes proporcionais ao risco e realizar a validação manual aplicável.

### 6.4 Coletar a evidência

Registrar os campos obrigatórios, o resultado real e somente os arquivos necessários. Não alterar ou omitir uma falha para aparentar aprovação.

### 6.5 Sanitizar e revisar

Remover dados sensíveis e conferir legibilidade, correspondência com o commit e ausência de arquivos indevidos.

### 6.6 Publicar na fonte principal

Incluir a evidência no PR, no repositório ou em artefato protegido. Preferir links duráveis e acessíveis à equipe.

### 6.7 Referenciar no cartão

O integrante responsável registra no cartão o resumo da entrega, a situação, o link do PR e as demais evidências. O arquivo não precisa ser duplicado no Trello quando o link permanente for suficiente.

### 6.8 Solicitar revisão por par

O revisor confere escopo, resultado, segurança, clareza e possibilidade de reprodução antes de aprovar.

### 6.9 Concluir após integração

Depois da aprovação, das verificações aplicáveis e da integração na branch definida pela equipe, validar o resultado integrado e manter os links finais vinculados ao cartão.

## 7. Procedimento por tipo de evidência

### 7.1 Pull Request

- relacionar o cartão e declarar o objetivo e as áreas alteradas;
- informar como validar, testes executados, validações manuais, riscos e limitações;
- registrar impacto em banco, migration, autenticação e documentação;
- aguardar revisão de outra pessoa e verificações automáticas aplicáveis antes da integração.

### 7.2 Teste automatizado

- vincular o resultado ao commit testado e ao job do CI;
- registrar casos executados, aprovados, falhos, bloqueados e não aplicáveis;
- guardar apenas o trecho relevante do log;
- quando houver falha, registrar o observado e o defeito associado.

### 7.3 Teste manual

- registrar perfil, rota ou tela, entrada, pré-condições, esperado e observado;
- informar Ubuntu ou Windows e, quando visual, navegador e viewport;
- em cenários de saldo, verificar o estado antes e depois e os registros persistidos;
- repetir no ambiente relevante quando a mudança puder variar entre plataformas.

### 7.4 Tela e UX

- capturar a tela real ou o protótipo correto e identificar claramente a origem;
- remover barras do navegador, notificações e dados pessoais;
- garantir que textos, estados e mensagens estejam legíveis;
- registrar perfil, cenário, viewport e commit correspondente;
- salvar em PNG e, para os protótipos oficiais, usar a pasta e os nomes já definidos.

### 7.5 Documento

- identificar versão, data, cartão, responsável e fontes consultadas;
- revisar clareza, ortografia, links, privacidade e coerência com o sistema;
- versionar o arquivo Markdown para facilitar revisão e histórico;
- gerar PDF apenas quando for necessária uma cópia estável para leitura ou entrega;
- evitar reescrever documentação existente e referenciar a fonte oficial.

## 8. Privacidade, segurança e retenção

### 8.1 Nunca versionar nem anexar

- senhas, tokens, chaves, segredos, cookies completos, hashes de senha ou credenciais privadas;
- bancos locais (`*.db`, `*.db-wal`, `*.db-shm`, `*.sqlite`, `*.sqlite3`);
- arquivos de build como `bin/` e `obj/`;
- dados pessoais, caminhos privados ou telas de outras aplicações;
- logs integrais quando um resumo ou trecho sanitizado comprovar o resultado;
- capturas com barras do navegador, notificações, e-mails pessoais ou identificadores desnecessários.

### 8.2 Tratamento de situações comuns

| Situação | Ação correta |
| --- | --- |
| Credencial apareceu na tela | Não anexar. Revogar ou trocar quando necessário e refazer a captura com dados de teste |
| Log contém cookie ou token | Gerar trecho sanitizado e preservar somente horário, caso e resultado necessários |
| Arquivo é grande | Usar link permanente e controlado; no cartão, registrar descrição e localização |
| Evidência contém dado pessoal | Remover ou anonimizar antes de publicar; se não for possível, usar área protegida |
| Link pode expirar | Registrar também PR, commit, resumo do resultado e local de retenção |
| Resultado é uma falha | Preservar a falha como evidência, registrar o impacto e vincular o defeito |

## 9. Checklist de clareza e privacidade

- [ ] O arquivo abre e está legível.
- [ ] O cartão, o responsável, a data e o commit estão identificados.
- [ ] O resultado esperado, o observado e a situação são coerentes.
- [ ] O link pode ser acessado por quem fará a revisão.
- [ ] Não há senha, cookie, token, banco local, dado pessoal ou caminho privado.
- [ ] A evidência corresponde ao sistema ou protótipo declarado, sem simulação enganosa.
- [ ] Não existe duplicação desnecessária de arquivo grande.
- [ ] Outra pessoa consegue reproduzir ou compreender a validação.

## 10. Modelo reutilizável

```markdown
**ID:** EV-S{n}-{categoria}-{id}-{seq}
**Data/hora:** DD/MM/AAAA HH:MM (America/Sao_Paulo)
**Cartão:** [S{n}-{categoria}-{id}] Título
**Responsável:** Nome
**Tipo:** PR | teste automatizado | teste manual | tela | documento | demonstração
**Versão:** commit / versão do arquivo
**Ambiente:** CI | Ubuntu | Windows; navegador e viewport, se aplicável
**Cenário/entrada:** descrição objetiva
**Resultado esperado:** comportamento verificável
**Resultado observado:** resultado real
**Situação:** aprovado | falhou | bloqueado | não aplicável
**Local da evidência:** link do PR, job, commit, arquivo ou artefato protegido
**Revisão:** revisor, data e observações
```

## 11. Exemplo preenchido

Este exemplo representa a entrega do próprio padrão. Os dados do PR, commit e revisor devem ser substituídos pelos valores reais antes do registro no cartão.

| Campo | Registro de exemplo |
| --- | --- |
| ID | `EV-S1-DOC-011-001` |
| Data e hora | `01/09/2026 19:00 (America/Sao_Paulo)` |
| Cartão | `[S1-DOC-011] Padronizar evidências - registro de entregas e testes` |
| Responsável | Integrante responsável pelo cartão |
| Tipo | Documento / revisão |
| Versão | `1.0` e hash do commit do PR |
| Ambiente | Markdown renderizado no GitHub e leitura local |
| Cenário | Abrir o arquivo, percorrer todas as seções e verificar conteúdo, links e privacidade |
| Resultado esperado | Documento legível, completo, coerente e sem dados sensíveis |
| Resultado observado | Documento criado e verificações locais concluídas |
| Situação | Pronto para revisão por par |
| Local | `docs/07-operacional/s1-doc-011-padrao-entrega-evidencias.md` |
| Revisão | Pendente de nome, data e parecer do revisor da equipe |

## 12. Forma de entrega desta atividade

1. Versionar este arquivo na branch `docs/s1-doc-011-felipe`.
2. Usar um commit documental claro, vinculado ao cartão.
3. Enviar a branch ao repositório remoto.
4. Abrir Pull Request para a branch-base definida pela equipe, sem direcioná-lo automaticamente para `main`.
5. No PR, informar objetivo, arquivo criado, forma de validação e ausência de impacto em código, banco e autenticação.
6. Solicitar revisão de outro integrante.
7. O responsável pelo cartão registra o link do PR e a evidência final no Trello.

### Texto curto para o Pull Request

```markdown
## Cartão
[S1-DOC-011] Padronizar evidências - registro de entregas e testes

## Objetivo
Definir um padrão único para registrar e entregar evidências do projeto.

## Alteração
Inclusão do guia em Markdown com campos mínimos, fluxo de entrega, procedimentos por tipo, privacidade, checklist e exemplo preenchido.

## Como validar
1. Abrir o arquivo Markdown.
2. Conferir a renderização das tabelas, listas e blocos de código.
3. Revisar ortografia, links, coerência e privacidade.

## Evidências
- Arquivo criado e revisado localmente.
- `git diff --check` sem erros.

## Impactos
- Código da aplicação: não.
- Banco ou migration: não.
- Autenticação: não.
- Documentação: atualizada.
```

### Texto curto para o cartão

```markdown
Entrega: padrão de registro e entrega de evidências concluído.
Arquivo: docs/07-operacional/s1-doc-011-padrao-entrega-evidencias.md
Conteúdo: campos mínimos, locais de armazenamento, fluxo por tipo, privacidade, checklist e exemplo.
Validação: Markdown revisado; revisão por par pendente/concluída em [data].
PR/commit: [inserir link permanente].
```

## 13. Critérios de aceite

- [x] Define nome, data, cartão, responsável, tipo, versão, ambiente, resultado e local.
- [x] Explica onde publicar PRs, testes, telas, documentos e demonstrações.
- [x] Estabelece fluxo de entrega e revisão por par.
- [x] Impede versionamento de segredos, bancos locais e capturas com credenciais.
- [x] Evita duplicar arquivos grandes quando um link permanente é suficiente.
- [x] Inclui checklist, modelo reutilizável e exemplo preenchido.
- [ ] Recebeu revisão de outro integrante.
- [ ] Foi vinculado ao PR e ao cartão com dados reais.

## 14. Referências

- [README do projeto](../../README.md);
- [Guia de contribuição](../../CONTRIBUTING.md);
- [Guia do fluxo de desenvolvimento XP](guia-fluxo-desenvolvimento-xp.md);
- [Plano de testes](../09-testing/plano-de-testes.md);
- [Especificação de testes do vendedor](../09-testing/especificacao-testes-vendedor.md);
- [Protocolo de evidências dos protótipos](../05-ux/evidencias/README.md);
- Manual de governança do Trello de Felipe, mantido como arquivo de gestão externo a este repositório.

## 15. Histórico

| Versão | Data | Alteração | Responsável |
| --- | --- | --- | --- |
| 1.0 | 01/09/2026 | Versão inicial com forma de entrega, campos mínimos, segurança, checklist e exemplo | Equipe SQUAD Estoque |
