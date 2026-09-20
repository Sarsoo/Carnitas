FROM --platform=$BUILDPLATFORM hashicorp/terraform:latest as terraform

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS base
COPY --from=terraform /bin/terraform /bin/

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
