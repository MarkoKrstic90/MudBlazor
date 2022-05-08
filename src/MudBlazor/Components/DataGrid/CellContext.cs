// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Types;

namespace MudBlazor
{
    public class CellContext<T> where T : IIdentifiable<int>
    {
        internal HashSet<IIdentifiable<int>> selection;

        public T Item { get; set; }
        public CellActions Actions { get; internal set; }
        public bool IsSelected
        {
            get
            {
                if (selection != null)
                {
                    return selection.Any(s => s.Id == Item.Id);
                }

                return false;
            }
        }

        public class CellActions
        {
            public Action<bool> SetSelectedItem { get; internal set; }
            public Action StartEditingItem { get; internal set; }
            public Action CancelEditingItem { get; internal set; }
        }
    }
}
