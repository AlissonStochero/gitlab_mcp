# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar solução e projetos para restaurar dependências
COPY ["GitLabMcp.sln", "./"]
COPY ["src/GitLabMcp.Application/GitLabMcp.Application.csproj", "src/GitLabMcp.Application/"]
COPY ["src/GitLabMcp.Domain/GitLabMcp.Domain.csproj", "src/GitLabMcp.Domain/"]
COPY ["src/GitLabMcp.Infrastructure/GitLabMcp.Infrastructure.csproj", "src/GitLabMcp.Infrastructure/"]
COPY ["src/GitLabMcp.Presentation.Http/GitLabMcp.Presentation.Http.csproj", "src/GitLabMcp.Presentation.Http/"]
# Se houver testes, copiar também se necessário para o build, mas para deploy foca-se no app
COPY ["tests/GitLabMcp.UnitTests/GitLabMcp.UnitTests.csproj", "tests/GitLabMcp.UnitTests/"]
COPY ["tests/GitLabMcp.IntegrationTests/GitLabMcp.IntegrationTests.csproj", "tests/GitLabMcp.IntegrationTests/"]

RUN dotnet restore "GitLabMcp.sln"

# Copiar todo o código fonte
COPY . .

# Publicar a aplicação
WORKDIR "/src/src/GitLabMcp.Presentation.Http"
RUN dotnet publish "GitLabMcp.Presentation.Http.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final / Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Criar usuário não-root para segurança (opcional, imagens .NET 8+ chiseled ou user-defined já são seguras, mas boa prática explícita)
USER app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GitLabMcp.Presentation.Http.dll"]
