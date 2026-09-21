#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

pushd "${SCRIPT_DIR}/../../"

docker buildx bake \
    --file ./DevOps/Container/docker-bake.hcl \
    --provenance=false

popd