#!/bin/bash

cat data.txt | sed -e "s/\([.\!?,'/()]\)/ \1 /g" | tr "[:upper:]" "[:lower:]" > data.preprocessed.txt
