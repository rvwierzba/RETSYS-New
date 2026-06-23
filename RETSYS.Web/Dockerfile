# =========================================================
# Estágio 1: Instalação e Compilação dos Assets do Frontend
# =========================================================
FROM node:20-alpine AS frontend-builder
WORKDIR /app

# Copia os arquivos de definição do npm para otimizar o cache de camadas
COPY RETSYS.Web/package*.json ./
RUN npm install

# Copia os fontes do front e executa o build do Vite v8
COPY RETSYS.Web/ ./
RUN npm run build

# =========================================================
# Estágio 2: Compilação e Publicação do Binário .NET 10
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-builder
WORKDIR /src

# Copia os arquivos de projeto isolados para restaurar dependências via Nuget
COPY ["RETSYS.Domain/RETSYS.Domain.csproj", "RETSYS.Domain/"]
COPY ["RETSYS.Infrastructure/RETSYS.Infrastructure.csproj", "RETSYS.Infrastructure/"]
COPY ["RETSYS.Web/RETSYS.Web.csproj", "RETSYS.Web/"]
RUN dotnet restore "RETSYS.Web/RETSYS.Web.csproj"

# Copia todo o restante do código fonte do projeto
COPY . .

# Injeta os arquivos estáticos gerados pelo Vite no estágio 1 para dentro da wwwroot
COPY --from=frontend-builder /app/wwwroot/ ./RETSYS.Web/wwwroot/

WORKDIR "/src/RETSYS.Web"
RUN dotnet publish "RETSYS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =========================================================
# Estágio 3: Runtime de Produção Leve e Seguro do .NET 10
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=backend-builder /app/publish .

# 🛡️ GARANTIA ABSOLUTA: Copia a pasta pública do front direto para o runtime final
COPY --from=frontend-builder /app/wwwroot/ ./wwwroot/

# Define as portas nativas expostas pelo runtime do .NET
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "RETSYS.Web.dll"]