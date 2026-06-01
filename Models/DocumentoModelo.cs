namespace GestaoRH.Models;

public class DocumentoModelo
{
    public int      Id           { get; set; }
    public string   Nome         { get; set; } = string.Empty;
    public string   Descricao    { get; set; } = string.Empty;
    public string   Categoria    { get; set; } = string.Empty;
    public string   TipoUso      { get; set; } = "individual";
    public string   Status       { get; set; } = "rascunho";
    public int      Versao       { get; set; } = 1;
    public DateTime CriadoEm    { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    // Navegação (populado via JOIN)
    public List<DocumentoModeloSecao>      Secoes     { get; set; } = [];
    public List<DocumentoModeloAssinante>  Assinantes { get; set; } = [];
}

public class DocumentoModeloSecao
{
    public int    Id       { get; set; }
    public int    ModeloId { get; set; }
    public string Titulo   { get; set; } = "Seção";
    public string Tipo     { get; set; } = "texto";   // texto | campos | assinaturas | anexos
    public string Conteudo { get; set; } = string.Empty;
    public int    Ordem    { get; set; }

    public List<DocumentoModeloCampo> Campos { get; set; } = [];
}

public class DocumentoModeloCampo
{
    public int     Id          { get; set; }
    public int     SecaoId     { get; set; }
    public string  Label       { get; set; } = string.Empty;
    public string  TipoCampo   { get; set; } = "texto_curto";
    public bool    Obrigatorio { get; set; } = true;
    public int     Ordem       { get; set; }
    public string? ConfigJson  { get; set; }
}

public class DocumentoModeloAssinante
{
    public int    Id          { get; set; }
    public int    ModeloId    { get; set; }
    public string Papel       { get; set; } = "funcionario";  // funcionario | rh | chefe
    public string Rotulo      { get; set; } = string.Empty;
    public bool   Obrigatorio { get; set; } = true;
    public int    Ordem       { get; set; } = 1;
    public bool   ExibirPdf   { get; set; } = true;
}
