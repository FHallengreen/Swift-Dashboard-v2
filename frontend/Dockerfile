# Stage 1: Build the Vite application
FROM node:24-alpine AS build
WORKDIR /app

# Copy package.json and package-lock.json (if exists)
COPY package.json package-lock.json* ./

# Install dependencies
RUN npm install

# Ensure node_modules/.bin scripts are executable
RUN chmod -R +x node_modules/.bin

# Copy the rest of the application code (excluding node_modules)
COPY . .

# Verify tsc is available and executable
RUN ls -la node_modules/.bin/tsc && ./node_modules/.bin/tsc --version

# Run the build
RUN npm run build

# Stage 2: Serve with Nginx
FROM nginx:1.28-alpine
WORKDIR /usr/share/nginx/html

# Remove default Nginx files
RUN rm -rf ./*

# Copy built assets from the build stage
COPY --from=build /app/dist .

# Copy Nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]