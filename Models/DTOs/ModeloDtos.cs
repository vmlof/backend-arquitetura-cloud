namespace GestaoRH.Models.DTOs;

// ── Request DTOs ─────────────────────────────────────────────

public class SecaoCadastroDto
{
    public string  Id       { get; set; } = string.Empty;  // id local do front (ignorado no back)
    public string  Titulo   { get; set; } = "Seção";
    public string  Tipo     { get; set; } = "texto";
    public string  Conteudo { get; set; } = string.Empty;
    public int     Ordem    { get; set; }
    public List<CampoCadastroDto> Campos { get; set; } = [];
}

public class CampoCadastroDto
{
    public string  Id          { get; set; } = string.Empty;
    public string  Label       { get; set; } = string.Empty;
    public string  TipoCampo   { get; set; } = "texto_curto";
    public bool    Obrigatorio { get; set; } = true;
    public int     Ordem       { get; set; }
}

public class AssinanteCadastroDto
{
    public string  Id          { get; set; } = string.Empty;
    public string  Papel       { get; set; } = "funcionario";
    public string  Rotulo      { get; set; } = string.Empty;
    public bool    Obrigatorio { get; set; } = true;
    public int     Ordem       { get; set; } = 1;
    public bool    ExibirPdf   { get; set; } = true;
}

public class ModeloCadastroDto
{
    public string  Nome       { get; set; } = string.Empty;
    public string  Descricao  { get; set; } = string.Empty;
    public string  Categoria  { get; set; } = string.Empty;
    public string  TipoUso    { get; set; } = "individual";
    public List<SecaoCadastroDto>     Secoes     { get; set; } = [];
    public List<AssinanteCadastroDto> Assinantes { get; set; } = [];
}

// ── Response DTOs ────────────────────────────────────────────

public class CampoResponseDto
{
    public int    Id          { get; set; }
    public string Label       { get; set; } = string.Empty;
    public string TipoCampo   { get; set; } = string.Empty;
    public bool   Obrigatorio { get; set; }
    public int    Ordem       { get; set; }
}

public class SecaoResponseDto
{
    public int    Id       { get; set; }
    public string Titulo   { get; set; } = string.Empty;
    public string Tipo     { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public int    Ordem    { get; set; }
    public List<CampoResponseDto> Campos { get; set; } = [];
}

public class AssinanteResponseDto
{
    public int    Id          { get; set; }
    public string Papel       { get; set; } = string.Empty;
    public string Rotulo      { get; set; } = string.Empty;
    public bool   Obrigatorio { get; set; }
    public int    Ordem       { get; set; }
    public bool   ExibirPdf   { get; set; }
}

public class ModeloResponseDto
{
    public int      Id           { get; set; }
    public string   Nome         { get; set; } = string.Empty;
    public string   Descricao    { get; set; } = string.Empty;
    public string   Categoria    { get; set; } = string.Empty;
    public string   TipoUso      { get; set; } = string.Empty;
    public string   Status       { get; set; } = string.Empty;
    public int      Versao       { get; set; }
    public DateTime CriadoEm    { get; set; }
    public DateTime AtualizadoEm { get; set; }
    public List<SecaoResponseDto>     Secoes     { get; set; } = [];
    public List<AssinanteResponseDto> Assinantes { get; set; } = [];
}

// Listagem simplificada (sem seções/campos — para a tabela)
public class ModeloListagemDto
{
    public int      Id        { get; set; }
    public string   Nome      { get; set; } = string.Empty;
    public string   Descricao { get; set; } = string.Empty;
    public string   Categoria { get; set; } = string.Empty;
    public string   TipoUso   { get; set; } = string.Empty;
    public string   Status    { get; set; } = string.Empty;
    public int      Versao    { get; set; }
    public DateTime CriadoEm { get; set; }
}
