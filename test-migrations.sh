#!/bin/bash

# Test database migrations

echo "CharityPay .NET - Database Migration Test"
echo "========================================="
echo ""

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Check if database is accessible
echo -n "1. Checking database connection... "
if docker exec charitypay-postgres psql -U charitypay_user -d charitypay -c '\q' 2>/dev/null; then
    echo -e "${GREEN}✓ Connected${NC}"
else
    echo -e "${RED}✗ Cannot connect to database${NC}"
    exit 1
fi

# Check current tables
echo ""
echo "2. Current database tables:"
echo "----------------------------"
tables=$(docker exec charitypay-postgres psql -U charitypay_user -d charitypay -t -c "SELECT tablename FROM pg_tables WHERE schemaname = 'public';" 2>/dev/null | grep -v '^$' | sed 's/^[ \t]*//')

if [ -z "$tables" ]; then
    echo -e "${YELLOW}No tables found. Migrations need to be applied.${NC}"
    need_migration=true
else
    echo "Found tables:"
    echo "$tables" | while read -r table; do
        echo "  - $table"
    done
    need_migration=false
fi

# Check for EF Core migrations table
echo ""
echo -n "3. Checking for EF Core migrations history... "
if echo "$tables" | grep -q "__EFMigrationsHistory"; then
    echo -e "${GREEN}✓ Found${NC}"
    
    # List applied migrations
    echo ""
    echo "Applied migrations:"
    docker exec charitypay-postgres psql -U charitypay_user -d charitypay -t -c "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";" 2>/dev/null | grep -v '^$' | sed 's/^[ \t]*/  - /'
else
    echo -e "${YELLOW}Not found${NC}"
fi

# Run migrations if needed
if [ "$need_migration" = true ]; then
    echo ""
    echo "4. Attempting to run migrations..."
    echo "-----------------------------------"
    
    # Option 1: Try to run migrations in the container
    echo -n "Checking if EF Core tools are available in container... "
    if docker exec charitypay-api dotnet ef --version 2>/dev/null; then
        echo -e "${GREEN}✓ Available${NC}"
        
        echo "Running migrations..."
        if docker exec charitypay-api dotnet ef database update --project src/CharityPay.Infrastructure --startup-project src/CharityPay.API 2>/dev/null; then
            echo -e "${GREEN}✓ Migrations applied successfully${NC}"
        else
            echo -e "${RED}✗ Migration failed${NC}"
        fi
    else
        echo -e "${YELLOW}Not available${NC}"
        echo ""
        echo "To apply migrations, you can:"
        echo "1. Install .NET SDK locally and run:"
        echo "   dotnet ef database update -p src/CharityPay.Infrastructure -s src/CharityPay.API"
        echo ""
        echo "2. Or run the migration script in the container:"
        echo "   docker exec charitypay-api bash /app/migrate.sh"
    fi
fi

# Verify expected tables
echo ""
echo "5. Verifying expected tables..."
echo "--------------------------------"

expected_tables=(
    "Users"
    "Organizations"
    "Payments"
    "__EFMigrationsHistory"
)

for table in "${expected_tables[@]}"; do
    echo -n "Checking for table '$table'... "
    if docker exec charitypay-postgres psql -U charitypay_user -d charitypay -t -c "SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = '$table');" 2>/dev/null | grep -q 't'; then
        echo -e "${GREEN}✓ Exists${NC}"
    else
        echo -e "${RED}✗ Missing${NC}"
    fi
done

# Check table schemas
echo ""
echo "6. Table Schema Information"
echo "---------------------------"

if docker exec charitypay-postgres psql -U charitypay_user -d charitypay -t -c "SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Users');" 2>/dev/null | grep -q 't'; then
    echo "Users table columns:"
    docker exec charitypay-postgres psql -U charitypay_user -d charitypay -c "\d \"Users\"" 2>/dev/null | grep -E "^ " | awk '{print "  - " $1 ": " $3}'
fi

echo ""
echo "Migration test completed!"