using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProgramInstaller.Models;

public class Programa {
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Caminho { get; set; }
    public string Argumentos { get; set; }
    public string x86 { get; set; }
    public string x64 { get; set; }
}

[XmlRoot("Programas")]
public class Programas {
    [XmlElement("Programa")]
    public List<Programa> ListaProgramas { get; set; }

    public Programas()
    {
        ListaProgramas = new();
    }
}