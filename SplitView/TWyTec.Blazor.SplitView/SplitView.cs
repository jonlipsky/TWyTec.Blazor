using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TWyTec.Blazor
{
    public class SplitView : IComponent, IHandleEvent
    {
        private bool _rendererIsWorked = false;
        private RenderHandle _renderHandle;
        private IReadOnlyDictionary<string, object> _dict;
        private string _cssClass;
        private RenderFragment _childContent;

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        string SplitViewClass { get; set; }
        string _spClass = "TWyTecSplitView";
        
        void IComponent.Configure(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        Task IComponent.SetParametersAsync(ParameterCollection p)
        {
            p.TryGetValue(RenderTreeBuilder.ChildContent, out _childContent);
            p.TryGetValue("class", out _cssClass);
            _spClass = p.GetValueOrDefault(nameof(SplitViewClass), _spClass);

            _dict = p.ToDictionary();
            StateHasChanged();

            return Task.FromResult(true);
        }

        Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem binding, object args)
            => Based.HandleEventExtensions.HandleEventAsync(binding, args, StateHasChanged);

        private void StateHasChanged()
        {
            if (_rendererIsWorked)
            {
                return;
            }

            _rendererIsWorked = true;
            _renderHandle.Render(RenderTree);
        }

        private void RenderTree(RenderTreeBuilder builder)
        {
            int seqIndex = 0;

            builder.OpenElement(seqIndex++, "div");
            builder.AddAttribute(seqIndex, "class", _spClass);

            var anyAttr = _dict.Where(
                k => k.Key != RenderTreeBuilder.ChildContent && k.Key != "class" && k.Key != nameof(SplitViewClass));

            foreach (var item in anyAttr)
            {
                builder.AddAttribute(0, item.Key, item.Value);
            }

            builder.AddContent(seqIndex, _childContent);

            builder.CloseElement();

            _rendererIsWorked = false;
        }
    }
}

