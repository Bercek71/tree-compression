#!/bin/bash

# Set project name (adjust if necessary)
PROJECT_NAME="TreeCompresion.sln"

# Define output directory
OUTPUT_DIR="publish"

# remove all bin and obj folders in lib and src
echo "Removing bin and obj folders..."
find ../../tree-compression/src -type d -name bin -exec rm -rf {} +
find ../../tree-compression/lib -type d -name bin -exec rm -rf {} +
find ../../tree-compression/src -type d -name obj -exec rm -rf {} +
find ../../tree-compression/lib -type d -name obj -exec rm -rf {} +

echo "Restoring dependencies..."
dotnet restore "../$PROJECT_NAME"

echo "Building the project..."
dotnet build "../$PROJECT_NAME" --configuration Release --no-restore

echo "Build process completed successfully!"