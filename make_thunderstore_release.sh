#!/bin/bash

cp ./bin/Release/netstandard2.1/Minimap.dll ./thunderstore/Minimap.dll
cp ./README.md ./thunderstore/README.md
# make zip file from gzip
cd thunderstore || exit 1
zip -r ../Release-thunderstore.zip ./*
cd .. || exit 1