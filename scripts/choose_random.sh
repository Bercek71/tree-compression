#!/bin/bash

# Folder containing all the text files
input_dir="$1"

# Output folder to store the random selection
output_dir="$2"

# Number of files to pick
num_files=100

# Check arguments
if [ -z "$input_dir" ] || [ -z "$output_dir" ]; then
  echo "Usage: $0 <input_directory> <output_directory>"
  exit 1
fi

# Create output directory if it doesn't exist
mkdir -p "$output_dir"

# Randomly pick files and copy them
find "$input_dir" -type f -name "*.txt" | shuf -n $num_files | while read -r file; do
  cp "$file" "$output_dir"
done

echo "✅ $num_files random .txt files copied to '$output_dir'"
