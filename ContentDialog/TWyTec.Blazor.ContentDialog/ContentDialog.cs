using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace TWyTec.Blazor
{
    public class ContentDialog : IComponent, IHandleEvent
    {
        bool _rendererIsWorked = false;
        private RenderHandle _renderHandle;
        private IReadOnlyDictionary<string, object> _dict;
        private string _cssClass;
        private string _cssStyle;
        private RenderFragment _childContent;
        private bool _isShow = false;

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        string WrapperClass { get; set; }
        string _wrapperClass = "TWyTecContentDlgWrapper";

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        string ContentClass { get; set; }
        string _contentClass = "TWyTecContentDlgContent";

        /// <summary>
        /// CSS Style. Default is "opacity: 1; z-index: 1050; top: 0px;"
        /// </summary>
        [Parameter]
        string FadeInStyle { get; set; }
        string _fadeInStyle = "opacity: 1; z-index: 1050; top: 0px;";

        /// <summary>
        /// CSS Style. Default is "opacity: 0; z-index: -1; top: -100px;"
        /// </summary>
        [Parameter]
        string FadeOutStyle { get; set; }
        string _fadeOutStyle = "opacity: 0; z-index: -1; top: -100px;";

        #region Fade

        public void Show()
        {
            FadeIn();
        }

        public async Task ShowAsync()
        {
            FadeIn();

            while (_isShow == true)
            {
                await Task.Delay(500);
            }
        }

        public void Hide()
        {
            FadeOut();
        }

        private void FadeIn()
        {
            _isShow = true;
            StateHasChanged();
        }

        private void FadeOut()
        {
            _isShow = false;
            StateHasChanged();
        }

        #endregion

        void IComponent.Configure(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        Task IComponent.SetParametersAsync(ParameterCollection p)
        {
            p.TryGetValue(RenderTreeBuilder.ChildContent, out _childContent);
            p.TryGetValue("class", out _cssClass);
            p.TryGetValue("style", out _cssStyle);

            _wrapperClass = p.GetValueOrDefault(nameof(WrapperClass), _wrapperClass);
            _contentClass = p.GetValueOrDefault(nameof(ContentClass), _contentClass);
            _fadeInStyle = p.GetValueOrDefault(nameof(FadeInStyle), _fadeInStyle);
            _fadeOutStyle = p.GetValueOrDefault(nameof(FadeOutStyle), _fadeOutStyle);

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
            builder.OpenElement(0, "div");
            builder.AddAttribute(0, "class", $"{_wrapperClass} {_cssClass}");
            builder.AddAttribute(0, "role", "dialog");

            var anyAttr = _dict.Where(
                k => k.Key != RenderTreeBuilder.ChildContent && k.Key != "class" && k.Key != "style" &&
                k.Key != nameof(WrapperClass) &&
                k.Key != nameof(ContentClass)
                );

            foreach (var item in anyAttr)
            {
                builder.AddAttribute(0, item.Key, item.Value);
            }

            if (_isShow)
                builder.AddAttribute(0, "style", $"{_fadeInStyle}{_cssStyle}");
            else
                builder.AddAttribute(0, "style", $"{_fadeOutStyle}{_cssStyle}");
            
            builder.OpenElement(1, "div");
            builder.AddAttribute(1, "class", _contentClass);
            builder.AddAttribute(1, "role", "document");

            builder.AddContent(2, _childContent);

            builder.CloseElement();
            builder.CloseElement();

            _rendererIsWorked = false;
        }
    }
}
