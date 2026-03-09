# Personnel Performance Evaluation System

ASP.NET Core (.NET 8) based employee performance evaluation system with a multi-stage approval workflow.

---

## Overview

This project is a web-based platform developed to manage employee performance evaluations through a structured and role-based workflow.

The evaluation process progresses through multiple stages including managers, final reviewers and HR departments.

---

## Features

- Multi-stage evaluation workflow  
  **Manager 1 → Manager 2 → Final Manager → HR**

- Automated score calculation  
- Role-based authorization system  
- Excel import and export for bulk operations  
- Evaluation period management  
- Customizable UI theme and logo  
- Audit logging for system actions  
- Reporting and Excel export functionality

---

## Technology Stack

### Backend
- .NET 8
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server

### Frontend
- Razor Views
- Bootstrap
- jQuery
- Custom CSS

---

## Project Structure

```
PerformansSitesi
│
├── Controllers
├── Views
├── Application
│   └── Services
├── Domain
│   ├── Entities
│   └── Enums
├── Infrastructure
│   ├── Data
│   └── Seed
├── Web
│   ├── ViewModels
│   ├── Helpers
│   └── Filters
├── wwwroot
└── Properties
```

---

## Getting Started

### Requirements

- Visual Studio 2022
- .NET 8 SDK
- SQL Server

### Run the project

1. Open the solution

```
PerformansSitesi.sln
```

2. Apply database migrations

```
Update-Database
```

or

```
dotnet ef database update
```

3. Run the application

```
dotnet run
```

---

## Roles

| Role | Description |
|-----|-------------|
| SystemAdmin | Full system control |
| Admin | System administration |
| HR | Evaluation management |
| FinalManager | Final approval |
| Manager2 | Secondary evaluation |
| Manager1 | Initial evaluation |

---

## Author

Developed by **Zeynep Tuncay**

---

## License

Internal use
