// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common.Types;
using Microsoft.AspNetCore.Components;
using MudBlazor.Utilities;

namespace MudBlazor
{
    public partial class Column<T, TViewModel> : MudComponentBase where T : IIdentifiable<int> where TViewModel : IIdentifiable<int>
    {
        [CascadingParameter] public MudDataGrid<T, TViewModel> DataGrid { get; set; }

        [Parameter] public TViewModel Value { get; set; }
        [Parameter] public EventCallback<TViewModel> ValueChanged { get; set; }
        /// <summary>
        /// Specifies the name of the object's property bound to the column
        /// </summary>
        [Parameter] public bool Visible { get; set; } = true;
        [Parameter] public string Field { get; set; }
        [Parameter] public string Title { get; set; }
        [Parameter] public bool HideSmall { get; set; }
        [Parameter] public int FooterColSpan { get; set; } = 1;
        [Parameter] public int HeaderColSpan { get; set; } = 1;
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public RenderFragment<HeaderContext<T, TViewModel>> HeaderTemplate { get; set; }
        [Parameter] public RenderFragment<CellContext<IIdentifiable<int>>> CellTemplate { get; set; }
        [Parameter] public RenderFragment<FooterContext<T, TViewModel>> FooterTemplate { get; set; }
        [Parameter] public RenderFragment<GroupDefinition<TViewModel>> GroupTemplate { get; set; }
        [Parameter] public Func<TViewModel, object> GroupBy { get; set; }

        #region HeaderCell Properties

        [Parameter] public string HeaderClass { get; set; }
        [Parameter] public Func<TViewModel, string> HeaderClassFunc { get; set; }
        [Parameter] public string HeaderStyle { get; set; }
        [Parameter] public Func<TViewModel, string> HeaderStyleFunc { get; set; }
        /// <summary>
        /// Determines whether this columns data can be sorted. This overrides the Sortable parameter on the DataGrid.
        /// </summary>
        [Parameter] public bool? Sortable { get; set; }
        /// <summary>
        /// Determines whether this columns data can be filtered. This overrides the Filterable parameter on the DataGrid.
        /// </summary>
        [Parameter] public bool? Filterable { get; set; }
        /// <summary>
        /// Determines whether this column can be hidden. This overrides the Hideable parameter on the DataGrid.
        /// </summary>
        [Parameter] public bool? Hideable { get; set; }
        [Parameter] public bool Hidden { get; set; }
        [Parameter] public EventCallback<bool> HiddenChanged { get; set; }
        /// <summary>
        /// Determines whether to show or hide column options. This overrides the ShowColumnOptions parameter on the DataGrid.
        /// </summary>
        [Parameter] public bool? ShowColumnOptions { get; set; }
        [Parameter]
        public Func<TViewModel, object> SortBy
        {
            get
            {
                CompileSortBy();
                return _sortBy;
            }
            set
            {
                _sortBy = value;
            }
        }
        [Parameter] public SortDirection InitialDirection { get; set; } = SortDirection.None;
        [Parameter] public string SortIcon { get; set; } = Icons.Material.Filled.ArrowUpward;
        /// <summary>
        /// Specifies whether the column can be grouped.
        /// </summary>
        [Parameter] public bool? Groupable { get; set; }
        /// <summary>
        /// Specifies whether the column is grouped.
        /// </summary>
        [Parameter] public bool Grouping { get; set; }

        #endregion

        #region Cell Properties

        [Parameter] public string CellClass { get; set; }
        [Parameter] public Func<TViewModel, string> CellClassFunc { get; set; }
        [Parameter] public string CellStyle { get; set; }
        [Parameter] public Func<TViewModel, string> CellStyleFunc { get; set; }
        [Parameter] public bool IsEditable { get; set; } = true;
        [Parameter] public RenderFragment<CellContext<IIdentifiable<int>>> EditTemplate { get; set; }

        #endregion

        #region FooterCell Properties

        [Parameter] public string FooterClass { get; set; }
        [Parameter] public Func<TViewModel, string> FooterClassFunc { get; set; }
        [Parameter] public string FooterStyle { get; set; }
        [Parameter] public Func<TViewModel, string> FooterStyleFunc { get; set; }
        [Parameter] public bool EnableFooterSelection { get; set; }
        [Parameter] public AggregateDefinition<TViewModel> AggregateDefinition { get; set; }

