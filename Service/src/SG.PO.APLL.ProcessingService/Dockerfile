﻿FROM microsoft/dotnet:2.0-runtime
ARG source=.
WORKDIR /app
COPY $source .
ENTRYPOINT ["dotnet", "SG.PO.APLL.ProcessingService.dll"]
