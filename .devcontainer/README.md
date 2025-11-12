# Dev Container Setup for CV App

This Dev Container configuration provides a consistent development environment for the CV App project while working alongside your existing Docker Compose setup.

## 🚀 **How It Works**

### **Architecture**
- **Dev Container**: Provides the development environment (VS Code, extensions, tools)
- **Docker Compose**: Manages application services (backend, PostgreSQL)
- **Integration**: Dev Container uses your existing `docker-compose.dev.yml` as a base

### **Benefits**
- ✅ **Consistent environment** across team members
- ✅ **Hot reload** with `dotnet watch`
- ✅ **Pre-configured extensions** for .NET development
- ✅ **Database access** from within the container
- ✅ **Port forwarding** for easy debugging

## 📋 **Prerequisites**

1. **VS Code** with the **Dev Containers** extension
2. **Docker Desktop** running
3. **Git** for version control

## 🛠️ **Getting Started**

### **1. Open in Dev Container**
1. Open the project in VS Code
2. Press `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac)
3. Select **"Dev Containers: Reopen in Container"**
4. Wait for the container to build and start

### **2. Alternative: Command Palette**
- **"Dev Containers: Open Folder in Container"**
- Navigate to your project folder
- Select the folder

## 🔧 **What Happens When You Open the Container**

1. **VS Code detects** the `.devcontainer` folder
2. **Builds** the development container
3. **Mounts** your project files
4. **Installs** pre-configured extensions
5. **Starts** the backend service with hot reload
6. **Connects** to PostgreSQL database

## 📁 **File Structure**

```
.devcontainer/
├── devcontainer.json          # Main Dev Container configuration
├── docker-compose.override.yml # Override for Dev Container
└── README.md                  # This file
```

## 🔌 **Port Forwarding**

- **8080**: Backend API (main application)
- **5000**: HTTP development server
- **5001**: HTTPS development server  
- **5432**: PostgreSQL database

## 🚀 **Development Workflow**

### **Inside the Dev Container**
1. **Edit code** - Changes are automatically synced
2. **Terminal** - Access to .NET CLI and other tools
3. **Debugging** - Full debugging support
4. **Extensions** - Pre-configured for .NET development

### **Hot Reload**
- Backend automatically restarts on file changes
- No need to manually rebuild containers
- Fast development iteration

## 🐳 **Docker Compose Integration**

### **How They Work Together**
- **Dev Container** provides the development environment
- **Docker Compose** manages the application services
- **Shared volumes** ensure code changes are reflected immediately

### **Commands Available**
```bash
# Inside the Dev Container
dotnet run                    # Run the application
dotnet watch run             # Run with hot reload
dotnet build                 # Build the project
dotnet test                  # Run tests
dotnet ef migrations add     # Add database migrations
dotnet ef database update    # Update database
```

## 🔍 **Troubleshooting**

### **Container Won't Start**
1. Ensure Docker Desktop is running
2. Check if ports 8080, 5000, 5001, 5432 are available
3. Try rebuilding: **"Dev Containers: Rebuild Container"**

### **Hot Reload Not Working**
1. Check if `dotnet watch` is running in the terminal
2. Verify file watching is enabled
3. Restart the container if needed

### **Database Connection Issues**
1. Ensure PostgreSQL container is healthy
2. Check connection string in `appsettings.Development.json`
3. Verify port 5432 is forwarded

## 🎯 **Best Practices**

1. **Always work inside the Dev Container** for consistency
2. **Use the integrated terminal** for .NET commands
3. **Leverage hot reload** for faster development
4. **Keep the container running** during development sessions
5. **Commit the `.devcontainer` folder** to version control

## 🔄 **Updating the Setup**

### **Add New Extensions**
Edit `.devcontainer/devcontainer.json` and add to the `extensions` array

### **Change .NET Version**
Update the `version` in the `dotnet` feature

### **Add New Services**
Modify `docker-compose.override.yml` to add new services

## 📚 **Useful Commands**

```bash
# Rebuild the Dev Container
Ctrl+Shift+P → "Dev Containers: Rebuild Container"

# Open new terminal in container
Ctrl+Shift+` (backtick)

# Access container shell
Ctrl+Shift+P → "Dev Containers: Execute in Container"

# View container logs
Ctrl+Shift+P → "Dev Containers: View Container Log"
```

## 🎉 **You're Ready!**

Once the Dev Container is running, you'll have:
- **Full .NET 8 development environment**
- **Hot reload** for fast iteration
- **Database access** for development
- **Consistent tools** across your team
- **Professional development experience**

Happy coding! 🚀



