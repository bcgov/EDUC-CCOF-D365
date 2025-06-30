using CCOF.Infrastructure.WebAPI.Models;
using CCOF.Infrastructure.WebAPI.Services.Processes;

namespace CCOF.Infrastructure.WebAPI.Services.Documents;

public interface ID365DocumentService 
{
    Task<HttpResponseMessage> GetAsync(string documentId);
    Task<ProcessResult> UploadAsync(IFormFileCollection files, IEnumerable<FileMapping> fileMappings);
    Task<HttpResponseMessage> RemoveAsync(string documentId);
}