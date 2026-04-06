#!/bin/bash

# Simple secret scanner for Agency CRM
# This script looks for common patterns of hardcoded secrets.

echo "--- Starting Security Hardening Scan ---"

# 1. Check for .env files that should be ignored
echo "Checking for .env files..."
find . -name "*.env" -not -path "*/node_modules/*" -not -path "*/.vercel/*"

# 2. Scan for hardcoded API keys and secrets
echo "Scanning for hardcoded secrets (API_KEY, SECRET, PASSWORD)..."
grep -rE "API_KEY|SECRET_KEY|PASSWORD|PRIVATE_KEY" . \
    --exclude-dir={.git,node_modules,bin,obj,dist,.next,out} \
    --exclude={*.md,*.test.ts,*.test.tsx,*.css,*.scss,package-lock.json} | grep -v "Placeholder" | grep -v "mock"

# 3. Check for specific dangerous patterns
echo "Checking for dangerous patterns..."
grep -r "TODO" . --exclude-dir={node_modules,bin,obj,dist,.next,out}

echo "--- Security Scan Complete ---"
