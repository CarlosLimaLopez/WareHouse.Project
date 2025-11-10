# Warehouse API – Scalable Solution with CQRS Architecture

## Overview

This repository contains the **Warehouse API** project, designed with modern software architecture principles to ensure scalability, maintainability, and high availability.

## Key Technologies & Concepts

- **.NET 8**: Main framework for API development.
- **CQRS (Command Query Responsibility Segregation)**: Clear separation between command and query operations for better scalability and maintainability.
- **DDD (Domain-Driven Design)**: The domain model is designed following DDD principles, ensuring business logic is well-encapsulated and aligned with real-world processes.
- **Hexagonal Architecture (Ports & Adapters)**: The solution is structured to isolate the domain from external dependencies, making testing and technology changes easier.
- **MassTransit**: Distributed application framework for building message-based systems.
- **RabbitMQ**: Message broker used for asynchronous communication and event-driven architecture.
- **Entity Framework Core**: ORM for .NET, used for data access and persistence.
- **SQL Server**: Relational database engine for storing and querying structured data.
- **Docker**: The entire solution is containerized for portability and easy deployment.
- **Kubernetes**: Application deployment and management are handled in a Kubernetes cluster, enabling advanced orchestration and high availability.
- **Autoscaling**: Uses Kubernetes Horizontal Pod Autoscaler (HPA) to automatically adjust the number of replicas based on load.
- **Prometheus**: Integrated for API monitoring and metrics collection.

### Key Components

#### Main Solution Projects
- **WareHouse.API/**: Main API project for core warehouse operations, serving as the foundation for business logic, integration, and orchestration with other services.
- **WareHouse.API.Query/**: Query-side API implementing CQRS, DDD, and hexagonal architecture, focused on scalable read operations.
- **WareHouse.Model/**: Domain models, value objects, and aggregates, following DDD principles.
- **WareHouse.Data/**: Data access layer using Entity Framework Core, responsible for database context, repositories, and persistence.
- **WareHouse.Services/**: Application and domain services, business logic, orchestration, and messaging integration (MassTransit, RabbitMQ).

#### Integration Test Projects
- **WareHouse.API.Query.IntegrationTests/**: Integration tests for the query API, validating end-to-end scenarios and infrastructure integration.
- **WareHouse.API.IntegrationTests/**: Integration tests for the main API, ensuring correct behavior of core operations and service interactions.
- **WareHouse.Data.IntegrationTests/**: Integration tests for the data layer, verifying database operations and repository logic.

#### Unit Test Projects
- **WareHouse.API.Query.UnitTests/**: Unit tests for the query API, covering controllers, services, and business logic.
- **WareHouse.API.UnitTests/**: Unit tests for the main API, ensuring correctness of individual components.
- **WareHouse.Data.UnitTests/**: Unit tests for the data layer, focusing on repository and data access logic.
- **WareHouse.Services.UnitTests/**: Unit tests for application and domain services.

#### Infrastructure & Configuration
- **Infrastructure/Docker/docker-compose.yml**: Orchestrates multi-container local development, including SQL Server, RabbitMQ, the main API (`warehouse-api`), and the query API (`warehouse-api-query`).
- **Infrastructure/Kubernetes/**: Contains Kubernetes manifests for deploying the APIs, including deployment configurations and autoscaling settings.

> This structure supports clean separation of concerns, testability, scalability, and maintainability, leveraging modern DevOps and architectural best practices.
