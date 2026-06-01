namespace GestaoRH.Models.DTOs;

public class GerarDocumentoDto
{
    public int    ModeloId      { get; set; }
    public int    FuncionarioId { get; set; }                   
    public int?   SetorId       { get; set; }                    
    public List<VariavelManualDto> VariaveisManuals { get; set; } = [];
}

public class VariavelManualDto
{
    public string Token { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}


public class AssinarDocumentoDto
{
    public string AssinaturaBase64   { get; set; } = string.Empty;
    public bool   SalvarNoPerfil     { get; set; } = false;   // "usar como padrão"
    public string? IpAddress         { get; set; }
}

public class AssinaturaResponseDto
{
    public int      Id                  { get; set; }
    public string   Papel               { get; set; } = string.Empty;
    public string   SignerNomeSnapshot  { get; set; } = string.Empty;
    public string   Status              { get; set; } = string.Empty;
    public bool     Obrigatorio         { get; set; }
    public int      Ordem               { get; set; }
    public DateTime? AssinadoEm        { get; set; }
    // AssinaturaBase64 NÃO é retornada na listagem — só no PDF final
}

public class InstanciaListagemDto
{
    public int      Id                  { get; set; }
    public string   ModeloNome          { get; set; } = string.Empty;
    public string   NomeFuncionario     { get; set; } = string.Empty;
    public string   NomeSetor           { get; set; } = string.Empty;
    public string   Status              { get; set; } = string.Empty;
    public DateTime CriadoEm           { get; set; }
    public DateTime? ConcluidoEm       { get; set; }
    public int      TotalAssinaturas    { get; set; }
    public int      AssinaturasPendentes { get; set; }
}

public class InstanciaDetalheDto
{
    public int      Id                  { get; set; }
    public int      ModeloId            { get; set; }
    public string   ModeloNome          { get; set; } = string.Empty;
    public int?     LoteId              { get; set; }
    public int      FuncionarioId       { get; set; }
    public string   NomeFuncionario     { get; set; } = string.Empty;
    public string   NomeSetor           { get; set; } = string.Empty;
    public string   Status              { get; set; } = string.Empty;
    public string   ConteudoHtml        { get; set; } = string.Empty;
    public DateTime CriadoEm           { get; set; }
    public DateTime? ConcluidoEm       { get; set; }
    public List<AssinaturaResponseDto> Assinaturas { get; set; } = [];
    public List<VariavelManualDto>     Variaveis   { get; set; } = [];
    // MinhaAssinaturaPendente: id da assinatura que o usuário logado deve assinar (ou null)
    public int? MinhaAssinaturaPendenteId { get; set; }
}

public class LoteResponseDto
{
    public int      Id         { get; set; }
    public int      ModeloId   { get; set; }
    public string   ModeloNome { get; set; } = string.Empty;
    public int?     SetorId    { get; set; }
    public string?  NomeSetor  { get; set; }
    public int      Total      { get; set; }
    public string   Status     { get; set; } = string.Empty;
    public DateTime CriadoEm  { get; set; }
    public List<InstanciaListagemDto> Instancias { get; set; } = [];
}

// ── Assinatura salva no perfil ───────────────────────────────

public class SalvarAssinaturaPerfilDto
{
    public string AssinaturaBase64 { get; set; } = string.Empty;
}

public class AssinaturaPerfilResponseDto
{
    public bool    Possui           { get; set; }
    public string? AssinaturaBase64 { get; set; }   // retornada para pré-visualização
}