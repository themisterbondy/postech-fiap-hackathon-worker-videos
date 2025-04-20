# 🎥 Postech.Fiap.Hackathon.VideoProcessing.Worker

> Worker de processamento de vídeos utilizado no Hackathon FIAP. Responsável por extrair frames de vídeos de forma assíncrona e enviar notificações em caso de falha.

---

## 📌 Funcionalidades

- Consumo de mensagens de uma fila do Azure Queue Storage
- Processamento de vídeo com FFMpegCore
- Extração de frames
- Atualização de status no banco de dados
- Envio de notificações por e-mail
- Agendamento de tarefas com Quartz.NET
- Logging estruturado com Serilog

---

## 🧱 Tecnologias

- **.NET 9.0**
- **Quartz.NET** – Agendamento de jobs
- **Azure Queue Storage** – Consumo de mensagens
- **FFMpegCore** – Processamento de vídeo
- **Entity Framework Core** – Acesso ao banco de dados
- **FluentValidation** – Validação de dados
- **Identity Framework Core** – Identidade de usuários
- **Serilog** – Log estruturado
- **Result Pattern** – Padronização de retorno
- **MediatR** – Comunicação entre componentes
- **Helm** – para deploy no Kubernetes

---

## 🚀 Execução Local

### CLI

```bash
dotnet build
dotnet run --project src/Postech.Fiap.Hackathon.VideoProcessing.Worker
```

### Docker

```bash
docker build -t fiap-hackathon-worker .
docker run --env-file .env fiap-hackathon-worker
```

---

## 📦 CI/CD

O projeto utiliza GitHub Actions com etapas para:

- Build da aplicação
- Execução de testes
- Build e push da imagem Docker
- Deploy no Kubernetes com Helm

---

## 🧰 Testes

```bash
dotnet test tests/Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests
```

---

## 📧 Notificações

Em caso de sucesso ou falha no processamento de vídeo, um e-mail será enviado ao usuário.

---