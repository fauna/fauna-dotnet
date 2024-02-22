#!/bin/sh

set -eou

cd ./repo.git

apk add xmlstarlet
PACKAGE_VERSION=$(xml sel -t -v "/Project/PropertyGroup/Version" ./Fauna/Fauna.csproj)

dotnet clean
dotnet restore
dotnet build ./Fauna --configuration Release
dotnet pack ./Fauna/Fauna.csproj --no-build --no-restore --include-symbols -p:SymbolPackageFormat=snupkg --include-source --configuration Release

dotnet nuget push ./Fauna/bin/Release/*.nupkg -s https://api.nuget.org/v3/index.json -k $NUGET_API_KEY --skip-duplicate

# echo "fauna-dotnet@$PACKAGE_VERSION has been published to nuget @driver-release-watchers" > ../slack-message/publish
