#!/bin/sh
set -e

VERSION=$(date +"%Y.%m.%d.%H%M%S")
dotnet restore
dotnet build --no-restore --configuration Release
mkdir -p ./dist
dotnet dotnet pack BeGenerate --output ./dist --no-build --no-restore --configuration Release -p:Version=$VERSION

echo "Published version $VERSION"