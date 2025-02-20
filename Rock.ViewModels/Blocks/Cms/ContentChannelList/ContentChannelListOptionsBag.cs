﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelList
{
    /// <summary>
    /// The additional configuration options for the Content Channel List block.
    /// </summary>
    public class ContentChannelListOptionsBag
    {
        /// <summary>
        /// Gets or sets the available content channel types for the Content Channel Type filter.
        /// </summary>
        /// <value>
        /// The content channel type options.
        /// </value>
        public List<ListItemBag> ContentChannelTypeItems { get; set; }
    }
}
