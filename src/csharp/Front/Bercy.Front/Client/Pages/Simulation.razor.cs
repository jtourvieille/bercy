namespace Bercy.Front.Client.Pages
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Front.Shared;
    using Helpers;
    using Models;
    using Newtonsoft.Json;
    using Radzen;

    public partial class Simulation
    {
        private readonly List<int> yearsToCover = new List<int> { 2019, 2020, -2 };

        private readonly SimulationModel model = SimulationModel.Default;

        private bool showResult;

        private DataItem[] repartition;

        private async Task Submit(SimulationModel arg)
        {
            TaxComputationRequestDto requestObject = new TaxComputationRequestDto
            {
                Year = arg.Year,
                Wage = arg.Wage,
                TaxHouseholdComposition = new TaxHouseholdCompositionDto
                {
                    NbChildren = arg.NbChildren,
                    NbAdults = arg.NbAdults
                }
            };

            var httpResponse = await Http.PostAsJsonAsync("https://localhost:44342/api/v1.0/TaxComputer", requestObject);

            var notificationMessage = new NotificationMessage();

            if (httpResponse.IsSuccessStatusCode)
            {
                var taxDto = await httpResponse.ReadAsync<TaxDto>();

                this.showResult = true;
                this.repartition = new[]
                {
                    new DataItem
                    {
                        Name = "Revenus restants",
                        Value = arg.Wage - taxDto.Amount
                    },
                    new DataItem
                    {
                        Name = "Impôts",
                        Value = taxDto.Amount
                    }
                };

                notificationMessage.Severity = NotificationSeverity.Success;
                notificationMessage.Summary = "Calcul réalisé";
                notificationMessage.Detail = "Calcul réalisé avec succès";
                notificationMessage.Duration = 5000;
                
            }
            else
            {
                if (httpResponse.StatusCode == HttpStatusCode.InternalServerError)
                {
                    notificationMessage.Severity = NotificationSeverity.Error;
                    notificationMessage.Summary = "Calcul en échec";
                    notificationMessage.Detail = $"{HttpStatusCode.InternalServerError} - Erreur du service de calcul";
                    notificationMessage.Duration = 5000;
                }

                if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>();

                    notificationMessage.Severity = NotificationSeverity.Error;
                    notificationMessage.Summary = "Calcul en échec";
                    notificationMessage.Detail = problemDetails.Title;
                    notificationMessage.Duration = 5000;
                }
            }

            NotificationService.Notify(notificationMessage);
            this.StateHasChanged();
        }
    }

    public class DataItem
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }
}
