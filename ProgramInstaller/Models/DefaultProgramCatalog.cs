using System;

namespace ProgramInstaller.Models;

public static class DefaultProgramCatalog
{
    public static Programas Create() =>
        new()
        {
            ListaProgramas =
            [
                new Programa
                {
                    Id = Guid.Parse("1109afca-bbe5-4204-8dea-d058327d5135"),
                    Nome = "Chrome",
                    Caminho = "winget",
                    Argumentos = "install --id Google.Chrome -e --source winget --accept-package-agreements --accept-source-agreements --silent --disable-interactivity",
                    x86 = "S",
                    x64 = "S",
                    Ativo = true
                },
                new Programa
                {
                    Id = Guid.Parse("b4b81a5f-e13e-413e-a9be-441dbaa00123"),
                    Nome = "AnyDesk",
                    Caminho = "winget",
                    Argumentos = "install --id AnyDesk.AnyDesk -e --source winget --accept-package-agreements --accept-source-agreements --silent --disable-interactivity",
                    x86 = "S",
                    x64 = "S",
                    Ativo = true
                },
                new Programa
                {
                    Id = Guid.Parse("af89cbcc-9465-4ec6-92a1-5a23c1f2510c"),
                    Nome = "Microsoft Office 365",
                    Caminho = "winget",
                    Argumentos = "install --id Microsoft.Office --accept-package-agreements --accept-source-agreements --scope machine --silent --force --ignore-security-hash",
                    x86 = "S",
                    x64 = "S",
                    Ativo = true
                },
                new Programa
                {
                    Id = Guid.Parse("7385dbd0-356f-4bba-ab76-2e378617a9fa"),
                    Nome = "Avast Free",
                    Caminho = "winget",
                    Argumentos = "install --id XPDNZJFNCR1B07 -e --source winget --accept-package-agreements --accept-source-agreements --silent --disable-interactivity",
                    x86 = "S",
                    x64 = "S",
                    Ativo = true
                },
                new Programa
                {
                    Id = Guid.Parse("51438a93-6f31-45bc-a9f2-3f65bc12571f"),
                    Nome = "WinRAR",
                    Caminho = "winget",
                    Argumentos = "install --id RARLab.WinRAR -e --source winget --accept-package-agreements --accept-source-agreements --silent --disable-interactivity",
                    x86 = "S",
                    x64 = "S",
                    Ativo = true
                },
                new Programa
                {
                    Id = Guid.Parse("ec66183a-079b-4f9a-b256-cfc071b3d2f5"),
                    Nome = "VLC Player",
                    Caminho = "winget",
                    Argumentos = "install --id VideoLAN.VLC -e --source winget --accept-package-agreements --accept-source-agreements --silent --disable-interactivity",
                    x86 = "S",
                    x64 = "S",
                    Ativo = true
                }
            ]
        };
}
