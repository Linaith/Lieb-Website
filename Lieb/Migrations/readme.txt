https://learn.microsoft.com/en-us/aspnet/core/data/ef-rp/migrations?view=aspnetcore-6.0&tabs=visual-studio-code

open ProjectFolder in Terminal
dotnet ef migrations add InitialCreate

remove migrations
1: revert to migration you want to keep
dotnet ef database update RemovedRaidLog
2: remove
dotnet ef migrations remove