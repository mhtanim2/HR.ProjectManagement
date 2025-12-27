#!/bin/bash
# Database migration script for Linux/Mac

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting database migrations...${NC}"

# Check if database is ready
until dotnet ef database update --project HR.ProjectManagement --startup-project HR.ProjectManagement; do
  echo -e "${RED}Database is not ready yet. Waiting...${NC}"
  sleep 3
done

echo -e "${GREEN}Migrations completed successfully!${NC}"
