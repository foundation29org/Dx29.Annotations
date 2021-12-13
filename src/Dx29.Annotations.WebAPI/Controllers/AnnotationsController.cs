using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Dx29.Jobs;
using Dx29.Services;

namespace Dx29.Annotations.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AnnotationsController : ControllerBase
    {
        const int SIZE_LIMIT = 24 * 1024 * 1024; // 24 MB

        public AnnotationsController(ServiceBus serviceBus, BlobStorage blobStorage)
        {
            ServiceBus = serviceBus;
            BlobStorage = blobStorage;
        }

        public ServiceBus ServiceBus { get; }
        public BlobStorage BlobStorage { get; }

        [HttpPost("process")]
        [RequestSizeLimit(SIZE_LIMIT)]
        public async Task<IActionResult> Process(double threshold = 0.89)
        {
            try
            {
                var client = AnnotationsClient.CreateNew(ServiceBus, BlobStorage);
                await client.UploadInputAsync("document.bin", Request.Body);

                var jobInfo = await client.InitializeAsync();
                jobInfo.Args["threshold"] = threshold.ToString();
                var status = await client.SendMessageAsync(jobInfo);

                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("process/{userId}/{caseId}/{reportId}")]
        [RequestSizeLimit(SIZE_LIMIT)]
        public async Task<IActionResult> ProcessWithCase(string userId, string caseId, string reportId, double threshold = 0.89)
        {
            try
            {
                var client = AnnotationsClient.CreateNew(ServiceBus, BlobStorage);
                await client.UploadInputAsync("document.bin", Request.Body);

                var jobInfo = await client.InitializeAsync();
                jobInfo.Args["userId"] = userId;
                jobInfo.Args["caseId"] = caseId;
                jobInfo.Args["reportId"] = reportId;
                jobInfo.Args["threshold"] = threshold.ToString();
                var status = await client.SendMessageAsync(jobInfo);

                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status([FromQuery] TokenParams parms)
        {
            try
            {
                var client = new AnnotationsClient(ServiceBus, BlobStorage, parms.Token);
                var status = await client.GetStatusAsync();
                if (status != null)
                {
                    return Ok(status);
                }
                return StatusCode(404);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("results")]
        public async Task<IActionResult> Results([FromQuery] TokenParams parms)
        {
            try
            {
                var client = new AnnotationsClient(ServiceBus, BlobStorage, parms.Token);
                var status = await client.GetStatusAsync();
                if (status != null)
                {
                    if (status.Status == CommonStatus.Succeeded.ToString())
                    {
                        var results = await client.GetResultsAsync();
                        return Ok(results);
                    }
                    return BadRequest("Invalid job status for results request.");
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
