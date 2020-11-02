FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["azure/Microsoft.Azure.IIoT.Azure.ActiveDirectory/src/Microsoft.Azure.IIoT.Azure.ActiveDirectory.csproj", "azure/Microsoft.Azure.IIoT.Azure.ActiveDirectory/src/"]
COPY ["azure/Microsoft.Azure.IIoT.Azure.AspNetCore/src/Microsoft.Azure.IIoT.Azure.AspNetCore.csproj", "azure/Microsoft.Azure.IIoT.Azure.AspNetCore/src/"]
COPY ["azure/Microsoft.Azure.IIoT.Azure.KeyVault/src/Microsoft.Azure.IIoT.Azure.KeyVault.csproj", "azure/Microsoft.Azure.IIoT.Azure.KeyVault/src/"]
COPY ["azure/Microsoft.Azure.IIoT.Azure.Monitor/src/Microsoft.Azure.IIoT.Azure.Monitor.csproj", "azure/Microsoft.Azure.IIoT.Azure.Monitor/src/"]
COPY ["extensions/Microsoft.Azure.IIoT.Extensions.Abstractions/src/Microsoft.Azure.IIoT.Extensions.Abstractions.csproj", "extensions/Microsoft.Azure.IIoT.Extensions.Abstractions/src/"]
COPY ["extensions/Microsoft.Azure.IIoT.Extensions.AspNetCore/src/Microsoft.Azure.IIoT.Extensions.AspNetCore.csproj", "extensions/Microsoft.Azure.IIoT.Extensions.AspNetCore/src/"]
COPY ["extensions/Microsoft.Azure.IIoT.Extensions.Bouncycastle/src/Microsoft.Azure.IIoT.Extensions.Bouncycastle.csproj", "extensions/Microsoft.Azure.IIoT.Extensions.Bouncycastle/src/"]
COPY ["extensions/Microsoft.Azure.IIoT.Extensions.LiteDb/src/Microsoft.Azure.IIoT.Extensions.LiteDb.csproj", "extensions/Microsoft.Azure.IIoT.Extensions.LiteDb/src/"]
COPY ["extensions/Microsoft.Azure.IIoT.Extensions.MessagePack/src/Microsoft.Azure.IIoT.Extensions.MessagePack.csproj", "extensions/Microsoft.Azure.IIoT.Extensions.MessagePack/src/"]
COPY ["extensions/Microsoft.Azure.IIoT.Extensions.NewtonSoft/src/Microsoft.Azure.IIoT.Extensions.NewtonSoft.csproj", "extensions/Microsoft.Azure.IIoT.Extensions.NewtonSoft/src/"]
COPY ["extensions/Microsoft.Azure.IIoT.Extensions.OpenId/src/Microsoft.Azure.IIoT.Extensions.OpenId.csproj", "extensions/Microsoft.Azure.IIoT.Extensions.OpenId/src/"]
COPY ["extensions/Microsoft.Azure.IIoT.Extensions.Orleans/src/Microsoft.Azure.IIoT.Extensions.Orleans.csproj", "extensions/Microsoft.Azure.IIoT.Extensions.Orleans/src/"]
COPY ["extensions/Microsoft.Azure.IIoT.Extensions/src/Microsoft.Azure.IIoT.Extensions.csproj", "extensions/Microsoft.Azure.IIoT.Extensions/src/"]
COPY ["platform/common/Microsoft.Azure.IIoT.Platform.Abstractions/src/Microsoft.Azure.IIoT.Platform.Abstractions.csproj", "platform/common/Microsoft.Azure.IIoT.Platform.Abstractions/src/"]
COPY ["platform/common/Microsoft.Azure.IIoT.Platform.Api.Internal/src/Microsoft.Azure.IIoT.Platform.Api.Internal.csproj", "platform/common/Microsoft.Azure.IIoT.Platform.Api.Internal/src/"]
COPY ["platform/common/Microsoft.Azure.IIoT.Platform.Api/src/Microsoft.Azure.IIoT.Platform.Api.csproj", "platform/common/Microsoft.Azure.IIoT.Platform.Api/src/"]
COPY ["platform/common/Microsoft.Azure.IIoT.Platform.OpcUa/src/Microsoft.Azure.IIoT.Platform.OpcUa.csproj", "platform/common/Microsoft.Azure.IIoT.Platform.OpcUa/src/"]
COPY ["platform/discovery/Microsoft.Azure.IIoT.Platform.Discovery.Api/src/Microsoft.Azure.IIoT.Platform.Discovery.Api.csproj", "platform/discovery/Microsoft.Azure.IIoT.Platform.Discovery.Api/src/"]
COPY ["platform/publisher/Microsoft.Azure.IIoT.Platform.Publisher.Api/src/Microsoft.Azure.IIoT.Platform.Publisher.Api.csproj", "platform/publisher/Microsoft.Azure.IIoT.Platform.Publisher.Api/src/"]
COPY ["platform/registry/Microsoft.Azure.IIoT.Platform.Registry.Api/src/Microsoft.Azure.IIoT.Platform.Registry.Api.csproj", "platform/registry/Microsoft.Azure.IIoT.Platform.Registry.Api/src/"]
COPY ["platform/twin/Microsoft.Azure.IIoT.Platform.Twin.Api/src/Microsoft.Azure.IIoT.Platform.Twin.Api.csproj", "platform/twin/Microsoft.Azure.IIoT.Platform.Twin.Api/src/"]
COPY ["platform/twin/Microsoft.Azure.IIoT.Platform.Twin.Service/src/Microsoft.Azure.IIoT.Platform.Twin.Service.csproj", "platform/twin/Microsoft.Azure.IIoT.Platform.Twin.Service/src/"]
COPY ["platform/twin/Microsoft.Azure.IIoT.Platform.Twin/src/Microsoft.Azure.IIoT.Platform.Twin.csproj", "platform/twin/Microsoft.Azure.IIoT.Platform.Twin/src/"]
COPY ["platform/vault/Microsoft.Azure.IIoT.Platform.Vault.Api/src/Microsoft.Azure.IIoT.Platform.Vault.Api.csproj", "platform/vault/Microsoft.Azure.IIoT.Platform.Vault.Api/src/"]

RUN dotnet restore "platform/twin/Microsoft.Azure.IIoT.Platform.Twin.Service/src/Microsoft.Azure.IIoT.Platform.Twin.Service.csproj"
COPY . .
WORKDIR "/src/platform/twin/Microsoft.Azure.IIoT.Platform.Twin.Service/src"
RUN dotnet build "Microsoft.Azure.IIoT.Platform.Twin.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Microsoft.Azure.IIoT.Platform.Twin.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microsoft.Azure.IIoT.Platform.Twin.Service.dll"]