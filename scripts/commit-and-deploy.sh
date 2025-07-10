#!/bin/bash

# commit-and-deploy.sh - Automated commit and deployment workflow
# Usage: ./scripts/commit-and-deploy.sh "commit message" [task-id]

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
MAX_RETRY_ATTEMPTS=10

# Arguments
COMMIT_MESSAGE="$1"
TASK_ID="$2"

if [ -z "$COMMIT_MESSAGE" ]; then
    echo -e "${RED}❌ Commit message is required${NC}"
    echo -e "Usage: ./scripts/commit-and-deploy.sh \"commit message\" [task-id]"
    exit 1
fi

echo -e "${BLUE}🔄 CharityPay .NET Automated Workflow${NC}"
echo -e "${BLUE}=====================================${NC}"
echo -e "Commit Message: ${YELLOW}$COMMIT_MESSAGE${NC}"
echo -e "Task ID: ${YELLOW}${TASK_ID:-"N/A"}${NC}"
echo -e "Max Retry Attempts: ${YELLOW}$MAX_RETRY_ATTEMPTS${NC}"
echo ""

cd "$PROJECT_ROOT"

# Function to run tests with retry logic
run_tests_with_retry() {
    local attempt=1
    local max_attempts=$MAX_RETRY_ATTEMPTS
    
    while [ $attempt -le $max_attempts ]; do
        echo -e "${YELLOW}🧪 Running tests (attempt $attempt/$max_attempts)...${NC}"
        
        if ./scripts/test-all.sh --quick; then
            echo -e "${GREEN}✅ Tests passed on attempt $attempt${NC}"
            return 0
        else
            echo -e "${RED}❌ Tests failed on attempt $attempt${NC}"
            
            if [ $attempt -eq $max_attempts ]; then
                echo -e "${RED}💥 Maximum retry attempts reached. Tests still failing.${NC}"
                echo -e "${RED}Please review and fix the issues manually.${NC}"
                return 1
            fi
            
            echo -e "${YELLOW}⏳ Waiting 5 seconds before retry...${NC}"
            sleep 5
            ((attempt++))
        fi
    done
}

# Function to check if there are changes to commit
check_for_changes() {
    if git diff --quiet && git diff --cached --quiet; then
        echo -e "${YELLOW}⚠️  No changes detected to commit${NC}"
        return 1
    fi
    return 0
}

# Function to display git status
show_git_status() {
    echo -e "${BLUE}📋 Current Git Status:${NC}"
    git status --short
    echo ""
}

# Function to commit changes
commit_changes() {
    echo -e "${YELLOW}📝 Staging changes...${NC}"
    git add .
    
    echo -e "${YELLOW}💾 Committing changes...${NC}"
    
    # Enhanced commit message with task ID and metadata
    local full_commit_message="$COMMIT_MESSAGE"
    
    if [ -n "$TASK_ID" ]; then
        full_commit_message="$full_commit_message ($TASK_ID)"
    fi
    
    # Add automated signature
    full_commit_message="$full_commit_message

🤖 Generated with [Claude Code](https://claude.ai/code)

Co-Authored-By: Claude <noreply@anthropic.com>"
    
    git commit -m "$full_commit_message"
    
    local commit_hash=$(git rev-parse --short HEAD)
    echo -e "${GREEN}✅ Committed successfully: $commit_hash${NC}"
    echo -e "${GREEN}📝 Message: $COMMIT_MESSAGE${NC}"
}

# Function to deploy
deploy_changes() {
    echo -e "${YELLOW}🚀 Starting deployment...${NC}"
    
    if ./scripts/deploy.sh --env=dev; then
        echo -e "${GREEN}✅ Deployment successful${NC}"
        return 0
    else
        echo -e "${RED}❌ Deployment failed${NC}"
        return 1
    fi
}

# Main workflow
echo -e "${YELLOW}🔍 Checking for changes...${NC}"
show_git_status

if ! check_for_changes; then
    echo -e "${BLUE}ℹ️  No changes to process. Workflow completed.${NC}"
    exit 0
fi

# Step 1: Run tests with retry logic
echo -e "${BLUE}Step 1: Testing${NC}"
if ! run_tests_with_retry; then
    echo -e "${RED}❌ Workflow failed at testing step${NC}"
    exit 1
fi

# Step 2: Commit changes
echo -e "${BLUE}Step 2: Committing${NC}"
if ! commit_changes; then
    echo -e "${RED}❌ Workflow failed at commit step${NC}"
    exit 1
fi

# Step 3: Deploy
echo -e "${BLUE}Step 3: Deploying${NC}"
if ! deploy_changes; then
    echo -e "${RED}❌ Workflow failed at deployment step${NC}"
    echo -e "${YELLOW}ℹ️  Changes were committed but deployment failed${NC}"
    exit 1
fi

# Success summary
echo ""
echo -e "${GREEN}🎉 Workflow completed successfully!${NC}"
echo -e "${GREEN}================================${NC}"
echo -e "${GREEN}✅ Tests passed${NC}"
echo -e "${GREEN}✅ Changes committed${NC}"
echo -e "${GREEN}✅ Deployment successful${NC}"
echo ""

# Display deployment info
echo -e "${BLUE}📋 Deployment Information:${NC}"
echo -e "  Commit: ${YELLOW}$(git rev-parse --short HEAD)${NC}"
echo -e "  Message: ${YELLOW}$COMMIT_MESSAGE${NC}"
if [ -n "$TASK_ID" ]; then
    echo -e "  Task ID: ${YELLOW}$TASK_ID${NC}"
fi
echo -e "  API URL: ${YELLOW}http://localhost:5000${NC}"
echo -e "  Frontend URL: ${YELLOW}http://localhost:5173${NC}"
echo ""

echo -e "${GREEN}✨ Ready for next development cycle!${NC}"