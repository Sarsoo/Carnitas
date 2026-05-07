FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Carnitas/Carnitas.csproj", "Carnitas/"]
COPY ["Carnitas.Web/Carnitas.Web.csproj", "Carnitas.Web/"]
COPY ["Carnitas.Model/Carnitas.Model.csproj", "Carnitas.Model/"]
RUN dotnet restore "Carnitas.Web/Carnitas.Web.csproj" -a $TARGETARCH
COPY . .
WORKDIR "/src/Carnitas.Web"
RUN dotnet build -a $TARGETARCH "./Carnitas.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -a $TARGETARCH "./Carnitas.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM --platform=$BUILDPLATFORM hashicorp/terraform:latest as terraform

FROM base AS final
COPY --from=terraform /bin/terraform /bin/
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Carnitas.Web.dll"]
