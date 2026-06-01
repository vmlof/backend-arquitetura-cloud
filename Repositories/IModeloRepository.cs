using GestaoRH.Models;

namespace GestaoRH.Repositories;

public interface IModeloRepository
{
    // Modelo principal
    Task<int>                          CriarAsync(DocumentoModelo modelo);
    Task<DocumentoModelo?>             ObterPorIdAsync(int id);
    Task<IEnumerable<DocumentoModelo>> ListarAsync();
    Task                               AtualizarAsync(DocumentoModelo modelo);
    Task                               PublicarAsync(int id);
    Task                               ArquivarAsync(int id);

    // Seções
    Task<int>  CriarSecaoAsync(DocumentoModeloSecao secao);
    Task       DeletarSecoesPorModeloAsync(int modeloId);

    // Campos
    Task<int>  CriarCampoAsync(DocumentoModeloCampo campo);
    Task       DeletarCamposPorSecaoAsync(int secaoId);

    // Assinantes
    Task<int>  CriarAssinanteAsync(DocumentoModeloAssinante assinante);
    Task       DeletarAssinantesPorModeloAsync(int modeloId);
}
