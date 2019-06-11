using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TWyTec.Blazor
{
    public class VerticalStepper : IComponent, IHandleEvent, IHandleAfterRender
    {
        private bool _setNewParameters = true;
        private bool _rendererIsWorked = false;
        private RenderHandle _renderHandle;
        private IReadOnlyDictionary<string, object> _dict;
        private string _cssClass;
        private RenderFragment _childContent;

        private List<VerticalStepperTree> _stepperTrees;
        private int _selectedIndex = 0;
        private bool _navBtnDisabled = false;
        private string _stepperClass = "TWyTecVerticalStepper";
        private string _stepperItemClass = "TWyTecVerticalStepperItem";
        private string _stepperBtnClass = "TWyTecVerticalStepperItemButton";
        private string _stepperNavBtnActiveClass = "TWyTecVerticalStepperItemButtonActive";
        private string _stepperContentClass = "TWyTecVerticalStepperItemContent";
        private string _stepperContentAnimateClass = "TWyTecVerticalStepperContentTranslation";

        [Inject]
        IJSRuntime JsRuntime { get; set; }

        #region public methods

        public async void ChangeSelectedIndex(int index)
        {
            if (index < 0)
                return;
            else if (index >= _stepperTrees.Count)
                return;
            
            _selectedIndex = index;
            var item = _stepperTrees[index];
            item.Height = await JsRuntime.InvokeAsync<double>("twytecStepperGetPanelHeight", item.Id);
            StateHasChanged();
        }

        public int GetSelectedIndex()
            => _selectedIndex;

        public void GoToNext()
        {
            var i = GetSelectedIndex() + 1;
            ChangeSelectedIndex(i);
        }

        public void GoToPrevious()
        {
            var i = GetSelectedIndex() - 1;
            ChangeSelectedIndex(i);
        }

        public void Reset()
        {
            _selectedIndex = 0;
            foreach (var item in _stepperTrees)
            {
                item.IsCompleted = false;
            }
            StateHasChanged();
        }

        public void SetIndexToCompleted(int index)
        {
            if (index > 0 && _stepperTrees.Count > index)
            {
                _stepperTrees[index].IsCompleted = true;
                StateHasChanged();
            }
        }

        #endregion

        #region Propertys

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string StepperClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string StepperItemClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string StepperItemButtonClass { get; set; }

        /// <summary>
        /// Default is false
        /// </summary>
        [Parameter]
        protected bool StepperItemButtonDisabled { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string StepperItemButtonActiveClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string StepperItemContentClass { get; set; }

        /// <summary>
        /// Set index
        /// </summary>
        [Parameter]
        protected int SelectedIndex { get; set; }

        #endregion

        #region GetStepperContentId

        Dictionary<int, string> _stepperContentIds;

        internal string GetStepperContentId(int index)
        {
            if (_stepperContentIds == null)
                _stepperContentIds = new Dictionary<int, string>();

            if (_stepperContentIds.ContainsKey(index))
                return _stepperContentIds[index];
            else
            {
                var id = Guid.NewGuid().ToString();
                _stepperContentIds.Add(index, id);
                return id;
            }
        }

        #endregion

        internal void StateHasChanged()
        {
            if (_rendererIsWorked)
            {
                return;
            }

            _rendererIsWorked = true;
            _renderHandle.Render(RenderTree);
        }

        void IComponent.Configure(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        Task IComponent.SetParametersAsync(ParameterCollection p)
        {
            _setNewParameters = true;
            p.TryGetValue(RenderTreeBuilder.ChildContent, out _childContent);
            p.TryGetValue("class", out _cssClass);

            _stepperClass = p.GetValueOrDefault(nameof(StepperClass), _stepperClass);
            _stepperItemClass = p.GetValueOrDefault(nameof(StepperItemClass), _stepperItemClass);
            _stepperBtnClass = p.GetValueOrDefault(nameof(StepperItemButtonClass), _stepperBtnClass);
            _stepperNavBtnActiveClass = p.GetValueOrDefault(nameof(StepperItemButtonActiveClass), _stepperNavBtnActiveClass);
            _stepperContentClass = p.GetValueOrDefault(nameof(StepperItemContentClass), _stepperContentClass);
            _navBtnDisabled = p.GetValueOrDefault(nameof(StepperItemButtonDisabled), _navBtnDisabled);
            _selectedIndex = p.GetValueOrDefault(nameof(SelectedIndex), _selectedIndex);

            _dict = p.ToDictionary();
            _renderHandle.Render(CreateTree);
            return Task.FromResult(true);
        }

        Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem binding, object args)
            => Based.HandleEventExtensions.HandleEventAsync(binding, args, StateHasChanged);

        #region Tree

        int _seqIndex;

        #region CreateTree
        
        private void CreateTree(RenderTreeBuilder builder)
        {
            _stepperTrees = new List<VerticalStepperTree>();
            ExploreTree(builder);
            StateHasChanged();
        }

        private void ExploreTree(RenderTreeBuilder builder)
        {
            builder.Clear();
            _childContent(builder);
            var frames = builder.GetFrames().ToArray();

            int si = 0;

            for (int i = 0; i < frames.Count(); i++)
            {
                var frame = frames[i];

                if (frame.FrameType == RenderTreeFrameType.Component && frame.ComponentType == typeof(StepperItem))
                {
                    var stepperTree = new VerticalStepperTree(this, si);

                    for (int f = 0; f < frame.ComponentSubtreeLength; f++)
                    {
                        i++;
                        var nextFrame = frames[i];

                        if (nextFrame.FrameType == RenderTreeFrameType.Attribute)
                        {
                            if (nextFrame.AttributeName == "Header")
                            {
                                stepperTree.Header = nextFrame.AttributeValue.ToString();
                            }
                            else if (nextFrame.AttributeName == RenderTreeBuilder.ChildContent)
                            {
                                if (nextFrame.AttributeValue is RenderFragment nextChild)
                                {
                                    stepperTree.Child = nextChild;
                                }
                            }
                            else
                            {
                                stepperTree.AnyAttrDict.Add(nextFrame.AttributeName, nextFrame.AttributeValue);
                            }
                        }
                    }

                    _stepperTrees.Add(stepperTree);
                    si++;
                }
            }
        }

        #endregion

        #region RenderTree

        private void RenderTree(RenderTreeBuilder builder)
        {
            builder.Clear();
            _seqIndex = 0;

            RenderContent(builder);

            _rendererIsWorked = false;
        }

        private void RenderContent(RenderTreeBuilder builder)
        {
            builder.OpenElement(_seqIndex++, "div");
            if (_cssClass == null)
                builder.AddAttribute(_seqIndex, "class", _stepperClass);
            else
                builder.AddAttribute(_seqIndex, "class", $"{_stepperClass} {_cssClass}");

            var anyAttr = _dict.Where(
                k => k.Key != RenderTreeBuilder.ChildContent && k.Key != "class" &&
                k.Key != nameof(StepperClass) &&
                k.Key != nameof(StepperItemClass) &&
                k.Key != nameof(StepperItemButtonClass) &&
                k.Key != nameof(StepperItemButtonActiveClass) &&
                k.Key != nameof(StepperItemContentClass) &&
                k.Key != nameof(StepperItemButtonDisabled) &&
                k.Key != nameof(SelectedIndex));

            foreach (var item in anyAttr)
            {
                builder.AddAttribute(_seqIndex, item.Key, item.Value);
            }

            builder.OpenElement(_seqIndex++, "div");
            builder.AddAttribute(_seqIndex, "role", "tablist");

            foreach (var item in _stepperTrees)
            {
                Action onclick = item.OnClick;

                builder.OpenElement(_seqIndex++, "div");

                if (item.AnyAttrDict.ContainsKey("class"))
                    builder.AddAttribute(_seqIndex, "class", $"{_stepperItemClass} {item.AnyAttrDict["class"]}");
                else
                    builder.AddAttribute(_seqIndex, "class", $"{_stepperItemClass}");

                foreach (var a in item.AnyAttrDict)
                {
                    builder.AddAttribute(_seqIndex, a.Key, a.Value);
                }

                builder.OpenElement(_seqIndex++, "div");
                builder.AddAttribute(_seqIndex, "role", "tab");
                builder.AddAttribute(_seqIndex, "aria-controls", item.Id);

                if (_navBtnDisabled == false)
                    builder.AddAttribute(_seqIndex, "onclick", onclick);

                if (item.Index == _selectedIndex)
                {
                    builder.AddAttribute(_seqIndex, "class", $"{_stepperBtnClass} {_stepperNavBtnActiveClass}");
                    builder.AddAttribute(_seqIndex, "aria-expanded", "true");
                }
                else
                {
                    builder.AddAttribute(_seqIndex, "class", _stepperBtnClass);
                    builder.AddAttribute(_seqIndex, "aria-expanded", "false");
                }

                RenderHeaderLable(builder, item);
                builder.CloseElement();

                builder.OpenElement(_seqIndex++, "div");
                builder.AddAttribute(_seqIndex, "role", "tabpanel");
                builder.AddAttribute(_seqIndex, "aria-labelledby", item.Header);
                builder.AddAttribute(_seqIndex, "id", item.Id);
                builder.AddAttribute(_seqIndex, "data-AnimateClass", $"{_stepperContentAnimateClass}");
                builder.AddAttribute(_seqIndex, "class", $"{_stepperContentClass} {_stepperContentAnimateClass}");

                if (item.Index == _selectedIndex)
                {
                    if (item.Height > 0)
                        builder.AddAttribute(_seqIndex, "style", $"max-height: {item.Height}px;");
                    else
                        builder.AddAttribute(_seqIndex, "style", $"max-height: auto;");
                }
                else
                    builder.AddAttribute(_seqIndex, "style", $"max-height: 0px;");

                builder.AddContent(_seqIndex, item.Child);
                builder.CloseElement();

                builder.CloseElement();
            }

            builder.CloseElement();
            builder.CloseElement();
        }

        void RenderHeaderLable(RenderTreeBuilder builder, VerticalStepperTree item)
        {
            builder.OpenElement(_seqIndex++, "div");
            builder.AddAttribute(_seqIndex, "style", $"display: flex;");

            #region Circle

            if (_stepperTrees.Count < 11)
            {
                builder.OpenElement(_seqIndex++, "div");
                builder.AddAttribute(_seqIndex, "style", $"margin-left: -21px; margin-right: 10px; font-size: 1em;");
                builder.AddContent(_seqIndex, new MarkupString($"&#{10102 + item.Index};"));
                builder.CloseElement();
            }

            #endregion

            builder.OpenElement(_seqIndex++, "div");
            builder.AddAttribute(_seqIndex, "style", $"margin-right: 10px;");
            builder.AddContent(_seqIndex, item.Header);
            builder.CloseElement();

            if (_selectedIndex == item.Index)
            {
                builder.OpenElement(_seqIndex++, "div");
                builder.AddAttribute(_seqIndex, "aria-label", $"Focus");
                builder.AddContent(_seqIndex, new MarkupString("&#9737;"));
                builder.CloseElement();
            }
            else if (item.IsCompleted)
            {
                builder.OpenElement(_seqIndex++, "div");
                builder.AddAttribute(_seqIndex, "aria-label", "completed");
                builder.AddContent(_seqIndex, new MarkupString("&#x2713;"));
                builder.CloseElement();
            }
            else if (item.IsCompleted == false)
            {
                builder.OpenElement(_seqIndex++, "div");
                builder.AddAttribute(_seqIndex, "aria-label", "not completed");
                builder.AddContent(_seqIndex, new MarkupString("&#10005;"));
                builder.CloseElement();
            }


            builder.CloseElement();
        }

        #endregion

        #endregion

        public async Task OnAfterRenderAsync()
        {
            if (_setNewParameters)
            {
                _setNewParameters = false;
                await SetPaneHeightSelectedItemAsync();
            }
        }

        async Task SetPaneHeightSelectedItemAsync()
        {
            var item = _stepperTrees[_selectedIndex];
            await JsRuntime.InvokeAsync<bool>("twytecStepperSetPanelHeight", item.Id);
        }
    }

    internal class VerticalStepperTree
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public double Height { get; set; }
        public string Header { get; set; }
        public RenderFragment Child { get; set; }
        public Dictionary<string, object> AnyAttrDict;
        public bool IsCompleted { get; set; }

        private VerticalStepper _stepper;

        public VerticalStepperTree(VerticalStepper stepper, int index)
        {
            _stepper = stepper;
            Index = index;
            Id = _stepper.GetStepperContentId(Index);
            AnyAttrDict = new Dictionary<string, object>();
        }

        public void OnClick()
        {
            _stepper.ChangeSelectedIndex(Index);
        }
    }
}
