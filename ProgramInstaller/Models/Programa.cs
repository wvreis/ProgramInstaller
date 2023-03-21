using System;

namespace ProgramInstaller.Models;

public class Programa {
    public string Nome { get; set; }
    public string Caminho { get; set; }
    public string Argumentos { get; set; }
    public string x86 { get; set; }
    public string x64 { get; set; }

    public Guid Id { get; set; }

    public Programa()
    {
        Id = Guid.NewGuid();
    }
}

