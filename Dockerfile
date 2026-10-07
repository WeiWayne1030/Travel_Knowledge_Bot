# 階段 1：使用 .NET SDK 進行編譯 (Build Stage)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# 複製專案檔並還原依賴 (這一步會被 Docker 快取，加速後續建置)
COPY ["src/OkinawaBot/OkinawaBot.csproj", "src/OkinawaBot/"]
RUN dotnet restore "src/OkinawaBot/OkinawaBot.csproj"

# 複製剩餘的程式碼並發布
COPY . .
WORKDIR "/source/src/OkinawaBot"
RUN dotnet publish "OkinawaBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 階段 2：使用更輕量的 ASP.NET Runtime 進行執行 (Runtime Stage)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# 把階段 1 編譯好的檔案複製過來
COPY --from=build /app/publish .

# 設定 Zeabur 常用的 8080 Port 供外部存取
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# 啟動應用程式
ENTRYPOINT ["dotnet", "OkinawaBot.dll"]
