# AudioChord
## A music utility library for [Discord.net](https://github.com/RogueException/Discord.Net)

### Pushing new nuget packages
Pushing packages is already supported by the dotnet tool

To be able to push we need to 'pack' a project first. navigate to the project in question and execute

**before packing, check if you incremented the package version in the csproj**

```
dotnet pack --configuration Release
```

The command result will tell you where the package was created, navigate to the containing folder and push the package to a server
```
dotnet nuget push {path_to_package} -k {api_key} -s {nuget_package_server}
```

### requirements:
- MongoDB database with GridFS enabled
- FFMpeg 
- Dotnet Core 2.0
