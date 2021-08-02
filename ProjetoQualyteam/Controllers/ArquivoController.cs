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
    public class ArquivoController : Controller
    {
        private readonly Contexto _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ArquivoController(Contexto context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            this._hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.arquivos.OrderBy(t => t.titulo).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arquivo = await _context.arquivos
                .FirstOrDefaultAsync(m => m.id == id);

            if (arquivo == null)
            {
                return NotFound();
            }

            return View(arquivo);
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

            var arquivo = await _context.arquivos
                .FirstOrDefaultAsync(m => m.id == id);

            var download = new Arquivo()
            {
                id = arquivo.id,
                titulo = arquivo.titulo,
                processo = arquivo.processo,
                categoria = arquivo.categoria,
                Arquivofile = arquivo.Arquivofile,
                Extensaoarquivo = arquivo.Extensaoarquivo

            };


            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            var nomeArquivo = download.titulo + download.Extensaoarquivo;
            int i = 1;
            var qunatidadeArquivo = Directory.EnumerateFileSystemEntries(path, nomeArquivo).ToList<string>().Count;
            if (qunatidadeArquivo >= 1)
            {
                while (qunatidadeArquivo != 0)
                {
                    i++;
                    nomeArquivo = string.Format("{0}{1}{2}", download.titulo, $"({i})", download.Extensaoarquivo);
                    qunatidadeArquivo = Directory.EnumerateFileSystemEntries(path, nomeArquivo).ToList<string>().Count;
                }
            }
            FileStream stream = new FileStream(Path.Combine(path, nomeArquivo), FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter gravabinario = new BinaryWriter(stream);
            gravabinario.Write(download.Arquivofile);
            gravabinario.Close();
            stream.Close();


            new ToastContentBuilder()
                             .AddText("Download realizado com sucesso!")
                             .AddText($"Arquivo foi salvo em: {path}")
                             .Show();

            if (arquivo == null)
            {
                new ToastContentBuilder()
                             .AddText("Houve um problema ao cadastrar o documento")
                             .AddText($"O arquivo não existe")
                             .Show();

                return NotFound();
            }
            return RedirectToAction("Index", "Arquivo");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("id,titulo,processo,categoria,extensaoarquivo")] Arquivo arquivo, IFormFile UploadArquivo, int? id)
        {
            if (ModelState.IsValid)
            {
                if (!ArquivoExists(arquivo.id) && UploadArquivo != null)
                {
                    var extensaoDoArquivo = Path.GetExtension(UploadArquivo.FileName).ToLower();

                    var extensoesPermitidas = new[] { ".doc", ".docx", ".xls", ".xlsx", ".pdf" };

                    if (!extensoesPermitidas.Contains(extensaoDoArquivo))
                    {
                        new ToastContentBuilder()
                             .AddText("Houve um problema ao cadastrar o documento")
                             .AddText($"O formato {extensaoDoArquivo} não é permitido")
                             .Show();
                        return View(arquivo);
                    }

                    var arquivoNovo = new Arquivo()
                    {
                        id = arquivo.id,
                        titulo = arquivo.titulo,
                        processo = arquivo.processo,
                        categoria = arquivo.categoria,
                        Extensaoarquivo = extensaoDoArquivo

                    };

                    using (var stream = new MemoryStream())
                    {
                        UploadArquivo.CopyTo(stream);
                        arquivoNovo.Arquivofile = stream.ToArray();
                    }

                    _context.arquivos.Add(arquivoNovo);
                    _context.SaveChangesAsync();

                    new ToastContentBuilder()
                        .AddText($"Documento {arquivoNovo.id} cadastrado com sucesso!")
                        .Show();

                    return RedirectToAction(nameof(Index));
                }
                else
                {

                    new ToastContentBuilder()
                            .AddText("Houve um problema ao cadastrar o documento")
                            .AddText($"O código {arquivo.id} já existe")
                            .Show();
                }

            }
            return View(arquivo);
        }

        public async Task<IActionResult> Edit(int? id, IFormFile UploadArquivoEdit)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arquivo = await _context.arquivos.FindAsync(id);
            if (arquivo == null)
            {
                return NotFound();
            }
            return View(arquivo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,titulo,processo,categoria,ArquivoFile")] Arquivo arquivo, IFormFile UploadArquivoEdit)
        {
            if (id != arquivo.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if(UploadArquivoEdit != null){
                        var arquivoTemp = await _context.arquivos.AsNoTracking().FirstOrDefaultAsync(m => m.id == id);
                        var extensaoDoArquivo = Path.GetExtension(UploadArquivoEdit.FileName).ToLower();

                        var extensoesPermitidas = new[] { ".doc", ".docx", ".xls", ".xlsx", ".pdf" };

                        if (!extensoesPermitidas.Contains(extensaoDoArquivo))
                        {
                            new ToastContentBuilder()
                                 .AddText("Houve um problema ao cadastrar o documento")
                                 .AddText($"O formato {extensaoDoArquivo} não é permitido")
                                 .Show();
                            return View(arquivo);
                        }


                        var arquivoUpdate = new Arquivo()
                        {
                            id = arquivo.id,
                            titulo = arquivo.titulo,
                            processo = arquivo.processo,
                            categoria = arquivo.categoria,
                            Extensaoarquivo = extensaoDoArquivo

                        };
                        using (var stream = new MemoryStream())
                        {
                            UploadArquivoEdit.CopyTo(stream);
                            arquivoUpdate.Arquivofile = stream.ToArray();
                        }
                        _context.Update(arquivoUpdate);
                    }
                    else
                    {
                        var arquivoTemp = await _context.arquivos.AsNoTracking().FirstOrDefaultAsync(m => m.id == id);

                        var arquivoUpdate = new Arquivo()
                        {
                            id = arquivo.id,
                            titulo = arquivo.titulo,
                            processo = arquivo.processo,
                            categoria = arquivo.categoria,
                            Arquivofile = arquivoTemp.Arquivofile,
                            Extensaoarquivo = arquivoTemp.Extensaoarquivo

                        };
                        _context.Update(arquivoUpdate);
                    }
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArquivoExists(arquivo.id))
                    {
                        new ToastContentBuilder()
                            .AddText("Falha ao tentar realizar a alteração, tente novamente")
                            .Show();

                        return NotFound();
                    }
                    else
                    {
                        new ToastContentBuilder()
                            .AddText("Falha ao tentar realizar a alteração, tente novamente")
                            .Show();
                        throw;
                    }
                }
                new ToastContentBuilder()
                            .AddText("Alteração realizada com sucesso!")
                            .Show();

                return RedirectToAction(nameof(Index));
            }
            return View(arquivo);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arquivo = await _context.arquivos
                .FirstOrDefaultAsync(m => m.id == id);
            if (arquivo == null)
            {
                return NotFound();
            }

            return View(arquivo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var arquivo = await _context.arquivos.FindAsync(id);

            _context.arquivos.Remove(arquivo);
            await _context.SaveChangesAsync();

            new ToastContentBuilder()
            .AddText("Exclusão realizada com sucesso!")
            .Show();

            return RedirectToAction(nameof(Index));
        }

        private bool ArquivoExists(int id)
        {
            return _context.arquivos.Any(e => e.id == id);
        }
    }
}