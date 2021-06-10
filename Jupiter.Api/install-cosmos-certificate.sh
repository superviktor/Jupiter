#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")/.."

CERT_FILE="$(pwd)/local-cosmos-certificate.pem"
rm -rf "$CERT_FILE"

function downloadCert(){
    set +e
    curl -sk https://localhost:8081/_explorer/emulator.pem > "$CERT_FILE"
    set -e
}

echo -n "Downloading cosmos certificate from https://localhost:8081/_explorer/emulator.pem ..."
while true; do
    downloadCert
    CERT=$(cat "$CERT_FILE")
    if [[ $CERT == *"BEGIN CERTIFICATE"* ]]; then
        break
    fi
    sleep 1
    echo -n "."
done
echo ""

UNAME="$(uname -s)"
case "${UNAME}" in
    Darwin*)    echo "Installing certificate $CERT_FILE in keychain (you may be asked for your sudo password)" && \
                sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain "$CERT_FILE" && \
                echo "Cert installation successful!";;
    Linux*)     echo "Installing certificate $CERT_FILE (this requires ca-certificates to be installed)" && \
                sudo mkdir /usr/local/share/ca-certificates/jupiter/ -p && \
                sudo openssl x509 -inform pem -in "$CERT_FILE" -out /usr/local/share/ca-certificates/jupiter/cosmos-db-test.crt && \
                sudo update-ca-certificates && \
                echo "Cert installation successful!" && \
                echo "If you are on windows with WSL right click the file install-cosmos-certificate.ps1 and run in powershell.";;
    *)          echo "Don't know how do install $CERT_FILE on this system ($UNAME), sorry!" && \
                exit 1;;
esac
