#!/bin/bash

set -e

# Delete the two collections when tests are done
fauna eval --url=$FAUNA_ENDPOINT --secret=$FAUNA_SECRET 'if (Collection.byName("Product").exists()) Product.definition.delete()'
fauna eval --url=$FAUNA_ENDPOINT --secret=$FAUNA_SECRET 'if (Collection.byName("Manufacturer").exists()) Manufacturer.definition.delete()'
