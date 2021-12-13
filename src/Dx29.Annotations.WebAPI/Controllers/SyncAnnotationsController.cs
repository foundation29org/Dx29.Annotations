using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Dx29.Annotations.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SyncAnnotationsController : ControllerBase
    {
        public SyncAnnotationsController(SyncAnnotationService syncAnnotationService)
        {
            SyncAnnotationService = syncAnnotationService;
        }

        public SyncAnnotationService SyncAnnotationService { get; }

        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] TextDocument document)
        {
            try
            {
                var res = await SyncAnnotationService.ExecuteAsync(document.Text);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class TextDocument
    {
        public string Text { get; set; }
    }
}
