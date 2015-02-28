﻿/// <copyright file="OperationParameters.cs" company="Eötvös Loránd University (ELTE)">
///     Copyright (c) 2011-2015 Roberto Giachetta. Licensed under the
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
/// <author>Roberto Giachetta</author>

using ELTE.AEGIS.Management;
using ELTE.AEGIS.Operations.Management;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ELTE.AEGIS.Operations
{
    /// <summary>
    /// Represents a collection of known <see cref="OperationParameter" /> instances.
    /// </summary>
    [OperationParameterCollection]
    public class CommonOperationParameters
    {
        #region Query fields

        private static OperationParameter[] _all;

        #endregion

        #region Query properties

        /// <summary>
        /// Gets all <see cref="OperationParameter" /> instances within the collection.
        /// </summary>
        /// <value>A read-only list containing all <see cref="OperationParameter" /> instances within the collection.</value>
        public static IList<OperationParameter> All
        {
            get
            {
                if (_all == null)
                    _all = typeof(CommonOperationParameters).GetProperties().
                                                       Where(property => property.Name != "All").
                                                       Select(property => property.GetValue(null, null) as OperationParameter).
                                                       ToArray();
                return Array.AsReadOnly(_all);
            }
        }

        #endregion

        #region Query methods

        /// <summary>
        /// Returns all <see cref="OperationParameter" /> instances matching a specified identifier.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns>A list containing the <see cref="OperationParameter" /> instances that match the specified identifier.</returns>
        public static IList<OperationParameter> FromIdentifier(String identifier)
        {
            if (identifier == null)
                return null;

            return All.Where(obj => System.Text.RegularExpressions.Regex.IsMatch(obj.Identifier, identifier)).ToList();
        }
        /// <summary>
        /// Returns all <see cref="OperationParameter" /> instances matching a specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A list containing the <see cref="OperationParameter" /> instances that match the specified name.</returns>
        public static IList<OperationParameter> FromName(String name)
        {
            if (name == null)
                return null;

            return All.Where(obj => System.Text.RegularExpressions.Regex.IsMatch(obj.Name, name)).ToList();
        }

        #endregion

        #region Private static fields

        private static OperationParameter _geometryFactory;
        private static OperationParameter _metadataPreservation;
        private static OperationParameter _numberOfParts;
        private static OperationParameter _overlapMargin;

        #endregion

        #region Public static properties

        /// <summary>
        /// Geometry factory.
        /// </summary>
        public static OperationParameter GeometryFactory
        {
            get
            {
                return _geometryFactory ?? (_geometryFactory =
                    OperationParameter.CreateOptionalParameter<IGeometryFactory>(
                        "221001", "Geometry factory",
                        "The factory used for producing resulting geometry instances.", null,
                        (IGeometryFactory)null
                    ));
            }
        }

        /// <summary>
        /// Metadata preservation.
        /// </summary>
        public static OperationParameter MetadataPreservation
        {
            get
            {
                return _metadataPreservation ?? (_metadataPreservation =
                    OperationParameter.CreateOptionalParameter<Boolean>(
                        "AEGIS::221005", "Metadata preservation",
                        "Indicates whether the metadata should be perserved suring transformation.", null,
                        false
                    ));
            }
        }

        /// <summary>
        /// Number of parts.
        /// </summary>
        public static OperationParameter NumberOfParts
        {
            get
            {
                return _numberOfParts ?? (_numberOfParts =
                    OperationParameter.CreateOptionalParameter<Int32>(
                        "AEGIS::221561", "Number of parts",
                        "The number of parts to create during partitioning.", null,
                        1,
                        Conditions.IsPositive()
                    ));
            }
        }

        /// <summary>
        /// Overlap zone.
        /// </summary>
        public static OperationParameter OverlapMargin
        {
            get
            {
                return _overlapMargin ?? (_overlapMargin =
                    OperationParameter.CreateOptionalParameter<Double>(
                        "AEGIS::221575", "Overlap margin",
                        "The margin by which the individual parts should overlap after partitiining.", null,
                        0,
                        Conditions.IsNotNegative()
                    ));
            }
        }

        #endregion
    }
}