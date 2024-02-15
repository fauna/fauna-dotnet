#!/bin/sh
set -eou
cd ./repo.git

apk add xmlstarlet
PACKAGE_VERSION=$(xml sel -t -v "/Project/PropertyGroup/Version" ./Fauna/Fauna.csproj)

echo "Current docs version: $PACKAGE_VERSION"
cd ../
git clone docs.git docs-updated.git

cd docs-updated.git

mkdir "${PACKAGE_VERSION}"
cd "${PACKAGE_VERSION}"

sed -i.bak "s/_DOC_VERSION_/${PACKAGE_VERSION}/" ../../repo.git/doc/Doxyfile

apk add doxygen

doxygen "../../repo.git/doc/Doxyfile"
rm -r man
rm -r latex
cp -a ./html/. ./
rm -r html

echo "Documentation created"

apk add --no-progress --no-cache sed

echo "================================="
echo "Adding google manager tag to head"
echo "================================="

HEAD_GTM=$(cat ../../repo.git/concourse/scripts/head_gtm.dat)
sed -i.bak "0,/<\/title>/{s/<\/title>/<\/title>${HEAD_GTM}/}" ./index.html

echo "================================="
echo "Adding google manager tag to body"
echo "================================="

BODY_GTM=$(cat ../../repo.git/concourse/scripts/body_gtm.dat)
sed -i.bak "0,/<body>/{s/<body>/<body>${BODY_GTM}/}" ./index.html

rm ./index.html.bak

git config --global user.email "nobody@fauna.com"
git config --global user.name "Fauna, Inc"

git add -A
git commit -m "Update docs to version: $PACKAGE_VERSION"

echo "Documentation committed"