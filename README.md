# PangYaSeason2-BR
 

> ⚠️ **Este projeto é fornecido como base de estudo. Você é livre para modificar, adaptar ou utilizar como quiser.**

---
 ### 📌 Visão Geral

Este projeto simula os principais componentes de um servidor PangYa:

- **LoginServer** – Autenticação de jogadores.
- **GameServer** – Lobby, salas e partidas.
- **RankServer** – Rank dos jogadores, melhores 12 jogadores no map e etc...
- **AuthServer** – Sicronia entre os servidores, dados, envio e conversa entre si. 

---
### ✅ Status do Projeto

| Componente       | Progresso |
|------------------|-----------|
| GameServer       | 20%       |
| LoginServer      | 100%      |
| RankServer       | 0%        |
| AuthServer       | 0%        |

---

### 🧩 Requisitos

Você vai precisar de alguns programas e ferramentas:

- [Visual Studio](https://visualstudio.microsoft.com/pt-br/) – para compilar o projeto.
- [SQL Server](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) – para gerenciar o banco de dados.
- Cliente do **Pangya BR S2**.

---
---

### 🧱 Arquitetura do Projeto

O PangYaSeason2-BR
 é dividido em 5 principais bibliotecas (`PangyaAPI`) que organizam o código de forma modular:

| API                        | Função principal                                                                      |
|----------------------------|---------------------------------------------------------------------------------------|
| **PangyaAPI.Network**      | Gerencia conexões TCP, sessões, buffers, envio/recebimento e tratamento de pacotes.   |
| **PangyaAPI.SQL**          | Interface de acesso ao banco de dados (SQL Server), comandos e respostas assíncronas. |
| **PangyaAPI.IFF.BR.S2**       | Manipula os arquivos IFF do cliente japonês (itens, personagens, cursos etc.).        |
| **PangyaAPI.Utilities**    | Ferramentas auxiliares: Log, enums, config `.ini`, criptografia, estrutura de erros.  |

Essa separação torna o código mais limpo, reutilizável e facilita a manutenção e expansão.

### 🚀 Como começar

> **Nota:** Eu não vou ensinar como conectar o servidor ao cliente, mas...  
> 💡 **Dica:** leia os comentários no código — cada parte tem explicações úteis para te guiar!

---

### 🧠 Dicas rápidas

- Confira os arquivos `.ini` para ajustar configurações de porta, IP e nome do servidor.
- Observe o `pangya_packet_handle.cs` para entender como os pacotes são tratados.
- Observe o `SessionManager.cs` para entender como os jogadores são tratados.
- Use os logs no console para debugar conexões e autenticações.

---

### 🖼️ Capturas de Tela
 
---

### 👨‍💻 Autores

| Nome           | Função         | Projeto                          |
|----------------|----------------|----------------------------------|
| **Luis MK**    | Make           | [Dev Pangya Unogames](https://github.com/luismk)  
| **Eric**       | Contribuidor   | [Old ADM Pangya Unogames](https://github.com/eantoniobr)
| **Vitinho**    | Contribuidor   | [Old ADM Pangya Unogames](https://github.com/vitinhonunes)

---

### 📜 Licença

Este projeto não possui uma licença formal. Use por sua conta e risco.  
**Não recomendado para uso comercial sem entendimento profundo do código.**

---
