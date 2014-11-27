﻿/// <copyright file="SpectralOperationParameters.Filtering.cs" company="Eötvös Loránd University (ELTE)">
///     Copyright (c) 2011-2014 Roberto Giachetta. Licensed under the
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
using ELTE.AEGIS.Numerics;
using System;

namespace ELTE.AEGIS.Operations.Spectral
{
    /// <summary>
    /// Represents a collection of known <see cref="OperationParameter" /> instances for spectral operations.
    /// </summary>
    public static partial class SpectralOperationParameters
    {
        #region Private static fields

        private static OperationParameter _filterFactor;
        private static OperationParameter _filterKernel;
        private static OperationParameter _filterOffset;
        private static OperationParameter _filterRadius;
        private static OperationParameter _filterWeight;
        private static OperationParameter _gaussianStandardDeviation;
        private static OperationParameter _sharpeningAmount;
        private static OperationParameter _sharpeningRadius;
        private static OperationParameter _sharpeningThreshold;

        #endregion

        #region Public static properties

        /// <summary>
        /// Filter factor.
        /// </summary>
        public static OperationParameter FilterFactor
        {
            get
            {
                return _filterFactor ?? (_filterFactor =
                    OperationParameter.CreateOptionalParameter<Double>("AEGIS::223201", "Filter factor",
                                                                       "The factor used by the filter operation to divide the result.", null,
                                                                       1)
                    );
            }
        }

        /// <summary>
        /// Filter kernel.
        /// </summary>
        public static OperationParameter FilterKernel
        {
            get
            {
                return _filterKernel ?? (_filterKernel =
                    OperationParameter.CreateRequiredParameter<Matrix>("AEGIS::223200", "Filter kernel",
                                                                       "The odd sized matrix used by filters for multipling neightbour values.", null,
                                                                       value => (value is Matrix) && (value as Matrix).NumberOfColumns == (value as Matrix).NumberOfRows && (value as Matrix).NumberOfRows % 2 == 1)
                    );
            }
        }

        /// <summary>
        /// Filter offset.
        /// </summary>
        public static OperationParameter FilterOffset
        {
            get
            {
                return _filterOffset ?? (_filterOffset =
                    OperationParameter.CreateOptionalParameter<Double>("AEGIS::223202", "Filter offset",
                                                                       "The offset used by the filter operation to add to the result.", null,
                                                                       0)
                    );
            }
        }

        /// <summary>
        /// Radius of the filter.
        /// </summary>
        public static OperationParameter FilterRadius
        {
            get
            {
                return _filterRadius ?? (_filterRadius =
                    OperationParameter.CreateOptionalParameter<Int32>("AEGIS::223205", "Radius of the filter",
                                                                      "The radius of the filter determining the number of neighbouring pixels to be convoluted by the filter. The radius must be a positive number.", null,
                                                                      1,
                                                                      Conditions.IsPositive())
                    );
            }
        }

        /// <summary>
        /// Filter weight.
        /// </summary>
        public static OperationParameter FilterWeight
        {
            get
            {
                return _filterWeight ?? (_filterWeight =
                    OperationParameter.CreateOptionalParameter<Double>("AEGIS::223204", "Filter weight",
                                                                       "The weight of the central value in the filter.", null,
                                                                       1)
                    );
            }
        }

        /// <summary>
        /// Gaussian standard deviation.
        /// </summary>
        public static OperationParameter GaussianStandardDeviation
        {
            get
            {
                return _gaussianStandardDeviation ?? (_gaussianStandardDeviation =
                    OperationParameter.CreateOptionalParameter<Double>("AEGIS::223271", "Gaussian standard deviation",
                                                                       "The standard deviation value for the Gaussian blur filter.", null, 1)
                );
            }
        }

        /// <summary>
        /// Amount of sharpening.
        /// </summary>
        public static OperationParameter SharpeningAmount
        {
            get
            {
                return _sharpeningAmount ?? (_sharpeningAmount =
                    OperationParameter.CreateOptionalParameter<Double>("AEGIS::223241", "Amount of sharpening",
                                                                       "The strength of the sharpening effect.", null,
                                                                       0.8,
                                                                       Conditions.IsBetween(0, 1))
                );

            }
        }

        /// <summary>
        /// Radius of sharpening.
        /// </summary>
        public static OperationParameter SharpeningRadius
        {
            get
            {
                return _sharpeningRadius ?? (_sharpeningRadius =
                    OperationParameter.CreateOptionalParameter<Int32>("AEGIS::223242", "Radius of sharpening",
                                                                      "Radius affects the size of the edges to be enhanced or how wide the edge rims become, so a smaller radius enhances smaller-scale detail.", null,
                                                                      1,
                                                                      Conditions.IsPositive())
                    );
            }
        }

        /// <summary>
        /// Threshold of sharpening.
        /// </summary>
        public static OperationParameter SharpeningThreshold
        {
            get
            {
                return _sharpeningThreshold ?? (_sharpeningThreshold =
                    OperationParameter.CreateOptionalParameter<UInt32>("AEGIS::223243", "Threshold of sharpening",
                                                                       "Threshold controls the minimum brightness change that will be sharpened or how far apart adjacent tonal values have to be before the filter does anything.", null,
                                                                       0,
                                                                       Conditions.IsBetween(0, 1))
                );

            }
        }

        #endregion
    }
}