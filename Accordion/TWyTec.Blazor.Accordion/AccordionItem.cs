using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TWyTec.Blazor
{
    public class AccordionItem : IComponent
    {
        [Parameter]
        protected string Header { get; set; }

        public void Configure(RenderHandle renderHandle)
        {
        }

        public Task SetParametersAsync(ParameterCollection parameters)
        {
            return Task.FromResult(true);
        }
    }
}
