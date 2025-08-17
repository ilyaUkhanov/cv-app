# CV App Makefile
# A comprehensive Makefile for the .NET MVC application with PostgreSQL

# Variables
PROJECT_NAME = cv-app
BACKEND_DIR = backend
DOCKER_COMPOSE_DEV = docker-compose.dev.yml
DOCKER_COMPOSE_PROD = docker-compose.prod.yml
DOTNET_VERSION = 8.0

# Colors for output
RED = \033[0;31m
GREEN = \033[0;32m
YELLOW = \033[1;33m
BLUE = \033[0;34m
NC = \033[0m # No Color

# Default target
.DEFAULT_GOAL := help

# Help target
.PHONY: help
help: ## Show this help message
	@echo "$(BLUE)CV App - Available Commands:$(NC)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(GREEN)%-20s$(NC) %s\n", $$1, $$2}'
	@echo ""

# Development targets
.PHONY: dev
dev: ## Start development environment with Docker Compose
	@echo "$(BLUE)Starting development environment...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_DEV) up -d
	@echo "$(GREEN)Development environment started!$(NC)"
	@echo "$(YELLOW)Web App: http://localhost:8080$(NC)"
	@echo "$(YELLOW)Database: localhost:5432$(NC)"

.PHONY: dev-logs
dev-logs: ## Show development environment logs
	docker-compose -f $(DOCKER_COMPOSE_DEV) logs -f

.PHONY: dev-stop
dev-stop: ## Stop development environment
	@echo "$(BLUE)Stopping development environment...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_DEV) down
	@echo "$(GREEN)Development environment stopped!$(NC)"

# Production targets
.PHONY: prod
prod: ## Start production environment with Docker Compose
	@echo "$(BLUE)Starting production environment...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_PROD) up --build -d
	@echo "$(GREEN)Production environment started!$(NC)"
	@echo "$(YELLOW)Web App: http://localhost:8080$(NC)"

.PHONY: prod-stop
prod-stop: ## Stop production environment
	@echo "$(BLUE)Stopping production environment...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_PROD) down
	@echo "$(GREEN)Production environment stopped!$(NC)"

# Local development (without Docker)
.PHONY: local-dev
local-dev: ## Run application locally (requires .NET 8 SDK and PostgreSQL)
	@echo "$(BLUE)Starting local development...$(NC)"
	cd $(BACKEND_DIR) && dotnet run

.PHONY: local-build
local-build: ## Build the application locally
	@echo "$(BLUE)Building application locally...$(NC)"
	cd $(BACKEND_DIR) && dotnet build

.PHONY: local-clean
local-clean: ## Clean local build artifacts
	@echo "$(BLUE)Cleaning build artifacts...$(NC)"
	cd $(BACKEND_DIR) && dotnet clean

.PHONY: local-restore
local-restore: ## Restore NuGet packages
	@echo "$(BLUE)Restoring NuGet packages...$(NC)"
	cd $(BACKEND_DIR) && dotnet restore

# Database operations
.PHONY: db-migrate
db-migrate: ## Create and apply database migrations
	@echo "$(BLUE)Creating database migration...$(NC)"
	cd $(BACKEND_DIR) && dotnet ef migrations add InitialCreate
	@echo "$(BLUE)Applying migrations...$(NC)"
	cd $(BACKEND_DIR) && dotnet ef database update

.PHONY: db-update
db-update: ## Update database with existing migrations
	@echo "$(BLUE)Updating database...$(NC)"
	cd $(BACKEND_DIR) && dotnet ef database update

.PHONY: db-remove
db-remove: ## Remove last migration
	@echo "$(BLUE)Removing last migration...$(NC)"
	cd $(BACKEND_DIR) && dotnet ef migrations remove

# Docker operations
.PHONY: docker-build
docker-build: ## Build Docker images
	@echo "$(BLUE)Building Docker images...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_DEV) build

.PHONY: docker-clean
docker-clean: ## Clean Docker images and containers
	@echo "$(BLUE)Cleaning Docker resources...$(NC)"
	docker system prune -f
	docker volume prune -f

.PHONY: docker-logs
docker-logs: ## Show all Docker logs
	docker-compose -f $(DOCKER_COMPOSE_DEV) logs

# Testing
.PHONY: test
test: ## Run tests
	@echo "$(BLUE)Running tests...$(NC)"
	cd $(BACKEND_DIR) && dotnet test

.PHONY: test-watch
test-watch: ## Run tests in watch mode
	@echo "$(BLUE)Running tests in watch mode...$(NC)"
	cd $(BACKEND_DIR) && dotnet test --watch

# Utility targets
.PHONY: status
status: ## Show status of running containers
	@echo "$(BLUE)Container Status:$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_DEV) ps

.PHONY: shell
shell: ## Open shell in backend container
	@echo "$(BLUE)Opening shell in backend container...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_DEV) exec backend /bin/bash

.PHONY: db-shell
db-shell: ## Open shell in PostgreSQL container
	@echo "$(BLUE)Opening shell in PostgreSQL container...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_DEV) exec postgres psql -U postgres -d cvapp

.PHONY: logs-backend
logs-backend: ## Show backend logs
	docker-compose -f $(DOCKER_COMPOSE_DEV) logs -f backend

.PHONY: logs-db
logs-db: ## Show database logs
	docker-compose -f $(DOCKER_COMPOSE_DEV) logs -f postgres

# Cleanup targets
.PHONY: clean
clean: local-clean ## Clean all build artifacts and Docker resources
	@echo "$(BLUE)Cleaning everything...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_DEV) down -v
	docker-compose -f $(DOCKER_COMPOSE_PROD) down -v
	docker-clean

.PHONY: reset
reset: ## Reset everything (WARNING: This will delete all data)
	@echo "$(RED)WARNING: This will delete all data and containers!$(NC)"
	@read -p "Are you sure? [y/N] " -n 1 -r; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		echo ""; \
		echo "$(BLUE)Resetting everything...$(NC)"; \
		docker-compose -f $(DOCKER_COMPOSE_DEV) down -v; \
		docker-compose -f $(DOCKER_COMPOSE_PROD) down -v; \
		docker system prune -af; \
		docker volume prune -f; \
		echo "$(GREEN)Reset complete!$(NC)"; \
	else \
		echo ""; \
		echo "$(YELLOW)Reset cancelled.$(NC)"; \
	fi

# Development workflow shortcuts
.PHONY: restart
restart: dev-stop dev ## Restart development environment

.PHONY: rebuild
rebuild: dev-stop docker-build dev ## Rebuild and restart development environment

.PHONY: fresh-start
fresh-start: clean dev ## Clean everything and start fresh

# Show current environment info
.PHONY: info
info: ## Show current environment information
	@echo "$(BLUE)CV App Environment Info:$(NC)"
	@echo "$(YELLOW)Project:$(NC) $(PROJECT_NAME)"
	@echo "$(YELLOW).NET Version:$(NC) $(DOTNET_VERSION)"
	@echo "$(YELLOW)Backend Directory:$(NC) $(BACKEND_DIR)"
	@echo "$(YELLOW)Docker Compose Dev:$(NC) $(DOCKER_COMPOSE_DEV)"
	@echo "$(YELLOW)Docker Compose Prod:$(NC) $(DOCKER_COMPOSE_PROD)"
	@echo ""
	@echo "$(BLUE)Running Containers:$(NC)"
	@docker-compose -f $(DOCKER_COMPOSE_DEV) ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"
