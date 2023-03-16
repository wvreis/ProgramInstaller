using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramInstaller.Models;
class Programa {
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Caminho { get; set; }
    public string Argumentos { get; set; }
    public string x86 { get; set; }
    public string x64 { get; set; }
}
