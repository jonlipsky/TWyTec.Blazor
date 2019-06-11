using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TWyTec.Blazor
{
    public class HorizontalStepper : IComponent, IHandleEvent
    {
        private List<HorizontalStepperTree> _stepperTrees;
        private bool _rendererIsWorked = false;
        private RenderHandle _renderHandle;
        private IReadOnlyDictionary<string, object> _dict;
        private string _cssClass;
        private RenderFragment _childContent;

        
        private bool _navBtnDisabled = false;
        private int _selectedIndex = 0;
        private string _stepperClass = "TWyTecHorizontalStepper";
        private string _stepperNavClass = "TWyTecHorizontalStepperNav";
        private string _stepperBtnClass = "TWyTecHorizontalStepperNavButton";
        private string _stepperNavBtnActiveClass = "TWyTecHorizontalStepperBtnActive";
        private string _stepperContentClass = "TWyTecHorizontalStepperContent";
        private string _stepperInternContentItemClass = "TWyTecHorizontalStepperContentItem";
        private string _stepperInternContentItemActiveClass = "TWyTecHorizontalStepperContentItemActive";
        
        #region public Methods

        public void ChangeSelectedIndex(int index)
        {
            if (index < 0)
                return;
            else if (index >= _stepperTrees.Count)
                return;

            _selectedIndex = index;
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
        protected string StepperNavClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string StepperNavButtonClass { get; set; }

        /// <summary>
        /// Default is false
        /// </summary>
        [Parameter]
        protected bool StepperNavButtonDisabled { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string StepperNavButtonActiveClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string StepperContentClass { get; set; }

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
            p.TryGetValue(RenderTreeBuilder.ChildContent, out _childContent);
            p.TryGetValue("class", out _cssClass);

            _stepperClass = p.GetValueOrDefault(nameof(StepperClass), _stepperClass);
            _stepperNavClass = p.GetValueOrDefault(nameof(StepperNavClass), _stepperNavClass);
            _stepperBtnClass = p.GetValueOrDefault(nameof(StepperNavButtonClass), _stepperBtnClass);
            _navBtnDisabled = p.GetValueOrDefault(nameof(StepperNavButtonDisabled), _navBtnDisabled);
            _stepperNavBtnActiveClass = p.GetValueOrDefault(nameof(StepperNavButtonActiveClass), _stepperNavBtnActiveClass);
            _stepperContentClass = p.GetValueOrDefault(nameof(StepperContentClass), _stepperContentClass);
            _stepperInternContentItemClass = p.GetValueOrDefault(nameof(_stepperInternContentItemClass), _stepperInternContentItemClass);
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
            _stepperTrees = new List<HorizontalStepperTree>();
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
                    var stepperTree = new HorizontalStepperTree(this, si);

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

            builder.OpenElement(_seqIndex++, "div");
            builder.AddAttribute(_seqIndex, "class", _stepperClass);

            var anyAttr = _dict.Where(
                k => k.Key != RenderTreeBuilder.ChildContent && k.Key != "class" &&
                k.Key != nameof(StepperClass) &&
                k.Key != nameof(StepperNavClass) &&
                k.Key != nameof(StepperNavButtonClass) &&
                k.Key != nameof(StepperNavButtonDisabled) &&
                k.Key != nameof(StepperNavButtonActiveClass) &&
                k.Key != nameof(StepperContentClass) &&
                k.Key != nameof(SelectedIndex));

            foreach (var item in anyAttr)
            {
                builder.AddAttribute(_seqIndex, item.Key, item.Value);
            }

            RenderNav(builder);
            RenderContent(builder);

            builder.CloseElement();

            _rendererIsWorked = false;
        }

        private void RenderNav(RenderTreeBuilder builder)
        {
            builder.OpenElement(_seqIndex++, "div");
            builder.AddAttribute(_seqIndex, "class", $"{_stepperNavClass}");
            builder.AddAttribute(_seqIndex, "role", "tablist");
            
            for (int i = 0; i < _stepperTrees.Count; i++)
            {
                var item = _stepperTrees[i];

                if (i > 0)
                {
                    Console.WriteLine($"{i} : addline");
                    builder.OpenElement(_seqIndex++, "div");
                    builder.AddAttribute(_seqIndex, "class", $"TWyTecHorizontalStepperLine");
                    builder.CloseElement();
                }

                Action onclick = item.OnClick;

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
            }
            
            builder.CloseElement();
        }

        void RenderHeaderLable(RenderTreeBuilder builder, HorizontalStepperTree item)
        {
            builder.OpenElement(_seqIndex++, "div");
            builder.AddAttribute(_seqIndex, "style", $"display: flex;");

            #region Circle

            if (_stepperTrees.Count < 11)
            {
                builder.OpenElement(_seqIndex++, "div");
                builder.AddAttribute(_seqIndex, "style", $"margin-right: 10px; font-size: 1em;");
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

        private void RenderContent(RenderTreeBuilder builder)
        {
            builder.OpenElement(_seqIndex++, "div");
            builder.AddAttribute(_seqIndex, "class", _stepperContentClass);

            foreach (var item in _stepperTrees)
            {
                builder.OpenElement(_seqIndex++, "div");
                builder.AddAttribute(_seqIndex, "role", "tabpanel");
                builder.AddAttribute(_seqIndex, "aria-labelledby", item.Header);
                builder.AddAttribute(_seqIndex, "id", item.Id);

                if (item.Index == _selectedIndex)
                {
                    builder.AddAttribute(_seqIndex, "class", $"{_stepperInternContentItemClass} {_stepperInternContentItemActiveClass}");
                }
                else
                {
                    builder.AddAttribute(_seqIndex, "class", $"{_stepperInternContentItemClass}");
                }

                if (item.Index == _selectedIndex)
                    builder.AddAttribute(_seqIndex, "style", $"transform: translate(0%, 0px); min-height: 1px;");
                else if (item.Index > _selectedIndex)
                    builder.AddAttribute(_seqIndex, "style", $"transform: translate(100%, 0px); min-height: 1px;");
                else if (item.Index < _selectedIndex)
                    builder.AddAttribute(_seqIndex, "style", $"transform: translate(-100%, 0px); min-height: 1px;");

                builder.AddContent(_seqIndex, item.Child);

                builder.CloseElement();
            }

            builder.CloseElement();
        }

        #endregion

        #endregion
    }

    internal class HorizontalStepperTree
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public double Height { get; set; }
        public string Header { get; set; }
        public RenderFragment Child { get; set; }
        public Dictionary<string, object> AnyAttrDict;
        public bool IsCompleted { get; set; }

        private HorizontalStepper _stepper;

        public HorizontalStepperTree(HorizontalStepper stepper, int index)
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
