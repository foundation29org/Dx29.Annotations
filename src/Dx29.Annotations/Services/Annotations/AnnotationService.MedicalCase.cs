using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Dx29.Data;
using Dx29.Services;

namespace Dx29.Annotations
{
    partial class AnnotationService
    {
        private async Task UpdateMedicalCaseAsync(string userId, string caseId, string reportId, double threshold, DocAnnotations[] docAnnotations)
        {
            // Get Phenotype Reports ResourceGroup
            var reportsResourceGroup = await ResourceGroupService.GetResourceGroupByTypeNameAsync(userId, caseId, ResourceGroupType.Reports, "Medical");
            var report = reportsResourceGroup.Resources.TryGetValue(reportId);
            if (report != null)
            {
                // Get Phenotype ResourceGroup
                var phensResourceGroup = await ResourceGroupService.GetResourceGroupByTypeNameAsync(userId, caseId, ResourceGroupType.Phenotype, reportId);
                if (phensResourceGroup == null)
                {
                    phensResourceGroup = await ResourceGroupService.CreateResourceGroupAsync(userId, caseId, ResourceGroupType.Phenotype, reportId);
                }
                foreach (var docAnns in docAnnotations)
                {
                    AppendResources(phensResourceGroup, docAnns, threshold);
                }
                NormalizeSegments(phensResourceGroup);
                await ResourceGroupService.UpdateResourceGroupAsync(phensResourceGroup);

                report.Status = "Ready";
                await ResourceGroupService.UpdateResourceGroupAsync(reportsResourceGroup);
            }
        }

        private async Task UpdateMedicalCaseAsync(string userId, string caseId, string reportId, Exception exception)
        {
            var reportsResourceGroup = await ResourceGroupService.GetResourceGroupByTypeNameAsync(userId, caseId, ResourceGroupType.Reports, "Medical");
            var report = reportsResourceGroup.Resources.TryGetValue(reportId);
            report.Status = "Error";
            report.Properties["ErrorMessage"] = exception.Message;
            report.Properties["ErrorDetails"] = exception.StackTrace;
            await ResourceGroupService.UpdateResourceGroupAsync(reportsResourceGroup);
        }

        static private void AppendResources(ResourceGroup resourceGroup, DocAnnotations docAnnotations, double threshold)
        {
            foreach (var symptom in GetSymptoms(docAnnotations))
            {
                string id = symptom.Item1;
                string segId = symptom.Item2;
                bool isNegated = symptom.Item3;
                double score = isNegated ? 0 : symptom.Item4;

                var resource = resourceGroup.Resources.TryGetValue(id);
                if (resource == null)
                {
                    resource = new ResourceSymptom(id);
                    resource.Properties["Segments"] = "";
                    resourceGroup.AddResource(resource);
                }

                score = Math.Max(score, resource.Properties.TryGetValue("Score").AsDouble());
                resource.Properties["Score"] = score.ToString();

                var status = CalcStatus(score, threshold);
                resource.Status = status.ToString();

                var segs = resource.Properties["Segments"];
                resource.Properties["Segments"] = $"{segs};{segId}".TrimStart(';');
            }
        }

        private static TermStatus CalcStatus(double score, double threshold)
        {
            return score > threshold ? TermStatus.Selected : TermStatus.Undefined;
        }

        static private IEnumerable<(string, string, bool, double)> GetSymptoms(DocAnnotations docAnnotations)
        {
            foreach (var seg in docAnnotations.Segments)
            {
                foreach (var ann in seg.Annotations)
                {
                    if (!ann.IsDiscarded && ann.Links != null)
                    {
                        var hpo = ann.Links.Where(r => r.DataSource == "HPO").FirstOrDefault();
                        if (hpo != null)
                        {
                            yield return (hpo.Id, seg.Id, ann.IsNegated, ann.ConfidenceScore);
                        }
                    }
                }
            }
        }

        static private void NormalizeSegments(ResourceGroup resourceGroup)
        {
            foreach (var key in resourceGroup.Resources.Keys)
            {
                var resource = resourceGroup.Resources[key];
                var segIds = resource.Properties["Segments"];
                var segs = new HashSet<string>(segIds.Split(';'));
                resource.Properties["Segments"] = String.Join(';', segs);
            }
        }
    }
}
