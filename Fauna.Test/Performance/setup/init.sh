#!/bin/bash

set -e

# Needed for Fauna CLI to run, even with --url and --secret overrides
touch .fauna-project

# Push the schema from ./fauna/main.fsl to the db
fauna schema push --force --url=$FAUNA_ENDPOINT --secret=$FAUNA_SECRET --dir=./fauna

# Initialize data in the collections
fauna eval --url=$FAUNA_ENDPOINT --secret=$FAUNA_SECRET --file=./fauna/seeddata.fql
