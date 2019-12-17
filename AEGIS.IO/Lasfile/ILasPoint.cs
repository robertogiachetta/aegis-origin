﻿/// <copyright file="ILasPoint.cs" company="Eötvös Loránd University (ELTE)">
///     Copyright (c) 2011-2019 Roberto Giachetta. Licensed under the
///     Educational Community License, Version 2.0 (the "License"); you may
///     not use this file except in compliance with the License. You may
///     obtain a copy of the License at
///     http://opensource.org/licenses/ECL-2.0
///
///     Unless required by applicable law or agreed to in writing,
///     software distributed under the License is distributed on an "AS IS"
///     BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
///     or implied. See the License for the specific language governing
///     permissions and limitations under the License.
/// </copyright>
/// <author>Antal Tar</author>

namespace ELTE.AEGIS.IO.Lasfile
{
    using System;

    /// <summary>
    /// Defines behavior of LAS point geometries.
    /// </summary>
    public interface ILasPoint : IBasicPoint
    {
        /// <summary>
        /// Gets the point data record format associated with the point.
        /// </summary>
        Byte Format { get; }
    }
}