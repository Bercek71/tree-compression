#!/bin/bash

input_file="$1"

if [ -z "$input_file" ]; then
  echo "Usage: $0 path_to_csv_file"
  exit 1
fi

# Use awk to extract header indices
header=$(head -n 1 "$input_file")
IFS=',' read -ra cols <<< "$header"

title_idx=-1
text_idx=-1

for i in "${!cols[@]}"; do
  if [[ "${cols[$i]}" == "\"case_title\"" || "${cols[$i]}" == "case_title" ]]; then
    title_idx=$i
  elif [[ "${cols[$i]}" == "\"case_text\"" || "${cols[$i]}" == "case_text" ]]; then
    text_idx=$i
  fi
done

if [[ $title_idx -eq -1 || $text_idx -eq -1 ]]; then
  echo "Could not find 'case_title' or 'case_text' columns in header."
  exit 1
fi

# Python part: fixed line with backslash escape
python3 - <<EOF
import csv
import os

with open("$input_file", newline='', encoding='utf-8') as csvfile:
    reader = csv.DictReader(csvfile)
    for row in reader:
        title = row['case_title'].strip().replace('/', '_').replace('\\\\', '_')
        text = row['case_text']
        if not title:
            continue
        filename = f"{title}.txt"
        with open(filename, 'w', encoding='utf-8') as f:
            f.write(text)
EOF

echo "Files created based on 'case_title'."
