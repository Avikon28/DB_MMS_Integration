FROM microsoft/aspnetcore:2.0
ARG source=.
WORKDIR /app
COPY $source .
ENTRYPOINT ["dotnet", "SG.PO.Chino.ProcessingService.dll"]
