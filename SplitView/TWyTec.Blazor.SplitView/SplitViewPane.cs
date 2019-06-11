using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TWyTec.Blazor
{
    public class SplitViewPane : IComponent, IHandleEvent
    {
        bool _rendererIsWorked = false;
        private RenderHandle _renderHandle;
        private double _paneWidth;
        private string _paneId;
        private IReadOnlyDictionary<string, object> _dict;
        private string _cssClass;
        private string _cssStyle;
        RenderFragment _childContent;

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        string PaneClass { get; set; }
        string _paneClass = "TWyTecSplitViewPane";

        /// <summary>
        /// default is <see cref="SplitViewPaneMode.Inline"/>
        /// </summary>
        [Parameter]
        SplitViewPaneMode PaneMode { get; set; }
        SplitViewPaneMode _paneMode = SplitViewPaneMode.Inline;

        /// <summary>
        /// default is 250
        /// </summary>
        [Parameter]
        double PaneOpenLength { get; set; }
        double _paneOpenLength = 250;

        /// <summary>
        /// default is 40
        /// </summary>
        [Parameter]
        double PaneCompactLength { get; set; }
        double _paneCompactLength = 40;

        /// <summary>
        /// default is true. Worth this can only be set once at the start. Then use the <see cref="TogglePane"/> method 
        /// </summary>
        [Parameter]
        bool IsPaneOpen { get; set; }
        bool _isPaneOpen = true;
        bool _setIsPaneOpen = false;

        public void TogglePane()
        {
            _isPaneOpen = !_isPaneOpen;
            StateHasChanged();
        }

        public void Configure(RenderHandle renderHandle)
        {
            _paneId = Guid.NewGuid().ToString();
            _renderHandle = renderHandle;
        }

        public Task SetParametersAsync(ParameterCollection p)
        {
            p.TryGetValue(RenderTreeBuilder.ChildContent, out _childContent);
            p.TryGetValue("class", out _cssClass);
            p.TryGetValue("style", out _cssStyle);

            if (_setIsPaneOpen == false)
            {
                _setIsPaneOpen = true;
                _isPaneOpen = p.GetValueOrDefault(nameof(IsPaneOpen), _isPaneOpen);
            }
            
            _paneClass = p.GetValueOrDefault(nameof(PaneClass), _paneClass);
            _paneCompactLength = p.GetValueOrDefault(nameof(PaneCompactLength), _paneCompactLength);
            _paneMode = p.GetValueOrDefault(nameof(PaneMode), _paneMode);
            _paneOpenLength = p.GetValueOrDefault(nameof(PaneOpenLength), _paneOpenLength);
            
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
            if (_isPaneOpen)
            {
                _paneWidth = _paneOpenLength;
            }
            else
            {
                if (_paneMode == SplitViewPaneMode.CompactInline || _paneMode == SplitViewPaneMode.CompactOverlay)
                    _paneWidth = _paneCompactLength;
                else
                    _paneWidth = 0;
            }

            builder.OpenElement(0, "div");
            builder.AddAttribute(0, "id", _paneId);
            if (_cssClass != null)
                builder.AddAttribute(0, "class", $"{_cssClass}");
            else
                builder.AddAttribute(0, "class", $"{_paneClass}");

            var anyAttr = _dict.Where(
                k => k.Key != RenderTreeBuilder.ChildContent && k.Key != "class" && k.Key != "style" && k.Key != "id" &&
                k.Key != nameof(PaneClass) &&
                k.Key != nameof(IsPaneOpen) &&
                k.Key != nameof(PaneCompactLength) &&
                k.Key != nameof(PaneMode) &&
                k.Key != nameof(PaneOpenLength)
                );

            foreach (var item in anyAttr)
            {
                builder.AddAttribute(0, item.Key, item.Value);
            }
            
            if (_paneMode == SplitViewPaneMode.CompactOverlay || _paneMode == SplitViewPaneMode.Overlay)
            {
                if (_cssStyle != null)
                    builder.AddAttribute(0, "style", $"max-width: {_paneWidth}px; position: absolute; {_cssStyle}");
                else
                    builder.AddAttribute(0, "style", $"max-width: {_paneWidth}px; position: absolute;");
            }
            else
            {
                if (_cssStyle != null)
                    builder.AddAttribute(0, "style", $"max-width: {_paneWidth}px; {_cssStyle}");
                else
                    builder.AddAttribute(0, "style", $"max-width: {_paneWidth}px;");
            }

            builder.AddContent(1, _childContent);

            builder.CloseElement();
            
            _rendererIsWorked = false;
        }
    }
}
