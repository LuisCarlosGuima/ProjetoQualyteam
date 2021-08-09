using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Uwp.Notifications;
using ProjetoQualyteam.Models;

namespace ProjetoQualyteam.Controllers
{
    public class DocumentoController : Controller
    {
        private readonly Contexto _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DocumentoController(Contexto context, IWebHostEnvironment hostEnvironment)
        {                       
            _context = context;
            this._hostEnvironment = hostEnvironment;
        }

        public string[] ExtensoesPermitidas = new[] { ".doc", ".docx", ".xls", ".xlsx", ".pdf" };
        public bool VerificaExtensaoDoArquivo(IFormFile arquivo)
        {
            var extensao = Path.GetExtension(arquivo.FileName);

            if (ExtensoesPermitidas.Contains(extensao))
                return true;

            new ToastContentBuilder()
                .AddText("Houve um problema ao cadastrar o documento")
                .AddText($"O formato '{extensao.ToUpper()}' não é permitido")
                .AddText($"Extensões validas são: {string.Join(',', ExtensoesPermitidas)}")
                .Show();

            return false;
        }

        public async Task<IActionResult> Index()
            => View(await _context.Documentos.OrderBy(documento => documento.Titulo).ToListAsync());
        

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var documento = await BuscarDocumentoPorId(id);

            if (documento == null)
                return NotFound();

            return View(documento);
        }
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null)
            {
                new ToastContentBuilder()
                             .AddText("Houve um problema ao cadastrar o documento")
                             .AddText($"O arquivo não existe")
                             .Show();
                return NotFound();
            }

            var arquivo = await BuscarDocumentoPorId(id);
            var caminhoDoArquivo = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            var nomeDoArquivo = arquivo.Titulo + arquivo.ExtensaoDoArquivo;

            var quantidadeDeArquivos = Directory.EnumerateFileSystemEntries(caminhoDoArquivo, nomeDoArquivo).ToList<string>().Count;

            if (quantidadeDeArquivos >= 1)
            {
                for (int i = 0; quantidadeDeArquivos != 0; i++)
                {
                    nomeDoArquivo = $"{arquivo.Titulo}({i + 1}){arquivo.ExtensaoDoArquivo}";
                    quantidadeDeArquivos = Directory.EnumerateFiles(caminhoDoArquivo, nomeDoArquivo).ToList().Count;
                }
            }

            var fileStream = new FileStream(Path.Combine(caminhoDoArquivo, nomeDoArquivo), FileMode.OpenOrCreate, FileAccess.Write);

            using (var binaryWriter = new BinaryWriter(fileStream))
            {
                binaryWriter.Write(arquivo.Arquivo);
                binaryWriter.Close();
            }
            fileStream.Close();

            new ToastContentBuilder()
                .AddText("Download realizado com sucesso!")
                .AddText($"Arquivo foi salvo em: {caminhoDoArquivo}")
                .Show();

            if (arquivo == null)
            {
                new ToastContentBuilder()
                             .AddText("Houve um problema ao cadastrar o documento")
                             .AddText($"O arquivo não existe")
                             .Show();

                return NotFound();
            }
            return RedirectToAction("Index", "Documento");
        }

        private async Task<Documento> BuscarDocumentoPorId(int? id)
            => await _context.Documentos.FirstOrDefaultAsync(documento => documento.Id == id);
        

        public IActionResult Create()
            => View();
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Titulo,Processo,Categoria,extensaoarquivo")] Documento documento, IFormFile arquivoParaUpload, int? id)
        {
            if (ModelState.IsValid)
            {
                if (!ArquivoExiste(documento.Id))
                {
                    if (!ArquivoValido(arquivoParaUpload))
                        return View(documento);
                    

                    using (var memoryStream = new MemoryStream())
                    {
                        arquivoParaUpload.CopyTo(memoryStream);
                        documento.Arquivo = memoryStream.ToArray();
                        documento.ExtensaoDoArquivo = Path.GetExtension(arquivoParaUpload.FileName);
                    }

                    _context.Documentos.Add(documento);
                    _context.SaveChangesAsync();

                    new ToastContentBuilder()
                        .AddText($"Documento {documento.Id} cadastrado com sucesso!")
                        .Show();

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    new ToastContentBuilder()
                        .AddText("Houve um problema ao cadastrar o documento")
                        .AddText($"O código {documento.Id} já existe")
                        .Show();
                }

            }

            return View(documento);
        }

        private bool ArquivoValido(IFormFile arquivoParaUpload)
            => arquivoParaUpload != null && VerificaExtensaoDoArquivo(arquivoParaUpload);
        

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            

            var documento = await _context.Documentos.FindAsync(id);

            if (documento == null)
                return NotFound();

            return View(documento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Processo,Categoria,ArquivoFile")] Documento documento, IFormFile arquivoParaUpload)
        {
            if (id != documento.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if(arquivoParaUpload != null)
                    {
                        var extensaoDoArquivo = Path.GetExtension(arquivoParaUpload.FileName).ToLower();

                        if (!ArquivoValido(arquivoParaUpload))
                            return View(documento);

                        using (var memoryStream = new MemoryStream())
                        {
                            arquivoParaUpload.CopyTo(memoryStream);
                            documento.Arquivo = memoryStream.ToArray();
                            documento.ExtensaoDoArquivo = extensaoDoArquivo;
                        }

                        _context.Update(documento);
                    }
                    else
                    {
                        var arquivoTemporario = await BuscarDocumentoPorId(id);

                        documento.Arquivo = arquivoTemporario.Arquivo;
                        documento.ExtensaoDoArquivo = arquivoTemporario.ExtensaoDoArquivo;

                        _context.Update(documento);
                    }
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArquivoExiste(documento.Id))
                    {
                        new ToastContentBuilder()
                            .AddText("Falha ao tentar realizar a alteração, tente novamente")
                            .Show();

                        return NotFound();
                    }
                    else
                        new ToastContentBuilder()
                            .AddText("Falha ao tentar realizar a alteração, tente novamente")
                            .Show();

                    throw;
                }

                new ToastContentBuilder()
                            .AddText("Alteração realizada com sucesso!")
                            .Show();

                return RedirectToAction(nameof(Index));
            }
            return View(documento);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            

            var documento = await BuscarDocumentoPorId(id);

            if (documento == null)
                return NotFound();

            return View(documento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var documento = await _context.Documentos.FindAsync(id);

            _context.Documentos.Remove(documento);
            await _context.SaveChangesAsync();

            new ToastContentBuilder()
                .AddText("Exclusão realizada com sucesso!")
                .Show();

            return RedirectToAction(nameof(Index));
        }

        private bool ArquivoExiste(int Id)
            => _context.Documentos.Any(documento => documento.Id == Id);
        
    }
}