FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS base

USER root

RUN <<EOT bash
set -euo pipefail

apt-get update

export DEBIAN_FRONTEND=noninteractive
apt-get install --no-install-recommends --no-install-suggests -y \
    git \
    curl \
    unzip

rm -rf /var/lib/apt/lists/*
EOT

#######################
#       AWS
#######################
RUN <<EOT bash
set -euo pipefail
curl -fsSL https://awscli.amazonaws.com/v2/install.sh | bash -s -- --system
EOT

#######################
#     TERRAFORM
#######################
RUN <<EOT bash
set -euo pipefail

git clone --depth=1 https://github.com/tfutils/tfenv.git /usr/local/lib/tfenv
ln -s /usr/local/lib/tfenv/bin/* /usr/local/bin
EOT

RUN <<EOT bash
set -euo pipefail

tfenv install latest
tfenv use latest
EOT

USER $APP_UID
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-aot AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Carnitas/Carnitas.csproj", "Carnitas/"]
COPY ["Carnitas.Grpc/Carnitas.Grpc.csproj", "Carnitas.Grpc/"]
COPY ["Carnitas.CLI/Carnitas.CLI.csproj", "Carnitas.CLI/"]

RUN dotnet restore "Carnitas.CLI/Carnitas.CLI.csproj" -a $TARGETARCH
COPY . .
WORKDIR "/src/Carnitas.CLI"
RUN dotnet build -a $TARGETARCH "./Carnitas.CLI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -a $TARGETARCH "./Carnitas.CLI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["cns"]
