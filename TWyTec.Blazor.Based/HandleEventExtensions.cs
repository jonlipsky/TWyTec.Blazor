using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TWyTec.Blazor.Based
{
    public static class HandleEventExtensions
    {
        public static async Task HandleEventAsync(EventCallbackWorkItem binding, object args, Action stateHasChanged)
        {
            try
            {
               await binding.InvokeAsync(args);
                stateHasChanged();
            }
            catch (Exception e)
            {
                HandlingException.HandleException(e);
                throw;
            }
        }
    }
}
