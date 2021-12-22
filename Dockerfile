FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /app
COPY 7zbson-decompress .
CMD ["dotnet","run"]