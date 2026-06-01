namespace GestaoRH.Models;

public class DocumentoLote
{
    public int      Id        { get; set; }
    public int      ModeloId  { get; set; }
    public int?     SetorId   { get; set; }
    public int      CriadoPor { get; set; }
    public int      Total     { get; set; }
    public string   Status    { get; set; } = "gerado";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

public class DocumentoInstancia
{
    public int      Id                   { get; set; }
    public int      ModeloId             { get; set; }
    public int      ModeloVersao         { get; set; } = 1;
    public string   ModeloNomeSnapshot   { get; set; } = string.Empty;
    public int?     LoteId               { get; set; }
    public int      FuncionarioId        { get; set; }
    public string   Status               { get; set; } = "pendente";
    public string   ConteudoHtml         { get; set; } = string.Empty;
    public string?  PdfBase64            { get; set; }
    public DateTime CriadoEm            { get; set; } = DateTime.UtcNow;
    public DateTime? ConcluidoEm        { get; set; }

    // Navegação (populados via JOIN)
    public string?  NomeFuncionario      { get; set; }
    public string?  NomeSetor            { get; set; }
    public string?  ModeloNome           { get; set; }
    public List<DocumentoAssinatura>     Assinaturas  { get; set; } = [];
    public List<DocumentoVariavelValor>  Variaveis    { get; set; } = [];
}

public class DocumentoAssinatura
{
    public int      Id                  { get; set; }
    public int      InstanciaId         { get; set; }
    public string   Papel               { get; set; } = "funcionario";
    public int      SignerId            { get; set; }
    public string   SignerTipo          { get; set; } = "funcionario";
    public string   SignerNomeSnapshot  { get; set; } = string.Empty;
    public string   SignerEmailSnapshot { get; set; } = string.Empty;
    public string   Status              { get; set; } = "pendente";
    public bool     Obrigatorio         { get; set; } = true;
    public int      Ordem               { get; set; } = 1;
    public string?  AssinaturaBase64    { get; set; }
    public DateTime? AssinadoEm        { get; set; }
    public string?  IpAddress           { get; set; }
    public DateTime CriadoEm           { get; set; } = DateTime.UtcNow;
}

public class DocumentoVariavelValor
{
    public int    Id          { get; set; }
    public int    InstanciaId { get; set; }
    public string Token       { get; set; } = string.Empty;
    public string Valor       { get; set; } = string.Empty;
}