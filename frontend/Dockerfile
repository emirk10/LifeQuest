# Frontend Dockerfile (Development Mode with Hot Reload)
FROM node:20-alpine

WORKDIR /app

# Copy package.json and package-lock.json
COPY package*.json ./

# Install dependencies
RUN npm install --legacy-peer-deps

# Copy all files
COPY . .

# Expose Vite's default port
EXPOSE 5173

# Run Vite in development mode, binding to all interfaces so Docker can expose it
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]
