using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TWyTec.Blazor
{
    public class TabItem : IComponent
    {
        [Parameter]
        protected string Header { get; set; }

        public void Configure(RenderHandle renderHandle)
        {
        }

        public Task SetParametersAsync(ParameterCollection parameters)
        {
            return Task.CompletedTask;
        }
    }
}
