#!/bin/bash

# Check if a directory is provided
if [ -z "$1" ]; then
    echo "Usage: $0 <directory>"
    exit 1
fi

# Folder to process
folder="$1"

# Recursively find all files in the directory
find "$folder" -type f | while read -r file; do
    # Skip binary files (optional)
    if file "$file" | grep -q "text"; then
        # Remove paired brackets and the pipe symbol | from text
        sed -i "" 's/\[[^]]*\]//g; s/[|]//g' "$file"
        echo "Modified: $file"
    else
        echo "Skipped binary file: $file"
    fi
done
