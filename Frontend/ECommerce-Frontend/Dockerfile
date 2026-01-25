# ECommerce Frontend (Angular SSR) Dockerfile
# Multi-stage build for optimized image size

# Stage 1: Build
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --legacy-peer-deps

# Copy source code
COPY . .

# Build for production (SSR)
RUN npm run build

# Stage 2: Runtime
FROM node:22-alpine AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup -g 1001 -S nodejs && \
    adduser -S angular -u 1001

# Copy built application
COPY --from=build --chown=angular:nodejs /app/dist ./dist
COPY --from=build --chown=angular:nodejs /app/package*.json ./

# Install only production dependencies
RUN npm ci --only=production --legacy-peer-deps && \
    npm cache clean --force

# Switch to non-root user
USER angular

# Environment variables
ENV NODE_ENV=production

# Health check (Render için önerilir)
HEALTHCHECK CMD wget --no-verbose --tries=1 --spider http://localhost:4000 || exit 1

# SSR başlatma komutu (Render için)
ENV PORT=4000
ENV API_URL=http://localhost:5000/api
CMD ["node", "dist/ECommerce-Frontend/server/server.mjs"]

EXPOSE 4000

# Start SSR server
CMD ["node", "dist/ECommerce-Frontend/server/server.mjs"]
