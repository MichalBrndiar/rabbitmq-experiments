# RabbitMQ in .NET Core
This repository contains code that demostrates how to use RabbitMQ like producer and consumer using standard RabbitMQ.Client library.

## Run RabbitMQ in docker
```bash
docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

## How to run
In separate terminal
```bash
dotnet run --project MessageSender
```

In other terminal
```bash
dotnet run --project MessageReceiver
```

## Web management
**User**: guest

**Password**: guest