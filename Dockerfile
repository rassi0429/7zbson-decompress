FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY 7zbson-decompress .
CMD ["dotnet","run"]