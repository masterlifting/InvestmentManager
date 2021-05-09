FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

COPY */*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
COPY *.sln ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/out/ .
ENTRYPOINT ["dotnet", "InvestmentManager.Server.dll"]
