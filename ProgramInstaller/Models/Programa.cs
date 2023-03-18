using System.Collections.Generic;

namespace ProgramInstaller.Models;
public class Programas {
    public List<Programa> ListaProgramas { get; set; }
}

public class Programa {
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Caminho { get; set; }
    public string Argumentos { get; set; }
    public string x86 { get; set; }
    public string x64 { get; set; }
}