        #endregion

        public Action ColumnStateHasChanged { get; set; }

        internal string headerClassname =>
            new CssBuilder("mud-table-cell")
                .AddClass("mud-table-cell-hide", HideSmall)
                .AddClass(Class)
            .Build();
        internal string cellClassname =>
            new CssBuilder("mud-table-cell")
                .AddClass("mud-table-cell-hide", HideSmall)
                .AddClass(Class)
            .Build();
        internal string footerClassname =>
            new CssBuilder("mud-table-cell")
                .AddClass("mud-table-cell-hide", HideSmall)
                .AddClass(Class)
            .Build();

        internal bool grouping;

        #region Computed Properties

        internal Type dataType
        {
            get
            {
                if (Field == null)
                    return typeof(object);

                return typeof(TViewModel).GetProperty(Field).PropertyType;
            }
        }
        internal bool isNumber
        {
            get
            {
                return FilterOperator.NumericTypes.Contains(dataType);
            }
        }
        internal string computedTitle
        {
            get
            {
                return Title ?? Field;
            }
        }
        internal bool groupable
        {
            get
            {
                return Groupable ?? DataGrid?.Groupable ?? false;
            }
        }

        #endregion

        internal Func<TViewModel, object> _sortBy;
        internal Func<TViewModel, object> groupBy;
        internal HeaderContext<T, TViewModel> headerContext;
        internal FooterContext<T, TViewModel> footerContext;
        private bool initialGroupBySet;

        protected override void OnInitialized()
        {
            if (!Hideable.HasValue)
                Hideable = DataGrid?.Hideable;

            groupBy = GroupBy;
            CompileGroupBy();

            if (groupable && Grouping)
                grouping = Grouping;

            DataGrid?.AddColumn(this);

            // Add the HeaderContext
            headerContext = new HeaderContext<T, TViewModel>
            {
                dataGrid = DataGrid,
                Actions = new HeaderContext<T, TViewModel>.HeaderActions
                {
                    SetSelectAll = async (x) => await DataGrid.SetSelectAllAsync(x),
                }
            };
            // Add the FooterContext
            footerContext = new FooterContext<T, TViewModel>
            {
                dataGrid = DataGrid,
                Actions = new FooterContext<T, TViewModel>.FooterActions
                {
                    SetSelectAll = async (x) => await DataGrid.SetSelectAllAsync(x),
                }
            };
        }

        protected override void OnParametersSet()
        {
            // This needs to be removed but without it, the initial grouping is not set... Need to figure this out.
            if (!initialGroupBySet && grouping)
            {
                initialGroupBySet = true;
                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    groupBy = GroupBy;
                    CompileGroupBy();
                    DataGrid?.ChangedGrouping(this);
                });
            }
        }

        internal void CompileSortBy()
        {
            if (_sortBy == null)
            {
                // set the default SortBy
                var parameter = Expression.Parameter(typeof(TViewModel), "x");
                var field = Expression.Convert(Expression.Property(parameter, typeof(TViewModel).GetProperty(Field)), typeof(object));
                _sortBy = Expression.Lambda<Func<TViewModel, object>>(field, parameter).Compile();
            }
        }

        internal void CompileGroupBy()
        {
            if (groupBy == null && !string.IsNullOrWhiteSpace(Field))
            {
                // set the default GroupBy
                var parameter = Expression.Parameter(typeof(TViewModel), "x");
                var field = Expression.Convert(Expression.Property(parameter, typeof(TViewModel).GetProperty(Field)), typeof(object));
                groupBy = Expression.Lambda<Func<TViewModel, object>>(field, parameter).Compile();
            }
        }

        // Allows child components to change column grouping.
        internal void SetGrouping(bool g)
        {
            if (groupable)
            {
                grouping = g;
                DataGrid?.ChangedGrouping(this);
            }
        }

        /// <summary>
        /// This method's sole purpose is for the DataGrid to remove grouping in mass.
        /// </summary>
        internal void RemoveGrouping()
        {
            grouping = false;
        }

        public void Hide()
        {
            Hidden = true;
        }

        public void Show()
        {
            Hidden = false;
        }

        public void Toggle()
        {
            Hidden = !Hidden;
            DataGrid.ExternalStateHasChanged();
        }

    }
}
