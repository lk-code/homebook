#!/bin/bash
set -e

# Configuration variables
SOLUTION="./homebook.slnx"
LICENSES_JSON="./source/HomeBook.Backend.Core.Licenses/Licenses.json"
LICENSES_DIR="./source/HomeBook.Backend.Core.Licenses/Licenses"

dotnet tool install --global nuget-license
dotnet restore ${SOLUTION}
dotnet build ${SOLUTION} --configuration Release --no-restore

rm "${LICENSES_JSON}"
rm -rf "${LICENSES_DIR}"
mkdir -p "${LICENSES_DIR}"

nuget-license -i ${SOLUTION} --include-transitive --output JsonPretty --file-output ${LICENSES_JSON}
#nuget-license -i ./homebook.slnx --include-transitive --output JsonPretty --file-output /source/HomeBook.Backend.Core.Licenses/Licenses.json
nuget-license -i ${SOLUTION} --include-transitive -d ${LICENSES_DIR}
#nuget-license -i ./homebook.slnx --include-transitive -d ./source/HomeBook.Backend.Core.Licenses/Licenses

echo "License information updated successfully."

# commands to execute => copy ready
# rm "./source/HomeBook.Backend.Core.Licenses/Licenses.json"
# rm -rf "./source/HomeBook.Backend.Core.Licenses/Licenses"
# mkdir -p "./source/HomeBook.Backend.Core.Licenses/Licenses"
# nuget-license -i "./homebook.slnx" --include-transitive --output JsonPretty --file-output "./source/HomeBook.Backend.Core.Licenses/Licenses.json"
# nuget-license -i "./homebook.slnx" --include-transitive -d "./source/HomeBook.Backend.Core.Licenses/Licenses"
