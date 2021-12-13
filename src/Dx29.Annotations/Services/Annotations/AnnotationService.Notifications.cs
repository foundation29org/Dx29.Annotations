using System;
using System.Threading.Tasks;

using Dx29.Data;
using Dx29.Services;

namespace Dx29.Annotations
{
    partial class AnnotationService
    {
        private async Task SendNotificationAsync(ReportInfo reportInfo)
        {
            await SignalRService.SendUserAsync(reportInfo.UserId, "AnnotationsReady", reportInfo);
        }

        private async Task SendNotificationAsync(ReportInfo reportInfo, Exception exception)
        {
            await SignalRService.SendUserAsync(reportInfo.UserId, "AnnotationsError", reportInfo, exception.Message);
        }
    }
}
