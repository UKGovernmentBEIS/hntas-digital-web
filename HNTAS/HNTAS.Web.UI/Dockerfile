ARG BUILD_CONFIGURATION=Release
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /_solution
COPY . .
WORKDIR /_solution/HNTAS.Web.UI
RUN dotnet restore 
RUN dotnet build --configuration $BUILD_CONFIGURATION --no-restore
RUN dotnet publish --configuration $BUILD_CONFIGURATION --no-build --no-restore --output /_publish
RUN ls -l /_publish


FROM  public.ecr.aws/docker/library/node:20-alpine as node
WORKDIR /with-node-modules
RUN apk add --no-cache util-linux
COPY --from=build /_publish .
RUN npm install


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=node /with-node-modules .
ENV ASPNETCORE_HTTP_PORTS=8000
EXPOSE 8000
ENTRYPOINT ["dotnet", "HNTAS.Web.UI.dll"]
