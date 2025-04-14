#!/bin/bash

# Make a folder to store the files
mkdir -p kernel_docs && cd kernel_docs

# Get all .txt or README-style file links (case-insensitive), clean them up
curl -s https://www.kernel.org/doc/readme/ | \
grep -Eio 'href="[^"]+(README[^"]*|[^"]+\.txt)"' | \
sed -E 's/href="//;s/"$//' | \
while read -r file; do
    wget "https://www.kernel.org/doc/readme/$file"
done

