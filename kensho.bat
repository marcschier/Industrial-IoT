
set ENDPOINT_URI=https://iiotcogad1.cognitiveservices.azure.com/
set API_KEY=0b0eb7987aa144a1b2f6d9b8078bc905

docker run --rm -it -p 5000:5000 --memory 4g --cpus 1 mcr.microsoft.com/azure-cognitive-services/decision/anomaly-detector:latest Eula=accept Billing=%ENDPOINT_URI% ApiKey=%API_KEY%