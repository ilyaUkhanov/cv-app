FROM node:18-alpine

WORKDIR /app

# Copy package files
COPY package*.json ./
COPY yarn.lock ./

# Create a startup script that installs dependencies and starts the app
RUN echo '#!/bin/sh' > /start.sh && \
    echo 'if [ ! -d "node_modules" ] || [ -z "$(ls -A node_modules)" ]; then' >> /start.sh && \
    echo '  echo "Installing dependencies..."' >> /start.sh && \
    echo '  npm install --include=dev' >> /start.sh && \
    echo 'fi' >> /start.sh && \
    echo 'echo "Building TypeScript..."' >> /start.sh && \
    echo 'npm run build' >> /start.sh && \
    echo 'echo "Starting application..."' >> /start.sh && \
    echo 'npm start' >> /start.sh && \
    chmod +x /start.sh

# Expose port
EXPOSE 3000

# Run the startup script
CMD ["/start.sh"]