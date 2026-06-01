using GestaoRH.Models;

namespace GestaoRH.Repositories;

public interface IDocumentoRepository
{
    // Lote
    Task<int>                        CriarLoteAsync(DocumentoLote lote);
    Task<DocumentoLote?>             ObterLotePorIdAsync(int id);

    // Instância
    Task<int>                        CriarInstanciaAsync(DocumentoInstancia inst);
    Task<DocumentoInstancia?>        ObterInstanciaPorIdAsync(int id);
    Task<IEnumerable<DocumentoInstancia>> ListarInstanciasPorFuncionarioAsync(int funcionarioId);
    Task<IEnumerable<DocumentoInstancia>> ListarInstanciasPorSetorAsync(int setorId);
    Task<IEnumerable<DocumentoInstancia>> ListarTodasInstanciasAsync();
    Task                             AtualizarStatusInstanciaAsync(int id, string status, DateTime? concluidoEm = null);
    Task                             SalvarPdfAsync(int id, string pdfBase64);

    // Variáveis manuais
    Task                             InserirVariavelAsync(DocumentoVariavelValor v);

    // Assinaturas
    Task<int>                        CriarAssinaturaAsync(DocumentoAssinatura a);
    Task<DocumentoAssinatura?>       ObterAssinaturaPorIdAsync(int id);
    Task<IEnumerable<DocumentoAssinatura>> ListarAssinaturasPorInstanciaAsync(int instanciaId);
    Task<IEnumerable<DocumentoAssinatura>> ListarAssinaturasPendentesPorSignerAsync(int signerId, string signerTipo);
    Task                             RegistrarAssinaturaAsync(int id, string base64, DateTime assinadoEm, string? ip);

    // Assinatura salva no perfil do funcionário
    Task<string?>                    ObterAssinaturaPerfilAsync(int funcionarioId);
    Task                             SalvarAssinaturaPerfilAsync(int funcionarioId, string base64);
}