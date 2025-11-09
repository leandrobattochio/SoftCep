# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["src/SoftCep.Api/SoftCep.Api.csproj", "src/SoftCep.Api/"]

RUN dotnet restore "src/SoftCep.Api/SoftCep.Api.csproj"

COPY . .

# Build da aplicação
WORKDIR "/src/src/SoftCep.Api"
RUN dotnet build "SoftCep.Api.csproj" -c Release -o /app/build

# Publicar a aplicação
FROM build AS publish
RUN dotnet publish "SoftCep.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio final - runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Instalar curl para healthcheck
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Criar usuário não-root para segurança
RUN addgroup --system --gid 1001 appuser && \
    adduser --system --uid 1001 --ingroup appuser appuser

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Alterar proprietário dos arquivos
RUN chown -R appuser:appuser /app

# Usar usuário não-root
USER appuser

# Expor porta
EXPOSE 8080

# Variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Comando de inicialização
ENTRYPOINT ["dotnet", "SoftCep.Api.dll"]
