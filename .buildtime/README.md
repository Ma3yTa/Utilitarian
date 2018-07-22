# Buildtime

This folder houses a multi-platform build process defined with [FAKE 5](https://fake.build/). There is a main build script called `build.fsx` with build target definitions and target ordering for being executed by the FAKE script engine.

The build process is split into modules distributed over `build_<capability>.fsx` files. Those are best used from within a buildtime `.fsx` file with a module alias:

```fsharp
#load build_dotnet.fsx
module DotNet = Build_dotnet
```

Note that FSI implicitly constructs a module from `.fsx` filename (e.g. `Build.dotnet` from `build_dotnet.fsx`).

## Build modules explained

### `build_filesystem.fsx`
enforces the required file system of the build process realtive to the root of the repository. 

### `build_projectinfo.fsx`
contains the typical project info boilerplate like release and version info, description tags, ... in a central place. Buildtime code that needs this info (for creation of NuGet packages,GitHub releases, Docker images, app bundles for CDN ...) can simply reference this build module.

### `build_dotnet.fsx`
contains helper functions to execute commands of the new [`dotnet`](https://www.microsoft.com/net/learn/get-started/) SDK.

### `build_Fable.fsx`
contains helper functions to execute commands for app and library development with Fable.