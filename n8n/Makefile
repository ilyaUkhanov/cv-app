.PHONY: help up down start stop restart logs logs-n8n logs-traefik status clean

help:
	@echo "N8N Docker Management"
	@echo "====================="
	@echo ""
	@echo "Available commands:"
	@echo "  make up          - Start all services (create and start containers)"
	@echo "  make down        - Stop and remove all containers"
	@echo "  make start       - Start existing containers"
	@echo "  make stop        - Stop running containers"
	@echo "  make restart     - Restart all services"
	@echo "  make logs        - Show logs from all services (follow mode)"
	@echo "  make logs-n8n    - Show logs from n8n service only (follow mode)"
	@echo "  make logs-traefik - Show logs from traefik service only (follow mode)"
	@echo "  make status      - Show status of all containers"
	@echo "  make clean       - Stop containers and remove volumes (WARNING: deletes data)"
	@echo ""

up:
	@echo "Starting n8n services..."
	docker-compose up -d
	@echo "Services started. Use 'make logs' to view logs."

down:
	@echo "Stopping n8n services..."
	docker-compose down
	@echo "Services stopped."

start:
	@echo "Starting n8n containers..."
	docker-compose start
	@echo "Containers started."

stop:
	@echo "Stopping n8n containers..."
	docker-compose stop
	@echo "Containers stopped."

restart:
	@echo "Restarting n8n services..."
	docker-compose restart
	@echo "Services restarted."

logs:
	@echo "Showing logs for all services (Ctrl+C to exit)..."
	docker-compose logs -f

logs-n8n:
	@echo "Showing logs for n8n service (Ctrl+C to exit)..."
	docker-compose logs -f n8n

logs-traefik:
	@echo "Showing logs for traefik service (Ctrl+C to exit)..."
	docker-compose logs -f traefik

status:
	@echo "Container status:"
	docker-compose ps

clean:
	@echo "WARNING: This will delete all data in volumes!"
	@read -p "Are you sure? [y/N] " -n 1 -r; \
	echo; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		docker-compose down -v; \
		echo "Containers and volumes removed."; \
	else \
		echo "Operation cancelled."; \
	fi