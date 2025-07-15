#!/bin/bash

# Install EF Core tools
dotnet tool install --global dotnet-ef

# Run migrations
dotnet ef database update --project src/CharityPay.API/CharityPay.API.csproj
