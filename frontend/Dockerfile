FROM node:20-alpine

WORKDIR /app

# Set environment variables
ENV VITE_API_URL=http://charitypay-api:8081/api/v1

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy source code
COPY . .

# Expose port
EXPOSE 3000

# Start development server
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "3000"]
