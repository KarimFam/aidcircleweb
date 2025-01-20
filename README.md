# AidCircle

AidCircle is a full-stack application designed to streamline operations for organizations, volunteers, and general users in the philanthropic and social service sectors. With AidCircle, you can manage users, organizations, items, volunteers, and orders, fostering a community-driven platform for coordinating aid and resources.

## Table of Contents
- [Features](#features)
- [Purpose](#purpose)
- [Architecture Overview](#architecture-overview)
- [Technology Stack](#technology-stack)
- [Naming Conventions](#naming-conventions)
- [Software Development Best Practices](#software-development-best-practices)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Contributing](#contributing)
- [License](#license)

---

## Features

1. **User Management**  
   - Register and manage user profiles.  
   - Support for volunteer roles, external authentication providers, and user-specific addresses.

2. **Organization Management**  
   - Create and maintain organization profiles.  
   - Each organization can have multiple addresses and items listed under it.

3. **Item Management**  
   - Create, view, and manage items (e.g., requests, events, todos).  
   - Assign items to volunteers or organizations.

4. **Volunteer Coordination**  
   - Assign volunteers to organizations or items.  
   - Track skills and availability of volunteers.

5. **Orders**  
   - Aggregate items into an order that can be fulfilled by volunteers or organizations.  
   - Single user ownership with the flexibility of multiple volunteers and organizations involved.

6. **Addresses**  
   - Unified and consistent management of addresses across users, volunteers, organizations, and items.

7. **Local Weather Integration**  
   - Display local weather information (e.g., for Tampa, FL) to assist in planning and logistics.

---

## Purpose

AidCircle aims to:
- **Facilitate Collaboration**: Connect volunteers, organizations, and end-users on a single platform.
- **Streamline Resource Management**: Manage items and orders efficiently, ensuring the right resources reach those in need.
- **Enhance Communication**: Provide clear and consistent channels for volunteers, organizations, and donors to collaborate.
- **Promote Accountability**: Keep track of items, orders, and volunteer efforts in a transparent way.

---

## Architecture Overview

AidCircle employs a **clean, multi-layered architecture** that separates concerns and ensures maintainability, scalability, and testability:

1. **Domain Layer**  
   - Contains core business entities (e.g., `User`, `Volunteer`, `Organization`, `Item`, `Order`, `Address`).  
   - Free from external dependencies, focusing solely on business logic.

2. **Application Layer**  
   - Implements application-specific logic through services and interfaces (e.g., `UserService`, `OrderService`).  
   - Acts as a mediator between the domain and presentation layers.

3. **Infrastructure Layer**  
   - Houses data persistence and external integrations (e.g., EF Core repositories, weather API).  
   - Implements repository interfaces defined in the domain layer.

4. **Presentation Layer**  
   - Contains Blazor Server (and potentially Blazor Hybrid for mobile) UI components.  
   - Handles user interactions, rendering Razor components, and calling application services.

---

## Technology Stack

- **.NET 7 / C#**: Core application and domain logic.  
- **Blazor Server**: Web-based user interface.  
- **Entity Framework Core**: Data access and repository implementation.  
- **SQL Server (Azure-ready)**: Primary database for storage.  
- **FluentValidation / Data Annotations**: Input validation.  
- **Swagger (Optional)**: API documentation.

---

## Naming Conventions

- **Classes & Methods**: PascalCase (e.g., `UserService`, `GetLatestOrdersAsync`)  
- **Properties & Fields**: PascalCase (e.g., `Username`, `CreatedDate`)  
- **Local Variables & Parameters**: camelCase (e.g., `userService`, `orderId`)  
- **Interfaces**: Prefixed with `I` (e.g., `IUserService`, `IOrderRepository`)  
- **Folders**: Organized by feature or layer (e.g., `Domain/Entities`, `Infrastructure/Repositories`).  
- **Files**: Named according to their contained class or responsibility.

---

## Software Development Best Practices

- **SOLID Principles**: Promotes clean, maintainable code (Single Responsibility, Open-Closed, Liskov Substitution, Interface Segregation, Dependency Inversion).  
- **DRY (Don't Repeat Yourself)**: Avoid duplication by encapsulating common functionality in services and reusable components.  
- **Layered Architecture**: Enforces separation of concerns, improving testability and scalability.  
- **Dependency Injection**: Reduces coupling between layers and improves flexibility.  
- **Unit & Integration Testing**: Encouraged in both the domain and application layers to ensure reliability.

---

## Project Structure

```plaintext
AidCircle.Solution
│
├── AidCircle.Domain
│   ├── Entities (User, Volunteer, Organization, Item, Order, Address, etc.)
│   ├── Enums (ItemType, AddressType, etc.)
│   ├── Interfaces (IUserRepository, IOrderRepository, etc.)
│   └── ValueObjects (Potential value objects)
│
├── AidCircle.Application
│   ├── Interfaces (IUserService, IOrderService, etc.)
│   ├── Services (UserService, OrderService, etc.)
│   ├── Validators (UserValidator, OrganizationValidator)
│   └── DTOs (Optional if needed)
│
├── AidCircle.Infrastructure
│   ├── Data (AidCircleDbContext, Migrations)
│   ├── Repositories (UserRepository, OrderRepository, etc.)
│   ├── ExternalServices (Weather API integration)
│   └── Logging (Optional)
│
├── AidCircle.Presentation.Web
│   ├── Pages (Razor components for CRUD operations)
│   ├── Shared (Layout and shared UI components)
│   ├── Services (Optional front-end specific services)
│   └── wwwroot (Static content like CSS, JS)
│
└── AidCircle.Presentation.API (Optional if a separate API is needed)
    ├── Controllers (UserController, OrderController, etc.)
    └── Models (Any API-specific models)
