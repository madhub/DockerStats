# DockerStats
CLI tool to log Docker Stats in CSV format using C# Docker Client SDK.

![image](https://github.com/madhub/DockerStats/assets/8907962/2532086d-2183-4e84-b00e-96a1c650e75e)

# Source code build
1. Checkout the DockerStats repo
    ```shell
    git clone https://github.com/madhub/DockerStats.git
    ```  
2. Build self contained executable that work on windows & Linux
      * Windows Build
        ```shell
        // Binary will be published to bin\Release\net6.0\publish\win-x64\
        dotnet publish -c Release -p:PublishProfile=WinProfile
        ```  
    * Linux Build
        ```shell
        // Binary will be published to bin\Release\net6.0\publish\linux-x64 directory
        dotnet publish -c Release -p:PublishProfile=LinuxProfile
        ```  
3. Run command
    ```shell
        DockerStats --help
    ```
