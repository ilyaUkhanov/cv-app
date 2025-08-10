# CV App - Dockerized .NET MVC with PostgreSQL

This is a dockerized ASP.NET Core MVC application with PostgreSQL database and Razor templating capabilities.

## Features

- **ASP.NET Core MVC** with Razor Pages for templating
- **PostgreSQL** database with Entity Framework Core
- **Docker Compose** for easy deployment
- **Template Management** - Create, edit, and render templates
- **Variable Substitution** - Use `{{PropertyName}}` syntax in templates

## Prerequisites

- Docker Desktop
- Docker Compose

## Quick Start

1. **Clone and navigate to the project:**
   ```bash
   cd cv-app
   ```

2. **Start the application with Docker Compose:**
   ```bash
   docker-compose up --build
   ```

3. **Access the application:**
   - Web Application: http://localhost:5000
   - PostgreSQL Database: localhost:5432

## Application Structure

### Backend (.NET MVC)
- **Controllers**: Handle HTTP requests and responses
- **Models**: Entity Framework models for database entities
- **Views**: Razor views for templating and UI
- **Data**: Entity Framework DbContext and database configuration

### Database (PostgreSQL)
- **Templates**: Store template content and metadata
- **Users**: User management (extensible)

## Template System

The application includes a template system where you can:

1. **Create Templates**: Use the web interface to create templates with variable substitution
2. **Edit Templates**: Modify existing templates
3. **Render Templates**: See how templates render with sample data

### Template Syntax

Use `{{PropertyName}}` syntax for variable substitution:

```
Hello {{Name}},

Your email is {{Email}} and your skills include:
{{#each Skills}}
- {{this}}
{{/each}}

Best regards,
CV App
```

## API Endpoints

- `GET /Template` - List all templates
- `GET /Template/Create` - Create new template form
- `POST /Template/Create` - Create new template
- `GET /Template/Edit/{id}` - Edit template form
- `POST /Template/Edit/{id}` - Update template
- `GET /Template/Render/{id}` - Render template with sample data
- `GET /Template/Delete/{id}` - Delete template confirmation
- `POST /Template/Delete/{id}` - Delete template

## Development

### Running Locally (without Docker)

1. **Install .NET 8 SDK**
2. **Install PostgreSQL** and create a database
3. **Update connection string** in `appsettings.json`
4. **Run the application:**
   ```bash
   cd backend
   dotnet run
   ```

### Database Migrations

To create and apply database migrations:

```bash
cd backend
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Docker Configuration

- **Backend**: .NET 8 runtime with multi-stage build
- **PostgreSQL**: Version 15 with persistent volume
- **Networking**: Custom bridge network for service communication

## Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ASPNETCORE_URLS`: HTTP/HTTPS endpoints
- `POSTGRES_DB`: Database name
- `POSTGRES_USER`: Database user
- `POSTGRES_PASSWORD`: Database password

## Troubleshooting

1. **Port conflicts**: Ensure ports 5000, 5001, and 5432 are available
2. **Database connection**: Check if PostgreSQL container is running
3. **Build issues**: Ensure Docker Desktop is running and has sufficient resources

## License

This project is open source and available under the MIT License.
